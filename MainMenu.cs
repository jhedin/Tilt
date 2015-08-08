using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace tilt
{
    class MainMenu
    {
        globals g;
        Texture2D background;
        Texture2D point;
        Rectangle play;
        Rectangle cont;
        Rectangle instructions;
        Rectangle highScores;
        TouchCollection touches;
        public MainMenu(globals gIn)
        {
            g = gIn;
            cont = new Rectangle(76, 345, 28, 122);
            play = new Rectangle(107, 345, 28, 122);
            instructions = new Rectangle(44, 345, 28, 122);
            highScores = new Rectangle(12, 345, 28, 122);
        }
        public void loadBackground(IServiceProvider serviceProvider)
        {
            ContentManager content = new ContentManager(serviceProvider, "Content");
            background = content.Load<Texture2D>("MenuBack");
            point = content.Load<Texture2D>("bottom_bar");
        }
        public void update()
        {

            touches = TouchPanel.GetState();
            if (touches.Count != 3)
            {
                foreach (TouchLocation touch in touches)
                {
                    if (play.Contains((int)touch.Position.X, (int)touch.Position.Y) && touch.State == TouchLocationState.Pressed)
                    {
                        g.player.gameTimer.value = 0;
                        g.player.deathCounter.value = 0;
                        g.map.selectRoom(0);
                        g.state = GameState.play;
                    }
                    else if (cont.Contains((int)touch.Position.X, (int)touch.Position.Y) && touch.State == TouchLocationState.Pressed)
                    {
                        g.state = GameState.play;
                        g.player.stop();
                    }
                    else if (highScores.Contains((int)touch.Position.X, (int)touch.Position.Y) && touch.State == TouchLocationState.Pressed)
                    {
                        g.state = GameState.highScores;
                    }
                    else if (instructions.Contains((int)touch.Position.X, (int)touch.Position.Y) && touch.State == TouchLocationState.Pressed)
                    {
                        g.state = GameState.instructions;
                    }
                }
            }
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(point, new Rectangle(0, 0, 480, 270), Color.DarkOrange);
            spriteBatch.Draw(background,Vector2.Zero, Color.White);
            
        }
    }
}