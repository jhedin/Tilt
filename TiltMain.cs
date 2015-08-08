using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace tilt
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class TiltMain : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private RenderTarget2D renderTarget;
        private StorageDevice storageDevice;
        private XmlSerializer xmlSerializer;
        globals g;
        private AccelerometerState accelState;
       /* Counter myCounter;
        Texture2D font;*/
        PauseMenu paws;
        MainMenu mainMenu;
        EndMenu endMenu;
        HighScores highScores;
        InstructionsMenu instructions;

        SaveData saveData;

        public TiltMain()
        {
            g = new globals();
            g.gravity = Direction.right;//what side is up. ->Don't start a room unless the direction matches that of the player.
                  
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 480,
                PreferredBackBufferHeight = 272
            };
            IAsyncResult r = Guide.BeginShowStorageDeviceSelector(null, null);
            while (!r.IsCompleted) { }
            storageDevice = Guide.EndShowStorageDeviceSelector(r);

            // Frame rate is 30 fps by default for Zune.
            TargetElapsedTime = TimeSpan.FromSeconds(1 / 30.0);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            g.state = GameState.init;
            String path = "Content/tilt_1.txt";
            path = Path.Combine(StorageContainer.TitleLocation, path);
            MapParser newMap;
            if (File.Exists(path))
            {
                 newMap = new MapParser(path);
            }
            else
            {
                throw new Exception("No levels found.");
            }

            path = "Content/" + newMap.backgroundName + "_key.key";
            path = Path.Combine(StorageContainer.TitleLocation, path);
            if (File.Exists(path))
            {
                newMap.parseTilesetData(path);
            }
            else
            {
                throw new Exception("Tileset not found.");
            }
            g.map = new Map(StorageContainer.TitleLocation, newMap,g);
            
            path = "Content/";
            path = Path.Combine(StorageContainer.TitleLocation, path);
            /*if (File.Exists(path))
            {
                g.player = new Player(path, g);
            }
            else
            {
                throw new Exception("Player Tileset missing.");
            }*/
            g.player = new Player(path, g);
            g.highScores = new List<uint>();
            g.map.selectRoom(0);
            paws = new PauseMenu(g);
            mainMenu = new MainMenu(g);
            endMenu = new EndMenu(g);
            highScores = new HighScores(g);
            instructions = new InstructionsMenu(g);

            g.state = GameState.mainMenu;

            saveData = new SaveData();
            xmlSerializer = new XmlSerializer(typeof(SaveData));


            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            renderTarget = new RenderTarget2D(GraphicsDevice, 480, 272, 1, SurfaceFormat.Color);
            spriteBatch = new SpriteBatch(GraphicsDevice);
            String path = g.map.getTileSetName();
            g.map.loadTileSet(Services, path);
            g.player.loadTileSet(Services);
            paws.loadBackground(Services);
            mainMenu.loadBackground(Services);
            endMenu.loadBackground(Services);
            highScores.loadBackground(Services);
            instructions.loadBackground(Services);
            load();
        }
        protected override void UnloadContent()
        {
            save();
            g.map = null;
            g.player = null;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            /*if (g.state == GameState.endGame)
            {
                this.Exit();
            }*/

            switch (g.state)
            {
                case GameState.play:
                    //set direction, if need be.
                    accelState = Accelerometer.GetState();
                    if (accelState.Acceleration.X >= 0.35 && (accelState.Acceleration.Y > -0.5 && accelState.Acceleration.Y < 0.5) && (accelState.Acceleration.Z > -0.85 && accelState.Acceleration.Z < 0.85))
                        g.gravity = Direction.left;
                    else if (accelState.Acceleration.Y >= 0.35 && (accelState.Acceleration.X > -0.5 && accelState.Acceleration.X < 0.5) && (accelState.Acceleration.Z > -0.85 && accelState.Acceleration.Z < 0.85))
                        g.gravity = Direction.down;
                    else if (accelState.Acceleration.X < -0.35 && (accelState.Acceleration.Y > -0.5 && accelState.Acceleration.Y < 0.5) && (accelState.Acceleration.Z > -0.85 && accelState.Acceleration.Z < 0.85))
                        g.gravity = Direction.right;
                    else if (accelState.Acceleration.Y < -0.35 && (accelState.Acceleration.X > -0.5 && accelState.Acceleration.X < 0.5) && (accelState.Acceleration.Z > -0.85 && accelState.Acceleration.Z < 0.85))
                        g.gravity = Direction.up;
                    g.player.update();
                    break;
                case GameState.paws:
                    paws.update();
                    break;
                case GameState.mainMenu:
                    mainMenu.update();
                    break;
                case GameState.endScreen:
                    endMenu.update();
                    break;
                case GameState.highScores:
                    highScores.update();
                    break;
                case GameState.instructions:
                    instructions.update();
                    break;
                default:
                    break;
            }
            base.Update(gameTime);
        }

        // override BeginDraw so we can set the render target and Viewport before  
        // any game drawing occurs.  
        protected override bool BeginDraw()
        {
            if (base.BeginDraw())
            {
                GraphicsDevice.SetRenderTarget(0, renderTarget);
                GraphicsDevice.Viewport = new Viewport
                {
                    X = 0,
                    Y = 0,
                    Width = 480,
                    Height = 272,
                    MinDepth = GraphicsDevice.Viewport.MinDepth,
                    MaxDepth = GraphicsDevice.Viewport.MaxDepth
                };
                return true;
            }
            return false;
        }

        // override EndDraw to handle unsetting the render target, resetting the Viewport,  
        // and drawing the render target's contents to the screen  
        protected override void EndDraw()
        {
            GraphicsDevice.SetRenderTarget(0, null);
            GraphicsDevice.Viewport = new Viewport
            {
                X = 0,
                Y = 0,
                Width = 272,
                Height = 480,
                MinDepth = GraphicsDevice.Viewport.MinDepth,
                MaxDepth = GraphicsDevice.Viewport.MaxDepth
            };

            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            spriteBatch.Draw(
                renderTarget.GetTexture(),
                new Vector2(136f, 240f),
                null,
                Color.White,
                MathHelper.PiOver2,
                new Vector2(240f, 136f),
                1f,
                SpriteEffects.None,
                0);
            spriteBatch.End();

            base.EndDraw();
        } 
        
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            switch (g.state)
            {
                case GameState.play:
                    GraphicsDevice.Clear(Color.Black);
                    
                    spriteBatch.Begin();                    
                    g.map.Draw(gameTime, spriteBatch);
                    g.player.Draw(gameTime, spriteBatch);
                    base.Draw(gameTime);

                    spriteBatch.End();
                    break;
                case GameState.paws:
                    GraphicsDevice.Clear(Color.Black);

                    spriteBatch.Begin();
                    g.map.Draw(gameTime, spriteBatch);
                    g.player.Draw(gameTime, spriteBatch);
                    paws.Draw(gameTime, spriteBatch);
                    spriteBatch.End();
                    break;
                case GameState.mainMenu:
                    GraphicsDevice.Clear(Color.Black);

                    spriteBatch.Begin();
                    mainMenu.Draw(gameTime, spriteBatch);
                    spriteBatch.End();
                    break;
                case GameState.endScreen:
                    GraphicsDevice.Clear(Color.Black);

                    spriteBatch.Begin();
                    endMenu.Draw(gameTime, spriteBatch);
                    spriteBatch.End();
                    break;
                case GameState.highScores:
                    GraphicsDevice.Clear(Color.Black);

                    spriteBatch.Begin();
                    highScores.Draw(gameTime, spriteBatch);
                    spriteBatch.End();
                    break;
                case GameState.instructions:
                    GraphicsDevice.Clear(Color.Black);

                    spriteBatch.Begin();
                    instructions.Draw(gameTime, spriteBatch);
                    spriteBatch.End();
                    break;
                default:
                    break;
            }
        }

        void save()
        {
            g.addToSaveData(saveData);
            StorageContainer container = storageDevice.OpenContainer("Tilt");
            string file = Path.Combine(container.Path, "tiltSave.sav");
            FileStream stream = File.Open(file, FileMode.Create);
            xmlSerializer = new XmlSerializer(typeof(SaveData));
            xmlSerializer.Serialize(stream, saveData);
            stream.Close();
            container.Dispose();
        }
        void load()
        {
            StorageContainer container = storageDevice.OpenContainer("Tilt");
            string file = Path.Combine(container.Path, "tiltSave.sav");
            if (File.Exists(file))
            {
                FileStream fileStream = File.Open(file, FileMode.Open, FileAccess.Read);
                saveData = (SaveData)xmlSerializer.Deserialize(fileStream);
                g.loadFromSaveData(saveData);
                fileStream.Close();
                container.Dispose();
            }
        }
    }
}