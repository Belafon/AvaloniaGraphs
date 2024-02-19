using System;
using System.ComponentModel;
using Avalonia.Media;
using ReactiveUI;

namespace AvaloniaGraphs.GraphControl;

public class GraphEdge : ReactiveObject, INotifyPropertyChanged
{
	private GraphNode start = null!;
	public GraphNode Start { 
		get => start;
		set => this.RaiseAndSetIfChanged(ref start, value);
	}
	private GraphNode end = null!;
	public GraphNode End { 
		get => end;
		set => this.RaiseAndSetIfChanged(ref end, value);
	}
	private SolidColorBrush? color;
	public SolidColorBrush? Color { 
		get => color;
		set => this.RaiseAndSetIfChanged(ref color, value);
	}
	public int Thickness { get; set; }
	public bool isDirected = false;
	public bool IsDirected { 
		get => isDirected;
		set => this.RaiseAndSetIfChanged(ref isDirected, value);
	}
	
	private SolidColorBrush? arrowHeadColor;
	public SolidColorBrush? ArrowHeadColor { 
		get => arrowHeadColor;
		set => this.RaiseAndSetIfChanged(ref arrowHeadColor, value);
	}
	
	private double arrowHeadLength = 12;
	public double ArrowHeadLength { 
		get => arrowHeadLength;
		set => this.RaiseAndSetIfChanged(ref arrowHeadLength, value);
	}
	private double arrowHeadWidth = 12;
	public double ArrowHeadWidth { 
		get => arrowHeadWidth;
		set => this.RaiseAndSetIfChanged(ref arrowHeadWidth, value);
	}
	public GraphEdge(GraphNode start, GraphNode end, 
	int thickness = 10)
	{
		Start = start;
		End = end;
		Color = new SolidColorBrush(Colors.Red);
		Thickness = thickness;
		IsDirected = false;
		ArrowHeadColor = new SolidColorBrush(Colors.Red);
	}
	
	public EventHandler<GraphEdge>? EdgeRemovedEventHandler;

}