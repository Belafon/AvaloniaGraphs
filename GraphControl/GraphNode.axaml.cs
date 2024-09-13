using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaGraphs.ViewModels;
using DynamicData;

namespace AvaloniaGraphs.GraphControl;

public partial class GraphNode : UserControl
{
	public GraphNodeViewModel Model { get; }
	public StackPanel ContentContainer;
	public SubGraph? ContainerSubGraph { get; set; }
	public GraphNode(int x = 0, int y = 0)
	{
		InitializeComponent();
		ContentContainer = this.FindControl<StackPanel>("content")!;
		Border = this.FindControl<Border>("nodesBorder")!;
		Model = new GraphNodeViewModel(this);
		DataContext = Model;
		if (double.IsNaN(Width))
			Width = 50;

		if (double.IsNaN(Height))
			Height = 50;

		Background = Brushes.Azure;

		RealPosition = new Point(x, y);
	}

	public bool IsInvariantPositionToGraphLayout { get; set; } = false;
	public Border Border { get; set; }
	private Control? _contentControl;
	public Control? ContentControl
	{
		get => _contentControl;
		set
		{
			ContentContainer.Children.Clear();
			if (value is not null)
				ContentContainer.Children.Add(value);
			_contentControl = value;
		}
	}

	public Point PositionInCanvas
	{
		get => Model.PositionInCanvas;
		set => Model.PositionInCanvas = value;
	}

	internal Point RealPosition
	{
		get => Model.RealPosition;
		set => Model.RealPosition = value;
	}

	public Point LastRealPosition
	{
		get => Model.LastRealPosition;
		set => Model.LastRealPosition = value;
	}
	
	public Point LastPositionInCanvas
	{
		get => Model.LastPositionInCanvas;
		set => Model.LastPositionInCanvas = value;
	}
	


	internal EventHandler<EventArgsWithPositionDiff>? OnRealPositionChangedHandler;
	public void SetRealPosition(Point position)
	{
		var diff = RealPosition - PositionInCanvas;
		RealPosition = position;
		var args = new EventArgsWithPositionDiff(diff);

		if (ContainerSubGraph is not null
			&& !ContainerSubGraph.StartBorderNode.IsVisible)
			return;
			
		OnRealPositionChangedHandler?.Invoke(this, args);
	}


	public EventHandler<PointerPressedEventArgs>? OnNodePointerPressedHandler;

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}

	public Point? DistanceToSubGraph { get; set; }
}

public class GraphNodeViewModel : ViewModelBase, INotifyPropertyChanged
{
	private GraphNode _node;
	private double x;
	public double X
	{
		get => x;
		set
		{
			this.RaiseAndSetIfChanged(ref x, value);
		}
	}

	private double y;
	public double Y
	{
		get => y;
		set
		{
			this.RaiseAndSetIfChanged(ref y, value);
		}
	}

	public Point LastPositionInCanvas { get; set; }
	public Point LastRealPosition { get; set; }
	private Point positionInCanvas;
	internal Point PositionInCanvas
	{
		get => new Point(X + _node.Width / 2, Y + _node.Height / 2);
		set
		{	
			X = value.X - _node.Width / 2;
			Y = value.Y - _node.Height / 2;
			this.RaiseAndSetIfChanged(ref positionInCanvas, value);
		}
	}

	private Point realPosition;
	public Point RealPosition
	{
		get => realPosition;
		set
		{
			this.RaiseAndSetIfChanged(ref realPosition, value);	
		}
	}

	public GraphNodeViewModel(GraphNode node)
	{
		_node = node;
	}
}

public class EventArgsWithPositionDiff : EventArgs
{
	public Point Diff { get; }
	public EventArgsWithPositionDiff(Point diff)
	{
		Diff = diff;
	}
}

