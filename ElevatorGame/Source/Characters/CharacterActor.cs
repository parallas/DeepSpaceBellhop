using System;
using System.Collections;
using AsepriteDotNet.Aseprite;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;

namespace ElevatorGame.Source.Characters;

public class CharacterActor
{
    public CharacterDef Def { get; set; }
    public int FloorNumberCurrent { get; set; }
    public int FloorNumberTarget { get; set; }
    public int Patience { get; set; }

    public int OffsetXTarget { get; set; }
    private float _offsetX;
    private float _offsetY;
    private float _squashStretchOffset = 0;

    private int _seed;
    private AnimatedSprite _currentAnimation;
    private AnimatedSprite _animFront;
    private AnimatedSprite _animBack;
    private bool _isInElevator;
    private float _currentWalkSpeed;

    private const int StandingRoomSize = 85;

    private void PlayAnimation(AnimatedSprite animation)
    {
        if (_currentAnimation == animation) return;

        _currentAnimation?.Stop();
        _currentAnimation = animation;
        _currentAnimation.Play();
    }

    public void LoadContent()
    {
        var spriteFile = ContentLoader.Load<AsepriteFile>(Def.SpritePath)!;
        var spriteSheet =
            spriteFile.CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, false, spacing: 8, innerPadding: 8);
        _animFront = spriteSheet.CreateAnimatedSprite("Front");
        _animBack = spriteSheet.CreateAnimatedSprite("Back");

        PlayAnimation(_animFront);

        _seed = Random.Shared.Next(500);

        _currentWalkSpeed = Def.WalkSpeed;
    }

    public void Update(GameTime gameTime)
    {
        _currentAnimation.Update(1f/60f);

        _offsetX = MathUtil.ExpDecay(_offsetX, OffsetXTarget, _currentWalkSpeed, 1f / 60f);
        if (MathUtil.Approximately(_offsetX, OffsetXTarget, 1))
            _offsetX = OffsetXTarget;

        _offsetY = MathUtil.ExpDecay(_offsetY, 0, 8, 1f / 60f);
        _squashStretchOffset = MathUtil.ExpDecay(_squashStretchOffset, 0, 8, 1f / 60f);
    }

    public void Draw(SpriteBatch spriteBatch, int index = 0)
    {
        if (!_isInElevator && FloorNumberCurrent != MainGame.CurrentFloor) return;

        var depthInterpolated = MathUtil.InverseLerp01(0, 8, index);
        var depth = _isInElevator ? (depthInterpolated * Elevator.Elevator.ParallaxWalls - 1) : Elevator.Elevator.ParallaxDoors + 10;

        _currentAnimation.Origin = new Vector2(_currentAnimation.Width * 0.5f, _currentAnimation.Height);

        _currentAnimation.ScaleX = 1 - _squashStretchOffset;
        _currentAnimation.ScaleY = 1 + _squashStretchOffset;

        Vector2 pos = new Vector2(
            MainGame.GameBounds.Center.X + _offsetX,
            MainGame.GameBounds.Bottom + 5 + -MathHelper.Max(MathF.Sin((MainGame.Frame + _seed) / 60f * 3), 0f) + _offsetY
        );
        pos = Vector2.Round(pos);

        _currentAnimation.Color = Color.Black;
        _currentAnimation.Draw(
            MainGame.SpriteBatch,
            MainGame.Camera.GetParallaxPosition(
                pos + Vector2.One * 2,
                depth
            )
        );

        _currentAnimation.Color = Color.White;
        _currentAnimation.Draw(
            MainGame.SpriteBatch,
            MainGame.Camera.GetParallaxPosition(
                pos,
                depth
            )
        );
    }

    public void MoveOutOfTheWay()
    {
        int newTarget = OffsetXTarget;
        while (MathUtil.Approximately(newTarget, 0, 24))
        {
            newTarget = Random.Shared.Next(-StandingRoomSize, StandingRoomSize + 1);
        }
        OffsetXTarget = newTarget;
    }

    public IEnumerator GetInElevatorBegin()
    {
        OffsetXTarget = 0;
        while (MathUtil.RoundToInt(_offsetX) != 0)
        {
            yield return null;
        }

        _isInElevator = true;
    }

    public IEnumerator GetInElevatorEnd()
    {
        int newX = Random.Shared.Next(-StandingRoomSize, StandingRoomSize + 1);
        OffsetXTarget = newX;
        while (MathUtil.RoundToInt(_offsetX) != OffsetXTarget)
        {
            yield return null;
        }

        // _offsetY = 4;
        _squashStretchOffset = -0.1f;

        PlayAnimation(_animBack);
    }

    public IEnumerator GetOffElevatorBegin()
    {
        OffsetXTarget = 0;
        while (MathUtil.RoundToInt(_offsetX) != 0)
        {
            yield return null;
        }

        _squashStretchOffset = -0.1f;
        PlayAnimation(_animFront);
    }

    public IEnumerator GetOffElevatorEnd()
    {
        _currentWalkSpeed = MathUtil.CeilToInt(Def.WalkSpeed * 0.5f);
        _squashStretchOffset = -0.1f;
        PlayAnimation(_animBack);
        int newX = MathUtil.FloorToInt((Random.Shared.Next(2) - 0.5f) * 2 * 85);
        OffsetXTarget = newX;
        while (MathUtil.RoundToInt(_offsetX) != OffsetXTarget)
        {
            yield return null;
        }
        _isInElevator = false;
    }
}
