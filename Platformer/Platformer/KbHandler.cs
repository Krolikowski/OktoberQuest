using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace OktoberQuest
{
    public class KbHandler
    {
        private Keys[] lastPressedKeys;
        public string keysPressed = "";
        private bool shift = false;
        public bool loop = true;

        public KbHandler()
        {
            lastPressedKeys = new Keys[0];
        }

        public void Update()
        {
            KeyboardState kbState = Keyboard.GetState();
            Keys[] pressedKeys = kbState.GetPressedKeys();

            // Check if any of the previous update's keys are no longer pressed
            foreach (Keys key in lastPressedKeys)
            {
                if (!pressedKeys.Contains(key))
                    OnKeyUp(key);
            }

            // Check if the currently pressed keys were already pressed.
            foreach (Keys key in pressedKeys)
            {
                if (!lastPressedKeys.Contains(key))
                {
                    if (key == Keys.LeftShift | key == Keys.RightShift)
                    {
                        shift = true;
                    }
                    else
                    {
                        OnKeyDown(key);
                        shift = false;
                    }
                }
            }

            // Save the currently pressed keys so we can compare on the next update.
            lastPressedKeys = pressedKeys;
        }

        private void OnKeyDown(Keys key)
        {
            // Fires after any toggle of the shift key to produce uppercase letters.
            if (shift == true)
            {
                switch (key)
                {
                    case Keys.A:
                        keysPressed += "A";
                        break;
                    case Keys.B:
                        keysPressed += "B";
                        break;
                    case Keys.C:
                        keysPressed += "C";
                        break;
                    case Keys.D:
                        keysPressed += "D";
                        break;
                    case Keys.E:
                        keysPressed += "E";
                        break;
                    case Keys.F:
                        keysPressed += "F";
                        break;
                    case Keys.G:
                        keysPressed += "G";
                        break;
                    case Keys.H:
                        keysPressed += "H";
                        break;
                    case Keys.I:
                        keysPressed += "I";
                        break;
                    case Keys.J:
                        keysPressed += "J";
                        break;
                    case Keys.K:
                        keysPressed += "K";
                        break;
                    case Keys.L:
                        keysPressed += "L";
                        break;
                    case Keys.M:
                        keysPressed += "M";
                        break;
                    case Keys.N:
                        keysPressed += "N";
                        break;
                    case Keys.O:
                        keysPressed += "O";
                        break;
                    case Keys.P:
                        keysPressed += "P";
                        break;
                    case Keys.Q:
                        keysPressed += "Q";
                        break;
                    case Keys.R:
                        keysPressed += "R";
                        break;
                    case Keys.S:
                        keysPressed += "S";
                        break;
                    case Keys.T:
                        keysPressed += "T";
                        break;
                    case Keys.U:
                        keysPressed += "U";
                        break;
                    case Keys.V:
                        keysPressed += "V";
                        break;
                    case Keys.W:
                        keysPressed += "W";
                        break;
                    case Keys.X:
                        keysPressed += "X";
                        break;
                    case Keys.Y:
                        keysPressed += "Y";
                        break;
                    case Keys.Z:
                        keysPressed += "Z";
                        break;
                }
            }
            else
            {
                switch (key)
                {
                    case Keys.D0:
                        keysPressed += "0";
                        break;
                    case Keys.D1:
                        keysPressed += "1";
                        break;
                    case Keys.D2:
                        keysPressed += "2";
                        break;
                    case Keys.D3:
                        keysPressed += "3";
                        break;
                    case Keys.D4:
                        keysPressed += "4";
                        break;
                    case Keys.D5:
                        keysPressed += "5";
                        break;
                    case Keys.D6:
                        keysPressed += "6";
                        break;
                    case Keys.D7:
                        keysPressed += "7";
                        break;
                    case Keys.D8:
                        keysPressed += "8";
                        break;
                    case Keys.D9:
                        keysPressed += "9";
                        break;
                    case Keys.NumPad0:
                        keysPressed += "0";
                        break;
                    case Keys.NumPad1:
                        keysPressed += "1";
                        break;
                    case Keys.NumPad2:
                        keysPressed += "2";
                        break;
                    case Keys.NumPad3:
                        keysPressed += "3";
                        break;
                    case Keys.NumPad4:
                        keysPressed += "4";
                        break;
                    case Keys.NumPad5:
                        keysPressed += "5";
                        break;
                    case Keys.NumPad6:
                        keysPressed += "6";
                        break;
                    case Keys.NumPad7:
                        keysPressed += "7";
                        break;
                    case Keys.NumPad8:
                        keysPressed += "8";
                        break;
                    case Keys.NumPad9:
                        keysPressed += "9";
                        break;
                    //case Keys.OemPeriod:
                    //    keysPressed += ".";
                    //    break;
                    case Keys.A:
                        keysPressed += "a";
                        break;
                    case Keys.B:
                        keysPressed += "b";
                        break;
                    case Keys.C:
                        keysPressed += "c";
                        break;
                    case Keys.D:
                        keysPressed += "d";
                        break;
                    case Keys.E:
                        keysPressed += "e";
                        break;
                    case Keys.F:
                        keysPressed += "f";
                        break;
                    case Keys.G:
                        keysPressed += "g";
                        break;
                    case Keys.H:
                        keysPressed += "h";
                        break;
                    case Keys.I:
                        keysPressed += "i";
                        break;
                    case Keys.J:
                        keysPressed += "j";
                        break;
                    case Keys.K:
                        keysPressed += "k";
                        break;
                    case Keys.L:
                        keysPressed += "l";
                        break;
                    case Keys.M:
                        keysPressed += "m";
                        break;
                    case Keys.N:
                        keysPressed += "n";
                        break;
                    case Keys.O:
                        keysPressed += "o";
                        break;
                    case Keys.P:
                        keysPressed += "p";
                        break;
                    case Keys.Q:
                        keysPressed += "q";
                        break;
                    case Keys.R:
                        keysPressed += "r";
                        break;
                    case Keys.S:
                        keysPressed += "s";
                        break;
                    case Keys.T:
                        keysPressed += "t";
                        break;
                    case Keys.U:
                        keysPressed += "u";
                        break;
                    case Keys.V:
                        keysPressed += "v";
                        break;
                    case Keys.W:
                        keysPressed += "w";
                        break;
                    case Keys.X:
                        keysPressed += "x";
                        break;
                    case Keys.Y:
                        keysPressed += "y";
                        break;
                    case Keys.Z:
                        keysPressed += "z";
                        break;
                    case Keys.Enter:
                        if (keysPressed.Length > 0)
                        {
                            loop = false;
                        }
                        break;
                    case Keys.Back:
                        if (keysPressed.Length > 0)
                        {
                            keysPressed = keysPressed.Remove(keysPressed.Length - 1, 1);
                        }
                        break;
                    case Keys.Escape:
                        {
                            keysPressed = "Profile";
                            loop = false;
                        }
                        break;
                }
            }
        }

        private void OnKeyUp(Keys key)
        {
            //do stuff
        }

    }
}
