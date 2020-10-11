﻿using UnityEngine;
using GraphTheory.BuiltInNodes;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GraphTheory
{
    [Serializable]
    public abstract class NodeGraph : ScriptableObject
    {
        public abstract Type GraphPropertiesType { get; }
        public virtual bool UseIMGUIPropertyDrawer { get { return false; } }

        [SerializeField]
        private NodeCollection m_nodeCollection;
        [SerializeReference, HideInInspector]
        public AGraphProperties GraphProperties;
        [SerializeField]
        private BlackboardData m_blackboardData;

#if UNITY_EDITOR
        public NodeCollection NodeCollection { get { return m_nodeCollection; } }
        public BlackboardData BlackboardData { get { return m_blackboardData; } }
        public Action<string> OnNodeOutportAdded = null;
        public Action<string, int> OnNodeOutportRemoved = null;
        public Action<string> OnNodeAllOutportsRemoved = null;

        public NodeGraph()
        {
            m_nodeCollection = new NodeCollection();
            ANode entryNode = m_nodeCollection.CreateNode(typeof(EntryNode), Vector2.zero);
            m_nodeCollection.SetEntryNode(entryNode.Id);
            GraphProperties = Activator.CreateInstance(GraphPropertiesType) as AGraphProperties;
            m_blackboardData = new BlackboardData();
            m_blackboardData.AddElement(new StringBlackboardElement());
            m_blackboardData.AddElement(new BoolBlackboardElement());
        }

        public void Awake()
        {
            //TODO: Register to runtime tracker here
        }

        public static void AddOutportToNode(SerializedProperty serializedNode)
        {
            (serializedNode.serializedObject.targetObject as NodeGraph).AddOutportToNode_Internal(serializedNode);
        }

        public static void RemoveOutportFromNode(SerializedProperty serializedNode, int index = -1)
        {
            (serializedNode.serializedObject.targetObject as NodeGraph).RemoveOutportFromNode_Internal(serializedNode, index);
        }

        public static void RemoveAllOutportsFromNode(SerializedProperty serializedNode)
        {
            (serializedNode.serializedObject.targetObject as NodeGraph).RemoveAllOutportsFromNode_Internal(serializedNode);
        }


        private void AddOutportToNode_Internal(SerializedProperty serializedNode)
        {
            serializedNode.serializedObject.Update();
            string nodeId = serializedNode.FindPropertyRelative("m_id").stringValue;
            string newOutportId = Guid.NewGuid().ToString();

            SerializedProperty outportsProperty = serializedNode.FindPropertyRelative("m_outports");
            outportsProperty.InsertArrayElementAtIndex(outportsProperty.arraySize);
            SerializedProperty newOutportProperty = outportsProperty.GetArrayElementAtIndex(outportsProperty.arraySize - 1);
            newOutportProperty.FindPropertyRelative("Id").stringValue = newOutportId;
            newOutportProperty.FindPropertyRelative("ConnectedNodeId").stringValue = "";

            serializedNode.serializedObject.ApplyModifiedProperties();
            OnNodeOutportAdded?.Invoke(nodeId);
        }

        private void RemoveOutportFromNode_Internal(SerializedProperty serializedNode, int index)
        {
            if(index == -1)
            {
                index = serializedNode.FindPropertyRelative("m_outports").arraySize - 1;
            }

            serializedNode.serializedObject.Update();
            string nodeId = serializedNode.FindPropertyRelative("m_id").stringValue;
            serializedNode.FindPropertyRelative("m_outports").DeleteArrayElementAtIndex(index);
            serializedNode.serializedObject.ApplyModifiedProperties();
            OnNodeOutportRemoved?.Invoke(nodeId, index);
        }

        private void RemoveAllOutportsFromNode_Internal(SerializedProperty serializedNode)
        {
            string nodeId = serializedNode.FindPropertyRelative("m_id").stringValue;
            serializedNode.FindPropertyRelative("m_outports").arraySize = 0;
            serializedNode.serializedObject.ApplyModifiedProperties();
            OnNodeAllOutportsRemoved?.Invoke(nodeId);
        }

        /// For SerializedProperties ///
        public static readonly string NodeCollection_VarName = "m_nodeCollection";
        public static readonly string GraphProperties_VarName = "GraphProperties";
        public static readonly string BlackboardData_VarName = "m_blackboardData";

#endif
    }
}