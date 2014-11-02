#region File Description
//-----------------------------------------------------------------------------
// Enemy.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace OktoberQuest
{
    /// <summary>
    /// Facing direction along the X axis.
    /// </summary>
    enum FaceDirection
    {
        Left = -1,
        Right = 1,
    }



    /// <summary>
    /// A monster who is impeding the progress of our fearless adventurer.
    /// </summary>
    class Enemy
    {
        #region Properties and Fields
        public Level Level
        {
            get { return level; }
        }
        Level level;

        /// <summary>
        /// Position in world space of the bottom center of this enemy.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
        }
        Vector2 position;

        #region BoundingRectangles
        private Rectangle localBounds;
        /// <summary>
        /// Gets a rectangle which bounds this enemy in world space.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }
        #endregion

        #region SpotlightRectangle
        public Rectangle SpotlightRectangle
        {
             get
             {
                  int left = (int)Math.Round
                       (Position.X - sprite.Origin.X) +
                       localBounds.X;
                  int top = (int)Math.Round
                       (Position.Y - sprite.Origin.Y) +
                       localBounds.Y;
 
                  if((int)direction == 1)
                       return new Rectangle(
                            left + localBounds.Width,
                            top,
                            spotlightTexture.Width,
                            (spotlightTexture.Height / 2));
                  else
                       return new Rectangle(
                            left - spotlightTexture.Width,
                            top,
                            spotlightTexture.Width,
                            (spotlightTexture.Height / 2));
             }
        }
        #endregion

        public bool iSeeYou;
        Texture2D spotlightTexture; 

        // Animations
        private Animation runAnimation;
        private Animation idleAnimation;
        private AnimationPlayer sprite;
        private Animation dieAnimation;

        // Sounds
        private SoundEffect killedSound;

        public bool IsAlive { get; private set; }

        private const float deathTimeMax = 1.0f;
        public float deathTime = deathTimeMax;

        /// <summary>
        /// The direction this enemy is facing and moving along the X axis.
        /// </summary>
        private FaceDirection direction = FaceDirection.Left;

        /// <summary>
        /// How long this enemy has been waiting before turning around.
        /// </summary>
        private float waitTime;

        /// <summary>
        /// How long to wait before turning around.
        /// </summary>
        private const float MaxWaitTime = 0.5f;

        /// <summary>
        /// The speed at which this enemy moves along the X axis.
        /// </summary>
        private const float defaultSpeed = 64.0f;
        private const float playerSpottedSpeed = defaultSpeed * 3;

        public enum enemyAI { pacer, patroller, flyer }
        enemyAI aiType;

        #endregion

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public Enemy(Level level, Vector2 position, string spriteSet, enemyAI aiType)
        {
            this.level = level;
            this.position = position;
            this.IsAlive = true;
            this.aiType = aiType;

            LoadContent(spriteSet);
        }

        /// <summary>
        /// Loads a particular enemy sprite sheet and sounds.
        /// </summary>
        public void LoadContent(string spriteSet)
        {
            // Load animations.
            spriteSet = "Sprites/" + spriteSet + "/";
            runAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Run"), 0.1f, true);
            idleAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Idle"), 0.15f, true);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Die"), 0.07f, false);
            sprite.PlayAnimation(idleAnimation);

            // Load sounds.
            killedSound = Level.Content.Load<SoundEffect>("Sounds/MonsterKilled");

            // Calculate bounds within texture size.
            int width = (int)(idleAnimation.FrameWidth * 0.35);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.7);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);

            // Used to display where the enemy can see and also create the bounding rectangle for enemy sight.
            spotlightTexture = Level.Content.Load<Texture2D>("Overlays/spotlight2");
        }


        /// <summary>
        /// Paces back and forth along a platform, waiting at either end.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (IsAlive)
            {
                UpdateEnemy(gameTime);
            }
            else
            {
                deathTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        private void UpdateEnemy(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calculate tile position based on the side we are walking towards.
            float posX = Position.X + localBounds.Width / 2 * (int)direction;
            int tileX = (int)Math.Floor(posX / Tile.Width) - (int)direction;
            int tileY = (int)Math.Floor(Position.Y / Tile.Height);

            // For enemy spotlight
            if (SpotlightRectangle.Intersects(Level.Player.BoundingRectangle))
                iSeeYou = true;
            else
                iSeeYou = false;

            if (waitTime > 0)
            {
                // Wait for some amount of time.
                waitTime = Math.Max(0.0f, waitTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
                if (waitTime <= 0.0f)
                {
                    // Then turn around.
                    direction = (FaceDirection)(-(int)direction);
                }
            }
            else
            {
                // If we are about to run into a wall or off a cliff, start waiting.
                if (Level.GetCollision(tileX + (int)direction, tileY - 1) == TileCollision.Impassable ||
                    Level.GetCollision(tileX + (int)direction, tileY) == TileCollision.Passable)
                {
                    waitTime = MaxWaitTime;
                }
                else
                {
                    switch (aiType)
                    {
                        case enemyAI.pacer:
                            EnemyMovement(elapsed, defaultSpeed);
                            break;
                        case enemyAI.patroller:
                            if (iSeeYou)
                            {
                                EnemyMovement(elapsed, playerSpottedSpeed);
                            }
                            else
                            {
                                EnemyMovement(elapsed, defaultSpeed);
                            }
                            break;
                        case enemyAI.flyer:
                            if (iSeeYou)
                            {
                                EnemyMovement(elapsed, playerSpottedSpeed);
                            }
                            else
                            {
                                EnemyMovement(elapsed, defaultSpeed);
                            }
                            break;
                    }
                }
            }
        }

        private void EnemyMovement(float elapsed, float moveSpeed)
        {
            // Move in the current direction.
            Vector2 velocity = new Vector2((int)direction * moveSpeed * elapsed, 0.0f);
            position = position + velocity;
        }

        #region Draw
        /// <summary>
        /// Draws the animated enemy.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Play death animation if we're dead, stop running
            // when the game is paused or before turning around.
            if (deathTime < deathTimeMax)
                sprite.PlayAnimation(dieAnimation);
            else if (!Level.Player.IsAlive ||
                Level.ReachedExit ||
                waitTime > 0)
            {
                sprite.PlayAnimation(idleAnimation);
            }
            else
            {
                sprite.PlayAnimation(runAnimation);
            }

            // Draw facing the way the enemy is moving.
            SpriteEffects flip = direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            sprite.Draw(gameTime, spriteBatch, Position, flip);

            // For enemy spotlight. Uncomment to see enemy line of sight.
            //if (iSeeYou)
            //    spriteBatch.Draw(spotlightTexture, SpotlightRectangle, null, Color.Red);
            //else
            //    spriteBatch.Draw(spotlightTexture, SpotlightRectangle, null, Color.White);
        }
        #endregion

        public void OnKilled()
        {
            IsAlive = false;
            killedSound.Play();
        }

        public void OnKilled(Player killedBy)
        {
            IsAlive = false;
            killedSound.Play();
        }
    }
}
