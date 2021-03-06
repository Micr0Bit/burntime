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

using Burntime.Platform;
using Burntime.Platform.Graphics;
using Burntime.Framework;
using Burntime.Framework.GUI;
using Burntime.Classic.Logic.Generation;

namespace Burntime.Classic
{
    public class OptionsScene : Scene
    {
        GuiFont disabled;
        GuiFont red;
        GuiFont hover;
        GuiFont hoverRed;
        GuiFont green;
        SavegameInputWindow input;
        Button load;
        Button save;
        Button delete;
        Button music;

        Button[] savegames = new Button[8];

        public OptionsScene(Module App)
            : base(App)
        {
            Background = "opti.pac";
            Music = "16_MUS 16_HSC.ogg";
            Position = (app.Engine.GameResolution - new Vector2(320, 200)) / 2;

            disabled = new GuiFont(BurntimeClassic.FontName, new PixelColor(100, 100, 100));
            red = new GuiFont(BurntimeClassic.FontName, new PixelColor(164, 0, 0));
            hover = new GuiFont(BurntimeClassic.FontName, new PixelColor(108, 116, 168));
            hoverRed = new GuiFont(BurntimeClassic.FontName, new PixelColor(240, 64, 56));
            green = new GuiFont(BurntimeClassic.FontName, new PixelColor(0, 108, 0));

            Image image = new Image(App);
            image.Background = "opt.ani";
            image.Position = new Vector2(0, 4);
            Windows += image;

            // menu buttons
            Button button = new Button(App);
            button.Font = red;
            button.HoverFont = hover;
            button.Text = "@burn?388";
            button.Position = new Vector2(212, 61);
            button.SetTextOnly();
            button.Command += app.SceneManager.PreviousScene;
            Windows += button;
            button = new Button(App);
            if (BurntimeClassic.Instance.DisableMusic)
            {
                button.Font = disabled;
                button.Text = "@burn?389";
            }
            else
            {
                button.Font = red;
                button.HoverFont = hover;
                button.Text = BurntimeClassic.Instance.MusicPlayback ? "@burn?389" : "@burn?424";
                button.Command += OnButtonMusicSwitch;
            }
            button.Position = new Vector2(212, 81);
            button.SetTextOnly();
            music = button;
            Windows += button;
            button = new Button(App);
            button.Font = red;
            button.HoverFont = hover;
            button.Text = "@burn?390";
            button.Position = new Vector2(212, 102);
            button.SetTextOnly();
            button.Command += OnButtonRestart;
            Windows += button;
            button = new Button(App);
            button.Font = disabled;
            button.Text = "@burn?392";
            button.Position = new Vector2(212, 124);
            button.SetTextOnly();
            Windows += button;
            button = new Button(App);
            button.Font = red;
            button.HoverFont = hover;
            button.Text = "@burn?391";
            button.Position = new Vector2(212, 145);
            button.SetTextOnly();
            button.Command += OnButtonExit;
            Windows += button;

            // save buttons
            load = new Button(App);
            load.Font = hover;
            load.HoverFont = hoverRed;
            load.Text = "@burn?382";
            load.Position = new Vector2(40, 122);
            load.SetTextOnly();
            load.Command += OnLoad;
            Windows += load;
            save = new Button(App);
            save.Font = hover;
            save.HoverFont = hoverRed;
            save.Text = "@burn?383";
            save.Position = new Vector2(74, 122);
            save.SetTextOnly();
            save.Command += OnSave;
            Windows += save;
            delete = new Button(App);
            delete.Font = hover;
            delete.HoverFont = hoverRed;
            delete.Text = "@burn?384";
            delete.Position = new Vector2(126, 122);
            delete.SetTextOnly();
            delete.Command += OnDelete;
            Windows += delete;

            // savegame name input
            input = new SavegameInputWindow(App);
            input.Font = hover;
            input.Position = new Vector2(40, 108);
            input.Size = new Vector2(120, 10);
            Windows += input;

            // radio cover
            button = new Button(App);
            button.Image = "opta.raw?0";
            button.HoverImage = "opta.raw?1";
            button.Position = new Vector2(186, 51);
            button.Layer+=2;
            Windows += button;

            CreateSaveGameButtons();
        }

        protected override void OnActivateScene(object parameter)
        {
            RefreshSaveGames();
            input.Name = "";
        }

        void RefreshSaveGames()
        {
            string[] files = Burntime.Platform.IO.FileSystem.GetFileNames("savegame/", ".sav");

            for (int i = 0; i < 8; i++)
            {
                if (files.Length > i)
                {
                    SaveGame game = new SaveGame("savegame/" + files[i]);

                    savegames[i].Text = files[i].ToUpper();
                    savegames[i].Font = game.Version == BurntimeClassic.SavegameVersion ? green : disabled;
                    savegames[i].SetTextOnly();

                    game.Close();
                }
                else
                    savegames[i].Text = "";
            }
        }

        void CreateSaveGameButtons()
        {
            for (int i = 0; i < 8; i++)
            {
                int y = i % 4;
                int x = (i - i % 4) / 4;

                x = 38 + x * 67;
                y = 58 + y * 10;

                savegames[i] = new Button(app);
                savegames[i].Position = new Vector2(x, y);
                savegames[i].Text = "";
                savegames[i].Font = green;
                savegames[i].HoverFont = hoverRed;
                savegames[i].SetTextOnly();
                savegames[i].Command += new CommandHandler(OnSelect, i);
                Windows += savegames[i];
            }
        }

        public override void OnUpdate(float Elapsed)
        {
            if (load.IsHover)
                input.Mode = SavegameMode.Load;
            else if (save.IsHover)
                input.Mode = SavegameMode.Save;
            else if (delete.IsHover)
                input.Mode = SavegameMode.Delete;
            else
                input.Mode = SavegameMode.None;
        }

        void OnSelect(int index)
        {
            string str = savegames[index].Text;
            input.Name = str.Substring(0, str.Length - 4);
        }

        void OnSave()
        {
            if (input.Name == "")
                return;

            if (app.Server != null && app.Server.StateContainer != null)
            {
                GameCreation creation = new GameCreation(app as BurntimeClassic);
                creation.SaveGame("savegame/" + input.Name + ".sav");
            }

            input.Name = "";
            RefreshSaveGames();
        }

        void OnLoad()
        {
            if (Burntime.Platform.IO.FileSystem.ExistsFile("savegame/" + input.Name + ".sav"))
            {
                app.SceneManager.SetScene("WaitScene");
                app.SceneManager.BlockBlendIn();

                GameCreation creation = new GameCreation(app as BurntimeClassic);
                if (!creation.LoadGame("savegame/" + input.Name + ".sav"))
                    app.SceneManager.PreviousScene();

                app.SceneManager.UnblockBlendIn();
            }

            input.Name = "";
            RefreshSaveGames();
        }

        void OnDelete()
        {
            if (Burntime.Platform.IO.FileSystem.ExistsFile("savegame/" + input.Name + ".sav"))
                Burntime.Platform.IO.FileSystem.RemoveFile("savegame/" + input.Name + ".sav");

            input.Name = "";
            RefreshSaveGames();
        }

        void OnButtonMusicSwitch()
        {
            if (!BurntimeClassic.Instance.DisableMusic)
            {
                BurntimeClassic.Instance.MusicPlayback = !BurntimeClassic.Instance.MusicPlayback;
                music.Text = BurntimeClassic.Instance.MusicPlayback ? "@burn?389" : "@burn?424";
                music.SetTextOnly();
                if (BurntimeClassic.Instance.MusicPlayback)
                {
                    // start music
                    app.Engine.Music.Enabled = true;
                    app.Engine.Music.Play(Music);
                }
                else
                {
                    // stop music
                    app.Engine.Music.Enabled = false;
                    app.Engine.Music.Stop();
                }
            }
        }

        void OnButtonRestart()
        {
            app.StopGame();
            app.SceneManager.SetScene("MenuScene");
        }

        void OnButtonExit()
        {
            app.Close();
        }
    }
}
