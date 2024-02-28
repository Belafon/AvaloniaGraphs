using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using AvaloniaGraphs.GraphControl;

namespace AvaloniaGraphs.GraphsLayout;

public class SpringGraphLayoutWithSubGraphs : SpringGraphLayout
{
	public override void ApplyLayout(Graph graph)
	{
		var widthOfSubgraphsContent = this.Width / Math.Sqrt(graph.Nodes.Count + 1);
		var heightOfSubgraphsContent = this.Height / Math.Sqrt(graph.Nodes.Count + 1);

		base.ApplyLayout(graph);

		foreach (var node in graph.Nodes)
		{

			if (node is SubGraph subGraph)
			{
				handleSubGraph(widthOfSubgraphsContent, heightOfSubgraphsContent, subGraph);
			}
		}
	}

	private void handleSubGraph(double widthOfContainer, double heightOfContainer, SubGraph subGraph)
	{
		var widthOfContent = widthOfContainer - StartPosition.X * 2;
		var heightOfContent = heightOfContainer - StartPosition.Y * 2;

		var subGraphsLayout = subGraph.Graph.Layout;
		if (subGraphsLayout is not null)
		{
			if(subGraphsLayout.StartPosition == default)
				subGraphsLayout.StartPosition = StartPosition;
				
			subGraphsLayout.Width = (int)widthOfContent;
			subGraphsLayout.Height = (int)heightOfContent;
			subGraphsLayout.ApplyLayout(subGraph.Graph);
		}

		double widthSum = 0;
		double heightSum = 0;
		double maxWidth = 0;
		double maxHeight = 0;
		foreach (var subGraphNode in subGraph.Graph.Nodes)
		{
			widthSum += subGraphNode.Width;
			heightSum += subGraphNode.Height;
			if (subGraphNode.Width > maxWidth)
				maxWidth = subGraphNode.Width;
			if (subGraphNode.Height > maxHeight)
				maxHeight = subGraphNode.Height;
		}
		double diameterOfSubGraphNodesWidth = widthSum / subGraph.Graph.Nodes.Count;
		double diameterOfSubGraphNodesHeight = heightSum / subGraph.Graph.Nodes.Count;


		subGraph.MinWidthOfBorderContainer = Math.Sqrt(subGraph.Graph.Nodes.Count) * maxWidth + StartPosition.X * 2;
		subGraph.MinHeightOfBorderContainer = Math.Sqrt(subGraph.Graph.Nodes.Count) * maxHeight + StartPosition.Y * 2;
		subGraph.StartBorderNode.Width = widthOfContainer;
		subGraph.StartBorderNode.Height = heightOfContainer;

		subGraph.StartBorderNode.PositionInCanvas = new Point(0, 0);
		subGraph.StartBorderNode.RealPosition = new Point(0, 0);

		//var nodeSizeMove = new Point(subGraph.Width / 2, subGraph.Height / 2);
		//var containerSizeMove = -new Point(subGraph.StartBorderNode.Width / 2, subGraph.StartBorderNode.Height / 2);
		var startPosition = subGraph.RealPosition; // + nodeSizeMove + containerSizeMove;


		var moveToLeftTopCorner = -new Point(subGraph.StartBorderNode.Width / 2, subGraph.StartBorderNode.Height / 2);
		moveSubGraphsNodes(subGraph, startPosition + moveToLeftTopCorner);
		subGraph.StartBorderNode.PositionInCanvas += startPosition;
		subGraph.StartBorderNode.RealPosition += startPosition;
		
		//moveSubGraphsNodes(subGraph, startPosition + moveToLeftTopCorner);
	}

	private void getSizesForInnerContent(
		double widthOfSubgraphsContent, double heightOfSubgraphsContent,
		out double widthContent, out double heightContent)
	{
		if (widthOfSubgraphsContent > 50 + StartPosition.X)
			widthContent = widthOfSubgraphsContent - 50 - StartPosition.X;
		else
			widthContent = widthOfSubgraphsContent * 0.6;

		if (heightOfSubgraphsContent > 50 + StartPosition.Y)
			heightContent = heightOfSubgraphsContent - 50 - StartPosition.Y;
		else
			heightContent = heightOfSubgraphsContent * 0.6;
	}

	private void moveSubGraphsNodes(SubGraph subGraph, Point diff)
	{
		foreach (var node in subGraph.Graph.Nodes)
		{
			if (node is SubGraph subsubGraph)
			{
				node.SetRealPosition(node.RealPosition + diff);
			}
			else 
			{
				node.SetRealPosition(node.RealPosition + diff);
			}
		}

	}

	public override void AddSubGraph(SubGraph subGraph, Graph graph)
	{
		throw new NotImplementedException();
		var widthOfSubgraphsContent = this.Width / Math.Sqrt(graph.Nodes.Count);
		var heightOfSubgraphsContent = this.Height / Math.Sqrt(graph.Nodes.Count);

		handleSubGraph(widthOfSubgraphsContent, heightOfSubgraphsContent, subGraph);
		moveSubGraphsNodes(subGraph, new Point(widthOfSubgraphsContent / 2, heightOfSubgraphsContent / 2));
	}
}