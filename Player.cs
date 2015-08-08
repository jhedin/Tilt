using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using System.Text;
using System.IO;

namespace tilt
{
    class Player
    {
        private struct playerModeInfo
        {
            public animMode playerAnim;
            public bool direction; //true = flip sprite(aka, left)
            public int animFrame; //the current frame number of an animation. Usually 0.
            public bool go; //whether the player is allowed to move.
        }
        playerModeInfo playerMode;
        private Texture2D tileSet;
        private Texture2D ditherSet;

        private Texture2D turnArrow; //arrow to show when the gravity isn't correct. Could really have a polymorphism for sprite-owning things here.
        private Rectangle turnArrowSrcRect;
        private Rectangle turnArrowDestRect;
        private Vector2 turnArrowOrigin;
        private float turnArrowRot;//the current rotation
        private bool turnArrowDirection;
        private bool turnArrowShow;

        private struct tileData
        {
            public animMode animType;
            public int frameNo;
            public Rectangle srcRect;
            public int[] origin;//x/y, measured from the top right hand corner or srcRect.(for picking a good destination)
        }
        List<List<tileData>> anim;
        List<List<tileData>> ditherAnim;
        int touchID;
        int waitCounter;
        
        int touchType;      
        bool jump;
        bool grounded;
        bool ceiling;

        bool dither;
        bool inDither;
        Direction ditherDirection;

        Direction gravity;
        Direction gravityPrev;
        Direction waitDirection;

        int hDirect; // 0 or 1: axis conversion.
        int vDirect;
        int hPos;// +/- 1, defines positive direction.
        int vPos;
        double hSpeed;
        double vSpeed;

        int frameTime;//how long to show each frame of an animation
        double touchSpeed;
        int jumpStartTime;
        int animFrameCounter;
        double accel;//gravity

        public Counter gameTimer;
        public Counter deathCounter;
        
        double [] playerLoc;
        globals g;//to communicate with the current room for collision dectection and start loc
        public Player(String path, globals gIn)
        {
            initAnimSet(path);
            playerLoc = new double[2];
            int[] tempLoc;
            tempLoc = gIn.map.getPlayerStartLoc();
            playerLoc[0] = 30*tempLoc[0] + 15;
            playerLoc[1] = 30*tempLoc[1] + 12;

            frameTime = 2;
            touchSpeed = 3;
            accel = 0.3;

            //starts as if its right(generic start)
            hDirect = 0;
            vDirect = 1;
            hPos = 1;
            vPos = 1;

            turnArrowSrcRect = new Rectangle(0,0,55,49);
            turnArrowDestRect = new Rectangle(240, 135, 55, 49);//needs changing(x,y)
            turnArrowDirection = false;
            turnArrowOrigin = new Vector2(28, 25);
            turnArrowRot = 0.0f;
            turnArrowShow = false;

            grounded = true;
            ceiling = false;
            
            dither = false;
            inDither = false;

            playerMode = new playerModeInfo();
            playerMode.animFrame = 0;
            playerMode.direction = false;
            playerMode.go = false;
            playerMode.playerAnim = animMode.stand;

            touchID = 0;

            gameTimer = new Counter(0, new Vector2(0, -15));
            deathCounter = new Counter(0, new Vector2(0, -15));

            g = gIn;
        }
        public void die()
        {
            stop();
            playerMode.playerAnim = animMode.die;
            playerMode.animFrame = 0;
        }
        private void initAnimSet(String path)
        {
            anim = new List<List<tileData>>();
            ditherAnim = new List<List<tileData>>();

            String animPath = Path.Combine(path, "playerTiles.key");
            String ditherPath = Path.Combine(path, "DitherTiles.key");
            for (int i = 0; i < (int)animMode.STORAGE_SIZE; i++)
            {
                anim.Add(new List<tileData>());
                ditherAnim.Add(new List<tileData>());
            }

            int index;
            tileData tempData;
            StreamReader reader = new StreamReader(animPath);
            String line;
            String tempString;
            line = reader.ReadLine();
            while (line != null)
            {
                //assume all frames are written in order, from 0 up
                index = 0;
                tempData = new tileData();
                tempData.origin = new int[2];
                tempData.srcRect = new Rectangle();
                tempString = MapParser.getString(line, index);
                index = MapParser.strSearch(line, " ", index);
                tempData.frameNo = MapParser.getInt(line, index);
                index = MapParser.strSearch(line, " ", index);
                tempData.srcRect.X = MapParser.getInt(line, index);
                index = MapParser.strSearch(line, " ", index);
                tempData.srcRect.Y = MapParser.getInt(line, index);
                index = MapParser.strSearch(line, " ", index);
                tempData.srcRect.Width = MapParser.getInt(line, index);
                index = MapParser.strSearch(line, " ", index);
                tempData.srcRect.Height = MapParser.getInt(line, index);
                index = MapParser.strSearch(line, " ", index);
                tempData.origin[0] = MapParser.getInt(line, index);
                index = MapParser.strSearch(line, " ", index);
                tempData.origin[1] = MapParser.getInt(line, index);

                switch (tempString)
                {
                    case "stand":
                        tempData.animType = animMode.stand;
                        anim[(int)animMode.stand].Add(tempData);
                        break;
                    case "run":
                        tempData.animType = animMode.run;
                        anim[(int)animMode.run].Add(tempData);
                        break;
                    case "win":
                        tempData.animType = animMode.win;
                        anim[(int)animMode.win].Add(tempData);
                        break;
                    case "drop":
                        tempData.animType = animMode.drop;
                        anim[(int)animMode.drop].Add(tempData);
                        break;
                    case "jump":
                        tempData.animType = animMode.jump;
                        anim[(int)animMode.jump].Add(tempData);
                        break;
                    case "die":
                        tempData.animType = animMode.die;
                        anim[(int)animMode.die].Add(tempData);
                        break;
                }

                line = reader.ReadLine();
            }
            //I'm lazy:
            reader = new StreamReader(ditherPath);
            line = reader.ReadLine();
            while (line != null)
            {
                //assume all frames are written in order, from 0 up
                index = 0;
                tempData = new tileData();
                tempData.origin = new int[2];
                tempData.srcRect = new Rectangle();
                tempString = MapParser.getString(line, index);
                index = MapParser.strSearch(line, " ", index);
                tempData.frameNo = MapParser.getInt(line, index);
                index = MapParser.strSearch(line, " ", index);
                tempData.srcRect.X = MapParser.getInt(line, index);
                index = MapParser.strSearch(line, " ", index);
                tempData.srcRect.Y = MapParser.getInt(line, index);
                index = MapParser.strSearch(line, " ", index);
                tempData.srcRect.Width = MapParser.getInt(line, index);
                index = MapParser.strSearch(line, " ", index);
                tempData.srcRect.Height = MapParser.getInt(line, index);
                index = MapParser.strSearch(line, " ", index);
                tempData.origin[0] = MapParser.getInt(line, index);
                index = MapParser.strSearch(line, " ", index);
                tempData.origin[1] = MapParser.getInt(line, index);

                switch (tempString)
                {
                    case "stand":
                        tempData.animType = animMode.stand;
                        ditherAnim[(int)animMode.stand].Add(tempData);
                        break;
                    case "run":
                        tempData.animType = animMode.run;
                        ditherAnim[(int)animMode.run].Add(tempData);
                        break;
                    case "win":
                        tempData.animType = animMode.win;
                        ditherAnim[(int)animMode.win].Add(tempData);
                        break;
                    case "drop":
                        tempData.animType = animMode.drop;
                        ditherAnim[(int)animMode.drop].Add(tempData);
                        break;
                    case "jump":
                        tempData.animType = animMode.jump;
                        ditherAnim[(int)animMode.jump].Add(tempData);
                        break;
                    case "die":
                        tempData.animType = animMode.die;
                        ditherAnim[(int)animMode.die].Add(tempData);
                        break;
                }

                line = reader.ReadLine();
            }
        }
        public void loadTileSet(IServiceProvider serviceProvider)
        {
            ContentManager content = new ContentManager(serviceProvider, "Content");

            tileSet = content.Load<Texture2D>("playerTiles");
            turnArrow = content.Load<Texture2D>("turnArrow");
            ditherSet = content.Load<Texture2D>("DitherTiles");
            Texture2D font = content.Load<Texture2D>("numbersFont");
            gameTimer.init(font, Color.Black);
            deathCounter.init(font, Color.Black);
        }
        public void update()
        {
            handleInput();
            gameTimer.update();
            if (playerMode.go)//is set to be off, after the room is won. Turned back on by the map, when its good and ready.
            {
                
                handleMotion();
                setAnimation();
                handleCollisions();          
            }
            else if (playerMode.playerAnim == animMode.win)
            {

                if (waitCounter == 30)
                {
                    g.map.selectNextRoom();
                }
                else
                {
                    waitCounter++;
                }
            }
            else if (playerMode.playerAnim == animMode.die)
            {
                setAnimation();
                if (playerMode.animFrame == anim[(int)playerMode.playerAnim].Count - 1)
                {
                    g.map.resetRoom();
                    deathCounter.value++;
                }
            }
            else
            {
                if (waitForDirection())
                {
                    go();
                }
            }
        }
        private bool waitForDirection()
        {
            //Direction startDirection = g.map.getPlayerStartDirection();
            if (g.gravity == waitDirection)
            {
                gravity = waitDirection;
                turnArrowShow = false;
                return true;
            }
            else
            {
                turnArrowShow = true;
                turnArrowDirection = (int)g.gravity < (int)waitDirection;
                if (g.gravity == Direction.up && waitDirection == Direction.right)
                    turnArrowDirection = false;
                if (g.gravity == Direction.right && waitDirection == Direction.up)
                    turnArrowDirection = true;
                gravity = waitDirection;
                gravityPrev = waitDirection;
                return false;
            }
        }
        private void handleInput()
        {
            if (playerMode.go)
            {
                gravity = g.gravity;
            }
            if (dither || inDither)
            {
                gravity = ditherDirection;
            }
            switch (gravity)
            {
                case Direction.down:
                    hDirect = 1;
                    vDirect = 0;
                    hPos = 1;
                    vPos = -1;
                    break;
                case Direction.up:
                    hDirect = 1;
                    vDirect = 0;
                    hPos = -1;
                    vPos = 1;
                    break;
                case Direction.left:
                    hDirect = 0;
                    vDirect = 1;
                    hPos = 1;
                    vPos = -1;
                    break;
                case Direction.right:
                    hDirect = 0;
                    vDirect = 1;
                    hPos = 1;
                    vPos = 1;
                    break;
            }
            if (gravity != gravityPrev && playerMode.playerAnim != animMode.die && playerMode.playerAnim != animMode.win)
            {
                playerMode.playerAnim = animMode.drop;
                playerMode.animFrame = 0;
            }
            //gravityPrev = gravity;
            TouchCollection touchCollection = TouchPanel.GetState();
            touchType = 0;
            jump = false;
            //todo: add test for three finger touch
            
            switch (touchCollection.Count)
            {
                case 1:
                    touchType = getTouchState(touchCollection[0]);
                    touchID = touchCollection[0].Id;
                    break;

                case 2:
                    jump = true;
                    if (touchCollection[0].Id == touchID)
                    {
                        touchType = getTouchState(touchCollection[0]);
                    }
                    else if (touchCollection[1].Id == touchID)
                    {
                        touchType = getTouchState(touchCollection[1]);
                    }
                    break;

                case 3:
                    stop();
                    g.state = GameState.paws;
                    waitDirection = gravity;
                    break;

                case 4:
                    if (playerMode.playerAnim != animMode.die)
                    {
                        die();
                    }
                    break;

                default:
                    //no touches
                    break;
            }
            GamePadState gamepadState = GamePad.GetState(PlayerIndex.One);
            if (gamepadState.Buttons.Back == ButtonState.Pressed)
            {
                stop();
                g.state = GameState.paws;
                waitDirection = gravity;
            }
            /*
            if (keyState.IsKeyDown(Keys.Up))
                jump = true;
            if (keyState.IsKeyDown(Keys.Left))
                touchType = -1;
            if (keyState.IsKeyDown(Keys.Right))
                touchType = 1;*/

        }
        private int getTouchState(TouchLocation touchLocation)
        {
            if (gravity == Direction.right || gravity == Direction.left)
            {
                if (touchLocation.Position.Y < 240)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            else if(gravity == Direction.up)
            {
                if (touchLocation.Position.X < 136)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                if (touchLocation.Position.X > 136)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
        }
        private void handleMotion()
        {
            /*if (gravity != gravityPrev)
            {
                double switchTemp;
                if (gravityPrev == Direction.up)
                {
                    switch (gravity)
                    {
                        case Direction.down:
                            hSpeed = -hSpeed;
                            vSpeed = -vSpeed;
                            break;
                        case Direction.right:
                            switchTemp = hSpeed;
                            hSpeed = vSpeed;
                            vSpeed = switchTemp;
                            break;
                        case Direction.left:
                            switchTemp = hSpeed;
                            hSpeed = vSpeed;
                            vSpeed = switchTemp;
                            break;
                    }
                }
            }*/
            //if (gravity != gravityPrev)
            //{
                //if ((gravity == Direction.right && gravityPrev == Direction.down) || (gravity == Direction.left && gravityPrev == Direction.up)/* || (inDither && playerMode.playerAnim == animMode.drop)*/)
                //{
                //    vSpeed = -vSpeed;
                //}
                //else 
                  //  vSpeed = 0;
            //}
            
            //effects the input on the next frame?
            /*playerLoc[hDirect] += hSpeed;
            playerLoc[vDirect] += vSpeed;
            */
            //deal with left/right motion
            //playerLoc[hDirect] += (hPos) * touchType * touchSpeed;


            if (gravity != gravityPrev)
            {
                jumpStartTime = 5;//so he starts falling pretty quickly from the get-go.
            }

            if (touchType != 0)
            {
                hSpeed = (hPos) * touchType * touchSpeed;
            }
            else //quickly decelerate
            {
                double haccel = hSpeed > 0 ? -.9 : .9;
                hSpeed += haccel;
                if (Math.Abs(hSpeed) < 1)
                {
                    hSpeed = 0;
                }

            }
            
            //jumping
            if (jump && !ceiling && (playerMode.playerAnim == animMode.stand || playerMode.playerAnim == animMode.run))
            {
                jumpStartTime = 0;
                playerMode.playerAnim = animMode.jump;
                //vSpeed = -1 * vPos * touchSpeed * 1.8;
            }
            else if (playerMode.playerAnim == animMode.jump)
            {
                jumpStartTime++;
                double displacement = touchSpeed * 1.8 - accel * jumpStartTime;
                vSpeed = -1 * (vPos)*displacement; //move upwards
                if (displacement < 0)
                {
                    jumpStartTime = 0;
                    playerMode.playerAnim = animMode.drop;
                    //jump is also stopped by collisions, and set to drop
                }/*
                vSpeed += vPos * accel;// * jumpStartTime;
                if (vPos * vSpeed >= 0)
                {
                    //jumpStartTime = 0;
                    playerMode.playerAnim = animMode.drop;
                    //jump is also stopped by collisions, and set to drop
                }*/
            }
            else if (playerMode.playerAnim == animMode.drop)
            {
                double displacement = (vPos)*accel * jumpStartTime;
                if (Math.Abs(displacement) >= touchSpeed * 1.9)
                {
                    displacement = (vPos) * touchSpeed * 1.9;
                }
                vSpeed = displacement;//drop
                jumpStartTime++;
                /*vSpeed += vPos * accel;
                if (Math.Abs(vSpeed) >= touchSpeed * 1.9)
                {
                    vSpeed = (vPos) * touchSpeed * 1.9;
                }
                if (vPos * vSpeed < 0 && (gravityPrev - gravity)%2 != 0)
                {
                    vSpeed = -1 * vSpeed; 
                }*/

            }
            else if (touchType != 0)
            {
                playerMode.playerAnim = animMode.run;
                if (touchType < 0)
                    playerMode.direction = true;
                else
                    playerMode.direction = false;
            }
            else if (touchType == 0)
                playerMode.playerAnim = animMode.stand;
            playerLoc[hDirect] += hSpeed;
            playerLoc[vDirect] += vSpeed;
            gravityPrev = gravity;
        }
        int[,] getTiles()
        {
            /*
             * shape of basic output:
             * 0 1 2
             * 3 4 5
             * 6 7 8
             * (array indexes vs their place around the player)
             */
            
            int[,] tiles = new int[9,3];

            //center of box
            tiles[4,0] = (int)playerLoc[0] / 30;
            tiles[4,1] = (int)playerLoc[1] / 30;
            tiles[4,2] = g.map.getTileTypeCollision(tiles[4, 0], tiles[4, 1]);
            
            int i = 0;
            
            switch (gravity)
            {
                case Direction.right:
                    /*
                    * 0 1 2
                    * 3 4 5
                    * 6 7 8
                    */
                    for (int y = -1; y < 2; y++)
                    {
                        for (int x = -1; x < 2; x++)
                        {
                            if (i == 4)
                            {
                                i++;
                                continue;
                            }
                            tiles[i, 0] = tiles[4, 0] + x;
                            tiles[i, 1] = tiles[4, 1] + y;
                            tiles[i, 2] = g.map.getTileTypeCollision(tiles[i, 0], tiles[i, 1]);

                            i++;
                        }
                    }
                    break;
                case Direction.left:
                    /*
                    * 6 7 8
                    * 3 4 5
                    * 1 2 3
                    */
                    for (int y = 1; y > -2; y--)
                    {
                        for (int x = -1; x < 2; x++)
                        {
                            if (i == 4)
                            {
                                i++;
                                continue;
                            }
                            tiles[i, 0] = tiles[4, 0] + x;
                            tiles[i, 1] = tiles[4, 1] + y;
                            tiles[i, 2] = g.map.getTileTypeCollision(tiles[i, 0], tiles[i, 1]);

                            i++;
                        }
                    }
                    break;
                case Direction.down:
                /*
                * 7 4 1
                * 8 5 2
                * 9 6 3
                */
                    for (int x = 1; x > -2; x--)
                    {
                        for (int y = -1; y < 2; y++)
                        {
                            if (i == 4)
                            {
                                i++;
                                continue;
                            }
                            tiles[i, 0] = tiles[4, 0] + x;
                            tiles[i, 1] = tiles[4, 1] + y;
                            tiles[i, 2] = g.map.getTileTypeCollision(tiles[i, 0], tiles[i, 1]);

                            i++;
                        }
                    }
                    break;
                case Direction.up:
                    /*
                    * 1 4 7
                    * 2 5 8
                    * 3 6 9
                    */
                    for (int x = -1; x < 2; x++)
                    {
                        for (int y = 1; y > -2; y--)
                        {
                            if (i == 4)
                            {
                                i++;
                                continue;
                            }
                            tiles[i, 0] = tiles[4, 0] + x;
                            tiles[i, 1] = tiles[4, 1] + y;
                            tiles[i, 2] = g.map.getTileTypeCollision(tiles[i, 0], tiles[i, 1]);

                            i++;
                        }
                    }
                    break;
                default:
                    break;
            }

            return tiles;

        }
        private void handleCollisions()
        {
            tileData tempData = anim[(int)animMode.stand][0];
            Rectangle boundingRect = getBoundingBox(tempData);
            Rectangle playerRect = new Rectangle();
            Rectangle tileRect = new Rectangle(0, 0, 30, 30);//Xs and Ys will be scrolled through.
            int[,] tiles = getTiles();

            int collisionh = 0;
            int collisionv = 0;
            grounded = false;
            ceiling = false;

            playerRect.Width = Math.Abs(boundingRect.Width) + 2;
            playerRect.Height = Math.Abs(boundingRect.Height) + 2;
            playerRect.X = (boundingRect.Left < boundingRect.Right ? boundingRect.Left : boundingRect.Right) - 1;
            playerRect.Y = (boundingRect.Top < boundingRect.Bottom ? boundingRect.Top : boundingRect.Bottom) - 1;

            if (gravity == Direction.left || gravity == Direction.right)
            {
                

                tileRect.X = 30 * tiles[5, 0];
                tileRect.Y = 30 * tiles[5, 1];
                //check right
                if (tileRect.Intersects(playerRect))
                {
                    switch (tiles[5, 2])
                    {
                        case 1:
                            collisionh = tileRect.Left - playerRect.Right;
                            break;
                        default:
                            break;
                    }
                }
                if (collisionh == 0)
                {
                    tileRect.X = 30 * tiles[8, 0];
                    tileRect.Y = 30 * tiles[8, 1];
                    //bottom Right
                    if (tileRect.Intersects(playerRect))
                    {
                        switch (tiles[8, 2])
                        {
                            case 1:
                                if (vPos * (tileRect.Center.Y - playerRect.Bottom) > 0)
                                {
                                    grounded = true;
                                    collisionv = (Math.Abs(tileRect.Top - playerRect.Bottom) < Math.Abs(playerRect.Top - tileRect.Bottom) ? tileRect.Top - playerRect.Bottom : playerRect.Top - tileRect.Bottom) + 1;
                                }
                                else
                                    collisionh = tileRect.Left - playerRect.Right;
                                break;
                            default:
                                break;
                        }
                    }
                    //top right
                    tileRect.X = 30 * tiles[2, 0];
                    tileRect.Y = 30 * tiles[2, 1];
                    if (tileRect.Intersects(playerRect))
                    {
                        switch (tiles[2, 2])
                        {
                            case 1:

                                if (vPos * (tileRect.Center.Y - playerRect.Top) <= 0)//if its above and solid? Very iffy when things start rotating.
                                {
                                    ceiling = true;
                                    if (!grounded)
                                    {
                                        collisionv = -1 * (Math.Abs(tileRect.Top - playerRect.Bottom) < Math.Abs(playerRect.Top - tileRect.Bottom) ? tileRect.Top - playerRect.Bottom : playerRect.Top - tileRect.Bottom);
                                    }
                                }
                                else
                                {
                                    if (!grounded)
                                    {
                                        collisionh = tileRect.Left - playerRect.Right;
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    //left
                    tileRect.X = 30 * tiles[3, 0];
                    tileRect.Y = 30 * tiles[3, 1];
                    if (tileRect.Intersects(playerRect))
                    {
                        switch (tiles[3, 2])
                        {
                            case 1:
                                collisionh = tileRect.Right - playerRect.Left;
                                break;
                            default:
                                break;
                        }
                    }
                    if (collisionh == 0)
                    {
                        tileRect.X = 30 * tiles[6, 0];
                        tileRect.Y = 30 * tiles[6, 1];
                        //bottom left
                        if (tileRect.Intersects(playerRect))
                        {
                            switch (tiles[6, 2])
                            {
                                case 1:
                                    if (vPos * (tileRect.Center.Y - playerRect.Bottom) > 0)
                                    {
                                        grounded = true;
                                        collisionv = (Math.Abs(tileRect.Top - playerRect.Bottom) < Math.Abs(playerRect.Top - tileRect.Bottom) ? tileRect.Top - playerRect.Bottom : playerRect.Top - tileRect.Bottom) + 1;
                                    }
                                    else
                                        collisionh = tileRect.Right - playerRect.Left;
                                    break;
                                default:
                                    break;
                            }
                        }
                        //top left
                        tileRect.X = 30 * tiles[0, 0];
                        tileRect.Y = 30 * tiles[0, 1];
                        if (tileRect.Intersects(playerRect))
                        {
                            switch (tiles[0, 2])
                            {
                                case 1:
                                    if (vPos * (tileRect.Center.Y - playerRect.Top) <= 0)
                                    {
                                        ceiling = true;
                                        if (!grounded)
                                        {
                                            collisionv = -1 * (Math.Abs(tileRect.Top - playerRect.Bottom) < Math.Abs(playerRect.Top - tileRect.Bottom) ? tileRect.Top - playerRect.Bottom : playerRect.Top - tileRect.Bottom);
                                        }
                                    }
                                    else
                                    {
                                        if (!grounded)
                                        {
                                            collisionh = tileRect.Right - playerRect.Left;
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }

                    }

                }
                //check bottom
                if (!grounded)
                {
                    tileRect.X = 30 * tiles[7, 0];
                    tileRect.Y = 30 * tiles[7, 1];
                    if (tileRect.Intersects(playerRect))
                    {
                        switch (tiles[7, 2])
                        {
                            case 1:
                                grounded = true;

                                collisionv = (Math.Abs(tileRect.Top - playerRect.Bottom) < Math.Abs(playerRect.Top - tileRect.Bottom) ? tileRect.Top - playerRect.Bottom : playerRect.Top - tileRect.Bottom) + 1;
                                break;
                            default:
                                break;
                        }
                    }
                }
                //check top
                if (!ceiling)
                {
                    tileRect.X = 30 * tiles[1, 0];
                    tileRect.Y = 30 * tiles[1, 1];
                    if (tileRect.Intersects(playerRect))
                    {
                        switch (tiles[1, 2])
                        {
                            case 1:
                                ceiling = true;
                                if (!grounded)
                                {
                                    collisionv = -1 * (Math.Abs(tileRect.Top - playerRect.Bottom) < Math.Abs(playerRect.Top - tileRect.Bottom) ? tileRect.Top - playerRect.Bottom : playerRect.Top - tileRect.Bottom);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            else /////////////////////////////////////////////////////////////////////////////////// up/down
            {
                tileRect.X = 30 * tiles[5, 0];
                tileRect.Y = 30 * tiles[5, 1];
                //check right
                if (tileRect.Intersects(playerRect))
                {
                    switch (tiles[5, 2])
                    {
                        case 1:
                            collisionh = (Math.Abs(tileRect.Top - playerRect.Bottom) < Math.Abs(playerRect.Top - tileRect.Bottom) ? tileRect.Top - playerRect.Bottom : playerRect.Top - tileRect.Bottom);
                            break;
                        default:
                            break;
                    }
                }
                if (collisionh == 0)
                {
                    tileRect.X = 30 * tiles[8, 0];
                    tileRect.Y = 30 * tiles[8, 1];
                    //bottom Right
                    if (tileRect.Intersects(playerRect))
                    {                        
                        switch (tiles[8, 2])
                        {
                            case 1:
                                if (vPos * (tileRect.Center.X - playerRect.Right) > 0)
                                {
                                    grounded = true;
                                    collisionv = (Math.Abs(tileRect.Left - playerRect.Right) < Math.Abs(playerRect.Left - tileRect.Right) ? tileRect.Left - playerRect.Right : playerRect.Left - tileRect.Right) + 1;
                                }
                                else
                                    collisionh = (Math.Abs(tileRect.Top - playerRect.Bottom) < Math.Abs(playerRect.Top - tileRect.Bottom) ? tileRect.Top - playerRect.Bottom : playerRect.Top - tileRect.Bottom);
                                break;
                            default:
                                break;
                        }
                    }
                    //top right
                    tileRect.X = 30 * tiles[2, 0];
                    tileRect.Y = 30 * tiles[2, 1];
                    if (tileRect.Intersects(playerRect))
                    {
                        switch (tiles[2, 2])
                        {
                            case 1:

                                if (vPos * (tileRect.Center.X - playerRect.Left) <= 0)//if its above and solid? Very iffy when things start rotating.
                                {
                                    ceiling = true;
                                    if (!grounded)
                                    {
                                        collisionv = -1 * (Math.Abs(tileRect.Left - playerRect.Right) < Math.Abs(playerRect.Left - tileRect.Right) ? tileRect.Left - playerRect.Right : playerRect.Left - tileRect.Right);
                                    }
                                }
                                else
                                {
                                    if (!grounded)
                                    {
                                        collisionh = (Math.Abs(tileRect.Top - playerRect.Bottom) < Math.Abs(playerRect.Top - tileRect.Bottom) ? tileRect.Top - playerRect.Bottom : playerRect.Top - tileRect.Bottom);
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    //left
                    tileRect.X = 30 * tiles[3, 0];
                    tileRect.Y = 30 * tiles[3, 1];
                    if (tileRect.Intersects(playerRect))
                    {
                        switch (tiles[3, 2])
                        {
                            case 1:
                                collisionh = -1 * (Math.Abs(tileRect.Top - playerRect.Bottom) < Math.Abs(playerRect.Top - tileRect.Bottom) ? tileRect.Top - playerRect.Bottom : playerRect.Top - tileRect.Bottom);
                                break;
                            default:
                                break;
                        }
                    }
                    if (collisionh == 0)
                    {
                        tileRect.X = 30 * tiles[6, 0];
                        tileRect.Y = 30 * tiles[6, 1];
                        //bottom left
                        if (tileRect.Intersects(playerRect))
                        {
                            switch (tiles[6, 2])
                            {
                                case 1:
                                    if (vPos * (tileRect.Center.X - playerRect.Right) > 0)
                                    {
                                        grounded = true;
                                        collisionv = (Math.Abs(tileRect.Left - playerRect.Right) < Math.Abs(playerRect.Left - tileRect.Right) ? tileRect.Left - playerRect.Right : playerRect.Left - tileRect.Right) + 1;
                                    }
                                    else
                                        collisionh = -1 * (Math.Abs(tileRect.Top - playerRect.Bottom) < Math.Abs(playerRect.Top - tileRect.Bottom) ? tileRect.Top - playerRect.Bottom : playerRect.Top - tileRect.Bottom);
                                    break;
                                default:
                                    break;
                            }
                        }
                        //top left
                        tileRect.X = 30 * tiles[0, 0];
                        tileRect.Y = 30 * tiles[0, 1];
                        if (tileRect.Intersects(playerRect))
                        {
                            switch (tiles[0, 2])
                            {
                                case 1:
                                    if (vPos * (tileRect.Center.X - playerRect.Left) <= 0)
                                    {
                                        ceiling = true;
                                        if (!grounded)
                                        {
                                            collisionv = -1 * (Math.Abs(tileRect.Left - playerRect.Right) < Math.Abs(playerRect.Left - tileRect.Right) ? tileRect.Left - playerRect.Right : playerRect.Left - tileRect.Right);
                                        }
                                    }
                                    else
                                    {
                                        if (!grounded)
                                        {
                                            collisionh = -1 * (Math.Abs(tileRect.Top - playerRect.Bottom) < Math.Abs(playerRect.Top - tileRect.Bottom) ? tileRect.Top - playerRect.Bottom : playerRect.Top - tileRect.Bottom);
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }

                    }

                }
                //check bottom
                if (!grounded)
                {
                    tileRect.X = 30 * tiles[7, 0];
                    tileRect.Y = 30 * tiles[7, 1];
                    if (tileRect.Intersects(playerRect))
                    {
                        switch (tiles[7, 2])
                        {
                            case 1:
                                grounded = true;

                                collisionv = (Math.Abs(tileRect.Left - playerRect.Right) < Math.Abs(playerRect.Left - tileRect.Right) ? tileRect.Left - playerRect.Right : playerRect.Left - tileRect.Right) + 1;
                                break;
                            default:
                                break;
                        }
                    }
                }
                //check top
                if (!ceiling)
                {
                    tileRect.X = 30 * tiles[1, 0];
                    tileRect.Y = 30 * tiles[1, 1];
                    if (tileRect.Intersects(playerRect))
                    {
                        switch (tiles[1, 2])
                        {
                            case 1:
                                ceiling = true;
                                if (!grounded)
                                {
                                    //collisionv = tileRect.Bottom - playerRect.Top;
                                    collisionv = -1 * (Math.Abs(tileRect.Left - playerRect.Right) < Math.Abs(playerRect.Left - tileRect.Right) ? tileRect.Left - playerRect.Right : playerRect.Left - tileRect.Right);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            if (grounded && playerMode.playerAnim == animMode.drop)
            {
                playerMode.playerAnim = animMode.stand;
                playerMode.animFrame = 0;
            }

            if (!grounded && playerMode.playerAnim != animMode.jump)
            {
                playerMode.playerAnim = animMode.drop;
                playerMode.animFrame = 0;
            }
            if (ceiling && playerMode.playerAnim == animMode.jump)
            {
                playerMode.playerAnim = animMode.drop;
                playerMode.animFrame = 0;
            }
            /*if (grounded && tiles[1,2] == 1)
            {
                ceiling = true;
            }*/


            playerLoc[hDirect] += hPos * collisionh;
            playerLoc[vDirect] += vPos * collisionv;
            if (collisionh != 0)
            {
                hSpeed = 0;
            }
            if ((grounded && playerMode.playerAnim != animMode.jump))
            {
                vSpeed = 0;
            }
            if (ceiling && playerMode.playerAnim == animMode.jump)
            {
                vSpeed = -1 / 2 * vSpeed;
            }
            if (tiles[4, 2] != 5 && tiles[4, 2] != 7)
            {
                inDither = false;
            }
            switch (tiles[4, 2])
            {
                case 3://spikes
                    die();
                    break;
                case 5://in dither
                    if (!inDither)
                    {
                        inDither = true;
                        switch (gravity)
                        {
                            case Direction.up:
                                ditherDirection = Direction.down;
                                break;
                            case Direction.left:
                                ditherDirection = Direction.right;
                                break;
                            case Direction.down:
                                ditherDirection = Direction.up;
                                break;
                            case Direction.right:
                                ditherDirection = Direction.left;
                                break;
                        }
                        dither = !dither;
                        playerMode.playerAnim = animMode.drop;
                        playerMode.animFrame = 0;
                    }
                    break;
                case 4:
                    g.map.removeCollisionNumber(5);
                    g.map.emptyTile((int)playerLoc[0] / 30, (int)playerLoc[1] / 30);
                    break;
                case 6:
                    g.map.changeCollisionNumber(5, 1);
                    g.map.emptyTile((int)playerLoc[0] / 30, (int)playerLoc[1] / 30);
                    break;
                case 7:
                    if (!inDither)
                    {
                        inDither = true;
                        if (dither)
                        {
                            dither = false;
                            //playerMode.playerAnim = animMode.drop;
                            ditherDirection = g.gravity;
                        }
                        else
                        {
                            //reSetPlayerLoc();
                            //playerMode.playerAnim = animMode.die;
                            //playerMode.animFrame = 0;
                            die();
                        }
                        
                    }
                    break;
                case 8:
                    stop();
                    uint score;
                    if (gameTimer.value + 10 * deathCounter.value < 50 * g.map.getCurrentRoom())
                    {
                        score = (uint)(50 * g.map.getCurrentRoom() - gameTimer.value - 10 * deathCounter.value);
                    }
                    else
                    {
                        score = 0;
                    }
                    g.addScore(score);
                    g.state = GameState.endScreen;
                    break;
            }


            if (grounded)//did we win?
            {
                if (gravity == Direction.right || gravity == Direction.left)
                {
                    if (tiles[4, 2] == 2 && (int)playerLoc[0] / 30 == playerRect.Left / 30 && (int)playerLoc[0] / 30 == playerRect.Right / 30)
                    {
                        stop();
                        playerMode.playerAnim = animMode.win;
                        playerMode.animFrame = 0;
                    }
                }
                else
                {
                    if (tiles[4, 2] == 2 && (int)playerLoc[1] / 30 == playerRect.Top / 30 && (int)playerLoc[1] / 30 == playerRect.Bottom / 30)
                    {
                        stop();
                        playerMode.playerAnim = animMode.win;
                        playerMode.animFrame = 0;
                    }
                }
            }
    


            //if the shit hit the fan
            if (tiles[4, 2] == 1)
            {
                playerLoc[vDirect] -= vPos * 15;



                //oh wait, its always going to need to move upwards!
                /*
                 * tileRect.X = 30 * tiles[4, 0];
                tileRect.Y = 30 * tiles[4, 1];List<Vector2> corners = new List<Vector2>();
                corners.Add(new Vector2(playerRect.X,playerRect.Y));
                corners.Add(new Vector2(playerRect.X + playerRect.Width, playerRect.Y));
                corners.Add(new Vector2(playerRect.X + playerRect.Width, playerRect.Y + playerRect.Height));
                corners.Add(new Vector2(playerRect.X, playerRect.Y + playerRect.Height));
                Vector2 MaxCorner;
                int dist = 0;
                int maxDist = 0;

                foreach (Vector2 corner in corners)
                {                    
                    dist = (int)Math.Sqrt((corner.X - tileRect.Center.X) * (corner.X - tileRect.Center.X) + (corner.Y - tileRect.Center.Y) * (corner.Y - tileRect.Center.Y));
                    if (dist >= maxDist)
                    {
                        MaxCorner = corner;
                        maxDist = dist;
                    }
                }
                */

            }

        }      
        private void setAnimation()
        {
            if (anim[(int)playerMode.playerAnim].Count > 1)
            {
                animFrameCounter++;
                if (animFrameCounter == frameTime)
                {
                    playerMode.animFrame++;
                    animFrameCounter = 0;
                }
                if (playerMode.animFrame >= anim[(int)playerMode.playerAnim].Count)
                    playerMode.animFrame = 0;
            }
            else             
            {
                playerMode.animFrame = 0;
                animFrameCounter = 0;
            }


        }
        public void go()
        {
            playerMode.go = true;
        }
        public void stop()
        {
            playerMode.go = false;
        }
        private Rectangle getBoundingBox(tileData tempData)
        {
            
            Rectangle destRect = new Rectangle();
            switch (gravity)
            {
                case Direction.right:
                    if (playerMode.direction)//flip
                    {
                        destRect.X = (int)(playerLoc[0]) + tempData.origin[0];
                        destRect.Y = (int)(playerLoc[1]) - tempData.origin[1];
                        destRect.Width = -1 * tempData.srcRect.Width;
                        destRect.Height = tempData.srcRect.Height;
                    }
                    else//normal
                    {
                        destRect.X = (int)(playerLoc[0]) - tempData.origin[0];
                        destRect.Y = (int)(playerLoc[1]) - tempData.origin[1];
                        destRect.Width = tempData.srcRect.Width;
                        destRect.Height = tempData.srcRect.Height;
                    }
                    break;
                case Direction.left:
                    if (playerMode.direction)//flip
                    {
                        destRect.X = (int)(playerLoc[0]) + tempData.origin[0];
                        destRect.Y = (int)(playerLoc[1]) + tempData.origin[1];
                        destRect.Width = -1 * tempData.srcRect.Width;
                        destRect.Height = -1 * tempData.srcRect.Height;
                    }
                    else//normal
                    {
                        destRect.X = (int)(playerLoc[0]) - tempData.origin[0];
                        destRect.Y = (int)(playerLoc[1]) + tempData.origin[1];
                        destRect.Width = tempData.srcRect.Width;
                        destRect.Height =  -1 * tempData.srcRect.Height;
                    }
                    break;
                case Direction.down:
                    if (playerMode.direction)//flip
                    {
                        destRect.X = (int)(playerLoc[0]) + tempData.origin[0];
                        destRect.Y = (int)(playerLoc[1]) - tempData.origin[1];
                        destRect.Width = -1 *tempData.srcRect.Height;
                        destRect.Height = -1 *tempData.srcRect.Width;
                        destRect.X = destRect.X + (tempData.origin[1] - tempData.origin[0]);
                        destRect.Y = destRect.Y + tempData.origin[0] + tempData.origin[1];
                    }
                    else//normal
                    {
                        destRect.X = (int)(playerLoc[0]) - tempData.origin[0];
                        destRect.Y = (int)(playerLoc[1]) - tempData.origin[1];
                        destRect.Width = -1 *tempData.srcRect.Height;
                        destRect.Height = tempData.srcRect.Width;
                        destRect.Y = destRect.Y + (tempData.origin[1] - tempData.origin[0]);
                        destRect.X = destRect.X + (tempData.origin[0] + tempData.origin[1]);
                    } 
                    break;
                case Direction.up:
                    if (playerMode.direction)//flip
                    {
                        destRect.X = (int)(playerLoc[0]) + tempData.origin[0];
                        destRect.Y = (int)(playerLoc[1]) - tempData.origin[1];
                        destRect.Width = tempData.srcRect.Height;
                        destRect.Height = tempData.srcRect.Width;
                        destRect.Y = destRect.Y + (tempData.origin[1] - tempData.origin[0]);
                        destRect.X = destRect.X - (tempData.origin[0] + tempData.origin[1]);
                    }
                    else//normal
                    {
                        destRect.X = (int)(playerLoc[0]) - tempData.origin[0];
                        destRect.Y = (int)(playerLoc[1]) - tempData.origin[1];
                        destRect.Width = tempData.srcRect.Height;
                        destRect.Height = -1 * tempData.srcRect.Width;
                        destRect.X = destRect.X - (tempData.origin[1] - tempData.origin[0]);
                        destRect.Y = destRect.Y + tempData.origin[0] + tempData.origin[1];
                    } 
                    break;
            }
            return destRect;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            gameTimer.Draw(gameTime, spriteBatch);
            deathCounter.Draw(gameTime, spriteBatch);
            tileData tempData;
            if (dither)
            {
                tempData = ditherAnim[(int)playerMode.playerAnim][playerMode.animFrame];
            }
            else
            {
                tempData = anim[(int)playerMode.playerAnim][playerMode.animFrame];
            }
            Rectangle destRect;      
            if (gravity == Direction.up || gravity == Direction.down)
            {
                Direction temp = gravity;
                gravity = Direction.right;
                destRect = getBoundingBox(tempData);
                gravity = temp;
            }
            else
            {
                destRect = getBoundingBox(tempData);
            }

            double rot = 0;
            switch (gravity)
            {
                case Direction.down:
                    if (!playerMode.direction)
                    {
                        destRect.Y = destRect.Y + (tempData.origin[1] - tempData.origin[0]);
                        destRect.X = destRect.X + (tempData.origin[0] + tempData.origin[1]);
                    }
                    else
                    {
                        destRect.X = destRect.X + (tempData.origin[1] - tempData.origin[0]);
                        destRect.Y = destRect.Y + tempData.origin[0] + tempData.origin[1];
                    }
                    rot = Math.PI / 2.0f;

                    break;
                case Direction.up:
                    if (!playerMode.direction)
                    {
                        destRect.X = destRect.X - (tempData.origin[1] - tempData.origin[0]);
                        destRect.Y = destRect.Y + tempData.origin[0] + tempData.origin[1];
                    }
                    else
                    {
                        destRect.Y = destRect.Y + (tempData.origin[1] - tempData.origin[0]);
                        destRect.X = destRect.X - (tempData.origin[0] + tempData.origin[1]);
                    }
                    rot = 3 * Math.PI / 2.0f;
                    break;
                default:
                    break;
            }
            if (dither)
            {
                spriteBatch.Draw(ditherSet, destRect, tempData.srcRect, Color.White, (float)rot, new Vector2(), SpriteEffects.None, 0);
            }
            else
            {
                spriteBatch.Draw(tileSet, destRect, tempData.srcRect, Color.White, (float)rot, new Vector2(), SpriteEffects.None, 0);
            }
            //turn arrow stuff
            if (turnArrowShow)
            {
                SpriteEffects effects = SpriteEffects.None;
                if (turnArrowDirection)
                {
                    turnArrowRot = turnArrowRot - (float)Math.PI / 16;
                    effects = SpriteEffects.FlipHorizontally;
                }
                else
                {
                    turnArrowRot = turnArrowRot + (float)Math.PI / 16;
                    
                }
                spriteBatch.Draw(turnArrow, turnArrowDestRect, turnArrowSrcRect, Color.White, turnArrowRot, turnArrowOrigin, effects, 0);
            }

            //gravity tester
            /*if (gravity == Direction.up || gravity == Direction.down)
            {
                Direction temp = gravity;
                gravity = Direction.right;
                destRect = getBoundingBox(tempData);
                gravity = temp;
            }
            else
            {
                destRect = getBoundingBox(tempData);
            }

            Rectangle pixel = new Rectangle(0, 0, 1, 1);
            pixel.X = 168;
            pixel.Y = 67;

            Rectangle origin = new Rectangle(0, 0, 1, 1);
            origin.Y = destRect.Y + tempData.origin[1];

            
            spriteBatch.Draw(tileSet, destRect, pixel, Color.Navy, 0, new Vector2(), SpriteEffects.None, 0);
            
            if (!playerMode.direction)
            {
                origin.X = destRect.X + tempData.origin[0];               
                destRect.Y = destRect.Y + (tempData.origin[1] - tempData.origin[0]);
                destRect.X = destRect.X + (tempData.origin[0] + tempData.origin[1]);
            }
            else
            {
                origin.X = destRect.X - tempData.origin[0];
                destRect.X = destRect.X + (tempData.origin[1] - tempData.origin[0]);
                destRect.Y = destRect.Y + tempData.origin[0] + tempData.origin[1];
            }
            if (rot == 0)
                rot = Math.PI / 2.0f;
            spriteBatch.Draw(tileSet, destRect, pixel, Color.AliceBlue, (float) rot, new Vector2(), SpriteEffects.None, 0);

            spriteBatch.Draw(tileSet, origin, pixel, Color.Tomato, 0, new Vector2(), SpriteEffects.None, 0);

            destRect = getBoundingBox(tempData);//draw calculated bounding box
            spriteBatch.Draw(tileSet, destRect, pixel, Color.YellowGreen, 0, new Vector2(), SpriteEffects.None, 0);*/

        }
        public void reSetPlayerLoc()
        {
            stop();
            int[] tempLoc = g.map.getPlayerStartLoc();
            playerMode.playerAnim = animMode.stand;
            playerMode.animFrame = 0;
            playerLoc[0] = 30 * tempLoc[0] + 15;
            playerLoc[1] = 30 * tempLoc[1] + 12;
            hSpeed = 0;
            vSpeed = 0;
            waitCounter = 0;
            dither = false;
            inDither = false;
            gravity = g.map.getPlayerStartDirection();
            waitDirection = gravity;
            gravityPrev = gravity;
        }

        public void addToSaveData(SaveData dat)
        {
            dat.animFrame = playerMode.animFrame;
            dat.animFrameCounter = animFrameCounter;
            dat.ceiling = ceiling;
            dat.deaths = deathCounter.value;
            dat.direction = playerMode.direction;
            dat.dither = dither;
            dat.ditherDirection = ditherDirection;
            dat.go = playerMode.go;
            dat.gravity = gravity;
            dat.gravityPrev = gravityPrev;
            dat.grounded = grounded;
            dat.hDirect = hDirect;
            dat.hPos = hPos;
            dat.hSpeed = hSpeed;
            dat.inDither = inDither;
            dat.jump = jump;
            dat.jumpStartTime = jumpStartTime;
            dat.playerAnim = playerMode.playerAnim;
            dat.playerLoc = playerLoc;
            dat.time = gameTimer.value;
            dat.vDirect = vDirect;
            dat.vPos = vPos;
            dat.vSpeed = vSpeed;
            dat.waitCounter = waitCounter;
            dat.waitDirection = waitDirection;
        }
        public void loadFromSaveData(SaveData dat)
        {
            playerMode.animFrame = dat.animFrame;
            animFrameCounter = dat.animFrameCounter;
            ceiling = dat.ceiling;
            deathCounter.value = dat.deaths;
            playerMode.direction = dat.direction;
            dither = dat.dither;
            ditherDirection = dat.ditherDirection;
            playerMode.go = dat.go;
            gravity = dat.gravity;
            gravityPrev = dat.gravityPrev;
            grounded = dat.grounded;
            hDirect = dat.hDirect;
            hPos = dat.hPos;
            hSpeed = dat.hSpeed;
            inDither = dat.inDither;
            jump = dat.jump;
            jumpStartTime = dat.jumpStartTime;
            playerMode.playerAnim = dat.playerAnim;
            playerLoc = dat.playerLoc;
            gameTimer.value = dat.time;
            vDirect = dat.vDirect;
            vPos = dat.vPos;
            vSpeed = dat.vSpeed;
            waitCounter = dat.waitCounter;
            waitDirection = dat.gravity;
            stop();
        }
    }
}