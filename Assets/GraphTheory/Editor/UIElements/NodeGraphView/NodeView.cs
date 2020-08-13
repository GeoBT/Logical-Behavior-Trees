﻿using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphTheory.Editor.UIElements
{
    public class NodeView : Node
    {
        private NodeGraphView m_nodeGraphView = null;
        public ANode Node { get; private set; } = null;

        public string NodeId { get { return Node != null ? Node.Id : string.Empty; } }
        public PortView Inport { get; } = null;
        private List<PortView> m_outports = new List<PortView>();
        private List<EdgeView> m_edgeViews = new List<EdgeView>();
        private IEdgeConnectorListener m_edgeConnectorListener = null;

        public NodeView(ANode node, NodeGraphView nodeGraphView, IEdgeConnectorListener edgeConnectorListener) : base()
        {
            Node = node;
            m_nodeGraphView = nodeGraphView;
            m_edgeConnectorListener = edgeConnectorListener;
            bool isEntryNode = Node is BuiltInNodes.EntryNode;
            if (Node != null)
            {
                title = Node.Name;

                if (isEntryNode)
                {
                    this.capabilities = this.capabilities & (~Capabilities.Deletable);
                }

                Node.DrawNodeView(this);

                if (!isEntryNode)
                {
                    //Add ports
                    Inport = new PortView(this,
                        Orientation.Horizontal,
                        Direction.Input,
                        Port.Capacity.Single,
                        typeof(bool),
                        0,
                        m_edgeConnectorListener);
                    Inport.portName = "";
                    inputContainer.Add(Inport);
                }

                for (int j = 0; j < Node.NumOutports; j++)
                {
                    PortView newPort = new PortView(this,
                        Orientation.Horizontal,
                        Direction.Output,
                        Port.Capacity.Single,
                        typeof(bool),
                        j,
                        m_edgeConnectorListener);
                    newPort.portName = $"Outport {m_outports.Count}";
                    m_outports.Add(newPort);
                    outputContainer.Add(newPort);
                }
                RefreshExpandedState();
                RefreshPorts();
                
                SetPosition(new Rect(Node.Position, Node.Size));

                //this.RegisterCallback<GeometryChangedEvent>((GeometryChangedEvent gce) => { Debug.Log(gce.newRect.position); });
            }
        }

        public void OnLoadView()
        {
            List<OutportEdge> edges = Node.GetAllEdges();
            for (int k = 0; k < edges.Count; k++)
            {
                if (!edges[k].IsValid)
                {
                    continue;
                }
                EdgeView edgeView = new EdgeView()
                {
                    OutportEdge = edges[k],
                    input = m_nodeGraphView.GetNodeViewById(edges[k].ConnectedNodeId).Inport,
                    output = m_outports[k],
                };
                edgeView.Setup();
                AddEdgeView(edgeView);
            }
        }

        public void OnUnloadView()
        {
            for(int i = m_edgeViews.Count - 1; i >= 0; i--)
            {
                if (m_nodeGraphView.Contains(m_edgeViews[i]))
                {
                    m_nodeGraphView.RemoveElement(m_edgeViews[i]);
                }
            }
        }
        
        public void OnDeleteNode()
        {
            for (int i = m_edgeViews.Count - 1; i >= 0; i--)
            {
                RemoveEdge(m_edgeViews[i]);
            }
        }

        public bool OutportHasEdge(int outportIndex)
        {
            return Node.GetOutportEdge(outportIndex).IsValid;
        }

        public PortView GetOutport(int outportIndex)
        {
            return m_outports[outportIndex];
        }

        public void AddEdge(EdgeView edgeView)
        {
            if (edgeView.FirstPort.Node == this)
            {
                OutportEdge outportEdge = new OutportEdge() { ConnectedNodeId = edgeView.SecondPort.Node.NodeId };
                Node.AddOutportEdge(edgeView.FirstPort.PortIndex, outportEdge);
                edgeView.OutportEdge = outportEdge;
            }
            AddEdgeView(edgeView);
        }

        public void AddEdgeView(EdgeView edgeView)
        {
            if (edgeView.FirstPort.Node == this)
            {
                m_edgeViews.Add(edgeView);
                edgeView.FirstPort.Connect(edgeView);
                edgeView.SecondPort.Node.AddEdgeView(edgeView);
                m_nodeGraphView.Add(edgeView);
            }
            else if(edgeView.SecondPort.Node == this)
            {
                edgeView.SecondPort.Connect(edgeView);
            }
        }

        public void RemoveEdge(EdgeView edgeView)
        {
            if(edgeView.FirstPort.Node == this)
            {
                edgeView.FirstPort.Disconnect(edgeView);
                edgeView.SecondPort.Node.RemoveEdge(edgeView);
                if (edgeView.parent != null && edgeView.parent is GraphView)
                {
                    (edgeView.parent as GraphView).RemoveElement(edgeView);
                }
                m_edgeViews.Remove(edgeView);
                Node.RemoveOutportEdge(edgeView.FirstPort.PortIndex);
            }
            else if(edgeView.SecondPort.Node == this)
            {
                edgeView.SecondPort.Disconnect(edgeView);
            }
        }

        public EdgeView GetEdgeViewById(string id)
        {
            return m_edgeViews.Find(x => x.EdgeId == id);
        }

        public void UpdateNodeDataPosition()
        {
            Node.Position = this.GetPosition().position;
        }
    }
}