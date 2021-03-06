﻿using System;

using Burntime.Platform;
using Burntime.Platform.Graphics;
using Burntime.Platform.Resource;

namespace Burntime.Framework.GUI
{
    public class GuiImage : Sprite
    {
        public static implicit operator GuiImage(ResourceID id)
        {
            return new GuiImage(Module.Instance.ResourceManager.GetImage(id, ResourceLoadType.Delayed));
        }

        public static implicit operator GuiImage(string id)
        {
            return new GuiImage(Module.Instance.ResourceManager.GetImage(id, ResourceLoadType.Delayed));
        }

        public GuiImage(Sprite sprite)
        {
            sprite.CopyTo(this);
        }
    }
}
