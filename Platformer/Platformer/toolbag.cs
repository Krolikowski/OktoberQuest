using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace OktoberQuest
{
    class toolbag
    {
        

        private string[] toolBag = {"grap", "boom"};//will be a string so player.cs can handle the tools
                                                
        private int currentToolIndex;

        public toolbag()
        {
            currentToolIndex = 0;
        }
        
        public string getCurrentTool()
        {
            return toolBag[currentToolIndex];
        }

        public void shiftRight()
        {
            if (toolBag.Length - 1 > currentToolIndex)
            { //to see if we have more tools
                currentToolIndex++;
            }
            else
            {
                currentToolIndex = 0;
            }
        }

        public void shiftLeft()
        {
            if (currentToolIndex == 0)
            {
                currentToolIndex = toolBag.Length - 1;
            }
            else
            {
                currentToolIndex--;

            }
        }
    }
}
