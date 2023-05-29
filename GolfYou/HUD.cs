using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolfYou
{
    public class HUD //HUD class for display prompts, will need some work once we have scrolling levels since some hud elements are hard coded
    {
        Texture2D ClubOption;
        Texture2D VelBars;
        Texture2D AngleArrow;
        Rectangle[] sourceRectanglesClubs;
        Rectangle[] sourceRectanglesVelBars;

        private float timer;
        private float optionTimer1;
        private float optionTimer2;
        private float rotation;
        private float angle;

        private int VelBarsIndex;
        private int velModifier;
        private int prevHittingMode;
        private const float MaxAngle = 1.5f; // 45 degrees, I made these values before realizing that the rotation paramter in _spritebatch.draw() starts at 90 degrees, not 0 degrees, which is why Max and Min are swapped
        private const float MinAngle = 0f; // 90 degrees, essentially take this float and multiply by 30 and subtract that number from 90 to get the angle


        private bool barDirectionRight;
        private bool prevAnglePutting;
        private bool angleDirection;
        public void loadHudContent(ContentManager Content)
        {
            ClubOption = Content.Load<Texture2D>("Sprites/ClubOptions");
            VelBars = Content.Load<Texture2D>("Sprites/Bars/BarAll");
            AngleArrow = Content.Load<Texture2D>("Sprites/Arrow");

            sourceRectanglesClubs = new Rectangle[2];

            timer = 0f;
            optionTimer1 = 0f;
            optionTimer2 = 0f;

            barDirectionRight = true;
            angleDirection = true;
            prevAnglePutting = false;

            for (int i = 0; i < 2; i++)
            {
                sourceRectanglesClubs[i] = new Rectangle(i * 15, 0, 15, 18);
            }
            sourceRectanglesVelBars = new Rectangle[82];
            for (int j = 0; j < 2; j++) //This loop looks a little different since I couldn't fit all 82 sprites on the same line in the image, so its a matrix of size 2x41
            {
                for (int i = 0 + j * 41; i < 41 + j * 41; i++)
                {
                    sourceRectanglesVelBars[i] = new Rectangle(i * 91 - j * 3731, 0 + j * 17, 91, 17);
                }
            }

        }
        public void drawHudContent(SpriteBatch _spriteBatch, GameTime gameTime, int hittingMode, bool isPutting, bool wasPutting, Vector2 playerPosition, bool anglePutting, int facing)
        {
            if (hittingMode == 0 && prevHittingMode != hittingMode || hittingMode == 0 && optionTimer1 > 0) //This and the following cases are for displaying the golf club options on the top right, this is what will need work once we start scrolling levels
            {
                optionTimer2 = 0;
                if (anglePutting || isPutting) 
                {
                    optionTimer1 = 0;
                    
                }
                else if (optionTimer1 < 1000)
                {
                    _spriteBatch.Draw(ClubOption, new Rectangle((int)playerPosition.X, (int)playerPosition.Y - 40, 30, 36), sourceRectanglesClubs[1], Color.White);
                    optionTimer1 += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }
                else
                {
                    optionTimer1 = 0;
                }
            }
            else if (hittingMode == 1 && prevHittingMode != hittingMode || hittingMode == 1 && optionTimer2 > 0)
            {
                optionTimer1 = 0;
                if (anglePutting || isPutting)
                {
                    optionTimer2 = 0;

                }
                else if (optionTimer2 < 1000)
                {
                    _spriteBatch.Draw(ClubOption, new Rectangle((int)playerPosition.X + 10, (int)playerPosition.Y - 40, 30, 36), sourceRectanglesClubs[0], Color.White);
                    optionTimer2 += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }
                else
                {
                    optionTimer2 = 0;
                }
            }
            if ((isPutting || wasPutting) && optionTimer1 == 0 && optionTimer2 == 0) //Velocity bar is tied to the players location, appearing above their head
            {
                _spriteBatch.Draw(VelBars, new Rectangle((int)playerPosition.X - 28, (int)playerPosition.Y - 20, 91, 17), sourceRectanglesVelBars[VelBarsIndex], Color.White);
            }
            else if (anglePutting && optionTimer1 == 0 && optionTimer2 == 0) //This is the arrow display for the angle, again its tied to the players position
            {
                Vector2 origin = new Vector2(AngleArrow.Width / 2, AngleArrow.Height);
                if (facing == 1) { _spriteBatch.Draw(AngleArrow, new Rectangle((int)playerPosition.X + 36, (int)playerPosition.Y, 24, 36), null, Color.White, rotation, origin, SpriteEffects.None, 0f); }
                else if (facing == 0) { _spriteBatch.Draw(AngleArrow, new Rectangle((int)playerPosition.X, (int)playerPosition.Y, 24, 36), null, Color.White, -rotation, origin, SpriteEffects.None, 0f); }

            }
            prevHittingMode = hittingMode;
        }

        public void drawStaticHudContent(SpriteBatch _spriteBatch, int hittingMode)
        {
            if (hittingMode == 0)
            {
                _spriteBatch.Draw(ClubOption, new Rectangle(732, 0, 30, 36), sourceRectanglesClubs[1], Color.White);
            }
            else
            {
                _spriteBatch.Draw(ClubOption, new Rectangle(732, 0, 30, 36), sourceRectanglesClubs[0], Color.White);
            }
        }

        public void playHudAnimations(GameTime gameTime, bool isPutting, bool isRolling, bool anglePutting, int facing)
        {
            calcRotation(anglePutting, facing); //CalcRotation is here and not in drawHudContent() because the order these functions are called in the main game loop, otherwise
                                                //calcRotation would be called after the physics are applied, leading to wonkiness when driving
            if (isPutting)
            {
                float threshold = .01f;
                if (timer > threshold) //Animation for the velocity bar, goes up by three because otherwise it would be too slow filling up and draining down
                {
                    if (VelBarsIndex < 80 && barDirectionRight)
                    {
                        VelBarsIndex+=4;
                    }
                    else if (VelBarsIndex >= 80 && barDirectionRight)
                    {
                        barDirectionRight = false;
                    }
                    else if (VelBarsIndex > 0 && !barDirectionRight)
                    {
                        VelBarsIndex-=4;
                    }
                    else if (VelBarsIndex == 0 && !barDirectionRight)
                    {
                        barDirectionRight= true;
                    }
                    timer = 0;
                }
                else
                {
                    timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }
            }
            else if (isRolling) //resets the velocity bar to the lowest value after a put is completed so every putt starts the same
            {
                velModifier = VelBarsIndex;
                VelBarsIndex = 0;
            }
        }

        public int getVelModifier()
        {
            return velModifier;
        }

        public float getAngle()
        {
            return angle;
        }

        private float calcRotation(bool anglePutting, int facing) //Function for calculating the rotation of the arrow, see above (ln 28) for explanation on 'angle' variable math
        {
            if (anglePutting && !prevAnglePutting)
            {
                rotation = MaxAngle;
            }
            else if (anglePutting && prevAnglePutting) 
            {
                rotation = MathHelper.Clamp(rotation, MinAngle, MaxAngle);
                if (rotation < MaxAngle && angleDirection) 
                {   
                    rotation += .05f;
                    angle = 90 - (rotation * 30);
                }
                else if (rotation == MaxAngle && angleDirection)
                {
                    angleDirection = false;
                }
                if (rotation > MinAngle && !angleDirection) 
                { 
                    rotation -= .05f;
                    angle = 90 - (rotation * 30);
                }
                else if (rotation == MinAngle && !angleDirection) {angleDirection= true; }

            }
            prevAnglePutting = anglePutting;
            return rotation;
        }
    }
}
