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

    // Konstruktor yang menerima view model untuk langsung dipasang ke DataContext.
    public MainWindow(GameViewModel viewModel) : this()
    {
        SetViewModel(viewModel);
    }

    // Mengganti view model aktif dan mereset event handler terkait.
    private void SetViewModel(GameViewModel viewModel)
    {
        // Jaga subscription event tetap sinkron saat mengganti view model aktif.
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

    // Menampilkan dialog saat ronde berakhir.
    private void HandleRoundEnded(string message)
    {
        // Notifikasi UI saja; logika game ada di view model.
        MessageBox.Show(
            message,
            "Ronde Selesai",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    // Menampilkan dialog saat game selesai dan menangani alur main lagi.
    private void HandleGameEnded(Player winner)
    {
        // Alur UI untuk restart; dialog dan window tetap di layer view.
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

    // Menyimpan titik awal mouse untuk kebutuhan drag tile.
    private void HandTile_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Simpan posisi awal mouse untuk mendeteksi ambang drag.
        _dragStartPoint = e.GetPosition(null);
    }

    // Menyiapkan handler layout dan melakukan penempatan drop zone awal.
    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        // Posisikan ulang drop zone saat layout board berubah.
        if (BoardItems is not null)
            BoardItems.LayoutUpdated += BoardItems_LayoutUpdated;

        UpdateDropZones();
    }

    // Memperbarui posisi drop zone saat layout berubah.
    private void BoardItems_LayoutUpdated(object? sender, EventArgs e)
    {
        UpdateDropZones();
    }

    // Menghitung ulang ukuran, orientasi, dan posisi drop zone.
    private void UpdateDropZones()
    {
        // Hitung ukuran/orientasi drop zone dan posisinya relatif ke tile pertama/terakhir.
        if (BoardItems is null || BoardArea is null || DropLeftZone is null || DropRightZone is null)
            return;

        int count = BoardItems.Items.Count;
        if (count == 0)
        {
            // Saat board kosong, letakkan kedua drop zone di tengah.
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
            var flow = OShapePanel.GetFlowDirection(first);
            var opposite = Opposite(flow);
            SetDropOrientation(DropLeftZone, tileRect, opposite);
            Point target = OffsetByDirection(tileRect, DropLeftZone.Width, DropLeftZone.Height, opposite, 6);
            SetClampedPosition(DropLeftZone, target);
        }

        if (last is not null)
        {
            var tileRect = GetElementRect(last);
            var flow = OShapePanel.GetFlowDirection(last);
            SetDropOrientation(DropRightZone, tileRect, flow);
            Point target = OffsetByDirection(tileRect, DropRightZone.Width, DropRightZone.Height, flow, 6);
            SetClampedPosition(DropRightZone, target);
        }
    }

    // Mengembalikan arah yang berlawanan dari arah input.
    private static OShapePanel.PathDirection Opposite(OShapePanel.PathDirection direction)
        => direction switch
        {
            OShapePanel.PathDirection.Right => OShapePanel.PathDirection.Left,
            OShapePanel.PathDirection.Left => OShapePanel.PathDirection.Right,
            OShapePanel.PathDirection.Up => OShapePanel.PathDirection.Down,
            _ => OShapePanel.PathDirection.Up
        };

    // Menghitung posisi target drop zone berdasarkan arah aliran tile.
    private static Point OffsetByDirection(Rect origin, double width, double height, OShapePanel.PathDirection direction, double gap)
    {
        double centeredX = origin.Left + (origin.Width - width) / 2;
        return direction switch
        {
            OShapePanel.PathDirection.Right => new Point(origin.Right + gap, origin.Top),
            OShapePanel.PathDirection.Left => new Point(origin.Left - width - gap, origin.Top),
            OShapePanel.PathDirection.Down => new Point(centeredX, origin.Bottom + gap),
            _ => new Point(centeredX, origin.Top - height - gap)
        };
    }

    // Menyetel ukuran drop zone mengikuti orientasi tile.
    private static void SetDropOrientation(FrameworkElement element, Rect tileRect, OShapePanel.PathDirection direction)
    {
        bool horizontal = direction == OShapePanel.PathDirection.Left || direction == OShapePanel.PathDirection.Right;
        element.LayoutTransform = Transform.Identity;
        if (horizontal)
        {
            element.Width = tileRect.Width;
            element.Height = tileRect.Height;
        }
        else
        {
            element.Width = tileRect.Height;
            element.Height = tileRect.Width;
        }
    }

    // Menjaga posisi drop zone tetap berada di dalam area board.
    private void SetClampedPosition(FrameworkElement element, Point pos)
    {
        double x = Math.Max(0, Math.Min(pos.X, BoardArea!.ActualWidth - element.ActualWidth));
        double y = Math.Max(0, Math.Min(pos.Y, BoardArea!.ActualHeight - element.ActualHeight));
        Canvas.SetLeft(element, x);
        Canvas.SetTop(element, y);
    }

    // Mengambil bounding rectangle sebuah elemen relatif terhadap BoardArea.
    private Rect GetElementRect(FrameworkElement element)
    {
        Point pos = element.TranslatePoint(new Point(0, 0), BoardArea);
        return new Rect(pos, new Size(element.ActualWidth, element.ActualHeight));
    }

    // Memulai drag tile dari tangan pemain saat syarat terpenuhi.
    private void HandTile_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        // Mulai drag hanya jika tile playable dan pointer sudah bergerak cukup jauh.
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

    // Mengatur efek drag ketika tile melayang di atas drop zone.
    private void DropZone_DragOver(object sender, DragEventArgs e)
    {
        // Beri feedback visual apakah tile bisa di-drop di sisi tersebut.
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

    // Menangani drop tile di sisi kiri board.
    private void DropLeftZone_Drop(object sender, DragEventArgs e)
    {
        // Commit langkah lewat view model.
        if (!TryGetTileFromDrag(e, out var tile) || _viewModel is null)
            return;

        _viewModel.TryPlaceDominoFromDrag(tile, BoardSide.Left);
    }

    // Menangani drop tile di sisi kanan board.
    private void DropRightZone_Drop(object sender, DragEventArgs e)
    {
        // Commit langkah lewat view model.
        if (!TryGetTileFromDrag(e, out var tile) || _viewModel is null)
            return;

        _viewModel.TryPlaceDominoFromDrag(tile, BoardSide.Right);
    }

    // Mencoba mengambil tile dari data drag & drop.
    private static bool TryGetTileFromDrag(DragEventArgs e, out DominoTileViewModel tile)
    {
        // Ambil tile yang sedang di-drag (jika ada) dari payload data.
        tile = null!;
        if (!e.Data.GetDataPresent(typeof(DominoTileViewModel)))
            return false;

        tile = (DominoTileViewModel)e.Data.GetData(typeof(DominoTileViewModel))!;
        return tile is not null;
    }

    // Mencari parent visual bertipe T dari sebuah elemen.
    private static T? FindAncestor<T>(DependencyObject child) where T : DependencyObject
    {
        // Telusuri visual tree ke atas untuk mencari parent dengan tipe yang diminta.
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
