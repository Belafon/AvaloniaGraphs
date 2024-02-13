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
using DynamicData;
using System.Collections.Specialized;
using Avalonia.Input;
using System.Collections.Generic;

namespace AvaloniaGraphs.GraphControl;

public partial class GraphView : UserControl
{
	private Point startPointOfView = new Point(0, 0);
	private Canvas graphCanvas;
	private GraphControlViewModel model = null!;
	private GraphNode? draggdNode = null!;
	private bool movingCanvas = false;
	private Point lastPointerPositionWhenPressedOnCanvas = new Point(0, 0);

	public GraphView(Graph graph)
	{
		this.DataContext = model = new GraphControlViewModel { Graph = graph };
		InitializeComponent();
		graphCanvas = this.FindControl<Canvas>("canvas")!;

		drawEdges(graph);
		drawNodes(graph);

		foreach (var node in graph.Nodes)
		{
			node.OnNodePointerPressedHandler = new EventHandler<PointerPressedEventArgs>((sender, e) =>
			{
				if (e.GetCurrentPoint(node).Properties.IsLeftButtonPressed)
				{
					draggdNode = node;
				}
			});

			node.PointerPressed += node.OnNodePointerPressedHandler;


			node.OnRealPositionChangedHandler = new EventHandler<EventArgsWithPositionDiff>((s, e) =>
			{
				if (s is GraphNode node)
				{
					var position = new Point(node.RealPosition.X, node.RealPosition.Y);
					foreach (var change in changes)
					{
						if (change is ScrollChange scrollChange)
							position = mapNodePositionDuringCanvasScaling(scrollChange.PointerPosition, position, scrollChange.scale, node);
						else if (change is MoveChnage moveChnage)
							position = position + moveChnage.Diff;
					}
					node.PositionInCanvas = position;
				}
			});
		}

		graphCanvas.PointerPressed += (sender, e) =>
		{
			if (e.GetCurrentPoint(graphCanvas).Properties.IsLeftButtonPressed)
			{
				if (draggdNode is null)
				{
					movingCanvas = true;
					lastPointerPositionWhenPressedOnCanvas = new Point(e.GetPosition(graphCanvas).X, e.GetPosition(graphCanvas).Y);
				}
			}
		};

		graphCanvas.PointerReleased += (sender, e) =>
		{
			draggdNode = null;
			movingCanvas = false;
		};

		graphCanvas.PointerMoved += (sender, e) =>
		{
			if (draggdNode is not null && e.GetCurrentPoint(graphCanvas).Properties.IsLeftButtonPressed)
			{
				var x = e.GetPosition(graphCanvas).X - draggdNode.Width / 2;
				var y = e.GetPosition(graphCanvas).Y - draggdNode.Height / 2;
				draggdNode.Model.PositionInCanvas = new Point(x, y);
				draggdNode.Model.RealPosition = new Point(x + startPointOfView.X, y + startPointOfView.Y);
			}

			if (movingCanvas && e.GetCurrentPoint(graphCanvas).Properties.IsLeftButtonPressed)
			{
				var currentPointerPosition = e.GetPosition(graphCanvas);
				var xDiff = currentPointerPosition.X - lastPointerPositionWhenPressedOnCanvas.X;
				var yDiff = currentPointerPosition.Y - lastPointerPositionWhenPressedOnCanvas.Y;
				startPointOfView = new Point(startPointOfView.X + xDiff, startPointOfView.Y + yDiff);
				lastPointerPositionWhenPressedOnCanvas = currentPointerPosition;
				foreach (var node in graph.Nodes)
				{
					node.Model.PositionInCanvas = new Point(node.Model.PositionInCanvas.X + xDiff, node.Model.PositionInCanvas.Y + yDiff);
				}

				changes.Add(new MoveChnage
				{
					Diff = new Point(xDiff, yDiff)
				});
			}
		};

		graphCanvas.PointerWheelChanged += (sender, e) =>
		{
			// zoom in and out
			var delta = e.Delta.Y;
			var scale = delta < 0 ? 1.1 : 0.9;
			changes.Add(new ScrollChange
			{
				PointerPosition = e.GetPosition(graphCanvas),
				scale = scale
			});

			var currentPointerPosition = e.GetPosition(graphCanvas);
			foreach (var node in graph.Nodes)
			{
				node.PositionInCanvas = mapNodePositionDuringCanvasScaling(currentPointerPosition, node.PositionInCanvas, scale, node);
			}
		};

		graph.Nodes.CollectionChanged += (sender, args) =>
		{
			if (args.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (GraphNode node in args.NewItems!)
				{
					var x = node.Model.PositionInCanvas.X - startPointOfView.X;
					var y = node.Model.PositionInCanvas.Y - startPointOfView.Y;
					node.Model.PositionInCanvas = new Point(x, y);
				}
			}
			else if (args.Action == NotifyCollectionChangedAction.Remove)
			{
				foreach (GraphNode node in args.OldItems!)
				{
					node.PointerPressed -= node.OnNodePointerPressedHandler;

				}
			}
		};

	}

	private Point mapNodePositionDuringCanvasScaling(Point currentPointerPosition, Point nodePosition, double scale, GraphNode node)
	{
		var xDiff = currentPointerPosition.X - nodePosition.X;
		var yDiff = currentPointerPosition.Y - nodePosition.Y;
		var x = nodePosition.X + xDiff * (scale - 1);
		var y = nodePosition.Y + yDiff * (scale - 1);
		return new Point(x, y);
	}

	private List<Change> changes = new List<Change>(); // TODO change the way, avoid storing all changes, This is not efficient

	private interface Change { }
	private class MoveChnage : Change
	{
		public Point Diff { get; set; }
	}
	private class ScrollChange : Change
	{
		public Point PointerPosition { get; set; }
		public double scale { get; set; }
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

			if (edge.IsDirected)
			{
				var polygon = drawArrowHead(edge, line);
				bindArrowHeadToLine(edge, line, polygon);
			}
		}
	}

	private void bindArrowHeadToLine(GraphEdge edge, Line line, Polygon arrowHead)
	{
		edge.PropertyChanged += (s, e) =>
			{
				updateArrowHead(edge, line, arrowHead);
			};

		edge.End.Model.PropertyChanged += (s, e) =>
		{
			updateArrowHead(edge, line, arrowHead);
		};

		edge.Start.Model.PropertyChanged += (s, e) =>
		{
			updateArrowHead(edge, line, arrowHead);
		};
	}
	
	private void updateArrowHead(GraphEdge edge, Line line, Polygon arrowHead)
	{
		var direction = line.EndPoint - line.StartPoint;
		var lineLength = Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
		// normalize the direction, the length of the direction vector is 1
		direction = new Point(direction.X / lineLength, direction.Y / lineLength);

		// move the arrow head 1/3 of the line length from the end point
		var mainPoint = line.EndPoint - (line.EndPoint - line.StartPoint) * 1 / 3;
		arrowHead.Points = new Point[]
		{
					new Point(mainPoint.X, mainPoint.Y),
					new Point(mainPoint.X - edge.ArrowHeadLength * direction.X - edge.ArrowHeadWidth * direction.Y, mainPoint.Y - edge.ArrowHeadLength * direction.Y + edge.ArrowHeadWidth * direction.X),
					new Point(mainPoint.X - edge.ArrowHeadLength * direction.X + edge.ArrowHeadWidth * direction.Y, mainPoint.Y - edge.ArrowHeadLength * direction.Y - edge.ArrowHeadWidth * direction.X)
		};
	}

	private Polygon drawArrowHead(GraphEdge edge, Line line)
	{

		var arrowHead = new Polygon();
		var direction = line.EndPoint - line.StartPoint;
		var lineLength = Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
		// normalize the direction, the length of the direction vector is 1
		direction = new Point(direction.X / lineLength, direction.Y / lineLength);

		// move the arrow head 1/3 of the line length from the end point
		var mainPoint = line.EndPoint - (line.EndPoint - line.StartPoint) * 1 / 3;
		arrowHead.Points = new Point[]
		{
					new Point(mainPoint.X, mainPoint.Y),
					new Point(mainPoint.X - edge.ArrowHeadLength * direction.X - edge.ArrowHeadWidth * direction.Y, mainPoint.Y - edge.ArrowHeadLength * direction.Y + edge.ArrowHeadWidth * direction.X),
					new Point(mainPoint.X - edge.ArrowHeadLength * direction.X + edge.ArrowHeadWidth * direction.Y, mainPoint.Y - edge.ArrowHeadLength * direction.Y - edge.ArrowHeadWidth * direction.X)
		};
		arrowHead.Fill = edge.ArrowHeadColor;
		arrowHead.SetValue(Canvas.ZIndexProperty, 3);
		graphCanvas.Children.Add(arrowHead);
		return arrowHead;
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
				graphCanvas.Children.Add(node);
			}
		}
	}

	private static void bindEdgesStart(GraphEdge edge, Line line)
	{
		edge.Start.Model.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == "PositionInCanvas")
			{
				double centerX = edge.Start.Model.PositionInCanvas.X + edge.Start.Width / 2;
				double centerY = edge.Start.Model.PositionInCanvas.Y + edge.Start.Height / 2;
				var centerPoint = new Point(centerX, centerY);

				line.StartPoint = centerPoint;
			}
		};

		double centerX = edge.Start.Model.PositionInCanvas.X + edge.Start.Width / 2;
		double centerY = edge.Start.Model.PositionInCanvas.Y + edge.Start.Height / 2;
		var centerPoint = new Point(centerX, centerY);

		line.StartPoint = centerPoint;
	}

	private static void bindEdgesEnd(GraphEdge edge, Line line)
	{
		edge.End.Model.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == "PositionInCanvas")
			{
				double centerX = edge.End.Model.PositionInCanvas.X + edge.End.Width / 2;
				double centerY = edge.End.Model.PositionInCanvas.Y + edge.End.Height / 2;
				var centerPoint = new Point(centerX, centerY);

				line.EndPoint = centerPoint;
			}
		};

		double centerX = edge.End.Model.PositionInCanvas.X + edge.End.Width / 2;
		double centerY = edge.End.Model.PositionInCanvas.Y + edge.End.Height / 2;
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

