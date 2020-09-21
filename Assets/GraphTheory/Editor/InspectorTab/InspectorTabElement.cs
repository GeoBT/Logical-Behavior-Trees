﻿using GraphTheory.Editor.UIElements;
using UnityEditor;

namespace GraphTheory.Editor
{
    public class InspectorTabElement : TabContentElement
    {
        private NodeGraph m_nodeGraph = null;
        public GraphInspector GraphInspector { get; private set; }
        public NodeInspector NodeInspector { get; private set; }

        public InspectorTabElement(NodeGraphView nodeGraphView)
        {
            Add(GraphInspector = new GraphInspector(nodeGraphView));
            Add(NodeInspector = new NodeInspector());
            NodeInspector.SetVisible(false);
        }

        public void SetOpenNodeGraph(NodeGraph nodeGraph)
        {
            if (nodeGraph == null)
            {
                Reset();
                return;
            }

            m_nodeGraph = nodeGraph;
            GraphInspector.SetNodeGraph(nodeGraph);
        }

        private void Reset()
        {
            m_nodeGraph = null;
            GraphInspector.Reset();
            NodeInspector.Reset();
        }

        public void SetNode(ANode node, SerializedProperty serializedNode)
        {
            GraphInspector.SetVisible(node == null);
            NodeInspector.SetVisible(node != null);
            if (node != null)
            {
                NodeInspector.SetNode(node, serializedNode);
            }
        }

        public override void DeserializeData(string data)
        {
        }

        public override string GetSerializedData()
        {
            return ""; 
        }
    }
}