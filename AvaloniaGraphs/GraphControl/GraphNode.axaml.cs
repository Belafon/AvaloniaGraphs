using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaGraphs.ViewModels;

namespace AvaloniaGraphs.GraphControl;

public partial class GraphNode : UserControl
{
	public GraphNodeViewModel Model { get; }
	public StackPanel ContentContainer;
	public GraphNode()
	{
		InitializeComponent();
		ContentContainer = this.FindControl<StackPanel>("content")!;
		Model = new GraphNodeViewModel();
		DataContext = Model;
		Width = 50;
		Height = 50;
		Background = Brushes.Azure;
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
	
	public Border? Border
	{
		get => Model.Border;
		set => Model.Border = value;
	}
	private UserControl? _content;
	public UserControl? ContentControl
	{
		get => _content;
		set
		{
			ContentContainer.Children.Clear();
			if(value is not null)
				ContentContainer.Children.Add(value);
			_content = value;
		}
	}

	public Point Position { 
		get => Model.Position;
		set => Model.Position = value;
	} 
}

public class GraphNodeViewModel : ViewModelBase, INotifyPropertyChanged
{
	private Border? border;
	public Border? Border
	{
		get => border;
		set => this.RaiseAndSetIfChanged(ref border, value);
	}
	private double x;
	public double X
	{
		get => x;
		set {
			this.RaiseAndSetIfChanged(ref x, value);
			Position = new Point(X, Y);
		}
	}

	private double y;
	public double Y
	{
		get => y;
		set {
			this.RaiseAndSetIfChanged(ref y, value);
			Position = new Point(X, Y);
		}
	}

	private Point position;
	internal Point Position{
		get => new Point(X, Y);
		set {
			if(X == value.X && Y == value.Y)
				return;
			X = value.X;
			Y = value.Y;
			this.RaiseAndSetIfChanged(ref position, value);
		}
	}
}