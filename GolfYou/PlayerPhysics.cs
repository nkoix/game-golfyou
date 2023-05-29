using Microsoft.Xna.Framework;
using System;
using TiledCS;

namespace GolfYou
{
    public class PlayerPhysics //Player physics class, much of this is lifted from my (Will's) Levels 1-3 implementations
    {
        private const float MoveAcceleration = 13000.0f;
        private const float MaxMoveSpeed = 1750.0f;
        private float GroundDragFactor = 0.48f;
        private const float AirDragFactor = 0.98f;
        private bool IsOnGround;
        private bool prevWasPutting = false;
        private float timer = 0.0f;
        Vector2 velocity;

        private const float GravityAcceleration = 3400.0f; //This will control how floating our game feels in the air, right now (at 3400), gravity feels a little strong to me (Will)
        private const float MaxFallSpeed = 550.0f;


        public Vector2 getVelocity()
        {
            return velocity;
        }

        public Vector2 ApplyPhysics(GameTime gameTime, int windowHeight, int windowWidth, ref bool isRolling,
            Rectangle player, float movement, bool wasPutting, int facing, int hittingMode, int velModifier, float angle, TiledLayer collisionLayer) //Main workhorse of this class, lots of paramters from player class, which is a little annoying
        {
            Vector2 playerPosition = new Vector2(player.X, player.Y);
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = playerPosition;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            velocity.X += movement * MoveAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

            velocity = DoDrive(velocity, gameTime, ref isRolling, wasPutting, facing, hittingMode, velModifier, angle); //This controls the physics when the player is putting, both for a drive or a tap

            // Apply pseudo-drag horizontally.
            if (IsOnGround || !isRolling)
                velocity.X *= GroundDragFactor;
            else
                velocity.X *= AirDragFactor;

            // Prevent the player from running faster than his top speed.            
            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            // Apply velocity.
            playerPosition += velocity * elapsed;
            playerPosition = new Vector2((float)Math.Round(playerPosition.X), (float)Math.Round(playerPosition.Y));

            // If the player is now colliding with the level, separate them.
            //playerPosition.X = MathHelper.Clamp(playerPosition.X, 0, windowWidth - 60);
            //playerPosition.Y = MathHelper.Clamp(playerPosition.Y, 0, windowHeight);
            player.X = (int)playerPosition.X;
            player.Y = (int)playerPosition.Y;
            playerPosition = HandleCollisions(gameTime, player, collisionLayer);

            // If the collision stopped us from moving, reset the velocity to zero.
            if (playerPosition.X == previousPosition.X)
                velocity.X = 0;

            if (playerPosition.Y == previousPosition.Y)
                velocity.Y = 0;



            if (velocity.Y == 0)
            {
                IsOnGround = true;
            }
            else
            {
                IsOnGround = false;
            }
            return playerPosition;
        }
        private Vector2 DoDrive(Vector2 velocity, GameTime gameTime, ref bool isRolling, bool wasPutting, int facing, int hittingMode, int velModifier, float angle) //Physics for post-putt and rolling
        {
            int threshold = 1; //This acts as a buffer of time between when a player 'hits' themselves with the golf club and when the physics actually take effect,
                               //as the players X velocity is < 5 at the very beginning of the roll

            if (isRolling && timer > threshold)
            {
                GroundDragFactor = .96f; //Change the ground factor to have less friction when the player is rolling
                if (facing == 1 && velocity.X <= 5 || facing == 0 && velocity.X >= -5) //If the player has come close to a stop, kick them out of a roll and reset the ground drag factor
                {
                    isRolling = false;
                    GroundDragFactor = .48f;
                    timer = 0;
                }
            }
            else if (isRolling && timer < threshold)
            {
                timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }
            if (prevWasPutting && !wasPutting && hittingMode == 0) //These physics determine the players initial x and y velocity when they finish putting themselves
            {
                if (facing == 1)
                {
                    angle = angle * (float)(Math.PI / 180); //Need to convert to radians from degrees since thats what Math.Sin and Math.Cos use
                    velocity.Y = (-50f * velModifier) * (float)Math.Sin(angle); //These formulas are classic x and y componenet derivations, y = Length * sin(angle) and x = Length * cos(angle)
                    velocity.X = 50 * velModifier * (float)Math.Cos(angle);
                }
                else
                {
                    angle = angle * (float)(Math.PI / 180); //Need to convert to radians from degrees since thats what Math.Sin and Math.Cos use
                    velocity.Y = (-50f * velModifier) * (float)Math.Sin(angle);
                    velocity.X = -50 * velModifier * (float)Math.Cos(angle);
                }
                prevWasPutting = wasPutting;
                return new Vector2(velocity.X, velocity.Y);
            }
            else if (prevWasPutting && !wasPutting && hittingMode == 1) //Much simpler physics for tapping, as there are no angles involved, might attach hurtbox to this action to differentiate it from driving more.
            {
                if (facing == 1)
                {
                    velocity.Y = 0;
                    velocity.X = 25 * velModifier;
                }
                else
                {
                    velocity.Y = 0;
                    velocity.X = -25 * velModifier;
                }
                prevWasPutting = wasPutting;
                return new Vector2(velocity.X, velocity.Y);
            }
            prevWasPutting = wasPutting;
            return velocity;





        }
        public Vector2 HandleCollisions(GameTime gameTime, Rectangle player, TiledLayer collisionLayer)
        {

            foreach (var obj in collisionLayer.objects)
            {
                var objRect = new Rectangle((int)obj.x, (int)obj.y, (int)obj.width, (int)obj.height);
                bool intersectTop = player.Bottom - objRect.Top < 30 && player.X >= objRect.Left - 22 && player.X <= objRect.Right - 16;
                bool intersectBottom = objRect.Bottom - player.Top < 30 && player.X >= objRect.Left && player.X <= objRect.Right;
                bool intersectLeft = player.Right - objRect.Left < 30 && player.Y <= objRect.Bottom && player.Y >= objRect.Top;
                bool intersectRight = objRect.Right - player.Left < 30 && player.Y <= objRect.Bottom && player.Y >= objRect.Top;
                
                if (intersectTop)
                {

                    player.Y = MathHelper.Clamp(player.Y, 0, objRect.Top - 32);

                }
                if (intersectBottom) 
                {
                    player.Y = MathHelper.Clamp(player.Y, objRect.Bottom, 1000000);

                } 
                
                if (intersectLeft) 
                {
                    player.X = MathHelper.Clamp(player.X, 0, objRect.Left - 32);
                }
                if(intersectRight)
                {
                    player.X = MathHelper.Clamp(player.X, objRect.Right, 10000);
                }
            }
            return new Vector2(player.X, player.Y);
        }

        private float getXVelocity()
        {
            return velocity.X;
        }

    }

    }
