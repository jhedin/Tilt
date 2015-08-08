using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tilt
{

    public enum GameState
    {
        init,//loading and whatnot
        mainMenu,//main menu
        toneMenu,//sound manager(high scores?)
        paws,//pause screen(three fingers down)
        initLevel,//loading a level
        instructions,
        play,//allows the player to move around and whatnot
        died,//refresh the screen
        levelSelect,//level selector
        door,//player has reached the exit
        endScreen,//player has finished the game(aka, no more rooms) shows score and the like
        highScores, //high scores screen
        endGame//unload everything that is not a texture.
    }

    public enum animMode
    {
        //die,
        //tilt, needs drawing.
        run = 0,
        jump,
        drop,
        win,
        stand,
        die,
        STORAGE_SIZE
    }

    public enum Direction
    {
        up = 0,//bigger is clockwise, smaller is counter
        left,
        down,
        right//needs to wrap around for clockwise...
    }

    class globals
    {
        public Map map;
        public Player player;
        public GameState state;
        public Direction gravity;
        public List<uint> highScores;
        public int winCount;
        public void addScore(uint score)
        {
            winCount++;
            if (score != 0)
            {
                highScores.Add(score);
                highScores.Sort();
                highScores.Reverse();
                if (highScores.Count() > 5)
                {
                    highScores.RemoveAt(5);
                }
            }
        }
        public void addToSaveData(SaveData dat)
        {
            dat.winCount = winCount;
            dat.highScores = highScores;
            player.addToSaveData(dat);
            map.addToSaveData(dat);
        }
        public void loadFromSaveData(SaveData dat)
        {
            winCount = dat.winCount;
            highScores = dat.highScores;
            player.loadFromSaveData(dat);
            map.loadFromSaveData(dat);
        }
    }
}