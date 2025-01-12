using System;
using System.Collections;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Aseprite.Types;
using Engine;
using FmodForFoxes.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;
using MonoGame.Aseprite.Utils;

namespace ElevatorGame.Source.Elevator;

public class Doors : IDisposable
{
    private readonly Elevator _elevator;

    private Sprite _elevatorLeftDoorSprite;
    private Sprite _elevatorRightDoorSprite;
    private AsepriteSliceKey _elevatorDoorLeftSlice;
    private AsepriteSliceKey _elevatorDoorRightSlice;
    private Vector2 _doorLeftOrigin;
    private Vector2 _doorRightOrigin;

    private EventInstance _audioDoorOpen;
    private EventInstance _audioDoorClose;

    private bool _isOpen = true;
    private float _doorOpenedness;

    public Doors(Elevator elevator, AsepriteFile elevatorInteriorFile)
    {
        _elevator = elevator;

        _elevatorDoorLeftSlice = elevatorInteriorFile.GetSlice("DoorL").Keys[0];
        _elevatorDoorRightSlice = elevatorInteriorFile.GetSlice("DoorR").Keys[0];

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

        _audioDoorOpen = StudioSystem.GetEvent("event:/SFX/Elevator/Doors/Open").CreateInstance();
        _audioDoorClose = StudioSystem.GetEvent("event:/SFX/Elevator/Doors/Close").CreateInstance();
    }

    public void UnloadContent()
    {
        Dispose();
    }

    public void Update(GameTime gameTime)
    {
        int target = _isOpen ? 47 : 0;
        int speed = _isOpen ? 4 : 10;
        _doorOpenedness = MathUtil.ExpDecay(_doorOpenedness, target, speed, 1f / 60f);
        if (MathUtil.Approximately(_doorOpenedness, target, 1))
            _doorOpenedness = target;
    }

    public void Draw(SpriteBatch spriteBatch, int floorTop)
    {
        DrawLight(spriteBatch, floorTop);

        _elevatorLeftDoorSprite.Draw(spriteBatch, MainGame.Camera.GetParallaxPosition(_doorLeftOrigin + Vector2.UnitX * -_doorOpenedness, Elevator.ParallaxDoors));
        _elevatorRightDoorSprite.Draw(spriteBatch, MainGame.Camera.GetParallaxPosition(_doorRightOrigin + Vector2.UnitX * _doorOpenedness, Elevator.ParallaxDoors));
    }

    public void Dispose()
    {
        MainGame.Coroutines.Stop("elevator_door_open");
        MainGame.Coroutines.Stop("elevator_door_close");

        _audioDoorOpen?.Stop();
        _audioDoorClose?.Stop();

        _audioDoorOpen?.Dispose();
        _audioDoorClose?.Dispose();
    }

    private void DrawLight(SpriteBatch spriteBatch, int floorTop)
    {
        int lightTop = floorTop + 40;

        Vector2 barOnePosition = MainGame.Camera.GetParallaxPosition(new(0, lightTop), Elevator.ParallaxDoors);
        Vector2 barTwoPosition = MainGame.Camera.GetParallaxPosition(new(0, lightTop - 140), Elevator.ParallaxDoors);
        Vector2 blackBarPosition = MainGame.Camera.GetParallaxPosition(new(0, lightTop - 40), Elevator.ParallaxDoors);
        Vector2 blackBarPosition2 = MainGame.Camera.GetParallaxPosition(new(0, lightTop + 100), Elevator.ParallaxDoors);
        spriteBatch.Draw(MainGame.PixelTexture,
            new Rectangle((int)barOnePosition.X, (int)barOnePosition.Y, 240, 100), Color.White * (1 - (_doorOpenedness / 47f)));
        spriteBatch.Draw(MainGame.PixelTexture,
            new Rectangle((int)barTwoPosition.X, (int)barTwoPosition.Y, 240, 100), Color.White * (1 - (_doorOpenedness / 47f)));
        spriteBatch.Draw(MainGame.PixelTexture,
            new Rectangle((int)blackBarPosition.X, (int)blackBarPosition.Y, 240, 40), Color.Black * (1 - (_doorOpenedness / 47f)));
        spriteBatch.Draw(MainGame.PixelTexture,
            new Rectangle((int)blackBarPosition2.X, (int)blackBarPosition2.Y, 240, 40), Color.White * (1 - (_doorOpenedness / 47f)));
    }

    public CoroutineHandle Open(int delay = 0)
    {
        MainGame.Coroutines.Stop("elevator_door_close");
        MainGame.Coroutines.TryRun("elevator_door_open", OpenDoors(), delay, out var handle);
        return handle;
    }

    public CoroutineHandle Close(int delay = 0)
    {
        MainGame.Coroutines.Stop("elevator_door_open");
        MainGame.Coroutines.TryRun("elevator_door_close", CloseDoors(), delay, out var handle);
        return handle;
    }

    private IEnumerator OpenDoors()
    {
        _audioDoorOpen.Start();
        _isOpen = true;
        while(_doorOpenedness < 30)
        {
            yield return null;
        }
    }

    private IEnumerator CloseDoors()
    {
        _audioDoorClose.Start();
        _isOpen = false;
        while(_doorOpenedness > 1)
        {
            yield return null;
        }

        _elevator.SetState(Elevator.ElevatorStates.Moving);
    }
}
