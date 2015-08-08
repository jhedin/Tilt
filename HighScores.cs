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
    class HighScores
    {
        globals g;
        Texture2D background1;
        Texture2D background2;
        Texture2D point;
        Texture2D pumpkin;
        Texture2D font;
        Rectangle mainMenu;
        Counter score;
        Vector2 counterStartLoc;
        TouchCollection touches;
        List<Vector2> pumpkinLocs;
        public HighScores(globals gIn)
        {
            g = gIn;
            mainMenu = new Rectangle(18, 334, 28, 122);
            counterStartLoc = new Vector2(275, 50);
            score = new Counter(0, counterStartLoc);
            initPumpkinLocs();
        }
        public void initPumpkinLocs()
        {
            pumpkinLocs = new List<Vector2>();
            pumpkinLocs.Add(new Vector2(34, 189));
            pumpkinLocs.Add(new Vector2(61, 189));
            pumpkinLocs.Add(new Vector2(51, 172));
            pumpkinLocs.Add(new Vector2(89, 189));
            pumpkinLocs.Add(new Vector2(27, 171));
            pumpkinLocs.Add(new Vector2(76, 171));
            pumpkinLocs.Add(new Vector2(34, 153));
            pumpkinLocs.Add(new Vector2(60, 153));
            pumpkinLocs.Add(new Vector2(29, 136));
            pumpkinLocs.Add(new Vector2(50, 135));
            pumpkinLocs.Add(new Vector2(42, 117));
            pumpkinLocs.Add(new Vector2(420, 189));
            pumpkinLocs.Add(new Vector2(394, 189));
            pumpkinLocs.Add(new Vector2(425, 170));
            pumpkinLocs.Add(new Vector2(115, 190));
            pumpkinLocs.Add(new Vector2(101, 170));
            pumpkinLocs.Add(new Vector2(86, 152));
            pumpkinLocs.Add(new Vector2(74, 133));
            pumpkinLocs.Add(new Vector2(28, 101));
            pumpkinLocs.Add(new Vector2(62, 116));
            pumpkinLocs.Add(new Vector2(47, 99));
            pumpkinLocs.Add(new Vector2(401, 171));
            pumpkinLocs.Add(new Vector2(369, 189));
            pumpkinLocs.Add(new Vector2(427, 152));
            pumpkinLocs.Add(new Vector2(175, 190));
            pumpkinLocs.Add(new Vector2(266, 190));
            pumpkinLocs.Add(new Vector2(343, 190));
            pumpkinLocs.Add(new Vector2(372, 171));
            pumpkinLocs.Add(new Vector2(391, 153));
            pumpkinLocs.Add(new Vector2(410, 135));
            pumpkinLocs.Add(new Vector2(427, 118));
            //pumpkinLocs.Add(new Vector2(34, 189));
        }
        public void loadBackground(IServiceProvider serviceProvider)
        {
            ContentManager content = new ContentManager(serviceProvider, "Content");
            background1 = content.Load<Texture2D>("HighScores");
            background2 = content.Load<Texture2D>("HighScoresCopy");
            point = content.Load<Texture2D>("bottom_bar");
            font = content.Load<Texture2D>("numbersFont");
            pumpkin = content.Load<Texture2D>("pumpkin");
            score.init(font, Color.Black);

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

            if (g.winCount < 32)
            {
                for (int i = 0; i < g.winCount; i++)
                {
                    spriteBatch.Draw(pumpkin, pumpkinLocs[i], Color.White);
                }
                spriteBatch.Draw(background1, Vector2.Zero, Color.White);
            }
            else
                spriteBatch.Draw(background2, Vector2.Zero, Color.White);

            Vector2 counterLoc = new Vector2(counterStartLoc.X, counterStartLoc.Y);

            foreach (uint scoreVal in g.highScores)
            {
                score.value = scoreVal;
                score.origin = counterLoc;
                score.Draw(gameTime, spriteBatch, pumpkin);
                counterLoc.Y += 28;
            }
        }
    }
}