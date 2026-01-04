using Godot;
using System;
using System.Collections.Generic;

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
		private readonly Player _owner;
		private readonly PlayerWaterWake _waterWake;

		private Vector2 _frameVelocity = Vector2.Zero;

		/*
		===============
		PlayerController
		===============
		*/
		public PlayerController( Player owner, PlayerStats stats ) {
			_stats = stats;
			_owner = owner;
			_waterWake = new PlayerWaterWake( owner );
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
		public void Update( float delta ) {
			Vector2 inputVelocity = Input.GetVector( "move_west", "move_east", "move_north", "move_south" );
			Vector2 targetVelocity = inputVelocity * _stats.Speed;

			_frameVelocity += ( targetVelocity - _frameVelocity ) * (float)( 1.0f - Math.Exp( -8.0f * delta ) );
			_owner.Velocity = _frameVelocity;
			_waterWake.Update( delta );
			_owner.MoveAndSlide();
			_frameVelocity = _owner.Velocity;
		}
	};
};