using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using TiledCS;
using System;
using System.Diagnostics;

namespace GolfYou
{
	public class Game1 : Game //Main game loop, really its just a container to call functions from other classes.
	{
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;
		//Class instances are declared here
		private Player myPlayer = new Player();
		private PlayerPhysics myPhysics;
		private HUD myHUD = new HUD();
		private Level levelManager = new Level();
		private Camera myCamera = new Camera();
		private Menu myMenu = new Menu();


        private string[] levels = { "LevelOne.tmx", "LevelTwo.tmx", "LevelThree.tmx", "LevelFour.tmx", "LevelFive.tmx", "LevelSix.tmx", "LevelSeven.tmx", "LevelEight.tmx", "LevelNine.tmx", "LevelTen.tmx" };

        int levelCounter = 0;

		private Texture2D startMenuSprites;
        public static int ScreenHeight;
		public static int ScreenWidth;
		public static bool levelEnd;
		public static bool loadMainMenu;
		public static bool startMenu = true;
		public static bool startButtonPressed;
		public static bool loadControlMenu;
		public static bool controlButtonPressed;
		public static bool deathMenu;
		public List<Enemy> enemies;
		public static bool paused;

		private SpriteFont hudFont;

		public Game1()
		{
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
		}

		protected override void Initialize()
		{
			ScreenHeight = _graphics.PreferredBackBufferHeight;
			ScreenWidth = _graphics.PreferredBackBufferWidth;
			base.Initialize();
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			myMenu.loadMenus(this.Content);
			myPlayer.loadPlayerContent(this.Content);
			myHUD.loadHudContent(this.Content);
			startButtonPressed = false;
			loadMainMenu = false;
			controlButtonPressed = false;
			levelEnd = false;
			deathMenu = false;
			paused = false;
			enemies = new List<Enemy>();
			hudFont = Content.Load<SpriteFont>("File");

        }

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

            // TODO: Add your update logic here
			
            MouseState mouseState = Mouse.GetState();
			if(startMenu)
			{
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    // Check for intersection
                    if (myMenu.didPressStart(mouseState))
                    {
                        startButtonPressed = true;
                    }
                    if (myMenu.didPressControls(mouseState))
                    {
                        toControls();
                    }
                }
            }
            

			if(controlButtonPressed)
			{
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    if (myMenu.controlDidPressExitToStart(mouseState))
                    {
						toStart();
                    }
                }
            }

			if (paused)
			{
                myPlayer.handlePlayerInput(Keyboard.GetState(), GamePad.GetState(PlayerIndex.One), gameTime);
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
					if (myMenu.didPressExitToStart(mouseState))
						{
							toMenu();
						}
					if (myMenu.didPressRestart(mouseState))
						{
							restartLevel();
						}
				}
                if (myPlayer.wasEPressed())
				{
					Debug.WriteLine("entered");
					unPause();
				}
			}

			// if start pressed, start loading level and the game
			if (startButtonPressed && !levelEnd)
			{
				if (startMenu) { LoadLevel(); }
				startMenu = false;

                myPlayer.playAnimation(gameTime);
                myPlayer.handlePlayerInput(Keyboard.GetState(), GamePad.GetState(PlayerIndex.One), gameTime);
				if (myPlayer.wasEPressed())
				{
					toPause();
				}
                myCamera.Follow(myPlayer.getPlayerHitbox(), levelManager.getMapBounds());
                myHUD.playHudAnimations(gameTime, myPlayer.getIsPutting(), myPlayer.rolling, myPlayer.getAnglePutting(), myPlayer.getFacing()); //HUD MUST be drawn before physics as the physics relies on calculations done in the HUD class,
                                                                                                                                                //weird I know, but it was an easy solution
                myPlayer.setPlayerPosition(myPhysics.ApplyPhysics(gameTime, Window.ClientBounds.Height, Window.ClientBounds.Width, ref myPlayer.rolling, myPlayer.getPlayerHitbox(),
                myPlayer.getMovement(), myPlayer.getWasPutting(), myPlayer.getFacing(), myPlayer.getHittingMode(), myHUD.getVelModifier(), myHUD.getAngle(), levelManager.getCollisionLayer()));
                levelManager.endCurLevel(myPlayer.getPlayerHitbox());
				updateEnemies();
                if (levelManager.isPlayerOOB(myPlayer.getPosition()))
                {
                    deathMenu = true;
                }
            }

			if (deathMenu)
			{
				if (mouseState.LeftButton == ButtonState.Pressed)
				{
					if (myMenu.didPressExitToStartDeath(mouseState))
					{
						deathMenu = false;
						controlButtonPressed = false;
						startButtonPressed= false;
						startMenu = true;
						levelCounter = 0;
					}
					if (myMenu.didPressRestart(mouseState))
					{
						restartLevel();
                    }
				}
			}

			

			// if level end, press exit to main menu, go back and load main

			if (levelEnd)
			{
				if (levelCounter == 10)
				{
					if (mouseState.LeftButton == ButtonState.Pressed)
					{
						if (myMenu.didPressExitToStart(mouseState))
						{
							controlButtonPressed = false;
							startButtonPressed = false;
							startMenu = true;
							levelCounter = 0;
						}
					}
				}
				else
				{
                    if (mouseState.LeftButton == ButtonState.Pressed)
                    {
                        if (myMenu.didPressExitToStart(mouseState))
                        {
                            toMenu();
                        }
                        else { LoadLevel(); }
                    }
                }
            }
                

            base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);
			
            if(startMenu)
			{
                _spriteBatch.Begin();
				myMenu.drawStartMenu(_spriteBatch);
            }
			else if (deathMenu)
			{
				_spriteBatch.Begin();
				myMenu.drawDeathMenu(_spriteBatch);
			}
            else if (startButtonPressed && !levelEnd)
            {
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: myCamera.Transform);
                levelManager.drawLevel(_spriteBatch);
                myPlayer.drawPlayer(_spriteBatch, gameTime, myPhysics.getVelocity());
                myHUD.drawHudContent(_spriteBatch, gameTime, myPlayer.getHittingMode(), myPlayer.getIsPutting(), myPlayer.getWasPutting(), myPlayer.getPosition(), myPlayer.getAnglePutting(), myPlayer.getFacing());
                drawEnemies();
                _spriteBatch.End();
				_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				myHUD.drawStaticHudContent(_spriteBatch, myPlayer.getHittingMode()); 	
				_spriteBatch.DrawString(hudFont, "Press 'E' to Pause/Show Controls", new Vector2(40, 20), Color.Purple);
            }
			else if (controlButtonPressed)
			{
				_spriteBatch.Begin();
				myMenu.drawControlMenu(_spriteBatch);
			}
            else if (levelCounter == 10)
            {
                _spriteBatch.Begin();
                myMenu.drawCompleteMenu(_spriteBatch);
            }
            else if(levelEnd)
			{
                _spriteBatch.Begin();
				myMenu.drawLevelEndMenu(_spriteBatch);
            }
			else if (paused)
			{
				_spriteBatch.Begin();
				myMenu.drawPauseMenu(_spriteBatch);
			}
			



            _spriteBatch.End();
			
			base.Draw(gameTime);
		}

		private void LoadLevel()
		{
			myPhysics = new PlayerPhysics();
            levelManager.loadLevel(this.Content, levels[levelCounter]);
            myPlayer.setSpawnLocation(levelManager.getPlayerSpawnLocation());
            levelEnd = false;
            levelCounter++;
			// Create enemy objects and store in array
			enemies = new List<Enemy>(); // Delete old array
			TiledLayer enemytiles = levelManager.getEnemyLayer();
			foreach (var obj in enemytiles.objects)
			{
				var objRect = new Rectangle((int)obj.x, (int)obj.y, (int)obj.width, (int)obj.height);
				enemies.Add(new Enemy(this.Content, obj.name=="Stationary", new Vector2(obj.x, obj.y)));
			}
        }

		private void updateEnemies()
		{
			TiledLayer collisionLayer = levelManager.getCollisionLayer();
			int i=0;
			List<int> rem = new List<int>(); // List of enemies who died in this frame
			foreach (Enemy enemy in enemies)
			{
				playerEnemyCollision(enemy);
				if (enemy.isDead()) rem.Add(i);
				enemy.updateEnemy(collisionLayer);
				i++;
			}
			if (rem.Count>0)
			{
				// List needs to be in descending order
				rem.Sort();
				rem.Reverse();

				foreach (int j in rem)
				{
					enemies.RemoveAt(j);
				}
			}
		}

		private void drawEnemies()
		{
			foreach (Enemy enemy in enemies)
			{
				enemy.drawEnemy(_spriteBatch);
			}
		}

		private void playerEnemyCollision(Enemy enemy)
		{
			if (myPlayer.getPlayerHitbox().Intersects(enemy.getHitBox()) && !enemy.isDying())
			{
				if (myPlayer.getHittingMode()==1 && Math.Abs(myPhysics.getVelocity().X)>150 && myPlayer.rolling)
				{
					enemy.setDeath(); // Kill enemy if player is putting and moving fast enough
				}
				else 
				{
					deathMenu = true; // Kill player if player is either not putting or not moving fast enough
				}

			}
		}

		private void toControls()
		{
			controlButtonPressed = true;
			startMenu = false;
			paused = false;
		}

		private void toMenu()
		{
			levelEnd = false;
			controlButtonPressed = false;
			startButtonPressed= false;
			startMenu = true;
			levelCounter = 0;
			paused = false;
			deathMenu = false;
		}
		public void toPause()
		{
			paused = true;
			controlButtonPressed = false;
			startButtonPressed = false;
			levelEnd = false;
			deathMenu = false;
		}
		public void unPause()
		{
			
			controlButtonPressed = false;
			startButtonPressed = true;
			levelEnd = false;
			deathMenu = false;
            paused = false;
        }
		private void restartLevel()
		{
			levelCounter--;
			LoadLevel();
			deathMenu = false;
			paused = false;
			startButtonPressed = true;
			controlButtonPressed = false;
		}

		private void toStart()
		{
			controlButtonPressed = false;
			startMenu = true;
		}
	}
}