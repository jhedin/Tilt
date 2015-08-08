using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Text;

namespace tilt
{
    class Button
    {
        Texture2D image;
        Rectangle boundingRect;
        GameState clickFunction;
        int roomIndex;
        Counter numb;
        public Button(Texture2D img, Rectangle rect, GameState click)
        {
            image = img;
            boundingRect = rect;
            clickFunction = click;
            roomIndex = -1;
        }
        public Button(bool cleared, int index, Texture2D img, Texture2D numbs)
        {
            image = img;
            roomIndex = index;
            Color fontColour;
            boundingRect = new Rectangle(0, 0, 60, 60);
            if (cleared)
            {
                fontColour = Color.Black;
            }
            else
            {
                fontColour = Color.DarkOrange;
            }
            numb = new Counter((uint)index,new Vector2(0,0));
            numb.init(numbs, fontColour);
        }
        
        public void setLoc(Vector2 newLoc)
        {
            boundingRect.X = (int)newLoc.X;
            boundingRect.Y = (int)newLoc.Y;
            if (numb != null)
            {
                numb.origin.X = newLoc.X + 8;
                numb.origin.Y = newLoc.Y + 8;
            }
        }
        /*public GameState clicked()
        {
            return clickFunction;
        }*/
        public uint clicked()
        {
            return numb.value;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(image,new Vector2(boundingRect.X,boundingRect.Y), Color.White);
            if (numb != null)
            {
                numb.Draw(gameTime, spriteBatch);
            }
        }
    }
}
