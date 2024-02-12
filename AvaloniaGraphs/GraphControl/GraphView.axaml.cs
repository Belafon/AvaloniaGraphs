using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using AvaloniaGraphs.ViewModels;
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using Avalonia.Media;
using Avalonia.Platform;
using System.ComponentModel;

namespace AvaloniaGraphs.GraphControl;

public partial class GraphView : UserControl
{
	private Canvas graphCanvas;
	private GraphControlViewModel model = null!;

	public GraphView(Graph graph)
	{
		this.DataContext = model = new GraphControlViewModel { Graph = graph };
		InitializeComponent();
		graphCanvas = this.FindControl<Canvas>("canvas")!;

		drawEdges(graph);
		drawNodes(graph);
	}

	private void drawEdges(Graph graph)
	{
		foreach (var edge in graph.Edges)
		{
			var line = new Line();
			line.Bind(Line.StrokeProperty, new Binding()
			{
				Source = edge,
				Path = nameof(edge.Color)
			});
			line.Bind(Line.StrokeThicknessProperty, new Binding()
			{
				Source = edge,
				Path = nameof(edge.Thickness)
			});

			bindEdgesStart(edge, line);
			bindEdgesEnd(edge, line);

			// set z-index
			line.SetValue(Canvas.ZIndexProperty, 1);			

			graphCanvas.Children.Add(line);
		}
	}

	private void drawNodes(Graph graph)
	{
		foreach (var node in graph.Nodes)
		{
			if (node is not null)
			{
				node.Bind(Canvas.LeftProperty, new Binding()
				{
					Source = node.Model,
					Path = nameof(node.Model.X)
				});
				node.Bind(Canvas.TopProperty, new Binding()
				{
					Source = node.Model,
					Path = nameof(node.Model.Y)
				});
				node.SetValue(Canvas.ZIndexProperty, 2);
				
				// add dragging effect, when dropping over the canvas, change position to the pointers position
				node.Dragged
				
				graphCanvas.Children.Add(node);
			}
		}
	}

	private static void bindEdgesStart(GraphEdge edge, Line line)
	{
		edge.Start.Model.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == "Position")
			{
				double centerX = edge.Start.Model.Position.X + edge.Start.Width / 2;
				double centerY = edge.Start.Model.Position.Y + edge.Start.Height / 2;
				var centerPoint = new Point(centerX, centerY);

				line.StartPoint = centerPoint;
			}
		};

		double centerX = edge.Start.Model.Position.X + edge.Start.Width / 2;
		double centerY = edge.Start.Model.Position.Y + edge.Start.Height / 2;
		var centerPoint = new Point(centerX, centerY);

		line.StartPoint = centerPoint;
	}

	private static void bindEdgesEnd(GraphEdge edge, Line line)
	{
		edge.End.Model.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == "Position")
			{
				double centerX = edge.End.Model.Position.X + edge.End.Width / 2;
				double centerY = edge.End.Model.Position.Y + edge.End.Height / 2;
				var centerPoint = new Point(centerX, centerY);

				line.EndPoint = centerPoint;
			}
		};

		double centerX = edge.End.Model.Position.X + edge.End.Width / 2;
		double centerY = edge.End.Model.Position.Y + edge.End.Height / 2;
		var centerPoint = new Point(centerX, centerY);

		line.EndPoint = centerPoint;
	}
}

public class GraphControlViewModel : ViewModelBase
{

	private Graph graph = null!;
	public Graph Graph
	{
		get => graph;
		set => this.RaiseAndSetIfChanged(ref graph, value);
	}
}

