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

using Burntime.Platform;
using Burntime.Platform.Graphics;
using Burntime.Framework;
using Burntime.Framework.GUI;
using Burntime.Framework.Network;

namespace Burntime.Classic.Scenes
{
    class WaitScene : Scene
    {
        GuiFont font;
        float timer = 0;
        const float WAIT_DISPLAY_DELAY = 10;

        ITurnNews news;

        public WaitScene(Module App)
            : base(App)
        {
            Size = new Burntime.Platform.Vector2(320, 200);
            Position = (app.Engine.GameResolution - new Vector2(320, 200)) / 2;
            //Background = new SpriteImage(App, "blz.pac");
            font = new GuiFont(BurntimeClassic.FontName, new PixelColor(255, 255, 255));
        }

        public override void OnRender(RenderTarget Target)
        {
            if (timer >= WAIT_DISPLAY_DELAY)
                font.DrawText(Target, new Vector2(160, 100), new GuiString("@newburn?10"), TextAlignment.Center, VerticalTextAlignment.Center);
        }

        public override void OnUpdate(float Elapsed)
        {
            BurntimeClassic classic = app as BurntimeClassic;

            classic.ActiveClient = GameClient.NoClient;

            news = classic.GameServer.PopNews();

            if (news != null)
            {
                if (news is DeathNews)
                    app.SceneManager.SetScene("DeathScene", (news as DeathNews).Name);
                else if (news is VictoryNews)
                    app.SceneManager.SetScene("VictoryScene", (news as VictoryNews));
            }
            else
            {
                bool gameOver = true;

                for (int i = 0; i < classic.Clients.Count; i++)
                {
                    if (classic.Clients[i].IsReady)
                        classic.ActiveClient = classic.Clients[i];
                    gameOver &= (classic.Clients[i].IsGameOver || classic.Clients[i].State == GameClientState.Dead);
                }

                if (gameOver)
                {
                    app.Server.Stop();
                    app.SceneManager.SetScene("MenuScene");
                }
                else if (classic.ActiveClient.IsReady)
                {
                    ClassicGame game = app.GameState as ClassicGame;
                    game.World.ActivePlayer = classic.ActiveClient.Player;

                    if (!game.World.ActivePlayerObj.OnMainMap)
                        app.SceneManager.SetScene("LocationScene");
                    else
                        app.SceneManager.SetScene("MapScene");
                }

                if (timer < WAIT_DISPLAY_DELAY)
                    timer += Elapsed;
            }
        }

        //public override bool OnMouseClick(Vector2 Position, MouseButton Button)
        //{
        //    app.SceneManager.PreviousScene();
        //    return true;
        //}

        protected override void OnActivateScene(object parameter)
        {
            app.RenderMouse = false;
            timer = 0;
        }

        protected override void OnInactivateScene()
        {
            app.RenderMouse = true;
        }
    }
}
