using System.Threading;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaGraphs.GraphControl;
using Avalonia;
using Avalonia.Threading;
using System.Collections.ObjectModel;

namespace AvaloniaGraphs.GraphsLayout;
public class SpringGraphLayout : GraphLayout
{
	double forceAttract = 0.1;
	double forceSpreadOut = 200;
	double t = 3;
	public int Iterations = 200;
	public int Width { get; set; } = 600;
	public int Height {get; set; } = 700;
	public Point StartPosition { get; set; } = new Point(0, 0);
	public bool withAnimation = false;
	public virtual void ApplyLayout(Graph graph)
	{
		if (graph.Nodes.Count == 0)
			return;

		var allGraphComponents = findAllGraphsComponents(graph);

		var multiGraph = new Graph();
		var subgraphsToNodes = new Dictionary<Graph, GraphNode>();
		int xGraphBias, yGraphBias;
		applyGridLayoutToGraphComponents(allGraphComponents, multiGraph, subgraphsToNodes, out xGraphBias, out yGraphBias);

		foreach (var subgraph in allGraphComponents)
		{
			var startPoint = subgraphsToNodes[subgraph].RealPosition + StartPosition;
			var positions = springLayoutFindingAlgorithm(startPoint, subgraph, xGraphBias, yGraphBias);
		}
	}

	private void applyGridLayoutToGraphComponents(
		List<Graph> allGraphComponents, 
		Graph multiGraph, 
		Dictionary<Graph, GraphNode> subgraphsToNodes, 
		out int xGraphBias, 
		out int yGraphBias)
	{
		foreach (var subgraph in allGraphComponents)
		{
			var node = new GraphNode();
			multiGraph.Nodes.Add(node);
			subgraphsToNodes[subgraph] = node;
		}


		int numberOfColumns = 1; 
		int numberOfRows = 1;
		
		if(multiGraph.Nodes.Count > 1){
			numberOfColumns = (int)Math.Sqrt(multiGraph.Nodes.Count) + 1;
			numberOfRows = numberOfColumns - 1;
			if (numberOfColumns * numberOfRows < multiGraph.Nodes.Count)
				numberOfRows++;
		}

		

		var nodeList = multiGraph.Nodes.ToList();

		xGraphBias = Width / numberOfColumns;
		yGraphBias = Height / numberOfRows;
		for (int x = 0; x < numberOfColumns; x++)
		{
			for (int y = 0; y < numberOfRows; y++)
			{
				if (x * numberOfRows + y >= nodeList.Count)
					break;

				nodeList[x * numberOfRows + y].SetRealPosition(new Point(x * xGraphBias, y * yGraphBias));
			}
		}
	}

	private List<Graph> findAllGraphsComponents(Graph graph)
	{
		var visited = new HashSet<GraphNode>();
		var graphs = new List<Graph>();
		foreach (var node in graph.Nodes)
		{
			if (visited.Contains(node))
				continue;
			var currentGraph = new Graph();
			var queue = new Queue<GraphNode>();
			queue.Enqueue(node);
			while (queue.Count > 0)
			{
				var currentNode = queue.Dequeue();
				if (!visited.Contains(currentNode))
				{
					visitNextNode(graph, visited, currentGraph, queue, currentNode);
				}
			}
			graphs.Add(currentGraph);
		}
		return graphs;
	}

	private static void visitNextNode(Graph graph, HashSet<GraphNode> visited, Graph currentGraph, Queue<GraphNode> queue, GraphNode currentNode)
	{
		visited.Add(currentNode);
		currentGraph.Nodes.Add(currentNode);

		// if current node has no edges, it is a single graph component
		if (graph.EdgesByNode.ContainsKey(currentNode) == false)
			return;

		foreach (var edge in graph.EdgesByNode[currentNode])
		{
			var nextNode = edge.Start == currentNode ? edge.End : edge.Start;
			if (visited.Contains(nextNode) == false)
			{
				queue.Enqueue(nextNode);

				//currentGraph.Edges.Add(edge); // only the sceleton of the graph will be added
			}

			if (visited.Contains(nextNode) == false)
				currentGraph.Edges.Add(edge);

			// add edge to currentGraph.Edges, if the nextNode doesnt have an edge to the currentNode
			if (graph.EdgesByNode.ContainsKey(nextNode) && visited.Contains(nextNode))
			{
				var hasEdge = false;
				foreach (var nextEdge in graph.EdgesByNode[nextNode])
				{
					if (nextEdge.Start == currentNode || nextEdge.End == currentNode)
					{
						hasEdge = true;
						break;
					}
				}
				if (hasEdge == false)
				{
					currentGraph.Edges.Add(edge);
				}
			}
		}
	}

	private Dictionary<GraphNode, (double x, double y)> springLayoutFindingAlgorithm(
		Point startPosition, Graph graph, int width, int height)
	{
		if (graph.Nodes.Count == 0)
			return new Dictionary<GraphNode, (double x, double y)>();

		if (graph.Nodes.Count == 1)
		{
			var node = graph.Nodes.First();
			node.SetRealPosition(new Point(startPosition.X + width / 2, startPosition.Y + height / 2)); // + width / 2 - node.Width / 2, startPosition.Y + height / 2 - node.Height / 2));
			
			return new Dictionary<GraphNode, (double x, double y)>() { { node, (startPosition.X, startPosition.Y) } };
		}

		forceSpreadOut = width / Math.Sqrt(graph.Nodes.Count);

		var nodes = graph.Nodes;
		var edges = graph.Edges;
		var nodeCount = nodes.Count;
		var edgeCount = edges.Count;
		var random = new Random();
		var nodePositions = new Dictionary<GraphNode, (double x, double y)>();
		foreach (var node in nodes)
		{
			nodePositions[node] = (random.NextDouble() * width, random.NextDouble() * height);
		}

		if (withAnimation)
			Task.Run(() =>
			{
				for (var i = 0; i < Iterations; i++)
				{
					Thread.Sleep(2);
					Dispatcher.UIThread.InvokeAsync(() =>
					{
						iteration(graph, nodes, edges, nodePositions, width, height);

						setRealPositions(nodes, nodePositions, startPosition);
					}).Wait();
				}
			});
		else
		{
			for (var i = 0; i < Iterations; i++)
			{
				iteration(graph, nodes, edges, nodePositions, width, height);

			}
			
			setRealPositions(nodes, nodePositions, startPosition);
		}

		return nodePositions;
	}
	
	private void setRealPositions(ObservableCollection<GraphNode> nodes, Dictionary<GraphNode, (double x, double y)> nodePositions, Point startPosition)
	{
		foreach (var node in nodes)
		{
			node.SetRealPosition(new Point(nodePositions[node].x, nodePositions[node].y) + startPosition);
		}
	}
	

	private void iteration(
		Graph graph,
		ObservableCollection<GraphNode> nodes,
		ObservableCollection<GraphEdge> edges,
		Dictionary<GraphNode, (double x, double y)> nodePositions,
		int width, int height)
	{
		var delta = new Dictionary<GraphNode, (double dx, double dy)>();
		foreach (var node in nodes)
		{
			delta[node] = (0, 0);
		}

		foreach (var edge in edges)
		{
			countForceCausedByEdgeLength(nodePositions, delta, edge);
		}

		foreach (var node in nodes)
		{
			// according the angle between two edges, move the node towards the two sibling nodes,
			// so the angle between the edges will be bigger, do it only if the angle is smaller than 30 degrees
			var edgesByNode = graph.EdgesByNode[node];
			foreach (var edge1 in edgesByNode)
			{
				foreach (var edge2 in edgesByNode)
				{
					countForceCausedByLowAngle(nodePositions, delta, node, edge1, edge2);
				}
			}
		}

		foreach (var node in nodes)
		{
			var dx = delta[node].dx;
			var dy = delta[node].dy;
			var distance = Math.Sqrt(dx * dx + dy * dy);
			
			if(distance < 0.00001)
				distance = 0.00001;
			
			var distanceClamped = Math.Max(1, distance);
			var fx = Math.Min(t, distanceClamped) * (dx / distance);
			var fy = Math.Min(t, distanceClamped) * (dy / distance);
			if(!node.IsInvariantPositionToGraphLayout)
				nodePositions[node] = (nodePositions[node].x + fx, nodePositions[node].y + fy);

			if (nodePositions[node].x > width)
				nodePositions[node] = (width, nodePositions[node].y);
			if (nodePositions[node].y > height)
				nodePositions[node] = (nodePositions[node].x, height);
			if (nodePositions[node].x < 0)
				nodePositions[node] = (0, nodePositions[node].y);
			if (nodePositions[node].y < 0)
				nodePositions[node] = (nodePositions[node].x, 0);
		}
	}

	private void countForceCausedByEdgeLength(
		Dictionary<GraphNode, (double x, double y)> nodePositions,
		Dictionary<GraphNode, (double dx, double dy)> delta,
		GraphEdge edge)
	{
		var source = edge.Start;
		var target = edge.End;
		var dx = nodePositions[target].x - nodePositions[source].x;
		var dy = nodePositions[target].y - nodePositions[source].y;
		var distance = Math.Sqrt(dx * dx + dy * dy);
		if(distance < 0.00001)
			distance = 0.00001;

		var force = forceAttract * (distance - forceSpreadOut);
		var fx = (dx / distance) * force;
		var fy = (dy / distance) * force;
		delta[source] = (delta[source].dx + fx, delta[source].dy + fy);
		delta[target] = (delta[target].dx - fx, delta[target].dy - fy);
		if(double.IsNaN(delta[source].dx) || double.IsNaN(delta[source].dy))
			throw new Exception("NaN");
	}

	private static void countForceCausedByLowAngle(
		Dictionary<GraphNode, (double x, double y)> nodePositions,
		Dictionary<GraphNode, (double dx, double dy)> delta,
		GraphNode node,
		GraphEdge edge1, GraphEdge edge2)
	{
		if (edge1 == edge2)
			return;

		var source1 = edge1.Start == node ? edge1.End : edge1.Start;
		var source2 = edge2.Start == node ? edge2.End : edge2.Start;

		if (source1 == source2)
			return;


		var dx1 = nodePositions[node].x - nodePositions[source1].x;
		var dy1 = nodePositions[node].y - nodePositions[source1].y;
		var sizeVector1 = Math.Sqrt(dx1 * dx1 + dy1 * dy1);
		
		if(sizeVector1 < 0.00001)
			sizeVector1 = 0.00001;

		var dx2 = nodePositions[node].x - nodePositions[source2].x;
		var dy2 = nodePositions[node].y - nodePositions[source2].y;
		var sizeVector2 = Math.Sqrt(dx2 * dx2 + dy2 * dy2);

		if (sizeVector2 < 0.00001)
			sizeVector2 = 0.00001;

		var dotProduct = dx1 * dx2 + dy1 * dy2;

		var angle = Math.Acos(dotProduct / (sizeVector1 * sizeVector2));
		if (angle < Math.PI / 4d)
		{
			if (angle < 0.000001)
				angle = 0.000001;

			// lower angle means bigger force, make the force strong 
			var force = 2 / angle;

			var fx1 = (dx1 / sizeVector1) * force;
			var fy1 = (dy1 / sizeVector1) * force;
			var fx2 = (dx2 / sizeVector2) * force;
			var fy2 = (dy2 / sizeVector2) * force;
			delta[node] = (delta[node].dx + fx1 + fx2, delta[node].dy + fy1 + fy2);

			var vector3 = new Point(nodePositions[source1].x - nodePositions[source2].x, nodePositions[source1].y - nodePositions[source2].y);
			var sizeVector3 = Math.Sqrt(vector3.X * vector3.X + vector3.Y * vector3.Y);
			if (sizeVector3 < 0.00001)
				sizeVector3 = 0.00001;
			var fx3 = (vector3.X / sizeVector3) * force;
			var fy3 = (vector3.Y / sizeVector3) * force;
			delta[source1] = (delta[source1].dx - fx1 + fx3, delta[source1].dy - fy1 + fy3);
			delta[source2] = (delta[source2].dx - fx2 - fx3, delta[source2].dy - fy2 - fy3);
			if(double.IsNaN(delta[source1].dx) || double.IsNaN(delta[source1].dy))
				throw new Exception("NaN");

		}
	}

	public virtual void AddSubGraph(SubGraph subGraph, Graph graph)
	{
		throw new NotImplementedException();
	}
}