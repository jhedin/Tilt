using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tilt
{
    //contains all data about a room. Can draw itself, based on a tileset.
    class Room
    {
        int[,] tileMap;//for drawing. Remember to remove player.
        int[,] collisionMap;//for testing collisions(no matter which direction... keeping things simplistic)
        int[] playerStartLoc;
        int[] gameTimerLoc;
        int[] deathCounterLoc;
        Direction startDirection;
        List<String> collisionKey;
        public Room(int[,] roomMap)
        {
            startDirection = Direction.right;//in case there is no player in the room
            tileMap = roomMap;

        }
        public void init(List<String> collisionKey)
        {
            lazyFillCollisionKey();
            playerStartLoc = new int[2];
            gameTimerLoc = new int[2];
            gameTimerLoc[0] = -15;
            gameTimerLoc[1] = -15;
            deathCounterLoc = new int[2];
            deathCounterLoc[0] = -15;
            deathCounterLoc[1] = -15;
            createCollisionMap(collisionKey);          
        }
        private void lazyFillCollisionKey()
        {
            //I'm incredibly lazy right now...
            collisionKey = new List<string>();
            collisionKey.Add("#none");
            collisionKey.Add("#solid");
            collisionKey.Add("#door");
            collisionKey.Add("#spikes");
            collisionKey.Add("#key");
            collisionKey.Add("#dither");
            collisionKey.Add("#fill");//counterPart to key
            collisionKey.Add("#pop");//counterpart to dither
            collisionKey.Add("#pumpkin");
        }
        private void createCollisionMap(List<String> collisionKey)
        {
            collisionMap = new int[tileMap.GetLength(0), tileMap.GetLength(1)];
            for (int y = 0; y < tileMap.GetLength(1); y++)
            {
                for (int x = 0; x < tileMap.GetLength(0); x++)
                {
                    int val = tileMap[x, y];
                    if (val != 0 && val <= collisionKey.Count())
                    {
                        switch (collisionKey[val - 1])
                        {
                            case "#none":
                                collisionMap[x, y] = 0;
                                break;
                            case "#playerLeft":
                                playerStartLoc[0] = x;
                                playerStartLoc[1] = y;
                                tileMap[x, y] = 0;
                                collisionMap[x, y] = 0;
                                startDirection = Direction.left;
                                break;
                            case "#playerRight":
                                playerStartLoc[0] = x;
                                playerStartLoc[1] = y;
                                tileMap[x, y] = 0;
                                collisionMap[x, y] = 0;
                                startDirection = Direction.right;
                                break;
                            case "#playerUp":
                                playerStartLoc[0] = x;
                                playerStartLoc[1] = y;
                                tileMap[x, y] = 0;
                                collisionMap[x, y] = 0;
                                startDirection = Direction.up;
                                break;
                            case "#playerDown":
                                playerStartLoc[0] = x;
                                playerStartLoc[1] = y;
                                tileMap[x, y] = 0;
                                collisionMap[x, y] = 0;
                                startDirection = Direction.down;
                                break;
                            case "#player"://just in case
                                playerStartLoc[0] = x;
                                playerStartLoc[1] = y;
                                tileMap[x, y] = 0;
                                collisionMap[x, y] = 0;
                                startDirection = Direction.right;
                                break;
                            case "#solid":
                                collisionMap[x, y] = 1;
                                break;
                            case "#door":
                                collisionMap[x, y] = 2;
                                break;
                            case "#spikes":
                                collisionMap[x, y] = 3;
                                break;
                            case "#key":
                                collisionMap[x, y] = 4;
                                break;
                            case "#dither":
                                collisionMap[x, y] = 5;
                                break;
                            case "#fill":
                                collisionMap[x, y] = 6;
                                break;
                            case "#pop":
                                collisionMap[x, y] = 7;
                                break;
                            case "#timer":
                                gameTimerLoc[0] = x;
                                gameTimerLoc[1] = y;
                                break;
                            case "#death":
                                deathCounterLoc[0] = x;
                                deathCounterLoc[1] = y;
                                break;
                            case "#pumpkin":
                                collisionMap[x, y] = 8;
                                break;
                            default:
                                break;

                        }
                    }
                    else
                    {
                        collisionMap[x, y] = 0;
                    }
                }
            }
        }
        public int[,] getTileMap()
        {
            int[,] mapCopy;
            mapCopy = new int[tileMap.GetLength(0), tileMap.GetLength(1)];
            for (int i = 0; i < mapCopy.GetLength(0); i++)
            {
                for (int j = 0; j < mapCopy.GetLength(1); j++)
                {
                    mapCopy[i, j] = tileMap[i, j];
                }
            }
            return mapCopy;
        }
        public int[,] getCollisionMap()
        {
            int[,] mapCopy;
            mapCopy = new int[collisionMap.GetLength(0), collisionMap.GetLength(1)];
            for (int i = 0; i < mapCopy.GetLength(0); i++)
            {
                for (int j = 0; j < mapCopy.GetLength(1); j++)
                {
                    mapCopy[i, j] = collisionMap[i, j];
                }
            }
            return mapCopy;
        }
        public int getTileCollisionType(int x, int y)
        {
            return collisionMap[x, y];
        }
        public List<String> getCollisionKey()
        {
            return collisionKey;
        }
        public int[] getPlayerStartLoc()
        {
            return playerStartLoc;
        }
        public int[] getGameTimerLoc()
        {
            return gameTimerLoc;
        }
        public int[] getDeathCounterLoc()
        {
            return deathCounterLoc;
        }
        public Direction getPlayerStartDirection()
        {
            return startDirection;
        }
    }
}