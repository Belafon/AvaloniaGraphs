using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaGraphs.GraphControl;

namespace AvaloniaGraphs.GraphsLayout;
public class SpringLayout : GraphLayout
{
	double forceParam = 0.1;
	double c = 0.1;
	double t = 0.1;
	public int Iterations = 100;
	public int Width = 800;
	public int Height = 600;
	public void ApplyLayout(Graph graph)
	{
		var nodes = graph.Nodes;
		var edges = graph.Edges;
		var nodeCount = nodes.Count;
		var edgeCount = edges.Count;
		var random = new Random();
		var nodePositions = new Dictionary<GraphNode, (double x, double y)>();
		foreach (var node in nodes)
		{
			nodePositions[node] = (random.NextDouble() * Width, random.NextDouble() * Height);
		}
		for (var i = 0; i < Iterations; i++)
		{
			var delta = new Dictionary<GraphNode, (double dx, double dy)>();
			foreach (var node in nodes)
			{
				delta[node] = (0, 0);
			}
			foreach (var edge in edges)
			{
				var source = edge.Start;
				var target = edge.End;
				var dx = nodePositions[target].x - nodePositions[source].x;
				var dy = nodePositions[target].y - nodePositions[source].y;
				var distance = Math.Sqrt(dx * dx + dy * dy);
				var force = forceParam * (distance - c);
				var fx = (dx / distance) * force;
				var fy = (dy / distance) * force;
				delta[source] = (delta[source].dx + fx, delta[source].dy + fy);
				delta[target] = (delta[target].dx - fx, delta[target].dy - fy);
			}
			foreach (var node in nodes)
			{
				var dx = delta[node].dx;
				var dy = delta[node].dy;
				var distance = Math.Sqrt(dx * dx + dy * dy);
				var distanceClamped = Math.Max(1, distance);
				var fx = Math.Min(t, distanceClamped) * (dx / distance);
				var fy = Math.Min(t, distanceClamped) * (dy / distance);
				nodePositions[node] = (nodePositions[node].x + fx, nodePositions[node].y + fy);
			}
		}
		 
		foreach (var node in nodes)
		{
			node.SetRealPosition(new Avalonia.Point(nodePositions[node].x, nodePositions[node].y));
		}
	}
}