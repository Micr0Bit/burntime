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

using SlimDX;
using Burntime.Platform.IO;
using Burntime.Platform.Graphics;
using Burntime.Platform.Resource;

namespace Burntime.Platform.Resource
{
    struct Replacement
    {
        public String Argument;
        public String Value;
    }

    public enum ResourceLoadType
    {
        Now,
        Delayed,
        LinkOnly
    }

    struct ResourceInfoFont
    {
        public String Name;
        public PixelColor Fore;
        public PixelColor Back;
    }

    public class ResourceManager : IDisposable
    {
        Engine engine;
        
        // global resource register
        Dictionary<String, Sprite> sprites = new Dictionary<String, Sprite>();
        Dictionary<string, DataObject> dataObjects = new Dictionary<string, DataObject>();
        
        // resource processor
        Dictionary<String, ISpriteProcessor> spriteProcessors = new Dictionary<string, ISpriteProcessor>();
        Dictionary<ResourceInfoFont, Font> fonts = new Dictionary<ResourceInfoFont, Font>();
        internal SlimDX.Direct3D9.Texture EmptyTexture;
        
        Dictionary<String, IFontProcessor> fontProcessors = new Dictionary<string, IFontProcessor>();
        Dictionary<String, Type> dataProcessors = new Dictionary<string, Type>();

        DelayLoader delayLoader;
        Encoding encoding;
        ConfigFile replacement;

        public Encoding Encoding
        {
            get { return encoding; }
            set { encoding = value; }
        }

        int memoryPeek;
        int memoryUsage;
        int MemoryUsage
        {
            get { return memoryUsage; }
            set 
            { 
                memoryUsage = value; if (value > memoryPeek) memoryPeek = value;
                Debug.SetInfoMB("sprite memory usage", memoryUsage);
                Debug.SetInfoMB("sprite memory peek", memoryPeek);
            }
        }

        public bool IsLoading
        {
            get { return delayLoader.IsLoading; }
        }

        public ResourceManager(Engine Engine)
        {
            engine = Engine;
            engine.ResourceManager = this;
            AddSpriteProcessor("png", new SpriteProcessorPng());
            AddDataProcessor("png", typeof(SpriteProcessorPng));
            AddSpriteProcessor("pngani", new AniProcessorPng());
            AddDataProcessor("pngani", typeof(AniProcessorPng));
            AddFontProcessor("txt", new FontProcessorTxt());

            memoryUsage = 0;
            Debug.SetInfoMB("sprite memory usage", memoryUsage);
            Debug.SetInfoMB("sprite memory peak", memoryPeek);

            delayLoader = new DelayLoader(Engine);
        }

        public void Run()
        {
            delayLoader.Run();
        }

        public void Dispose()
        {
            ReleaseAll();   

            Log.Info("texture memory peek: " + (memoryPeek / 1024 / 1024).ToString() + " MB");
            delayLoader.Stop();
        }

        public void Reset()
        {
            delayLoader.Reset();
            fonts.Clear();
            sprites.Clear();
        }

        // processor
        public void AddSpriteProcessor(String Extension, ISpriteProcessor Processor)
        {
            spriteProcessors.Add(Extension, Processor);
        }

        public void AddFontProcessor(String Extension, IFontProcessor Processor)
        {
            fontProcessors.Add(Extension, Processor);
        }

        public void AddDataProcessor(String format, Type dataProcessor)
        {
            dataProcessors.Add(format, dataProcessor);
        }

        public IDataProcessor GetDataProcessor(String Format)
        {
            return (IDataProcessor)Activator.CreateInstance(dataProcessors[Format]);
        }

        // common
        internal void ReleaseAll()
        {
            // TODO: critical section
            foreach (Sprite sprite in sprites.Values)
            {
                for (int i = 0; i < sprite.internalFrames.Length; i++)
                {
                    if (sprite.internalFrames[i].texture == null || sprite.internalFrames[i].texture.Disposed)
                        continue;
                    try
                    {
                        SlimDX.Direct3D9.SurfaceDescription desc = sprite.internalFrames[i].texture.GetLevelDescription(0);
                        MemoryUsage -= desc.Width * desc.Height * 4;
                    }
                    catch (Exception)
                    {
                        // TODO make cleaner
                    }
                }

                sprite.Unload();
            }

            MemoryUsage = 0;

            foreach (Font font in fonts.Values)
            {
                if (font.sprite.internalFrames[0].texture == null)
                    continue;

                try
                {
                    SlimDX.Direct3D9.SurfaceDescription desc = font.sprite.internalFrames[0].texture.GetLevelDescription(0);
                    MemoryUsage -= desc.Width * desc.Height * 4;
                }
                catch
                {
                }

                font.sprite.Unload();
            }
        }

        internal void ReloadAll()
        {
        }

        // font
        public Font GetFont(String File, PixelColor ForeColor)
        {
            return GetFont(File, ForeColor, PixelColor.Black);
        }

        public Font GetFont(String File, PixelColor ForeColor, PixelColor BackColor)
        {
            ResourceInfoFont info;
            info.Name = File;
            if (BackColor != PixelColor.Black)
            {
                info.Fore = ForeColor;
                info.Back = BackColor;
            }
            else
            {
                info.Fore = PixelColor.White;
                info.Back = PixelColor.Black;
            }

            Font font = new Font(engine);
            font.Info = new FontInfo();
            font.Info.Font = File.ToLower();
            font.Info.ForeColor = ForeColor;
            font.Info.BackColor = BackColor;
            font.Info.UseBackColor = !(BackColor.a == 255 && BackColor.r == 0 && BackColor.g == 0 && BackColor.b == 0);

            if (fonts.ContainsKey(info))
            {
                font.charInfo = fonts[info].charInfo;
                font.sprite = fonts[info].sprite;
                font.offset = fonts[info].offset;
                font.height = fonts[info].height;
                return font;
            }

            FilePath path = File;
            if (!fontProcessors.ContainsKey(path.Extension))
                return null;

            engine.IncreaseLoadingCount();

            IFontProcessor processor = (IFontProcessor)Activator.CreateInstance(fontProcessors[path.Extension].GetType());
            processor.Process((string)path);
            Log.Debug("load \"" + path.FullPath + "\"");

            SlimDX.Direct3D9.Texture tex = engine.Device.CreateTexture(MakePowerOfTwo(processor.Size.x), MakePowerOfTwo(processor.Size.y));

            SlimDX.Direct3D9.SurfaceDescription desc = tex.GetLevelDescription(0);
            MemoryUsage += desc.Width * desc.Height * 4;

            SlimDX.DataRectangle data = tex.LockRectangle(0, SlimDX.Direct3D9.LockFlags.Discard);
            System.IO.MemoryStream systemCopy = new System.IO.MemoryStream();
            processor.Render(systemCopy, data.Pitch, ForeColor, BackColor);
            processor.Render(data.Data, data.Pitch, ForeColor, BackColor);
            tex.UnlockRectangle(0);

            //// debug output
            //System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(processor.Size.x, processor.Size.y, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            //System.Drawing.Imaging.BitmapData bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, processor.Size.x, processor.Size.y), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            //byte[] bytes = new byte[processor.Size.y * bmpdata.Stride];
            //systemCopy.Seek(0, System.IO.SeekOrigin.Begin);

            //for (int y = 0; y < processor.Size.y; y++)
            //{
            //    systemCopy.Read(bytes, y * bmpdata.Stride, processor.Size.x * 4);
            //}

            //System.Runtime.InteropServices.Marshal.Copy(bytes, 0, bmpdata.Scan0, bytes.Length);

            //bmp.UnlockBits(bmpdata);

            //bmp.Save("c:\\font.png");
            //bmp.Dispose();

            SpriteFrame frame = new SpriteFrame(this, tex, processor.Size, systemCopy.ToArray());
            font.sprite = new Sprite(this, "", frame);
            font.sprite.Resolution = processor.Factor;

            font.charInfo = processor.CharInfo;
            font.offset = processor.Offset;
            font.height = processor.Size.y;

            engine.DecreaseLoadingCount();

            fonts.Add(info, font);
            return font;
        }

        // sprites
        public Sprite GetImage(ResourceID ID)
        {
            return GetSprite(ID, ResourceLoadType.Delayed);
        }

        public Sprite GetImage(ResourceID ID, ResourceLoadType LoadType)
        {
            return GetSprite(ID, LoadType);
        }

        //Sprite GetSprite(string id, int index)
        //{
        //    id = id.ToLower();
        //    return GetSprite(id + "?" + index.ToString());
        //}

        ResourceID GetReplacementID(ResourceID id)
        {
            if (replacement != null)
            {
                string idstring = null;

                if (replacement["replacement"].ContainsKey(id.Format + "@" + id.File))
                {
                    idstring = replacement["replacement"].Get(id.Format + "@" + id.File);
                }
                else if (replacement["replacement"].ContainsKey(id.File))
                {
                    idstring = replacement["replacement"].Get(id.File);
                }

                if (idstring != null)
                {

                    // construct new resource id
                    if (id.IndexProvided)
                    {
                        idstring += "?" + id.Index.ToString();
                        if (id.EndIndex != -1)
                            idstring += "-" + id.EndIndex.ToString();
                        if (!string.IsNullOrEmpty(id.Custom))
                            idstring += "?" + id.Custom;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(id.Custom))
                            idstring += "??" + id.Custom;
                    }

                    return idstring;
                }
            }
            
            return null;
        }

        bool CheckReplacementID(ResourceID id)
        {
            string format = id.Format;
            if (spriteProcessors.ContainsKey(format))
            {
                lock (this)
                {
                    ISpriteProcessor loader = spriteProcessors[format];
                    //loader.IsAvailable(newid);

                    // TODO
                    // for the moment just check the newid.file file existance
                    return FileSystem.ExistsFile(string.Format(id.File, id.Index));
                }
            }

            return false;
        }

        Sprite GetSprite(ResourceID id, ResourceLoadType LoadType)
        {
            float factor = 1;
            ResourceID realid = (string)id;

            // check replacements
            if (replacement != null)
            {
                ResourceID newid = GetReplacementID(id);
                if (newid != null)
                {

                    // if available return newid sprite right away
                    if (sprites.ContainsKey(newid))
                    {
                        return sprites[newid].Clone();
                    }
                    // otherwise check if newid is available, if not fallback to old id
                    else if (CheckReplacementID(newid))
                    {
                        id = newid;
                        factor = replacement[""].GetFloat("sprite_resolution");
                    }
                }
            }

            if (sprites.ContainsKey(id))
            {
                return sprites[id].Clone();
            }
            else
            {
                Sprite s;

                String format = id.Format;

                Log.Debug("load \"" + id + "\"");

                if (!spriteProcessors.ContainsKey(format))
                    return null;

                engine.IncreaseLoadingCount();

                lock (this)
                {
                    ISpriteProcessor loader = GetSpriteProcessor(id, false);
                    if (loader is ISpriteAnimationProcessor)
                    {
                        ISpriteAnimationProcessor loaderAni = (ISpriteAnimationProcessor)loader;

                        loader.Process(id);

                        int count = loaderAni.FrameCount;

                        SpriteFrame[] frames = new SpriteFrame[loaderAni.FrameCount];
                        for (int i = 0; i < loaderAni.FrameCount; i++)
                        {
                            //loaderAni.SetFrame(i);

                            //SlimDX.Direct3D9.Texture tex = new SlimDX.Direct3D9.Texture(engine.Device, loaderAni.FrameSize.x, loaderAni.FrameSize.y, 1,
                            //    SlimDX.Direct3D9.Usage.Dynamic, SlimDX.Direct3D9.Format.A8R8G8B8,
                            //    SlimDX.Direct3D9.Pool.Default);

                            //SlimDX.DataRectangle data = tex.LockRectangle(0, SlimDX.Direct3D9.LockFlags.Discard);
                            //loader.Render(data.Data, data.Pitch);
                            //tex.UnlockRectangle(0);

                            frames[i] = new SpriteFrame(this);
                        }

                        s = new Sprite(this, realid, frames, new SpriteAnimation(loaderAni.FrameCount));
                        s.LoadType = LoadType;
                        s.Resolution = factor;

                        sprites.Add(id, s);
                    }
                    else
                    {
                        //loader.Process(id);

                        //SlimDX.Direct3D9.Texture tex = new SlimDX.Direct3D9.Texture(engine.Device, loader.Size.x, loader.Size.y, 1,
                        //    SlimDX.Direct3D9.Usage.Dynamic, SlimDX.Direct3D9.Format.A8R8G8B8,
                        //    SlimDX.Direct3D9.Pool.Default);

                        //SlimDX.DataRectangle data = tex.LockRectangle(0, SlimDX.Direct3D9.LockFlags.Discard);
                        //loader.Render(data.Data, data.Pitch);
                        //tex.UnlockRectangle(0);

                        SpriteFrame si = new SpriteFrame(this);
                        s = new Sprite(this, realid, si);
                        s.LoadType = LoadType;
                        s.Resolution = factor;

                        sprites.Add(id, s);
                    }
                }

                if (LoadType == ResourceLoadType.Now)
                    Reload(s, ResourceLoadType.Now);

                engine.DecreaseLoadingCount();
                return s;
            }
        }

        #region DataObject accesss
        public DataObject GetData(ResourceID id)
        {
            return GetData(id, ResourceLoadType.Now);
        }

        public DataObject GetData(ResourceID id, ResourceLoadType loadType)
        {
            DataObject obj;
            if (dataObjects.ContainsKey(id))
            {
                obj = dataObjects[id];
            }
            else if (loadType == ResourceLoadType.Now)
            {
                IDataProcessor processor = GetDataProcessor(id.Format);

                engine.IncreaseLoadingCount();
                obj = processor.Process(id, this);
                Log.Debug("load \"" + id + "\"");
                obj.resourceManager = this;
                obj.DataName = id;
                obj.PostProcess();
                engine.DecreaseLoadingCount();

                dataObjects.Add(id, obj);
            }
            else
            {
                obj = new NullDataObject(id, this);
            }

            return obj;
        }

        public void RegisterDataObject(ResourceID id, DataObject obj)
        {
            obj.DataName = id;
            obj.resourceManager = this;

            if (dataObjects.ContainsKey(id))
            {
                Log.Warning("RegisterDataObject: object \"" + id + "\" is already registered!");
                return;
            }

            dataObjects.Add(id, obj);
        }
        #endregion

        internal void Reload(Sprite Sprite, ResourceLoadType LoadType)
        {
            if (Sprite.internalFrames != null)
                Sprite.internalFrames[0].loading = true;

            if (LoadType == ResourceLoadType.Delayed)
            {
                delayLoader.Enqueue(Sprite);
                return;
            }

            for (int i = 0; i < Sprite.internalFrames.Length; i++)
            {
                if (Sprite.internalFrames[i].HasSystemCopy)
                {
                    SlimDX.Direct3D9.Texture tex = engine.Device.CreateTexture(MakePowerOfTwo(Sprite.internalFrames[i].size.x), MakePowerOfTwo(Sprite.internalFrames[i].size.y));
                    SlimDX.Direct3D9.SurfaceDescription desc = tex.GetLevelDescription(0);
                    MemoryUsage += desc.Width * desc.Height * 4;

                    Sprite.internalFrames[i].texture = tex;
                    Sprite.internalFrames[i].RestoreFromSystemCopy();
                    Sprite.internalFrames[i].loading = false;
                    Sprite.internalFrames[i].loaded = true;
                }
            }
 
            ResourceID id = Sprite.ID;
            if (id.File == "" || !spriteProcessors.ContainsKey(id.Format))
                return;

            ResourceID realid = (string)id;

            // check replacements
            if (replacement != null)
            {
                ResourceID newid = GetReplacementID(id);
                if (newid != null)
                {
                    if (CheckReplacementID(newid))
                        id = newid;
                }
            }

            engine.IncreaseLoadingCount();

            ISpriteProcessor loader = GetSpriteProcessor(id, true);
            if (loader is ISpriteAnimationProcessor)
            {
                ISpriteAnimationProcessor loaderAni = (ISpriteAnimationProcessor)loader;
                loader.Process(id);
                Log.Debug("reload \"" + id + "\"");

                for (int i = 0; i < loaderAni.FrameCount; i++)
                {
                    if (!loaderAni.SetFrame(i))
                    {
                        SpriteFrame[] frames = new SpriteFrame[i];
                        for (int j = 0; j < i; j++)
                            frames[j] = Sprite.Frames[j];
                        Sprite.internalFrames = frames;
                        Sprite.ani.frameCount = i;
                        break;
                    }

                    SlimDX.Direct3D9.Texture tex = engine.Device.CreateTexture(MakePowerOfTwo(loaderAni.FrameSize.x), MakePowerOfTwo(loaderAni.FrameSize.y));

                    SlimDX.Direct3D9.SurfaceDescription desc = tex.GetLevelDescription(0);
                    MemoryUsage += desc.Width * desc.Height * 4;

                    SlimDX.DataRectangle data = tex.LockRectangle(0, SlimDX.Direct3D9.LockFlags.Discard);
                    loader.Render(data.Data, data.Pitch);
                    tex.UnlockRectangle(0);

                    Sprite.Frames[i].Texture = tex;
                    Sprite.Frames[i].Width = loaderAni.FrameSize.x;
                    Sprite.Frames[i].Height = loaderAni.FrameSize.y;
                    Sprite.Frames[i].TimeStamp = System.Diagnostics.Stopwatch.GetTimestamp();
                }
            }
            else
            {
                loader.Process(id);
                Log.Debug("reload \"" + id + "\"");

                SlimDX.Direct3D9.Texture tex = engine.Device.CreateTexture(MakePowerOfTwo(loader.Size.x), MakePowerOfTwo(loader.Size.y));

                SlimDX.Direct3D9.SurfaceDescription desc = tex.GetLevelDescription(0);
                MemoryUsage += desc.Width * desc.Height * 4;

                SlimDX.DataRectangle data = tex.LockRectangle(0, SlimDX.Direct3D9.LockFlags.Discard);
                loader.Render(data.Data, data.Pitch);
                tex.UnlockRectangle(0);

                Sprite.Frame.Texture = tex;
                Sprite.Frame.Width = loader.Size.x;
                Sprite.Frame.Height = loader.Size.y;
                Sprite.Frame.TimeStamp = System.Diagnostics.Stopwatch.GetTimestamp();
            }

            if (Sprite.internalFrames != null)
            {
                Sprite.internalFrames[0].loading = false;
                Sprite.internalFrames[0].loaded = true;
            }

            engine.DecreaseLoadingCount();
        }

        private ISpriteProcessor GetSpriteProcessor(ResourceID id, bool ownInstance)
        {

#warning this should be handled differently

            string format = id.Format;
            if (id.Format == "raw" && id.HasMultipleFrames)
            {
                format = "ani";
            }

            if (ownInstance)
                return (ISpriteProcessor)Activator.CreateInstance(spriteProcessors[format].GetType());

            return spriteProcessors[format];
        }

        // from textDB

        Dictionary<String, TextResourceFile> txtDB = new Dictionary<string, TextResourceFile>();
        List<Replacement> listArguments = new List<Replacement>();

        public void AddDB(string filename)
        {
            File file = FileSystem.GetFile(filename + (filename.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase) ? "" : ".txt"));
            file.Encoding = encoding;
            TextResourceFile db = new TextResourceFile(file);
            txtDB.Add(filename, db);
        }

        public void AddArgument(String Argument, int Value)
        {
            AddArgument(Argument, Value.ToString());
        }

        public void AddArgument(String Argument, String Value)
        {
            Replacement repl = new Replacement();
            repl.Argument = Argument;
            repl.Value = Value;
            listArguments.Add(repl);
        }

        public void ClearArguments()
        {
            listArguments.Clear();
        }

        public String ShiftID(String id, int shift)
        {
            String fileName;
            int pos = shift;
            int atmark = id.LastIndexOf('?');
            if (atmark > 0)
            {
                fileName = id.Substring(0, atmark);
                pos += int.Parse(id.Substring(atmark + 1));
            }
            else
                fileName = id;

            return fileName + "?" + pos;
        }

        public String[] GetStrings(String id)
        {
            int atmark = id.LastIndexOf('?');

            string file = id.Substring(0, atmark);

            int index = 0;
            if (id.Substring(atmark + 1).ToLower().StartsWith("s"))
            {
                if (!txtDB.ContainsKey(file))
                    AddDB(file);

                int section = int.Parse(id.Substring(atmark + 2));

                int sections = 0;
                for (int i = 0; i < txtDB[file].Data.Count && sections < section; i++)
                {
                    int f = txtDB[file].Data[i].IndexOf("}#");
                    if (f != -1)
                    {
                        sections++;
                        index = i + 1;
                    }
                }
            }
            else
                index = int.Parse(id.Substring(atmark + 1));

            return GetStrings(file, index);
        }

        public String[] GetStrings(String file, int startIndex)
        {
            if (!txtDB.ContainsKey(file))
                AddDB(file);

            int last = startIndex + 1;

            for (int i = startIndex; i < txtDB[file].Data.Count; i++)
            {
                int f = txtDB[file].Data[i].IndexOf("}#");
                if (f != -1)
                {
                    last = i;
                    break;
                }
            }

            int count = last - startIndex;
            String[] strs = new String[count];
            for (int i = 0; i < count; i++)
            {
                strs[i] = txtDB[file].Data[i + startIndex].Replace("}", "");
            }

            return strs;
        }

        public String GetString(String id)
        {
            if (id.StartsWith("@"))
                id = id.Substring(1);

            int atmark = id.LastIndexOf('?');
            return GetString(id.Substring(0, atmark), int.Parse(id.Substring(atmark + 1)));
        }

        public String GetString(String file, int index)
        {
            if (!txtDB.ContainsKey(file))
                AddDB(file);
            String res = txtDB[file].Data[index];

            if (res.EndsWith("}"))
                return res.Substring(0, res.Length - 1);
            return res;
        }

        public void SetResourceReplacement(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                replacement = null;
            }
            else
            {
                replacement = new ConfigFile();
                replacement.Open(FileSystem.GetFile(file));
            }
        }

        int MakePowerOfTwo(int nValue)
        {
            nValue--;
            int i;
            for (i = 0; nValue != 0; i++)
                nValue >>= 1;
            return 1 << i;
        }

        //public virtual String GetString(TextRegion region, int index)
        //{
        //    String Text = "";
        //    switch (region)
        //    {
        //        case TextRegion.CityNames:
        //        case TextRegion.MainFile:
        //            Text = BurnTxt.Data[index].Replace("}", ""); break;
        //        case TextRegion.EntranceNames:
        //            Text = BurnTxt.Data[index + 660].Replace("}", ""); break;
        //        case TextRegion.MenuStrings:
        //            Text = BurnTxt.Data[index + 350].Replace("}", ""); break;
        //        case TextRegion.StatisticsStrings:
        //            Text = BurnTxt.Data[index + 569].Replace("}", ""); break;
        //        case TextRegion.CharTypes:
        //            Text = BurnTxt.Data[index + 40].Replace("}", ""); break;
        //        case TextRegion.Names:
        //            Text = BurnTxt.Data[index + 170].Replace("}", ""); break;
        //        case TextRegion.Conversation:
        //            Text = BurnTxt.Data[index + 490].Replace("}", ""); break;
        //        case TextRegion.OptionMenu:
        //            Text = BurnTxt.Data[index + 382].Replace("}", ""); break;
        //        default:
        //            Text = BurnTxt.Data[index].Replace("}", ""); break;
        //    }

        //    foreach (Replacement repl in listArguments)
        //        Text = Text.Replace(repl.Argument, repl.Value);

        //    return Text;
        //}

        //public String[] GetGreeting(int FileId)
        //{
        //    return MenTxt[FileId].GetStrings(0);
        //}
    }
}
