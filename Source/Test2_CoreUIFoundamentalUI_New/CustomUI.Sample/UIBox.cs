﻿//2014 Apache2, WinterDev
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using LayoutFarm.Text;
using LayoutFarm.UI;

namespace LayoutFarm.SampleControls
{
    public abstract class UIBox : UIElement
    {
        int _left;
        int _top;
        int _width;
        int _height;

        public event EventHandler<UIMouseEventArgs> MouseDown;

        public UIBox(int width, int height)
        {
            this._width = width;
            this._height = height;
        }
        public void SetLocation(int left, int top)
        {
            this._left = left;
            this._top = top;

            if (this.HasReadyRenderElement)
            {
                RenderElement.DirectSetVisualElementLocation(
                    this.CurrentPrimaryRenderElement,
                    _left,
                    _top);
            }
        }
        public void SetSize(int width, int height)
        {
            this._width = width;
            this._height = height;

            if (this.HasReadyRenderElement)
            {
                RenderElement.DirectSetVisualElementSize(
                   this.CurrentPrimaryRenderElement,
                   _width,
                   _height);
            }
        }
        public void SetBound(int left, int top, int width, int height)
        {
            this._left = left;
            this._top = top;
            this._width = width;
            this._height = height;

            if (this.HasReadyRenderElement)
            {
                RenderElement.DirectSetVisualElementLocation(
                    this.CurrentPrimaryRenderElement,
                    _left,
                    _top);

                RenderElement.DirectSetVisualElementSize(
                    this.CurrentPrimaryRenderElement,
                    _width,
                    _height);
            }
        }
        protected override void OnMouseDown(UIMouseEventArgs e)
        {
            if (this.MouseDown != null)
            {
                this.MouseDown(this, e);
            }
        }
        protected abstract RenderElement CurrentPrimaryRenderElement
        {
            get;
        }
        protected abstract bool HasReadyRenderElement
        {
            get;
        }
        public int Left
        {
            get
            {

                if (this.HasReadyRenderElement)
                {
                    return this.CurrentPrimaryRenderElement.X;
                }
                else
                {
                    return this._left;
                }
            }
        }
        public int Top
        {
            get
            {
                if (this.HasReadyRenderElement)
                {
                    return this.CurrentPrimaryRenderElement.Y;
                }
                else
                {
                    return this._top;
                }
            }
        }
        public int Width
        {
            get
            {
                if (this.HasReadyRenderElement)
                {
                    return this.CurrentPrimaryRenderElement.Width;
                }
                else
                {
                    return this._width;
                }
            }
        }
        public int Height
        {
            get
            {
                if (this.HasReadyRenderElement)
                {
                    return this.CurrentPrimaryRenderElement.Height;
                }
                else
                {
                    return this._height;
                }
            }
        }

        public override void InvalidateGraphic()
        {
            if (this.HasReadyRenderElement)
            {
                this.CurrentPrimaryRenderElement.InvalidateGraphic();
            }
        }
    }
}