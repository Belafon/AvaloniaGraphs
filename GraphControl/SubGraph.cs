using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AvaloniaGraphs.GraphControl;

public class SubGraph : GraphNode, INotifyPropertyChanged
{
	private Graph graph = new();
	public Graph Graph { 
		get => graph;
		set
		{
			graph = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Graph)));
		}
	}

	public new event PropertyChangedEventHandler? PropertyChanged;
	
	public GraphNode StartBorderNode { get; private set; } = new();
	public TextBlock BorderContainerTitle { get; set; } = new();
	public Border BorderContainer { get; set; } = new();
	/// <summary>
	/// Tells the minimum width of the border container
	/// When the width is higher than this value, the nodes of
	/// the subgraphs are hidden. Also the border of the container is hidden
	/// This value can be changed by a <see cref="GraphLayout"/>
	/// </summary>

	private double minWidthOfBorderContainer;
	public double MinWidthOfBorderContainer { 
		get => minWidthOfBorderContainer;
		set
		{
			minWidthOfBorderContainer = value;
			OnMinWidthOrHeightBorderContainerChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MinWidthOfBorderContainer)));
		}
	}

	private double minHeightOfBorderContainer;
	public double MinHeightOfBorderContainer { 
		get => minHeightOfBorderContainer;
		set
		{
			minHeightOfBorderContainer = value;
			OnMinWidthOrHeightBorderContainerChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MinHeightOfBorderContainer)));
		}
	}

	public event PropertyChangedEventHandler? OnMinWidthOrHeightBorderContainerChanged;
	public SubGraph(int x = 0, int y = 0) : base(x, y)
	{
		BorderContainer = StartBorderNode.Border;
		Background = Brushes.LightBlue;
		setBorderContainer();
		
		PropertyChanged += (sender, args) =>
		{
			if (args.PropertyName == nameof(Graph))
			{
				foreach (var node in Graph.Nodes)
				{
					node.ContainerSubGraph = this;
				}
				Graph.Nodes.CollectionChanged += (sender, args) =>
				{
					if (args.NewItems is not null)
					{
						foreach (GraphNode node in args.NewItems)
						{
							node.ContainerSubGraph = this;
						}
					}
				};
			}
		};
	}


	private void setBorderContainer()
	{
		var dockPanel = new DockPanel();

		var textBlock = new TextBlock()
		{
			Text = "Subgraph title",
			Foreground = Brushes.White,
		};
		
		BorderContainerTitle = textBlock;

		DockPanel.SetDock(textBlock, Dock.Bottom);
		dockPanel.Children.Add(textBlock);

		var emptyContent = new StackPanel();


		dockPanel.Children.Add(emptyContent);
		DockPanel.SetDock(emptyContent, Dock.Top);


		StartBorderNode.ContentControl = dockPanel;
		StartBorderNode.Background = Brushes.Transparent;
		BorderContainer.BorderBrush = Brushes.Red;
		BorderContainer.BorderThickness = new Thickness(1);
		BorderContainer.Background = Brushes.Transparent;

		BorderContainer.SetValue(Canvas.ZIndexProperty, -2);
		StartBorderNode.SetValue(Canvas.ZIndexProperty, -2);
		BorderContainerTitle.SetValue(Canvas.ZIndexProperty, -2);
		emptyContent.SetValue(Canvas.ZIndexProperty, -2);
		dockPanel.SetValue(Canvas.ZIndexProperty, -2);
	}
}
