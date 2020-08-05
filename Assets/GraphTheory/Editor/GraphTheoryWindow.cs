﻿using GraphTheory.Editor.UIElements;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

//https://docs.unity3d.com/Manual/UIE-Events-Reference.html

namespace GraphTheory.Editor
{
    public class GraphTheoryWindow : EditorWindow
    {
        private const string DATA_STRING = "GraphWindowData";
        private const string TOOLBAR = "toolbar";
        private const string MAIN_SPLITVIEW = "main-TwoPanelSplit";
        private const string MAIN_PANEL_LEFT = "main-panel-left";
        private const string MAIN_PANEL_RIGHT = "main-panel-right";

        private GraphWindowData m_graphWindowData = null;
        private Toolbar m_toolbar = null;
        private TwoPaneSplitView m_mainSplitView = null;
        private TabGroupElement m_mainTabGroup = null;
        private LibraryTabElement m_libraryTab = null;
        private NodeGraphView m_nodeGraphView = null;
        private BreadcrumbsView m_breadcrumbs = null;

        private NodeGraph m_openedGraphInstance = null;

        [MenuItem("Graph/Clear Graph Data")]
        public static void ClearGraphData()
        {
            EditorPrefs.SetString(DATA_STRING, JsonUtility.ToJson(new GraphWindowData(), true));
        }

        private void OnEnable() 
        {
            var xmlAsset = Resources.Load<VisualTreeAsset>("GraphTheory/GraphTheoryWindow");
            xmlAsset.CloneTree(rootVisualElement);

            // Get all the elements
            m_mainSplitView = rootVisualElement.Q<TwoPaneSplitView>(MAIN_SPLITVIEW);
            VisualElement mainPanelRight = rootVisualElement.Q<VisualElement>(MAIN_PANEL_RIGHT);
            VisualElement mainPanelLeft = rootVisualElement.Q<VisualElement>(MAIN_PANEL_LEFT);
            m_toolbar = rootVisualElement.Q<Toolbar>(TOOLBAR);

            RegisterMainPanelLeft(mainPanelLeft);
            RegisterMainPanelRight(mainPanelRight);

            //Register toolbar last!
            RegisterToolbarButton_CreateNewGraph();

            GraphModificationProcessor.OnGraphCreated += OnNewGraphCreated;
            GraphModificationProcessor.OnGraphWillDelete += OnGraphWillDelete;

            DeserializeData();
        }

        private void OnDisable()
        {
            SerializeData();
            GraphModificationProcessor.OnGraphCreated -= OnNewGraphCreated;
        }

        private void DeserializeData()
        {
            string serializedData = EditorPrefs.GetString(DATA_STRING, "");
            if(string.IsNullOrEmpty(serializedData))
            {
                m_graphWindowData = new GraphWindowData(); 
            }
            else
            {
                m_graphWindowData = JsonUtility.FromJson<GraphWindowData>(serializedData);
            }
            Debug.Log("Deserialized data: " + serializedData);

            // Window siz 
            //Rect window = position; 
            //window.size = m_graphWindowData.WindowDimensions; 
            //position = window;
            
            m_mainSplitView.SetSplitPosition(m_graphWindowData.MainSplitViewPosition);
            if(!string.IsNullOrEmpty(m_graphWindowData.OpenGraphGUID))
            {
                OpenGraph(m_graphWindowData.OpenGraphGUID, m_graphWindowData.GraphBreadcrumbPath);
            }
            m_mainTabGroup.DeserializeData(m_graphWindowData.MainTabGroup);
        }

        private void SerializeData()
        {
            m_graphWindowData.WindowDimensions = position.size;
            m_graphWindowData.MainSplitViewPosition = m_mainSplitView.SplitPosition;
            m_graphWindowData.MainTabGroup = m_mainTabGroup.GetSerializedData();

            Debug.Log("Serializing data: " + JsonUtility.ToJson(m_graphWindowData, true));
            EditorPrefs.SetString(DATA_STRING, JsonUtility.ToJson(m_graphWindowData, true));
        }

        private void RegisterToolbarButton_CreateNewGraph()
        { 
            var graphCreateButton = new ToolbarButton(() =>
            {
                CreateNewGraphPopup.OpenWindow();
            });
            graphCreateButton.text = "Create Graph";
            m_toolbar.Add(graphCreateButton);

            var saveGraphButton = new ToolbarButton(() =>
            {
                if (m_openedGraphInstance != null)
                {
                    EditorUtility.SetDirty(m_openedGraphInstance);
                    AssetDatabase.SaveAssets();
                }
            });
            saveGraphButton.text = "Save";
            m_toolbar.Add(saveGraphButton);
        }

        private void RegisterMainPanelLeft(VisualElement leftPanel)
        {
            List<(string, TabContentElement)> tabs = new List<(string, TabContentElement)>();
            tabs.Add(("Library", m_libraryTab = new LibraryTabElement((string guid) => { OpenGraph(guid); })));
            tabs.Add(("Inspector", new TestContent()));

            m_mainTabGroup = new TabGroupElement(tabs);
            m_mainTabGroup.StretchToParentSize();
            leftPanel.Add(m_mainTabGroup);
        }

        private void RegisterMainPanelRight(VisualElement rightPanel)
        {
            m_nodeGraphView = new NodeGraphView();
            m_nodeGraphView.StretchToParentSize();
            rightPanel.Add(m_nodeGraphView);

            m_nodeGraphView.Add(m_breadcrumbs = new BreadcrumbsView());
            m_breadcrumbs.OnBreadcrumbChanged += (path) => { SetGraphBreadcrumbPath(m_openedGraphInstance, path); };
        }

        public void OpenGraph(string guid, string breadcrumb = "")
        {
            m_graphWindowData.OpenGraphGUID = guid;
            m_openedGraphInstance = AssetDatabase.LoadAssetAtPath<NodeGraph>(AssetDatabase.GUIDToAssetPath(guid));
            m_libraryTab.SetOpenNodeGraph(m_openedGraphInstance, guid);
            if(string.IsNullOrEmpty(breadcrumb))
            {
                breadcrumb = "base/";
            }
            SetGraphBreadcrumbPath(m_openedGraphInstance, breadcrumb);
        }

        private void CloseCurrentGraph()
        {
            m_graphWindowData.OpenGraphGUID = "";
            m_openedGraphInstance = null;
            m_libraryTab.SetOpenNodeGraph(null, null);
            SetGraphBreadcrumbPath(null, null);
        }

        private void SetGraphBreadcrumbPath(NodeGraph graph, string path)
        {
            Debug.Log("New breadcrumb path is " + path);
            m_graphWindowData.GraphBreadcrumbPath = path;
            m_breadcrumbs.SetBreadcrumbPath(path);
            m_nodeGraphView.SetNodeCollection(graph, path);
        }

        private void OnNewGraphCreated(NodeGraph graph)
        {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(graph, out string guid, out long localId);
            m_libraryTab.RegisterNewlyCreatedGraph(graph, guid);
            OpenGraph(guid);
        }

        private void OnGraphWillDelete(NodeGraph graph)
        {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(graph, out string guid, out long localId);
            if (graph == m_openedGraphInstance)
            {
                CloseCurrentGraph();
            }
            m_libraryTab.HandleDeletedGraph(graph, guid);
        }
    }

    public class GraphModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        public static Action<NodeGraph> OnGraphCreated = null;
        public static Action<NodeGraph> OnGraphWillDelete = null;
        public static Action<NodeGraph, string> OnGraphWillMove = null;

        public static void OnAssetCreated(NodeGraph nodeGraph)
        {
            OnGraphCreated?.Invoke(nodeGraph);
        }

        private static AssetDeleteResult OnWillDeleteAsset(string sourcePath, RemoveAssetOptions removeAssetOptions)
        {
            if(OnGraphWillDelete != null)
            {
                NodeGraph graphToDelete = AssetDatabase.LoadAssetAtPath<NodeGraph>(sourcePath);
                if(graphToDelete != null)
                {
                    OnGraphWillDelete?.Invoke(graphToDelete);
                }
            }
            return AssetDeleteResult.DidNotDelete;
        }

        private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        {
            if (OnGraphWillMove != null)
            {
                NodeGraph graphToDelete = AssetDatabase.LoadAssetAtPath<NodeGraph>(sourcePath);
                if (graphToDelete != null)
                {
                    OnGraphWillMove?.Invoke(graphToDelete, destinationPath);
                }
            }
            return AssetMoveResult.DidNotMove;
        }

        private static void OnGraphLoadedInRuntime(NodeGraph nodeGraph)
        {

        }
    }
}