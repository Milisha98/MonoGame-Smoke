using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smoke.Sprites;

public class SpriteSheet
{
	private Dictionary<string, SpriteFrame> _frames;
	
	public SpriteSheet()
	{
		_frames = new Dictionary<string, SpriteFrame>();
    }

	public void Add(SpriteFrame spriteFrame)
	{		
		if (_frames.ContainsKey(spriteFrame.Name))
		{
			_frames[spriteFrame.Name] = spriteFrame;
		}
		else
		{
			_frames.Add(spriteFrame.Name, spriteFrame);
		}
	}

	public void AddRange(SpriteSheet spriteSheet)
	{	
		foreach (var sprite in spriteSheet.SpriteList)
		{
			this.Add(sprite);
		}
	}


	public SpriteFrame? Get(string name) =>
		_frames.GetValueOrDefault(name);


	public List<SpriteFrame> SpriteList
	{
		get => _frames.Values.ToList();
	}
}
