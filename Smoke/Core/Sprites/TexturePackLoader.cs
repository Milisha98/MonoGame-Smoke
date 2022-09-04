using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Smoke.Sprites;

public class TexturePackLoader
{
	public SpriteSheet LoadContent(ContentManager contentManager, string imageResource)
	{
        var texture = contentManager.Load<Texture2D>(imageResource);
        var dataFile = Path.Combine(contentManager.RootDirectory, Path.ChangeExtension(imageResource, "json"));

        var raw =  System.IO.File.ReadAllText(dataFile);
        var json = JsonConvert.DeserializeObject<dynamic>(raw);        

        var spriteSheet = new SpriteSheet();
        foreach (var tile in json!.frames)
        {
            string filename = (string)tile.filename;
            string name = Path.ChangeExtension(filename, null);
            int x = (int)tile.frame.x;
            int y = (int)tile.frame.y;
            int w = (int)tile.frame.w;
            int h = (int)tile.frame.h;
            var rectangle = new Rectangle(x, y, w, h);
            var size = new Vector2((int)tile.sourceSize.w, (int)tile.sourceSize.h);
            var frame = new SpriteFrame(name, texture, rectangle, size); 
            spriteSheet.Add(frame);
        }
        return spriteSheet;
    }

}
