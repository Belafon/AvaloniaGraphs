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
using DynamicData;
using System.Collections.Specialized;
using Avalonia.Input;
using System.Collections.Generic;
using System.Linq;
using Avalonia.VisualTree;

namespace AvaloniaGraphs.GraphControl;

/// <summary>
/// subgraph container can be hidden and shown
/// when it is hidden, all nodes of it are hidden and all edges are
/// connected to the main node of the subgraph
/// 
/// When it is shown, we have to momorize the distances from a subgraph node
/// to the main node and move the nodes to the right position.
/// 
/// </summary>
public partial class GraphView : UserControl
{
	private Point GetPositionOfRealStartInCanvas()
	{
		return getPositionInCanvasFromoRealPosition(new Point(0, 0));
	}
	private Canvas graphCanvas;
	private GraphNode? draggedNode = null!;
	private bool movingCanvas = false;
	private bool scalingCanvas = false;
	private Graph FirstLevelGraph;

	/// <summary>
	/// It is loaded during drawing edges
	/// </summary>
	/// <returns></returns>
	private Dictionary<GraphNode, List<GraphEdge>> allEdgesByNode = new();
	public GraphView(Graph graph)
	{
		InitializeComponent();
		graphCanvas = this.FindControl<Canvas>("canvas")!;

/*		var rectangle = new GraphNode
		{
			Width = graph.Layout!.Width,
			Height = graph.Layout!.Height,
			ContentControl = new Rectangle()
			{
				Fill = new SolidColorBrush(Colors.DarkGray),
			},
			Background = new SolidColorBrush(Colors.DarkGray),
		};

		var rectangle2 = new GraphNode
		{
			Width = graph.Layout!.Width / 2,
			Height = graph.Layout!.Height / 2,
			ContentControl = new Rectangle()
			{
				Fill = new SolidColorBrush(Colors.Gray),
				Width = graph.Layout!.Width / 2,
				Height = graph.Layout!.Height,
			},
			Background = new SolidColorBrush(Colors.Gray),
		};*/


		InitializeGraphView(graph);
/*
		rectangle2.SetValue(Canvas.ZIndexProperty, -9);
		rectangle2.ContentControl.SetValue(Canvas.ZIndexProperty, -9);
		Canvas.SetLeft(rectangle2, graph.Layout.StartPosition.X + graph.Layout.Width / 2);
		Canvas.SetTop(rectangle2, graph.Layout.StartPosition.Y);


		rectangle.SetValue(Canvas.ZIndexProperty, -10);
		rectangle.ContentControl.SetValue(Canvas.ZIndexProperty, -10);
		Canvas.SetLeft(rectangle, graph.Layout.StartPosition.X);
		Canvas.SetTop(rectangle, graph.Layout.StartPosition.Y);

		graphCanvas.Children.Add(rectangle);
		graphCanvas.Children.Add(rectangle2);
*/
		
		Queue<SubGraph> subGraphsToCheck = new();
		foreach (var node in graph.Nodes)
		{
			if (node is SubGraph subGraph)
			{
				subGraphsToCheck.Enqueue(subGraph);
			}
		}

		while (subGraphsToCheck.Count > 0)
		{
			var subGraph = subGraphsToCheck.Dequeue();
			checkVisibilityOfNodes(subGraph);
			if(!subGraph.IsVisible)
				continue;

			foreach (var node in subGraph.Graph.Nodes)
			{
				if (node is SubGraph subGraphIn)
				{
					subGraphsToCheck.Enqueue(subGraphIn);
				}
			}
		}
	}

	private void InitializeGraphView(Graph graph)
	{
		FirstLevelGraph = graph;


		Point lastPointerPositionWhenPressedOnCanvas = new Point(0, 0);

		graphCanvas.PointerPressed += (sender, e) =>
		{
			if (e.GetCurrentPoint(graphCanvas).Properties.IsLeftButtonPressed)
			{
				if (draggedNode is null)
				{
					movingCanvas = true;
					lastPointerPositionWhenPressedOnCanvas = new Point(e.GetPosition(graphCanvas).X, e.GetPosition(graphCanvas).Y);
				}
			}
		};

		graphCanvas.PointerMoved += (sender, e) =>
		{
			// test

			//testMap();
			//mapTest();

			if (draggedNode is not null
				&& e.GetCurrentPoint(graphCanvas).Properties.IsLeftButtonPressed)
			{
				var x = e.GetPosition(graphCanvas).X;
				var y = e.GetPosition(graphCanvas).Y;
				//draggedNode.PositionInCanvas = new Point(x, y);

				//var realPos = getRealPositionFromPositionInCanvas(draggedNode.PositionInCanvas);
				//draggedNode.RealPosition = realPos;
				var realPos = getRealPositionFromPositionInCanvas(new Point(x, y));
				draggedNode.SetRealPosition(realPos);

				//Console.WriteLine($"Real position: {realPos}");
				//Console.WriteLine($"Canvas position: {draggedNode.PositionInCanvas}");

			}

			if (movingCanvas && e.GetCurrentPoint(graphCanvas).Properties.IsLeftButtonPressed)
			{
				var currentPointerPosition = e.GetPosition(graphCanvas);
				var xDiff = currentPointerPosition.X - lastPointerPositionWhenPressedOnCanvas.X;
				var yDiff = currentPointerPosition.Y - lastPointerPositionWhenPressedOnCanvas.Y;
				lastPointerPositionWhenPressedOnCanvas = currentPointerPosition;

				// addMoveChange(xDiff, yDiff);
				changes.Add(new MoveChnage
				{
					Diff = new Point(xDiff, yDiff)
				});

			}
		};

		graphCanvas.PointerWheelChanged += (sender, e) =>
		{
			scalingCanvas = true;

			// zoom in and out
			var delta = e.Delta.Y;
			var scale = delta <= 0 ? 0.9 : 1.1;
			var currentPointerPosition = e.GetPosition(graphCanvas);

			// change subgraphs containers sizes and checks the visibility content
			OnScalingCanvasEvent?.Invoke(this, new EventArgsWithScale
			{
				scale = scale,
				PointerPosition = currentPointerPosition,
			});

			changes.Add(new ScrollChange
			{
				PointerPosition = e.GetPosition(graphCanvas),
				scale = scale
			});

			scalingCanvas = false;
		};

		drawAllSubGraphsInAGraph(graph);
	}

	private void drawAllSubGraphsInAGraph(Graph graph)
	{
		drawGraph(graph);
		Queue<GraphNode> nodesToCheck = new();
		foreach (var node in graph.Nodes)
		{
			nodesToCheck.Enqueue(node);
		}

		while (nodesToCheck.Count > 0)
		{
			var node = nodesToCheck.Dequeue();
			if (node is SubGraph subGraph)
			{
				drawGraph(subGraph.Graph);
				drawSubGraph(subGraph);
				foreach (var n in subGraph.Graph.Nodes)
				{
					nodesToCheck.Enqueue(n);
				}
			}
		}
	}
	private void drawSubGraph(SubGraph subGraph)
	{
		subGraph.StartBorderNode.Bind(Canvas.LeftProperty, new Binding()
		{
			Source = subGraph.StartBorderNode.Model,
			Path = nameof(subGraph.StartBorderNode.Model.X)
		});
		subGraph.StartBorderNode.Bind(Canvas.TopProperty, new Binding()
		{
			Source = subGraph.StartBorderNode.Model,
			Path = nameof(subGraph.StartBorderNode.Model.Y)
		});

		subGraph.OnMinWidthOrHeightBorderContainerChanged += (s, e) =>
		{
			checkVisibilityOfNodes(subGraph);
		};

		subGraph.SetValue(Canvas.ZIndexProperty, 5);
		// 
		subGraph.Model.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == "PositionInCanvas")
			{
				var position = subGraph.PositionInCanvas;
				subGraph.StartBorderNode.PositionInCanvas = position;

				if (!movingCanvas && !scalingCanvas)
				{
					foreach (var node in subGraph.Graph.Nodes)
					{
						if(!node.IsVisible)
						{
							node.PositionInCanvas = subGraph.PositionInCanvas;
						}
					}
				}
				subGraph.LastPositionInCanvas = subGraph.PositionInCanvas;
			}
		};

		// if subnodes are not visible, then set their canvas position to the position of the subgraph
		subGraph.Model.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == "PositionInCanvas")
			{
				if (!subGraph.StartBorderNode.IsVisible)
				{
					foreach (var node in subGraph.Graph.Nodes)
					{
						node.PositionInCanvas = subGraph.PositionInCanvas;
					}
				}
			}
		};

		graphCanvas.Children.Add(subGraph.StartBorderNode);

		subGraph.OnRealPositionChangedHandler += (s, e) =>
		{
			moveWithAllSubnodesWhenSubGraphIsNotVisibleAndItsPositionChanged(subGraph);
		};

		OnScalingCanvasEvent += (s, e) =>
		{
			var scale = e.scale;
			var pointerPosition = e.PointerPosition;
			changeSizesOfContainersDuringScalingCanvas(subGraph, scale, pointerPosition);
			checkVisibilityOfNodes(subGraph);
		};

		checkVisibilityOfNodes(subGraph);

		subGraph.Graph.Nodes.CollectionChanged += (sender, args) => checkVisibilityOfNodes(subGraph);
	}

	private void moveWithAllSubnodesWhenSubGraphIsNotVisibleAndItsPositionChanged(SubGraph subGraph)
	{
		if (!movingCanvas && !scalingCanvas)
		{
			foreach (var node in subGraph.Graph.Nodes)
			{
				Point position = subGraph.RealPosition;
				Point diff = position - subGraph.LastRealPosition;
				moveNodeAndSubnodesWhenIsNotVisible(node, diff);
			}
		}
		subGraph.LastRealPosition = subGraph.RealPosition;
	}

	private static void moveNodeAndSubnodesWhenIsNotVisible(GraphNode node, Point diff)
	{
		if (node.IsVisible || (node is SubGraph subGraph && subGraph.StartBorderNode.IsVisible))
			node.SetRealPosition(node.RealPosition + diff);
		else
		{
			node.RealPosition += diff;
			node.LastRealPosition += diff;
			if (node is SubGraph subsubGraph)
			{
				foreach (var subNode in subsubGraph.Graph.Nodes)
				{
					subNode.RealPosition += diff;
					subNode.LastRealPosition += diff;
				}
			}
		}
	}

	private void checkVisibilityOfNodes(SubGraph subGraph)
	{
		if (subGraph.StartBorderNode.Width < subGraph.MinWidthOfBorderContainer
			|| subGraph.StartBorderNode.Height < subGraph.MinHeightOfBorderContainer)
		{
			if (subGraph.Graph.Nodes.Count == 0)
				return;

			if (subGraph.StartBorderNode?.IsVisible == false)
				return;

			hideSubgraphsContent(subGraph);
		}
		else
		{
			if (subGraph.Graph.Nodes.Count == 0)
				return;

			if (subGraph.IsVisible == true)
				subGraph.IsVisible = false;

			if (subGraph.StartBorderNode?.IsVisible == true)
				return;

			showSubgraphsContent(subGraph);
		}
	}

	private void showSubgraphsContent(SubGraph subGraph)
	{
		subGraph.IsVisible = false;
		subGraph.StartBorderNode.IsVisible = true;


		Queue<GraphNode> subgraphsToCheck = new();
		foreach (var node in subGraph.Graph.Nodes)
			subgraphsToCheck.Enqueue(node);

		while (subgraphsToCheck.Count > 0)
		{
			var node = subgraphsToCheck.Dequeue();
			node.IsVisible = true;
			/*var position = node.ContainerSubGraph?.PositionInCanvas;
			if(position is not null && node.DistanceToSubGraph is not null)
			{
				Point a = (Point)position! + (Point)node!.DistanceToSubGraph!;
				node.PositionInCanvas = getPositionInCanvasFromoRealPosition(a);
			} 
			else*/ 
			node.PositionInCanvas = getPositionInCanvasFromoRealPosition(node.RealPosition);
		}
	}

	private void hideSubgraphsContent(SubGraph subGraph)
	{
		Stack<SubGraph> subgraphsToHide = new();
		Queue<SubGraph> subgraphsToCheck = new();
		subgraphsToCheck.Enqueue(subGraph);
		while (subgraphsToCheck.Count > 0)
		{
			var subgraph = subgraphsToCheck.Dequeue();
			subgraphsToHide.Push(subgraph);
			foreach (var node in subgraph.Graph.Nodes)
			{
				if (node is SubGraph subGraphInSubGraph)
				{
					subgraphsToCheck.Enqueue(subGraphInSubGraph);
				}
			}
		}

		foreach (var subsubGraph in subgraphsToHide)
		{
			subsubGraph.IsVisible = false;
		}

		subGraph.IsVisible = true;
		subGraph.StartBorderNode.IsVisible = false;

		// change all edges in hiding tree of SubGraphs
		foreach (var subsubGraph in subgraphsToHide)
		{
			foreach (var node in subsubGraph.Graph.Nodes)
			{
				node.DistanceToSubGraph = subGraph.RealPosition - node.RealPosition;
				node.IsVisible = false;
				moveEdgesEndByMovingSiblingNode(node, subGraph);
//				if(!allEdgesByNode.ContainsKey(node)) // TODO 
//					continue;
					
//				foreach (var edge in allEdgesByNode[node])
//				{
//					edge.HideEdgeEventHandler?.Invoke(edge, new Tuple<GraphEdge, SubGraph?>(edge, subGraph));
//				}
			}
		}
	}

	private void moveEdgesEndByMovingSiblingNode(GraphNode sibling, SubGraph? ancestorSubGraph)
	{
		if (ancestorSubGraph is null)
			return;

		if (ancestorSubGraph.IsVisible == false)
			moveEdgesEndByMovingSiblingNode(sibling, ancestorSubGraph.ContainerSubGraph);
		else
			sibling.PositionInCanvas = ancestorSubGraph.PositionInCanvas;
	}

	private void drawGraph(Graph graph)
	{
		drawEdges(graph);
		drawNodes(graph);

		foreach (var node in graph.Nodes)
		{
			bindNodesActions(node);
		}

		Point lastPointerPositionWhenPressedOnCanvas = new Point(0, 0);

		graphCanvas.PointerPressed += (sender, e) =>
		{
			if (e.GetCurrentPoint(graphCanvas).Properties.IsLeftButtonPressed)
			{
				if (draggedNode is null)
				{
					movingCanvas = true;
					lastPointerPositionWhenPressedOnCanvas = new Point(e.GetPosition(graphCanvas).X, e.GetPosition(graphCanvas).Y);
				}
			}
		};

		graphCanvas.PointerReleased += (sender, e) =>
		{
			draggedNode = null;
			movingCanvas = false;
		};

		graphCanvas.PointerMoved += (sender, e) =>
		{

			if (movingCanvas && e.GetCurrentPoint(graphCanvas).Properties.IsLeftButtonPressed)
			{
				var currentPointerPosition = e.GetPosition(graphCanvas);
				var thisChange = changes.LastOrDefault() as MoveChnage;
				if (thisChange is null)
				{
					throw new Exception("LastChange is not this MoveChange");
				}

				var xDiff = thisChange.Diff.X;
				var yDiff = thisChange.Diff.Y;
				foreach (var node in graph.Nodes)
				{
					node.PositionInCanvas = new Point(node.PositionInCanvas.X + xDiff, node.PositionInCanvas.Y + yDiff);
				}

			}
		};

		graphCanvas.PointerWheelChanged += (sender, e) =>
		{
			scalingCanvas = true;

			// zoom in and out
			var delta = e.Delta.Y;
			var scale = delta <= 0 ? 0.9 : 1.1;
			var currentPointerPosition = e.GetPosition(graphCanvas);
			foreach (var node in graph.Nodes)
			{
				// if the node is subgraph and the container is opened and visible, change the nodes position
				if(!node.IsVisible
					&& node is SubGraph subGraph 
					&& subGraph.StartBorderNode.IsVisible)
				{
					node.PositionInCanvas = mapNodePositionDuringCanvasScaling(currentPointerPosition, node.PositionInCanvas, scale);
				}
				
				// if the node is not visible, the position is changed by binding to the subgraph container
				if(node.IsVisible){
					node.PositionInCanvas = mapNodePositionDuringCanvasScaling(currentPointerPosition, node.PositionInCanvas, scale);
				}
			}

			scalingCanvas = false;
		};

		graph.Nodes.CollectionChanged += (sender, args) =>
		{
			if (args.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (GraphNode node in args.NewItems!)
				{
					if (node is SubGraph subGraph)
					{
						throw new NotImplementedException("SubGraph cannot be added to the graph");
						//bool wasRefresh = addSubGraph(subGraph);
						//if (wasRefresh)
						//	continue;
					}

					node.PositionInCanvas = getPositionInCanvasFromoRealPosition(node.RealPosition);
					drawNode(node);
					bindNodesActions(node);
				}
			}
			else if (args.Action == NotifyCollectionChangedAction.Remove)
			{
				foreach (GraphNode node in args.OldItems!)
				{
					if (node is SubGraph subGraph)
						removeSubgraph(subGraph);

					node.PointerPressed -= node.OnNodePointerPressedHandler;
					graphCanvas.Children.Remove(node);
				}
			}
		};
	}

	private bool addSubGraph(SubGraph subGraph)
	{
		if (FirstLevelGraph.ApplyLayoutOnEachSubGraphAdd
			|| FirstLevelGraph.ApplyLayoutOnEachAdd)
		{
			FirstLevelGraph.Layout?.ApplyLayout(FirstLevelGraph);
			refresh();
			return true;
		}

		FirstLevelGraph.Layout?.AddSubGraph(subGraph, subGraph.Graph);
		drawSubGraph(subGraph);
		drawAllSubGraphsInAGraph(subGraph.Graph);

		var nodeToCheck = new Queue<GraphNode>();
		nodeToCheck.Enqueue(subGraph);

		while (nodeToCheck.Count > 0)
		{
			var node = nodeToCheck.Dequeue();
			if (node is SubGraph subGraphIn)
			{
				subGraphIn.Graph.Layout?.ApplyLayout(subGraphIn.Graph);
				foreach (var n in subGraphIn.Graph.Nodes)
					nodeToCheck.Enqueue(n);
			}
		}
		return false;
	}

	private void removeSubgraph(SubGraph subGraph)
	{
		subGraph.PointerPressed -= subGraph.OnNodePointerPressedHandler;
		graphCanvas.Children.Remove(subGraph);

		foreach (var node in subGraph.Graph.Nodes)
		{
			if (node is SubGraph subGraphIn)
			{
				removeSubgraph(subGraphIn);
			}
			else
			{
				node.PointerPressed -= node.OnNodePointerPressedHandler;
				graphCanvas.Children.Remove(node);
			}
		}
	}

	private void addMoveChange(double xDiff, double yDiff)
	{
		if (changes.Count == 0 || changes[changes.Count - 1] is not MoveChnage)
		{
			changes.Add(new MoveChnage
			{
				Diff = new Point(xDiff, yDiff)
			});
			return;
		}

		var lastChange = changes[changes.Count - 1] as MoveChnage;
		lastChange!.Diff = new Point(lastChange.Diff.X + xDiff, lastChange.Diff.Y + yDiff);
	}

	private void changeSizesOfContainersDuringScalingCanvas(SubGraph subGraph, double scale, Point pointerPosition)
	{
		var moveToLeftTopCorner = -new Point(subGraph.StartBorderNode.Width / 2, subGraph.StartBorderNode.Height / 2);
		var mappedStartPoint = mapNodePositionDuringCanvasScaling(pointerPosition, subGraph.StartBorderNode.PositionInCanvas + moveToLeftTopCorner, scale);
		var mappedEndPoint = mapNodePositionDuringCanvasScaling(pointerPosition, subGraph.StartBorderNode.PositionInCanvas - moveToLeftTopCorner, scale);
		subGraph.StartBorderNode.Width = mappedEndPoint.X - mappedStartPoint.X;
		subGraph.StartBorderNode.Height = mappedEndPoint.Y - mappedStartPoint.Y;
	}

	private event EventHandler<EventArgsWithScale>? OnScalingCanvasEvent;

	private void bindNodesActions(GraphNode node)
	{
		node.OnNodePointerPressedHandler = new EventHandler<PointerPressedEventArgs>((sender, e) =>
		{
			if (e.GetCurrentPoint(node).Properties.IsLeftButtonPressed)
			{
				draggedNode = node;
			}
		});

		node.PointerPressed += node.OnNodePointerPressedHandler;


		node.OnRealPositionChangedHandler = new EventHandler<EventArgsWithPositionDiff>((s, e) =>
		{
			if (s is GraphNode node)
			{
				Point position = getPositionInCanvasFromoRealPosition(node.RealPosition);
				node.PositionInCanvas = position;
			}
		});
	}

	private Point getPositionInCanvasFromoRealPosition(Point realPosition)
	{
		var position = new Point(realPosition.X, realPosition.Y);
		foreach (var change in changes)
		{
			if (change is ScrollChange scrollChange)
				position = mapNodePositionDuringCanvasScaling(scrollChange.PointerPosition, position, scrollChange.scale);
			else if (change is MoveChnage moveChnage)
				position = position + moveChnage.Diff;
		}

		return position;
	}

	private Point getRealPositionFromPositionInCanvas(Point positionInCanvas)
	{
		var position = new Point(positionInCanvas.X, positionInCanvas.Y);
		for (int i = changes.Count - 1; i >= 0; i--)
		{
			var change = changes[i];
			if (change is ScrollChange scrollChange)
				position = mapReversePositionDuringCanvasScaling(scrollChange.PointerPosition, position, scrollChange.scale);
			else if (change is MoveChnage moveChnage)
				position = position - moveChnage.Diff;
		}

		return position;
	}

	private Point mapNodePositionDuringCanvasScaling(Point currentPointerPosition, Point nodePosition, double scale)
	{
		var output = new Point(0, 0);
		var diff = (currentPointerPosition - nodePosition) * (1 - scale);
		output = nodePosition + diff;
		return output;
	}

	private Point mapReversePositionDuringCanvasScaling(Point currentPointerPosition, Point nodePositionAfterScaling, double scale)
	{
		var output = new Point(0, 0);
		output = nodePositionAfterScaling - (currentPointerPosition * (1 - scale));
		output = output / scale;
		return output;
	}

	private void mapTest()
	{
		var examplePoint = new Point(1, 1);
		var pointerPositionExample = new Point(100, 100);
		var scaleExample = 1.1;
		var mappedPoint = mapNodePositionDuringCanvasScaling(pointerPositionExample, examplePoint, scaleExample);
		var reversePoint = mapReversePositionDuringCanvasScaling(pointerPositionExample, mappedPoint, scaleExample);
		var secondMappedPoint = mapNodePositionDuringCanvasScaling(pointerPositionExample, reversePoint, scaleExample);
		if (examplePoint.X - reversePoint.X > 0.0001 || examplePoint.Y - reversePoint.Y > 0.0001)
			throw new Exception("Error in mapping");

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
			drawEdge(edge, graph);
		}
		graph.Edges.CollectionChanged += (sender, args) =>
		{
			if (args.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (GraphEdge edge in args.NewItems!)
				{
					drawEdge(edge, graph);
				}
				if (graph.ApplyLayoutOnEachAdd)
					graph.Layout?.ApplyLayout(graph);
			}
		};
	}

	private void drawEdge(GraphEdge edge, Graph graph)
	{
		addEdgeToDictOfAll(edge);

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

		Polygon? arrowHead = null;
		if (edge.IsDirected)
		{
			arrowHead = drawArrowHead(edge, line);
			bindArrowHeadToLine(edge, line, arrowHead);
		}

		edge.EdgeRemovedEventHandler += (sender, args) =>
		{
			// remove the line
			graphCanvas.Children.Remove(line);
			// remove the arrow head
			if (arrowHead is not null)
			{
				graphCanvas.Children.Remove(arrowHead);
			}

			allEdgesByNode[edge.Start].Remove(edge);
		};

		edge.HideEdgeEventHandler += (sender, args) =>
		{
			hideEdge(args, line, arrowHead);
		};

		edge.ShowEdgeEventHandler += (sender, args) =>
		{
			showEdge(args, line, arrowHead);
		};
		
		edge.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == "ZIndex")
			{
				line.SetValue(Canvas.ZIndexProperty, edge.ZIndex);
				if (arrowHead is not null)
					arrowHead.SetValue(Canvas.ZIndexProperty, edge.ZIndex);
			}
		};
	}

	private void hideEdge(Tuple<GraphEdge, SubGraph?> args, Line line, Polygon? arrowHead)
	{
		var edge = args.Item1;
		var subGraph = args.Item2;

		if (isSubgraphSubgraphOfOtherSubgraphOrEqual(edge.Start.ContainerSubGraph, edge.End.ContainerSubGraph)
			|| isSubgraphSubgraphOfOtherSubgraphOrEqual(edge.End.ContainerSubGraph, edge.Start.ContainerSubGraph))
		{
			if (arrowHead is not null)
				arrowHead.IsVisible = false;
			line.IsVisible = false;
		}
		else
		{
			if (edge.Start.ContainerSubGraph == subGraph)
			{
				moveEdgesEndByMovingSiblingNode(edge.Start, subGraph);
			}
			else
			{
				moveEdgesEndByMovingSiblingNode(edge.End, subGraph);
			}
		}
	}

	private void showEdge(Tuple<GraphEdge, SubGraph?> args, Line line, Polygon? arrowHead)
	{
		var edge = args.Item1;
		var subGraph = args.Item2;

		if (isSubgraphSubgraphOfOtherSubgraphOrEqual(edge.Start.ContainerSubGraph, edge.End.ContainerSubGraph)
			|| isSubgraphSubgraphOfOtherSubgraphOrEqual(edge.End.ContainerSubGraph, edge.Start.ContainerSubGraph))
		{
			if (arrowHead is not null)
				arrowHead.IsVisible = true;
			line.IsVisible = true;
		}
		else
		{
			if (edge.Start.ContainerSubGraph == subGraph)
			{
				moveEdgesEndByMovingSiblingNode(edge.Start, subGraph);
			}
			else
			{

				moveEdgesEndByMovingSiblingNode(edge.End, subGraph);
			}
		}
	}

	private bool isSubgraphSubgraphOfOtherSubgraphOrEqual(SubGraph? containerSubGraph, SubGraph? childSubGraph)
	{
		if (containerSubGraph is null || childSubGraph is not null)
			return false;

		if (containerSubGraph == childSubGraph)
			return true;


		foreach (var node in containerSubGraph.Graph.Nodes)
		{
			if (node is SubGraph subGraph)
			{
				return isSubgraphSubgraphOfOtherSubgraphOrEqual(subGraph, childSubGraph);
			}
		}
		return false;
	}

	private void addEdgeToDictOfAll(GraphEdge edge)
	{
		if (!allEdgesByNode.ContainsKey(edge.Start))
			allEdgesByNode[edge.Start] = new List<GraphEdge>();
		if (!allEdgesByNode.ContainsKey(edge.End))
			allEdgesByNode[edge.End] = new List<GraphEdge>();
		allEdgesByNode[edge.Start].Add(edge);
		allEdgesByNode[edge.End].Add(edge);
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
		arrowHead.Fill = edge.ArrowHeadColor;
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
				drawNode(node);
			}
		}
	}

	private void drawNode(GraphNode node)
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

	private static void bindEdgesStart(GraphEdge edge, Line line)
	{
		edge.Start.Model.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == "PositionInCanvas")
			{
				double centerX = edge.Start.Model.PositionInCanvas.X;
				double centerY = edge.Start.Model.PositionInCanvas.Y;
				var centerPoint = new Point(centerX, centerY);

				line.StartPoint = centerPoint;
			}
		};

		double centerX = edge.Start.Model.PositionInCanvas.X;
		double centerY = edge.Start.Model.PositionInCanvas.Y;
		var centerPoint = new Point(centerX, centerY);

		line.StartPoint = centerPoint;
	}

	private static void bindEdgesEnd(GraphEdge edge, Line line)
	{
		edge.End.Model.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == "PositionInCanvas")
			{
				double centerX = edge.End.Model.PositionInCanvas.X;
				double centerY = edge.End.Model.PositionInCanvas.Y;
				var centerPoint = new Point(centerX, centerY);

				line.EndPoint = centerPoint;
			}
		};

		double centerX = edge.End.Model.PositionInCanvas.X;
		double centerY = edge.End.Model.PositionInCanvas.Y;
		var centerPoint = new Point(centerX, centerY);

		line.EndPoint = centerPoint;
	}

	private void refresh()
	{
		var nodeToCheck = new Queue<GraphNode>();
		foreach (var node in FirstLevelGraph.Nodes)
		{
			nodeToCheck.Enqueue(node);
		}

		while (nodeToCheck.Count > 0)
		{
			var node = nodeToCheck.Dequeue();
			if (node is SubGraph subGraph)
			{
				foreach (var n in subGraph.Graph.Nodes)
					nodeToCheck.Enqueue(n);
			}
			node.OnNodePointerPressedHandler = null;
			node.OnRealPositionChangedHandler = null;
			node.PointerPressed -= node.OnNodePointerPressedHandler;
		}

		graphCanvas.Children.Clear();
		graphCanvas.PointerPressed -= (sender, e) => { };
		graphCanvas.PointerMoved -= (sender, e) => { };
		graphCanvas.PointerWheelChanged -= (sender, e) => { };
		changes.Clear();

		InitializeGraphView(FirstLevelGraph);
	}
}

internal class EventArgsWithScale
{
	public double scale { get; set; }
	public Point PointerPosition { get; set; }
}