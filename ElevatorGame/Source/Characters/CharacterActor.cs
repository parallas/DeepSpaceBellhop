using System;
using AsepriteDotNet.Aseprite;
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
    public int OffsetX { get; set; }

    private int _seed;
    private AnimatedSprite _currentAnimation;
    private AnimatedSprite _animFront;
    private AnimatedSprite _animBack;
    private bool _isInElevator;
    
    public void LoadContent()
    {
        var spriteFile = ContentLoader.Load<AsepriteFile>(Def.SpritePath)!;
        var spriteSheet = spriteFile.CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, false);
        _animFront = spriteSheet.CreateAnimatedSprite("Front");
        _animBack = spriteSheet.CreateAnimatedSprite("Back");

        _currentAnimation = _animFront;
        _currentAnimation.Play();

        _seed = Random.Shared.Next(500);
    }
    
    public void Update(GameTime gameTime)
    {
        _currentAnimation.Update(1f/60f);
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        if (!_isInElevator && FloorNumberCurrent != MainGame.CurrentFloor) return;
        
        var depth = _isInElevator ? 0 : Elevator.Elevator.ParallaxDoors + 10;
        
        _currentAnimation.Origin = new Vector2(_currentAnimation.Width * 0.5f, _currentAnimation.Height);

        Vector2 pos = new Vector2(
            MainGame.GameBounds.Center.X + OffsetX, 
            MainGame.GameBounds.Bottom + 5 + -MathHelper.Max(MathF.Sin((MainGame.Frame + _seed) / 60f * 3), 0f)
        );
        
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
}