using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace tilt
{
    //a list of rooms, and renders the selected room.
    //contains the tileset.
    //draws just about everything that's not the player, and holds (a copy of the) collision data.
    class Map
    {
        List<Room> rooms;
        int numberOfRooms;
        int lastRoom;//set by #endRoom:
        String tileSetName;
        public int roomX;
        public int roomY;
        int currentRoom;
        Texture2D tileset;
        Texture2D bottomBar;//the extra 2 pixels need to be black >:[
        public List<String> tileKey;
        public List<String> collisionKey;
        int[,] tileMap;
        int[,] collisionMap;
        int colsPerRow;
        //int roomMax;
        globals g;

        public Map(string path, MapParser inputParams, globals gIn)
        {
            currentRoom = 0;
            //roomMax = 0;
            tileSetName = inputParams.backgroundName;
            numberOfRooms = inputParams.numberOfRooms;
            tileKey = inputParams.tileKey;
            lastRoom = inputParams.lastRoom;
            collisionKey = inputParams.collisionKey;
            colsPerRow = inputParams.colsPerRow;
            roomX = inputParams.roomX;
            roomY = inputParams.roomY;
            rooms = new List<Room>();
            for (int roomNo = 0; roomNo < numberOfRooms; roomNo++)
            {
                rooms.Add(new Room(inputParams.roomData[roomNo]));
                rooms[roomNo].init(collisionKey);
            }

            g = gIn;
        }
        public void loadTileSet(IServiceProvider serviceProvider, String path)
        {
            ContentManager content = new ContentManager(serviceProvider, "Content");
            
            tileset = content.Load<Texture2D>(path);
            bottomBar = content.Load<Texture2D>("bottom_Bar");
        }
        public String getTileSetName()
        {
            return tileSetName;
        }
        public void selectRoom(int roomNo)
        {
            currentRoom = roomNo;
            /*if (currentRoom > roomMax)
            {
                roomMax = currentRoom;
            }*/
            tileMap = rooms[currentRoom].getTileMap();
            collisionMap = rooms[currentRoom].getCollisionMap();
            if (g.player != null)
            {
                g.player.reSetPlayerLoc();
                g.player.gameTimer.origin.X = rooms[currentRoom].getGameTimerLoc()[0]*30 + 5;
                g.player.gameTimer.origin.Y = rooms[currentRoom].getGameTimerLoc()[1]*30;
                g.player.deathCounter.origin.X = rooms[currentRoom].getDeathCounterLoc()[0]*30 + 5;
                g.player.deathCounter.origin.Y = rooms[currentRoom].getDeathCounterLoc()[1]*30;
            }
        }
        public void resetRoom()
        {
            selectRoom(currentRoom);
        }
        public void selectNextRoom()
        {
            if (currentRoom + 1 == lastRoom)
                selectRoom(0);
            else
                selectRoom(currentRoom + 1);
        }
        public int getTileTypeCollision(int x, int y)
        {
            if (x >= roomX || y >= roomY || x < 0 || y < 0)
                return 0;
            else
                return collisionMap[x, y];
                //return rooms[currentRoom].getTileCollisionType(x,y);
                
        }
        public int[] getPlayerStartLoc()
        {
            return rooms[currentRoom].getPlayerStartLoc();
        }
        public Direction getPlayerStartDirection()
        {
            return rooms[currentRoom].getPlayerStartDirection();
        }
        public int getNumOfRooms()
        {
            return numberOfRooms;
        }
        public int getFilledRooms()
        {
            return lastRoom;
        }
        public int getCurrentRoom()
        {
            return currentRoom;
        }
        public void removeCollisionNumber(int collisionNumb)
        {
            changeCollisionNumber(collisionNumb, 0);
        }
        public void changeCollisionNumber(int from, int to)
        {
            int drawTo = convertCollisionToDraw(to);
            for (int i = 0; i < collisionMap.GetLength(0); i++)
            {
                for (int j = 0; j < collisionMap.GetLength(1); j++)
                {
                    if (collisionMap[i, j] == from)
                    {
                        collisionMap[i, j] = to;
                        tileMap[i, j] = drawTo; // hopefully there's not too much work on this later...
                    }
                }
            }
        }
        public int convertCollisionToDraw(int from)
        {
            List<string> collKey = rooms[currentRoom].getCollisionKey();
            string searchKey = collKey[from];
            if (searchKey.Equals("#none"))
            {
                return 0;
            }
            for(int i = 0 ; i < tileKey.Count; i++)
            {
                if (tileKey[i].Contains(searchKey))
                    return i + 1;
            }

            return 0;
        }
        public void emptyTile(int x, int y)
        {
            collisionMap[x, y] = 0;
            tileMap[x, y] = 0;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(bottomBar, new Rectangle(0, 0, 480, 270), Color.DarkOrange);//bottom bar is actually and orage bar. should be renamed to point
      
            Rectangle destRect = new Rectangle(0,0,30,30);
            Rectangle srcRect  = new Rectangle(0,0,30,30);
            int srcX = 0;
            int srcY = 0;
            for (int y = 0; y < roomY; y++)
            {
                destRect.Y = 30 * y;
                for (int x = 0; x < roomX; x++)
                {
                    destRect.X = 30 * x;
                    if (tileMap[x, y] != 0)
                    {
                        srcX = tileMap[x, y] % colsPerRow - 1;
                        srcY = tileMap[x, y] / colsPerRow;
                        if (srcX < 0)
                        {
                            srcX = colsPerRow - 1;
                            srcY--;
                        }
                        srcRect.X = 30 * srcX;
                        srcRect.Y = 30 * srcY;
                        spriteBatch.Draw(tileset, destRect, srcRect, Color.White);
                    }
                }
            }
        }
        public void addToSaveData(SaveData dat)
        {
            dat.tileMap = new int[144];
            int i = 0;
            int j = 0;
            int count = 0;
            foreach (int k in tileMap)
            {
                dat.tileMap[i*9+j] = k;
                count++;
                j++;
                if (j == 9)
                {
                    j = 0;
                    i++;
                }
            }
            dat.collisionMap = new int[144];
            count = 0;
            i = 0;
            j = 0;
            foreach (int k in collisionMap)
            {
                dat.collisionMap[i * 9 + j] = k;
                j++;
                count++;
                if (j == 9)
                {
                    j = 0;
                    i++;
                }
            }
            dat.currentRoom = currentRoom;
        }
        public void loadFromSaveData(SaveData dat)
        {
            for(int i = 0; i < 16; i++)
                for (int j = 0; j < 9; j++)
                {
                    tileMap[i, j] = dat.tileMap[i * 9 + j];
                    collisionMap[i, j] = dat.collisionMap[i * 9 + j];
                }
            currentRoom = dat.currentRoom;
        }
    }
}