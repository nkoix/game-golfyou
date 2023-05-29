using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GolfYou
{
	public class Menu
	{
        private Texture2D menuBackground;
        private SpriteFont font;
        private Microsoft.Xna.Framework.Rectangle startMenuHitbox = new Microsoft.Xna.Framework.Rectangle(350, 150, 100, 50);
        private Microsoft.Xna.Framework.Rectangle controlMenuHitbox = new Microsoft.Xna.Framework.Rectangle(350, 200, 100, 50);
        private Microsoft.Xna.Framework.Rectangle controlsExitToStartMenuHitbox = new Microsoft.Xna.Framework.Rectangle(350, 250, 100, 50);
        private Microsoft.Xna.Framework.Rectangle exitToStartMenuHitbox = new Microsoft.Xna.Framework.Rectangle(350, 250, 100, 50);
        private Microsoft.Xna.Framework.Rectangle exitToStartMenuHitboxDeath = new Microsoft.Xna.Framework.Rectangle(350, 350, 100, 50);
        private Microsoft.Xna.Framework.Rectangle exitToStartMenuHitboxComplete = new Microsoft.Xna.Framework.Rectangle(350, 250, 100, 50);
        private Microsoft.Xna.Framework.Rectangle restartMenuHitbox = new Microsoft.Xna.Framework.Rectangle(350, 300, 100, 50);

        public void loadMenus(ContentManager Content)
        {
            font = Content.Load<SpriteFont>("MenuText");
            menuBackground = Content.Load<Texture2D>("badend");
        }

        public bool didPressStart(MouseState mouseState)
        {
            if(startMenuHitbox.Contains(mouseState.X, mouseState.Y))
            {
                return true;
            }
            return false;
        }

        public bool didPressControls(MouseState mouseState)
        {
            if (controlMenuHitbox.Contains(mouseState.X, mouseState.Y))
            {
                return true;
            }
            return false;
        }

        public bool controlDidPressExitToStart(MouseState mouseState)
        {
            if (controlsExitToStartMenuHitbox.Contains(mouseState.X, mouseState.Y))
            {
                return true;
            }
            return false;
        }

        public bool didPressExitToStart(MouseState mouseState)
        {
            if (exitToStartMenuHitbox.Contains(mouseState.X, mouseState.Y))
            {
                return true;
            }
            return false;
        }

        public bool didPressExitToStartDeath(MouseState mouseState)
        {
            if (exitToStartMenuHitboxDeath.Contains(mouseState.X, mouseState.Y))
            {
                return true;
            }
            return false;
        }

        public bool didPressRestart(MouseState mouseState)
        {
            if (restartMenuHitbox.Contains(mouseState.X, mouseState.Y))
            {
                return true;
            }
            return false;
        }

        public bool didPressExitToStartComplete(MouseState mouseState)
        {
            if (exitToStartMenuHitboxComplete.Contains(mouseState.X, mouseState.Y))
            {
                return true;
            }
            return false;
        }

        public void drawStartMenu(SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(menuBackground, new Vector2(0, 0), new Microsoft.Xna.Framework.Rectangle(0, 870, 800, 480), Microsoft.Xna.Framework.Color.White);
            _spriteBatch.DrawString(font, "Golf You!", new Vector2(350, 100), Microsoft.Xna.Framework.Color.White);
            _spriteBatch.DrawString(font, "Start", new Vector2(350, 150), Microsoft.Xna.Framework.Color.Cyan);
            _spriteBatch.DrawString(font, "Controls", new Vector2(350, 200), Microsoft.Xna.Framework.Color.Cyan);
        }

        public void drawControlMenu(SpriteBatch _spriteBatch)
        {
            adjustMenuBox(new Vector2(340, 275));
            _spriteBatch.Draw(menuBackground, new Vector2(0, 0), new Microsoft.Xna.Framework.Rectangle(0, 870, 800, 480), Microsoft.Xna.Framework.Color.White);
            _spriteBatch.DrawString(font, "Controls", new Vector2(360, 100), Microsoft.Xna.Framework.Color.White);
            _spriteBatch.DrawString(font, "A: Move Left", new Vector2(350, 150), Microsoft.Xna.Framework.Color.White);
            _spriteBatch.DrawString(font, "D: Move Right", new Vector2(350, 175), Microsoft.Xna.Framework.Color.White);
            _spriteBatch.DrawString(font, "Space to enter putting mode, Space again to choose angle, Space again to choose velocity", new Vector2(100, 200), Microsoft.Xna.Framework.Color.White);
            _spriteBatch.DrawString(font, "C to cancel out of putting mode, Q to switch between putter/driver", new Vector2(175, 225), Microsoft.Xna.Framework.Color.White);
            _spriteBatch.DrawString(font, "Defeat enemies by putting into them (make sure you're going fast too!)", new Vector2(125, 250), Microsoft.Xna.Framework.Color.White);
            _spriteBatch.DrawString(font, "Exit to Main Menu", new Vector2(340, 275), Microsoft.Xna.Framework.Color.Cyan);
        }

        public void drawLevelEndMenu(SpriteBatch _spriteBatch)
        {
            adjustMenuBox(new Vector2(350, 250));
            _spriteBatch.Draw(menuBackground, new Vector2(0, 0), new Microsoft.Xna.Framework.Rectangle(0, 870, 800, 480), Microsoft.Xna.Framework.Color.White);
            _spriteBatch.DrawString(font, "Level Over", new Vector2(350, 100), Microsoft.Xna.Framework.Color.White);
            _spriteBatch.DrawString(font, "Next Level", new Vector2(350, 150), Microsoft.Xna.Framework.Color.Cyan);
            _spriteBatch.DrawString(font, "Main Menu", new Vector2(350, 250), Microsoft.Xna.Framework.Color.Cyan);
        }



        public void drawDeathMenu(SpriteBatch _spriteBatch)
        {
            adjustMenuBox(new Vector2(350, 350));
            adjustRestartBox(new Vector2(350, 300));
            _spriteBatch.Draw(menuBackground, new Vector2(0, 0), new Microsoft.Xna.Framework.Rectangle(0, 870, 800, 480), Microsoft.Xna.Framework.Color.White);
            _spriteBatch.DrawString(font, "You Died :(", new Vector2(348, 150), Microsoft.Xna.Framework.Color.White);
            _spriteBatch.DrawString(font, "(Tip: Make sure you are in putt mode to defeat enemies, and are moving fast!)", new Vector2(125, 200), Microsoft.Xna.Framework.Color.White);
            _spriteBatch.DrawString(font, "Restart Level", new Vector2(350, 300), Microsoft.Xna.Framework.Color.Cyan);
            _spriteBatch.DrawString(font, "Main Menu", new Vector2(350, 350), Microsoft.Xna.Framework.Color.Cyan);
        }
        public void drawPauseMenu(SpriteBatch _spriteBatch)
        {
            adjustRestartBox(new Vector2(350, 200));
            adjustMenuBox(new Vector2(350, 250));
            _spriteBatch.Draw(menuBackground, new Vector2(0, 0), new Microsoft.Xna.Framework.Rectangle(0, 870, 800, 480), Microsoft.Xna.Framework.Color.White);
            _spriteBatch.DrawString(font, "Game Paused (Press 'E' to Unpause)", new Vector2(240, 150), Microsoft.Xna.Framework.Color.White);
            _spriteBatch.DrawString(font, "Restart Level", new Vector2(350, 200), Microsoft.Xna.Framework.Color.Cyan);
            _spriteBatch.DrawString(font, "Main Menu", new Vector2(350, 250), Microsoft.Xna.Framework.Color.Cyan);
            _spriteBatch.DrawString(font, "W: Move Left | D: Move Right | Q: Switch to Putter/Driver | Space: Swing Club | C: Cancel Swing", new Vector2(50, 300), Microsoft.Xna.Framework.Color.White);
        }

        public void drawCompleteMenu(SpriteBatch _spriteBatch)
        {
            adjustMenuBox(new Vector2(350, 250));
            _spriteBatch.Draw(menuBackground, new Vector2(0, 0), new Microsoft.Xna.Framework.Rectangle(0, 870, 800, 480), Microsoft.Xna.Framework.Color.White);
            _spriteBatch.DrawString(font, "GAME COMPLETE, YOU WON!", new Vector2(275, 150), Microsoft.Xna.Framework.Color.White);
            _spriteBatch.DrawString(font, "Main Menu", new Vector2(350, 250), Microsoft.Xna.Framework.Color.Cyan);
        }

        private void adjustMenuBox(Vector2 vec)
        {
            exitToStartMenuHitbox = new Rectangle((int)vec.X, (int)vec.Y, 100, 50);
        }
        private void adjustRestartBox(Vector2 vec)
        {
            restartMenuHitbox = new Rectangle((int)vec.X, (int)vec.Y, 100, 50);
        }
    }
}

