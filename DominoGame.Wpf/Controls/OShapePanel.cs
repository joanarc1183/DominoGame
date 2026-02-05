using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DominoGame.Wpf.Controls;

public class OShapePanel : Panel
{
    public double Padding { get; set; } = 8;
    public double ItemSpacing { get; set; } = 4;

    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (UIElement child in InternalChildren)
            child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        int count = InternalChildren.Count;
        if (count == 0)
            return finalSize;

        double maxW = 0;
        double maxH = 0;
        foreach (UIElement child in InternalChildren)
        {
            maxW = Math.Max(maxW, child.DesiredSize.Width);
            maxH = Math.Max(maxH, child.DesiredSize.Height);
        }

        double left = Padding;
        double top = Padding;
        double right = Math.Max(left, finalSize.Width - Padding);
        double bottom = Math.Max(top, finalSize.Height - Padding);

        double vertW = maxW;
        double vertH = maxH;
        double horizW = maxH;
        double horizH = maxW;

        double topLen = Math.Max(0, right - left - horizW);
        double rightLen = Math.Max(0, bottom - top - vertH);

        if (topLen <= 0 && rightLen <= 0)
        {
            foreach (UIElement child in InternalChildren)
            {
                double cx = (finalSize.Width - maxW) / 2;
                double cy = (finalSize.Height - maxH) / 2;
                child.Arrange(new Rect(new Point(cx, cy), new Size(maxW, maxH)));
            }
            return finalSize;
        }

        double minX = left + vertW + ItemSpacing;
        double maxX = right - vertW - ItemSpacing - horizW;
        double minY = top + horizH + ItemSpacing;
        double maxY = bottom - horizH - ItemSpacing - vertH;

        if (maxX < minX || maxY < minY)
        {
            foreach (UIElement child in InternalChildren)
            {
                double cx = (finalSize.Width - maxW) / 2;
                double cy = (finalSize.Height - maxH) / 2;
                child.Arrange(new Rect(new Point(cx, cy), new Size(maxW, maxH)));
            }
            return finalSize;
        }

        int topCap = Math.Max(1, (int)Math.Floor((maxX - minX) / (horizW + ItemSpacing)) + 1);
        int rightCap = Math.Max(1, (int)Math.Floor((maxY - minY) / (vertH + ItemSpacing)) + 1);
        int bottomCap = topCap;
        int leftCap = rightCap;

        int index = 0;

        // Top row: left -> right (horizontal)
        for (int i = 0; i < topCap && index < count; i++, index++)
        {
            double x = minX + i * (horizW + ItemSpacing);
            double y = top;
            ArrangeChild(InternalChildren[index], new Point(x, y), new Size(horizW, horizH), rotateHorizontal: true, flip: false);
        }

        // Right column: top -> bottom (vertical)
        for (int i = 0; i < rightCap && index < count; i++, index++)
        {
            double x = right - vertW;
            double y = minY + i * (vertH + ItemSpacing);
            ArrangeChild(InternalChildren[index], new Point(x, y), new Size(vertW, vertH), rotateHorizontal: false, flip: false);
        }

        // Bottom row: right -> left (horizontal)
        for (int i = 0; i < bottomCap && index < count; i++, index++)
        {
            double x = maxX - i * (horizW + ItemSpacing);
            double y = bottom - horizH;
            ArrangeChild(InternalChildren[index], new Point(x, y), new Size(horizW, horizH), rotateHorizontal: true, flip: true);
        }

        // Left column: bottom -> top (vertical)
        for (int i = 0; i < leftCap && index < count; i++, index++)
        {
            double x = left;
            double y = maxY - i * (vertH + ItemSpacing);
            ArrangeChild(InternalChildren[index], new Point(x, y), new Size(vertW, vertH), rotateHorizontal: false, flip: true);
        }

        return finalSize;
    }

    private static void ArrangeChild(UIElement child, Point position, Size size, bool rotateHorizontal, bool flip)
    {
        if (child is FrameworkElement element)
        {
            double angle = rotateHorizontal ? -90 : 0;
            if (flip)
                angle += 180;
            element.LayoutTransform = angle == 0 ? Transform.Identity : new RotateTransform(angle);
        }

        child.Arrange(new Rect(position, size));
    }

    private enum Direction
    {
        Right,
        Down,
        Left,
        Up
    }
}
