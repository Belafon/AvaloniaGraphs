using System.Drawing;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaGraphs.GraphControl;
using Avalonia.ReactiveUI;
using Avalonia;
using System.Threading.Tasks;
using Avalonia.Threading;
using System.Collections;
using AvaloniaGraphs.GraphsLayout;


namespace AvaloniaGraphs.Views;

public partial class MainWindow : Window
{
	private Graph graph;
	public MainWindow()
	{
		InitializeComponent();
		var mainPanel = this.FindControl<StackPanel>("MainPanel")!;

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
				}
			},
			Background = new SolidColorBrush(Colors.Green)
		};

		var node1 = new GraphNode();
		var node2 = new GraphNode();

		graph = new Graph()
		{
			Nodes = {
				node0,
				node1,
				node2
			},
			Edges = {
				new GraphEdge(node0, node1){
					IsDirected = true
				},
				new GraphEdge(node1, node2)
				{
					IsDirected = true
				},
				new GraphEdge(node2, node0)
				{
					IsDirected = true
				}
			},
			Layout = new SpringLayout()
			{
				Iterations = 100,
				Width = 800,
				Height = 400
			}
		};
		var graphView = new GraphView(graph)
		{
			[!WidthProperty] = this[!WidthProperty],
			[!HeightProperty] = this[!HeightProperty]
		};
		
		graph.Layout?.ApplyLayout(graph);
		
		mainPanel.Children.Add(graphView);
		
		
	/*	new Task(async () =>
		{
			await Task.Delay(2000);
			await Dispatcher.UIThread.InvokeAsync(() => 
				node0.SetRealPosition(new(100, 100)));
			await Task.Delay(2000);
			await Dispatcher.UIThread.InvokeAsync(() => 
				node0.SetRealPosition(new(50, 300)));
			await Task.Delay(2000);
			await Dispatcher.UIThread.InvokeAsync(() => 
				node0.SetRealPosition(new(200, 150)));
		}).Start();*/
	}
	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}