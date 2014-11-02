#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using System.Windows.Forms;
#endregion

namespace OktoberQuest
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class MainMenuScreen : MenuScreen
    {
      #region Initialization


        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreen() : base("OktoberQuest")
        {
            // Create our menu entries.
            MenuEntry playerProfileMenuEntry = new MenuEntry("Player Profile");
            MenuEntry helpMenuEntry = new MenuEntry("Help");
            MenuEntry exitMenuEntry = new MenuEntry("Exit");

            // Hook up menu event handlers.
            playerProfileMenuEntry.Selected += PlayerProfileMenuEntrySelected;
            helpMenuEntry.Selected += HelpMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(playerProfileMenuEntry);
            MenuEntries.Add(helpMenuEntry);
            MenuEntries.Add(exitMenuEntry);
        }

        #endregion

        #region Handle Input

        /// <summary>
        /// Event handler for when the Player Profile menu entry is selected.
        /// </summary>
        void PlayerProfileMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
          ScreenManager.AddScreen(new ProfileScreen(), e.PlayerIndex);
        }

        
        /// <summary>
        /// Event handler for when the Help menu entry is selected.
        /// </summary>
        void HelpMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            System.Diagnostics.Process.Start(Application.StartupPath + "/Content/Help/OktoberQuestHelp.chm");  
        }

        /// <summary>
        /// When the user cancels the main menu, ask if they want to exit the sample.
        /// </summary>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            const string message = "Are you sure you want to exit the game?";

            MessageBoxScreen confirmExitMessageBox = new MessageBoxScreen(message);

            confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmExitMessageBox, playerIndex);
        }

        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.Game.Exit();
        }

        #endregion                
    }
}
