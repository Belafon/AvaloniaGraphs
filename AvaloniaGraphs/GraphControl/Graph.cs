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
	public GraphLayout? Layout { get; set; }
	public Graph()
	{
		Nodes.CollectionChanged += (sender, args) =>
		{
			if(Layout is not null)
			{
				Layout.ApplyLayout(this);
			}
		};
		Edges.CollectionChanged += (sender, args) =>
		{
			if(Layout is not null)
			{
				Layout.ApplyLayout(this);
			}
		};
	}

	
}
