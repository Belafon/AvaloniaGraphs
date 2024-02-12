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
	public GraphEdge(GraphNode start, GraphNode end, 
	int thickness = 10)
	{
		Start = start;
		End = end;
		Color = new SolidColorBrush(Colors.Red);
		Thickness = thickness;
	}
}