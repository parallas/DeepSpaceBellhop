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
    public Guid CharacterId { get; set; } = Guid.NewGuid();
    public CharacterDef Def { get; set; }
    public int FloorNumberCurrent { get; set; }
    public int FloorNumberTarget { get; set; }
    public int Patience { get; set; }
    public int InitialPatience { get; private set; }
    public bool DrawAngryIcon { get; set; }
    public bool CanRandomlyTurnAround { get; set; }

    public int FloorTargetDirection => Math.Sign(FloorNumberTarget - FloorNumberCurrent);

    public int OffsetXTarget { get; set; }
    private float _offsetX;
    private float _offsetY;
    private float _squashStretchOffset = 0;

    private int _seed;
    private AnimatedSprite _currentAnimation;
    private AnimatedSprite _animFront;
    private AnimatedSprite _animBack;
    private AnimatedSprite _animAngry;
    private AnimatedSprite _angryIcon;
    private bool _isInElevator;
    private float _currentWalkSpeed;
    private int _turnAroundCooldown;

    private int _targetDepth;
    private float _renderDepth;
    public const int StandingRoomSize = 85;

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
        _animAngry = spriteSheet.CreateAnimatedSprite("Angry");

        _angryIcon = ContentLoader.Load<AsepriteFile>("graphics/Anger")
            .CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, false)
            .CreateAnimatedSprite("Tag");
        _angryIcon.Origin = Vector2.One * 7 - Def.AngryIconPosition;
        _angryIcon.Play();

        PlayAnimation(_animFront);

        _seed = Random.Shared.Next(500);

        _currentWalkSpeed = Def.WalkSpeed;

        InitialPatience = Patience;
    }

    public void Update(GameTime gameTime)
    {
        if (_turnAroundCooldown > 0)
            _turnAroundCooldown--;
        if (CanRandomlyTurnAround && _turnAroundCooldown <= 0)
        {
            if (Random.Shared.Next(MathUtil.CeilToInt(240 * (1f - (Def.WalkSpeed / 30) + 0.2f))) == 0)
            {
                // Turn around
                _turnAroundCooldown = Random.Shared.Next(60, 300 + 1);

                TurnAround();

                if (_currentAnimation == _animFront)
                    _turnAroundCooldown = 30;
            }
        }

        _currentAnimation.Update(1f/60f);
        _angryIcon.Update(1f/60f);

        _offsetX = MathUtil.ExpDecay(_offsetX, OffsetXTarget, _currentWalkSpeed, 1f / 60f);
        if (MathUtil.Approximately(_offsetX, OffsetXTarget, 1))
            _offsetX = OffsetXTarget;

        _offsetY = MathUtil.ExpDecay(_offsetY, 0, 8, 1f / 60f);
        _squashStretchOffset = MathUtil.ExpDecay(_squashStretchOffset, 0, 8, 1f / 60f);
    }

    public void Draw(SpriteBatch spriteBatch, int index = 0)
    {
        if (!_isInElevator && FloorNumberCurrent != MainGame.CurrentFloor) return;

        var depthInterpolated = MathUtil.InverseLerp01(8, 0, index);
        _targetDepth = MathUtil.FloorToInt(_isInElevator ? (depthInterpolated * Elevator.Elevator.ParallaxWalls - 1) : Elevator.Elevator.ParallaxDoors + 10);
        _renderDepth = MathUtil.ExpDecay(_renderDepth, _targetDepth, 13f, 1f / 60f);

        _currentAnimation.Origin = new Vector2(_currentAnimation.Width * 0.5f, _currentAnimation.Height);

        _currentAnimation.ScaleX = 1 - _squashStretchOffset;
        _currentAnimation.ScaleY = 1 + _squashStretchOffset;

        Vector2 pos = new(
            MainGame.GameBounds.Center.X + _offsetX,
            MainGame.GameBounds.Bottom + 8 + -MathHelper.Max(MathF.Sin((MainGame.Step + _seed) / 60f * 3), 0f) + _offsetY
        );
        pos = Vector2.Round(pos);

        _currentAnimation.Color = Color.Black;
        _currentAnimation.Draw(
            spriteBatch,
            MainGame.Camera.GetParallaxPosition(
                pos + Vector2.One * 2,
                _renderDepth
            )
        );

        if(DrawAngryIcon)
        {
            _angryIcon.Color = Color.Black;
            _angryIcon.Draw(
                spriteBatch,
                MainGame.Camera.GetParallaxPosition(
                    pos + Vector2.One * 2,
                    _renderDepth
                )
            );
        }

        _currentAnimation.Color = Color.White;
        _currentAnimation.Draw(
            spriteBatch,
            MainGame.Camera.GetParallaxPosition(
                pos,
                _renderDepth
            )
        );

        if(DrawAngryIcon)
        {
            _angryIcon.Color = Color.White;
            _angryIcon.Draw(
                spriteBatch,
                MainGame.Camera.GetParallaxPosition(
                    pos,
                    _renderDepth
                )
            );
        }
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
        int entranceRange = 0;
        OffsetXTarget = Math.Clamp(OffsetXTarget, -entranceRange, entranceRange);
        while (!MathUtil.Approximately(_offsetX, 0, entranceRange + 1))
        {
            yield return null;
        }
        _offsetX = OffsetXTarget;

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

    public IEnumerator GetOffElevatorBegin(bool isAngry)
    {
        PlayAnimation(_animBack);

        OffsetXTarget = 0;
        while (MathUtil.RoundToInt(_offsetX) != 0)
        {
            yield return null;
        }

        _squashStretchOffset = -0.1f;
        PlayAnimation(isAngry ? _animAngry : _animFront);
    }

    public IEnumerator GetOffElevatorEnd(Action onEnd)
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
        onEnd?.Invoke();
    }

    public void TurnAround()
    {
        _squashStretchOffset = -0.1f;
        PlayAnimation(_currentAnimation == _animFront ? _animBack : _animFront);
    }
}
