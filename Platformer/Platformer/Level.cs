#region File Description
//-----------------------------------------------------------------------------
// Level.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace OktoberQuest
{
    /// <summary>
    /// A uniform grid of tiles with collections of gems and enemies.
    /// The level owns the player and controls the game's win and lose
    /// conditions as well as scoring.
    /// </summary>
    class Level : IDisposable
    {
        // Physical structure of the level.
        private Tile[,] tiles;
        private Layer[] layers;
        // The layer which entities are drawn on top of.
        private const int EntityLayer = 2;

        // Entities in the level.
        public Player Player
        {
            get { return player; }
        }
        Player player;

        private List<Gem> gems = new List<Gem>();
        public List<Enemy> enemies = new List<Enemy>();

        // Key locations in the level.        
        private Vector2 start;
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);

        // Level game state.
        private Random random = new Random(354668); // Arbitrary, but constant seed
        private float cameraPositionXAxis;
        private float cameraPositionYAxis;

        public int Score
        {
            get { return score; }
        }
        int score;

        public bool ReachedExit
        {
            get { return reachedExit; }
        }
        bool reachedExit;

        private const int PointsPerSecond = 5;

        // Level content.        
        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;

        private SoundEffect exitReachedSound;

        #region Loading

        #region Constructor
        /// <summary>
        /// Constructs a new level.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider that will be used to construct a ContentManager.
        /// </param>
        /// <param name="fileStream">
        /// A stream containing the tile data.
        /// </param>
        public Level(IServiceProvider serviceProvider, Stream fileStream, int levelIndex)
        {
            // Create a new content manager to load content used just by this level.
            content = new ContentManager(serviceProvider, "Content");

            LoadTiles(fileStream);

            //Each of these layers has a different scrolling speed
            //0 => no scrolling to 1
            layers = new Layer[3];
            layers[0] = new Layer(Content, "Backgrounds/Level" + levelIndex.ToString() + "/Layer0", 0.2f);
            layers[1] = new Layer(Content, "Backgrounds/Level" + levelIndex.ToString() + "/Layer1", 0.5f);
            layers[2] = new Layer(Content, "Backgrounds/Level" + levelIndex.ToString() + "/Layer2", 0.7f);
            

            // Load sounds.
            exitReachedSound = Content.Load<SoundEffect>("Sounds/ExitReached");
                        
        }
        #endregion

        #region LoadTiles
        /// <summary>
        /// Iterates over every tile in the structure file and loads its
        /// appearance and behavior. This method also validates that the
        /// file is well-formed with a player start point, exit, etc.
        /// </summary>
        /// <param name="fileStream">
        /// A stream containing the tile data.
        /// </param>
        private void LoadTiles(Stream fileStream)
        {
            // Load the level and ensure all of the lines are the same length.
            int width;
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    lines.Add(line);
                    if (line.Length != width)
                        throw new Exception(String.Format("The length of line {0} is different from all preceeding lines.", lines.Count));
                    line = reader.ReadLine();
                }
            }

            // Allocate the tile grid.
            tiles = new Tile[width, lines.Count];

            // Loop over every tile position,
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // to load each tile.
                    char tileType = lines[y][x];
                    tiles[x, y] = LoadTile(tileType, x, y);
                }
            }

            // Verify that the level has a beginning and an end.
            if (Player == null)
                throw new NotSupportedException("A level must have a starting point.");
            if (exit == InvalidPosition)
                throw new NotSupportedException("A level must have an exit.");

        }
        #endregion

        #region LoadTile
        /// <summary>
        /// Loads an individual tile's appearance and behavior.
        /// </summary>
        /// <param name="tileType">
        /// The character loaded from the structure file which
        /// indicates what should be loaded.
        /// </param>
        /// <param name="x">
        /// The X location of this tile in tile space.
        /// </param>
        /// <param name="y">
        /// The Y location of this tile in tile space.
        /// </param>
        /// <returns>The loaded tile.</returns>
        private Tile LoadTile(char tileType, int x, int y)
        {
            switch (tileType)
            {
                // Blank space
                case '.':
                    return new Tile(null, TileCollision.Passable);

                // Exit
                case 'X':
                    return LoadExitTile(x, y);

                // Gem
                case 'B':
                    return LoadGemTile(x, y);

                // Floating platform
                case '-':
                    return LoadTile("Platform", TileCollision.Platform);

                // Various enemies: Monster1 is Gnome, Monster2 is Wolf, Monster3 is Goat, 
                //Monster4 is Satyr, Monster5 is Bat, Monster6 is Ghost, and Monster7 is Skeleton.
                case '1':
                    return LoadEnemyTile(x, y, "Monster1", Enemy.enemyAI.pacer);
                case '2':
                    return LoadEnemyTile(x, y, "Monster2", Enemy.enemyAI.patroller);
                case '3':
                    return LoadEnemyTile(x, y, "Monster3", Enemy.enemyAI.patroller);
                case '4':
                    return LoadEnemyTile(x, y, "Monster4", Enemy.enemyAI.pacer);
                case '5':
                    return LoadEnemyTile(x, y, "Monster5", Enemy.enemyAI.patroller);
                case '6':
                    return LoadEnemyTile(x, y, "Monster6", Enemy.enemyAI.flyer);
                case '7':
                    return LoadEnemyTile(x, y, "Monster7", Enemy.enemyAI.pacer);

                // Platform block
                case '~':
                    return LoadVarietyTile("BlockB", 2, TileCollision.Platform);

                // Passable block
                case ':':
                    return LoadVarietyTile("BlockB", 2, TileCollision.Passable);
                case 'C':
                    return LoadVarietyTile("ChainA", 0, TileCollision.Passable);

                // Player 1 start point
                case 'S':
                    return LoadStartTile(x, y);

                // Impassable block
                case '#':
                    return LoadVarietyTile("BlockA", 7, TileCollision.Impassable);
                case 'd':
                    return LoadVarietyTile("DirtA", 0, TileCollision.Impassable);
                case 'g':
                    return LoadVarietyTile("GrassA", 0, TileCollision.Impassable);
                case 'L':
                    return LoadVarietyTile("LightA", 0, TileCollision.Passable);
                case 'K':
                    return LoadVarietyTile("BarrelA", 0, TileCollision.Impassable);

                //Invisible block for flying enemies to fly
                case 'i':
                    return LoadVarietyTile("InvisibleA", 0, TileCollision.Platform);
                case 't':
                    return LoadVarietyTile("TableA", 0, TileCollision.Platform);
                case 'I':
                    return LoadVarietyTile("IronA", 0, TileCollision.Platform);

                // Unknown tile type character
                default:
                    throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }
        #endregion

        #region LoadTile (used by other load tile methods)
        /// <summary>
        /// Creates a new tile. The other tile loading methods typically chain to this
        /// method after performing their special logic.
        /// </summary>
        /// <param name="name">
        /// Path to a tile texture relative to the Content/Tiles directory.
        /// </param>
        /// <param name="collision">
        /// The tile collision type for the new tile.
        /// </param>
        /// <returns>The new tile.</returns>
        private Tile LoadTile(string name, TileCollision collision)
        {
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision);
        }
        #endregion

        #region LoadVarietyTile
        /// <summary>
        /// Loads a tile with a random appearance.
        /// </summary>
        /// <param name="baseName">
        /// The content name prefix for this group of tile variations. Tile groups are
        /// name LikeThis0.png and LikeThis1.png and LikeThis2.png.
        /// </param>
        /// <param name="variationCount">
        /// The number of variations in this group.
        /// </param>
        private Tile LoadVarietyTile(string baseName, int variationCount, TileCollision collision)
        {
            int index = random.Next(variationCount);
            return LoadTile(baseName + index, collision);
        }
        #endregion

        #region LoadStartTile
        /// <summary>
        /// Instantiates a player, puts him in the level, and remembers where to put him when he is resurrected.
        /// </summary>
        private Tile LoadStartTile(int x, int y)
        {
            if (Player != null)
                throw new NotSupportedException("A level may only have one starting point.");

            start = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            player = new Player(this, start);

            return new Tile(null, TileCollision.Passable);
        }
        #endregion

        #region LoadExitTile
        /// <summary>
        /// Remembers the location of the level's exit.
        /// </summary>
        private Tile LoadExitTile(int x, int y)
        {
            if (exit != InvalidPosition)
                throw new NotSupportedException("A level may only have one exit.");

            exit = GetBounds(x, y).Center;

            return LoadTile("Exit", TileCollision.Passable);
        }
        #endregion

        #region LoadEnemyTile
        /// <summary>
        /// Instantiates an enemy and puts him in the level.
        /// </summary>
        private Tile LoadEnemyTile(int x, int y, string spriteSet, Enemy.enemyAI aiType)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            enemies.Add(new Enemy(this, position, spriteSet, aiType));

            return new Tile(null, TileCollision.Passable);
        }
        #endregion

        #region LoadGemTile
        /// <summary>
        /// Instantiates a gem and puts it in the level.
        /// </summary>
        private Tile LoadGemTile(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            gems.Add(new Gem(this, new Vector2(position.X, position.Y)));

            return new Tile(null, TileCollision.Passable);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Unloads the level content.
        /// </summary>
        public void Dispose()
        {
            Content.Unload();
        }
        #endregion

        #endregion

        #region Bounds and Collision

        /// <summary>
        /// Gets the collision mode of the tile at a particular location.
        /// This method handles tiles outside of the levels boundries by making it
        /// impossible to escape past the left or right edges, but allowing things
        /// to jump beyond the top of the level and fall off the bottom.
        /// </summary>
        public TileCollision GetCollision(int x, int y)
        {
            // Prevent escaping past the level ends.
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= Height)
                return TileCollision.Passable;

            return tiles[x, y].Collision;
        }

        /// <summary>
        /// Gets the bounding rectangle of a tile in world space.
        /// </summary>        
        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        /// <summary>
        /// Width of level measured in tiles.
        /// </summary>
        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        /// <summary>
        /// Height of the level measured in tiles.
        /// </summary>
        public int Height
        {
            get { return tiles.GetLength(1); }
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates all objects in the world, performs collision between them,
        /// and handles the scoring.
        /// </summary>
        public void Update(
            GameTime gameTime, 
            KeyboardState keyboardState, 
            GamePadState gamePadState, 
            AccelerometerState accelState,
            DisplayOrientation orientation)
        {
            // Pause while the player is dead.
            if (!Player.IsAlive)
            {
                // Still want to perform physics on the player.
                Player.ApplyPhysics(gameTime);
            }
            // Pause when exit is reached.
            else if (ReachedExit)
            {
                // Area to add code for any effects to execute on level exit reached.
            }
            else
            {
                Player.Update(gameTime, keyboardState, gamePadState, accelState, orientation);
                UpdateGems(gameTime);
                
                //if the player is using a tool
                if (Player.stillToolUse)
                {
                    string currentTool = Player.tools.getCurrentTool();
                    if (currentTool == "grap")
                    {
                        //do grapping hook logic here
                    }
                    else if (currentTool == "boom")
                    {
                        //do boom logic here
                    }
                }
                // Falling off the bottom of the level kills the player.
                if (Player.BoundingRectangle.Top >= Height * Tile.Height)
                    OnPlayerKilled(null);

                UpdateEnemies(gameTime);

                // The player has reached the exit if they are standing on the ground and
                // his bounding rectangle contains the center of the exit tile. They can only
                // exit when they have collected all of the gems.
                if (Player.IsAlive &&
                    Player.IsOnGround &&
                    Player.BoundingRectangle.Contains(exit))
                {
                    OnExitReached();
                }
            }
        }

        /// <summary>
        /// Animates each gem and checks to allows the player to collect them.
        /// </summary>
        private void UpdateGems(GameTime gameTime)
        {
            for (int i = 0; i < gems.Count; ++i)
            {
                Gem gem = gems[i];

                gem.Update(gameTime);

                if (gem.BoundingCircle.Intersects(Player.BoundingRectangle))
                {
                    gems.RemoveAt(i--);
                    OnGemCollected(gem, Player);
                }
            }
        }

        /// <summary>
        /// Animates each enemy and allow them to kill the player.
        /// </summary>
        private void UpdateEnemies(GameTime gameTime)
        {
            foreach (Enemy enemy in enemies)
            {
                enemy.Update(gameTime);

                // Touching an enemy instantly kills the player
                if (enemy.BoundingRectangle.Intersects(Player.BoundingRectangle))
                {
                    if (enemy.IsAlive)
                    {
                        OnPlayerKilled(enemy);
                    }
                }

                if (enemy.IsAlive && enemy.BoundingRectangle.Intersects(Player.MeleeRectangle))
                {
                    if (Player.stillAttacking)
                        OnEnemyKilled(enemy, Player);
                }
            }
        }

        private void OnEnemyKilled(Enemy enemy, Player killedBy)
        {
            enemy.OnKilled(killedBy);
        }

        /// <summary>
        /// Called when a gem is collected.
        /// </summary>
        /// <param name="gem">The gem that was collected.</param>
        /// <param name="collectedBy">The player who collected this gem.</param>
        private void OnGemCollected(Gem gem, Player collectedBy)
        {
            score += Gem.PointValue;

            gem.OnCollected(collectedBy);
        }

        /// <summary>
        /// Called when the player is killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This is null if the player was not killed by an
        /// enemy, such as when a player falls into a hole.
        /// </param>
        private void OnPlayerKilled(Enemy killedBy)
        {
            Player.OnKilled(killedBy);
        }

        /// <summary>
        /// Called when the player reaches the level's exit.
        /// </summary>
        private void OnExitReached()
        {
            Player.OnReachedExit();
            exitReachedSound.Play();
            reachedExit = true;
        }

        /// <summary>
        /// Restores the player to the starting point to try the level again.
        /// </summary>
        public void StartNewLife()
        {
            Player.Reset(start);
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draw everything in the level from background to foreground.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            ///Draw Background
            spriteBatch.Begin();
            int bottomOfStage = (Height * Tile.Height);
            bottomOfStage = Math.Min(bottomOfStage, Height);

            bottomOfStage -= Tile.Height; //Offset by one more because of a slight margin left
            for (int i = 0; i <= EntityLayer; ++i)
            {
                layers[i].Draw(spriteBatch, cameraPositionXAxis, cameraPositionYAxis, bottomOfStage);
            }
            spriteBatch.End();

            ScrollCamera(spriteBatch.GraphicsDevice.Viewport);

            Matrix cameraTransform = Matrix.CreateTranslation(-cameraPositionXAxis, -cameraPositionYAxis, 0.0f);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, cameraTransform);
            
            DrawTiles(spriteBatch);

            foreach (Gem gem in gems)
                gem.Draw(gameTime, spriteBatch);

            Player.Draw(gameTime, spriteBatch);

            foreach (Enemy enemy in enemies)
                if (enemy.IsAlive || enemy.deathTime > 0)
                    enemy.Draw(gameTime, spriteBatch);

            spriteBatch.End();

            // Figured out this is for foreground layers. We don't need it yet.
            //spriteBatch.Begin();
            //for (int i = EntityLayer + 1; i < layers.Length; ++i)
            //{
            //    layers[i].Draw(spriteBatch, cameraPositionXAxis, cameraPositionYAxis);
            //}
            //spriteBatch.End();
        }

        #region DrawTiles
        /// <summary>
        /// Draws each tile in the level.
        /// </summary>
        private void DrawTiles(SpriteBatch spriteBatch)
        {
            
            //To avoid drawing/calculating titles offscreen
            //NOTE: Gems / Enemies are still drawn.
            int left = (int)(Math.Floor(cameraPositionXAxis / Tile.Width));
            int right = left + spriteBatch.GraphicsDevice.Viewport.Width / Tile.Width + 2;
            right = Math.Min(right, Width);
            
            int top = (int)(Math.Floor(cameraPositionYAxis / Tile.Height));
            int bottom = top + spriteBatch.GraphicsDevice.Viewport.Height / Tile.Height;
            bottom = Math.Min(bottom, Height); 
            
            // For each tile position
            for (int y = 0; y < Height; ++y)
            {
                for (int x = left; x < right; ++x)//Draw only inbetween those points.
                {
                    // If there is a visible tile in that position
                    Texture2D texture = tiles[x, y].Texture;
                    if (texture != null)
                    {
                        // Draw it in screen space.
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.White);
                    }
                }
            }
        }
        #endregion

        #endregion

        #region Camera

        private void ScrollCamera(Viewport viewport)
        {
            const float ViewMargin = .35f;
            const float TopMargin = 0.3f;
            const float BottomMargin = 0.3f;

            //Edges of the Screen
            float marginWidth = viewport.Width * ViewMargin;
            float marginLeft = cameraPositionXAxis + marginWidth;
            float marginRight = cameraPositionXAxis + viewport.Width - marginWidth;
            float marginTop = cameraPositionYAxis + viewport.Height * TopMargin;
            float marginBottom = cameraPositionYAxis + viewport.Height - viewport.Height * BottomMargin;
            
            //How far to scroll when player nears edges
            float cameraMovement = 0.0f;
            if (Player.Position.X < marginLeft)
            {
                cameraMovement = Player.Position.X - marginLeft;
            }
            else if (Player.Position.X > marginRight)
            {
                cameraMovement = Player.Position.X - marginRight;
            }
            
            // Calculate how far to vertically scroll when the player is near the top or bottom of the screen.  
            float cameraMovementY = 0.0f;

            if (Player.Position.Y < marginTop)
            {
                cameraMovementY = Player.Position.Y - marginTop;
            }
            else if (Player.Position.Y > marginBottom)
            {
                cameraMovementY = Player.Position.Y - marginBottom;
            }

            

            //update cam Position except when @ the ends of the levels
            float maxCameraPosition = Tile.Width * Width - viewport.Width;
            float maxCameraPositionYOffset = Tile.Height * Height - viewport.Height;


            //Clamp restricts a value to be within a specific range
            //clamp( value, min, max) is the syntax
            cameraPositionXAxis = MathHelper.Clamp(cameraPositionXAxis + cameraMovement, 0.0f, maxCameraPosition);
            cameraPositionYAxis = MathHelper.Clamp(cameraPositionYAxis + cameraMovementY, 0.0f, maxCameraPositionYOffset);
        }

        #endregion

        internal void Update(GameTime gameTime, KeyboardState keyboardState, GamePadState gamePadState, AccelerometerState accelerometerState)
        {
            throw new NotImplementedException();
        }
    }
}
