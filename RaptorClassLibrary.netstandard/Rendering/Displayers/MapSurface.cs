using System;
using System.Drawing;
using System.Linq;
using VSS.VisionLink.Raptor.Common;
using VSS.TRex.Rendering.Abstractions;
using VSS.VisionLink.Raptor.DI;

namespace VSS.VisionLink.Raptor.Rendering.Displayers
{
    public class MapSurface
    {
        private const double MaxViewDimensionMetres = 20000000;
//        private const double MaxViewDimensionFeet = 60000000;
        private const double MinViewDimensionMetres = 0.001;

        //  private
        public bool SquareAspect = true;
        public double OriginX = Consts.NullDouble;
        public double OriginY = Consts.NullDouble;
        public double LimitX = Consts.NullDouble;
        public double LimitY = Consts.NullDouble;
        public double WidthX = Consts.NullDouble;
        public double WidthY = Consts.NullDouble;
        double centerX = Consts.NullDouble;
        double centerY = Consts.NullDouble;

        public int XAxesDirection = 1;
        public int YAxesDirection = 1;

        //    FRePaintCount : LongWord; // Count of number of RePaints for this map

        //    FDisplayer : TMapDisplayBase;

        int XAxisAdjust;
        int YAxisAdjust;

        //    RePaintEntityCount : Integer;
        //    EntityDisplayCount : Integer;

        // int LastPtX = 0; // Updated by move_to, line_to, move_by etc 
        // int LastPtY = 0;
        // double WLastPtX; // World coordinate versions of lastptx/y 
        // double WLastPtY;

        bool Rotating;
        public double Rotation;

        double CosOfRotation = 1.0;
        double SinOfRotation; //= 0.0;

        public double XPixelSize = Consts.NullDouble;
        public double YPixelSize = Consts.NullDouble;

        //    FOnScaleChanged : TOnScaleChangedEvent;

        //    FInBulkUpdate : Boolean;
        //    FSavedInBulkUpdate : Boolean;
        //    FInForegroundUpdate : Boolean;

        //    FLastUpdateTime : TDateTime;

        // PenMode : TPenMode;

        //    FTextRotation : Word; { Angle in tenths of degrees }

        //    FPrintScaleFactor : Float;

        //        int DotSymbolSize = 0; // Number of pixels square a 'dot' should be.
        //        int HalfDotSymbolSize1 = 0;
        //        int HalfDotSymbolSize2 = 0;

        //    FForceRepaint : Boolean;

        // If HoldOriginOnResize true, then the origin coordinate will remain at the
        // bottom lefthand corner of the view when the view is resize. If false,
        // the coorindate at the top right hand corner remains the same, and the
        // origin is altered
        public bool HoldOriginOnResize = false;

        int MinPenWidth = 1;

        //    FPrintingDisplay        :Boolean;
        //    FPrintTextReportSection :Boolean;

        Color DrawCanvasPenColor = Color.Black; // Cached pen color for the DrawCanvas
        IPen DrawCanvasPen;
        IBrush DrawCanvasBrush;

        // Polypoints is an array of screen coordinate vertices used for calls to the
        // WIN32 DC API Polygon() and Polyline() calls. It is defined here in order to
        // avoid the overhead of allocating such as array for each polyline/polygon
        // that is drawn
        //FPolyPoints : array of TPoint;

        //FTriangleDrawingData : Array[1..3] of TPoint;

        //    fControlDisplayedDetecter: TControlDisplayedDetecter;

        //    procedure OnObservedControlDisplayedChanged(Sender: TObject);

        //procedure CMColorChanged(var Message: TMessage); message CM_COLORCHANGED;

        //protected
        protected double DQMScaleX = Consts.NullDouble; // Scale used in world to screen transform 
        protected double DQMScaleY = Consts.NullDouble; // Scale used in world to screen transform 
        protected int XOffset;
        protected int YOffset;

        //    FZoomList : Contnrs.TObjectList;

        //    procedure SetParent(AParent: TWinControl); override;

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
            double x1, y1, x2, y2; // Original line coordinates

            x1 = from_x;
            y1 = from_y;
            x2 = to_x;
            y2 = to_y;

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
                    DrawCanvasPen.Color = PenColor;

                    // We used to just draw the line with the given coordinates if part of the line
                    // was visible (ie we didn't care about the coordinates of the clipped ends of the
                    // line. However, a bug in either Windows or the device driver meant that lines using
                    // a pen style other than psSolid did not draw at all if one of their end points was
                    // outside of the view. So, we now just draw the line to the clipped coordinates...

                    DrawCanvas.DrawLine(DrawCanvasPen,
                                        XAxisAdjust + XAxesDirection * (int)Math.Truncate(from_x),
                                        YAxisAdjust - YAxesDirection * (int)Math.Truncate(from_y),
                                        XAxisAdjust + XAxesDirection * (int)Math.Truncate(to_x),
                                        YAxisAdjust - YAxesDirection * (int)Math.Truncate(to_y));
                }
            }
        }

        public void Rotate_point(double fromX, double fromY, out double toX, out double toY)
        {
            toX = centerX + (fromX - centerX) * CosOfRotation - (fromY - centerY) * SinOfRotation;
            toY = centerY + (fromY - centerY) * CosOfRotation + (fromX - centerX) * SinOfRotation;
        }

        public void Rotate_point_about(double fromX, double fromY, out double toX, out double toY, double CX, double CY)
        {
            toX = CX + (fromX - CX) * CosOfRotation - (fromY - CY) * SinOfRotation;
            toY = CY + (fromY - CY) * CosOfRotation + (fromX - CX) * SinOfRotation;
        }

        public void Rotate_point_no_origin(double fromX, double fromY, out double toX, out double toY)
        {
            double mid_E = WidthX / 2;
            double mid_N = WidthY / 2;

            toX = mid_E + (fromX - mid_E) * CosOfRotation - (fromY - mid_N) * SinOfRotation;
            toY = mid_N + (fromY - mid_N) * CosOfRotation + (fromX - mid_E) * SinOfRotation;
        }

        //    procedure rotate_rectangle(const x1, y1, x2, y2 : FLOAT;
        //                               VAR rx1, ry1, rx2, ry2, rx3, ry3, rx4, ry4 : FLOAT;
        //                               VAR brx1, bry1, brx2, bry2 : FLOAT);

        //    Function GetPixelSize : Float; // If you don't care about aspect...

        public void SetPenWidth(int width)
        {
            if (width < MinPenWidth)
            {
                width = MinPenWidth;
            }

            if (DrawCanvasPen.Width != width)
            {
                DrawCanvasPen.Width = width;
            }
        }

        public int GetPenWidth() => (int)DrawCanvasPen.Width;

        //    procedure Loaded; override;

        //    procedure Paint {(Sender : TObject)}; override;// Not override on purpose

        //  public
        //DisplaySurface : TPaintBox;
        //DrawCanvas : TCanvas; { The canvas being displayed on, screen or printer }

        public IBitmap BitmapCanvas;
        public IRenderingFactory RenderingFactory = DIContext.RenderingFactory;
        public IGraphics DrawCanvas;

        public int ClipHeight; // Viewport height
        public int ClipWidth; // Viewport width

        //    { This is the view of the map as seen on the screen }

        //    OffScreenBitmap : TBitmap;
        //    CompositingBitmap : TBitmap;

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

        //    Procedure _moveto(const X, Y : Integer);
        //Procedure _lineto(const X, Y : Integer);
        //procedure _arc(const a, b, c, d, e, f, g, h : Integer);
        //Procedure _ellipse(const TLX, TLY, BRX, BRY : Integer);
        // procedure _textout(const x, y : Integer; const s : string);
        //    function _TextWidth(const S :String) :Integer;

        public void _rectangle(int x1, int y1, int x2, int y2)
        {
            DrawCanvas.DrawRectangle(DrawCanvasPen, new Rectangle(x1, x2, x2 - x1, y2 - y1));
        }

        //procedure Repaint; override;

        //    property PrintScaleFactor: Float read FPrintScaleFactor;

        public double CenterX => centerX;
        public double CenterY => centerY;

        //    Property RePaintCount : LongWord read FRePaintCount;
        //    Property Displayer : TMapDisplayBase read FDisplayer write FDisplayer;

        double PixelSize => XPixelSize;
        //    property XPixelSize : FLOAT read FXPixelSize;
        //    property YPixelSize : FLOAT read FYPixelSize;

        //    property OnScaleChanged : TOnScaleChangedEvent read FOnScaleChanged write FOnScaleChanged;

        public int PenWidth { get { return GetPenWidth(); } set { SetPenWidth(value); } }
        //    property PenMode : TPenMode read FPenMode;

        //    property InBulkUpdate : Boolean read FInBulkUpdate;
        //    property InForeGroundUpdate : Boolean read FInForeGroundUpdate;

        //    property Color : TColor read GetColor write SetColor;

        //    property PrintingDisplay        :Boolean read FPrintingDisplay;
        //    property PrintTextReportSection :Boolean read FPrintTextReportSection write FPrintTextReportSection;

        public MapSurface()
        {
            ClipWidth = 100;
            ClipHeight = 100;

            WidthX = 1000;
            WidthY = 1000;
            LimitX = 1000;
            LimitY = 1000;
            OriginX = 0;
            OriginY = 0;

            XPixelSize = WidthX / (ClipWidth + 1);
            YPixelSize = WidthY / (ClipHeight + 1);

            //            FTextRotation:= 0;
            //            FPenMode:= pmCopy;

            ScaleBarRHSIndent = 0;

            DrawNonSquareAspectScale = true;
            DrawNonSquareAspectScaleAsVerticalDistanceBar = false;

            DrawCanvasPen = RenderingFactory.CreatePen(Color.Black);
            DrawCanvasBrush = RenderingFactory.CreateBrush(Color.Black);

            BitmapCanvas = RenderingFactory.CreateBitmap(100, 100);
            DrawCanvas = RenderingFactory.CreateGraphics(BitmapCanvas);

            DrawCanvasPenColor = Color.Black;

            Rotation = 0.0;

            CalculateXYOffsets();
        }

        public void CalculateXYOffsets()
        {
            if (DrawCanvas == null)
            {
                return;
            }

            //            DotSymbolSize = Math.Round(GetDeviceCaps(DrawCanvas.Handle, LOGPIXELSX) / DotSymbolsPerInch) + 2;
            //            HalfDotSymbolSize1 = DotSymbolSize / 2;
            //            HalfDotSymbolSize2 = DotSymbolSize - DotSymbolSize / 2;

            ClipWidth = BitmapCanvas.Width - 1;
            ClipHeight = BitmapCanvas.Height - 1;

            XOffset = 0;
            YOffset = 0;
            XAxisAdjust = XAxesDirection == 1 ? 0 : ClipWidth;
            YAxisAdjust = YAxesDirection == 1 ? ClipHeight : 0;
        }

        //        procedure DisplaySurfacePaint(Sender : TObject);

        //Procedure ForcePaint;

        //procedure BeginBulkUpdate;
        //procedure EndBulkUpdate(display_in_progress : boolean);

        //procedure BeginForegroundUpdate;
        //procedure EndForegroundUpdate;

        //Procedure ShiftDrawing(const x, y : Integer);
        public void Clear()
        {
            DrawCanvas.Clear(DrawCanvasPenColor);
        }

        //procedure Move(const NewLeft, NewTop : Integer);
        //procedure ChangeSize(const NewWidth, NewHeight : Integer);

        //Procedure RecordZoom;
        //Procedure PerformMooz;
        //Procedure ClearZoomlist;
        //Function ViewIsZoomed : Boolean;

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

        public void SetRotation(double rotation)
        {
            Rotation = rotation * Math.PI / 180;

            SinOfRotation = Math.Sin(rotation);
            CosOfRotation = Math.Cos(rotation);

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

        //function GetOriginX : FLOAT;
        //    function GetOriginY : FLOAT;

        //    function GetLimitX : FLOAT;
        //    function GetLimitY : FLOAT;

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

            Raptor.Utilities.MinMax.SetMinMax(ref MinX, ref MaxX);
            Raptor.Utilities.MinMax.SetMinMax(ref MinY, ref MaxY);

            // We restrict the maximum zoom extent (ie width/height across view) to +-20,000,000 metres
            if ((MaxX - MinX) > MaxViewDimensionMetres)
            {
                MinX = ((MaxX + MinX) / 2) - (MaxViewDimensionMetres / 2);
                MaxX = MinX + MaxViewDimensionMetres;
            }
            if ((MaxY - MinY) > MaxViewDimensionMetres)
            {
                MinY = ((MaxY + MinY) / 2) - (MaxViewDimensionMetres / 2);
                MaxY = MinY + MaxViewDimensionMetres;
            }

            // We restrict the minimum zoom extent (ie width/height across view) to +0.001 metres
            if ((MaxX - MinX) < MinViewDimensionMetres)
            {
                MinX = ((MaxX + MinX) / 2) - (MinViewDimensionMetres / 2);
                MaxX = MinX + MinViewDimensionMetres;
            }
            if ((MaxY - MinY) < MinViewDimensionMetres)
            {
                MinY = ((MaxY + MinY) / 2) - (MinViewDimensionMetres / 2);
                MaxY = MinY + MinViewDimensionMetres;
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

            Raptor.Utilities.MinMax.SetMinMax(ref MinX, ref MaxX);
            Raptor.Utilities.MinMax.SetMinMax(ref MinY, ref MaxY);

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


        public void SetPenColor(Color PenColor) => DrawCanvasPen.Color = PenColor;
        public void SetBrushColor(Color BrushColor) => DrawCanvasBrush.Color = BrushColor;
        //procedure SetBrushStyle(const BrushStyle : TBrushStyle);
        //procedure SetFontColor(FontColor : TColor);

        //procedure move_to(x, y : FLOAT);
        //procedure move_by(x, y : FLOAT);
        //procedure line_to(x, y : FLOAT);
        //procedure draw_by(x, y : FLOAT);

        //Function clip_line_to_view(VAR from_x, from_y, to_x, to_y : FLOAT) : boolean;
        //    function Clip_Line(var from_X, From_Y, To_X, To_Y : FLOAT ) : boolean;

        //    procedure DrawRotatedBox(x1, y1, x2, y2, x3, y3, x4, y4 : FLOAT; PenColor : TColor);
        //    procedure DrawRotatedBoxNoClip(x1, y1, x2, y2, x3, y3, x4, y4 : FLOAT; PenColor : TColor);

        //    procedure DrawArc(Sx, Sy, Ex, Ey, Cx, Cy : FLOAT;
        //Angle : Float;
        //                      PenColor : TColor);

        //    procedure DrawCircle(const Cx, Cy : FLOAT;
        //const Radius : Float;
        //                         const PenColor : TColor;
        //                         const Fill : Boolean);

        // DrawCircleWithSquareAspect draws a circle that is drawn as a proper circle about
        // the center point regardless of the aspect ratio. The radial distance will be correct
        // on the axis indicated as the reference axis
        //    procedure DrawCircleWithSquareAspect(const Cx, Cy : FLOAT;
        //const Radius : Float;
        //                                         const PenColor : TColor;
        //                                         const Fill : Boolean;
        //                                         const ReferenceAxis : TMapRelativeReferenceAxis);

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

        //    procedure DrawArrowHead(x1, y1, x2, y2, HeadSize : FLOAT; PenColor : TColor);
        //    procedure DrawArrow(x1, y1, x2, y2, HeadSize : FLOAT; PenColor : TColor);
        //        procedure DrawLineEx(x1, y1, x2, y2 : FLOAT; PenColor : TColor; First : Boolean);
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

            DrawCanvasPen.Color = PenColor;
            DrawCanvas.DrawLine(DrawCanvasPen, px1, py1, px2, py2);
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
            DrawCanvasPen.Color = PenColor;
            DrawCanvas.DrawLine(DrawCanvasPen,
                                XAxisAdjust + XAxesDirection * px1, YAxisAdjust - YAxesDirection * py1,
                                XAxisAdjust + XAxesDirection * px2, YAxisAdjust - YAxesDirection * py2);
        }

        //    procedure DrawPoint(x, y : FLOAT; PenColor : TColor);
        //    procedure DrawPointNoClip(x, y : FLOAT; PenColor : TColor);
        //    procedure DrawPointNoOrigin(x, y : FLOAT; PenColor : TColor);
        //    procedure DrawPointNoOriginNoClip(x, y : FLOAT; PenColor : TColor);

        private void FinaliseViewPortCoords(bool IncludeRightAndBottomBoundary, ref int px1, ref int py1, ref int px2, ref int py2)
        {
            if (IncludeRightAndBottomBoundary)
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

        public void DrawNonRotatedRect(double x, double y, double w, double h, bool Fill, Color PenColor)
        {
            //int px1, py1, px2, py2;

            try
            {
                int px1 = (int)Math.Truncate((x - OriginX) * DQMScaleX);
                int py1 = (int)Math.Truncate((y - OriginY) * DQMScaleY);
                int px2 = (int)Math.Truncate((x - OriginX + w) * DQMScaleX);
                int py2 = (int)Math.Truncate((y - OriginY + h) * DQMScaleY);

                SetPenColor(PenColor);

                if (Fill)
                {
                    FinaliseViewPortCoords(true, ref px1, ref py1, ref px2, ref py2);
                    SetBrushColor(PenColor);
                    DrawCanvasPen.Brush = DrawCanvasBrush;
                    DrawCanvas.FillRectangle(DrawCanvasBrush, px1, py1, px2 - px1 + 1, Math.Abs(py2 - py1) + 1);
                }
                else
                {
                    FinaliseViewPortCoords(true, ref px1, ref py1, ref px2, ref py2);
                    SetBrushColor(Color.Empty);
                    DrawCanvasPen.Brush = DrawCanvasBrush;
                    DrawCanvas.DrawRectangle(DrawCanvasPen, px1, py1, px2 - px1 + 1, Math.Abs(py2 - py1) + 1);
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
            // const double Epsilon = 0;

            try
            {
                if (!Rotating)
                {
                    DrawNonRotatedRect(x, y, w, h, Fill, PenColor);
                    return;
                }

                //rotate_point(x - Epsilon, y - Epsilon, out rx1, out ry1);
                //rotate_point(x - Epsilon, y + h + Epsilon, out rx2, out ry2);
                //rotate_point(x + w + Epsilon, y + h + Epsilon, out rx3, out ry3);
                //rotate_point(x + w + Epsilon, y - Epsilon, out rx4, out ry4);

                Rotate_point(x, y, out double rx1, out double ry1);
                Rotate_point(x, y + h, out double rx2, out double ry2);
                Rotate_point(x + w, y + h, out double rx3, out double ry3);
                Rotate_point(x + w, y, out double rx4, out double ry4);

                //The coordinates are in world units. We must first transform them to pixel coordinates.
                DrawRectPoints[0].X = XAxisAdjust + XAxesDirection * (int)Math.Truncate((rx1 - OriginX) * DQMScaleX);
                DrawRectPoints[0].Y = YAxisAdjust - YAxesDirection * (int)Math.Truncate((ry1 - OriginY) * DQMScaleY);
                DrawRectPoints[1].X = XAxisAdjust + XAxesDirection * (int)Math.Truncate((rx2 - OriginX) * DQMScaleX);
                DrawRectPoints[1].Y = YAxisAdjust - YAxesDirection * (int)Math.Truncate((ry2 - OriginY) * DQMScaleY);
                DrawRectPoints[2].X = XAxisAdjust + XAxesDirection * (int)Math.Truncate((rx3 - OriginX) * DQMScaleX);
                DrawRectPoints[2].Y = YAxisAdjust - YAxesDirection * (int)Math.Truncate((ry3 - OriginY) * DQMScaleY);
                DrawRectPoints[3].X = XAxisAdjust + XAxesDirection * (int)Math.Truncate((rx4 - OriginX) * DQMScaleX);
                DrawRectPoints[3].Y = YAxisAdjust - YAxesDirection * (int)Math.Truncate((ry4 - OriginY) * DQMScaleY);

                /*
                                Point[] Points = new Point[4]
                                {
                                  new Point(XAxisAdjust + XAxesDirection * (int)Math.Truncate((rx1 - OriginX) * DQMScaleX),
                                            YAxisAdjust - YAxesDirection * (int)Math.Truncate((ry1 - OriginY) * DQMScaleY)),
                                  new Point(XAxisAdjust + XAxesDirection * (int)Math.Truncate((rx2 - OriginX) * DQMScaleX),
                                            YAxisAdjust - YAxesDirection * (int)Math.Truncate((ry2 - OriginY) * DQMScaleY)),
                                  new Point(XAxisAdjust + XAxesDirection * (int)Math.Truncate((rx3 - OriginX) * DQMScaleX),
                                            YAxisAdjust - YAxesDirection * (int)Math.Truncate((ry3 - OriginY) * DQMScaleY)),
                                  new Point(XAxisAdjust + XAxesDirection * (int)Math.Truncate((rx4 - OriginX) * DQMScaleX),
                                            YAxisAdjust - YAxesDirection * (int)Math.Truncate((ry4 - OriginY) * DQMScaleY))
                                };
                */

                if (Fill)
                {
                    DrawCanvasBrush.Color = PenColor;
                    DrawCanvasPen.Color = PenColor;
                    DrawCanvasPen.Brush = DrawCanvasBrush;
                    DrawCanvas.FillPolygon(DrawCanvasBrush, DrawRectPoints);
                }
                else
                {
                    DrawCanvasBrush.Color = Color.Empty;
                    DrawCanvasPen.Color = PenColor;
                    DrawCanvasPen.Brush = DrawCanvasBrush;
                    DrawCanvas.DrawPolygon(DrawCanvasPen, DrawRectPoints);
                }
            }
            catch
            {
                // Ignore it 
            }
        }

        //    procedure DrawTriangle(const x1, y1, x2, y2, x3, y3 : Double;
        //                           const Fill : Boolean;
        //                           const PenColor : TColor);

        //    procedure DrawOctogon(x0, y0, width, height : double;
        //const Fill : Boolean;
        //                          const PenColor : TColor);

        //    procedure DrawPentagon(x0, y0, Radius : double; const Fill : Boolean; const PenColor : TColor);

        //    procedure DrawShadedTriangle(const x1, y1, x2, y2, x3, y3 : Double;
        //PenColor1, PenColor2, PenColor3 : TColor);
        //    procedure DrawPolygon(const Vertices: TWorldPointArray;
        //                          Count : Integer;
        //                          Color: TColor); overload;

        //procedure DrawPolygon(const Vertices: TWorldPointArray;
        //                      Count : Integer;
        //                      Color: TColor;
        //                      FillPattern: TBrushStyle); overload;

        //procedure DrawPolyline(const Vertices : TWorldPointArray;
        //                        Count : Integer;
        //                        PenColor : TColor); overload;

        //procedure DrawPolyline(const Vertices : TWorldPointArray;
        //                       Start, Count : Integer;
        //                       PenColor : TColor); overload;

        // procedure DrawCross(const x, y : FLOAT; const PenColor : TColor);

        //    procedure DrawSelectionTag(x, y : FLOAT;
        //Pencolor : TColor);

        //    procedure DrawDisplayPoint(X, Y : Integer; PenColor : TColor);
        //    {         ----------------
        //    Draw a point where X&Y are in display context coordinates.No checking is
        //    performed, X&Y are assumed to be correct, and have been corrected for
        //    Y axis orientation.
        //    }

        //    procedure DrawDisplayLine(const X1, Y1, X2, Y2 : Integer; const PenColor : TColor);
        /*         ---------------
        Draw a line where the end points are in display context coordinates.
        No checking is performed, coordinates are assumed to be correct, and have
        been corrected for Y axis orientation.
        */

        /*         ----------
    Draw the given bitmap into the given world coordinate rectangle specified
    by X1, Y1 and X2, Y2.
    */
        //    procedure DrawBitmap(X1, Y1, X2, Y2 : Integer;
        //BMP : TBitMap); overload;


        //    procedure DrawBitmap(X1, Y1, X2, Y2 : Float;
        //BMP : TBitMap); overload;
        /*         ----------
        Draw the given bitmap into the given world coordinate rectangle specified
        by X1, Y1 and X2, Y2.
        */

        /*
             procedure DrawGrid(MajorGridIntervalX, MinorGridIntervalX : FLOAT;
        MajorGridIntervalY, MinorGridIntervalY : FLOAT;
                               GridRotation : FLOAT; {Degrees}
                               MajorColor, MinorColor : TColor;
                               PenStyle : TPenStyle;
                               LabelGrid : Boolean;
                               GridLabelFormatX : GridLabelFormater;
                               GridLabelFormatY : GridLabelFormater;
                               ToMetresXAxisFactor : Float;
                               ToMetresYAxisFactor : Float;
                               UseIntervalToSetDPs : Boolean);

            Procedure DrawText(const T : String;
        const x, y : FLOAT;
                               const Font : TFont;
                               const Size : FLOAT; { Metres in world }
                               const Rotation : FLOAT; {Radians}
                               const PenColor : TColor);

            Procedure DrawGridLabelText(const T : String;
        IsYAxis : Boolean;
                                        GridValue : FLOAT;
                                        PenColor : TColor);

            Function TextWidth(const T : String;
        const Font : TFont) : Integer;
            // Return number of screen pixels wide text will be in given font

            Function TextHeight(const T : String;
        const Font : TFont) : Integer;
            // Return number of screen pixels high text will be in given font

            Procedure DrawStandard8BitText(const T : String;
        const X, Y : FLOAT;
                                           PenColor : TColor);
        */

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

        //    Procedure SetPenDrawMode(Mode : TPenMode);
        //Function GetPenDrawMode : TPenMode;
        //    Procedure ResetPenDrawMode;

        //Procedure SetPenDrawStyle(PenStyle : TPenStyle;
        //GapsAreTransparent : Boolean = false);
        //    Function GetPenDrawStyle : TPenStyle;
        //    Procedure ResetPenDrawStyle;

        //Function GetGreekSize : FLOAT;
        //    Function BitMapXCharSize : FLOAT; { Size in world units of standard 8bit text }
        //    Function BitMapYCharSize : FLOAT; { Size in world units of standard 8bit text }

        //    Function PointInView(const x, y : FLOAT) : Boolean;
        //    Function PointInViewWithTolerance(const x, y, tol : FLOAT) : Boolean;

        // RectangleInView will accept non-normalised rectangles (ie: where x1, y1
        // is not the bottom left corner). If the Normalised parameter is false the
        // Function will normalise the given rectangle.
        //    Function RectangleInView(const x1, y1, x2, y2 : FLOAT;
        //Normalised : Boolean = True) : Boolean;

        //    Procedure SetEntityRePaintCount(NewCount : Integer);

        //Procedure BeginPrinting;
        //Procedure AbortPrinting;
        //Procedure EndPrinting;

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

            BitmapCanvas = RenderingFactory.CreateBitmap(AWidth, AHeight);
            DrawCanvas = RenderingFactory.CreateGraphics(BitmapCanvas);

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

//    Function GreekingNecessary : Boolean;

//    PROCEDURE draw_mouse_tool_box(x0, y0, x1, y1 : float);

// Procedure DoDrawScale;
// Procedure DoDrawWaterMark(const WaterMark :String);
//    Procedure DoDrawTotalArea(const TotalArea :Double);

// Procedure PointToPixel(x, y : float; var PixelX, PixelY : Float );

//    Procedure DoPaint;

//function MillimetersToPixels(MM: Double): Double;

    // SaveToThumbNail saves the bitmap representing the view to the given
    // FileName. The widest dimension of the thumnail bitmap controlled by
    // WidestDimension (the bitmap is scaled to this size).

//    Procedure SaveToThumbNail(FileName : TFileName;
//WidestDimension : Integer);

//    Procedure RecordRepaint;

    public double Scale { get { return GetScale(); } set { SetScale(value); } }
    }
}
