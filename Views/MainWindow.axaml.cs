using System.Drawing;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaGraphs.GraphControl;
using AvaloniaGraphs.GraphsLayout;
using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AvaloniaGraphs.Views;

public partial class MainWindow : Window
{
	private Graph graph;
	public MainWindow()
	{
		InitializeComponent();

		var mainPanel = this.FindControl<StackPanel>("MainPanel")!;

		// Uncomment one line to see an example
		// simpleGraphExample(mainPanel);
		// hugeRandomGraphExample(mainPanel, 5, 10);
		// hugeRandomGraphExample(mainPanel, 100, 200);
		// hugeRandomGraphExample(mainPanel, 100, 1000);
		simpleSubGraphExample(mainPanel);
		// nodesInSubgraphExample(mainPanel, 50, 100);
		// simpleSubGraphDeepNestingExample(mainPanel);
		// oldExample(mainPanel);
	}

	private void oldExample(StackPanel mainPanel)
	{
		var subsubnode = new GraphNode()
		{
			ContentControl = new TextBlock()
			{
				Text = "SubSubNode",
				Foreground = new SolidColorBrush(Colors.White)
			},

			Width = 200,
			Height = 50
		};
		var subsubnode2 = new GraphNode()
		{
			ContentControl = new TextBlock()
			{
				Text = "SubSubNode2",
				Foreground = new SolidColorBrush(Colors.White)
			},
			Width = 200,
			Height = 50
		};

		var node0 = new GraphNode();
		node0.Width = 100;
		node0.Height = 100;
		node0.ContentControl = new StackPanel()
		{
			Children = {
				new TextBlock()
				{
					Text = "Node 0",
					Foreground = new SolidColorBrush(Colors.White)
				},
				new TextBlock()
				{
					Text = "Node 0",
					Foreground = new SolidColorBrush(Colors.White)
				},
				new TextBlock()
				{
					Text = "Node 0",
					Foreground = new SolidColorBrush(Colors.White)
				},
				new TextBox()
				{
					Text = "Node 0",
					Foreground = new SolidColorBrush(Colors.White)
				},
			},
			Background = new SolidColorBrush(Colors.Green)
		};

		var node1 = new GraphNode()
		{
			ContentControl = new TextBlock()
			{
				Text = "Node 1",
				Foreground = new SolidColorBrush(Colors.White)
			}
		};
		var node3 = new GraphNode();

		var node2 = new GraphNode()
		{
			ContentControl = new TextBlock()
			{
				Text = "Node 2",
				Foreground = new SolidColorBrush(Colors.White)
			}
		};

		var subNode1 = new GraphNode()
		{
			ContentControl = new TextBlock()
			{
				Text = "SubNode 1",
				Foreground = new SolidColorBrush(Colors.White)
			}
		};

		var subNode2 = new GraphNode()
		{
			ContentControl = new TextBlock()
			{
				Text = "SubNode 2",
				Foreground = new SolidColorBrush(Colors.White)
			}
		};


		var subsubGraph = new SubGraph()
		{
			ContentControl = new TextBlock()
			{
				Text = "SubSubGraph",
				Foreground = new SolidColorBrush(Colors.White),
			},
			Graph = new Graph()
			{
				Nodes = {
					subsubnode,
					subsubnode2
				},
				Edges = {
				new GraphEdge(subsubnode, subsubnode2)
					{
						IsDirected = true
					},
					new GraphEdge(subsubnode2, subsubnode)
					{
						IsDirected = true
					},
					new GraphEdge(subsubnode, subNode1)
					{
						IsDirected = true
					},
					new GraphEdge(subsubnode2, node0)
					{
						IsDirected = true
					}
				},
				Layout = new SpringGraphLayout()
				{
					Iterations = 100,
					Width = 800,
					Height = 400
				}
			}
		};


		subsubGraph.BorderContainerTitle.Text = "SubSubGraph";


		var subGraph = new SubGraph()
		{
			ContentControl = new TextBlock()
			{
				Text = "SubGraph",
				Foreground = new SolidColorBrush(Colors.White),
			},
			Graph = new Graph()
			{
				Nodes = {
					subNode1,
					subNode2,
					subsubGraph
				},
				Edges = {
					new GraphEdge(subNode1, subNode2)
					{
						IsDirected = true
					},
					new GraphEdge(subNode2, node0)
					{
						IsDirected = true
					}
				},
				Layout = new SpringGraphLayoutWithSubGraphs()
				{
					Iterations = 100,
					Width = 800,
					Height = 400
				}
			}
		};


		var subgraph2 = new SubGraph()
		{
			ContentControl = new TextBlock() { Text = "SubGraph 2" },
			Graph = new Graph()
			{
				Nodes = {
							new GraphNode() { ContentControl = new TextBlock() { Text = "SubNode 3" } },
							new GraphNode() { ContentControl = new TextBlock() { Text = "SubNode 4" } }
						},
				Edges = {
							new GraphEdge(subNode1, subNode2) { IsDirected = true },
							new GraphEdge(subNode2, node0) { IsDirected = true }
						},
				Layout = new SpringGraphLayout()
				{
					Iterations = 100,
					Width = 800,
					Height = 400
				}
			}
		};

		// GRAPH EXAMPLE -----------------------------------------------------

		graph = new Graph()
		{
			Nodes = {
				node0,
				node1,
				subGraph,
			},
			Layout = new SpringGraphLayoutWithSubGraphs()
			{
				Iterations = 100,
				Width = 800,
				Height = 400,
			},
			ApplyLayoutOnEachAdd = false
		};

		initializeGraphView(mainPanel);

		new Task(async () =>
		{
			await Task.Delay(500);
			//await Dispatcher.UIThread.InvokeAsync(() => graph.Nodes.Add(subGraph));

			await Task.Delay(1000);
			await Dispatcher.UIThread.InvokeAsync(() =>
			{
				var newNode = new GraphNode() { ContentControl = new TextBlock() { Text = "Node 3" } };
				graph.Nodes.Add(newNode);
				graph.Edges.Add(new GraphEdge(node0, newNode) { IsDirected = true });
				graph.Edges.Add(new GraphEdge(newNode, node1) { IsDirected = true });
				graph.Nodes.Remove(node2);
			});
			await Task.Delay(1000);
			await Dispatcher.UIThread.InvokeAsync(() =>
			{
				//subNode2.SetRealPosition(subNode2.RealPosition + new Avalonia.Point(100, 100));

				//subGraph.Graph.Nodes.Add(subgraph2);
			});
		}).Start();
		autoMoveNode(subNode2);
	}

	private void initializeGraphView(StackPanel mainPanel)
	{
		var graphView = new GraphView(graph)
		{
			[!WidthProperty] = this[!WidthProperty],
			[!HeightProperty] = this[!HeightProperty]
		};
		graph.Layout?.ApplyLayout(graph);


		mainPanel.Children.Add(graphView);
	}

	private static void autoMoveNode(GraphNode subNode2)
	{
		new Task(async () =>
		{
			while (true)
			{
				await Task.Delay(1000);
				await Dispatcher.UIThread.InvokeAsync(() =>
					subNode2.SetRealPosition(new Avalonia.Point(0, 100) + subNode2.RealPosition));
				await Task.Delay(1000);
				await Dispatcher.UIThread.InvokeAsync(() =>
					subNode2.SetRealPosition(new Avalonia.Point(100, 0) + subNode2.RealPosition));
				await Task.Delay(1000);
				await Dispatcher.UIThread.InvokeAsync(() =>
					subNode2.SetRealPosition(new Avalonia.Point(0, -100) + subNode2.RealPosition));
				await Task.Delay(1000);
				await Dispatcher.UIThread.InvokeAsync(() =>
					subNode2.SetRealPosition(new Avalonia.Point(-100, 0) + subNode2.RealPosition));
			}
		}).Start();
	}

	private void simpleGraphExample(StackPanel mainPanel)
	{
		var node0 = new GraphNode() { ContentControl = new TextBlock() { Text = "Node 0" } };
		var node1 = new GraphNode() { ContentControl = new TextBlock() { Text = "Node 1" } };
		var node2 = new GraphNode() { ContentControl = new TextBlock() { Text = "Node 2" } };
		var node3 = new GraphNode() { ContentControl = new TextBlock() { Text = "Node 3" } };

		graph = new Graph()
		{
			Nodes = {
				node0,
				node1,
				node2,
				node3
			},
			Edges = {
				new GraphEdge(node0, node1) { IsDirected = true },
				new GraphEdge(node1, node2) { IsDirected = true },
				new GraphEdge(node2, node0) { IsDirected = true },
				new GraphEdge(node3, node1) { IsDirected = true }
			},
			Layout = new SpringGraphLayout()
			{
				Iterations = 100,
				Width = 800,
				Height = 400,
				WithAnimation = true
			},
		};
		
		initializeGraphView(mainPanel);
	}
	
	private GraphNode? selectedNode = null;
	private void hugeRandomGraphExample(StackPanel mainPanel, int nodesCount = 100, int edgesCount = 1000)
	{
		// Create nodes and edges
		var random = new Random();
		var nodes = new List<GraphNode>();
		var edges = new List<GraphEdge>();
		for (int i = 0; i < nodesCount; i++)
		{
			nodes.Add(new GraphNode() { ContentControl = new TextBlock() { Text = $"Node {i}" } });
		}
		for (int i = 0; i < edgesCount; i++)
		{
			edges.Add(new GraphEdge(nodes[random.Next(0, nodes.Count)], nodes[random.Next(0, nodes.Count)]) { 
				IsDirected = true,
				Thickness = 1,
				ArrowHeadLength = 5,
				ArrowHeadWidth = 5,
			});
		}
		
		
		// Create graph
		graph = new Graph()
		{
			Layout = new SpringGraphLayout()
			{
				Iterations = 150,
				Width = 1600,
				Height = 800,
				WithAnimation = true,
				//Algorithm = SpringGraphLayout.AlgorithmType.FruchtermanReingold,
			},
		};
		graph.AddAllNodes(nodes);
		graph.AddAllEdges(edges);
		
		
		// Initialize graph view
		initializeGraphView(mainPanel);
		
		
		
		// Add event to change color of edges
		foreach (var node in nodes)
		{
			node.PointerPressed += (sender, args) =>
			{
				foreach (var edge in graph.Edges)
				{
					if (edge.Start == selectedNode || edge.End == selectedNode)
					{
						edge.Color = new SolidColorBrush(Colors.Red);
						edge.Thickness = 1;
						edge.ArrowHeadColor = new SolidColorBrush(Colors.Red);
						edge.ArrowHeadWidth = 5;
						edge.ArrowHeadLength = 5;
						edge.ZIndex = 1;
					}

					if (edge.Start == node || edge.End == node)
					{
						edge.Color = new SolidColorBrush(Colors.SeaGreen);
						edge.Thickness = 3;
						edge.ArrowHeadColor = new SolidColorBrush(Colors.SeaGreen);
						edge.ArrowHeadWidth = 10;
						edge.ArrowHeadLength = 10;
						edge.ZIndex = 2;
					}
				}

				selectedNode = node;
			};
		}
	}
	
	private void simpleSubGraphExample(StackPanel mainPanel)
	{
		var node0 = new GraphNode() { ContentControl = new TextBlock() { Text = "Node 0" } };
		var node1 = new GraphNode() { ContentControl = new TextBlock() { Text = "Node 1" } };
		var node2 = new GraphNode() { ContentControl = new TextBlock() { Text = "Node 2" } };
		var node3 = new GraphNode() { ContentControl = new TextBlock() { Text = "Node 3" } };

		var subGraph = new SubGraph()
		{
			ContentControl = new TextBlock() { Text = "SubGraph" },
			Graph = new Graph()
			{
				Nodes = {
					node2,
					node3
				},
				Edges = {
					new GraphEdge(node2, node3) { IsDirected = true }
				},
				Layout = new SpringGraphLayout()
				{
					Iterations = 100,
					Width = 800,
					Height = 400
				}
			}
		};

		graph = new Graph()
		{
			Nodes = {
				node0,
				node1,
				subGraph
			},
			Edges = {
				new GraphEdge(node0, node1) { IsDirected = true },
				new GraphEdge(node1, node2) { IsDirected = true }
			},
			Layout = new SpringGraphLayoutWithSubGraphs()
			{
				Iterations = 1000,
				Width = 800,
				Height = 400,
			}
		};

		initializeGraphView(mainPanel);
	}


	private void nodesInSubgraphExample(StackPanel mainPanel, int numberOfNodesInSubgraph, int numberOfEdgesInSubgraph)
	{
		var node0 = new GraphNode() { ContentControl = new TextBlock() { Text = "Node 0" } };
		var node1 = new GraphNode() { ContentControl = new TextBlock() { Text = "Node 1" } };


		var subNodes = new List<GraphNode>();
		for (int i = 0; i < numberOfNodesInSubgraph; i++)
		{
			subNodes.Add(new GraphNode() { ContentControl = new TextBlock() { Text = $"{i}" } });
		}

		var subEdges = new List<GraphEdge>();
		for (int i = 0; i < numberOfEdgesInSubgraph; i++)
		{
			var random = new Random();
			var randomNode1 = subNodes[random.Next(0, subNodes.Count)];
			var randomNode2 = subNodes[random.Next(0, subNodes.Count)];
			subEdges.Add(new GraphEdge(randomNode1, randomNode2) { 
				IsDirected = true,
				Thickness = 1, 
			});
		}

		var graphOfSubGraph = new Graph()
		{
			Layout = new SpringGraphLayout()
			{
				Iterations = 100,
				Width = 800,
				Height = 400
			}
		};
		graphOfSubGraph.AddAllNodes(subNodes);
		graphOfSubGraph.AddAllEdges(subEdges);

		var subGraph = new SubGraph()
		{
			ContentControl = new TextBlock() { Text = "SubGraph" },
			Graph = graphOfSubGraph,
		};

		graph = new Graph()
		{
			Nodes = {
				node0,
				node1,
				subGraph
			},
			Edges = {
				new GraphEdge(node0, node1) { IsDirected = true },
				new GraphEdge(node1, subNodes[0]) { IsDirected = true }
			},
			Layout = new SpringGraphLayoutWithSubGraphs()
			{
				Iterations = 100,
				Width = 800,
				Height = 400
			}
		};

		initializeGraphView(mainPanel);
	}
	
	private void simpleSubGraphDeepNestingExample(StackPanel mainPanel)
	{
		var node0 = new GraphNode() { ContentControl = new TextBlock() { Text = "Node 0" } };
		var node1 = new GraphNode() { ContentControl = new TextBlock() { Text = "Node 1" } };
		var node2 = new GraphNode() { ContentControl = new TextBlock() { Text = "Node 2" } };
		var node3 = new GraphNode() { ContentControl = new TextBlock() { Text = "Node 3" } };
		var node4 = new GraphNode() { ContentControl = new TextBlock() { Text = "Node 4" } };
		var node5 = new GraphNode() { ContentControl = new TextBlock() { Text = "Node 5" } };
		var node6 = new GraphNode() { ContentControl = new TextBlock() { Text = "Node 6" } };
		var node7 = new GraphNode() { ContentControl = new TextBlock() { Text = "Node 7" } };
		var node8 = new GraphNode() { ContentControl = new TextBlock() { Text = "Node 8" } };

		var subGraph = new SubGraph()
		{
			ContentControl = new TextBlock() { Text = "SubGraph" },
			Graph = new Graph()
			{
				Nodes = {
					node2,
					node3
				},
				Edges = {
					new GraphEdge(node2, node3) { IsDirected = true }
				},
				Layout = new SpringGraphLayoutWithSubGraphs()
				{
					Iterations = 100,
					Width = 800,
					Height = 400
				}
			}
		};

		var subGraph2 = new SubGraph()
		{
			ContentControl = new TextBlock() { Text = "SubGraph 2" },
			Graph = new Graph()
			{
				Nodes = {
					node4,
					node5,
					subGraph
				},
				Edges = {
					new GraphEdge(node4, node5) { IsDirected = true },
					new GraphEdge(node5, node2) { IsDirected = true }
				},
				Layout = new SpringGraphLayoutWithSubGraphs()
				{
					Iterations = 100,
					Width = 800,
					Height = 400
				}
			}
		};

		var subGraph3 = new SubGraph()
		{
			ContentControl = new TextBlock() { Text = "SubGraph 3" },
			Graph = new Graph()
			{
				Nodes = {
					node6,
					subGraph2
				},
				Edges = {
					new GraphEdge(node6, node2) { IsDirected = true },
				},
				Layout = new SpringGraphLayoutWithSubGraphs()
				{
					Iterations = 100,
					Width = 800,
					Height = 400
				}
			}
		};

		var subGraph4 = new SubGraph()
		{
			ContentControl = new TextBlock() { Text = "SubGraph 4" },
			Graph = new Graph()
			{
				Nodes = {
					node7,
					subGraph3
				},
				Edges = {
					new GraphEdge(node7, node2) { IsDirected = true },
				},
				Layout = new SpringGraphLayoutWithSubGraphs()
				{
					Iterations = 100,
					Width = 800,
					Height = 400
				}
			}
		};
		
		graph = new Graph()
		{
			Nodes = {
				node8,
				subGraph4
			},
			Edges = {
				new GraphEdge(node8, node2) { IsDirected = true },
			},
			Layout = new SpringGraphLayoutWithSubGraphs()
			{
				Iterations = 100,
				Width = 800,
				Height = 400,
			},
		};
		
		initializeGraphView(mainPanel);

	}
			

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
