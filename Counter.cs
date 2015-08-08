using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Text;

namespace tilt
{
    class Counter
    {
        public Vector2 origin;//actually, this is the lefthand/top corner
        public uint value;
        private int altVal;//for timed stuff.
        Texture2D font;
        Color fontColor;
        /*Rectangle srcRect;//oh wait... passing pointers, not copies!
        Rectangle destRect;*/
        public Counter clone()
        {
            Counter temp = new Counter(value, origin);
            temp.init(font, fontColor);
            return temp;
        }
        public Counter( uint val, Vector2 org)
        {
            value = val;
            origin = org;
            altVal = 0;
        }
        public void init(Texture2D numbs, Color colour)
        {
            font = numbs;
            fontColor = colour;
        }
        public uint update()
        {
            altVal++;
            if (altVal % 30 == 0)
            {
                value++;
                altVal = 0;
            }
            return value;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //write the number, with the leftmost digit centered on the origin
            uint div = value;
            Stack<uint> digits = new Stack<uint>();
            int count = 0;//count*(7+1) describes the distance between images 7 = width, 1 = spacing
            if (div == 0)
            {
                digits.Push(0); 
            } 
            
            while (div % 10 != 0 || div / 10 != 0)
            {
                digits.Push(div % 10);//what is the current digit?
                div = div / 10;//steps to the left              
            }
            //so, now we have a stack of digits, now to draw in order!
            
            while (digits.Count != 0)
            {
                spriteBatch.Draw(font, new Rectangle((int)origin.X + count * 8, (int)origin.Y+ 5, 7, 14), new Rectangle((int)digits.Pop() * 7 ,0,7,14), fontColor);
                count++;
            }


        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Texture2D pumpkin)
        {
            //write the number, with the leftmost digit centered on the origin
            //-29, -5 // + 11, -5
            uint div = value;
            Stack<uint> digits = new Stack<uint>();
            int count = 0;//count*(7+1) describes the distance between images 7 = width, 1 = spacing
            if (div == 0)
            {
                digits.Push(0);
            }

            while (div % 10 != 0 || div / 10 != 0)
            {
                digits.Push(div % 10);//what is the current digit?
                div = div / 10;//steps to the left              
            }
            //so, now we have a stack of digits, now to draw in order!
            spriteBatch.Draw(pumpkin, origin - new Vector2(29, 0), Color.White);
            while (digits.Count != 0)
            {
                spriteBatch.Draw(font, new Rectangle((int)origin.X + count * 8, (int)origin.Y + 5, 7, 14), new Rectangle((int)digits.Pop() * 7, 0, 7, 14), fontColor);
                count++;
            }
            spriteBatch.Draw(pumpkin, new Vector2(origin.X + count * 8,origin.Y) + new Vector2(3, 0), Color.White);

        }
    }
}