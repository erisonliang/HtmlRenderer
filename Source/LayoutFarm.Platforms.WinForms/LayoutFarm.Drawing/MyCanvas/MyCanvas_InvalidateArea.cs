﻿//2014 BSD, WinterDev
//ArthurHub

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
using System.Text;
using LayoutFarm.Drawing;


namespace LayoutFarm
{
    partial class MyCanvas
    {
        bool isCanvasReady;
        public bool DimensionInvalid
        {
            get
            {
                return (pageFlags & CANVAS_DIMEN_CHANGED) != 0;
            }
            set
            {
                if (value)
                {
                    pageFlags |= CANVAS_DIMEN_CHANGED;
                }
                else
                {
                    pageFlags &= ~CANVAS_DIMEN_CHANGED;
                }
            }
        }

        public Rect InvalidateArea
        {
            get
            {
                return invalidateArea;
            }
        }

        public bool IsContentReady
        {
            get { return this.isCanvasReady; }
            set
            {   
                //if (value)
                //{
                //    Console.WriteLine((dbugCount++) + "c_ready:true");
                //}
                //else
                //{
                //    Console.WriteLine((dbugCount++) + "c_ready:false");
                //}
                this.isCanvasReady = value;
            }
        }
        static int dbugCount = 0;

        public override void Invalidate(Rect rect)
        {
            invalidateArea.MergeRect(rect);
            this.IsContentReady = false;
        }


    }
}