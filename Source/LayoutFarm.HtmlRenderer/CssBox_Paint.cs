﻿//BSD, 2014-present, WinterDev +
//ArthurHub, Jose Manuel Menendez Poo

using System;
using PixelFarm.Drawing;
namespace LayoutFarm.HtmlBoxes
{
    partial class CssBox
    {

        Rectangle GetVisualRectBounds()
        {
            return new Rectangle((int)this.LocalX, (int)this.LocalY, (int)this.VisualWidth, (int)this.VisualHeight);
        }
        public void InvalidateGraphics()
        {
            //bubble invalidate area to to parent?


            if (_justBlockRun != null)
            {

                Rectangle clientArea = this.GetVisualRectBounds();

#if DEBUG
                if (_viewportY != 0)
                {
                    //TODO review here again***
                    //clientArea.Offset(0, -_viewportY);
                    //clientArea.Intersect(justBlockRun.HostLine.OwnerBox.GetVisualRectBounds());
                    ////#if DEBUG
                    ////                    Console.WriteLine(__aa_dbugId + ":i2_" + _viewportY.ToString());
                    ////#endif
                }
#endif

                clientArea.Offset(
                  (int)(_justBlockRun.Left),
                  (int)(_justBlockRun.Top + _justBlockRun.HostLine.CachedLineTop));

                _justBlockRun.HostLine.OwnerBox.InvalidateGraphics(clientArea);

                return;
            }

            CssBox parentBox = _absLayerOwner ?? this.ParentBox;
            if (parentBox != null)
            {
                Rectangle clientArea = this.GetVisualRectBounds();
#if DEBUG
                if (_viewportY != 0)
                {
                    ////TODO review here again***
                    //clientArea = new Rectangle(0, 0, (int)parentBox.VisualWidth, (int)parentBox.VisualHeight);
                    ////clientArea.Offset(0, -_viewportY);
                    ////clientArea.Intersect(parentBox.GetVisualRectBounds());
                    ////#if DEBUG
                    ////                    Console.WriteLine(__aa_dbugId + ":i2_" + _viewportY.ToString());
                    ////#endif
                }
#endif

                parentBox.InvalidateGraphics(clientArea);
            }
        }

        public virtual void InvalidateGraphics(Rectangle clientArea)
        {
            //bubble up to parent
            //clientArea => area relative to this element
            //adjust to 
            //adjust client area 

#if DEBUG
            if (_viewportY != 0)
            {

                System.Diagnostics.Debug.WriteLine(__aa_dbugId + ":i1_" + _viewportY.ToString());

            }
#endif

            if (_justBlockRun != null)
            {

                clientArea.Offset(
                    (int)(_justBlockRun.Left),
                    (int)(_justBlockRun.Top + _justBlockRun.HostLine.CachedLineTop));
                _justBlockRun.HostLine.OwnerBox.InvalidateGraphics(clientArea);

                return;
            }

            CssBox parentBox = _absLayerOwner ?? this.ParentBox;
            if (parentBox != null)
            {
                clientArea.Offset((int)this.LocalX, (int)this.LocalY);
                parentBox.InvalidateGraphics(clientArea);
            }
        }

        public static void Paint(CssBox box, PaintVisitor p)
        {
#if DEBUG         
            dbugCounter.dbugBoxPaintCount++;
#endif
            if (!box._isVisible) { return; }

            //offset 
            if (box._mayHasViewport && (box.ViewportX != 0 || box.ViewportY != 0))
            {
                int enter_canvas_X = p.CanvasOriginX;
                int enter_canvas_Y = p.CanvasOriginY;

                p.SetCanvasOrigin(enter_canvas_X - box.ViewportX, enter_canvas_Y - box.ViewportY);
                box.PaintImp(p);
                p.SetCanvasOrigin(enter_canvas_X, enter_canvas_Y);//restore
            }
            else
            {
                box.PaintImp(p);
            }

        }
#if DEBUG
        public void dbugPaint(PaintVisitor p, RectangleF r)
        {
            if (!PaintVisitor.dbugDrawWireFrame)
            {
                return;
            }
            //
            var htmlE = CssBox.UnsafeGetController(this);
            if (htmlE == null)
            {
                //anonymous box
                //Font f = new Font("tahoma", 10);
                //p.Gfx.DrawString(__aa_dbugId.ToString(), f, System.Drawing.Color.Black,
                //     new PointF(r.Left + 10, r.Top + 10), new SizeF(r.Width, r.Height));
                //f.Dispose();

                p.dbugDrawDiagonalBox(KnownColors.Gray, r.Left, r.Top, r.Right, r.Bottom);
            }
            else
            {
                Color color = Color.FromArgb(255, 0, 128, 0);
                switch (_cssDisplay)
                {
                    case Css.CssDisplay.TableCell:
                        color = KnownColors.OrangeRed;
                        break;
                }
                p.dbugDrawDiagonalBox(color, r.Left, r.Top, r.Right, r.Bottom);
            }
        }
#endif
        protected virtual void PaintImp(PaintVisitor p)
        {

#if DEBUG
            //if (__aa_dbugId == 6)
            //{

            //}
            //if (this.dbugMark2 == 10 || this.dbugMark2 == 12)
            //{ 
            //}
#endif


            Css.CssDisplay display = this.CssDisplay;
            //
            if (display == Css.CssDisplay.TableCell &&
                this.EmptyCells == Css.CssEmptyCell.Hide &&
                this.IsSpaceOrEmpty)
            {
                return;
            }


#if DEBUG
            p.dbugEnterNewContext(this, PaintVisitor.PaintVisitorContextName.Init);
#endif
            Color prevBgColorHint = p.CurrentSolidBackgroundColorHint;
            //----------------------------------------------- 
            bool hasPrevClip = false;
            RectangleF prevClip = RectangleF.Empty;
            p.EnterNewLatePaintContext();
            //---------------------------------------------
            //if (display != Css.CssDisplay.Inline ||
            //    this.Position == Css.CssPosition.Absolute ||
            //    this.Position == Css.CssPosition.Fixed)
            if (_renderBGAndBorder)
            {

                RectangleF bounds = new RectangleF(0, 0, this.VisualWidth, this.VisualHeight);
                PaintBackground(p, bounds, true, true);
                if (this.HasSomeVisibleBorder)
                {
                    p.PaintBorders(this, bounds, true, true);
                }
#if DEBUG
                dbugPaint(p, bounds);
#endif

            }

            //---------------------------------------------
            if (this.LineBoxCount > 0)
            {
                float viewport_top = p.ViewportTop;
                float viewport_bottom = p.ViewportBottom;
                int drawState = 0;
                var c_lineNode = _clientLineBoxes.First;
                while (c_lineNode != null)
                {
                    CssLineBox line = c_lineNode.Value;
                    if (line.CachedLineBottom >= viewport_top &&
                        line.CachedLineTop <= viewport_bottom)
                    {
                        Rectangle currentClipRect = p.CurrentClipRect;
                        drawState = 1;//drawing in viewport area 
#if DEBUG
                        //System.Diagnostics.Debug.WriteLine("clip_rect:" + currentClipRect); 
                        dbugCounter.dbugLinePaintCount++;
#endif

                        int cX = p.CanvasOriginX;
                        int cy = p.CanvasOriginY;
                        int newCy = cy + (int)line.CachedLineTop;

                        if (newCy <= (cy + currentClipRect.Bottom) &&
                            newCy + line.CacheLineHeight >= (cy + currentClipRect.Top))
                        {
                            p.SetCanvasOrigin(cX, newCy);
                            //1.                                 
                            line.PaintBackgroundAndBorder(p);

                            SelectionSegment selSegment = line.SelectionSegment;
                            if (selSegment != null)
                            {
                                switch (selSegment.Kind)
                                {
                                    case SelectionSegmentKind.FullLine:
                                        {
                                            Color prevColor2 = p.CurrentSolidBackgroundColorHint;//save2
                                            p.CurrentSolidBackgroundColorHint = p.CssBoxSelectionColor;

                                            selSegment.PaintSelection(p, line);

                                            line.PaintRuns(p);

                                            p.CurrentSolidBackgroundColorHint = prevColor2; //restore2
                                        }
                                        break;
                                    case SelectionSegmentKind.PartialBegin:
                                    case SelectionSegmentKind.SingleLine:
                                    case SelectionSegmentKind.PartialEnd:
                                        {
                                            //TODO: review here again***
                                            //partial line

                                            //[A]
                                            line.PaintRuns(p); //normal line
                                                               //-----

                                            //[B]
                                            //selection part with clip rect

                                            Color prevColor2 = p.CurrentSolidBackgroundColorHint;//save2
                                            //p.CurrentSolidBackgroundColorHint = prevBgColorHint;


                                            int xpos = selSegment.BeginAtPx;
                                            int w = selSegment.WidthPx;

                                            Rectangle clipRect = p.CurrentClipRect;
                                            p.SetClipArea(xpos, 0, w, (int)line.CacheLineHeight);
                                            selSegment.PaintSelection(p, line);

                                            p.CurrentSolidBackgroundColorHint = p.CssBoxSelectionColor;

                                            line.PaintRuns(p);
                                            p.SetClipArea(clipRect.X, clipRect.Top, clipRect.Width, clipRect.Height);//restore

                                            p.CurrentSolidBackgroundColorHint = prevColor2; //restore2
                                        }
                                        break;
                                }

                            }
                            else
                            {
                                //2.

                                line.PaintRuns(p);
                            }


                            //3. 
                            line.PaintDecoration(p);
#if DEBUG
                            line.dbugPaintRuns(p);
#endif

                            p.SetCanvasOrigin(cX, cy);//back

                        }

                    }
                    else if (drawState == 1)
                    {
                        //outof viewport -> break
                        break;
                    }

                    //----------------------------------------
                    c_lineNode = c_lineNode.Next;
                }
            }
            else
            {
                if (this.HasContainingBlockProperty)
                {
                    p.PushContaingBlock(this);
                    int ox = p.CanvasOriginX;
                    int oy = p.CanvasOriginY;
                    var node = _aa_boxes.GetFirstLinkedNode();
                    while (node != null)
                    {
                        CssBox b = node.Value;
                        if (b.CssDisplay == Css.CssDisplay.None || b.IsAddedToAbsoluteLayer)
                        {
                            node = node.Next;
                            continue;
                        }
                        else if (b.IsOutOfFlowBox)
                        {
                            //
                            p.AddToLatePaintList(b);
                            node = node.Next;
                            continue;
                        }
                        //move to left-top of client box 
                        p.SetCanvasOrigin(ox + (int)b.LocalX, oy + (int)b.LocalY);
                        if (b._decorator != null)
                        {
                            b._decorator.Paint(b, p);
                        }

                        if (b.HasClipArea)
                        {
                            if (p.PushLocalClipArea(b.VisualWidth, b.VisualHeight))
                            {
                                CssBox.Paint(b, p);
                                p.PopLocalClipArea();
                            }

                        }
                        else
                        {
                            CssBox.Paint(b, p);
                        }

                        node = node.Next;
                    }
                    p.SetCanvasOrigin(ox, oy);
                    p.PopContainingBlock();
                }
                else
                {
                    //if not
                    int ox = p.CanvasOriginX;
                    int oy = p.CanvasOriginY;
                    var node = _aa_boxes.GetFirstLinkedNode();
                    while (node != null)
                    {
                        CssBox b = node.Value;
                        if (b.CssDisplay == Css.CssDisplay.None || b.IsAddedToAbsoluteLayer)
                        {
                            node = node.Next;
                            continue;
                        }
                        p.SetCanvasOrigin(ox + (int)b.LocalX, oy + (int)b.LocalY);

                        CssBox.Paint(b, p);
                        node = node.Next;
                    }
                    p.SetCanvasOrigin(ox, oy);
                }
            }
            //------------------------------------------
            //debug
            //var clientLeft = this.ClientLeft;
            //g.DrawRectangle(Pens.GreenYellow, 0, 0, 5, 10);
            //g.DrawRectangle(Pens.HotPink, this.ClientRight - 5, 0, 5, 10);
            //------------------------------------------   

            if (this.HasAbsoluteLayer)
            {
                p.PushContaingBlock(this);
                int ox = p.CanvasOriginX;
                int oy = p.CanvasOriginY;
                int j = _absPosLayer.Count;
                for (int i = 0; i < j; ++i)
                {
                    CssBox b = _absPosLayer.GetBox(i);
                    if (b.CssDisplay == Css.CssDisplay.None)
                    {
                        continue;
                    }
                    p.SetCanvasOrigin(ox + (int)b.LocalX, oy + (int)b.LocalY);
                    CssBox.Paint(b, p);
                }
                //var node = _absPosLayer.GetFirstLinkedNode();
                //while (node != null)
                //{
                //    CssBox b = node.Value;
                //    if (b.CssDisplay == Css.CssDisplay.None)
                //    {
                //        node = node.Next;
                //        continue;
                //    }
                //    p.SetCanvasOrigin(ox + (int)b.LocalX, oy + (int)b.LocalY);
                //    b.Paint(p);
                //    node = node.Next;
                //}
                p.SetCanvasOrigin(ox, oy);
                p.PopContainingBlock();
            }

            if (p.LatePaintItemCount > 0)
            {
                //late paint -> floatBox 
                Rectangle latestClipRect = p.CurrentClipRect;
                p.PopLocalClipArea(); //temp
                p.PushContaingBlock(this);
                int j = p.LatePaintItemCount;
                int ox = p.CanvasOriginX;
                int oy = p.CanvasOriginY;
                for (int i = 0; i < j; ++i)
                {
                    CssBox box = p.GetLatePaintItem(i);
                    if (box.CssDisplay == Css.CssDisplay.None)
                    {
                        continue;
                    }
                    p.SetCanvasOrigin(ox + (int)box.LocalX, oy + (int)box.LocalY);
                    CssBox.Paint(box, p);
                    p.SetCanvasOrigin(ox, oy);
                }
                p.PopContainingBlock();
                p.PushLocalClipArea(latestClipRect.Width, latestClipRect.Height);//push back
            }
            p.ExitCurrentLatePaintContext();
            //must! , 
            if (hasPrevClip)
            {
                p.PopLocalClipArea();
            }

            p.CurrentSolidBackgroundColorHint = prevBgColorHint;


#if DEBUG
            p.dbugExitContext();
#endif
        }
        static LinearGradientBrush CreateLinearGradientBrush(RectangleF rect,
        Color startColor, Color stopColor, float degreeAngle)
        {
            //find radius
            int w = Math.Abs((int)(rect.Right - rect.Left));
            int h = Math.Abs((int)(rect.Bottom - rect.Top));
            int max = Math.Max(w, h);
            float radius = (float)Math.Pow(2 * (max * max), 0.5f);
            //find point1 and point2
            //not implement! 
            bool fromNegativeAngle = false;
            if (degreeAngle < 0)
            {
                fromNegativeAngle = true;
                degreeAngle = -degreeAngle;
            }

            PointF startPoint = new PointF(rect.Left, rect.Top);
            PointF stopPoint = new PointF(rect.Right, rect.Top);
            if (degreeAngle > 360)
            {
            }
            //-------------------------
            if (degreeAngle == 0)
            {
                startPoint = new PointF(rect.Left, rect.Bottom);
                stopPoint = new PointF(rect.Right, rect.Bottom);
            }
            else if (degreeAngle < 90)
            {
                startPoint = new PointF(rect.Left, rect.Bottom);
                double angleRad = PixelFarm.CpuBlit.AggMath.deg2rad(degreeAngle);

                stopPoint = new PointF(
                   rect.Left + (float)(Math.Cos(angleRad) * radius),
                   rect.Bottom - (float)(Math.Sin(angleRad) * radius));
            }
            else if (degreeAngle == 90)
            {
                startPoint = new PointF(rect.Left, rect.Bottom);
                stopPoint = new PointF(rect.Left, rect.Top);
            }
            else if (degreeAngle < 180)
            {

                startPoint = new PointF(rect.Right, rect.Bottom);
                double angleRad = PixelFarm.CpuBlit.AggMath.deg2rad(degreeAngle);
                float pos = (float)(Math.Cos(angleRad) * radius);
                stopPoint = new PointF(
                   rect.Right + (float)(Math.Cos(angleRad) * radius),
                   rect.Bottom - (float)(Math.Sin(angleRad) * radius));
            }
            else if (degreeAngle == 180)
            {
                startPoint = new PointF(rect.Right, rect.Bottom);
                stopPoint = new PointF(rect.Left, rect.Bottom);
            }
            else if (degreeAngle < 270)
            {
                startPoint = new PointF(rect.Right, rect.Top);
                double angleRad = PixelFarm.CpuBlit.AggMath.deg2rad(degreeAngle);
                stopPoint = new PointF(
                   rect.Right - (float)(Math.Cos(angleRad) * radius),
                   rect.Top + (float)(Math.Sin(angleRad) * radius));
            }
            else if (degreeAngle == 270)
            {
                startPoint = new PointF(rect.Left, rect.Top);
                stopPoint = new PointF(rect.Left, rect.Bottom);
            }
            else if (degreeAngle < 360)
            {
                startPoint = new PointF(rect.Left, rect.Top);
                double angleRad = PixelFarm.CpuBlit.AggMath.deg2rad(degreeAngle);
                stopPoint = new PointF(
                   rect.Left + (float)(Math.Cos(angleRad) * radius),
                   rect.Top + (float)(Math.Sin(angleRad) * radius));
            }
            else if (degreeAngle == 360)
            {
                startPoint = new PointF(rect.Left, rect.Bottom);
                stopPoint = new PointF(rect.Right, rect.Bottom);
            }

            return new LinearGradientBrush(startPoint, stopPoint, startColor, stopColor);
        }
        /// <summary>
        /// Paints the background of the box
        /// </summary>
        /// <param name="g">the device to draw into</param>
        /// <param name="rect">the bounding rectangle to draw in</param>
        /// <param name="isFirst">is it the first rectangle of the element</param>
        /// <param name="isLast">is it the last rectangle of the element</param>
        internal void PaintBackground(PaintVisitor p, RectangleF rect, bool isFirst, bool isLast)
        {

            if (!this.HasVisibleBgColor)
            {
                return;
            }

            if (rect.Width == 0 || rect.Height == 0)
            {
                return;
            }

            Brush brush = null;
            bool dispose = false;
            if (BackgroundGradient != Color.Transparent)
            {
                //use bg gradient 

                p.CurrentSolidBackgroundColorHint = Color.Transparent;

                //linear brush
                brush = CreateLinearGradientBrush(rect,
                    ActualBackgroundColor,
                    ActualBackgroundGradient,
                    ActualBackgroundGradientAngle);

                dispose = true; //dispose***
            }
            else if (RenderUtils.IsColorVisible(ActualBackgroundColor))
            {
                //TODO: review here,
                //
                //solid brush hint for text
                p.CurrentSolidBackgroundColorHint = (ActualBackgroundColor.A == 255) ? ActualBackgroundColor : Color.Transparent;

                brush = new SolidBrush(this.ActualBackgroundColor);
                dispose = true;
            }


            DrawBoard g = p.InnerDrawBoard;
            SmoothingMode smooth = g.SmoothingMode;
            if (brush != null)
            {
                // atodo: handle it correctly (tables background)
                // if (isLast)
                //  rectangle.Width -= ActualWordSpacing + CssUtils.GetWordEndWhitespace(ActualFont); 
                //GraphicsPath roundrect = null;
                bool hasSomeRoundCorner = this.HasSomeRoundCorner;
                if (hasSomeRoundCorner)
                {
                    //roundrect = RenderUtils.GetRoundRect(rect, ActualCornerNW, ActualCornerNE, ActualCornerSE, ActualCornerSW);
                }

                if (!p.AvoidGeometryAntialias && hasSomeRoundCorner)
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                }

                //if (roundrect != null)
                //{
                //    g.FillPath(brush, roundrect);
                //}
                //else
                //{

                g.FillRectangle(brush, (float)Math.Ceiling(rect.Left), (float)Math.Ceiling(rect.Top), rect.Width, rect.Height);

                //}

                g.SmoothingMode = smooth;
                //if (roundrect != null) roundrect.Dispose();
                if (dispose) brush.Dispose();
            }

            if (isFirst)
            {
                ImageBinder bgImageBinder = this.BackgroundImageBinder;
                if (bgImageBinder != null && bgImageBinder.LocalImage != null)
                {
                    BackgroundImagePaintHelper.DrawBackgroundImage(g, this, bgImageBinder, rect);
                }
            }

        }
        internal void PaintDecoration(DrawBoard g, RectangleF rectangle, bool isFirst, bool isLast)
        {
            float y;
            switch (this.TextDecoration)
            {
                default:
                    return;
                case Css.CssTextDecoration.Underline:
                    {
                        //TODO: review here,  //temp fix with 3
                        y = (float)Math.Round(rectangle.Bottom - 3);
                    }
                    break;
                case Css.CssTextDecoration.LineThrough:
                    {
                        y = rectangle.Top + rectangle.Height / 2f;
                    }
                    break;
                case Css.CssTextDecoration.Overline:
                    {
                        y = rectangle.Top;
                    }
                    break;
            }
            
            y -= (ActualPaddingBottom + ActualBorderBottomWidth);
            float x1 = rectangle.Left;
            if (isFirst)
            {
                x1 += ActualPaddingLeft + ActualBorderLeftWidth;
            }


            float x2 = rectangle.Right;
            if (isLast)
            {
                x2 -= ActualPaddingRight + ActualBorderRightWidth;
            }

            Color prevColor = g.StrokeColor;
            g.StrokeColor = ActualColor;
            g.DrawLine(x1, y, x2, y);
            g.StrokeColor = prevColor;
        }

        public virtual void Paint(PaintVisitor p, RectangleF r)
        {
        }
    }
}