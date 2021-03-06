﻿/*
 *  Burntime Platform
 *  Copyright (C) 2009
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 *  authors: 
 *    Juernjakob Harder (yn.harada@gmail.com)
 * 
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using SlimDX.Direct3D9;

namespace Burntime.Platform.Graphics
{
    public class RenderTarget
    {
        Rect rc;
        Engine engine;
        internal float elapsed;
        Vector2 offset;

        public float Elapsed
        {
            get { return elapsed; }
        }

        public int Width
        {
            get { return rc.Width; }
        }

        public int Height
        {
            get { return rc.Height; }
        }

        public Vector2 ScreenOffset
        {
            get { return rc.Position; }
        }

        public Vector2 ScreenSize
        {
            get { return engine.MainTarget.Size; }
        }

        public Vector2 Size
        {
            get { return rc.Size; }
        }

        public int Layer
        {
            get { return engine.Layer; }
            set { engine.Layer = value; }
        }

        public float Blend
        {
            set { engine.Device.Blend = value; }
            get { return engine.Device.Blend; }
        }
        internal RenderTarget(Engine engine, Rect rc)
        {
            this.engine = engine;
            this.rc = rc;
        }

        public Vector2 Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        #region DrawSprite methods
        public void DrawSprite(Sprite sprite)
        {
            if (sprite == null)
                return;

            DrawSprite(Vector2.Zero, sprite);
        }

        public void DrawSprite(Vector2 pos, Sprite sprite)
        {
            if (sprite == null)
                return;

            engine.RenderSprite(sprite, pos + rc.Position + offset);
        }

        public void DrawSprite(Vector2 pos, Sprite sprite, float alpha)
        {
            if (sprite == null)
                return;

            engine.RenderSprite(sprite, pos + rc.Position + offset, alpha);
        }

        public void DrawSprite(Vector2 pos, Sprite sprite, Rect srcRect)
        {
            if (sprite == null)
                return;

            engine.RenderSprite(sprite, pos + rc.Position + offset);
        }

        Sprite selImage;
        public void SelectSprite(Sprite sprite)
        {
            selImage = sprite;
        }

        public void DrawSelectedSprite(Vector2 pos, Rect srcRect)
        {
            engine.RenderSprite(selImage, pos + rc.Position + offset, srcRect.Position, srcRect.Width, srcRect.Height);
        }

        public void DrawSelectedSprite(Vector2 pos, Rect srcRect, PixelColor color)
        {
            engine.RenderSprite(selImage, pos + rc.Position + offset, srcRect.Position, srcRect.Width, srcRect.Height, color.ToInt());
        }
        #endregion

        public void RenderRect(Vector2 pos, Vector2 size, PixelColor color)
        {
            engine.RenderRect(pos + rc.Position + offset, size, color.ToInt());
        }

        public void RenderFrame(Vector2 pos, Vector2 size, PixelColor color)
        {
            engine.RenderFrame(pos + rc.Position + offset, size, color.ToInt());
        }

        public void RenderLine(Vector2 start, Vector2 end, PixelColor color)
        {
            engine.RenderLine(start + rc.Position + offset, end + rc.Position + offset, color.ToInt());
        }

        public RenderTarget GetSubBuffer(Rect rc)
        {
            RenderTarget target = new RenderTarget(engine, new Rect(this.rc.Position + rc.Position, rc.Size));
            target.elapsed = elapsed;
            target.offset = offset;
            return target;
        }
    }
}
