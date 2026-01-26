using Game.Mobs;
using Godot;

namespace Prefabs {
	/*
	===================================================================================

	Seagull

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public partial class Seagull : MobBase {
		private static readonly NodePath @GlobalPositionNodePath = "global_position";

		/*
		===============
		OnAnimationChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnAnimationChanged() {
			if ( _animation.Animation == DieAnimationName ) {
				// make them fall a little bit
				CreateTween().TweenProperty( this, GlobalPositionNodePath, new Vector2( GlobalPosition.X, GlobalPosition.Y + 100.0f ), 0.5f );
			}
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

			_animation.Connect( AnimatedSprite2D.SignalName.AnimationChanged, Callable.From( OnAnimationChanged ) );
		}
	};
};
