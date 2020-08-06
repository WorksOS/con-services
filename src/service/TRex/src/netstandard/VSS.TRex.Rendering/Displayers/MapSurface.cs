using System;
using System.Drawing;
using System.Linq;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace VSS.TRex.Rendering.Displayers
{
    public class MapSurface : IDisposable
    {
        private const double MaxViewDimensionMeters = 20000000;
//        private const double MaxViewDimensionFeet = 60000000;
        private const double MinViewDimensionMeters = 0.001;

        public bool SquareAspect = true;
        public double OriginX = Consts.NullDouble;
        public double OriginY = Consts.NullDouble;
        public double LimitX = Consts.NullDouble;
        public double LimitY = Consts.NullDouble;
        public double WidthX = Consts.NullDouble;
        public double WidthY = Consts.NullDouble;
        private double centerX = Consts.NullDouble;
        private double centerY = Consts.NullDouble;

        public int XAxesDirection = 1;
        public int YAxesDirection = 1;

        private int XAxisAdjust;
        private int YAxisAdjust;

        private bool Rotating;
        private double Rotation; // = 0.0

        private double CosOfRotation = 1.0;
        private double SinOfRotation; //= 0.0;

        private double CosOfUnRotation = 1.0;
        private double SinOfUnRotation; //= 0.0;

        public double XPixelSize = Consts.NullDouble;
        public double YPixelSize = Consts.NullDouble;

        // If HoldOriginOnResize true, then the origin coordinate will remain at the
        // bottom lefthand corner of the view when the view is resize. If false,
        // the coorindate at the top right hand corner remains the same, and the
        // origin is altered
        public bool HoldOriginOnResize = false;

        private int MinPenWidth = 1;

        private Color DrawCanvasPenColor = Color.Black; // Cached pen color for the DrawCanvas

        private SKPaint DrawCanvasPen;
        private SKPaint DrawCanvasBrush;

        protected double DQMScaleX = Consts.NullDouble; // Scale used in world to screen transform 
        protected double DQMScaleY = Consts.NullDouble; // Scale used in world to screen transform 
        protected int XOffset;
        protected int YOffset;

        private bool Clip_edge(ref double out_a, ref double out_b, //Point on edge of window.
                               double bdy_a,      // Boundary value.         
                               double a1, double b1, double a2, double b2,    // Original line coords. Point 1 is outside, point 2 may be inside.  
                               bool end_inside) // Point 2 inside?         
        {
            bool inside = end_inside;
            if (inside)
            {
                out_a = bdy_a;
                out_b = b1 + (bdy_a - a1) * (b2 - b1) / (a2 - a1);
            }

            return inside;
        }

        public void Clip_and_draw_line(double from_x, double from_y, double to_x, double to_y, Color PenColor)
        {
            bool inside;     // Reset if all the line segment is outside the window.
            
            // Original line coordinates
            double x1 = from_x;
            double y1 = from_y;
            double x2 = to_x;
            double y2 = to_y;

            inside = true;                  // Assume some part of the line segment is inside the window.

            if (from_x < 0)
            {
                inside = Clip_edge(ref from_x, ref from_y, 0, x1, y1, x2, y2, to_x >= 0);
            }
            else
            {
                if (to_x < 0)
                {
                    inside = Clip_edge(ref to_x, ref to_y, 0, x2, y2, x1, y1, from_x >= 0);
                }
            }

            if (inside)
            {
                if (from_x > ClipWidth)
                {
                    inside = Clip_edge(ref from_x, ref from_y, ClipWidth, x1, y1, x2, y2, to_x <= ClipWidth);
                }
                else
                {
                    if (to_x > ClipWidth)
                    {
                        inside = Clip_edge(ref to_x, ref to_y, ClipWidth, x2, y2, x1, y1, from_x <= ClipWidth);
                    }
                }
            }

            if (inside)
            {
                if (from_y < 0)
                {
                    inside = Clip_edge(ref from_y, ref from_x, 0, y1, x1, y2, x2, to_y >= 0);
                }
                else
                {
                    if (to_y < 0)
                    {
                        inside = Clip_edge(ref to_y, ref to_x, 0, y2, x2, y1, x1, from_y >= 0);
                    }
                }
            }

            if (inside)
            {
                if (from_y > ClipHeight)
                {
                    inside = Clip_edge(ref from_y, ref from_x, ClipHeight, y1, x1, y2, x2, to_y <= ClipHeight);
                }
                else
                {
                    if (to_y > ClipHeight)
                    {
                        inside = Clip_edge(ref to_y, ref to_x, ClipHeight, y2, x2, y1, x1, from_y <= ClipHeight);
                    }
                }

                if (inside)
                {
                    DrawCanvasPen.Color = new SKColor(PenColor.R, PenColor.G, PenColor.B);
                    var pt1 = new SKPoint(XAxisAdjust + XAxesDirection * (int)Math.Truncate(from_x),
                                          YAxisAdjust - YAxesDirection * (int)Math.Truncate(from_y));
                    var pt2 = new SKPoint(XAxisAdjust + XAxesDirection * (int)Math.Truncate(to_x),
                                          YAxisAdjust - YAxesDirection * (int)Math.Truncate(to_y));
                    DrawCanvas.DrawLine(pt1, pt2, DrawCanvasPen);
                }
            }
        }

        public void Rotate_point(double fromX, double fromY, out double toX, out double toY)
        {
            toX = centerX + (fromX - centerX) * CosOfRotation - (fromY - centerY) * SinOfRotation;
            toY = centerY + (fromX - centerX) * SinOfRotation + (fromY - centerY) * CosOfRotation;
        }

        public void Un_rotate_point(double fromX, double fromY, out double toX, out double toY)
        {
            toX = centerX + (fromX - centerX) * CosOfUnRotation - (fromY - centerY) * SinOfUnRotation;
            toY = centerY + (fromX - centerX) * SinOfUnRotation + (fromY - centerY) * CosOfUnRotation;
        }

        public void Rotate_point_about(double fromX, double fromY, out double toX, out double toY, double CX, double CY)
        {
            toX = CX + (fromX - CX) * CosOfRotation - (fromY - CY) * SinOfRotation;
            toY = CY + (fromX - CX) * SinOfRotation + (fromY - CY) * CosOfRotation;
        }

        public void Rotate_point_no_origin(double fromX, double fromY, out double toX, out double toY)
        {
            var mid_E = WidthX / 2;
            var mid_N = WidthY / 2;

            toX = mid_E + (fromX - mid_E) * CosOfRotation - (fromY - mid_N) * SinOfRotation;
            toY = mid_N + (fromX - mid_E) * SinOfRotation + (fromY - mid_N) * CosOfRotation;
        }

        public void SetPenWidth(int width)
        {
            if (width < MinPenWidth)
            {
                width = MinPenWidth;
            }

            if (DrawCanvasPen.StrokeWidth != width)
            {
               DrawCanvasPen.StrokeWidth = width;
            }
        }

        public int GetPenWidth() => (int)DrawCanvasPen.StrokeWidth;

       // public IRenderingFactory RenderingFactory = DIContext.Obtain<IRenderingFactory>();

        public SKBitmap BitmapCanvas;
        public SKCanvas DrawCanvas;

        public int ClipHeight; // Viewport height
        public int ClipWidth; // Viewport width

        /* All drawing takes place on the offscreen bitmap.This is periodically
          drawn on to the display surface */

        public bool DrawNonSquareAspectScale;
        public bool DrawNonSquareAspectScaleAsVerticalDistanceBar;

        // ScaleBarRHSIndent records how many pixels from the RHS of the view the scale
        // bar should be drawn.
        public int ScaleBarRHSIndent;

        // The following methods with _ prefixes are ghosted versions of the
        // matching canvas methods which are aware of the offscreenbitmap. These
        // should be used instead of the direct canvas method calls to ensure they
        // are drawn


        public double CenterX => centerX;
        public double CenterY => centerY;

        double PixelSize => XPixelSize;

        public int PenWidth { get { return GetPenWidth(); } set { SetPenWidth(value); } }

        public MapSurface()
        {
            ClipWidth = 100;
            ClipHeight = 100;

            WidthX = 1000;
            WidthY = 1000;
            LimitX = 1000;
            LimitY = 1000;
            centerX = 500;
            centerY = 500;
            OriginX = 0;
            OriginY = 0;

            XPixelSize = WidthX / (ClipWidth + 1);
            YPixelSize = WidthY / (ClipHeight + 1);

            // FPenMode:= pmCopy;

            ScaleBarRHSIndent = 0;

            DrawNonSquareAspectScale = true;
            DrawNonSquareAspectScaleAsVerticalDistanceBar = false;

            BitmapCanvas = new SKBitmap();
            DrawCanvas = new SKCanvas(BitmapCanvas);
            DrawCanvasPen = new SKPaint
            {
              Style = SKPaintStyle.Stroke,
              StrokeWidth = 10
            };

            DrawCanvasBrush = new SKPaint
            {
              Style = SKPaintStyle.StrokeAndFill
            };

            DrawCanvasPenColor = Color.Black;

            Rotation = 0.0;

            CalculateXYOffsets();
        }

        public void CalculateXYOffsets()
        {
          if (DrawCanvas != null)
          {
            ClipWidth = BitmapCanvas.Width - 1;
            ClipHeight = BitmapCanvas.Height - 1;

            XOffset = 0;
            YOffset = 0;
            XAxisAdjust = XAxesDirection == 1 ? 0 : ClipWidth;
            YAxisAdjust = YAxesDirection == 1 ? ClipHeight : 0;
          }
        }

        public void Clear()
        {
            DrawCanvas.Clear(new SKColor(DrawCanvasPenColor.R, DrawCanvasPenColor.G, DrawCanvasPenColor.B));
        }

        void SetScale(double scale)
        {
            /*We are given a scale value of the form 1:n where n = scale.
              We need to calculate a scale value that will convert the range of world
              coordinates that will be displayed on the canvas into the canvas pixel
              coordinates. For the time being we will assume that n represents a distance
              in world units that the X - axis is to represent. */

            double MinX = OriginX;
            double MinY = OriginY;
            double MaxX = OriginX + scale;
            double MaxY = OriginY + (WidthY / WidthX) * scale;

            FitBoundsToView(ref MinX, ref MinY, ref MaxX, ref MaxY);
            SetWorldBounds(MinX, MinY, MaxX, MaxY, 0);
        }

        double GetScale() => WidthX;

        /// <summary>
        /// Sets the rotation of the map view with the rotation specified in radians. The rotation sense
        /// is a survey rotation in that 0 is north and it increases clockwise. This is transformed
        /// internally into a mathematical sense rotation where 0 is east and increases counter clockwise.
        /// Note: The action of the rotation is a turned angle, not an orientation, so the magnitude and direction
        /// is all that is needed, so the given survey rotation is simply inverted.
        /// </summary>
        /// <param name="rotation"></param>
        public void SetRotation(double rotation)
        {
            Rotation = -rotation;

            SinOfRotation = Math.Sin(Rotation);
            CosOfRotation = Math.Cos(Rotation);

            SinOfUnRotation = Math.Sin(-Rotation);
            CosOfUnRotation = Math.Cos(-Rotation);

            Rotating = Rotation != 0.0;
        }

        public void SetOrigin(double originX, double originY)
        {
            OriginX = originX;
            OriginY = originY;
            LimitX = OriginX + WidthX;
            LimitY = OriginY + WidthY;
            centerX = OriginX + WidthX / 2;
            centerY = OriginY + WidthY / 2;
        }
        public void GetOrigin(out double originX, out double originY)
        {
            originX = OriginX;
            originY = OriginY;
        }

        public void SetCenter(double centerX, double centerY)
        {
            SetOrigin(centerX - WidthX / 2, centerY - WidthY / 2);
        }

        public void GetCenter(out double centerX, out double centerY)
        {
            centerX = CenterX;
            centerY = CenterY;
        }

        public void FitAndSetWorldBounds(double MinX, double MinY, double MaxX, double MaxY,
                                         double BorderSize)
        {
            FitBoundsToView(ref MinX, ref MinY, ref MaxX, ref MaxY);
            SetWorldBounds(MinX, MinY, MaxX, MaxY, BorderSize);
        }

        public void FitBoundsToView(ref double MinX, ref double MinY, ref double MaxX, ref double MaxY)
        {
            double Aspect = (double)BitmapCanvas.Height / (double)BitmapCanvas.Width;

            MinMax.SetMinMax(ref MinX, ref MaxX);
            MinMax.SetMinMax(ref MinY, ref MaxY);

            // We restrict the maximum zoom extent (ie width/height across view) to +-20,000,000 Meters
            if ((MaxX - MinX) > MaxViewDimensionMeters)
            {
                MinX = ((MaxX + MinX) / 2) - (MaxViewDimensionMeters / 2);
                MaxX = MinX + MaxViewDimensionMeters;
            }
            if ((MaxY - MinY) > MaxViewDimensionMeters)
            {
                MinY = ((MaxY + MinY) / 2) - (MaxViewDimensionMeters / 2);
                MaxY = MinY + MaxViewDimensionMeters;
            }

            // We restrict the minimum zoom extent (ie width/height across view) to +0.001 Meters
            if ((MaxX - MinX) < MinViewDimensionMeters)
            {
                MinX = ((MaxX + MinX) / 2) - (MinViewDimensionMeters / 2);
                MaxX = MinX + MinViewDimensionMeters;
            }
            if ((MaxY - MinY) < MinViewDimensionMeters)
            {
                MinY = ((MaxY + MinY) / 2) - (MinViewDimensionMeters / 2);
                MaxY = MinY + MinViewDimensionMeters;
            }

            if (!SquareAspect) // Do nothing, we will stretch it to the view...
            {
                return;
            }

            double wX = (MaxX - MinX);
            double wY = (MaxY - MinY);

            double cX = (MaxX + MinX) / 2;
            double cY = (MaxY + MinY) / 2;

            try
            {
                if (wY / wX > Aspect) // its skinner & taller..., ie: height is OK, width needs to change
                {
                    MinX = cX - (wY / Aspect) / 2;
                    MaxX = cX + (wY / Aspect) / 2;
                }
                else                   // its fatter & shorter..., ie: width is OK, height needs to change
                {
                    MinY = cY - (wX * Aspect) / 2;
                    MaxY = cY + (wX * Aspect) / 2;
                }
            }
            catch
            {
                // Ooops, we were fed some bad numbers, possibly resulting in a div by zero.
                // Leave the extents alone...
            }
        }

        /*
          This sets the extent of the world bounds in non-rotated coordinates.It
          is not intended for use as a general view extent set function. Use
          SetOrigin, SetScale and SetCenter to achieve this.
        */
        public void SetWorldBounds(double MinX, double MinY, double MaxX, double MaxY,
double BorderSize)
        {
            const double Epsilon = 0.001;

            double AspectRatio = ((double)ClipHeight + 1) / ((double)ClipWidth + 1);

            MinMax.SetMinMax(ref MinX, ref MaxX);
            MinMax.SetMinMax(ref MinY, ref MaxY);

            double BorderWidthX = (MaxX - MinX) * BorderSize;
            double BorderWidthY = (MaxY - MinY) * BorderSize;

            // Calculate the ranges of coordinates, including the border 
            WidthX = (MaxX - MinX) + 2 * BorderWidthX;
            WidthY = (MaxY - MinY) + 2 * BorderWidthY;

            if (WidthX == 0)
                WidthX = 10;

            if (SquareAspect)    // Width is sacrosanct, sacrifice height
            {
                if (Math.Abs((WidthY / WidthX) - AspectRatio) > Epsilon) //Make the displayed view fit the greatest extent 
                {
                    WidthY = AspectRatio * WidthX;
                }
            }
            else
            {
                if (WidthY < Epsilon)   // If the height is practically zero, set
                {
                    WidthY = 0.1 * WidthX; // it to 10% of the width
                }
            }

            // Calculate the middle coords 
            double midX = (MaxX + MinX) / 2;
            double midY = (MaxY + MinY) / 2;

            //Since the world extent of the view window has changed, then we must
            //  change the scale to suit. 

            DQMScaleX = BitmapCanvas.Width / WidthX;
            DQMScaleY = BitmapCanvas.Height / WidthY;

            SetOrigin(midX - WidthX / 2, midY - WidthY / 2);

            XPixelSize = WidthX / BitmapCanvas.Width;
            YPixelSize = WidthY / BitmapCanvas.Height;
        }

        /*
              This returns the bounding box that encloses the possibly rotated display
              view extents in rectangular world coordinates.
            */

        private void Bound_rect_set(double x, double y, ref double min_x, ref double min_y, ref double max_x, ref double max_y)

        {
            if (min_x > x)
                min_x = x;
            if (min_y > y)
                min_y = y;
            if (max_x < x)
                max_x = x;
            if (max_y < y)
                max_y = y;
        }

        public void GetWorldBounds(out double X1, out double Y1, out double X2, out double Y2)
        {
            double min_x, min_y, max_x, max_y;

            GetOrigin(out X1, out Y1);
            X2 = LimitX;
            Y2 = LimitY;

            if (Rotating)
            {
                //  Rotate the corners of the window. 
                min_x = X1;
                min_y = Y1;
                max_x = X2;
                max_y = Y2;

                Rotate_point(X1, Y1, out double rotatedX, out double rotatedY);
                Bound_rect_set(rotatedX, rotatedY, ref min_x, ref min_y, ref max_x, ref max_y);

                Rotate_point(X1, Y2, out rotatedX, out rotatedY);
                Bound_rect_set(rotatedX, rotatedY, ref min_x, ref min_y, ref max_x, ref max_y);

                Rotate_point(X2, Y1, out rotatedX, out rotatedY);
                Bound_rect_set(rotatedX, rotatedY, ref min_x, ref min_y, ref max_x, ref max_y);

                Rotate_point(X2, Y2, out rotatedX, out rotatedY);
                Bound_rect_set(rotatedX, rotatedY, ref min_x, ref min_y, ref max_x, ref max_y);

                X1 = min_x;
                Y1 = min_y;
                X2 = max_x;
                Y2 = max_y;
            }
        }


    public void SetPenColor(Color PenColor)
    {
      DrawCanvasPen.Color = new SKColor(PenColor.R, PenColor.G, PenColor.B);
    }

    public void SetBrushColor(Color BrushColor)
    {
      DrawCanvasBrush.Color = new SKColor(BrushColor.R, BrushColor.G, BrushColor.B);
    }

        public void DrawLine(double x1, double y1, double x2, double y2, Color PenColor)
        {
            // We are given the start and end coordinates of a line to be drawn.The
            // coordinates are in World units. We must first transform them to pixel
            // coordinates before drawing the line */

            double px1, py1, px2, py2;

            if (Rotating)
            {
                Rotate_point(x1, y1, out x1, out y1);
                Rotate_point(x2, y2, out x2, out y2);
            }

            px1 = (x1 - OriginX) * DQMScaleX;
            py1 = (y1 - OriginY) * DQMScaleY;
            px2 = (x2 - OriginX) * DQMScaleX;
            py2 = (y2 - OriginY) * DQMScaleY;

            //This gives us coordinates in pixel space -we must now clip this line
            // to the pixel coordinate view 

            Clip_and_draw_line(px1, py1, px2, py2, PenColor);
        }

        public void DrawLineNoClip(double x1, double y1, double x2, double y2, Color PenColor)
        {
            // We are given the start and end coordinates of a line to be drawn.The
            // coordinates are in World units. We must first transform them to pixel
            //  coordinates before drawing the line *)

            int px1, py1, px2, py2;

            if (Rotating)
            {
                Rotate_point(x1, y1, out double rx1, out double ry1);
                Rotate_point(x2, y2, out double rx2, out double ry2);
                px1 = XAxisAdjust + XAxesDirection * (int)Math.Truncate((rx1 - OriginX) * DQMScaleX);
                py1 = YAxisAdjust - YAxesDirection * (int)Math.Truncate((ry1 - OriginY) * DQMScaleY);
                px2 = XAxisAdjust + XAxesDirection * (int)Math.Truncate((rx2 - OriginX) * DQMScaleX);
                py2 = YAxisAdjust - YAxesDirection * (int)Math.Truncate((ry2 - OriginY) * DQMScaleY);
            }
            else
            {
                px1 = XAxisAdjust + XAxesDirection * (int)Math.Truncate((x1 - OriginX) * DQMScaleX);
                py1 = YAxisAdjust - YAxesDirection * (int)Math.Truncate((y1 - OriginY) * DQMScaleY);
                px2 = XAxisAdjust + XAxesDirection * (int)Math.Truncate((x2 - OriginX) * DQMScaleX);
                py2 = YAxisAdjust - YAxesDirection * (int)Math.Truncate((y2 - OriginY) * DQMScaleY);
            }

            // This gives us coordinates in pixel space - we must draw the line

            DrawCanvasPen.Color = new SKColor(PenColor.R, PenColor.G, PenColor.B);
            DrawCanvas.DrawLine(new SKPoint(px1, py1), new SKPoint(px2, py2), DrawCanvasPen);
    }

        public void DrawLineNoOrigin(double x1, double y1, double x2, double y2, Color PenColor)
        {
            if (Rotating)
            {
                Rotate_point_no_origin(x1, y1, out x1, out y1);
                Rotate_point_no_origin(x2, y2, out x2, out y2);
            }

            Clip_and_draw_line(x1 * DQMScaleX, y1 * DQMScaleY,
                               x2 * DQMScaleX, y2 * DQMScaleY, PenColor);
        }

        public void DrawLineNoOriginNoClip(double x1, double y1, double x2, double y2, Color PenColor)
        {
            // We are given the start and end coordinates of a line to be drawn.The
            // coordinates are in World units with their origin adjusted to the world
            // coordinate origin of the display view.We must first transform them to
            // pixel coordinates before drawing the line 

            int px1, py1, px2, py2;

            if (Rotating)
            {
                Rotate_point_no_origin(x1, y1, out x1, out y1);
                Rotate_point_no_origin(x2, y2, out x2, out y2);
            }

            px1 = (int)Math.Truncate(x1 * DQMScaleX);
            py1 = (int)Math.Truncate(y1 * DQMScaleY);
            px2 = (int)Math.Truncate(x2 * DQMScaleX);
            py2 = (int)Math.Truncate(y2 * DQMScaleY);

            // This gives us coordinates in pixel space -we must draw the line 
            DrawCanvasPen.Color = new SKColor(PenColor.R, PenColor.G, PenColor.B);
            DrawCanvas.DrawLine(new SKPoint(XAxisAdjust + XAxesDirection * px1, YAxisAdjust - YAxesDirection * py1), 
                                  new SKPoint(XAxisAdjust + XAxesDirection * px2, YAxisAdjust - YAxesDirection * py2), DrawCanvasPen);
         }

    /// <summary>
    /// Correct the pixel coordinates from a cartesian coordinate system converted from "bottom left" (0, 0) origin with north east increasing coordinates
    /// to the bitmap pixel coordinates based on a "top left" (0, 0) origin with south east increasing coordinates
    /// </summary>
    private void FinaliseViewPortCoords(bool includeRightAndBottomBoundary, ref int px1, ref int py1, ref int px2, ref int py2)
        {
            if (includeRightAndBottomBoundary)
            {
                px1 = XAxisAdjust + XAxesDirection * px1;
                py1 = YAxisAdjust - YAxesDirection * py1 + 1;
                px2 = XAxisAdjust + XAxesDirection * px2 + 1;
                py2 = YAxisAdjust - YAxesDirection * py2;
            }
            else
            {
                px1 = XAxisAdjust + XAxesDirection * px1;
                py1 = YAxisAdjust - YAxesDirection * py1;
                px2 = XAxisAdjust + XAxesDirection * px2;
                py2 = YAxisAdjust - YAxesDirection * py2;
            }

            if (px1 == px2)
                px2++;
            if (py1 == py2)
                py2++;
        }

        /// <summary>
        /// Draw a filled rectangle given the bottom left and top right coordinates in the world coordinate space
        /// </summary>
        public void DrawNonRotatedRect(double x, double y, double w, double h, bool fill, Color penColor)
        {
            try
            {
                var px1 = (int)Math.Truncate((x - OriginX) * DQMScaleX);
                var py1 = (int)Math.Truncate((y - OriginY) * DQMScaleY);
                var px2 = (int)Math.Truncate((x - OriginX + w) * DQMScaleX);
                var py2 = (int)Math.Truncate((y - OriginY + h) * DQMScaleY);

                if (fill)
                {
                    FinaliseViewPortCoords(true, ref px1, ref py1, ref px2, ref py2);
                    SetBrushColor(penColor);

                    // Use py2 as the 'top' of the filled rectangle to correct for reversal of y coordinates between world and bitmap contexts
                    DrawCanvas.DrawRect(px1, py2, px2 - px1 + 1, Math.Abs(py2 - py1) + 1, DrawCanvasBrush);
                }
                else
                {
                    FinaliseViewPortCoords(true, ref px1, ref py1, ref px2, ref py2);
                    SetPenColor(penColor);

                    // Use py2 as the 'top' of the filled rectangle to correct for reversal of y coordinates between world and bitmap contexts
                    DrawCanvas.DrawRect(px1, py2, px2 - px1 + 1, Math.Abs(py2 - py1) + 1, DrawCanvasPen);
                }
            }
            catch //Most likely math error on transform above 
            {
            }
        }

        /// <summary>
        /// Contains a local store of Point structures to be used by the DrawRect function to remove the overhead 
        /// of 5 memory allocations for each DrawRect invocation
        /// </summary>
        private Point[] DrawRectPoints = Enumerable.Range(0, 4).Select(x => new Point()).ToArray();

        public void DrawRect(double x, double y, double w, double h, bool Fill, Color PenColor)
        {
            try
            {
                if (!Rotating)
                {
                    DrawNonRotatedRect(x, y, w, h, Fill, PenColor);
                    return;
                }

                Rotate_point(x, y, out var rx1, out var ry1);
                Rotate_point(x, y + h, out var rx2, out var ry2);
                Rotate_point(x + w, y + h, out var rx3, out var ry3);
                Rotate_point(x + w, y, out var rx4, out var ry4);

                //The coordinates are in world units. We must first transform them to pixel coordinates.
                DrawRectPoints[0].X = XAxisAdjust + XAxesDirection * (int)Math.Truncate((rx1 - OriginX) * DQMScaleX);
                DrawRectPoints[0].Y = YAxisAdjust - YAxesDirection * (int)Math.Truncate((ry1 - OriginY) * DQMScaleY);
                DrawRectPoints[1].X = XAxisAdjust + XAxesDirection * (int)Math.Truncate((rx2 - OriginX) * DQMScaleX);
                DrawRectPoints[1].Y = YAxisAdjust - YAxesDirection * (int)Math.Truncate((ry2 - OriginY) * DQMScaleY);
                DrawRectPoints[2].X = XAxisAdjust + XAxesDirection * (int)Math.Truncate((rx3 - OriginX) * DQMScaleX);
                DrawRectPoints[2].Y = YAxisAdjust - YAxesDirection * (int)Math.Truncate((ry3 - OriginY) * DQMScaleY);
                DrawRectPoints[3].X = XAxisAdjust + XAxesDirection * (int)Math.Truncate((rx4 - OriginX) * DQMScaleX);
                DrawRectPoints[3].Y = YAxisAdjust - YAxesDirection * (int)Math.Truncate((ry4 - OriginY) * DQMScaleY);

                if (Fill)
                {
                    SetBrushColor(PenColor);
                    DrawCanvas.DrawPoints(SKPointMode.Polygon,
                      new SKPoint[]
                      {
                        new SKPoint(DrawRectPoints[0].X, DrawRectPoints[0].Y),
                        new SKPoint(DrawRectPoints[1].X, DrawRectPoints[1].Y),
                        new SKPoint(DrawRectPoints[2].X, DrawRectPoints[2].Y),
                        new SKPoint(DrawRectPoints[3].X, DrawRectPoints[3].Y)
                      },
                    DrawCanvasPen);
                }
                else
                {
                    SetBrushColor(PenColor);
                    DrawCanvas.DrawPoints(SKPointMode.Polygon,
                      new SKPoint[]
                      {
                        new SKPoint(DrawRectPoints[0].X, DrawRectPoints[0].Y),
                        new SKPoint(DrawRectPoints[1].X, DrawRectPoints[1].Y),
                        new SKPoint(DrawRectPoints[2].X, DrawRectPoints[2].Y),
                        new SKPoint(DrawRectPoints[3].X, DrawRectPoints[3].Y)
                      },
                    DrawCanvasBrush);
                }
            }
            catch
            {
                // Ignore it 
            }
        }

        public double GetTransformScale() => DQMScaleX;

        public void TransformToDisplay(double X, double Y, out int DX, out double DY)
        {
            // This transforms a world coordinate into a screen pixel coordinate with
            //  World Y increasing up the screen.Note that this function per se is not
            //  used internally by the map display surface - those transforms are inlined for
            // performance. 
            try
            {
                DX = XAxisAdjust + XAxesDirection * (int)Math.Truncate((X - OriginX) * DQMScaleX);
                DY = YAxisAdjust - YAxesDirection * (int)Math.Truncate((Y - OriginY) * DQMScaleY);
            }
            catch
            {
                // Most likely a math overflow exception, return -1, -1
                DX = -1;
                DY = -1;
            }
        }

        public void TransformFromDisplay(int DX, int DY, out double X, out double Y)
        {
            // This transforms a screen pixel coordinate into a world coordinate with
            // world Y increasing up the screen 
            X = OriginX + (DX - XAxisAdjust) / DQMScaleX / XAxesDirection;
            Y = OriginY + (YAxisAdjust - DY) / DQMScaleY / YAxesDirection;
        }

        public void SetBounds(int AWidth, int AHeight)
        {
            // Prevent divisions by zero downstream
            if (AWidth == 0)
                AWidth = 1;
            if (AHeight == 0)
                AHeight = 1;

            int PrevDisplayWidth = 1;
            int PrevDisplayHeight = 1;

            if (BitmapCanvas != null)
            {
                PrevDisplayWidth = BitmapCanvas.Width;
                PrevDisplayHeight = BitmapCanvas.Height;
            }

            BitmapCanvas?.Dispose();
            BitmapCanvas = new SKBitmap(AWidth, AHeight, SKColorType.Rgba8888, SKAlphaType.Unpremul);

            DrawCanvas?.Dispose();
            DrawCanvas = new SKCanvas(BitmapCanvas);

            if (BitmapCanvas != null)
            {
                if (PrevDisplayWidth > 0 && PrevDisplayHeight > 0)
                {
                    if (SquareAspect)
                    {
                        if (HoldOriginOnResize)
                        {
                            SetWorldBounds(OriginX, OriginY,
                                           OriginX + WidthX * ((double)AWidth / (double)PrevDisplayWidth),
                                           OriginY + WidthY * ((double)AHeight / (double)PrevDisplayHeight),
                                           0);
                        }
                        else
                        {
                            SetWorldBounds(LimitX - WidthX * ((double)AWidth / (double)PrevDisplayWidth),
                                           LimitY - WidthY * ((double)AHeight / (double)PrevDisplayHeight),
                                           LimitX, LimitY,
                                           0);
                        }
                    }
                    else
                    {
                        SetWorldBounds(OriginX, OriginY,
                                       OriginX + WidthX,
                                       OriginY + WidthY, 0);
                    }                  
                }
            }
            CalculateXYOffsets();
        }

    public double Scale { get => GetScale(); set => SetScale(value); }


    /// <summary>
    /// This will take an array of pixels matching the extent of the canvas and
    /// draw the content of those pixels onto the canvas. This is a destructive process
    /// - any content existing in the canvas will be overwritten
    /// </summary>
    public void DrawFromPixelArray(int width, int height, uint[] pixels)
    {
      if (width != BitmapCanvas.Width || height != BitmapCanvas.Height)
      {
        throw new ArgumentException("Extents of target canvas are not the same as the extents being drawn");
      }

      var gcHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned);

      var info = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Unpremul);
      _ = BitmapCanvas.InstallPixels(info, gcHandle.AddrOfPinnedObject(), info.RowBytes, delegate { gcHandle.Free(); });
    }

    #region IDisposable Support
    private bool disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          DrawCanvasPen?.Dispose();
          DrawCanvasPen = null;
          DrawCanvasBrush?.Dispose();
          DrawCanvasBrush = null;
          BitmapCanvas?.Dispose();
          BitmapCanvas = null;
          DrawCanvas?.Dispose();
          DrawCanvas = null;
        }

        disposedValue = true;
      }
    }

    public void Dispose()
    {
      Dispose(true);
    }
    #endregion
  }
}
