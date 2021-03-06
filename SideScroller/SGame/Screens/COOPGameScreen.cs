﻿using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using Penumbra;
using RPG2D.GameEngine;
using RPG2D.GameEngine.Screens;
using RPG2D.GameEngine.UI;
using RPG2D.GameEngine.World;
using RPG2D.SGame.Player;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPG2D.SGame.Screens
{
    public class COOPGameScreen : IGameScreen
    {
        Bag<UIElement> UI = new Bag<UIElement>();
        NetPeerConfiguration config;
        NetClient client;
        FrameCounter fpsCounter;

        string ip, name = "";
        NetworkPlayer player;
        double fadeInTimer = 0;
        int fadeInAlpha = 255;
        private Hull playerHull;
        private PointLight mouseLight;

        public COOPGameScreen(string ip, string name, string xml)
        {
            this.ip = ip;
            this.name = name;
            File.WriteAllText("globe.xml", xml);
            GameManager.Game.World = new World("globe.xml", false);
        }

        public void Init(ContentManager content)
        {
            GameManager.Game.Camera = new Camera2D(GameManager.Game.GraphicsDevice);
            GameManager.Game.Player = new Player.Player();
            GameManager.Game.Player.Init(content);
            fpsCounter = new FrameCounter();

            GameManager.Game.Camera.ZoomIn(1);

            ConnectToServer(ip);
            GameManager.Game.NetworkParser = new NetworkParser(client);
            GameManager.Game.NetworkParser.IP = ip;
            player = new NetworkPlayer();

            GameManager.Game.Inventory = new UI.InventoryUI();
            GameManager.Game.Stats = new UI.StatsOverlay();
            UI.Add(GameManager.Game.Stats);
            UI.Add(GameManager.Game.Inventory);

            playerHull = new Hull(new Vector2(12, 0), new Vector2(12, 55), new Vector2(14, 58), new Vector2(16, 59), new Vector2(20, 59), new Vector2(26, 56), new Vector2(37, 56), new Vector2(43, 59), new Vector2(47, 59), new Vector2(49, 58), new Vector2(51, 55), new Vector2(51, 0))
            {
                Scale = new Vector2(1),
                Origin = new Vector2(32)
            };


            mouseLight = new PointLight();
            mouseLight.CastsShadows = true;
            mouseLight.ShadowType = ShadowType.Solid;
            mouseLight.Scale = new Vector2(100);
            mouseLight.Intensity = 0.5f;
            GameManager.Game.Penumbra.Hulls.Add(playerHull);
            GameManager.Game.Penumbra.Lights.Add(mouseLight);
        }

        public void ConnectToServer(string ip)
        {
            config = new NetPeerConfiguration("RPG2D");
            client = new NetClient(config);
            client.Start();

            int port = 20666;
            client.Connect(ip, port);
        }

        public void Update(GameTime gameTime)
        {
            fadeInTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if (fadeInTimer > 0.10d && fadeInAlpha > 0)
                fadeInAlpha -= 1;

            GameManager.Game.Player.Update(gameTime);

            GameManager.Game.Camera.LookAt(GameManager.Game.Player.Position + (GameManager.Game.Player.Size / 2));
            GameManager.Game.Penumbra.Transform = GameManager.Game.Camera.GetViewMatrix();
            mouseLight.Position = GameManager.Game.Camera.ScreenToWorld(Mouse.GetState().Position.ToVector2());
            playerHull.Position = GameManager.Game.Camera.ScreenToWorld(GameManager.Game.ScreenSize / 2);

            GameManager.Game.World.Update(gameTime);

            GameManager.Game.NetworkParser.Update(gameTime);
            player.Update(gameTime);

            foreach (var ui in UI)
            {
                ui.Update(gameTime);
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            GameManager.Game.Penumbra.BeginDraw();
            GameManager.Game.GraphicsDevice.Clear(new Color(28, 17, 23));


            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            fpsCounter.Update(deltaTime);

            spriteBatch.Begin(samplerState: SamplerState.PointWrap, transformMatrix: GameManager.Game.Camera.GetViewMatrix());


            var fps = string.Format("FPS: {0}", fpsCounter.AverageFramesPerSecond);
            GameManager.Game.World.Draw(gameTime, spriteBatch);
            spriteBatch.DrawString(GlobalAssets.Arial12, name, new Vector2(player.X - 16 + (GlobalAssets.Arial12.MeasureString(name).X / 2), player.Y - GlobalAssets.Arial12.MeasureString(name).Y), Color.Black);
            player.Draw(gameTime, spriteBatch);

            spriteBatch.End();


            GameManager.Game.Penumbra.Draw(gameTime);

            spriteBatch.Begin(samplerState: SamplerState.PointWrap);

            foreach (var ui in UI)
            {
                ui.Draw(gameTime, spriteBatch);
            }

            spriteBatch.DrawString(GlobalAssets.Arial12, fps, new Vector2(1, 1), Color.White);

            spriteBatch.DrawString(GlobalAssets.Arial24, GameManager.Game.Tooltip, new Vector2(GameManager.Game.ScreenSize.X / 2 - GlobalAssets.Arial24.MeasureString(GameManager.Game.Tooltip).X / 2, GameManager.Game.ScreenSize.Y - (GlobalAssets.Arial24.MeasureString(GameManager.Game.Tooltip).Y) - 75), Color.White);
            spriteBatch.Draw(GameManager.Black, new Rectangle(0, 0, (int)GameManager.Game.ScreenSize.X, (int)GameManager.Game.ScreenSize.Y), new Color(Color.Black, fadeInAlpha));

            spriteBatch.End();
        }
    }
}
