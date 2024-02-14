using System.Collections.ObjectModel;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DynamicData;

namespace AvaloniaGraphs.GraphControl;

public partial class Graph : UserControl
{
	public ObservableCollection<GraphNode> Nodes { get; set; } = new();
	public ObservableCollection<GraphEdge> Edges { get; set; } = new();
	private Dictionary<GraphNode, List<GraphEdge>> _edgesByNode = new();
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
					foreach(var edge in _edgesByNode[node])
					{
						Edges.Remove(edge);
					}
					_edgesByNode.Remove(node);
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
		if (!_edgesByNode.ContainsKey(edge.Start))
		{
			_edgesByNode[edge.Start] = new();
		}
		if (!_edgesByNode.ContainsKey(edge.End))
		{
			_edgesByNode[edge.End] = new();
		}
		_edgesByNode[edge.Start].Add(edge);
		_edgesByNode[edge.End].Add(edge);
	}

}
