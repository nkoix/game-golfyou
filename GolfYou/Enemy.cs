using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using TiledCS;
using System;

namespace GolfYou
{
    public class Enemy //Enemy class
    {
        bool idle;
        Rectangle sourceRectangle;
        int[] walkingAnimation = {6, 7, 8, 9, 10, 11}; // Frames when enemy is moving
        int[] idleAnimation = {0, 1, 2, 3, 4, 5}; // Frames when enemy is idle
        Texture2D left;
        Texture2D right;
        Vector2 position;
        double yVelo;
        bool forward; // Determines whether enemy is facing left or right
        int frame;
        bool incFrame;
        Rectangle hitBox;
        int half = 0;
        int halfcap = 10; // Slows down animations with larger numbers (non-fixable fps, might want to change later to account for gameTime)
        const double velocity = 0.08; // How fast velocity is for enemies
        bool onPlatform;
        Rectangle platform = new Rectangle(-1, -2, 0, 0); // Dumb values so the platform identifying code works
        int[] inDeath = {12, 13, 14, 15, 0}; // Death animations and removal (if at end of array then enemy will be removed entirely)
        int iDeath; // Index of death array

        public Enemy(ContentManager Content, bool stationary, Vector2 pos)
        {
            idle = stationary;
            forward = true;
            frame = 0;
            incFrame = true;

            left = Content.Load<Texture2D>("Sprites/EnemyLeft");
            right = Content.Load<Texture2D>("Sprites/EnemyRight");
            position = pos;
            hitBox = new Rectangle((int)pos.X, (int)pos.Y, 28, 28);
            yVelo = 0;
            onPlatform = false;
            iDeath = -1;
        }

        public void updateEnemy(TiledLayer collisionLayer)
        {
            half++;
            if (half==halfcap)
            {
                half = 0;
                if (incFrame)
                {
                    frame++;
                    if (iDeath>=0 && frame>=inDeath.Length-1)
                    {
                        frame = inDeath.Length-1;
                        incFrame = false;
                    }
                    else if (idle && frame>=idleAnimation.Length-1)
                    {
                        frame = idleAnimation.Length-1;
                        incFrame = false;
                    }
                    else if (!idle && frame>=walkingAnimation.Length-1)
                    {
                        frame = walkingAnimation.Length-1;
                        incFrame = false;
                    }
                }
                else
                {
                    frame--;
                    if (frame==0) incFrame = true;
                }
            }
            HandleCollisions(collisionLayer);
            position.Y += (float)yVelo;
            hitBox.X = (int)position.X+10;
            hitBox.Y = (int)position.Y;
            if (iDeath>=0) iDeath++;
        }

        public void drawEnemy(SpriteBatch _spriteBatch)
        {
            Rectangle destinationRectangle = new Rectangle((int)position.X, (int)position.Y, 24*2, 24*2);
            if (iDeath>=0)
            {
                if (frame >= inDeath.Length) frame = inDeath.Length-1;
                sourceRectangle = getAnimFrame(inDeath[frame]);
            }
            else if (idle) sourceRectangle = getAnimFrame(idleAnimation[frame]);
            else sourceRectangle = getAnimFrame(walkingAnimation[frame]);
            if (idle || forward) _spriteBatch.Draw(right, destinationRectangle, sourceRectangle, Color.White);
            else _spriteBatch.Draw(left, destinationRectangle, sourceRectangle, Color.White);
        }

        public void drawHitBoxes(SpriteBatch _spriteBatch)
        {
            DrawRectangle(_spriteBatch, hitBox);
        }

        private Rectangle getAnimFrame(int i) // Determines what section of the image source to take from
        {
            return new Rectangle(10, 48*(i+1)-40, 24, 24);
        }

        private void DrawRectangle(SpriteBatch sb, Rectangle Rec)
        {
            Vector2 pos = new Vector2(Rec.X, Rec.Y);
            sb.Draw(left, pos, Rec,
                Color.Purple * 1.0f,
                0, Vector2.Zero, 1.0f,
                SpriteEffects.None, 0.00001f);
        }

        public Rectangle getHitBox()
        {
            return hitBox;
        }

        public void HandleCollisions(TiledLayer collisionLayer)
        {
            foreach (var obj in collisionLayer.objects)
            {
                Rectangle objRect = new Rectangle((int)obj.x, (int)obj.y, (int)obj.width, (int)obj.height);
                bool landingPlatform = objRect.Equals(platform);
                bool within = ((hitBox.Right>=objRect.Left && hitBox.Right <= objRect.Right) || (hitBox.Left>=objRect.Right && hitBox.Left <= objRect.Left));
                if (!onPlatform && objRect.Intersects(hitBox)) // If enemy is stuck in box, push upwards
                {
                    land(objRect);
                }
                if (!onPlatform)
                {
                    bool isAbove = within && hitBox.Bottom<objRect.Top; // Calculates if enemy is directly above collision layer
                    bool willLand = isAbove && hitBox.Bottom+yVelo>=objRect.Top; // Calculates if enemy will pass through object while falling
                    if (willLand)
                    {
                        land(objRect);
                    }
                    else
                    {
                        yVelo += velocity;
                        onPlatform = false;
                    }
                }
                else if (!idle)
                {
                    if (!landingPlatform) // Don't care if enemy doesn't move or if it collides with the same platform it's standing on
                    {
                        bool inBetween = ((hitBox.Top < objRect.Bottom && hitBox.Top > objRect.Top) || (hitBox.Bottom > objRect.Top && hitBox.Bottom < objRect.Bottom));
                        bool leftCollide = forward && inBetween && Math.Abs(hitBox.Right-objRect.Left)<2;
                        bool rightCollide = !forward && inBetween && (hitBox.Left == objRect.Right);
                        if (leftCollide || rightCollide) forward = !forward;
                    }
                    else if (!within) // Enemy goes into free fall if they walk off a platform
                    {
                        onPlatform = false;
                        platform = new Rectangle(-1, -2, 0, 0);
                    }
                }
            }
            if (idle) return;
            if (forward) position.X++;
            else position.X--;
        }

        private void land(Rectangle y)
        {
            yVelo = 0;
            position.Y = y.Top-hitBox.Height-6;
            onPlatform = true;
            platform = y;
        }

        public bool getOnPlatform()
        {
            return onPlatform;
        }

        public bool isDead()
        {
            return iDeath==inDeath.Length;
        }

        public void setDeath()
        {
            iDeath++;
        }

        public bool isDying()
        {
            return iDeath>=0;
        }
    }
}