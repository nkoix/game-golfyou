using Microsoft.Xna.Framework;

namespace GolfYou
{
    
    internal class Camera
    {
        public static int camPositionX;
        public static int camPositionY;
        public Matrix Transform { get; private set; }
        public void Follow(Rectangle target, Vector2 mapBounds)
        {
            camPositionX = -target.X - (target.Width / 2);
            camPositionY = -target.Y - (target.Height / 2);
            camPositionY = MathHelper.Clamp(camPositionY, (int)(-mapBounds.Y) + Game1.ScreenHeight / 2, -Game1.ScreenHeight / 2);
            camPositionX = MathHelper.Clamp(camPositionX, (int)(-mapBounds.X) + Game1.ScreenWidth / 2, -Game1.ScreenWidth / 2);
            var position = Matrix.CreateTranslation(
                camPositionX,
                camPositionY,
                0);

            var offset = Matrix.CreateTranslation(
                    Game1.ScreenWidth / 2,
                    Game1.ScreenHeight /2,
                    0);

            Transform = position * offset;


        }
    }
}
