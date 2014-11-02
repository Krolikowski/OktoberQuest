#region File Description
//-----------------------------------------------------------------------------
// PlayerProfileScreen.cs
//
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
#endregion

namespace OktoberQuest
{
    /// <summary>
    /// The Player profile screen is brought up over the top of the main menu
    /// screen, and gives the user a chance to pick a player profile
    /// to be able to save games.
    /// </summary>
    class ProfileScreen : MenuScreen
    { 
        #region Fields

        MenuEntry profile1;
        MenuEntry profile2;
        MenuEntry profile3;
        MenuEntry profile4;
        MenuEntry back;
        MenuEntry tempProfile;
        KbHandler profileName;
        bool kbLoop = false;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public ProfileScreen() : base("Player Profile")
        {
            // Create our menu entries.
            profile1 = new MenuEntry("Profile");
            profile1.Index = 1;
            profile2 = new MenuEntry("Profile");
            profile2.Index = 2;            
            profile3 = new MenuEntry("Profile");
            profile3.Index = 3;           
            profile4 = new MenuEntry("Profile");
            profile4.Index = 4;
            back = new MenuEntry("Back");           
                   
            

            // Hook up menu event handlers.
            profile1.Selected += selectProfile;
            profile2.Selected += selectProfile;
            profile3.Selected += selectProfile;
            profile4.Selected += selectProfile;
            back.Selected += OnCancel;
           
            SaveLoad loadProfiles = new SaveLoad();
            string tempName;
            // Loads any existing profile names to the correct profile spots.
            for (int profiles = 1; profiles < 5; profiles++)
            {
                loadProfiles.InitiateLoad(profiles);
                tempName = loadProfiles.SaveData.profileName;

                if (tempName != null)
                {
                    // I would really like to implemente something more elegant than this, but
                    // it works for now.
                    switch (profiles)
                    {
                        case 1:
                            profile1.Text = tempName;
                            break;
                        case 2:
                            profile2.Text = tempName;
                            break;
                        case 3:
                            profile3.Text = tempName;
                            break;
                        case 4:
                            profile4.Text = tempName;
                            break;
                    }
                }
            }

            // Add entries to the menu.
            MenuEntries.Add(profile1);
            MenuEntries.Add(profile2);
            MenuEntries.Add(profile3);
            MenuEntries.Add(profile4);
            MenuEntries.Add(back);
                       
            numberOfColumns = 2;
        }

        #endregion

        #region Handle Input

        /// <summary>
        /// Event handler for when the profile menu entry is selected. Allows the player to enter a custom
        /// name into the profile.
        /// </summary>
        private void selectProfile(object sender, PlayerIndexEventArgs e)
        {
            tempProfile = (MenuEntry)sender;
            if (tempProfile.Text == "Profile")
            {
                tempProfile.Text = "";
                profileName = new KbHandler();
                kbLoop = true;
            }
            // Starts the game if a non-empty or newly created profile is selected.
            else if (tempProfile.Text.Length > 0)
            {
                LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, new GameplayScreen());
            }
        }

        public override void HandleInput(InputState input)
        {
            // Prevents menu navigation while typing in a profile name.
            if (kbLoop == false)
            {
                base.HandleInput(input);
            }
        }
        /// <summary>
        /// Event handler for when the delete menu entry is selected. Allows the player to enter a custom
        /// name into the profile.
        /// </summary>
        private void selectDelete(object sender, PlayerIndexEventArgs e)
        {

        }
        #endregion

        /// <summary>
        /// Updates the menu.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Only starts to fire after a profile selected event has been started.
            if (kbLoop == true)
            {
                profileNameUpdate();
            }
        }

        private void profileNameUpdate()
        {
            profileName.Update();

            if (profileName.keysPressed.Length < 11)
            {
                tempProfile.Text = profileName.keysPressed;

                // Stops the if statements from firing if the enter key has been hit
                // during profile name entry.
                if (profileName.loop == false)
                {
                    if (tempProfile.Text != "Profile")
                    {
                        SaveLoad saveProfile = new SaveLoad();
                        saveProfile.InitiateSave(tempProfile.Index, profileName.keysPressed);
                    }
                    kbLoop = false;
                    profileName.keysPressed = null;
                }
            }
            else
            {
                profileName.keysPressed = profileName.keysPressed.Remove(profileName.keysPressed.Length - 1, 1);
            }
        }
    }
}
