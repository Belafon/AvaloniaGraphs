using System.Drawing;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaGraphs.GraphControl;
using AvaloniaGraphs.GraphsLayout;
using System;


namespace AvaloniaGraphs.Views;

public partial class MainWindow : Window
{
	private Graph graph;
	public MainWindow()
	{
		InitializeComponent();
		var mainPanel = this.FindControl<StackPanel>("MainPanel")!;

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
	//						new GraphEdge(subNode1, subNode2) { IsDirected = true },
	//						new GraphEdge(subNode2, node0) { IsDirected = true }
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
				//node2,
				//node3,
				subGraph,
				//subgraph2

			},
			Edges = {
/*				new GraphEdge(node0, node1){
					IsDirected = true
				},
				new GraphEdge(node1, node2)
				{
					IsDirected = true
				},
				new GraphEdge(node2, node0)
				{
					IsDirected = true
				},
				new GraphEdge(node3, subNode1)
				{
					IsDirected = true
				},*/
			},
			Layout = new SpringGraphLayoutWithSubGraphs()
			{
				Iterations = 100,
				Width = 800,
				Height = 400,
			},
			ApplyLayoutOnEachAdd = false
		};
		var graphView = new GraphView(graph)
		{
			[!WidthProperty] = this[!WidthProperty],
			[!HeightProperty] = this[!HeightProperty]
		};
		graph.Layout?.ApplyLayout(graph);


		mainPanel.Children.Add(graphView);


/*		new Task(async () =>
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
*/
/*		new Task(async () =>
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
		}).Start();*/
	}
	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}