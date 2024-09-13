using Avalonia;

namespace AvaloniaGraphs.GraphControl;

public interface GraphLayout
{
	public void ApplyLayout(Graph graph);
	public int Width { get; set; }
	public int Height { get; set; }
	public Point StartPosition { get; set; }
	public void AddSubGraph(SubGraph subGraph, Graph graph);
}