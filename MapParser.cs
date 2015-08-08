using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.IO;

namespace tilt
{
    /*reads a map from a file, parses it 
     * and initialises the map and all its room data*/
    class MapParser
    {
        public List<int[,]> roomData;
        public int numberOfRooms;
        public int lastRoom;
        public String backgroundName;
        public int roomX;
        public int roomY;
        public List<String> tileKey;
        public List<String> collisionKey;
        public int colsPerRow;
        public MapParser(string path)
        {
            roomX                   = 0;
            roomY                   = 0;
            numberOfRooms           = 0;
            roomData = new List<int [,]>();
            String mapData          = null;
            int searchResult        = -1;
            char[] DefinitionBuffer = { '0' };
            colsPerRow = 1;

            StreamReader reader = new StreamReader(path);
            mapData = reader.ReadLine();
            //get map size
            searchResult = strSearch(mapData, "#mapSize: point(", 0);
            if (searchResult != -1)
            {
                colsPerRow = getInt(mapData, searchResult);
                numberOfRooms = colsPerRow * getInt(mapData, searchResult + searchResult.ToString().Length + 2);
            }
            //get room size
            searchResult = strSearch(mapData, "#roomSize: point(", searchResult);
            if (searchResult != -1)
            {
                roomX = getInt(mapData, searchResult);
                roomY = getInt(mapData, searchResult + searchResult.ToString().Length + 2);
            }
            //get last room. if it is -1, there is no max
            searchResult = strSearch(mapData, "#endRoom: ", searchResult);
            if (searchResult != -1)
            {
                lastRoom = getInt(mapData, searchResult);
            }
            //get tileset name
            searchResult = strSearch(mapData, "#tileSet: #", searchResult);
            int i = 0;
            for (i = searchResult; mapData[i] != ','; i++);
            int tileSetNameLength = i - searchResult;
            backgroundName = mapData.Substring(searchResult, tileSetNameLength);

            //parse rooms...
            for (int j = 0; j < numberOfRooms; j++)
            {
                roomData.Add(new int[roomX, roomY]);
            }
            parseRoomData(mapData, roomData, searchResult, roomX, roomY);
            

        }
        //returns the index of the character imidiately after theString, -1 if not found
        public static int strSearch(String buffer, String theString, int startIndex)
        {
            int bufferLength = buffer.Length;
            int searchLength = theString.Length;

            for (int i = startIndex; i < (bufferLength - searchLength); i++)
            {
                if (buffer[i] == theString[0])
                {
                    if(searchLength == 1)
                        return i + 1;
                    for (int j = 1; buffer[i + j] == theString[j] ; j++)
                    {
                        if (j + 1 == searchLength)
                            return i + j + 1;//not sure bout the +1...
                    }
                }
            }
            
            
            return -1;
        }
        public static int getInt(String buffer, int startIndex)
        {
            //look for the end of the number
            int i = 0;
            int bufferLength = buffer.Length;
            for (i = startIndex; i < bufferLength && Char.IsDigit(buffer[i]); i++);
           
            int intLength = i - startIndex;
            int retValue;
            String temp = buffer.Substring(startIndex, intLength);
            retValue = int.Parse(temp);
            return retValue;
        }
        //string ends with a space(stops looking)
        public static String getString(String buffer, int startIndex)
        {
            if (buffer.Equals(""))
                return "";
            //look for the end of the number
            int i = 0;
            for (i = startIndex; buffer[i] != ' '; i++) ;

            int intLength = i - startIndex;
            String temp = buffer.Substring(startIndex, intLength);
            return temp;

        }
        private void parseRoomData(String mapData, List<int[,]> roomData, int startIndex, int roomX, int roomY)
        {
            int index = 0;
            int roomNo = 0;
            index = startIndex;
            int height = 0;
            while (true)
            {
                index = strSearch(mapData, "#num: ", index);
                if (index < 0)
                    break;
                roomNo = getInt(mapData, index) - 1;
                index = strSearch(mapData, "[[", index);
                index = strSearch(mapData, "[[", index);
                if (roomNo == 0)
                {
                    height++;
                }
                if (roomNo == 41 || roomNo + (height - 1) * colsPerRow == 41)
                {
                    roomNo = 41;
                }
                for (int i = 0; i < roomY; i++)
                {
                    int j = 0;
                    if (i != 0)
                        index = strSearch(mapData," [", index);
                    roomData[roomNo/* + (height - 1) * colsPerRow*/][j, i] = getInt(mapData, index);
                    for (j = 1; j < roomX; j++)
                    {
                        index = strSearch(mapData,", ",index);
                        if(Char.IsDigit(mapData[index]))
                        {
                            roomData[roomNo/* + (height - 1) * colsPerRow*/][j, i] = getInt(mapData, index);
                        }
                        else break;
                    }
                }
            }

        }
        public void parseTilesetData(String path)
        {
            tileKey = new List<String>();
            StreamReader reader = new StreamReader(path);

            String testLine;
            testLine = reader.ReadLine();
            testLine = reader.ReadLine();
            int index = strSearch(testLine, "-- ", 0);
            colsPerRow = getInt(testLine, index);
            while ((testLine = reader.ReadLine()) != null)
            {
                if(testLine.StartsWith("#"))
                    tileKey.Add(testLine);
            }
            parseCollisionKey(tileKey);
        }
        private void parseCollisionKey(List<String> tileSetData)
        {

            collisionKey = new List<string>();
            for(int val = 0; val < tileSetData.Count; val++)
            {
                switch (tileSetData[val])
                {
                    //changed players so that the direction can be checked when setting up a room
                    case "#playerUp":
                        collisionKey.Add("#playerUp");
                        break;
                    case "#playerDown":
                        collisionKey.Add("#playerDown");
                        break;
                    case "#playerLeft":
                        collisionKey.Add("#playerLeft");
                        break;
                    case "#playerRight":
                        collisionKey.Add("#playerRight");
                        break;
                    case "#doorUp":
                        collisionKey.Add("#door");
                        break;
                    case "#doorDown":
                        collisionKey.Add("#door");
                        break;
                    case "#doorLeft":
                        collisionKey.Add("#door");
                        break;
                    case "#doorRight":
                        collisionKey.Add("#door");
                        break;
                    case "#spikesUp":
                        collisionKey.Add("#spikes");
                        break;
                    case "#spikesDown":
                        collisionKey.Add("#spikes");
                        break;
                    case "#spikesLeft":
                        collisionKey.Add("#spikes");
                        break;
                    case "#spikesRight":
                        collisionKey.Add("#spikes");
                        break;

                    default:
                        collisionKey.Add(tileSetData[val]);
                        break;
                }

            }
        }
    }
}