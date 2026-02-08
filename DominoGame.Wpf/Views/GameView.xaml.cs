using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using DominoGame.Core;
using DominoGame.Wpf.Controls;
using DominoGame.Wpf.ViewModels;

namespace DominoGame.Wpf.Views;

public partial class GameView : UserControl
{
    // View model yang diikat ke GameView.
    private GameViewModel? _viewModel;
    // Titik awal drag untuk mendeteksi gesture drag-drop.
    private Point _dragStartPoint;
    // Ukuran default tile saat ukuran aktual belum tersedia.
    private const double DefaultTileWidth = 35;
    private const double DefaultTileHeight = 70;
    // Adorner untuk menampilkan preview drag.
    private DragAdorner? _dragAdorner;
    // Layer tempat adorner dirender.
    private AdornerLayer? _dragAdornerLayer;
    // Presenter yang sedang didrag.
    private ContentPresenter? _draggedPresenter;
    // Presenter kandidat saat mouse down.
    private ContentPresenter? _dragCandidatePresenter;

    /// <summary>
    /// Inisialisasi GameView dan pasang handler Loaded.
    /// </summary>
    public GameView()
    {
        InitializeComponent();
        Loaded += HandleLoaded;
    }

    /// <summary>
    /// Menetapkan view model dan DataContext.
    /// </summary>
    public void SetViewModel(GameViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    /// <summary>
    /// Menangkap titik awal drag saat mouse ditekan pada hand.
    /// </summary>
    private void HandItems_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ItemsControl itemsControl)
            return;

        _dragCandidatePresenter = GetContainerFromEvent(itemsControl, e.OriginalSource as DependencyObject);
        if (_dragCandidatePresenter is null)
            return;

        _dragStartPoint = e.GetPosition(this);
    }

    /// <summary>
    /// Handler Loaded untuk memasang update layout board dan posisi drop zone.
    /// </summary>
    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        if (BoardItems is not null)
            BoardItems.LayoutUpdated += BoardItems_LayoutUpdated;

        UpdateDropZones();
    }

    /// <summary>
    /// Handler update layout board untuk menghitung ulang drop zone.
    /// </summary>
    private void BoardItems_LayoutUpdated(object? sender, EventArgs e)
    {
        UpdateDropZones();
    }

    /// <summary>
    /// Mengatur posisi dan ukuran drop zone berdasarkan layout board.
    /// </summary>
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

    /// <summary>
    /// Mengembalikan arah berlawanan dari arah jalur.
    /// </summary>
    private static OShapePanel.PathDirection Opposite(OShapePanel.PathDirection direction)
        => direction switch
        {
            OShapePanel.PathDirection.Right => OShapePanel.PathDirection.Left,
            OShapePanel.PathDirection.Left => OShapePanel.PathDirection.Right,
            OShapePanel.PathDirection.Up => OShapePanel.PathDirection.Down,
            _ => OShapePanel.PathDirection.Up
        };

    /// <summary>
    /// Menghitung titik offset dari sebuah rect berdasarkan arah dan jarak.
    /// </summary>
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

    /// <summary>
    /// Menyesuaikan ukuran drop zone sesuai orientasi tile di board.
    /// </summary>
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

    /// <summary>
    /// Menempatkan drop zone agar tetap berada dalam batas board.
    /// </summary>
    private void SetClampedPosition(FrameworkElement element, Point pos)
    {
        double x = Math.Max(0, Math.Min(pos.X, BoardArea!.ActualWidth - element.ActualWidth));
        double y = Math.Max(0, Math.Min(pos.Y, BoardArea!.ActualHeight - element.ActualHeight));
        Canvas.SetLeft(element, x);
        Canvas.SetTop(element, y);
    }

    /// <summary>
    /// Mengambil bounding rect elemen relatif ke board area.
    /// </summary>
    private Rect GetElementRect(FrameworkElement element)
    {
        Point pos = element.TranslatePoint(new Point(0, 0), BoardArea);
        return new Rect(pos, new Size(element.ActualWidth, element.ActualHeight));
    }

    /// <summary>
    /// Memulai drag-drop saat mouse bergerak melewati threshold.
    /// </summary>
    private void HandItems_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
            return;

        Point position = e.GetPosition(this);
        if (Math.Abs(position.X - _dragStartPoint.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(position.Y - _dragStartPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;

        if (sender is not ItemsControl itemsControl)
            return;

        var presenter = _dragCandidatePresenter ?? GetContainerFromEvent(itemsControl, e.OriginalSource as DependencyObject);
        if (presenter is null)
            return;

        if (presenter.DataContext is not DominoTileViewModel tile)
            return;

        if (itemsControl.DataContext is not PlayerScoreViewModel playerVm)
            return;

        if (!playerVm.IsCurrent)
            return;

        BeginDragAdorner(presenter);
        try
        {
            var data = new DataObject(typeof(DominoTileViewModel), tile);
            DragDrop.DoDragDrop(presenter, data, DragDropEffects.Move);
        }
        finally
        {
            EndDragAdorner();
            _dragCandidatePresenter = null;
        }
    }

    /// <summary>
    /// Menentukan apakah drop zone bisa menerima domino saat drag-over.
    /// </summary>
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

    /// <summary>
    /// Handler drop di sisi kiri board.
    /// </summary>
    private void DropLeftZone_Drop(object sender, DragEventArgs e)
    {
        if (!TryGetTileFromDrag(e, out var tile) || _viewModel is null)
            return;

        _viewModel.TryPlaceDominoFromDrag(tile, BoardSide.Left);
    }

    /// <summary>
    /// Handler drop di sisi kanan board.
    /// </summary>
    private void DropRightZone_Drop(object sender, DragEventArgs e)
    {
        if (!TryGetTileFromDrag(e, out var tile) || _viewModel is null)
            return;

        _viewModel.TryPlaceDominoFromDrag(tile, BoardSide.Right);
    }

    /// <summary>
    /// Mengambil tile domino dari data drag-drop.
    /// </summary>
    private static bool TryGetTileFromDrag(DragEventArgs e, out DominoTileViewModel tile)
    {
        tile = null!;
        if (!e.Data.GetDataPresent(typeof(DominoTileViewModel)))
            return false;

        tile = (DominoTileViewModel)e.Data.GetData(typeof(DominoTileViewModel))!;
        return tile is not null;
    }

    /// <summary>
    /// Memperbarui posisi adorner dan efek drag saat drag-over di view.
    /// </summary>
    private void GameView_PreviewDragOver(object sender, DragEventArgs e)
    {
        if (_dragAdorner is not null)
            _dragAdorner.UpdatePosition(e.GetPosition(this));

        if (!e.Data.GetDataPresent(typeof(DominoTileViewModel)))
            return;

        e.Effects = DragDropEffects.Move;
    }

    /// <summary>
    /// Mencari parent visual terdekat dengan tipe tertentu.
    /// </summary>
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

    /// <summary>
    /// Menyiapkan adorner drag untuk preview tile.
    /// </summary>
    private void BeginDragAdorner(ContentPresenter presenter)
    {
        presenter.UpdateLayout();
        var size = GetPresenterSize(presenter);
        int width = (int)Math.Ceiling(size.Width);
        int height = (int)Math.Ceiling(size.Height);

        _dragAdornerLayer = AdornerLayer.GetAdornerLayer(this);
        if (_dragAdornerLayer is null)
            return;

        _draggedPresenter = presenter;
        _dragAdorner = new DragAdorner(this, presenter, new Size(width, height));
        _dragAdornerLayer.Add(_dragAdorner);
    }

    /// <summary>
    /// Membersihkan adorner drag setelah drag selesai.
    /// </summary>
    private void EndDragAdorner()
    {
        if (_dragAdornerLayer is not null && _dragAdorner is not null)
            _dragAdornerLayer.Remove(_dragAdorner);

        _dragAdorner = null;
        _dragAdornerLayer = null;
        _draggedPresenter = null;
    }

    /// <summary>
    /// Mengambil container ContentPresenter dari event source.
    /// </summary>
    private static ContentPresenter? GetContainerFromEvent(ItemsControl itemsControl, DependencyObject? source)
    {
        if (source is null)
            return null;

        return ItemsControl.ContainerFromElement(itemsControl, source) as ContentPresenter;
    }

    /// <summary>
    /// Mengambil ukuran presenter yang valid, fallback ke ukuran default bila perlu.
    /// </summary>
    private static Size GetPresenterSize(FrameworkElement presenter)
    {
        var size = presenter.RenderSize;
        if (size.Width <= 0 || size.Height <= 0)
        {
            presenter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            presenter.Arrange(new Rect(presenter.DesiredSize));
            size = presenter.DesiredSize;
        }

        if (size.Width <= 0 || size.Height <= 0)
            size = new Size(DefaultTileWidth, DefaultTileHeight);

        return size;
    }
}
