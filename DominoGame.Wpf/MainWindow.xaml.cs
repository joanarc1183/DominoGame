using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
