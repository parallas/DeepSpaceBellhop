using System;
using System.Diagnostics;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Aseprite.Types;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;
using MonoGame.Aseprite.Utils;

namespace ElevatorGame.Source.Elevator;

public class Elevator
{
    private Sprite _elevatorInteriorSprite;
    private Sprite _elevatorLeftDoorSprite;
    private Sprite _elevatorRightDoorSprite;
    private AnimatedSprite _elevatorNumbersAnimSprite;
    
    private AsepriteSliceKey _elevatorDoorLeftSlice;
    private AsepriteSliceKey _elevatorDoorRightSlice;
    private AsepriteSliceKey _elevatorNumberTensSlice;
    private AsepriteSliceKey _elevatorNumberOnesSlice;

    private Vector2 _doorLeftOrigin;
    private Vector2 _doorRightOrigin;

    public void LoadContent()
    {
        // Load the elevator interior sprite
        var elevatorInteriorFile = ContentLoader.Load<AsepriteFile>("graphics/ElevatorInterior");
        _elevatorInteriorSprite = elevatorInteriorFile!.CreateSprite(MainGame.Graphics.GraphicsDevice, 0, true);
        
        // Get the slices
        _elevatorDoorLeftSlice = elevatorInteriorFile.GetSlice("DoorL").Keys[0];
        _elevatorDoorRightSlice = elevatorInteriorFile.GetSlice("DoorR").Keys[0];
        _elevatorNumberTensSlice = elevatorInteriorFile.GetSlice("DigitTens").Keys[0];
        _elevatorNumberOnesSlice = elevatorInteriorFile.GetSlice("DigitOnes").Keys[0];
        
        // Set the target positions for the doors when closed (based on slices)
        var leftDoorSliceBounds = _elevatorDoorLeftSlice.Bounds.ToXnaRectangle();
        var leftDoorTopRight = new Vector2(leftDoorSliceBounds.Right - 1, leftDoorSliceBounds.Y);
        _doorLeftOrigin = leftDoorTopRight;
        var rightDoorSliceBounds = _elevatorDoorRightSlice.Bounds.ToXnaRectangle();
        var rightDoorTopLeft = rightDoorSliceBounds.Location.ToVector2();
        _doorRightOrigin = rightDoorTopLeft;
        
        // Load the door sprites, and set their properties
        var elevatorDoorFile = ContentLoader.Load<AsepriteFile>("graphics/ElevatorDoor");
        _elevatorLeftDoorSprite = elevatorDoorFile!.CreateSprite(MainGame.Graphics.GraphicsDevice, 0, true);
        _elevatorRightDoorSprite = elevatorDoorFile!.CreateSprite(MainGame.Graphics.GraphicsDevice, 0, true);
        _elevatorLeftDoorSprite.Origin = new Vector2(_elevatorLeftDoorSprite.Width - 1, 0);
        _elevatorRightDoorSprite.FlipHorizontally = true;
        
        // Load the animated numbers sprite
        var elevatorNumbersAnimFile = ContentLoader.Load<AsepriteFile>("graphics/ElevatorNumbers");
        var elevatorNumbersSpriteSheet = elevatorNumbersAnimFile!.CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, false);
        _elevatorNumbersAnimSprite = elevatorNumbersSpriteSheet.CreateAnimatedSprite("Tag");
        _elevatorNumbersAnimSprite.Speed = 0;
    }
    
    public void Update(GameTime gameTime)
    {
        
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        _elevatorLeftDoorSprite.Draw(spriteBatch, _doorLeftOrigin + Vector2.UnitX * (MathF.Cos(MainGame.Step / 10f) * 10 - 10));
        _elevatorRightDoorSprite.Draw(spriteBatch, _doorRightOrigin - Vector2.UnitX * (MathF.Cos(MainGame.Step / 10f) * 10 - 10));
        _elevatorInteriorSprite.Draw(spriteBatch, Vector2.Zero);
        // _elevatorNumbersAnimSprite.Draw(MainGame.SpriteBatch, new(0, 0));
    }
}