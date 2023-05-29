using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TiledCS;

namespace GolfYou
{
    public class Level //NOTE THAT IF YOU MAKE A LEVEL, THE LEVEL NEEDS A COLLISION LAYER, AND A LAYER TITLED "StartEnd", Collision layer handles collision, and startEnd
                       //has two objects tied to it, the BeginSquare and the EndSquare, the former is where the player spawns in a level, the latter is where the player goes to end the level,
                       //If the level does not have ALL of these, it will not function. Make sure to load any new tilesets too.
    {

        private TiledMap map;
        private Dictionary<int, TiledTileset> tilesets;
        private Texture2D tilesetTexture;
        private Texture2D tilesetTexture2;
        private Texture2D flagTexture;
        private TiledLayer collisionLayer;
        private TiledLayer endLayer;
        private TiledLayer enemyLayer;

        [Flags]
        enum Trans
        {
            None = 0,
            Flip_H = 1 << 0,
            Flip_V = 1 << 1,
            Flip_D = 1 << 2,

            Rotate_90 = Flip_D | Flip_H,
            Rotate_180 = Flip_H | Flip_V,
            Rotate_270 = Flip_V | Flip_D,

            Rotate_90AndFlip_H = Flip_H | Flip_V | Flip_D,
        }

        public void loadLevel(ContentManager Content, string levelName)
        {
            map = new TiledMap(Content.RootDirectory + "/Levels/" + levelName);
            tilesets = map.GetTiledTilesets(Content.RootDirectory + "/Levels/");

            tilesetTexture = Content.Load<Texture2D>("Levels/LevelMaterials/tileset");
            tilesetTexture2 = Content.Load<Texture2D>("Levels/LevelMaterials/back");
            flagTexture = Content.Load<Texture2D>("Levels/LevelMaterials/GolfFLag");

            collisionLayer = map.Layers.First(l => l.name == "Collision");
            endLayer = map.Layers.First(l => l.name == "StartEnd");
            enemyLayer = map.Layers.First(l => l.name == "Enemies");
        }
        
        public void drawLevel(SpriteBatch _spriteBatch)
        {
            var tileLayers = map.Layers.Where(x => x.type == TiledLayerType.TileLayer);
            var flagLayer = map.Layers.Where(x => x.type == TiledLayerType.ImageLayer).First();

            foreach (var layer in tileLayers)
            {
                for (var y = 0; y < layer.height; y++)
                {
                    for (var x = 0; x < layer.width; x++)
                    {
                        var index = (y * layer.width) + x; // Assuming the default render order is used which is from right to bottom
                        var gid = layer.data[index]; // The tileset tile index
                        var tileX = x * map.TileWidth;
                        var tileY = y * map.TileHeight;

                        // Gid 0 is used to tell there is no tile set
                        if (gid == 0)
                        {
                            continue;
                        }

                        // Helper method to fetch the right TieldMapTileset instance
                        // This is a connection object Tiled uses for linking the correct tileset to the gid value using the firstgid property
                        var mapTileset = map.GetTiledMapTileset(gid);

                        // Retrieve the actual tileset based on the firstgid property of the connection object we retrieved just now
                        var tileset = tilesets[mapTileset.firstgid];

                        // Use the connection object as well as the tileset to figure out the source rectangle
                        var rect = map.GetSourceRect(mapTileset, tileset, gid);

                        // Create destination and source rectangles
                        var source = new Rectangle(rect.x, rect.y, rect.width, rect.height);
                        var destination = new Rectangle(tileX, tileY, map.TileWidth, map.TileHeight);


                        // You can use the helper methods to get information to handle flips and rotations
                        Trans tileTrans = Trans.None;
                        if (map.IsTileFlippedHorizontal(layer, x, y)) tileTrans |= Trans.Flip_H;
                        if (map.IsTileFlippedVertical(layer, x, y)) tileTrans |= Trans.Flip_V;
                        if (map.IsTileFlippedDiagonal(layer, x, y)) tileTrans |= Trans.Flip_D;

                        SpriteEffects effects = SpriteEffects.None;
                        double rotation = 0f;
                        switch (tileTrans)
                        {
                            case Trans.Flip_H: effects = SpriteEffects.FlipHorizontally; break;
                            case Trans.Flip_V: effects = SpriteEffects.FlipVertically; break;

                            case Trans.Rotate_90:
                                rotation = Math.PI * .5f;
                                destination.X += map.TileWidth;
                                break;

                            case Trans.Rotate_180:
                                rotation = Math.PI;
                                destination.X += map.TileWidth;
                                destination.Y += map.TileHeight;
                                break;

                            case Trans.Rotate_270:
                                rotation = Math.PI * 3 / 2;
                                destination.Y += map.TileHeight;
                                break;

                            case Trans.Rotate_90AndFlip_H:
                                effects = SpriteEffects.FlipHorizontally;
                                rotation = Math.PI * .5f;
                                destination.X += map.TileWidth;
                                break;

                            default:
                                break;
                        }


                        // Render sprite at position tileX, tileY using the rect
                        _spriteBatch.Draw(flagTexture, new Vector2(flagLayer.offsetX, flagLayer.offsetY), Color.White);
                        if (layer.name == "Terrain") { _spriteBatch.Draw(tilesetTexture, destination, source, Color.White, (float)rotation, Vector2.Zero, effects, 0); }
                        else if (layer.name == "Background") { _spriteBatch.Draw(tilesetTexture2, destination, source, Color.White, (float)rotation, Vector2.Zero, effects, 0); }
                        


                    }
                }
            }
        }

        public TiledLayer getCollisionLayer()
        {
            return collisionLayer;
        }

        public Vector2 getPlayerSpawnLocation()
        {
            var startobj = endLayer.objects.First(l => l.name == "BeginSquare");
            return new Vector2(startobj.x, startobj.y);
        }

        public void endCurLevel(Rectangle player) //This is the logic to end the current level and load the next one, if the player's hitbox touches the EndSquare hitbox, then the current level ends, and the next is loaded
        {
            var endObj = endLayer.objects.First(l => l.name == "EndSquare");
            var objRect = new Rectangle((int)endObj.x, (int)endObj.y, (int)endObj.width, (int)endObj.height);
            if (player.Intersects(objRect))
            {
                Game1.levelEnd = true;
            }
        }

        public Vector2 getMapBounds()
        {
            return new Vector2(map.Width * 32, map.Height * 32);
        }

        public TiledLayer getEnemyLayer()
        {
            return enemyLayer;
        }

        public bool isPlayerOOB(Vector2 playerPosition)
        {
            if (playerPosition.X < 0 || playerPosition.X > map.Width*32 || playerPosition.Y < 0 || playerPosition.Y > map.Height * 32)
            {
                return true;
            }
            return false;
        }

    }

}

