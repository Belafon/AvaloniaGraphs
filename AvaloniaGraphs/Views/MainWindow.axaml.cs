using System.Drawing;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaGraphs.GraphControl;
using Avalonia.ReactiveUI;
using Avalonia;
using System.Threading.Tasks;
using Avalonia.Threading;


namespace AvaloniaGraphs.Views;

public partial class MainWindow : Window
{
	private Graph graph;
	public MainWindow()
	{
		InitializeComponent();
		var mainPanel = this.FindControl<StackPanel>("MainPanel")!;
		mainPanel.Background = new SolidColorBrush(Colors.DarkBlue);

		var node0 = new GraphNode() { Position = new(60, 60) };
		var node1 = new GraphNode() { Position = new(140, 140) };
		var node2 = new GraphNode() { Position = new(200, 200) };

		graph = new Graph()
		{
			Nodes = {
				node0,
				node1,
				node2
			},
			Edges = {
				new GraphEdge(node0, node1),
				new GraphEdge(node1, node2),
				new GraphEdge(node2, node0)
			}
		};
		var graphView = new GraphView(graph)
		{
			[!WidthProperty] = this[!WidthProperty],
			[!HeightProperty] = this[!HeightProperty]
		};
		mainPanel.Children.Add(graphView);
		
		
		new Task(async () =>
		{
			await Task.Delay(2000);
			await Dispatcher.UIThread.InvokeAsync(() => 
				node0.Position = new(100, 100));
			await Task.Delay(2000);
			await Dispatcher.UIThread.InvokeAsync(() => 
				node0.Position = new(200, 100));
			await Task.Delay(2000);
			await Dispatcher.UIThread.InvokeAsync(() => 
				node0.Position = new(50, 300));
		}).Start();
	}
	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}