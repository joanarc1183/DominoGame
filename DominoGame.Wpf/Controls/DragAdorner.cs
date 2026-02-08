using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace DominoGame.Wpf.Controls;

public sealed class DragAdorner : Adorner
{
    // Brush yang menggambar visual yang sedang didrag.
    private readonly VisualBrush _brush;
    // Ukuran adorner untuk mengikuti ukuran visual.
    private readonly Size _size;
    // Posisi terkini adorner mengikuti mouse.
    private Point _position;

    /// Membuat adorner untuk menampilkan visual saat drag.
    public DragAdorner(UIElement adornedElement, UIElement visual, Size size)
        : base(adornedElement)
    {
        _size = size;
        _brush = new VisualBrush(visual)
        {
            Opacity = 1.0,
            Stretch = Stretch.None,
            AlignmentX = AlignmentX.Left,
            AlignmentY = AlignmentY.Top
        };
        IsHitTestVisible = false;
    }

    /// Memperbarui posisi adorner dan memicu re-render.
    public void UpdatePosition(Point position)
    {
        _position = position;
        InvalidateVisual();
    }

    /// Menggambar adorner pada posisi yang baru.
    protected override void OnRender(DrawingContext dc)
    {
        var rect = new Rect(
            _position.X - _size.Width / 2,
            _position.Y - _size.Height / 2,
            _size.Width,
            _size.Height);
        dc.DrawRectangle(_brush, null, rect);
    }
}
