using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace tilt
{
    [Serializable]
    public class SaveData
    {
        public uint time;
        public uint deaths;
        public int winCount;
        public List<uint> highScores;
        public int currentRoom;
        public int[] collisionMap;
        public int[] tileMap;
        public Direction gravity;
        public Direction gravityPrev;
        public Direction ditherDirection;
        public Direction waitDirection;
        public animMode playerAnim;
        public bool direction;
        public int animFrame;
        public bool go;
        public bool jump;
        public bool grounded;
        public bool ceiling;
        public bool dither;
        public bool inDither;
        public int waitCounter;
        public int hDirect;
        public int vDirect;
        public int hPos;
        public int vPos;
        public double hSpeed;
        public double vSpeed;
        public int jumpStartTime;
        public int animFrameCounter;
        public double[] playerLoc;
        }
}