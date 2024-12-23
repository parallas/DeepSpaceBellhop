using System;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Aseprite.Types;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;

namespace ElevatorGame.Source.Elevator;

public class FloorNumberDisplay
{
    private AnimatedSprite _elevatorNumbersAnimSprite;
    private AsepriteSliceKey _elevatorNumberTensSlice;
    private AsepriteSliceKey _elevatorNumberOnesSlice;
    
    public void LoadContent(Elevator elevator, AsepriteFile elevatorInteriorFile)
    {
        // Load the animated numbers sprite
        var elevatorNumbersAnimFile = ContentLoader.Load<AsepriteFile>("graphics/ElevatorNumbers");
        var elevatorNumbersSpriteSheet = elevatorNumbersAnimFile!.CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, false);
        _elevatorNumbersAnimSprite = elevatorNumbersSpriteSheet.CreateAnimatedSprite("Tag");
        _elevatorNumbersAnimSprite.Speed = 0;
        
        _elevatorNumberTensSlice = elevatorInteriorFile.GetSlice("DigitTens").Keys[0];
        _elevatorNumberOnesSlice = elevatorInteriorFile.GetSlice("DigitOnes").Keys[0];
    }

    public void Draw(SpriteBatch spriteBatch, float floorNumber)
    {
        if ((int)MathF.Round(floorNumber) < 10)
            _elevatorNumbersAnimSprite.SetFrame(10);
        else
            _elevatorNumbersAnimSprite.SetFrame((int)MathF.Round(floorNumber) / 10 % 10);
        _elevatorNumbersAnimSprite.Draw(spriteBatch,
            MainGame.Camera.GetParallaxPosition(_elevatorNumberTensSlice.GetLocation(), Elevator.ParallaxWalls));

        if((int)MathF.Round(floorNumber) == 1)
            _elevatorNumbersAnimSprite.SetFrame(11);
        else
            _elevatorNumbersAnimSprite.SetFrame((int)MathF.Round(floorNumber) % 10);
        _elevatorNumbersAnimSprite.Draw(spriteBatch,
            MainGame.Camera.GetParallaxPosition(_elevatorNumberOnesSlice.GetLocation(), Elevator.ParallaxWalls));
    }
}