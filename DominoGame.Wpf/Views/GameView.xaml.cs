using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
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
    // Visual clone untuk ghost agar terpisah dari elemen asli.
    private UIElement? _dragVisualClone;
    // Menandakan apakah presenter disembunyikan saat drag.
    private bool _didHideDraggedPresenter;

    /// Inisialisasi GameView dan pasang handler Loaded.
    public GameView()
    {
        InitializeComponent();
        Loaded += HandleLoaded;
    }

    /// Menetapkan view model dan DataContext.
    public void SetViewModel(GameViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    /// Menangkap titik awal drag saat mouse ditekan pada hand.
    private void HandItems_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ItemsControl itemsControl)
            return;

        _dragCandidatePresenter = GetContainerFromEvent(itemsControl, e.OriginalSource as DependencyObject);
        if (_dragCandidatePresenter is null)
            return;

        _dragStartPoint = e.GetPosition(this);
    }

    /// Handler Loaded untuk memasang update layout board dan posisi drop zone.
    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        if (BoardItems is not null)
            BoardItems.LayoutUpdated += BoardItems_LayoutUpdated;

        UpdateDropZones();
    }

    /// Handler update layout board untuk menghitung ulang drop zone.
    private void BoardItems_LayoutUpdated(object? sender, EventArgs e)
    {
        UpdateDropZones();
    }

    /// Mengatur posisi dan ukuran drop zone berdasarkan layout board.
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

    /// Mengembalikan arah berlawanan dari arah jalur.
    private static OShapePanel.PathDirection Opposite(OShapePanel.PathDirection direction)
        => direction switch
        {
            OShapePanel.PathDirection.Right => OShapePanel.PathDirection.Left,
            OShapePanel.PathDirection.Left => OShapePanel.PathDirection.Right,
            OShapePanel.PathDirection.Up => OShapePanel.PathDirection.Down,
            _ => OShapePanel.PathDirection.Up
        };

    /// Menghitung titik offset dari sebuah rect berdasarkan arah dan jarak.
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

    /// Menyesuaikan ukuran drop zone sesuai orientasi tile di board.
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

    /// Menempatkan drop zone agar tetap berada dalam batas board.
    private void SetClampedPosition(FrameworkElement element, Point pos)
    {
        double x = Math.Max(0, Math.Min(pos.X, BoardArea!.ActualWidth - element.ActualWidth));
        double y = Math.Max(0, Math.Min(pos.Y, BoardArea!.ActualHeight - element.ActualHeight));
        Canvas.SetLeft(element, x);
        Canvas.SetTop(element, y);
    }

    /// Mengambil bounding rect elemen relatif ke board area.
    private Rect GetElementRect(FrameworkElement element)
    {
        Point pos = element.TranslatePoint(new Point(0, 0), BoardArea);
        return new Rect(pos, new Size(element.ActualWidth, element.ActualHeight));
    }

    /// Memulai drag-drop saat mouse bergerak melewati threshold.
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

    /// Menentukan apakah drop zone bisa menerima domino saat drag-over.
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

    /// Handler drop di sisi kiri board.
    private void DropLeftZone_Drop(object sender, DragEventArgs e)
    {
        if (!TryGetTileFromDrag(e, out var tile) || _viewModel is null)
            return;

        _viewModel.TryPlaceDominoFromDrag(tile, BoardSide.Left);
    }

    /// Handler drop di sisi kanan board.
    private void DropRightZone_Drop(object sender, DragEventArgs e)
    {
        if (!TryGetTileFromDrag(e, out var tile) || _viewModel is null)
            return;

        _viewModel.TryPlaceDominoFromDrag(tile, BoardSide.Right);
    }

    /// Mengambil tile domino dari data drag-drop.
    private static bool TryGetTileFromDrag(DragEventArgs e, out DominoTileViewModel tile)
    {
        tile = null!;
        if (!e.Data.GetDataPresent(typeof(DominoTileViewModel)))
            return false;

        tile = (DominoTileViewModel)e.Data.GetData(typeof(DominoTileViewModel))!;
        return tile is not null;
    }

    /// Memperbarui posisi adorner dan efek drag saat drag-over di view.
    private void GameView_PreviewDragOver(object sender, DragEventArgs e)
    {
        if (_dragAdorner is not null)
            _dragAdorner.UpdatePosition(e.GetPosition(this));

        if (!e.Data.GetDataPresent(typeof(DominoTileViewModel)))
            return;

        e.Effects = DragDropEffects.Move;
    }

    private void Pause_Click(object sender, RoutedEventArgs e)
    {
        var owner = Window.GetWindow(this);
        var pause = new PauseWindow { Owner = owner };
        bool? resume = pause.ShowDialog();
        if (resume == true)
            return;

        if (_viewModel is null)
            return;

        var players = _viewModel.Players;
        int maxScore = players.Max(p => p.Score);
        var topPlayers = players.Where(p => p.Score == maxScore).ToList();
        string winnerName = topPlayers.Count > 1
            ? "Seri"
            : $"{topPlayers[0].Name} ({topPlayers[0].Score})";

        var gameOver = new GameOverWindow(players, winnerName)
        {
            Owner = owner
        };

        bool? playAgain = gameOver.ShowDialog();
        if (playAgain == true)
        {
            var setup = new SetupWindow { Owner = owner };
            if (setup.ShowDialog() == true)
            {
                var gameVm = new GameViewModel(setup.Players, setup.MaxScoreToWin);
                var window = new MainWindow(gameVm);
                Application.Current.MainWindow = window;
                window.Show();
                owner?.Close();
                return;
            }
        }

        var start = new StartMenuWindow();
        Application.Current.MainWindow = start;
        start.Show();
        owner?.Close();
    }

    /// Mencari parent visual terdekat dengan tipe tertentu.
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

    /// Menyiapkan adorner drag untuk preview tile.
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
        _dragVisualClone = CreateDragVisualClone(presenter, width, height);
        _draggedPresenter.Opacity = 0.0;
        _didHideDraggedPresenter = true;

        _dragAdorner = new DragAdorner(this, _dragVisualClone, new Size(width, height));
        _dragAdornerLayer.Add(_dragAdorner);
    }

    /// Membersihkan adorner drag setelah drag selesai.
    private void EndDragAdorner()
    {
        if (_dragAdornerLayer is not null && _dragAdorner is not null)
            _dragAdornerLayer.Remove(_dragAdorner);

        if (_draggedPresenter is not null && _didHideDraggedPresenter)
            _draggedPresenter.Opacity = 1.0;

        _dragAdorner = null;
        _dragAdornerLayer = null;
        _draggedPresenter = null;
        _dragVisualClone = null;
        _didHideDraggedPresenter = false;
    }

    /// Mengambil container ContentPresenter dari event source.
    private static ContentPresenter? GetContainerFromEvent(ItemsControl itemsControl, DependencyObject? source)
    {
        if (source is null)
            return null;

        return ItemsControl.ContainerFromElement(itemsControl, source) as ContentPresenter;
    }

    /// Mengambil ukuran presenter yang valid, fallback ke ukuran default bila perlu.
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

    /// Membuat visual clone untuk ghost agar tidak terpengaruh opacity elemen asli.
    private UIElement CreateDragVisualClone(ContentPresenter presenter, int width, int height)
    {
        var content = presenter.Content ?? presenter.DataContext;
        var template = presenter.ContentTemplate;

        var handHorizontal = TryFindResource("HandTileTemplateHorizontal") as DataTemplate;
        var handVertical = TryFindResource("HandTileTemplateVertical") as DataTemplate;
        var ghostHorizontal = TryFindResource("HandTileGhostHorizontal") as DataTemplate;
        var ghostVertical = TryFindResource("HandTileGhostVertical") as DataTemplate;

        if (template is not null)
        {
            if (ReferenceEquals(template, handHorizontal) && ghostHorizontal is not null)
                template = ghostHorizontal;
            else if (ReferenceEquals(template, handVertical) && ghostVertical is not null)
                template = ghostVertical;
        }

        var clone = new ContentPresenter
        {
            Content = content,
            ContentTemplate = template,
            ContentTemplateSelector = presenter.ContentTemplateSelector,
            ContentStringFormat = presenter.ContentStringFormat,
            DataContext = content,
            Width = width > 0 ? width : DefaultTileWidth,
            Height = height > 0 ? height : DefaultTileHeight
        };
        clone.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        clone.Arrange(new Rect(0, 0, clone.Width, clone.Height));
        return clone;
    }
}
