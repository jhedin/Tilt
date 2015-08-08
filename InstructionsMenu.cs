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
    class InstructionsMenu
    {
        globals g;
        Texture2D background;
        Texture2D point;
        Rectangle mainMenu;
        TouchCollection touches;
        public InstructionsMenu(globals gIn)
        {
            g = gIn;
            mainMenu = new Rectangle(18, 334, 28, 122);
        }
        public void loadBackground(IServiceProvider serviceProvider)
        {
            ContentManager content = new ContentManager(serviceProvider, "Content");
            background = content.Load<Texture2D>("instructions");
            point = content.Load<Texture2D>("bottom_bar");
        }
        public void update()
        {
            touches = TouchPanel.GetState();
            if (touches.Count <= 2)
            {
                foreach (TouchLocation touch in touches)
                {
                    if (mainMenu.Contains((int)touch.Position.X, (int)touch.Position.Y) && touch.State == TouchLocationState.Pressed)
                    {
                        g.state = GameState.mainMenu;
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