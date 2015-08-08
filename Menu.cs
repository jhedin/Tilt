using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Text;

namespace tilt
{
    //shows all buttons, when needed/depending on g.gameState
    class Menu
    {
        globals g;
        List<Button> buttons;
        string name;
        Texture2D font;
        Texture2D clear;
        Texture2D locked;

        public Menu(globals gIn)
        {
            buttons = new List<Button>();
            g = gIn;
        }
        public void loadButtons(IServiceProvider serviceProvider)
        {
            ContentManager content = new ContentManager(serviceProvider, "Content");

            clear = content.Load<Texture2D>("cleared");
            locked = content.Load<Texture2D>("locked");
            font = content.Load<Texture2D>("numbersFont");
            fillButtons();
        }
        private void fillButtons()
        {
            int numRooms = g.map.getFilledRooms();
            int roomMax = g.map.getRoomMax();
            bool cleared = false;
            Texture2D isClear;
            int xPos;
            int yPos;
            Button newButton;
            for (int i = 0; i < numRooms; i++)
            {
                if (i <= roomMax)
                {
                    cleared = true;
                    isClear = clear;
                }
                else
                {
                    cleared = false;
                    isClear = locked;
                }
                xPos = i % 6;
                yPos = i / 6;
                newButton = new Button(cleared, i + 1, isClear, font);
                newButton.setLoc(new Vector2(xPos * 70 + 33, yPos * 70 + 10));
                buttons.Add(newButton);

            }
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            
            foreach (Button butt in buttons)
            {
                butt.Draw(gameTime, spriteBatch);
            }
        }
    }
}
