using Logical;
using System;

[Serializable]
[Node(typeof(TemplateNodeGraph))]
public class TemplateNode : ANode
{
    //============= WARNING Instance Variables WARNING =============//
    // Modifying instance variables from within the node is highly NOT recommended.
    // The Logical system greatly optimizes its memory footprint by using the same graph instance
    // when running all graphs of the same graph type. That means instance variables stored in this node
    // class will be shared among all of your graphs! 
    // 
    // Instead, the fully supported alternative is to access and modify the GraphProperties of the graph
    // through the provided GraphControls class in the overridable methods.
    //
    // In the event where you absolutely need instance variables that will be modified by the nodes, you can check
    // the "CreateGraphInstance" boolean in the GraphController monobehaviour so that the system creates
    // a unique instance of the graph asset before running each of your graphs. This way, no instance
    // variables will be shared across all your graph nodes.
    //==============================================================//


    // CORE METHODS TO OVERWRITE:
    // OnNodeEnter: Gets called when the graph enters this node.
    // OnNodeUpdate: Gets called every update while the node is the currently active node of the graph.
    // OnNodeExit: Gets called when the node finishes or the graph is stopped while this node is currently active.
    //
    // The GraphControls variable holds all the pertinent information and functionality from the graph for the node 
    // to run. This includes the graph's GraphProperties, the graph's BlackboardData, and a method to traverse to a connected node.
    // ex. graphControls.TraverseEdge(this, 0);

    /// <summary>
    /// Method that gets called when the graph enters this node.
    /// </summary>
    public override void OnNodeEnter(GraphControls graphControls)
    {
    }

    /// <summary>
    /// Method that gets called every update while the node is the currently active node of the graph.
    /// </summary>
    public override void OnNodeUpdate(GraphControls graphControls)
    {
    }

    /// <summary>
    /// Method that gets called when the node finishes or the graph is stopped while this node is currently active.
    /// </summary>
    public override void OnNodeExit(GraphControls graphControls)
    {
    }

    /// ===== IMPORTANT =====
    /// Make sure the following properties live within the UNITY_EDITOR preprocessor directive.
    /// Otherwise, your builds may fail due to "No suitable method found to override".
    /// =====================
#if UNITY_EDITOR
    /// <summary>
    /// This property sets the default number of outports for the node when the node is created from within the 
    /// graph editor. To dynamically add more outports, you can create a custom NodeViewDrawer and add the 
    /// necessary functionality.
    /// </summary>
    public override int DefaultNumOutports { get { return 1; } }

    /// <summary>
    /// By default, a node's property drawer in the graph editor's inspector uses a VisualElement property drawer.
    /// If you're more comfortable creating IMGUI property drawers instead, override this method to true and
    /// the inspector will draw this particular node as IMGUI instead.
    /// </summary>
    public override bool UseIMGUIPropertyDrawer { get { return false; } } // Toggle on for LogicalGraphWindow to draw these using IMGUI. Defaulted to use UIToolkit.
#endif

}
