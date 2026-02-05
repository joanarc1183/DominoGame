using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DominoGame.Wpf.Controls;
using DominoGame.Core;
using DominoGame.Wpf.ViewModels;

namespace DominoGame.Wpf;

public partial class MainWindow : Window
{
    private GameViewModel? _viewModel;
    private Point _dragStartPoint;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += HandleLoaded;
    }

    public MainWindow(GameViewModel viewModel) : this()
    {
        SetViewModel(viewModel);
    }

    private void SetViewModel(GameViewModel viewModel)
    {
        if (_viewModel is not null)
        {
            _viewModel.GameEnded -= HandleGameEnded;
            _viewModel.RoundEnded -= HandleRoundEnded;
        }

        _viewModel = viewModel;
        _viewModel.GameEnded += HandleGameEnded;
        _viewModel.RoundEnded += HandleRoundEnded;
        DataContext = _viewModel;
    }

    private void HandleRoundEnded(string message)
    {
        MessageBox.Show(
            message,
            "Ronde Selesai",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void HandleGameEnded(Player winner)
    {
        var result = MessageBox.Show(
            $"{winner.Name} menang! Main lagi?",
            "Game Over",
            MessageBoxButton.YesNo,
            MessageBoxImage.Information);

        if (result == MessageBoxResult.Yes)
        {
            Hide();
            var setup = new SetupWindow();
            if (setup.ShowDialog() == true)
            {
                var vm = new GameViewModel(setup.Players, maxScoreToWin: setup.MaxScoreToWin);
                var newWindow = new MainWindow(vm);
                Application.Current.MainWindow = newWindow;
                newWindow.Show();
                Close();
                return;
            }

            Close();
            return;
        }

        Close();
    }

    private void HandTile_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        if (BoardItems is not null)
            BoardItems.LayoutUpdated += BoardItems_LayoutUpdated;

        UpdateDropZones();
    }

    private void BoardItems_LayoutUpdated(object? sender, EventArgs e)
    {
        UpdateDropZones();
    }

    private void UpdateDropZones()
    {
        if (BoardItems is null || BoardArea is null || DropLeftZone is null || DropRightZone is null)
            return;

        int count = BoardItems.Items.Count;
        if (count == 0)
        {
            const double defaultWidth = 70;
            const double defaultHeight = 35;
            const double gap = 8;

            DropLeftZone.Visibility = Visibility.Visible;
            DropRightZone.Visibility = Visibility.Visible;
            DropLeftZone.Width = defaultWidth;
            DropLeftZone.Height = defaultHeight;
            DropRightZone.Width = defaultWidth;
            DropRightZone.Height = defaultHeight;

            double centerX = (BoardArea.ActualWidth - defaultWidth * 2 - gap) / 2;
            double centerY = (BoardArea.ActualHeight - defaultHeight) / 2;
            Canvas.SetLeft(DropLeftZone, centerX);
            Canvas.SetTop(DropLeftZone, centerY);
            Canvas.SetLeft(DropRightZone, centerX + defaultWidth + gap);
            Canvas.SetTop(DropRightZone, centerY);
            return;
        }

        DropLeftZone.Visibility = Visibility.Visible;
        DropRightZone.Visibility = Visibility.Visible;

        var first = BoardItems.ItemContainerGenerator.ContainerFromIndex(0) as FrameworkElement;
        var last = BoardItems.ItemContainerGenerator.ContainerFromIndex(count - 1) as FrameworkElement;

        if (first is not null)
        {
            var tileRect = GetElementRect(first);
            DropLeftZone.Width = tileRect.Width;
            DropLeftZone.Height = tileRect.Height;

            var flow = OShapePanel.GetFlowDirection(first);
            var opposite = Opposite(flow);
            ApplyDropRotation(DropLeftZone, opposite);
            Point target = OffsetByDirection(tileRect, DropLeftZone.Width, DropLeftZone.Height, opposite, 6);
            SetClampedPosition(DropLeftZone, target);
        }

        if (last is not null)
        {
            var tileRect = GetElementRect(last);
            DropRightZone.Width = tileRect.Width;
            DropRightZone.Height = tileRect.Height;

            var flow = OShapePanel.GetFlowDirection(last);
            ApplyDropRotation(DropRightZone, flow);
            Point target = OffsetByDirection(tileRect, DropRightZone.Width, DropRightZone.Height, flow, 6);
            SetClampedPosition(DropRightZone, target);
        }
    }

    private static OShapePanel.PathDirection Opposite(OShapePanel.PathDirection direction)
        => direction switch
        {
            OShapePanel.PathDirection.Right => OShapePanel.PathDirection.Left,
            OShapePanel.PathDirection.Left => OShapePanel.PathDirection.Right,
            OShapePanel.PathDirection.Up => OShapePanel.PathDirection.Down,
            _ => OShapePanel.PathDirection.Up
        };

    private static Point OffsetByDirection(Rect origin, double width, double height, OShapePanel.PathDirection direction, double gap)
    {
        return direction switch
        {
            OShapePanel.PathDirection.Right => new Point(origin.Right + gap, origin.Top),
            OShapePanel.PathDirection.Left => new Point(origin.Left - width - gap, origin.Top),
            OShapePanel.PathDirection.Down => new Point(origin.Left, origin.Bottom + gap),
            _ => new Point(origin.Left, origin.Top - height - gap)
        };
    }

    private static void ApplyDropRotation(FrameworkElement element, OShapePanel.PathDirection direction)
    {
        // Horizontal for top/bottom (Left/Right flow), vertical for left/right (Up/Down flow)
        if (direction == OShapePanel.PathDirection.Left || direction == OShapePanel.PathDirection.Right)
            element.LayoutTransform = new RotateTransform(-90);
        else
            element.LayoutTransform = Transform.Identity;
    }

    private void SetClampedPosition(FrameworkElement element, Point pos)
    {
        double x = Math.Max(0, Math.Min(pos.X, BoardArea!.ActualWidth - element.ActualWidth));
        double y = Math.Max(0, Math.Min(pos.Y, BoardArea!.ActualHeight - element.ActualHeight));
        Canvas.SetLeft(element, x);
        Canvas.SetTop(element, y);
    }

    private Rect GetElementRect(FrameworkElement element)
    {
        Point pos = element.TranslatePoint(new Point(0, 0), BoardArea);
        return new Rect(pos, new Size(element.ActualWidth, element.ActualHeight));
    }

    private void HandTile_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
            return;

        Point position = e.GetPosition(null);
        if (Math.Abs(position.X - _dragStartPoint.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(position.Y - _dragStartPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;

        if (sender is not ContentPresenter presenter)
            return;

        if (presenter.DataContext is not DominoTileViewModel tile)
            return;

        var itemsControl = FindAncestor<ItemsControl>(presenter);
        if (itemsControl?.DataContext is not PlayerScoreViewModel playerVm)
            return;

        if (!playerVm.IsCurrent || !tile.IsPlayable)
            return;

        DragDrop.DoDragDrop(presenter, tile, DragDropEffects.Move);
    }

    private void DropZone_DragOver(object sender, DragEventArgs e)
    {
        if (!TryGetTileFromDrag(e, out var tile) || _viewModel is null)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        var side = ReferenceEquals(sender, DropLeftZone) ? BoardSide.Left : BoardSide.Right;
        e.Effects = _viewModel.CanPlaceDomino(tile, side)
            ? DragDropEffects.Move
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void DropLeftZone_Drop(object sender, DragEventArgs e)
    {
        if (!TryGetTileFromDrag(e, out var tile) || _viewModel is null)
            return;

        _viewModel.TryPlaceDominoFromDrag(tile, BoardSide.Left);
    }

    private void DropRightZone_Drop(object sender, DragEventArgs e)
    {
        if (!TryGetTileFromDrag(e, out var tile) || _viewModel is null)
            return;

        _viewModel.TryPlaceDominoFromDrag(tile, BoardSide.Right);
    }

    private static bool TryGetTileFromDrag(DragEventArgs e, out DominoTileViewModel tile)
    {
        tile = null!;
        if (!e.Data.GetDataPresent(typeof(DominoTileViewModel)))
            return false;

        tile = (DominoTileViewModel)e.Data.GetData(typeof(DominoTileViewModel))!;
        return tile is not null;
    }

    private static T? FindAncestor<T>(DependencyObject child) where T : DependencyObject
    {
        DependencyObject current = child;
        while (current is not null)
        {
            if (current is T typed)
                return typed;
            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }
}
