using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaGraphs.ViewModels;

namespace AvaloniaGraphs.GraphControl;

public partial class GraphNode : UserControl
{
	public GraphNodeViewModel Model { get; }
	public StackPanel ContentContainer;
	public GraphNode(int x, int y)
	{
		InitializeComponent();
		ContentContainer = this.FindControl<StackPanel>("content")!;
		Border = this.FindControl<Border>("nodesBorder")!;
		Model = new GraphNodeViewModel();
		DataContext = Model;
		Width = 50;
		Height = 50;
		Background = Brushes.Azure;

		PositionInCanvas = new Point(x, y);
		RealPosition = new Point(x, y);

	}


	public double X
	{
		get => Model.X;
		set => Model.X = value;
	}

	public double Y
	{
		get => Model.Y;
		set => Model.Y = value;
	}

	public Border Border { get; set; }
	private Control? _content;
	public Control? ContentControl
	{
		get => _content;
		set
		{
			ContentContainer.Children.Clear();
			if (value is not null)
				ContentContainer.Children.Add(value);
			_content = value;
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

	internal EventHandler<EventArgsWithPositionDiff> OnRealPositionChangedHandler;
	public void SetRealPosition(Point position)
	{
		var diff = RealPosition - PositionInCanvas;
		RealPosition = position;
		var args = new EventArgsWithPositionDiff(diff);
		OnRealPositionChangedHandler?.Invoke(this, args);
	}


	public EventHandler<PointerPressedEventArgs> OnNodePointerPressedHandler;

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}

public class GraphNodeViewModel : ViewModelBase, INotifyPropertyChanged
{
	private double x;
	public double X
	{
		get => x;
		set
		{
			this.RaiseAndSetIfChanged(ref x, value);
			PositionInCanvas = new Point(X, Y);
		}
	}

	private double y;
	public double Y
	{
		get => y;
		set
		{
			this.RaiseAndSetIfChanged(ref y, value);
			PositionInCanvas = new Point(X, Y);
		}
	}

	private Point positionInCanvas;
	internal Point PositionInCanvas
	{
		get => new Point(X, Y);
		set
		{
			if (X == value.X && Y == value.Y)
				return;
			X = value.X;
			Y = value.Y;
			this.RaiseAndSetIfChanged(ref positionInCanvas, value);
		}
	}

	private Point realPosition;
	public Point RealPosition
	{
		get => realPosition;
		set => this.RaiseAndSetIfChanged(ref realPosition, value);
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