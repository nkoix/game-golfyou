using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;

namespace GolfYou
{
    public class Player //Player class to handle position, hitbox, input, and sprites
    {
        Texture2D runRightSet;
        Texture2D runLeftSet;
        Texture2D puttingRightSet;
        Texture2D puttingLeftSet;
        Texture2D head;

        Rectangle[] sourceRectanglesRun;
        Rectangle[] sourceRectanglesPutting;
        Rectangle playerHitbox = new Rectangle(0, 200, 32, 32);

        Vector2 origin;
        float rotation;

        byte currentAnimationIndexRun;
        byte currentAnimationIndexPuttingRight;
        byte currentAnimationIndexPuttingLeft;
        private int threshold;
        float timer;
        private float movement = 0.0f;
        private int facing = 1;
        public bool rolling; //Is the player rolling? This determines the drag factor when moving, rolling = less drag so momentum is carried farther
        private bool isPutting; //Is the player putting?
        private bool wasPutting; //Was the player just putting?
        private bool anglePutting; //Angle the player chose for the current putt
        private int hittingMode; // 0 = drive, 1 = tap / putter
        private bool EPressed;

        private const Buttons puttButton = Buttons.A;

        public void loadPlayerContent(ContentManager Content)
        {
            runRightSet = Content.Load<Texture2D>("Sprites/WalkanimFlipped");
            runLeftSet = Content.Load<Texture2D>("Sprites/Walkanim");
            puttingRightSet = Content.Load<Texture2D>("Sprites/PlayerPuttingAll");
            puttingLeftSet = Content.Load<Texture2D>("Sprites/PlayerPuttingAllFlipped");
            head = Content.Load<Texture2D>("Sprites/Head");

            currentAnimationIndexRun = 0;
            currentAnimationIndexPuttingLeft = 5;
            currentAnimationIndexPuttingRight = 0;
            rotation = 0;
            timer = 0;
            threshold = 10;
            hittingMode = 0;

            rolling = false;
            isPutting = false;
            EPressed = false;

            origin = new Vector2(head.Width / 2f, head.Height / 2f);

            sourceRectanglesRun = new Rectangle[4];
            for (int i = 0; i < 4; i++)
            {
                sourceRectanglesRun[i] = new Rectangle(i * 16, 0, 16, 16);
            }
            sourceRectanglesPutting = new Rectangle[6];
            for (int i = 0; i < 6; i++)
            {
                sourceRectanglesPutting[i] = new Rectangle(i * 64 + 23, 24, 18, 20);
            }
        }

        public void drawPlayer(SpriteBatch _spriteBatch, GameTime gameTime, Vector2 velocity) //Draws the player sprite depending on the action, most of these are self-explanatory 
        {
            if (facing == 1 && movement > 0 && !isPutting && !wasPutting && !rolling && !anglePutting)
            {
                _spriteBatch.Draw(runRightSet, playerHitbox, sourceRectanglesRun[currentAnimationIndexRun], Color.White);
            }
            else if (facing == 1 && movement == 0 && !isPutting && !wasPutting && !rolling && !anglePutting) //The 'idle' animation is just a frame from the running animation, since we have so few frames to work with
            {
                _spriteBatch.Draw(runRightSet, playerHitbox, sourceRectanglesRun[1], Color.White);
            }
            else if (facing == 1 && isPutting || facing == 1 && wasPutting || facing == 1 && anglePutting)
            {
                _spriteBatch.Draw(puttingRightSet, new Rectangle(playerHitbox.X, playerHitbox.Y, 34, 40), sourceRectanglesPutting[currentAnimationIndexPuttingRight], Color.White);
            }
            else if (facing == 0 && movement < 0 && !isPutting && !wasPutting && !rolling && !anglePutting)
            {
                _spriteBatch.Draw(runLeftSet, playerHitbox, sourceRectanglesRun[currentAnimationIndexRun], Color.White);
            }
            else if (facing == 0 && movement == 0 && !isPutting & !wasPutting && !rolling && !anglePutting)
            {
                _spriteBatch.Draw(runLeftSet, playerHitbox, sourceRectanglesRun[0], Color.White);
            }
            else if (facing == 0 && isPutting || facing == 0 && wasPutting || facing == 0 && anglePutting)
            {
                _spriteBatch.Draw(puttingLeftSet, new Rectangle(playerHitbox.X, playerHitbox.Y, 34, 40), sourceRectanglesPutting[currentAnimationIndexPuttingLeft], Color.White);
            }
            else if (rolling) //Head sprite is drawn with rotation tied to x velocity, the faster the player is going, the faster the rotation.
            {
                _spriteBatch.Draw(head, new Rectangle(playerHitbox.X + 12, playerHitbox.Y + 20, 26, 20), null, Color.White, rotation, origin, SpriteEffects.None, 0f);
                rotation = -.1f * velocity.X;
            }
        }

        public void playAnimation(GameTime gameTime)
        {
            if (facing == 1 && movement > 0 && !isPutting)
            {
                threshold = 150;
                if (timer > threshold)
                {
                    if (currentAnimationIndexRun < 3)
                    {
                        currentAnimationIndexRun++;
                    }
                    else
                    {
                        currentAnimationIndexRun = 0;
                    }
                    timer = 0;
                }
                else
                {
                    timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }
            }
            else if (facing == 0 && movement < 0 && !isPutting)
            {
                threshold = 150;
                if (timer > threshold)
                {
                    if (currentAnimationIndexRun > 0)
                    {
                        currentAnimationIndexRun--;
                    }
                    else
                    {
                        currentAnimationIndexRun = 3;
                    }
                    timer = 0;
                }
                else
                {
                    timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }
            }
            else if (facing == 1 && (isPutting || anglePutting))
            {
                threshold = 100;
                if (timer > threshold)
                {
                    if (currentAnimationIndexPuttingRight < 5)
                    {
                        currentAnimationIndexPuttingRight++;
                    }
                    timer = 0;
                }
                else
                {
                    timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }
            }
            else if (facing == 1 && wasPutting) //This is the 'downswing' animation where the player is hitting themselves
            {
                threshold = 15;
                if (timer > threshold)
                {
                    if (currentAnimationIndexPuttingRight > 0)
                    {
                        currentAnimationIndexPuttingRight--;
                    }
                    else
                    {
                        wasPutting = false;
                        rolling = true;
                    }
                    timer = 0;
                }
                else
                {
                    timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }


            }
            else if (facing == 0 && (isPutting || anglePutting))
            {
                threshold = 100;
                if (timer > threshold)
                {
                    if (currentAnimationIndexPuttingLeft > 0 )
                    {
                        currentAnimationIndexPuttingLeft--;
                    }
                    timer = 0;
                }
                else
                {
                    timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }
            }
            else if (facing == 0 && wasPutting) //This is the 'downswing' animation where the player is hitting themselves
            {
                threshold = 15;
                if (timer > threshold)
                {
                    if (currentAnimationIndexPuttingLeft < 5)
                    {
                        currentAnimationIndexPuttingLeft++;
                    }
                    else
                    {
                        wasPutting = false;
                        rolling = true;
                    }
                    timer = 0;
                }
                else
                {
                    timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }


            }
        }

        public void handlePlayerInput(
                    KeyboardState keyboardState,
                    GamePadState gamePadState,
                    GameTime gameTime)
        {
            // Get analog horizontal movement.
            if (!rolling && !isPutting && !wasPutting && !anglePutting) { movement = gamePadState.ThumbSticks.Left.X; }
            

            // Ignore small movements to prevent running in place.
            if (Math.Abs(movement) < 0.5f)
                movement = 0.0f;

            // If any digital horizontal movement input is found, override the analog movement.
            if ((gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                keyboardState.IsKeyDown(Keys.Left) ||
                keyboardState.IsKeyDown(Keys.A) || gamePadState.ThumbSticks.Left.X < 0) && !isPutting && !wasPutting && !rolling && !anglePutting)
            {
                facing = 0;
                movement = -1.0f;
            }
            else if ((gamePadState.IsButtonDown(Buttons.DPadRight) ||
                     keyboardState.IsKeyDown(Keys.Right) ||
                     keyboardState.IsKeyDown(Keys.D) || gamePadState.ThumbSticks.Left.X > 0) && !isPutting && !wasPutting && !rolling && !anglePutting)
            {
                facing = 1;
                movement = 1.0f;
            }


            // Check if the player wants to putt.
            myKeyboard.GetState(); //See below for an explanation on 'myKeboard'
            myGamepad.GetState();
            if (((myKeyboard.HasBeenPressed(Keys.Space) || myGamepad.HasBeenPressed(Buttons.A)) && isPutting && !anglePutting)) //The order of these statements matters, its probably slightly spaghetti, but it does work.
            {
                movement = 0.0f;
                isPutting = false;
                wasPutting = true;

            }
            else if ((myKeyboard.HasBeenPressed(Keys.Space) || myGamepad.HasBeenPressed(Buttons.A)) && anglePutting && hittingMode == 0|| (myKeyboard.HasBeenPressed(Keys.Space) || myGamepad.HasBeenPressed(Buttons.A)) && hittingMode == 1 && !isPutting) 
            {
                movement = 0.0f;
                anglePutting = false;
                isPutting = true;
                
            }

            else if ((myKeyboard.HasBeenPressed(Keys.Space) || myGamepad.HasBeenPressed(Buttons.A)) && !rolling && !isPutting && !anglePutting && !wasPutting && hittingMode == 0)
            {
                movement = 0.0f;
                anglePutting = true;

            }

            if (keyboardState.IsKeyDown(Keys.C) || gamePadState.IsButtonDown(Buttons.B))
            {
                isPutting = false;
                wasPutting = false;
                anglePutting= false;
                currentAnimationIndexPuttingRight = 0;
                currentAnimationIndexPuttingLeft = 5;
            }

            if ((myKeyboard.HasBeenPressed(Keys.Q) || myGamepad.HasBeenPressed(Buttons.Y)) && !isPutting && !wasPutting && !anglePutting)
            {
                if (hittingMode == 0) { hittingMode = 1; }
                else { hittingMode = 0; }
            }
            if ((myKeyboard.HasBeenPressed(Keys.E) || myGamepad.HasBeenPressed(Buttons.X)))
            {
                EPressed = true;
            }
            else if (((!myKeyboard.HasBeenPressed(Keys.E) && !myGamepad.HasBeenPressed(Buttons.X))))
            {
                EPressed = false;
            }


            playerHitbox.X += (int)movement;


        }

        //Lots of getters down here to share info with HUD and Physics classes


        public bool wasEPressed()
        {
            return EPressed;
        }

        public bool getIsPutting()
        {
            return isPutting; 
        }

        public Vector2 getPosition()
        {
            return new Vector2(playerHitbox.X, playerHitbox.Y);
        }

        public float getMovement()
        {
            return movement;
        }

        public void setPlayerPosition(Vector2 playerPosition)
        {
            playerHitbox.X = (int)playerPosition.X;
            playerHitbox.Y = (int)playerPosition.Y;
        }

        public bool getWasPutting()
        {
            return wasPutting;
        }

        public int getFacing()
        {
            return facing;
        }

        public int getHittingMode()
        {
            return hittingMode;
        }

        public bool getAnglePutting()
        {
            return anglePutting;
        }

        public Rectangle getPlayerHitbox()
        {
            return playerHitbox;
        }

        public void setSpawnLocation(Vector2 spawnCoords)
        {
            playerHitbox.X = (int)spawnCoords.X;
            playerHitbox.Y = (int)spawnCoords.Y;
        }

        public class myKeyboard // This is a very simple class to ensure that actions are only carried out once per key hit, otherwise putting actions would happen for as long as the space bar was held down
        {
            static KeyboardState currentKeyState;
            static KeyboardState previousKeyState;

            public static KeyboardState GetState() //Get state from directly from the XNA framework
            {
                previousKeyState = currentKeyState;
                currentKeyState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
                return currentKeyState;
            }

            public static bool HasBeenPressed(Keys key) //If a key is currently pressed and wasn't pressed on the tick before, then send true, else its false,
                                                        //so if its held down, then IsKeyDown on the tick before and current are both true and thus it will return false
            {
                return currentKeyState.IsKeyDown(key) && !previousKeyState.IsKeyDown(key);
            }

        }
        public class myGamepad // This is a very simple class to ensure that actions are only carried out once per key hit, otherwise putting actions would happen for as long as the space bar was held down
        {
            static GamePadState currentButtonState;
            static GamePadState previousButtonState;

            public static GamePadState GetState() //Get state from directly from the XNA framework
            {
                previousButtonState = currentButtonState;
                currentButtonState = Microsoft.Xna.Framework.Input.GamePad.GetState(0);
                return currentButtonState;
            }

            public static bool HasBeenPressed(Buttons button) //If a key is currently pressed and wasn't pressed on the tick before, then send true, else its false,
                                                        //so if its held down, then IsKeyDown on the tick before and current are both true and thus it will return false
            {
                return currentButtonState.IsButtonDown(button) && !previousButtonState.IsButtonDown(button);
            }
        }

    }

}