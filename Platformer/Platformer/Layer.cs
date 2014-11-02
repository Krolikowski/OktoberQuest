using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


namespace OktoberQuest
{
    class Layer
    {
        public Texture2D[] Textures { get; private set; }
        public float ScrollRate { get; private set; }

        public Layer(ContentManager content, string basePath, float scrollRate)
        {
            Textures = new Texture2D[2];

            for (int i = 0; i < 2; ++i)
            {
                Textures[i] = content.Load<Texture2D>(basePath + "_" + i);
            }

            ScrollRate = scrollRate;
        }

        public void Draw( SpriteBatch spriteBatch, float cameraPositionX, float cameraPositionY, int height )
        {
            //This assumes that all segments are the same width
            int segmentWidth = Textures[0].Width;

            //calc which segments to draw
            float x = cameraPositionX * ScrollRate;
            float y = height * ScrollRate;

            int leftSegment = (int) Math.Floor(x/segmentWidth);
            int rightSegment =  (int) leftSegment + 1;

            x = (x / segmentWidth - leftSegment) * -segmentWidth;

            spriteBatch.Draw(Textures[leftSegment % Textures.Length], new Vector2(x, -y),
                Color.White);
            spriteBatch.Draw(Textures[rightSegment % Textures.Length], new Vector2(x + segmentWidth, -y),
                Color.White);
        }
    }
}
