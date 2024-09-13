using System.Collections.ObjectModel;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DynamicData;
using System;
using System.Linq;
using System.ComponentModel;

namespace AvaloniaGraphs.GraphControl;

public class Graph : INotifyPropertyChanged
{
	public ObservableCollection<GraphNode> Nodes { get; private set; } = new();
	
	private ObservableCollection<GraphEdge> edges = new();
	public ObservableCollection<GraphEdge> Edges { 
		get => edges; 
		private set
		{
			edges = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Edges)));
		}
	}

	public Dictionary<GraphNode, List<GraphEdge>> EdgesByNode = new();

	public event PropertyChangedEventHandler? PropertyChanged;

	public GraphLayout? Layout { 
		get; 
		set; 
	}
	public bool ApplyLayoutOnEachAdd { get; set; } = true;
	public bool ApplyLayoutOnEachSubGraphAdd { get; set; } = true;
	
	public void ApplyLayout(){
		Layout?.ApplyLayout(this);
	}

	public Graph()
	{
		Nodes.CollectionChanged += (sender, args) =>
		{
			// Remove edges that are connected to removed nodes
			if(args.OldItems is not null)
			{
				foreach(GraphNode node in args.OldItems)
				{
					if(EdgesByNode.ContainsKey(node))
					{
						var edges = EdgesByNode[node].ToList();
						foreach(var edge in edges)
						{
							Edges.Remove(edge);
						}
						EdgesByNode.Remove(node);
					}
				}
			}
			
		};
		Edges.CollectionChanged += (sender, args) =>
		{
			if(args.NewItems is not null)
			{
				foreach(GraphEdge edge in args.NewItems)
				{
					addNewEdgeToDictionary(edge);
				}
			}
			else if(args.OldItems is not null)
			{
				foreach(GraphEdge edge in args.OldItems)
				{
					if(EdgesByNode.ContainsKey(edge.Start))
					{
						EdgesByNode[edge.Start].Remove(edge);
					}
					if(EdgesByNode.ContainsKey(edge.End))
					{
						EdgesByNode[edge.End].Remove(edge);
					}

					edge.EdgeRemovedEventHandler?.Invoke(this, edge);
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

    internal void AddAllNodes(List<GraphNode> nodes)
    {
		for (int i = 0; i < nodes.Count; i++)
		{
			Nodes.Add(nodes[i]);
		}
    }

    internal void AddAllEdges(List<GraphEdge> edges)
    {
		for (int i = 0; i < edges.Count; i++)
		{
			Edges.Add(edges[i]);
		}
    }
}
