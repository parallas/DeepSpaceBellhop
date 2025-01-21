using System;
using System.Collections.Generic;
using System.IO;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.IO;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ElevatorGame;

public static class ContentLoader
{
    private static ContentManager _content;

    private static readonly List<string> pathsThatDontWork = [];

    public static void Initialize(ContentManager content)
    {
        _content = content;
    }

    public static T? Load<T>(string assetName) where T : class
    {
        if(pathsThatDontWork.Contains(assetName)) return default;

        try
        {
            if(typeof(T).IsAssignableTo(typeof(AsepriteFile)))
            {
                using Stream stream = TitleContainer.OpenStream($"Content/{assetName}.aseprite");
                return AsepriteFileLoader.FromStream(fileName: "file", stream: stream, preMultiplyAlpha: true) as T;
            }
            else if(typeof(T).IsAssignableTo(typeof(SimpleModel)))
            {
                return SimpleModel.Load(assetName) as T;
            }

            return _content.Load<T>(assetName);
        }
        catch(Exception e)
        {
            Console.Error.WriteLine(e.GetType().FullName + $": The content file \"{assetName}\" was not found.");
            pathsThatDontWork.Add(assetName);
            return default;
        }
    }
}
