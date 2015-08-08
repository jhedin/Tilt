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
    class EndMenu
    {
        globals g;
        Texture2D background;
        Texture2D point;
        Rectangle mainMenu;
        Counter time;
        Counter deaths;
        Counter score;
        Vector2 timeLoc;
        Vector2 deathsLoc;
        Vector2 scoreLoc;
        TouchCollection touches;
        public EndMenu(globals gIn)
        {
            g = gIn;
            mainMenu = new Rectangle(18+30, 334, 28, 122);
            timeLoc = new Vector2(395,65-30);
            deathsLoc = new Vector2(395, 95-30);
            scoreLoc = new Vector2(395,125-30);
        }
        public void loadBackground(IServiceProvider serviceProvider)
        {
            ContentManager content = new ContentManager(serviceProvider, "Content");
            background = content.Load<Texture2D>("endScreen1");
            point = content.Load<Texture2D>("bottom_bar");
        }
        public void update()
        {
            touches = TouchPanel.GetState();
            if (touches.Count == 1)
            {
                foreach (TouchLocation touch in touches)
                {
                    if (mainMenu.Contains((int)touch.Position.X, (int)touch.Position.Y) && touch.State == TouchLocationState.Pressed)
                    {
                        g.state = GameState.mainMenu;
                        g.player.deathCounter.value = 0;
                        g.player.gameTimer.value = 0;
                        g.map.selectRoom(0);
                    }
                }
            }
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(point, new Rectangle(0, 0, 480, 270), Color.DarkOrange);
            spriteBatch.Draw(background,Vector2.Zero, Color.White);
            
            time = g.player.gameTimer.clone();
            deaths = g.player.deathCounter.clone();
            score = g.player.gameTimer.clone();
            score.origin = scoreLoc;
            time.origin = timeLoc;
            deaths.origin = deathsLoc;
            if (time.value + 10 * deaths.value < 50 * g.map.getCurrentRoom())
            {
                score.value = (uint)(50 * g.map.getCurrentRoom() - time.value - 10 * deaths.value);
            }
            else
            {
                score.value = 0;
            }

            time.Draw(gameTime, spriteBatch);
            deaths.Draw(gameTime, spriteBatch);
            score.Draw(gameTime, spriteBatch);
        }
    }
}