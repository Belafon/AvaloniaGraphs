using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaGraphs.GraphControl;
using Avalonia;

namespace AvaloniaGraphs.GraphsLayout;
public class GridGraphLayout : GraphLayout
{
	public int Width { get; set; } = 800;
	public int Height { get; set; } = 900;
    public Point StartPosition { get; set; } = new Point(30, 30);

    public void AddSubGraph(SubGraph subGraph, Graph graph)
    {
        throw new NotImplementedException();
    }

    public void ApplyLayout(Graph graph)
	{
		if (graph.Nodes.Count == 0)
			return;

		var allGraphComponents = findAllGraphsComponents(graph);

		var multiGraph = new Graph();
		var subgraphsToNodes = new Dictionary<Graph, GraphNode>();
		foreach (var subgraph in allGraphComponents)
		{
			var node = new GraphNode();
			multiGraph.Nodes.Add(node);
			subgraphsToNodes[subgraph] = node;
		}

		int xSize = (int)Math.Sqrt(graph.Nodes.Count) + 1;
		int ySize = xSize;

		var nodeList = new List<GraphNode>();

		foreach (var subgraph in allGraphComponents)
		{
			nodeList.AddRange(subgraph.Nodes);
		}

		int xGraphBias = Width / xSize;
		int yGraphBias = Height / ySize; 

		for (int x = 0; x < xSize; x++)
		{
			for (int y = 0; y < ySize; y++)
			{
				if (x * ySize + y >= nodeList.Count)
					break;

				nodeList[x * ySize + y].SetRealPosition(new Point(x * xGraphBias, y * yGraphBias) + StartPosition);
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
				if (visited.Contains(currentNode))
					continue;
				visited.Add(currentNode);
				currentGraph.Nodes.Add(currentNode);

				// if current node has no edges, it is a single graph component
				if (graph.EdgesByNode.ContainsKey(currentNode) == false)
					continue;

				foreach (var edge in graph.EdgesByNode[currentNode])
				{
					var nextNode = edge.Start == currentNode ? edge.End : edge.Start;
					if (visited.Contains(nextNode) == false)
					{
						queue.Enqueue(nextNode);

						currentGraph.Edges.Add(edge); // only the sceleton of the graph will be added
					}
					// currentGraph.Edges.Add(edge); // all edges will be added

				}
			}
			graphs.Add(currentGraph);
		}
		return graphs;
	}
}