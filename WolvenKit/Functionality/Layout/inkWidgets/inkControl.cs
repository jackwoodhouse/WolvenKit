using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WolvenKit.RED4.Types;
using Rect = System.Windows.Rect;

namespace WolvenKit.Functionality.Layout.inkWidgets
{
    public class inkControl : UIElement
    {
        public inkWidget Widget;

        public static readonly DependencyProperty NameProperty =
                    DependencyProperty.Register(
                                nameof(Name),
                                typeof(string),
                                typeof(inkControl),
                                new FrameworkPropertyMetadata(
                                    string.Empty,                           // defaultValue
                                    FrameworkPropertyMetadataOptions.None,  // flags
                                    null,                                   // propertyChangedCallback
                                    null,                                   // coerceValueCallback
                                    true)                                  // isAnimationProhibited
                                );

        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public virtual double Width => Widget.Size.X;
        public virtual double Height => Widget.Size.Y;

        public Thickness Margin => ToThickness(Widget.Layout.Margin);

        public Color TintColor => ToColor(Widget.TintColor);
        public Brush TintBrush => new SolidColorBrush(TintColor);

        public inkControl(inkWidget widget) : base()
        {
            Widget = widget;
            //Tag = Widget;

            //Background = Brushes.Transparent;

            //Background = ToBrush(Widget.TintColor);

            //ToolTip = Widget.Name + $" ({Widget.GetType().Name})";
            Name = Widget.Name;

            if (Widget.GetParent() is not null)
                Opacity = Widget.Opacity;
            //else
            //    HorizontalAlignment = HorizontalAlignment.Left;

            if (!Widget.Visible)
            {
                if (Widget.AffectsLayoutWhenHidden)
                {
                    Visibility = Visibility.Hidden;
                }
                else
                {
                    Visibility = Visibility.Collapsed;
                }
            }

            RenderTransformOrigin = ToPoint(Widget.RenderTransformPivot);
            RenderTransform = new TransformGroup()
            {
                Children = new TransformCollection(new List<System.Windows.Media.Transform>()
                {
                    new ScaleTransform(Widget.RenderTransform.Scale.X, Widget.RenderTransform.Scale.Y),
                    new TranslateTransform(Widget.RenderTransform.Translation.X, Widget.RenderTransform.Translation.Y),
                    new SkewTransform(Math.Atan(Widget.RenderTransform.Shear.X) * 180/Math.PI, Math.Atan(Widget.RenderTransform.Shear.Y) * 180/Math.PI),
                    new RotateTransform(Widget.RenderTransform.Rotation)
                })
            };
        }

        //protected override void OnRender(DrawingContext dc)
        //{
        //    base.OnRender(dc);
        //}

        protected override Size MeasureCore(Size availableSize)
        {
            var size = new Size(Width, Height);
            if (!Double.IsPositiveInfinity(availableSize.Width) && FillH(this))
                size.Width = availableSize.Width;
            if (!Double.IsPositiveInfinity(availableSize.Height) && FillV(this))
                size.Height = availableSize.Height;
            return size;
        }

        protected override void ArrangeCore(Rect finalRect)
        {
            finalRect.X = Math.Round(finalRect.X);
            finalRect.Y = Math.Round(finalRect.Y);
            finalRect.Width = Math.Round(finalRect.Width);
            finalRect.Height = Math.Round(finalRect.Height);

            base.ArrangeCore(finalRect);
        }

        public static System.Windows.Point ToPoint(Vector2 v)
        {
            return new System.Windows.Point(v.X, v.Y);
        }

        public static Color ToColor(HDRColor hdr)
        {
            return Color.FromArgb((byte)(hdr.Alpha * 255), (byte)(hdr.Red * 255), (byte)(hdr.Green * 255), (byte)(hdr.Blue * 255));
        }

        public static Brush ToBrush(HDRColor hdr)
        {
            return new SolidColorBrush(ToColor(hdr));
        }

        public static System.Drawing.Color ToDrawingColor(HDRColor hdr, float alpha = 1)
        {
            return System.Drawing.Color.FromArgb((byte)(hdr.Alpha * 255 * alpha), (byte)(hdr.Red * 255), (byte)(hdr.Green * 255), (byte)(hdr.Blue * 255));
        }

        public static System.Drawing.Brush ToDrawingBrush(HDRColor hdr)
        {
            return new System.Drawing.SolidBrush(ToDrawingColor(hdr));
        }

        public static Thickness AddToThickness(inkMargin m, Thickness t)
        {
            return new Thickness(m.Left + t.Left, m.Top + t.Top, m.Right + t.Right, m.Bottom + t.Bottom);
        }

        public static HorizontalAlignment ToHorizontalAlignment(CEnum<Enums.inkEHorizontalAlign> hAlign)
        {
            switch (hAlign.Value)
            {
                case Enums.inkEHorizontalAlign.Fill:
                    return HorizontalAlignment.Stretch;
                case Enums.inkEHorizontalAlign.Center:
                    return HorizontalAlignment.Center;
                case Enums.inkEHorizontalAlign.Left:
                    return HorizontalAlignment.Left;
                case Enums.inkEHorizontalAlign.Right:
                    return HorizontalAlignment.Right;
                default:
                    return HorizontalAlignment.Left;
            }
        }

        public static VerticalAlignment ToVerticalAlignment(CEnum<Enums.inkEVerticalAlign> hAlign)
        {
            switch (hAlign.Value)
            {
                case Enums.inkEVerticalAlign.Fill:
                    return VerticalAlignment.Stretch;
                case Enums.inkEVerticalAlign.Center:
                    return VerticalAlignment.Center;
                case Enums.inkEVerticalAlign.Top:
                    return VerticalAlignment.Top;
                case Enums.inkEVerticalAlign.Bottom:
                    return VerticalAlignment.Bottom;
                default:
                    return VerticalAlignment.Top;
            }
        }

        public static Thickness ToThickness(inkMargin value)
        {
            return new Thickness(value.Left, value.Top, value.Right, value.Bottom);
        }

        public static Thickness ToThickness(RectF value)
        {
            return new Thickness(value.Left, value.Top, value.Right, value.Bottom);
        }

        public static bool FillH(inkControl control)
        {
            switch (control.Widget.GetParent())
            {
                case inkCanvasWidget:
                    return AnchorToFillH(control);
                case inkFlexWidget:
                case inkBasePanelWidget:
                    return HAlignToFill(control);
                default:
                    return false;
            }
        }

        public static bool FillV(inkControl control)
        {
            switch (control.Widget.GetParent())
            {
                case inkCanvasWidget:
                    return AnchorToFillV(control);
                case inkFlexWidget:
                case inkBasePanelWidget:
                    return VAlignToFill(control);
                default:
                    return false;
            }
        }

        public static bool AnchorLeft(inkControl control)
        {
            switch (control.Widget.Layout.Anchor.Value)
            {
                case Enums.inkEAnchor.TopLeft:
                case Enums.inkEAnchor.CenterLeft:
                case Enums.inkEAnchor.BottomLeft:
                case Enums.inkEAnchor.LeftFillVerticaly:
                    return true;
                case Enums.inkEAnchor.CenterFillHorizontaly:
                case Enums.inkEAnchor.BottomFillHorizontaly:
                case Enums.inkEAnchor.TopFillHorizontaly:
                case Enums.inkEAnchor.Fill:
                case Enums.inkEAnchor.TopCenter:
                case Enums.inkEAnchor.Centered:
                case Enums.inkEAnchor.BottomCenter:
                case Enums.inkEAnchor.CenterFillVerticaly:
                case Enums.inkEAnchor.TopRight:
                case Enums.inkEAnchor.CenterRight:
                case Enums.inkEAnchor.BottomRight:
                case Enums.inkEAnchor.RightFillVerticaly:
                default:
                    return false;
            }
        }

        public static bool AnchorRight(inkControl control)
        {
            switch (control.Widget.Layout.Anchor.Value)
            {
                case Enums.inkEAnchor.TopRight:
                case Enums.inkEAnchor.CenterRight:
                case Enums.inkEAnchor.BottomRight:
                case Enums.inkEAnchor.RightFillVerticaly:
                    return true;
                case Enums.inkEAnchor.TopLeft:
                case Enums.inkEAnchor.CenterLeft:
                case Enums.inkEAnchor.BottomLeft:
                case Enums.inkEAnchor.LeftFillVerticaly:
                case Enums.inkEAnchor.CenterFillHorizontaly:
                case Enums.inkEAnchor.BottomFillHorizontaly:
                case Enums.inkEAnchor.TopFillHorizontaly:
                case Enums.inkEAnchor.Fill:
                case Enums.inkEAnchor.TopCenter:
                case Enums.inkEAnchor.Centered:
                case Enums.inkEAnchor.BottomCenter:
                case Enums.inkEAnchor.CenterFillVerticaly:
                default:
                    return false;
            }
        }

        public static double AnchorToX(inkControl control)
        {
            switch (control.Widget.Layout.Anchor.Value)
            {
                case Enums.inkEAnchor.TopLeft:
                case Enums.inkEAnchor.CenterLeft:
                case Enums.inkEAnchor.BottomLeft:
                case Enums.inkEAnchor.LeftFillVerticaly:
                case Enums.inkEAnchor.CenterFillHorizontaly:
                case Enums.inkEAnchor.BottomFillHorizontaly:
                case Enums.inkEAnchor.TopFillHorizontaly:
                case Enums.inkEAnchor.Fill:
                    return 0;
                case Enums.inkEAnchor.TopCenter:
                case Enums.inkEAnchor.Centered:
                case Enums.inkEAnchor.BottomCenter:
                case Enums.inkEAnchor.CenterFillVerticaly:
                    return 0.5;
                case Enums.inkEAnchor.TopRight:
                case Enums.inkEAnchor.CenterRight:
                case Enums.inkEAnchor.BottomRight:
                case Enums.inkEAnchor.RightFillVerticaly:
                    return 1;
                default:
                    return 0;
            }
        }

        public static bool AnchorTop(inkControl control)
        {
            switch (control.Widget.Layout.Anchor.Value)
            {
                case Enums.inkEAnchor.TopLeft:
                case Enums.inkEAnchor.TopFillHorizontaly:
                case Enums.inkEAnchor.TopCenter:
                case Enums.inkEAnchor.TopRight:
                    return true;
                case Enums.inkEAnchor.LeftFillVerticaly:
                case Enums.inkEAnchor.CenterFillVerticaly:
                case Enums.inkEAnchor.RightFillVerticaly:
                case Enums.inkEAnchor.Fill:
                case Enums.inkEAnchor.CenterLeft:
                case Enums.inkEAnchor.CenterFillHorizontaly:
                case Enums.inkEAnchor.Centered:
                case Enums.inkEAnchor.CenterRight:
                case Enums.inkEAnchor.BottomLeft:
                case Enums.inkEAnchor.BottomFillHorizontaly:
                case Enums.inkEAnchor.BottomCenter:
                case Enums.inkEAnchor.BottomRight:
                default:
                    return false;
            }
        }

        public static bool AnchorBottom(inkControl control)
        {
            switch (control.Widget.Layout.Anchor.Value)
            {
                case Enums.inkEAnchor.BottomLeft:
                case Enums.inkEAnchor.BottomFillHorizontaly:
                case Enums.inkEAnchor.BottomCenter:
                case Enums.inkEAnchor.BottomRight:
                    return true;
                case Enums.inkEAnchor.TopLeft:
                case Enums.inkEAnchor.TopFillHorizontaly:
                case Enums.inkEAnchor.TopCenter:
                case Enums.inkEAnchor.TopRight:
                case Enums.inkEAnchor.LeftFillVerticaly:
                case Enums.inkEAnchor.CenterFillVerticaly:
                case Enums.inkEAnchor.RightFillVerticaly:
                case Enums.inkEAnchor.Fill:
                case Enums.inkEAnchor.CenterLeft:
                case Enums.inkEAnchor.CenterFillHorizontaly:
                case Enums.inkEAnchor.Centered:
                case Enums.inkEAnchor.CenterRight:
                default:
                    return false;
            }
        }

        public static double AnchorToY(inkControl control)
        {
            switch (control.Widget.Layout.Anchor.Value)
            {
                case Enums.inkEAnchor.TopLeft:
                case Enums.inkEAnchor.TopFillHorizontaly:
                case Enums.inkEAnchor.TopCenter:
                case Enums.inkEAnchor.TopRight:
                case Enums.inkEAnchor.LeftFillVerticaly:
                case Enums.inkEAnchor.CenterFillVerticaly:
                case Enums.inkEAnchor.RightFillVerticaly:
                case Enums.inkEAnchor.Fill:
                    return 0;
                case Enums.inkEAnchor.CenterLeft:
                case Enums.inkEAnchor.CenterFillHorizontaly:
                case Enums.inkEAnchor.Centered:
                case Enums.inkEAnchor.CenterRight:
                    return 0.5;
                case Enums.inkEAnchor.BottomLeft:
                case Enums.inkEAnchor.BottomFillHorizontaly:
                case Enums.inkEAnchor.BottomCenter:
                case Enums.inkEAnchor.BottomRight:
                    return 1;
                default:
                    return 0;
            }
        }

        public static bool AnchorToFillH(inkControl control)
        {
            switch (control.Widget.Layout.Anchor.Value)
            {
                case Enums.inkEAnchor.TopFillHorizontaly:
                case Enums.inkEAnchor.CenterFillHorizontaly:
                case Enums.inkEAnchor.BottomFillHorizontaly:
                case Enums.inkEAnchor.Fill:
                    return true;
                case Enums.inkEAnchor.TopLeft:
                case Enums.inkEAnchor.CenterLeft:
                case Enums.inkEAnchor.BottomLeft:
                case Enums.inkEAnchor.LeftFillVerticaly:
                case Enums.inkEAnchor.TopCenter:
                case Enums.inkEAnchor.Centered:
                case Enums.inkEAnchor.BottomCenter:
                case Enums.inkEAnchor.CenterFillVerticaly:
                case Enums.inkEAnchor.TopRight:
                case Enums.inkEAnchor.CenterRight:
                case Enums.inkEAnchor.BottomRight:
                case Enums.inkEAnchor.RightFillVerticaly:
                default:
                    return false;
            }
        }

        public static bool AnchorToFillV(inkControl control)
        {
            switch (control.Widget.Layout.Anchor.Value)
            {
                case Enums.inkEAnchor.Fill:
                case Enums.inkEAnchor.LeftFillVerticaly:
                case Enums.inkEAnchor.CenterFillVerticaly:
                case Enums.inkEAnchor.RightFillVerticaly:
                    return true;
                case Enums.inkEAnchor.TopFillHorizontaly:
                case Enums.inkEAnchor.CenterFillHorizontaly:
                case Enums.inkEAnchor.BottomFillHorizontaly:
                case Enums.inkEAnchor.TopLeft:
                case Enums.inkEAnchor.CenterLeft:
                case Enums.inkEAnchor.BottomLeft:
                case Enums.inkEAnchor.TopCenter:
                case Enums.inkEAnchor.Centered:
                case Enums.inkEAnchor.BottomCenter:
                case Enums.inkEAnchor.TopRight:
                case Enums.inkEAnchor.CenterRight:
                case Enums.inkEAnchor.BottomRight:
                default:
                    return false;
            }
        }

        public static double HAlignToX(inkControl control)
        {
            switch (control.Widget.Layout.HAlign.Value)
            {
                case Enums.inkEHorizontalAlign.Left:
                case Enums.inkEHorizontalAlign.Fill:
                    return 0;
                case Enums.inkEHorizontalAlign.Center:
                    return 0.5;
                case Enums.inkEHorizontalAlign.Right:
                    return 1;
                default:
                    return 0;
            }
        }

        public static double VAlignToY(inkControl control)
        {
            switch (control.Widget.Layout.VAlign.Value)
            {
                case Enums.inkEVerticalAlign.Top:
                case Enums.inkEVerticalAlign.Fill:
                    return 0;
                case Enums.inkEVerticalAlign.Center:
                    return 0.5;
                case Enums.inkEVerticalAlign.Bottom:
                    return 1;
                default:
                    return 0;
            }
        }

        public static bool HAlignToFill(inkControl control)
        {
            return control.Widget.Layout.HAlign.Value == Enums.inkEHorizontalAlign.Fill;
        }

        public static bool VAlignToFill(inkControl control)
        {
            return control.Widget.Layout.VAlign.Value == Enums.inkEVerticalAlign.Fill;
        }
    }
}
