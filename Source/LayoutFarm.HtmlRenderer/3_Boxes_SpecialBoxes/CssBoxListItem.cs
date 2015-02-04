﻿//BSD 2014-2015,WinterDev

using System;
using System.Collections.Generic;
using PixelFarm.Drawing;

namespace LayoutFarm.HtmlBoxes
{

    public class CssBoxListItem : CssBox
    {
        CssBox _listItemBulletBox;
        public CssBoxListItem(object controller, Css.BoxSpec spec, RootGraphic rootgfx)
            : base(controller, spec, rootgfx)
        {
        }
        public CssBox BulletBox
        {
            get
            {
                return this._listItemBulletBox;
            }
            set
            {
                this._listItemBulletBox = value;
            }
        }
        protected override void PerformContentLayout(LayoutVisitor lay)
        {

            base.PerformContentLayout(lay);

            if (_listItemBulletBox != null)
            {
                //layout list item
                var prevSibling = lay.LatestSiblingBox;
                lay.LatestSiblingBox = null;//reset
                _listItemBulletBox.PerformLayout(lay);
                lay.LatestSiblingBox = prevSibling;
                var fRun = _listItemBulletBox.FirstRun;
                _listItemBulletBox.FirstRun.SetSize(fRun.Width, fRun.Height);
                _listItemBulletBox.FirstRun.SetLocation(_listItemBulletBox.SizeWidth - 5, this.ActualPaddingTop);
            }
        }
        protected override void PaintImp(PaintVisitor p)
        {
            base.PaintImp(p);

        }
    }
}