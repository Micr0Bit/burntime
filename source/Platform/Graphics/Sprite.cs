﻿
#region GNU General Public License - Burntime
/*
 *  Burntime
 *  Copyright (C) 2008-2011 Jakob Harder
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
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;

using Burntime.Platform.Resource;

namespace Burntime.Platform.Graphics
{
    // TODO: use gameTime
    [Serializable]
    public class SpriteAnimation
    {
        float frame;
        internal int frameCount;
        float intervalMargin = 0;
        float pause = 0;
        bool endless = true;
        float delay = 0;

        public int Frame
        {
            get { return (int)System.Math.Floor(frame); }
            set { frame = value; }
        }

        public int FrameCount
        {
            get { return frameCount; }
        }

        public float IntervalMargin
        {
            get { return intervalMargin; }
            set { intervalMargin = value; }
        }

        public float Delay
        {
            get { return delay; }
            set { pause = value; delay = value; }
        }

        public float Speed = 5;

        public bool Endless
        {
            get { return endless; }
            set { endless = value; }
        }
        public bool Progressive = true;

        protected bool reverse;
        public bool ReverseAnimation
        {
            set { reverse = value; }
            get { return reverse; }
        }

        //public bool ColorKey
        //{
        //    set
        //    {
        //        for (int i = 0; i < frameCount; i++)
        //        {

        //        }
        //    }
        //    get { if (frameCount <= 0) return false; return frames[0].ColorKey; }
        //}

        protected bool running;
        public void Stop()
        {
            running = false;
        }

        public void Start()
        {
            ended = false;
            running = true;
            pause = delay;
        }

        public void GoLastFrame()
        {
            frame = frameCount - 0.0001f;
        }

        public void GoFirstFrame()
        {
            frame = 0;
        }

        public SpriteAnimation(int FrameCount)
        {
            frameCount = FrameCount;
            frame = 0;
            endless = true;
            running = true;
        }

        bool ended ;
        public bool End
        {
            //get { if (!reverse) return (frame == frameCount - 1); else return (frame == 0); }
            get { return ended; }
        }

        public void Update(float elapsed)
        {
            if (ended || frameCount == 0 || !running)
                return;

            if (pause > 0)
            {
                pause -= elapsed;
                return;
            }

            if (!endless)
            {
                if (!reverse)
                {
                    frame += elapsed * Speed;
                    if (frame > frameCount - 0.0001f)
                    {
                        frame = frameCount - 0.0001f;
                        ended = true;
                        running = false;
                    }
                }
                else
                {
                    frame -= elapsed * Speed;
                    if (frame < 0)
                    {
                        frame = 0;
                        ended = true;
                        running = false;
                    }
                }
            }
            else
            {
                if (!reverse)
                {
                    frame += elapsed * Speed;
                    while (frame >= frameCount)
                    {
                        pause = intervalMargin;
                        frame -= frameCount;
                    }
                }
                else
                {
                    frame -= elapsed * Speed;
                    while (frame < 0)
                    {
                        pause = intervalMargin;
                        frame += frameCount;
                    }
                }
            }
        }
    }

    public class Sprite : DataObject
    {
        public ResourceLoadType LoadType = ResourceLoadType.Now;
        protected ResourceManager resMan;
        protected bool colorKey = true;
        internal SpriteFrame[] internalFrames;
        internal ResourceID id;
        internal SpriteAnimation ani;

        // sprite resolution relative to global resolution
        protected float resolution = 1;
        public float Resolution
        {
            get { return resolution; }
            set { resolution = value; }
        }

        protected Sprite()
        {
        }

        public bool IsLoaded
        {
            get { return (internalFrames != null && internalFrames[0].loaded); }
        }

        public bool IsLoading
        {
            get { return (internalFrames != null && internalFrames[0].loading); }
        }

        public int Width
        {
            get { /*Load();*/ if (internalFrames[0] == null) return 0; else return (int)(Frame.Width * resolution); }
        }
        public int Height
        {
            get { /*Load();*/ if (internalFrames[0] == null) return 0; else return (int)(Frame.Height * resolution); }
        }

        public Vector2 Size
        {
            get { return new Vector2(Width, Height); }
        }

        public Vector2 OriginalSize
        {
            get { if (internalFrames[0] == null) return new Vector2(); return new Vector2(Frame.Width, Frame.Height); }
        }

        public int CurrentFrame
        {
            get { if (ani != null) return ani.Frame; return 0; }
        }

        public bool ColorKey
        {
            get { return colorKey; }
            set { colorKey = value; }
        }

        public ResourceID ID
        {
            get { return id; }
        }

        public SpriteAnimation Animation
        {
            get { return ani; }
        }

        public void CopyTo(Sprite Sprite)
        {
            Sprite.resMan = resMan;
            Sprite.id = id;
            Sprite.internalFrames = internalFrames;
            Sprite.LoadType = LoadType;
            if (ani != null)
                Sprite.ani = new SpriteAnimation(ani.FrameCount);
            Sprite.Resolution = Resolution;
        }

        public Sprite Clone()
        {
            Sprite sprite = new Sprite(resMan, id, internalFrames, ani);
            sprite.resolution = resolution;
            return sprite;
        }

        public void Update(float Elapsed)
        {
            if (ani != null && internalFrames != null)
                ani.Update(Elapsed);
        }

        public void Touch()
        {
            Load();
        }

        // internal access
        internal SpriteFrame Frame
        {
            get 
            { 
                Load();
                if (CurrentFrame >= internalFrames.Length)
                    return internalFrames[0];
                return internalFrames[CurrentFrame]; 
            }
        }

        internal SpriteFrame[] Frames
        {
            get { Load(); return internalFrames; }
        }

        internal Sprite(ResourceManager ResMan, String ID, SpriteFrame Frame)
        {
            id = ID;
            internalFrames = new SpriteFrame[1];
            internalFrames[0] = Frame;
            resMan = ResMan;
        }

        internal Sprite(ResourceManager resMan, String id, SpriteFrame[] frames, SpriteAnimation animation)
        {
            this.id = id;
            internalFrames = frames;
            this.resMan = resMan;
            ani = animation;
        }

        internal void Unload()
        {
            if (internalFrames != null)
            {
                for (int i = 0; i < internalFrames.Length; i++)
                {
                    if (internalFrames[i].Texture != null)
                    {
                        internalFrames[i].Texture.Dispose();
                        internalFrames[i].Texture = null;
                    }
                }

                internalFrames[0].loaded = false;
            }
        }

        internal void Load()
        {
            if (IsLoaded || IsLoading)
                return;

            resMan.Reload(this, ResourceLoadType.Delayed);
        }
    }
}
