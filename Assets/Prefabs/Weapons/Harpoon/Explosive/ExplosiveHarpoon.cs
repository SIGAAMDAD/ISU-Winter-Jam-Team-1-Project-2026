using Game.Mobs;
using Game.Player.Weapons;
using Godot;

namespace Prefabs {
	/*
	===================================================================================
	
	ExplosiveHarpoon
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class ExplosiveHarpoon : Projectile {
		private static readonly StringName @DefaultAnimationName = "default";

		private AnimatedSprite2D _explosionAnimation;

		private const float EXPLOSION_DAMAGE = 20.0f;

		/*
		===============
		OnEnemyHit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="mob"></param>
		protected override void OnEnemyHit( MobBase mob ) {
			base.OnEnemyHit( mob );

			CallDeferred( MethodName.SetPhysicsProcess, false );

			var explosionArea = GetNode<Area2D>( "ExplosionArea" );

			var streamPlayer = GetNode<AudioStreamPlayer2D>( nameof( AudioStreamPlayer2D ) );
			streamPlayer.CallDeferred( AudioStreamPlayer2D.MethodName.Play );

			_explosionAnimation.SetDeferred( PropertyName.Visible, true );
			_explosionAnimation.CallDeferred( AnimatedSprite2D.MethodName.Play, DefaultAnimationName );

			Godot.Collections.Array<Area2D> bodies = explosionArea.GetOverlappingAreas();
			for ( int i = 0; i < bodies.Count; i++ ) {
				if ( bodies[ i ] is MobBase enemy ) {
					enemy.Damage( EXPLOSION_DAMAGE );
				}
			}
		}

		/*
		===============
		OnExplosionFinished
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnExplosionFinished() {
			QueueFree();
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public override void _Ready() {
			base._Ready();

			_explosionAnimation = GetNode<AnimatedSprite2D>( "ExplosionArea/AnimatedSprite2D" );
			_explosionAnimation.Connect( AnimatedSprite2D.SignalName.AnimationFinished, Callable.From( OnExplosionFinished ) );
		}
	};
};