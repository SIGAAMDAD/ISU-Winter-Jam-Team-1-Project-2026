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
		private void OnAnimationChanged() {
		}

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();

			_animation.Connect( AnimatedSprite2D.SignalName.AnimationChanged, Callable.From( OnAnimationChanged ) );
		}
	};
};
