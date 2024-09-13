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
	double gravityForce = 5;
	double optimalNodesDistance = 0;
	double maxDistanceClamped = 3;
	public int Iterations = 200;
	public int Width { get; set; } = 600;
	public int Height { get; set; } = 700;
	public Point StartPosition { get; set; } = new Point(0, 0);
	public bool WithAnimation = false;
	
	public AlgorithmType Algorithm { get; set; } = AlgorithmType.AngleRepulsion;
	public virtual void ApplyLayout(Graph graph)
	{
		if (graph.Nodes.Count == 0)
			return;

		var allGraphComponents = findAllGraphsComponents(graph);

		var multiGraph = new Graph();

		// each graph component will be represented by a single node, that holds the position 
		// of the component in the layout.
		var graphComponentsToNodes = new Dictionary<Graph, GraphNode>();
		int xGraphBias, yGraphBias;
		applyGridLayoutToGraphComponents(allGraphComponents, multiGraph, graphComponentsToNodes, out xGraphBias, out yGraphBias);

		foreach (var graphComponent in allGraphComponents)
		{
			var startPoint = graphComponentsToNodes[graphComponent].RealPosition + StartPosition;
			var positions = springLayoutFindingAlgorithm(startPoint, graphComponent, xGraphBias, yGraphBias);
		}
	}

	private void applyGridLayoutToGraphComponents(
		List<Graph> allGraphComponents,
		Graph multiGraph,
		Dictionary<Graph, GraphNode> graphComponentsToNodes,
		out int xGraphBias,
		out int yGraphBias)
	{
		foreach (var graphComponent in allGraphComponents)
		{
			var node = new GraphNode();
			multiGraph.Nodes.Add(node);
			graphComponentsToNodes[graphComponent] = node;
		}


		int numberOfColumns = 1;
		int numberOfRows = 1;

		if (multiGraph.Nodes.Count > 1)
		{
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

		optimalNodesDistance = Math.Sqrt(width * height) / (graph.Nodes.Count);

		var nodes = graph.Nodes;
		var edges = graph.Edges;
		var nodeCount = nodes.Count;
		var edgeCount = edges.Count;
		var random = new Random();
		var nodePositions = new Dictionary<GraphNode, (double x, double y)>();
		foreach (var node in nodes)
		{
			// random position, but close to the center
			var x = random.NextDouble() > 0.5 ? width / 2 + random.NextDouble() * width / 3 : width / 2 - random.NextDouble() * width / 3;
			var y = random.NextDouble() > 0.5 ? height / 2 + random.NextDouble() * height / 3 : height / 2 - random.NextDouble() * height / 3;
			nodePositions[node] = (x, y);

		}

		double temperature = width / 10;

		if (WithAnimation)
			Task.Run(() =>
			{
				for (var i = 0; i < Iterations; i++)
				{
					Thread.Sleep(2);
					Dispatcher.UIThread.InvokeAsync(() =>
					{
						switch (Algorithm)
						{
							case AlgorithmType.AngleRepulsion:
								iterationUsingAngleRepulsion(graph, nodes, edges, nodePositions, width, height);
								break;
							case AlgorithmType.FruchtermanReingold:
								iterationUsingFDP(graph, nodes, edges, nodePositions, width, height, temperature);
								break;
						}
						setRealPositions(nodes, nodePositions, startPosition);
					}).Wait();
					temperature *= 0.95;
				}
			});
		else
		{
			for (var i = 0; i < Iterations; i++)
			{
				switch (Algorithm)
				{
					case AlgorithmType.AngleRepulsion:
						iterationUsingAngleRepulsion(graph, nodes, edges, nodePositions, width, height);
						break;
					case AlgorithmType.FruchtermanReingold:
						iterationUsingFDP(graph, nodes, edges, nodePositions, width, height, temperature);
						break;
				}
				temperature *= 0.95;
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


	private void iterationUsingAngleRepulsion(
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

		countRepulsiveForces(nodePositions, delta, 0.0001);

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

			if (distance < 0.00001)
				distance = 0.00001;

			var distanceClamped = Math.Max(1, distance);
			var fx = Math.Min(maxDistanceClamped, distanceClamped) * (dx / distance);
			var fy = Math.Min(maxDistanceClamped, distanceClamped) * (dy / distance);
			if (!node.IsInvariantPositionToGraphLayout)
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

	// Fruchterman and Reingold algorithm
	private void iterationUsingFDP(
		Graph graph,
		ObservableCollection<GraphNode> nodes,
		ObservableCollection<GraphEdge> edges,
		Dictionary<GraphNode, (double x, double y)> nodePositions,
		int width, int height, double temperature)
	{
		double area = width * height;
		double k = Math.Sqrt(area / nodes.Count);

		// initialize displacement array
		var disp = new Dictionary<GraphNode, (double dx, double dy)>();
		foreach (var node in nodes)
		{
			disp[node] = (0, 0);
		}

		// repulsive forces
		foreach (var node in nodes)
		{
			foreach (var otherNode in nodes)
			{
				if (node == otherNode)
					continue;

				var distanceX = nodePositions[node].x - nodePositions[otherNode].x;
				var distanceY = nodePositions[node].y - nodePositions[otherNode].y;
				if(distanceX < 0.00001)
					distanceX = 0.00001;
				if(distanceY < 0.00001)
					distanceY = 0.00001;
				var displX = disp[node].dx + (distanceX / Math.Abs(distanceX) * k * k / distanceX);
				var displY = disp[node].dy + (distanceY / Math.Abs(distanceY) * k * k / distanceY);
				disp[node] = (displX, displY);
			}
		}

		// attractive forces
		foreach (var edge in edges)
		{
			var source = edge.Start;
			var target = edge.End;
			var distanceX = nodePositions[target].x - nodePositions[source].x;
			var distanceY = nodePositions[target].y - nodePositions[source].y;
			if (distanceX < 0.00001)
				distanceX = 0.00001;
			if (distanceY < 0.00001)
				distanceY = 0.00001;

			var displX = disp[source].dx - (distanceX / Math.Abs(distanceX) * Math.Abs(distanceX) * Math.Abs(distanceX) / k);
			var displY = disp[source].dy - (distanceY / Math.Abs(distanceY) * Math.Abs(distanceY) * Math.Abs(distanceY) / k);
			disp[source] = (displX, displY);

			displX = disp[target].dx + (distanceX / Math.Abs(distanceX) * Math.Abs(distanceX) * Math.Abs(distanceX) / k);
			displY = disp[target].dy + (distanceY / Math.Abs(distanceY) * Math.Abs(distanceY) * Math.Abs(distanceY) / k);
			disp[target] = (displX, displY);
		}

		foreach (var node in nodes)
		{
			// limit max displacement to fram; use temp. t to scale
			var nodePositionX = nodePositions[node].x;
			var nodePositionY = nodePositions[node].y;
			if(disp[node].dx != 0)
			{
				nodePositionX += (disp[node].dx / Math.Abs(disp[node].dx)) * Math.Min(Math.Abs(disp[node].dx), temperature);
			}
			if(disp[node].dy != 0)
			{
				nodePositionY += (disp[node].dy / Math.Abs(disp[node].dy)) * Math.Min(Math.Abs(disp[node].dy), temperature);
			}
			nodePositions[node] = (nodePositionX, nodePositionY);

			nodePositions[node] = (Math.Min(width / 2, Math.Max(-width / 2, nodePositions[node].x)),
				Math.Min(height / 2, Math.Max(-height / 2, nodePositions[node].y)));


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
		Dictionary<GraphNode, (double dx, double dy)> displ,
		GraphEdge edge)
	{
		var source = edge.Start;
		var target = edge.End;
		var diffVectorX = nodePositions[target].x - nodePositions[source].x;
		var diffVectorY = nodePositions[target].y - nodePositions[source].y;

		var distance = Math.Sqrt(diffVectorX * diffVectorX + diffVectorY * diffVectorY);
		if (distance < 0.00001)
			distance = 0.00001;

		var attractForce = gravityForce * (distance - optimalNodesDistance);

		var fx = (diffVectorX / distance) * attractForce;
		var fy = (diffVectorY / distance) * attractForce;
		displ[source] = (displ[source].dx + fx, displ[source].dy + fy);
		displ[target] = (displ[target].dx - fx, displ[target].dy - fy);
		if (double.IsNaN(displ[source].dx) || double.IsNaN(displ[source].dy))
			throw new Exception("NaN");
	}

	private static void countForceCausedByLowAngle(
		Dictionary<GraphNode, (double x, double y)> nodePositions,
		Dictionary<GraphNode, (double dx, double dy)> displ,
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

		if (sizeVector1 < 0.00001)
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
			displ[node] = (displ[node].dx + fx1 + fx2, displ[node].dy + fy1 + fy2);

			var vector3 = new Point(nodePositions[source1].x - nodePositions[source2].x, nodePositions[source1].y - nodePositions[source2].y);
			var sizeVector3 = Math.Sqrt(vector3.X * vector3.X + vector3.Y * vector3.Y);
			if (sizeVector3 < 0.00001)
				sizeVector3 = 0.00001;
			var fx3 = (vector3.X / sizeVector3) * force;
			var fy3 = (vector3.Y / sizeVector3) * force;
			displ[source1] = (displ[source1].dx - fx1 + fx3, displ[source1].dy - fy1 + fy3);
			displ[source2] = (displ[source2].dx - fx2 - fx3, displ[source2].dy - fy2 - fy3);
			if (double.IsNaN(displ[source1].dx) || double.IsNaN(displ[source1].dy))
				throw new Exception("NaN");

		}
	}

	private void countRepulsiveForces(
		Dictionary<GraphNode, (double x, double y)> nodePositions,
		Dictionary<GraphNode, (double dx, double dy)> displ,
		double repulsiveConstant)
	{
		Random random = new Random();
		if (random.NextDouble() > 0.33)
			return;

		foreach (var node1 in nodePositions.Keys)
		{
			displ[node1] = (0, 0);
			foreach (var node2 in nodePositions.Keys)
			{
				if (node1 == node2) continue;

				var diffVectorX = nodePositions[node2].x - nodePositions[node1].x;
				var diffVectorY = nodePositions[node2].y - nodePositions[node1].y;
				var distance = Math.Sqrt(diffVectorX * diffVectorX + diffVectorY * diffVectorY);

				if (distance < 0.00001) distance = 0.00001;

				var force = repulsiveConstant / (distance * distance * 3);
				var fx = (diffVectorX / distance) * force;
				var fy = (diffVectorY / distance) * force;

				displ[node1] = (displ[node1].dx - fx, displ[node1].dy - fy);
			}
		}
	}

	public virtual void AddSubGraph(SubGraph subGraph, Graph graph)
	{
		throw new NotImplementedException();
	}


	public enum AlgorithmType
	{
		AngleRepulsion,
		FruchtermanReingold
	}

}