using Game.Mobs;
using Godot;

namespace Prefabs {
	/*
	===================================================================================
	
	Squid
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class Squid : MobBase {
		private static readonly StringName @GrabAnimationName = "grab";
		private static readonly StringName @ReleaseAnimationName = "release";

		private AnimatedSprite2D _grabPlayer;

		private ProgressBar _healthBar;

		/*
		===============
		Damage
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="amount"></param>
		public override void Damage( float amount ) {
			base.Damage( amount );

			_healthBar.Value = _health;
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

			_healthBar = GetNode<ProgressBar>( "HealthBar" );
			_healthBar.MaxValue = _health;

			_grabPlayer = GetNode<AnimatedSprite2D>( "GrabbedPlayer" );
		}
	};
};
