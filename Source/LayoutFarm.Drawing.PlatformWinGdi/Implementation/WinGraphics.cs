﻿// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System;
using HtmlRenderer.Drawing;

namespace LayoutFarm.Drawing
{

    /// <summary>
    /// 
    /// </summary>
    public sealed class WinGraphics : LayoutFarm.Drawing.IGraphics
    {

        /// <summary>
        /// used for <see cref="MeasureString(string,System.Drawing.Font,float,out int,out int)"/> calculation.
        /// </summary>
        private static readonly int[] _charFit = new int[1];

        /// <summary>
        /// used for <see cref="MeasureString(string,System.Drawing.Font,float,out int,out int)"/> calculation.
        /// </summary>
        private static readonly int[] _charFitWidth = new int[1000];

        /// <summary>
        /// Used for GDI+ measure string.
        /// </summary>
        private static readonly System.Drawing.CharacterRange[] _characterRanges = new System.Drawing.CharacterRange[1];

        /// <summary>
        /// The string format to use for measuring strings for GDI+ text rendering
        /// </summary>
        private static readonly System.Drawing.StringFormat _stringFormat;

        /// <summary>
        /// The wrapped WinForms graphics object
        /// </summary>
        private readonly System.Drawing.Graphics _g;

        /// <summary>
        /// Use GDI+ text rendering to measure/draw text.
        /// </summary>
        private readonly bool _useGdiPlusTextRendering;

        /// <summary>
        /// the initialized HDC used
        /// </summary>
        private IntPtr _hdc;



        float canvasOriginX = 0;
        float canvasOriginY = 0;

        /// <summary>
        /// Init static resources.
        /// </summary>
        static WinGraphics()
        {
            _stringFormat = new System.Drawing.StringFormat(System.Drawing.StringFormat.GenericDefault);
            _stringFormat.FormatFlags = System.Drawing.StringFormatFlags.NoClip | System.Drawing.StringFormatFlags.MeasureTrailingSpaces;
        }

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="g">the real graphics to use</param>
        /// <param name="useGdiPlusTextRendering">Use GDI+ text rendering to measure/draw text.</param>
        public WinGraphics(System.Drawing.Graphics g, bool useGdiPlusTextRendering)
        {
            _g = g;
            _useGdiPlusTextRendering = useGdiPlusTextRendering;
        }
        public GraphicPlatform Platform
        {
            get { return WinGdiPlatform.WinGdi.Platform; }
        }
        public void SetCanvasOrigin(float x, float y)
        {
            ReleaseHdc();

            this._g.TranslateTransform(-this.canvasOriginX, -this.canvasOriginY);
            this._g.TranslateTransform(x, y);

            this.canvasOriginX = x;
            this.canvasOriginY = y;
        }
        public float CanvasOriginX
        {
            get { return this.canvasOriginX; }

        }
        public float CanvasOriginY
        {
            get { return this.canvasOriginY; }
            set { this.canvasOriginY = value; }
        }
        /// <summary>
        /// Gets the bounding clipping region of this graphics.
        /// </summary>
        /// <returns>The bounding rectangle for the clipping region</returns>
        public LayoutFarm.Drawing.RectangleF GetClip()
        {
            if (_hdc == IntPtr.Zero)
            {
                var clip1 = _g.ClipBounds;
                return new LayoutFarm.Drawing.RectangleF(
                    clip1.X, clip1.Y,
                    clip1.Width, clip1.Height);
            }
            else
            {
                System.Drawing.Rectangle lprc;
                Win32Utils.GetClipBox(_hdc, out lprc);


                return new LayoutFarm.Drawing.RectangleF(
                    lprc.X, lprc.Y,
                    lprc.Width, lprc.Height);
            }
        }

        /// <summary>
        /// Sets the clipping region of this <see cref="T:System.Drawing.Graphics"/> to the result of the specified operation combining the current clip region and the rectangle specified by a <see cref="T:System.Drawing.RectangleF"/> structure.
        /// </summary>
        /// <param name="rect"><see cref="T:System.Drawing.RectangleF"/> structure to combine. </param>
        /// <param name="combineMode">Member of the <see cref="T:System.Drawing.Drawing2D.CombineMode"/> enumeration that specifies the combining operation to use. </param>
        public void SetClip(RectangleF rect, CombineMode combineMode = CombineMode.Replace)
        {
            ReleaseHdc();
            _g.SetClip(rect.ToRectF(), (System.Drawing.Drawing2D.CombineMode)combineMode);
        }

        /// <summary>
        /// Measure the width and height of string <paramref name="str"/> when drawn on device context HDC
        /// using the given font <paramref name="font"/>.
        /// </summary>
        /// <param name="str">the string to measure</param>
        /// <param name="font">the font to measure string with</param>
        /// <returns>the size of the string</returns>
        public Size MeasureString(string str, Font font)
        {
            if (_useGdiPlusTextRendering)
            {
                ReleaseHdc();
                _characterRanges[0] = new System.Drawing.CharacterRange(0, str.Length);
                _stringFormat.SetMeasurableCharacterRanges(_characterRanges);

                var font2 = font.InnerFont as System.Drawing.Font;
                var size = _g.MeasureCharacterRanges(str,
                    font2,
                    System.Drawing.RectangleF.Empty,
                    _stringFormat)[0].GetBounds(_g).Size;

                return new Size((int)Math.Round(size.Width), (int)Math.Round(size.Height));
            }
            else
            {
                SetFont(font);

                var size = new System.Drawing.Size();
                Win32Utils.GetTextExtentPoint32(_hdc, str, str.Length, ref size);
                return size.ToSize();                

            }
        }
        public Size MeasureString2(char[] buff, int startAt, int len, Font font)
        {
            if (_useGdiPlusTextRendering)
            {
                ReleaseHdc();
                _characterRanges[0] = new System.Drawing.CharacterRange(0, len);
                _stringFormat.SetMeasurableCharacterRanges(_characterRanges);
                System.Drawing.Font font2 = (System.Drawing.Font)font.InnerFont;

                var size = _g.MeasureCharacterRanges(
                    new string(buff, startAt, len),
                    font2,
                    System.Drawing.RectangleF.Empty,
                    _stringFormat)[0].GetBounds(_g).Size;
                return new LayoutFarm.Drawing.Size((int)Math.Round(size.Width), (int)Math.Round(size.Height));
            }
            else
            {
                SetFont(font);
                var size = new System.Drawing.Size();
                unsafe
                {
                    fixed (char* startAddr = &buff[0])
                    {
                        Win32Utils.UnsafeGetTextExtentPoint32(_hdc, startAddr + startAt, len, ref size);
                    }
                }
                return size.ToSize();
            }
        }
        /// <summary>
        /// Measure the width and height of string <paramref name="str"/> when drawn on device context HDC
        /// using the given font <paramref name="font"/>.<br/>
        /// Restrict the width of the string and get the number of characters able to fit in the restriction and
        /// the width those characters take.
        /// </summary>
        /// <param name="str">the string to measure</param>
        /// <param name="font">the font to measure string with</param>
        /// <param name="maxWidth">the max width to render the string in</param>
        /// <param name="charFit">the number of characters that will fit under <see cref="maxWidth"/> restriction</param>
        /// <param name="charFitWidth"></param>
        /// <returns>the size of the string</returns>
        public Size MeasureString2(char[] buff, int startAt, int len, Font font, float maxWidth, out int charFit, out int charFitWidth)
        {
            if (_useGdiPlusTextRendering)
            {
                ReleaseHdc();
                throw new NotSupportedException("Char fit string measuring is not supported for GDI+ text rendering");
            }
            else
            {
                SetFont(font);

                var size = new System.Drawing.Size();
                unsafe
                {
                    fixed (char* startAddr = &buff[0])
                    {
                        Win32Utils.UnsafeGetTextExtentExPoint(
                            _hdc, startAddr + startAt, len,
                            (int)Math.Round(maxWidth), _charFit, _charFitWidth, ref size);
                    }

                }
                charFit = _charFit[0];
                charFitWidth = charFit > 0 ? _charFitWidth[charFit - 1] : 0;
                return size.ToSize();
            }
        }
        public Size MeasureString(string str, LayoutFarm.Drawing.Font font,
            float maxWidth, out int charFit, out int charFitWidth)
        {
            if (_useGdiPlusTextRendering)
            {
                ReleaseHdc();
                throw new NotSupportedException("Char fit string measuring is not supported for GDI+ text rendering");
            }
            else
            {
                SetFont(font);

                var size = new System.Drawing.Size();

                Win32Utils.GetTextExtentExPoint(
                    _hdc, str, str.Length,
                    (int)Math.Round(maxWidth), _charFit, _charFitWidth, ref size);
                charFit = _charFit[0];
                charFitWidth = charFit > 0 ? _charFitWidth[charFit - 1] : 0;
                return size.ToSize();
            }
        }
#if DEBUG
        public static class dbugCounter
        {
            public static int dbugDrawStringCount;
        }
#endif
        public void DrawString(char[] str, int startAt, int len, Font font, Color color, PointF point, SizeF size)
        {

#if DEBUG
            dbugCounter.dbugDrawStringCount++;
#endif
            if (_useGdiPlusTextRendering)
            {
                //ReleaseHdc();
                //_g.DrawString(
                //    new string(str, startAt, len),
                //    font,
                //    RenderUtils.GetSolidBrush(color),
                //    (int)Math.Round(point.X + canvasOriginX - FontsUtils.GetFontLeftPadding(font) * .8f),
                //    (int)Math.Round(point.Y + canvasOriginY));

            }
            else
            {
                if (color.A == 255)
                {
                    SetFont(font);
                    SetTextColor(color);
                    unsafe
                    {
                        fixed (char* startAddr = &str[0])
                        {
                            Win32Utils.TextOut2(_hdc, (int)Math.Round(point.X + canvasOriginX),
                                (int)Math.Round(point.Y + canvasOriginY), (startAddr + startAt), len);
                        }
                    }
                }
                else
                {
                    //translucent / transparent text
                    InitHdc();
                    unsafe
                    {
                        fixed (char* startAddr = &str[0])
                        {
                            Win32Utils.TextOut2(_hdc, (int)Math.Round(point.X + canvasOriginX),
                                (int)Math.Round(point.Y + canvasOriginY), (startAddr + startAt), len);
                        }
                    }

                    //DrawTransparentText(_hdc, str, font, new Point((int)Math.Round(point.X), (int)Math.Round(point.Y)), Size.Round(size), color);
                }
            }
        }
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            ReleaseHdc();
        }


        #region Delegate graphics methods

        /// <summary>
        /// Gets or sets the rendering quality for this <see cref="T:System.Drawing.Graphics"/>.
        /// </summary>
        /// <returns>
        /// One of the <see cref="T:System.Drawing.Drawing2D.SmoothingMode"/> values.
        /// </returns>
        /// <PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/></PermissionSet>
        public SmoothingMode SmoothingMode
        {
            get
            {
                ReleaseHdc();
                return (SmoothingMode)(_g.SmoothingMode);
            }
            set
            {
                ReleaseHdc();
                _g.SmoothingMode = (System.Drawing.Drawing2D.SmoothingMode)value;
            }
        }

        /// <summary>
        /// Draws a line connecting the two points specified by the coordinate pairs.
        /// </summary>
        /// <param name="pen"><see cref="T:System.Drawing.Pen"/> that determines the color, width, and style of the line. </param><param name="x1">The x-coordinate of the first point. </param><param name="y1">The y-coordinate of the first point. </param><param name="x2">The x-coordinate of the second point. </param><param name="y2">The y-coordinate of the second point. </param><exception cref="T:System.ArgumentNullException"><paramref name="pen"/> is null.</exception>
        public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
        {
            ReleaseHdc();
            _g.DrawLine(pen.InnerPen as System.Drawing.Pen, x1, y1, x2, y2);
        }

        /// <summary>
        /// Draws a rectangle specified by a coordinate pair, a width, and a height.
        /// </summary>
        /// <param name="pen">A <see cref="T:System.Drawing.Pen"/> that determines the color, width, and style of the rectangle. </param><param name="x">The x-coordinate of the upper-left corner of the rectangle to draw. </param><param name="y">The y-coordinate of the upper-left corner of the rectangle to draw. </param><param name="width">The width of the rectangle to draw. </param><param name="height">The height of the rectangle to draw. </param><exception cref="T:System.ArgumentNullException"><paramref name="pen"/> is null.</exception>
        public void DrawRectangle(Pen pen, float x, float y, float width, float height)
        {
            ReleaseHdc();
            _g.DrawRectangle((System.Drawing.Pen)pen.InnerPen, x, y, width, height);
        }

        public void FillRectangle(Brush getSolidBrush, float left, float top, float width, float height)
        {
            ReleaseHdc();
            _g.FillRectangle((System.Drawing.Brush)getSolidBrush.InnerBrush, left, top, width, height);
        }

        /// <summary>
        /// Draws the specified portion of the specified <see cref="T:System.Drawing.Image"/> at the specified location and with the specified size.
        /// </summary>
        /// <param name="image"><see cref="T:System.Drawing.Image"/> to draw. </param>
        /// <param name="destRect"><see cref="T:System.Drawing.RectangleF"/> structure that specifies the location and size of the drawn image. The image is scaled to fit the rectangle. </param>
        /// <param name="srcRect"><see cref="T:System.Drawing.RectangleF"/> structure that specifies the portion of the <paramref name="image"/> object to draw. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="image"/> is null.</exception>
        public void DrawImage(Image image, RectangleF destRect, RectangleF srcRect)
        {
            ReleaseHdc();
            _g.DrawImage(image.InnerImage as System.Drawing.Image,
                destRect.ToRectF(),
                srcRect.ToRectF(),
                System.Drawing.GraphicsUnit.Pixel);
        }

        /// <summary>
        /// Draws the specified <see cref="T:System.Drawing.Image"/> at the specified location and with the specified size.
        /// </summary>
        /// <param name="image"><see cref="T:System.Drawing.Image"/> to draw. </param><param name="destRect"><see cref="T:System.Drawing.Rectangle"/> structure that specifies the location and size of the drawn image. </param><exception cref="T:System.ArgumentNullException"><paramref name="image"/> is null.</exception><PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/></PermissionSet>
        public void DrawImage(Image image, RectangleF destRect)
        {
            ReleaseHdc();
            _g.DrawImage(image.InnerImage as System.Drawing.Image, destRect.ToRectF());
        }

        /// <summary>
        /// Draws a <see cref="T:System.Drawing.Drawing2D.GraphicsPath"/>.
        /// </summary>
        /// <param name="pen"><see cref="T:System.Drawing.Pen"/> that determines the color, width, and style of the path. </param><param name="path"><see cref="T:System.Drawing.Drawing2D.GraphicsPath"/> to draw. </param><exception cref="T:System.ArgumentNullException"><paramref name="pen"/> is null.-or-<paramref name="path"/> is null.</exception><PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/></PermissionSet>
        public void DrawPath(Pen pen, GraphicsPath path)
        {
            _g.DrawPath(pen.InnerPen as System.Drawing.Pen,
                path.InnerPath as System.Drawing.Drawing2D.GraphicsPath);
        }

        /// <summary>
        /// Fills the interior of a <see cref="T:System.Drawing.Drawing2D.GraphicsPath"/>.
        /// </summary>
        /// <param name="brush"><see cref="T:System.Drawing.Brush"/> that determines the characteristics of the fill. </param><param name="path"><see cref="T:System.Drawing.Drawing2D.GraphicsPath"/> that represents the path to fill. </param><exception cref="T:System.ArgumentNullException"><paramref name="brush"/> is null.-or-<paramref name="path"/> is null.</exception><PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/></PermissionSet>
        public void FillPath(Brush brush, GraphicsPath path)
        {
            ReleaseHdc();
            _g.FillPath(brush.InnerBrush as System.Drawing.Brush,
                path.InnerPath as System.Drawing.Drawing2D.GraphicsPath);
        }

        /// <summary>
        /// Fills the interior of a polygon defined by an array of points specified by <see cref="T:System.Drawing.PointF"/> structures.
        /// </summary>
        /// <param name="brush"><see cref="T:System.Drawing.Brush"/> that determines the characteristics of the fill. </param><param name="points">Array of <see cref="T:System.Drawing.PointF"/> structures that represent the vertices of the polygon to fill. </param><exception cref="T:System.ArgumentNullException"><paramref name="brush"/> is null.-or-<paramref name="points"/> is null.</exception>
        public void FillPolygon(Brush brush, PointF[] points)
        {
            ReleaseHdc();
            //create Point
            System.Drawing.PointF[] pps = new System.Drawing.PointF[points.Length];
            //?
            int j = points.Length;
            for (int i = 0; i < j; ++i)
            {
                pps[i] = points[i].ToPointF();
            }
            _g.FillPolygon(brush.InnerBrush as System.Drawing.Brush, pps);

        }

        #endregion


        #region Private methods

        /// <summary>
        /// Init HDC for the current graphics object to be used to call GDI directly.
        /// </summary>
        private void InitHdc()
        {
            if (_hdc == IntPtr.Zero)
            {
                //var clip = _g.Clip.GetHrgn(_g);
                _hdc = _g.GetHdc();
                Win32Utils.SetBkMode(_hdc, 1);
                //Win32Utils.SelectClipRgn(_hdc, clip);
                //Win32Utils.DeleteObject(clip);
            }
        }

        /// <summary>
        /// Release current HDC to be able to use <see cref="Graphics"/> methods.
        /// </summary>
        private void ReleaseHdc()
        {
            if (_hdc != IntPtr.Zero)
            {
                Win32Utils.SelectClipRgn(_hdc, IntPtr.Zero);
                _g.ReleaseHdc(_hdc);
                _hdc = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Set a resource (e.g. a font) for the specified device context.
        /// WARNING: Calling Font.ToHfont() many times without releasing the font handle crashes the app.
        /// </summary>
        private void SetFont(Font font)
        {
            InitHdc();
            Win32Utils.SelectObject(_hdc, HtmlRenderer.Drawing.FontsUtils.GetCachedHFont(font.InnerFont as System.Drawing.Font));
        }

        /// <summary>
        /// Set the text color of the device context.
        /// </summary>
        private void SetTextColor(Color color)
        {
            InitHdc();
            int rgb = (color.B & 0xFF) << 16 | (color.G & 0xFF) << 8 | color.R;
            Win32Utils.SetTextColor(_hdc, rgb);
        }

        /// <summary>
        /// Special draw logic to draw transparent text using GDI.<br/>
        /// 1. Create in-memory DC<br/>
        /// 2. Copy background to in-memory DC<br/>
        /// 3. Draw the text to in-memory DC<br/>
        /// 4. Copy the in-memory DC to the proper location with alpha blend<br/>
        /// </summary>
        private static void DrawTransparentText(IntPtr hdc, string str, Font font, Point point, Size size, Color color)
        {
            IntPtr dib;
            var memoryHdc = Win32Utils.CreateMemoryHdc(hdc, size.Width, size.Height, out dib);

            try
            {
                // copy target background to memory HDC so when copied back it will have the proper background
                Win32Utils.BitBlt(memoryHdc, 0, 0, size.Width, size.Height, hdc, point.X, point.Y, Win32Utils.BitBltCopy);

                // Create and select font
                Win32Utils.SelectObject(memoryHdc, HtmlRenderer.Drawing.FontsUtils.GetCachedHFont(font.InnerFont as System.Drawing.Font));
                Win32Utils.SetTextColor(memoryHdc, (color.B & 0xFF) << 16 | (color.G & 0xFF) << 8 | color.R);

                // Draw text to memory HDC
                Win32Utils.TextOut(memoryHdc, 0, 0, str, str.Length);

                // copy from memory HDC to normal HDC with alpha blend so achieve the transparent text
                Win32Utils.AlphaBlend(hdc, point.X, point.Y, size.Width, size.Height, memoryHdc, 0, 0, size.Width, size.Height, new BlendFunction(color.A));
            }
            finally
            {
                Win32Utils.ReleaseMemoryHdc(memoryHdc, dib);
            }
        }

        //=====================================================
        public LayoutFarm.Drawing.FontInfo GetFontInfo(Font f)
        {
            return HtmlRenderer.Drawing.FontsUtils.GetCachedFont(f.InnerFont as System.Drawing.Font);
        }
        public LayoutFarm.Drawing.FontInfo GetFontInfo(string fontname, float fsize, FontStyle st)
        {
            return HtmlRenderer.Drawing.FontsUtils.GetCachedFont(fontname, fsize, (System.Drawing.FontStyle)st);
        }
        public float MeasureWhitespace(LayoutFarm.Drawing.Font f)
        {
            return HtmlRenderer.Drawing.FontsUtils.MeasureWhitespace(this, f);
        }

        #endregion
    }



}