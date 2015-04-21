// 2015,2014 ,BSD, WinterDev
//ArthurHub  , Jose Manuel Menendez Poo

// "Therefore those skilled at the unorthodox
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
using System.Collections.Generic;
using PixelFarm.Drawing;
using LayoutFarm.Css;
using PixelFarm.Drawing;

namespace LayoutFarm.HtmlBoxes
{
    /// <summary>
    /// Helps on CSS Layout.
    /// </summary>
    static class CssLayoutEngine
    {

        const float CSS_OFFSET_THRESHOLD = 0.1f;

        /// <summary>
        /// Measure image box size by the width\height set on the box and the actual rendered image size.<br/>
        /// If no image exists for the box error icon will be set.
        /// </summary>
        /// <param name="imgRun">the image word to measure</param>
        public static void MeasureImageSize(CssImageRun imgRun, LayoutVisitor lay)
        {
            var width = imgRun.OwnerBox.Width;
            var height = imgRun.OwnerBox.Height;

            bool hasImageTagWidth = width.Number > 0 && width.UnitOrNames == CssUnitOrNames.Pixels;
            bool hasImageTagHeight = height.Number > 0 && height.UnitOrNames == CssUnitOrNames.Pixels;
            bool scaleImageHeight = false;

            if (hasImageTagWidth)
            {
                imgRun.Width = width.Number;
            }
            else if (width.Number > 0 && width.IsPercentage)
            {

                imgRun.Width = width.Number * lay.LatestContainingBlock.SizeWidth;
                scaleImageHeight = true;
            }
            else if (imgRun.HasUserImageContent)
            {
                imgRun.Width = imgRun.ImageRectangle == Rectangle.Empty ? imgRun.OriginalImageWidth : imgRun.ImageRectangle.Width;
            }
            else
            {
                imgRun.Width = hasImageTagHeight ? height.Number / 1.14f : 20;
            }

            var maxWidth = imgRun.OwnerBox.MaxWidth;// new CssLength(imageWord.OwnerBox.MaxWidth);
            if (maxWidth.Number > 0)
            {
                float maxWidthVal = -1;
                switch (maxWidth.UnitOrNames)
                {
                    case CssUnitOrNames.Percent:
                        {
                            maxWidthVal = maxWidth.Number * lay.LatestContainingBlock.SizeWidth;
                        } break;
                    case CssUnitOrNames.Pixels:
                        {
                            maxWidthVal = maxWidth.Number;
                        } break;
                }


                if (maxWidthVal > -1 && imgRun.Width > maxWidthVal)
                {
                    imgRun.Width = maxWidthVal;
                    scaleImageHeight = !hasImageTagHeight;
                }
            }

            if (hasImageTagHeight)
            {
                imgRun.Height = height.Number;
            }
            else if (imgRun.HasUserImageContent)
            {
                imgRun.Height = imgRun.ImageRectangle == Rectangle.Empty ? imgRun.OriginalImageHeight : imgRun.ImageRectangle.Height;
            }
            else
            {
                imgRun.Height = imgRun.Width > 0 ? imgRun.Width * 1.14f : 22.8f;
            }

            if (imgRun.HasUserImageContent)
            {
                // If only the width was set in the html tag, ratio the height.
                if ((hasImageTagWidth && !hasImageTagHeight) || scaleImageHeight)
                {
                    // Divide the given tag width with the actual image width, to get the ratio.
                    float ratio = imgRun.Width / imgRun.OriginalImageWidth;
                    imgRun.Height = imgRun.OriginalImageHeight * ratio;
                }
                // If only the height was set in the html tag, ratio the width.
                else if (hasImageTagHeight && !hasImageTagWidth)
                {
                    // Divide the given tag height with the actual image height, to get the ratio.
                    float ratio = imgRun.Height / imgRun.OriginalImageHeight;
                    imgRun.Width = imgRun.OriginalImageWidth * ratio;
                }
            }
            //imageWord.Height += imageWord.OwnerBox.ActualBorderBottomWidth + imageWord.OwnerBox.ActualBorderTopWidth + imageWord.OwnerBox.ActualPaddingTop + imageWord.OwnerBox.ActualPaddingBottom;
        }
        /// <summary>
        /// Check if the given box contains only inline child boxes.
        /// </summary>
        /// <param name="box">the box to check</param>
        /// <returns>true - only inline child boxes, false - otherwise</returns>
        static bool ContainsInlinesOnly(CssBox box)
        {
            var children = CssBox.UnsafeGetChildren(box);
            var linkedNode = children.GetFirstLinkedNode();
            while (linkedNode != null)
            {

                if (!linkedNode.Value.IsInline)
                {
                    return false;
                }
                linkedNode = linkedNode.Next;
            }
            return true;
        }
        public static void PerformContentLayout(CssBox box, LayoutVisitor lay)
        {
            //if (box.CssDisplay == CssDisplay.InlineBlock)
            //{
            //}

            //this box has its own  container property
            //this box may use...
            // 1) line formatting context  , or
            // 2) block formatting context 

            var myContainingBlock = lay.LatestContainingBlock;
            if (box.CssDisplay != Css.CssDisplay.TableCell)
            {
                //-------------------------------------------
                if (box.CssDisplay != Css.CssDisplay.Table)
                {
                    float availableWidth = myContainingBlock.GetClientWidth();

                    if (!box.Width.IsEmptyOrAuto)
                    {
                        availableWidth = CssValueParser.ConvertToPx(box.Width, availableWidth, box);
                    }

                    box.SetWidth(availableWidth);
                    // must be separate because the margin can be calculated by percentage of the width
                    box.SetWidth(availableWidth - box.ActualMarginLeft - box.ActualMarginRight);
                }
                //-------------------------------------------

                float localLeft = myContainingBlock.GetClientLeft() + box.ActualMarginLeft;
                float localTop = 0;
                var prevSibling = lay.LatestSiblingBox;



                if (prevSibling == null)
                {
                    //this is first child of parent
                    if (box.ParentBox != null)
                    {
                        localTop = myContainingBlock.GetClientTop();
                    }
                }
                else
                {
                    localTop = prevSibling.LocalBottom + prevSibling.ActualBorderBottomWidth;
                }

                localTop += box.UpdateMarginTopCollapse(prevSibling);

                box.SetLocation(localLeft, localTop);
                box.SetHeightToZero();
            }
            //--------------------------------------------------------------------------

            switch (box.CssDisplay)
            {
                case Css.CssDisplay.Table:
                case Css.CssDisplay.InlineTable:
                    {
                        //If we're talking about a table here..

                        lay.PushContaingBlock(box);
                        var currentLevelLatestSibling = lay.LatestSiblingBox;
                        lay.LatestSiblingBox = null;//reset

                        CssTableLayoutEngine.PerformLayout(box, myContainingBlock.GetClientWidth(), lay);

                        lay.LatestSiblingBox = currentLevelLatestSibling;
                        lay.PopContainingBlock();
                        //TODO: check if this can have absolute layer? 
                    } break;
                case CssDisplay.InlineFlex:
                case CssDisplay.Flex:
                    {
                        //------------------------------------------------
                        //arrange as normal first
                        if (box.IsCustomCssBox)
                        {
                            //has custom layout method
                            box.ReEvaluateComputedValues(lay.SampleIFonts, lay.LatestContainingBlock);
                            box.CustomRecomputedValue(lay.LatestContainingBlock, lay.GraphicsPlatform);
                        }
                        else
                        {
                            if (ContainsInlinesOnly(box))
                            {
                                //This will automatically set the bottom of this block
                                PerformLayoutLinesContext(box, lay);
                            }
                            else if (box.ChildCount > 0)
                            {
                                PerformLayoutBlocksContext(box, lay);
                            }

                            if (box.HasAbsoluteLayer)
                            {
                                LayoutContentInAbsoluteLayer(lay, box);
                            }
                        }
                        //------------------------------------------------
                        RearrangeWithFlexContext(box, lay);
                        //------------------------------------------------
                    } break;
                default:
                    {
                        //formatting context for...
                        //1. line formatting context
                        //2. block formatting context 
                        if (box.IsCustomCssBox)
                        {
                            //has custom layout method
                            box.ReEvaluateComputedValues(lay.SampleIFonts, lay.LatestContainingBlock);
                            box.CustomRecomputedValue(lay.LatestContainingBlock, lay.GraphicsPlatform);
                        }
                        else
                        {
                            if (ContainsInlinesOnly(box))
                            {
                                //This will automatically set the bottom of this block
                                PerformLayoutLinesContext(box, lay);
                            }
                            else if (box.ChildCount > 0)
                            {
                                PerformLayoutBlocksContext(box, lay);
                            }

                            if (box.HasAbsoluteLayer)
                            {
                                LayoutContentInAbsoluteLayer(lay, box);
                            }
                        }
                    } break;
            }
        }


        /// <summary>
        /// do layout line formatting context
        /// </summary>
        /// <param name="hostBlock"></param>
        /// <param name="lay"></param>
        static void PerformLayoutLinesContext(CssBox hostBlock, LayoutVisitor lay)
        {

            //this in line formatting context
            //*** hostBlock must confirm that it has all inline children        

            hostBlock.SetHeightToZero();
            hostBlock.ResetLineBoxes();

            //----------------------------------------------------------------------------------------
            float limitLocalRight = hostBlock.SizeWidth - (hostBlock.ActualPaddingRight + hostBlock.ActualBorderRightWidth);

            float localX = hostBlock.ActualTextIndent + hostBlock.ActualPaddingLeft + hostBlock.ActualBorderLeftWidth;
            float localY = hostBlock.ActualPaddingTop + hostBlock.ActualBorderTopWidth;


            int interlineSpace = 0;

            //First line box

            CssLineBox line = new CssLineBox(hostBlock);
            hostBlock.AddLineBox(line);
            //****
            FlowBoxContentIntoHost(lay, hostBlock, hostBlock,
                  limitLocalRight, localX,
                  ref line, ref localX);
            //**** 
            // if width is not restricted we need to lower it to the actual width
            if (hostBlock.SizeWidth + lay.ContainerBlockGlobalX >= CssBoxConstConfig.BOX_MAX_RIGHT)
            {
                float newWidth = localX + hostBlock.ActualPaddingRight + hostBlock.ActualBorderRightWidth;// CssBox.MAX_RIGHT - (args.ContainerBlockGlobalX + blockBox.LocalX);
                if (newWidth <= CSS_OFFSET_THRESHOLD)
                {
                    newWidth = CSS_OFFSET_THRESHOLD;
                }
                hostBlock.SetWidth(newWidth);
            }
            //--------------------- 
            float maxLineWidth = 0;
            if (hostBlock.CssDirection == CssDirection.Rtl)
            {
                CssTextAlign textAlign = hostBlock.CssTextAlign;
                foreach (CssLineBox linebox in hostBlock.GetLineBoxIter())
                {
                    ApplyAlignment(linebox, textAlign, lay);
                    ApplyRightToLeft(linebox); //*** 
                    linebox.CloseLine(lay); //*** 
                    linebox.CachedLineTop = localY;
                    localY += linebox.CacheLineHeight + interlineSpace; // + interline space?

                    if (maxLineWidth < linebox.CachedExactContentWidth)
                    {
                        maxLineWidth = linebox.CachedExactContentWidth;
                    }
                }
            }
            else
            {

                CssTextAlign textAlign = hostBlock.CssTextAlign;
                foreach (CssLineBox linebox in hostBlock.GetLineBoxIter())
                {
                    ApplyAlignment(linebox, textAlign, lay);

                    linebox.CloseLine(lay); //***

                    linebox.CachedLineTop = localY;
                    localY += linebox.CacheLineHeight + interlineSpace;

                    if (maxLineWidth < linebox.CachedExactContentWidth)
                    {
                        maxLineWidth = linebox.CachedExactContentWidth;
                    }
                }
            }



            hostBlock.SetHeight(localY + hostBlock.ActualPaddingBottom + hostBlock.ActualBorderBottomWidth);

            //final 
            SetFinalInnerContentSize(hostBlock, maxLineWidth, hostBlock.SizeHeight, lay);
        }
        static void PerformLayoutBlocksContext(CssBox box, LayoutVisitor lay)
        {

            //block formatting context.... 
            lay.PushContaingBlock(box);
            var currentLevelLatestSibling = lay.LatestSiblingBox;
            lay.LatestSiblingBox = null;//reset 
            //------------------------------------------  
            var children = CssBox.UnsafeGetChildren(box);
            var cnode = children.GetFirstLinkedNode();
            while (cnode != null)
            {
                var childBox = cnode.Value;
                //----------------------------
                if (childBox.IsBrElement)
                {
                    //br always block
                    CssBox.ChangeDisplayType(childBox, Css.CssDisplay.Block);
                    childBox.SetHeight(FontDefaultConfig.DEFAULT_FONT_SIZE * 0.95f);
                }
                //-----------------------------
                if (childBox.IsInline)
                {
                    //inline correction on-the-fly ! 
                    //1. collect consecutive inlinebox
                    //   and move to new anon block box

                    CssBox anoForInline = CreateAnonBlock(box, childBox);
                    anoForInline.ReEvaluateComputedValues(lay.SampleIFonts, box);

                    var tmp = cnode.Next;
                    do
                    {
                        children.Remove(childBox);
                        anoForInline.AppendChild(childBox);

                        if (tmp != null)
                        {
                            childBox = tmp.Value;
                            if (childBox.IsInline)
                            {
                                tmp = tmp.Next;
                                if (tmp == null)
                                {
                                    children.Remove(childBox);
                                    anoForInline.AppendChild(childBox);
                                    break;
                                }
                            }
                            else
                            {
                                break;//break from do while
                            }
                        }
                        else
                        {
                            break;
                        }
                    } while (true);

                    childBox = anoForInline;
                    //------------------------   
                    //2. move this inline box 
                    //to new anonbox 
                    cnode = tmp;
                    //------------------------ 
                    childBox.PerformLayout(lay);

                    if (childBox.CanBeReferenceSibling)
                    {
                        lay.LatestSiblingBox = childBox;
                    }
                }
                else
                {
                    childBox.PerformLayout(lay);

                    if (childBox.CanBeReferenceSibling)
                    {
                        lay.LatestSiblingBox = childBox;
                    }

                    cnode = cnode.Next;
                }
            }

            //------------------------------------------
            lay.LatestSiblingBox = currentLevelLatestSibling;
            lay.PopContainingBlock();
            //------------------------------------------------ 
            float boxWidth = CalculateActualWidth(box);

            if (lay.ContainerBlockGlobalX + boxWidth > CssBoxConstConfig.BOX_MAX_RIGHT)
            {
            }
            else
            {
                if (box.CssDisplay != Css.CssDisplay.TableCell)
                {
                    box.SetWidth(boxWidth);
                }
            }

            float boxHeight = box.GetHeightAfterMarginBottomCollapse(lay.LatestContainingBlock);
            box.SetHeight(boxHeight);
            //--------------------------------------------------------------------------------
            //final  
            SetFinalInnerContentSize(box, boxWidth, boxHeight, lay);

        }
        static void SetFinalInnerContentSize(CssBox box, float innerContentW, float innerContentH, LayoutVisitor lay)
        {
            box.InnerContentWidth = innerContentW;
            box.InnerContentHeight = innerContentH;

            if (!box.Height.IsEmptyOrAuto)
            {
                var h = CssValueParser.ConvertToPx(box.Height, lay.LatestContainingBlock.SizeWidth, lay.LatestContainingBlock);
                box.SetExpectedSize(box.ExpectedWidth, h);
                box.SetHeight(h);
            }
            else
            {
                switch (box.Position)
                {
                    case CssPosition.Fixed:
                    case CssPosition.Absolute:
                        box.SetHeight(box.InnerContentHeight);
                        break;
                }

            }
            if (!box.Width.IsEmptyOrAuto)
            {
                //find max line width  
                var w = CssValueParser.ConvertToPx(box.Width, lay.LatestContainingBlock.SizeWidth, lay.LatestContainingBlock);
                box.SetExpectedSize(w, box.ExpectedHeight);
                box.SetWidth(w);
            }
            else
            {
                switch (box.Position)
                {
                    case CssPosition.Fixed:
                    case CssPosition.Absolute:
                        box.SetWidth(box.InnerContentWidth);
                        break;
                }
            }

            switch (box.Overflow)
            {
                case CssOverflow.Scroll:
                case CssOverflow.Auto:
                    {
                        if ((box.InnerContentHeight > box.SizeHeight) ||
                        (box.InnerContentWidth > box.SizeWidth))
                        {
                            lay.RequestScrollView(box);
                        }
                    } break;
            }
        }
        static float CalculateActualWidth(CssBox box)
        {
            float maxRight = 0;
            var boxes = CssBox.UnsafeGetChildren(box);
            var cnode = boxes.GetFirstLinkedNode();
            while (cnode != null)
            {
                float nodeRight = cnode.Value.LocalRight;
                maxRight = nodeRight > maxRight ? nodeRight : maxRight;
                cnode = cnode.Next;
            }
            return maxRight + (box.ActualBorderLeftWidth + box.ActualPaddingLeft +
                box.ActualPaddingRight + box.ActualBorderRightWidth);
        }

        static CssBox CreateAnonBlock(CssBox parent, CssBox insertBefore)
        {
            //auto gen by layout engine ***
            var newBox = new CssBox(null, CssBox.UnsafeGetBoxSpec(parent).GetAnonVersion(), parent.RootGfx);
            CssBox.ChangeDisplayType(newBox, Css.CssDisplay.Block);
            parent.InsertChild(insertBefore, newBox);
            return newBox;
        }

        /// <summary>
        /// Recursively flows the content of the box using the inline model
        /// </summary>
        /// <param name="lay"></param>
        /// <param name="hostBox"></param>
        /// <param name="srcBox"></param>
        /// <param name="limitLocalRight"></param>
        /// <param name="firstRunStartX"></param>
        /// <param name="hostLine"></param>
        /// <param name="cx"></param>
        static void FlowBoxContentIntoHost(
          LayoutVisitor lay,
          CssBox hostBox, //target 
          CssBox srcBox, //src that has  runs /splitable content) to flow into hostBox line model
          float limitLocalRight,
          float firstRunStartX,
          ref CssLineBox hostLine,
          ref float cx)
        {

            //recursive *** 
            //--------------------------------------------------------------------
            var oX = cx;
            if (srcBox.HasRuns)
            {
                //condition 3 

                FlowRunsIntoHost(lay, hostBox, srcBox, srcBox, 0,
                     limitLocalRight, firstRunStartX,
                     0, 0,
                     CssBox.UnsafeGetRunList(srcBox),
                     ref hostLine, ref cx
                     );
            }
            else
            {

                int childNumber = 0;
                var ifonts = lay.SampleIFonts;
                foreach (CssBox b in srcBox.GetChildBoxIter())
                {
                    float leftMostSpace = 0, rightMostSpace = 0;
                    //if b has absolute pos then it is removed from the flow 
                    if (b.NeedComputedValueEvaluation)
                    {
                        b.ReEvaluateComputedValues(ifonts, hostBox);
                    }
                    b.MeasureRunsSize(lay);
#if DEBUG
                    if (b.Position == CssPosition.Absolute)
                    {
                        //should not found here!
                        throw new NotSupportedException();
                    }
#endif

                    cx += leftMostSpace;
                    //------------------------------------------------  
                    if (b.CssDisplay == CssDisplay.InlineBlock)
                    {
                        //can't split 
                        //create 'block-run'  
                        PerformContentLayout(b, lay);

                        CssBlockRun blockRun = b.JustBlockRun;
                        if (blockRun == null)
                        {
                            blockRun = new CssBlockRun(b);
                            blockRun.SetOwner(srcBox);
                            b.JustBlockRun = blockRun;
                        }


                        if (b.Width.IsEmptyOrAuto)
                        {
                            blockRun.SetSize(CssBox.GetLatestCachedMinWidth(b), b.SizeHeight);
                        }
                        else
                        {
                            blockRun.SetSize(b.SizeWidth, b.SizeHeight);
                        }

                        b.SetLocation(b.LocalX, 0); //because of inline***

                        FlowRunsIntoHost(lay, hostBox, srcBox, b, childNumber,
                            limitLocalRight, firstRunStartX,
                            leftMostSpace, rightMostSpace,
                            new List<CssRun>() { b.JustBlockRun },
                            ref hostLine, ref cx);
                    }
                    else if (b.HasRuns)
                    {
                        FlowRunsIntoHost(lay, hostBox, srcBox, b, childNumber,
                         limitLocalRight, firstRunStartX,
                         leftMostSpace, rightMostSpace,

                         CssBox.UnsafeGetRunList(b),
                         ref hostLine, ref cx);
                    }
                    else
                    {
#if DEBUG
                        if (srcBox.CssDisplay == CssDisplay.InlineBlock)
                        {
                            //should not found here!
                            throw new NotSupportedException();
                        }
#endif
                        //go deeper  
                        //recursive ***
                        FlowBoxContentIntoHost(lay, hostBox, b,
                                   limitLocalRight, firstRunStartX,
                                   ref hostLine, ref cx);
                    }

                    cx += rightMostSpace;
                    childNumber++;
                }
            }


            if (srcBox.Position == CssPosition.Relative)
            {
                //offset content relative to it 'flow' position'
                var left = CssValueParser.ConvertToPx(srcBox.Left, hostBox.SizeWidth, srcBox);
                var top = CssValueParser.ConvertToPx(srcBox.Top, hostBox.SizeWidth, srcBox);
                srcBox.SetLocation(srcBox.LocalX + left, srcBox.LocalY + top);
            }

        }
        static void LayoutContentInAbsoluteLayer(LayoutVisitor lay, CssBox srcBox)
        {


            var ifonts = lay.SampleIFonts;

            //css3 jan2015: absolute position
            //use offset relative to its normal the box's containing box***

            float containerW = lay.LatestContainingBlock.SizeWidth;

            float maxRight = 0;
            float maxBottom = 0;

            foreach (var b in srcBox.GetAbsoluteChildBoxIter())
            {
                if (b.NeedComputedValueEvaluation)
                {
                    b.ReEvaluateComputedValues(ifonts, lay.LatestContainingBlock);
                }

                b.MeasureRunsSize(lay);
                PerformContentLayout(b, lay);

                b.SetLocation(
                     CssValueParser.ConvertToPx(b.Left, containerW, b),
                     CssValueParser.ConvertToPx(b.Top, containerW, b));

                var localRight = b.LocalRight;
                var localBottom = b.LocalBottom;

                if (maxRight < localRight)
                {
                    maxRight = localRight;
                }
                if (maxBottom < localBottom)
                {
                    maxBottom = localBottom;
                }
            }

            int i_maxRight = (int)maxRight;
            int i_maxBottom = (int)maxBottom;
            if (i_maxRight > srcBox.InnerContentWidth)
            {
                srcBox.InnerContentWidth = i_maxRight;
            }
            if (i_maxBottom > srcBox.InnerContentHeight)
            {
                srcBox.InnerContentHeight = i_maxBottom;
            }
        }

        static void FlowRunsIntoHost(LayoutVisitor lay,
          CssBox hostBox,
          CssBox splitableBox,
          CssBox b,
          int childNumber, //child number of b
          float limitRight,
          float firstRunStartX,
          float leftMostSpace,
          float rightMostSpace,
          List<CssRun> runs,
          ref CssLineBox hostLine,
          ref float cx)
        {
            //flow runs into hostLine, create new line if need  
            bool wrapNoWrapBox = false;
            var bWhiteSpace = b.WhiteSpace;
            if (bWhiteSpace == CssWhiteSpace.NoWrap && cx > firstRunStartX)
            {
                var tmpRight = cx;
                for (int i = runs.Count - 1; i >= 0; --i)
                {
                    tmpRight += runs[i].Width;
                }
                //----------------------------------------- 
                if (tmpRight > limitRight)
                {
                    wrapNoWrapBox = true;
                }
            }

            //----------------------------------------------------- 

            int lim = runs.Count - 1;
            for (int i = 0; i <= lim; ++i)
            {
                var run = runs[i];
                //---------------------------------------------------
                //check if need to start new line ? 
                if ((cx + run.Width + rightMostSpace > limitRight &&
                     bWhiteSpace != CssWhiteSpace.NoWrap &&
                     bWhiteSpace != CssWhiteSpace.Pre &&
                     (bWhiteSpace != CssWhiteSpace.PreWrap || !run.IsSpaces))
                     || run.IsLineBreak || wrapNoWrapBox)
                {

                    wrapNoWrapBox = false; //once! 

                    //-------------------------------
                    //create new line ***
                    hostLine = new CssLineBox(hostBox);
                    hostBox.AddLineBox(hostLine);
                    //reset x pos for new line
                    cx = firstRunStartX;


                    // handle if line is wrapped for the first text element where parent has left margin/padding
                    if (childNumber == 0 && //b is first child of splitable box ('b' == splitableBox.GetFirstChild())
                        !run.IsLineBreak &&
                        (i == 0 || splitableBox.ParentBox.IsBlock))//this run is first run of 'b' (run == b.FirstRun)
                    {


                        cx += splitableBox.ActualMarginLeft +
                            splitableBox.ActualBorderLeftWidth +
                            splitableBox.ActualPaddingLeft;
                    }

                    if (run.IsSolidContent || i == 0)
                    {
                        cx += leftMostSpace;
                    }
                }
                //---------------------------------------------------

                if (run.IsSpaces && hostLine.RunCount == 0)
                {
                    //not add 
                    continue;
                }
                //---------------------------------------------------

                hostLine.AddRun(run); //***
                if (lim == 0)
                {
                    //single one
                    cx += b.ActualPaddingLeft;
                    run.SetLocation(cx, 0);
                    cx += run.Width + b.ActualPaddingRight;
                }
                else
                {
                    if (i == 0)
                    {
                        //first
                        cx += b.ActualPaddingLeft;
                        run.SetLocation(cx, 0);
                        cx = run.Right;
                    }
                    else if (i == lim)
                    {
                        run.SetLocation(cx, 0);
                        cx += run.Width + b.ActualPaddingRight;
                    }
                    else
                    {
                        run.SetLocation(cx, 0);
                        cx = run.Right;
                    }
                }
                //---------------------------------------------------
                //move current_line_x to right of run
                //cx = run.Right;
            }
        }
        /// <summary>
        /// Applies vertical and horizontal alignment to words in lineboxes
        /// </summary>
        /// <param name="g"></param>
        /// <param name="lineBox"></param> 
        static void ApplyAlignment(CssLineBox lineBox, CssTextAlign textAlign, LayoutVisitor lay)
        {
            switch (textAlign)
            {
                case CssTextAlign.Right:
                    ApplyRightAlignment(lineBox);
                    break;
                case CssTextAlign.Center:
                    ApplyCenterAlignment(lineBox);
                    break;
                case CssTextAlign.Justify:
                    ApplyJustifyAlignment(lineBox);
                    break;
                default:
                    break;
            }
            //--------------------------------------------- 
            // Applies vertical alignment to the linebox 
            return;
            //TODO: review here
            lineBox.ApplyBaseline(lineBox.CalculateTotalBoxBaseLine(lay));
            //---------------------------------------------  
        }

        /// <summary>
        /// Applies right to left direction to words
        /// </summary>
        /// <param name="blockBox"></param>
        /// <param name="lineBox"></param>
        static void ApplyRightToLeft(CssLineBox lineBox)
        {
            if (lineBox.RunCount > 0)
            {

                float left = lineBox.GetFirstRun().Left;
                float right = lineBox.GetLastRun().Right;
                foreach (CssRun run in lineBox.GetRunIter())
                {
                    float diff = run.Left - left;
                    float w_right = right - diff;
                    run.Left = w_right - run.Width;
                }
            }
        }
        static void ApplyJustifyAlignment(CssLineBox lineBox)
        {


            if (lineBox.IsLastLine) return;

            float indent = lineBox.IsFirstLine ? lineBox.OwnerBox.ActualTextIndent : 0f;

            float runWidthSum = 0f;
            int runCount = 0;

            float availableWidth = lineBox.OwnerBox.GetClientWidth() - indent;

            // Gather text sum
            foreach (CssRun w in lineBox.GetRunIter())
            {
                runWidthSum += w.Width;
                runCount++;
            }

            if (runCount == 0) return; //Avoid Zero division

            float spaceOfEachRun = (availableWidth - runWidthSum) / runCount; //Spacing that will be used

            float cX = lineBox.OwnerBox.GetClientLeft() + indent;
            CssRun lastRun = lineBox.GetLastRun();
            foreach (CssRun run in lineBox.GetRunIter())
            {
                run.Left = cX;
                cX = run.Right + spaceOfEachRun;
                if (run == lastRun)
                {
                    run.Left = lineBox.OwnerBox.GetClientRight() - run.Width;
                }
            }
        }

        /// <summary>
        /// Applies centered alignment to the text on the linebox
        /// </summary>
        /// <param name="g"></param>
        /// <param name="line"></param>
        private static void ApplyCenterAlignment(CssLineBox line)
        {

            if (line.RunCount == 0) return;
            CssRun lastRun = line.GetLastRun();
            float diff = (line.OwnerBox.GetClientWidth() - lastRun.Right) / 2;
            if (diff > CSS_OFFSET_THRESHOLD)
            {
                foreach (CssRun word in line.GetRunIter())
                {
                    word.Left += diff;
                }
                line.CachedLineContentWidth += diff;
            }
        }

        /// <summary>
        /// Applies right alignment to the text on the linebox
        /// </summary>
        /// <param name="g"></param>
        /// <param name="line"></param>
        private static void ApplyRightAlignment(CssLineBox line)
        {
            if (line.RunCount == 0)
            {
                return;
            }
            CssRun lastRun = line.GetLastRun();
            float diff = line.OwnerBox.GetClientWidth() - line.GetLastRun().Right;
            if (diff > CSS_OFFSET_THRESHOLD)
            {
                foreach (CssRun word in line.GetRunIter())
                {
                    word.Left += diff;
                }
            }
        }
        static void RearrangeWithFlexContext(CssBox box, LayoutVisitor lay)
        {

            //this is an experiment!,  
            var children = CssBox.UnsafeGetChildren(box);
            var cnode = children.GetFirstLinkedNode(); 

            List<FlexItem> simpleFlexLine = new List<FlexItem>();
            FlexLine flexLine = new FlexLine(box); 
            while (cnode != null)
            {
                flexLine.AddChild(new FlexItem(cnode.Value));
                cnode = cnode.Next;
            }
            flexLine.Arrange();


            if (box.Height.IsEmptyOrAuto)
            {
                //set new height                
                box.SetHeight(flexLine.LineHeightAfterArrange);
                //check if it need scrollbar or not 
            }
            if (box.Width.IsEmptyOrAuto)
            {
                box.SetWidth(flexLine.LineWidthAfterArrange);
            }

            SetFinalInnerContentSize(box, flexLine.LineWidthAfterArrange, flexLine.LineHeightAfterArrange, lay);

        }
    }
}