﻿// 2015,2014 ,Apache2, WinterDev
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PixelFarm.Drawing; 
using LayoutFarm.UI;
using LayoutFarm.RenderBoxes;
using LayoutFarm.CustomWidgets;

namespace LayoutFarm.HtmlWidgets
{

    public class ComboBox : UIBox
    {

        CustomRenderBox primElement;//background 
        Color backColor = Color.LightGray;
        bool isOpen;
        //1. land part
        UIBox landPart;

        //2. float part   
        UIBox floatPart;
        RenderElement floatPartRenderElement;
        HingeFloatPartStyle floatPartStyle;

        public ComboBox(int width, int height)
            : base(width, height)
        {

        }

        protected override bool HasReadyRenderElement
        {
            get { return this.primElement != null; }
        }
        protected override RenderElement CurrentPrimaryRenderElement
        {
            get { return this.primElement; }
        }
        public Color BackColor
        {
            get { return this.backColor; }
            set
            {
                this.backColor = value;
                if (HasReadyRenderElement)
                {
                    this.primElement.BackColor = value;
                }
            }
        }
        public override RenderElement GetPrimaryRenderElement(RootGraphic rootgfx)
        {
            if (primElement == null)
            {
                var renderE = new CustomRenderBox(rootgfx, this.Width, this.Height);
                 
                renderE.SetLocation(this.Left, this.Top);
                renderE.BackColor = backColor;
                renderE.SetController(this);
                renderE.HasSpecificSize = true;
                //------------------------------------------------
                //create visual layer
                var layers = new VisualLayerCollection();
                var layer0 = new PlainLayer(renderE);
                layers.AddLayer(layer0);
                renderE.Layers = layers;

                if (this.landPart != null)
                {
                    layer0.AddChild(this.landPart.GetPrimaryRenderElement(rootgfx));
                }
                if (this.floatPart != null)
                {

                } 
                //---------------------------------
                primElement = renderE;
            }
            return primElement;
        }
         
        //----------------------------------------------------  
        public UIBox LandPart
        {
            get { return this.landPart; }
            set
            {
                this.landPart = value;
                if (value != null)
                {
                    //if new value not null
                    //check existing land part
                    if (this.landPart != null)
                    {
                        //remove existing landpart

                    }

                    if (primElement != null)
                    {
                        //add 
                        var visualPlainLayer = primElement.Layers.GetLayer(0) as PlainLayer;
                        if (visualPlainLayer != null)
                        {
                            visualPlainLayer.AddChild(value.GetPrimaryRenderElement(primElement.Root));
                        }

                    }

                }
                else
                {
                    if (this.landPart != null)
                    {
                        //remove existing landpart

                    }
                }
            }
        }
        public UIBox FloatPart
        {
            get { return this.floatPart; }
            set
            {
                this.floatPart = value;
                if (value != null)
                {
                    //attach float part

                }
            }
        }
        //---------------------------------------------------- 
        public bool IsOpen
        {
            get { return this.isOpen; }
        }
        //---------------------------------------------------- 


        public void OpenHinge()
        {
            if (isOpen) return;
            this.isOpen = true;

            //-----------------------------------
            if (this.primElement == null) return;
            if (floatPart == null) return;


            switch (floatPartStyle)
            {
                default:
                case HingeFloatPartStyle.Popup:
                    {
                        //add float part to top window layer
                        var topRenderBox = primElement.GetTopWindowRenderBox();
                        if (topRenderBox != null)
                        {
                            Point globalLocation = primElement.GetGlobalLocation();
                            floatPart.SetLocation(globalLocation.X, globalLocation.Y + primElement.Height);
                            this.floatPartRenderElement = this.floatPart.GetPrimaryRenderElement(primElement.Root);
                            topRenderBox.AddChild(floatPartRenderElement);
                        }

                    } break;
                case HingeFloatPartStyle.Embeded:
                    {

                    } break;
            }
        }
        public void CloseHinge()
        {
            if (!isOpen) return;
            this.isOpen = false;

            if (this.primElement == null) return;
            if (floatPart == null) return;

            switch (floatPartStyle)
            {
                default:
                    {
                    } break;
                case HingeFloatPartStyle.Popup:
                    {
                        //if (floatPartRenderElement != null)
                        //{
                        //    //temp
                        //    var parentContainer = floatPartRenderElement.ParentRenderElement as CustomRenderBox;
                        //    if (parentContainer.Layers != null)
                        //    {
                        //        PlainLayer plainLayer = (PlainLayer)parentContainer.Layers.GetLayer(0);
                        //        plainLayer.RemoveChild(floatPartRenderElement);

                        //    }
                        //}
                        TopWindowRenderBox topRenderBox = primElement.GetTopWindowRenderBox();
                        if (topRenderBox != null)
                        {
                            topRenderBox.Layer0.RemoveChild(floatPartRenderElement);                             
                        }
                    } break;
                case HingeFloatPartStyle.Embeded:
                    {
                    } break;

            }
        }

        public HingeFloatPartStyle FloatPartStyle
        {
            get { return this.floatPartStyle; }
            set
            {
                this.floatPartStyle = value;
            }
        }
    }
}