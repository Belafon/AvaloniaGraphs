using System.Collections.ObjectModel;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DynamicData;

namespace AvaloniaGraphs.GraphControl;

public partial class Graph : UserControl
{
	public ObservableCollection<GraphNode> Nodes { get; private set; } = new();
	public ObservableCollection<GraphEdge> Edges { get; private set; } = new();
	public Dictionary<GraphNode, List<GraphEdge>> EdgesByNode = new();
	public GraphLayout? Layout { 
		get; 
		set; 
	}
	public bool ApplyLayoutOnEachAdd { get; set; } = true;
	public Graph()
	{
		Nodes.CollectionChanged += (sender, args) =>
		{
			if(Layout is not null && ApplyLayoutOnEachAdd)
			{
				Layout.ApplyLayout(this);
			}
			
			// Remove edges that are connected to removed nodes
			if(args.OldItems is not null)
			{
				foreach(GraphNode node in args.OldItems)
				{
					foreach(var edge in EdgesByNode[node])
					{
						Edges.Remove(edge);
					}
					EdgesByNode.Remove(node);
				}
			}
			
		};
		Edges.CollectionChanged += (sender, args) =>
		{
			if(Layout is not null && ApplyLayoutOnEachAdd)
			{
				Layout.ApplyLayout(this);
			}
			
			if(args.NewItems is not null)
			{
				foreach(GraphEdge edge in args.NewItems)
				{
					addNewEdgeToDictionary(edge);
				}
			}
		};
		
		foreach (var edge in Edges)
		{
			addNewEdgeToDictionary(edge);
		}
		
		if(Nodes.Count > 0)
		{
			Layout?.ApplyLayout(this);
		}
	}

	private void addNewEdgeToDictionary(GraphEdge edge)
	{
		if (!EdgesByNode.ContainsKey(edge.Start))
		{
			EdgesByNode[edge.Start] = new();
		}
		if (!EdgesByNode.ContainsKey(edge.End))
		{
			EdgesByNode[edge.End] = new();
		}
		EdgesByNode[edge.Start].Add(edge);
		EdgesByNode[edge.End].Add(edge);
	}

}
