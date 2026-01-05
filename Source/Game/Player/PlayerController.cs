using Godot;
using System;

namespace Game.Player {
	/*
	===================================================================================
	
	PlayerController
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class PlayerController {
//		private readonly Dictionary<Key, Action> _bindings = new Dictionary<Key, Action>();
		private readonly PlayerStats _stats;
		private readonly PlayerManager _owner;

		private Vector2 _frameVelocity = Vector2.Zero;

		/*
		===============
		PlayerController
		===============
		*/
		public PlayerController( PlayerManager owner, PlayerStats stats ) {
			_stats = stats;
			_owner = owner;
		}

		/*
		===============
		Update
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="delta"></param>
		/// <param name="inputWasActive"></param>
		public void Update( float delta, out bool inputWasActive ) {
			Vector2 inputVelocity = Input.GetVector( "move_west", "move_east", "move_north", "move_south" );
			inputWasActive = inputVelocity != Vector2.Zero;

			Vector2 targetVelocity = inputVelocity * _stats.Speed;

			_frameVelocity += ( targetVelocity - _frameVelocity ) * (float)( 1.0f - Math.Exp( -8.0f * delta ) );
			_owner.Velocity = _frameVelocity;
			_owner.MoveAndSlide();
			_frameVelocity = _owner.Velocity;
		}
	};
};