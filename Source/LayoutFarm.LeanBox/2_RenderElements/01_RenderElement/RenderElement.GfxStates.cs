﻿//2014 Apache2, WinterDev
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace LayoutFarm.Presentation
{
    partial class RenderElement
    {

        
      


        public bool TransparentForAllEvents
        {
            get
            {
                return (uiFlags & TRANSPARENT_FOR_ALL_EVENTS) != 0;

            }
            set
            {
                if (value)
                {
                    uiFlags |= TRANSPARENT_FOR_ALL_EVENTS;
                }
                else
                {
                    uiFlags &= ~TRANSPARENT_FOR_ALL_EVENTS;
                }

            }
        }

    }
}