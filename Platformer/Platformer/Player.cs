#region File Description
//-----------------------------------------------------------------------------
// Player.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using System.IO;

namespace OktoberQuest
{
    /// <summary>
    /// Our fearless adventurer!
    /// </summary>
    class Player
    {
        #region Declarations and Properties

        // For use with gun.
        GameObject arm;
        Vector2 armPosOffset = new Vector2(5, 18);
        //tool bag class
       
        public toolbag tools = new toolbag();

        GameObject boomerang;
        int boomerangFlyTime;
        const int boomerangMaxFlyTime = 30;

        GameObject grapplinghook;
        int grapplinghookFlyTime;
        bool isGrapplinghookConnected;
        const int grapplinghookMaxFlyTime = 30;

        GamePadState previousGamePadState;
        KeyboardState previousKeyboardState;

        // Animations
        private Animation idleAnimation;
        private Animation runAnimation;
        private Animation jumpAnimation;
        private Animation celebrateAnimation;
        private Animation dieAnimation;
        private Animation attackAnimation;
        private SpriteEffects flip = SpriteEffects.None;
        private AnimationPlayer sprite;

        // Sounds
        private SoundEffect killedSound;
        private SoundEffect jumpSound;
        private SoundEffect fallSound;
        //private Stream jumpFile;
        //private SoundEffect jumpEffect;
        //private SoundEffectInstance jumpInst;

        public Level Level
        {
            get { return level; }
        }
        Level level;

        public bool IsAlive
        {
            get { return isAlive; }
        }
        bool isAlive;

        // Physics state
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        Vector2 position;

        private float previousBottom;

        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }
        Vector2 velocity;

        // Constants for controling horizontal movement
        private const float MoveAcceleration = 13000.0f;
        private const float MaxMoveSpeed = 1750.0f;
        private const float GroundDragFactor = 0.48f;
        private const float AirDragFactor = 0.58f;

        // Constants for controlling vertical movement
        private const float MaxJumpTime = 0.35f;
        private const float JumpLaunchVelocity = -3500.0f;
        private const float GravityAcceleration = 3400.0f;
        private const float MaxFallSpeed = 550.0f;
        private const float JumpControlPower = 0.14f; 

        // Input configuration
        private const float MoveStickScale = 1.0f;
        private const float AccelerometerScale = 1.5f;
        private const Buttons JumpButton = Buttons.A;
        private const Buttons AttackButton = Buttons.X;

        /// <summary>
        /// Gets whether or not the player's feet are on the ground.
        /// </summary>
        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        bool isOnGround;

        /// <summary>
        /// Current user movement input.
        /// </summary>
        private float movement;

        // Jumping state
        private bool isJumping;
        private bool wasJumping;
        private float jumpTime;

        // Attacking state
        private bool isAttacking;
        public bool stillAttacking;                    // True when maxAttackTime has not been reached.
        private bool attackWait;                        // True when maxAttackInterval has not been reached.
        private float attackTime;                       // Incremented until it hits the maxAttackTime.
        private float attackInterval = 0.0f;            // Incremented until it hits maxAttackInterval.
        private const float maxAttackTime = 0.33f;      // The time for the attack animation to display. 
        private const float maxAttackInterval = 0.4f;   // How long the wait is between consecutive attacks.

        // Tool Use state
        private bool isToolUse;
        public bool stillToolUse;                    
        private bool toolUseWait;                       
        private float toolUseTime;                       
        private float toolUseInterval = 0.0f;            
        private const float maxToolUseTime = 0.33f;      
        private const float maxToolUseInterval = 0.4f;   


        #endregion

        #region BoundingRectangles
        private Rectangle localBounds;
        /// <summary>
        /// Gets a rectangle which bounds this player in world space.
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

        public Rectangle MeleeRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;
                if (flip == SpriteEffects.FlipHorizontally)
                    return new Rectangle(
                        (left + localBounds.Width),
                        top,
                        localBounds.Width,
                        localBounds.Height);
                else
                    return new Rectangle(
                        (left - localBounds.Width),
                        top,
                        localBounds.Width,
                        localBounds.Height);
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructors a new player.
        /// </summary>
        public Player(Level level, Vector2 position)
        {
            this.level = level;

            LoadContent();

            Reset(position);
        }
        #endregion

        #region LoadContent
        /// <summary>
        /// Loads the player sprite sheet and sounds.
        /// </summary>
        public void LoadContent()
        {
            // Load animated textures.
            idleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Idle"), 0.1f, true);
            runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Run"), 0.1f, true);
            jumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Jump"), 0.1f, false);
            celebrateAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Celebrate"), 0.1f, false);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Die"), 0.1f, false);
            attackAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Attack"), 0.1f, false);

            // Calculate bounds within texture size.            
            int width = (int)(idleAnimation.FrameWidth * 0.4);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.8);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);

            // Load sounds.            
            killedSound = Level.Content.Load<SoundEffect>("Sounds/PlayerKilled");
            jumpSound = Level.Content.Load<SoundEffect>("Sounds/PlayerJump");
            //jumpFile = TitleContainer.OpenStream(@"Content\Sounds\PlayerJump.wma");
            //jumpEffect = SoundEffect.FromStream(jumpFile);
            //jumpInst = jumpEffect.CreateInstance();
            //jumpInst.Volume = 0.5f;
            fallSound = Level.Content.Load<SoundEffect>("Sounds/PlayerFall");

            // For arm gun.
            arm = new GameObject(Level.Content.Load<Texture2D>("Sprites/Player/Arm_Gun"));
            boomerang = new GameObject(Level.Content.Load<Texture2D>("Tools/boomerang"));
            grapplinghook = new GameObject(Level.Content.Load<Texture2D>("Tools/boomerang"));
        }
        #endregion

        #region Reset
        /// <summary>
        /// Resets the player to life.
        /// </summary>
        /// <param name="position">The position to come to life at.</param>
        public void Reset(Vector2 position)
        {
            Position = position;
            Velocity = Vector2.Zero;
            isAlive = true;
            sprite.PlayAnimation(idleAnimation);
        }
        #endregion

        #region Update
        /// <summary>
        /// Handles input, performs physics, and animates the player sprite.
        /// </summary>
        /// <remarks>
        /// We pass in all of the input states so that our game is only polling the hardware
        /// once per frame. We also pass the game's orientation because when using the accelerometer,
        /// we need to reverse our motion when the orientation is in the LandscapeRight orientation.
        /// </remarks>
        public void Update(
            GameTime gameTime, 
            KeyboardState keyboardState, 
            GamePadState gamePadState,
            AccelerometerState accelState,
            DisplayOrientation orientation)
        {
            GetInput(keyboardState, gamePadState, accelState, orientation);

            ApplyPhysics(gameTime);

            DoAttack(gameTime);

            DoToolUse(gameTime);

            if (IsAlive && IsOnGround && !stillAttacking)
            {
                if (Math.Abs(Velocity.X) - 0.02f > 0)
                {
                    sprite.PlayAnimation(runAnimation);
                }
                else
                {
                    sprite.PlayAnimation(idleAnimation);
                }
            }

            // Clear input.
            movement = 0.0f;
            isJumping = false;
            isAttacking = false;
            isToolUse = false;

            // For arm gun.
            if (flip == SpriteEffects.FlipHorizontally)
                arm.position = new Vector2(position.X + 5, position.Y - 60);
            else
                arm.position = new Vector2(position.X - 15, position.Y - 60);

            UpdateBoomerang();

            UpdateGrapplinghook(gameTime);
        }
        #endregion

        #region GrapplingHook Logic

        private void FireGrapplinghook()
        {
            if (!grapplinghook.alive)
            {
                grapplinghook.alive = true;

                if (flip == SpriteEffects.FlipHorizontally)
                {
                    float armCos = (float)Math.Cos(arm.rotation - MathHelper.PiOver2);
                    float armSin = (float)Math.Sin(arm.rotation - MathHelper.PiOver2);

                    grapplinghook.position = new Vector2(
                        arm.position.X + 50 * armCos,
                        arm.position.Y + 50 * armSin);

                    grapplinghook.velocity = new Vector2(
                        armCos,
                        armSin) * 10.0f;
                }

                else
                {
                    float armCos = (float)Math.Cos(arm.rotation - MathHelper.PiOver2);
                    float armSin = (float)Math.Sin(arm.rotation - MathHelper.PiOver2);

                    grapplinghook.position = new Vector2(
                        arm.position.X - 50 * armCos,
                        arm.position.Y - 50 * armSin);

                    grapplinghook.velocity = new Vector2(
                        -armCos,
                        -armSin) * 10.0f;
                }
            }
            return;
        }

        private void UpdateGrapplinghook(GameTime gameTime)
        {
            if (grapplinghook.alive == true)
            {
                if (isGrapplinghookConnected == true) //did the hook connected with an impassable block
                {
                    float delta = (float)gameTime.ElapsedGameTime.TotalSeconds * 60 * 10;
                    Vector2 direction = grapplinghook.position - this.position;
                    direction.Normalize();

                    this.position += direction * delta;

                }

                if (grapplinghook.alive) //is the hook still being displayed?
                {

                    grapplinghookFlyTime += 1;

                    if (grapplinghookFlyTime > grapplinghookMaxFlyTime)
                    {
                        grapplinghook.alive = false;
                    }
                    else
                    {
                        grapplinghook.position += grapplinghook.velocity;
                    }
                }
                //collsion rect
                Rectangle grapplinghookRect = new Rectangle(
                    (int)grapplinghook.position.X - grapplinghook.sprite.Width * 2,
                    (int)grapplinghook.position.Y - grapplinghook.sprite.Height * 2,
                    grapplinghook.sprite.Width * 4,
                    grapplinghook.sprite.Height * 4);

                foreach (Enemy enemy in level.enemies)
                {
                    if (grapplinghookRect.Intersects(enemy.BoundingRectangle))
                    {
                        grapplinghook.alive = false;//if it hits an enemy destroy it.
                    }
                }

                Rectangle bounds = new Rectangle(
                    grapplinghookRect.Center.X - 6,
                    grapplinghookRect.Center.Y - 6,
                    grapplinghookRect.Width / 4,
                    grapplinghookRect.Height / 4);

                int lefttile = (int)Math.Floor((float)bounds.Left / Tile.Width);
                int righttile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
                int toptile = (int)Math.Floor((float)bounds.Top / Tile.Height);
                int bottomtile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;


                for (int y = toptile; y <= bottomtile; y++)
                {
                    for (int x = lefttile; x <= righttile; x++)
                    {
                        TileCollision collision = Level.GetCollision(x, y);

                        if (collision == TileCollision.Impassable ||
                            collision == TileCollision.Platform)
                        {
                            if (grapplinghookRect.Intersects(bounds))
                            {
                                isGrapplinghookConnected = true;
                            }


                        }
                    }
                }

                Rectangle pbounds = new Rectangle(
                    (int)this.position.X - 6,
                    (int)this.position.Y - 6,
                    this.BoundingRectangle.Width / 4,
                    this.BoundingRectangle.Height / 4);

                int plefttile = (int)Math.Floor((float)pbounds.Left / Tile.Width);
                int prighttile = (int)Math.Ceiling(((float)pbounds.Right / Tile.Width)) - 1;
                int ptoptile = (int)Math.Floor((float)pbounds.Top / Tile.Height);
                int pbottomtile = (int)Math.Ceiling(((float)pbounds.Bottom / Tile.Height)) - 1;


                for (int y = ptoptile; y <= pbottomtile; y++)
                {
                    for (int x = plefttile; x <= prighttile; x++)
                    {
                        TileCollision collision = Level.GetCollision(x, y);

                        if (collision == TileCollision.Impassable ||
                            collision == TileCollision.Platform)
                        {
                            if (this.BoundingRectangle.Intersects(pbounds))
                            {
                                isGrapplinghookConnected = false;
                                grapplinghook.alive = false;
                            }


                        }
                    }
                }
            }
        }
        #endregion

        #region Boomerang Logic

        private void FireBoomerang()
        {
            if (!boomerang.alive)
            {
                boomerang.alive = true;
                boomerangFlyTime = 0;

                if (flip == SpriteEffects.FlipHorizontally)
                {
                    float armCos = (float)Math.Cos(arm.rotation - MathHelper.PiOver2);
                    float armSin = (float)Math.Sin(arm.rotation - MathHelper.PiOver2);

                    boomerang.position = new Vector2(
                        arm.position.X + 42 * armCos,
                        arm.position.Y + 42 * armSin);

                    boomerang.velocity = new Vector2(
                        armCos,
                        armSin) * 10.0f;
                }
                else
                {
                    float armCos = (float)Math.Cos(arm.rotation - MathHelper.PiOver2);
                    float armSin = (float)Math.Sin(arm.rotation - MathHelper.PiOver2);

                    boomerang.position = new Vector2(
                        arm.position.X - 42 * armCos,
                        arm.position.Y - 42 * armSin);

                    boomerang.velocity = new Vector2(
                        -armCos,
                        -armSin) * 10.0f;
                }

                return;
            }
        }

        private void UpdateBoomerang()
        {
            if (boomerang.alive)
            {
                boomerangFlyTime += 1;

                if ((int) boomerangFlyTime / 2 > boomerangMaxFlyTime)
                {
                    boomerang.alive = false;
                }

                else if (boomerangFlyTime > boomerangMaxFlyTime)
                {
                    boomerang.position -= boomerang.velocity;
                }
                else
                {
                    boomerang.position += boomerang.velocity;
                }
                

                /*
                Rectangle screenRect = new Rectangle(0, 0, 1280, 720);
                //Used to prevent the boomerang from going on forever
                
                if (!screenRect.Contains(new Point(
                    (int)boomerang.position.X,
                    (int)boomerang.position.Y)))
                {
                    boomerang.alive = false;
                }
                */
                //collsion rect
                Rectangle boomerangRect = new Rectangle(
                    (int)boomerang.position.X - boomerang.sprite.Width * 2,
                    (int)boomerang.position.Y - boomerang.sprite.Height * 2,
                    boomerang.sprite.Width * 4,
                    boomerang.sprite.Height * 4);

                foreach (Enemy enemy in level.enemies)
                {
                    if (boomerangRect.Intersects(enemy.BoundingRectangle))
                    {
                        enemy.OnKilled();//enemy.IsAlive = false;
                    }
                }

                Rectangle bounds = new Rectangle(
                    boomerangRect.Center.X - 6,
                    boomerangRect.Center.Y - 6,
                    boomerangRect.Width / 4,
                    boomerangRect.Height / 4);

                int lefttile = (int)Math.Floor((float)bounds.Left / Tile.Width);
                int righttile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
                int toptile = (int)Math.Floor((float)bounds.Top / Tile.Height);
                int bottomtile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

                for (int y = toptile; y <= bottomtile; y++)
                {
                    for (int x = lefttile; x <= righttile; x++)
                    {
                        TileCollision collision = Level.GetCollision(x, y);

                        if (collision == TileCollision.Impassable ||
                            collision == TileCollision.Platform)
                        {
                            if (boomerangRect.Intersects(bounds))
                            {
                                boomerang.alive = false;
                            }
                        }
                    }
                }
            }
        }

            
        #endregion

        #region GetInput
        /// <summary>
        /// Gets player horizontal movement and jump commands from input.
        /// </summary>
        private void GetInput(
            KeyboardState keyboardState, 
            GamePadState gamePadState,
            AccelerometerState accelState, 
            DisplayOrientation orientation)
        {
            // Get analog horizontal movement.
            movement = gamePadState.ThumbSticks.Left.X * MoveStickScale;

            // Ignore small movements to prevent running in place.
            if (Math.Abs(movement) < 0.5f)
                movement = 0.0f;

            // Move the player with accelerometer
            if (Math.Abs(accelState.Acceleration.Y) > 0.10f)
            {
                // set our movement speed
                movement = MathHelper.Clamp(-accelState.Acceleration.Y * AccelerometerScale, -1f, 1f);

                // if we're in the LandscapeLeft orientation, we must reverse our movement
                if (orientation == DisplayOrientation.LandscapeRight)
                    movement = -movement;
            }

            // If any digital horizontal movement input is found, override the analog movement.
            if (gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                keyboardState.IsKeyDown(Keys.Left) ||
                keyboardState.IsKeyDown(Keys.A))
            {
                movement = -1.0f;
            }
            else if (gamePadState.IsButtonDown(Buttons.DPadRight) ||
                     keyboardState.IsKeyDown(Keys.Right) ||
                     keyboardState.IsKeyDown(Keys.D))
            {
                movement = 1.0f;
            }

            //Checking if player wants to switch tool
            if(keyboardState.IsKeyDown(Keys.V))
            {
               tools.shiftRight();
            }

            if (keyboardState.IsKeyDown(Keys.C))
            {
                tools.shiftLeft();
            }



            // Check if the player wants to jump.
            isJumping =
                gamePadState.IsButtonDown(JumpButton) ||
                keyboardState.IsKeyDown(Keys.Space) ||
                keyboardState.IsKeyDown(Keys.Up) ||
                keyboardState.IsKeyDown(Keys.W);

            //// Attack key binding.
            //if ((previousKeyboardState.IsKeyUp(Keys.F) && keyboardState.IsKeyDown(Keys.F)) ||
            //    previousGamePadState.IsButtonUp(AttackButton) && gamePadState.IsButtonDown(AttackButton))
            //    if (keyboardState.IsKeyDown(Keys.F) || gamePadState.IsButtonDown(AttackButton))
            //    {

            //        isAttacking = true;

            //    }


            isAttacking =
                gamePadState.IsButtonDown(AttackButton) ||
                keyboardState.IsKeyDown(Keys.F);

            //Checking if players wants to use the current tool
            isToolUse = keyboardState.IsKeyDown(Keys.Q);

            if ((previousGamePadState.Triggers.Right < .5 && gamePadState.Triggers.Right > .5) 
                || (previousKeyboardState.IsKeyDown(Keys.D2) && keyboardState.IsKeyUp(Keys.D2)))
                FireBoomerang();

            if ((previousGamePadState.Triggers.Left < .5 && gamePadState.Triggers.Left > .5)
                || (previousKeyboardState.IsKeyDown(Keys.D3) && keyboardState.IsKeyUp(Keys.D3)))
                FireGrapplinghook();

            #region ArmRotation
            //Arm rotation
            
            if( (float)Math.Atan2(gamePadState.ThumbSticks.Right.X, gamePadState.ThumbSticks.Right.Y) != 0 )
            {
                arm.rotation = (float)Math.Atan2(gamePadState.ThumbSticks.Right.X, gamePadState.ThumbSticks.Right.Y);
            }
            else if(keyboardState.IsKeyDown(Keys.D1))
            {
                arm.rotation += .18f;
            }
            if (flip == SpriteEffects.FlipHorizontally) //Facing right
            {
                //If we try to aim behind our head then flip the
                //character around so he doesn't break his arm!
                if (arm.rotation < 0)
                    flip = SpriteEffects.FlipHorizontally;
 
                //If we aren't rotating our arm then set it to the
                //default position. Aiming in front of us.
                if (arm.rotation == 0 && Math.Abs(gamePadState.ThumbSticks.Right.Length()) < 0.5f)
                    arm.rotation = MathHelper.PiOver2;
            }
            else //Facing left
            {
                //Once again, if we try to aim behind us then
                //flip our character.
                if (arm.rotation > 0)
                    flip = SpriteEffects.FlipHorizontally;
 
                //If we're not rotating our arm, default it to
                //aim the same direction we're facing.
                if (arm.rotation == 0 && Math.Abs(gamePadState.ThumbSticks.Right.Length()) < 0.5f)
                    arm.rotation = -MathHelper.PiOver2;
            }
            #endregion

            previousGamePadState = gamePadState;
            previousKeyboardState = keyboardState;
        }
        #endregion

        #region Physics
        /// <summary>
        /// Updates the player's velocity and position based on input, gravity, etc.
        /// </summary>
        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = Position;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            velocity.X += movement * MoveAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

            velocity.Y = DoJump(velocity.Y, gameTime);

            // Apply pseudo-drag horizontally.
            if (IsOnGround)
                velocity.X *= GroundDragFactor;
            else
                velocity.X *= AirDragFactor;

            // Prevent the player from running faster than his top speed.            
            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            // Apply velocity.
            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            // If the player is now colliding with the level, separate them.
            HandleCollisions();

            // If the collision stopped us from moving, reset the velocity to zero.
            if (Position.X == previousPosition.X)
                velocity.X = 0;

            if (Position.Y == previousPosition.Y)
                velocity.Y = 0;
        }
        #endregion

        #region DoJump
        /// <summary>
        /// Calculates the Y velocity accounting for jumping and
        /// animates accordingly.
        /// </summary>
        /// <remarks>
        /// During the accent of a jump, the Y velocity is completely
        /// overridden by a power curve. During the decent, gravity takes
        /// over. The jump velocity is controlled by the jumpTime field
        /// which measures time into the accent of the current jump.
        /// </remarks>
        /// <param name="velocityY">
        /// The player's current velocity along the Y axis.
        /// </param>
        /// <returns>
        /// A new Y velocity if beginning or continuing a jump.
        /// Otherwise, the existing Y velocity.
        /// </returns>
        private float DoJump(float velocityY, GameTime gameTime)
        {
            // If the player wants to jump
            if (isJumping)
            {
                // Begin or continue a jump
                if ((!wasJumping && IsOnGround) || jumpTime > 0.0f)
                {
                    if (jumpTime == 0.0f)
                        jumpSound.Play();
                        //jumpInst.Play();
                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    sprite.PlayAnimation(jumpAnimation);
                }

                // If we are in the ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                }
                else
                {
                    // Reached the apex of the jump
                    jumpTime = 0.0f;
                }
            }
            else
            {
                // Continues not jumping or cancels a jump in progress
                jumpTime = 0.0f;
            }
            wasJumping = isJumping;

            return velocityY;
        }
        #endregion

        #region DoToolUse
        private void DoToolUse(GameTime gameTime)
        {
            
            if (!toolUseWait)
            {
                // If the player wants to use current tool
                if (isToolUse || toolUseTime > 0.0f)
                {
                    // Begin or continue tool use
                    if (attackTime <= maxAttackTime)
                    {
                        toolUseTime += (float)gameTime.ElapsedGameTime.TotalSeconds;                
                        
                        //This is the section to check which tool to use!
                        if (tools.getCurrentTool() == "grap")//name gotten from toolbag class
                        { //we don't have any animations for it or sounds yet!
                            //todo: grapping hook code here
                            
                        }

                        stillToolUse = true;
                    }
                    
                    else
                    {
                        toolUseTime = 0.0f;
                        stillToolUse = false;
                        toolUseWait = true;
                    }
                }
                else
                {
                    
                    toolUseTime = 0.0f;
                    stillToolUse = false;
                }
            }
            else
            {
                toolUseInterval += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (maxToolUseInterval < toolUseInterval)
                {
                    toolUseWait = false;
                    toolUseInterval = 0.0f;
                }
            }
        }
        #endregion

        #region DoAttack
        /// <summary>
        /// Executes the player melee attack animation based on a button push from GetInput.
        /// Only allows the player to attack on a fixed interval dictated by the maxAttackInterval
        /// variable.
        /// </summary>
        private void DoAttack(GameTime gameTime)
        {
            // Only attack if another attack was not made withing the max attack interval.
            if (!attackWait)
            {
                // If the player wants to attack
                if (isAttacking || attackTime > 0.0f)
                {
                    // Begin or continue an attack
                    if (attackTime <= maxAttackTime)
                    {
                        attackTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                        sprite.PlayAnimation(attackAnimation);
                        stillAttacking = true;
                    }
                    // Reset the attack variables and start the attack interval.
                    else
                    {
                        attackTime = 0.0f;
                        stillAttacking = false;
                        attackWait = true;
                    }
                }
                else
                {
                    //Continues not attack or cancels an attack in progress
                    attackTime = 0.0f;
                    stillAttacking = false;
                }
            }
            else
            {
                attackInterval += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (maxAttackInterval < attackInterval)
                {
                    attackWait = false;
                    attackInterval = 0.0f;
                }
            }
        }
        #endregion

        #region HandleCollisions
        /// <summary>
        /// Detects and resolves all collisions between the player and his neighboring
        /// tiles. When a collision is detected, the player is pushed away along one
        /// axis to prevent overlapping. There is some special logic for the Y axis to
        /// handle platforms which behave differently depending on direction of movement.
        /// </summary>
        private void HandleCollisions()
        {
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            // Reset flag to search for ground collision.
            isOnGround = false;

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = Level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            // Resolve the collision along the shallow axis.
                            if (absDepthY < absDepthX || collision == TileCollision.Platform)
                            {
                                // If we crossed the top of a tile, we are on the ground.
                                if (previousBottom <= tileBounds.Top)
                                    isOnGround = true;

                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable || IsOnGround)
                                {
                                    // Resolve the collision along the Y axis.
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);

                                    // Perform further collisions with the new bounds.
                                    bounds = BoundingRectangle;
                                }
                            }
                            else if (collision == TileCollision.Impassable) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                Position = new Vector2(Position.X + depth.X, Position.Y);

                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;
                            }
                        }
                    }
                }
            }

            // Save the new bounds bottom.
            previousBottom = bounds.Bottom;
        }
        #endregion

        /// <summary>
        /// Called when the player has been killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This parameter is null if the player was
        /// not killed by an enemy (fell into a hole).
        /// </param>
        public void OnKilled(Enemy killedBy)
        {
            isAlive = false;

            if (killedBy != null)
                killedSound.Play();
            else
                fallSound.Play();

            sprite.PlayAnimation(dieAnimation);
        }

        /// <summary>
        /// Called when this player reaches the level's exit.
        /// </summary>
        public void OnReachedExit()
        {
            sprite.PlayAnimation(celebrateAnimation);
        }

        #region Draw
        /// <summary>
        /// Draws the animated player.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Flip the sprite to face the way we are moving.
            if (Velocity.X > 0)
                flip = SpriteEffects.FlipHorizontally;
            else if (Velocity.X < 0)
                flip = SpriteEffects.None;

            // Draw that sprite.
            sprite.Draw(gameTime, spriteBatch, Position, flip);

            if (IsAlive)
            {
                spriteBatch.Draw(
                    arm.sprite,
                    (arm.position + armPosOffset),
                    null,
                    Color.White,
                    arm.rotation,
                    arm.center,
                    1.0f,
                    flip,
                    0);
            }

            if (boomerang.alive)
            {
                spriteBatch.Draw(boomerang.sprite, boomerang.position, Color.White);
            }

            if (grapplinghook.alive)
            {
                spriteBatch.Draw(grapplinghook.sprite, grapplinghook.position, Color.White);
            }
        }
        #endregion
    }
}
