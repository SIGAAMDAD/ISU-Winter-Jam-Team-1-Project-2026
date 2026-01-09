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

			Modulate = Colors.Transparent;

			var explosionArea = GetNode<Area2D>( "ExplosionArea" );

			var explosionAnimation = explosionArea.GetNode<AnimatedSprite2D>( "AnimatedSprite2D" );
			explosionAnimation.Connect( AnimatedSprite2D.SignalName.AnimationFinished, Callable.From( OnExplosionFinished ) );
			explosionAnimation.Show();
			explosionAnimation.Play( DefaultAnimationName );

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
	};
};