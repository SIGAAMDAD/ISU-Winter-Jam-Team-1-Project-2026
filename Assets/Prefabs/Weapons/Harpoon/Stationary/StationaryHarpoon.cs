using System;
using Game.Common;
using Game.Mobs;
using Game.Player;
using Game.Player.Weapons;
using Godot;

namespace Prefabs {
	/*
	===================================================================================

	StationaryHarpoon

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed partial class StationaryHarpoon : Projectile {
		private static readonly StringName @UseHorizontalAnimationName = "use_horizontal";
		private static readonly StringName @UseDownAnimationName = "use_down";

		private Area2D _area;

		/*
		===============
		OnAnimationFinished
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnAnimationFinished() {
			_area.Reparent( GetTree().Root );
			_area.Show();
			SetPhysicsProcess( true );
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

			SetPhysicsProcess( false );

			_area = GetNode<Area2D>( nameof( Area2D ) );
			_area.Hide();

			switch ( Direction ) {
				case PlayerDirection.North:
					Play( UseDownAnimationName );
					break;
				case PlayerDirection.South:
					Play( UseDownAnimationName );
					break;
				case PlayerDirection.East:
					Play( UseHorizontalAnimationName );
					FlipH = false;
					break;
				case PlayerDirection.West:
					Play( UseHorizontalAnimationName );
					FlipH = true;
					break;
			}
			Connect( SignalName.AnimationFinished, Callable.From( OnAnimationFinished ) );
		}

		/*
		===============
		_PhysicsProcess
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="delta"></param>
		public override void _PhysicsProcess( double delta ) {
			Vector2 inputVelocity = MoveDirection;

			EntityUtils.CalcSpeed( ref _frameVelocity, new Vector2( _resource.Speed, _resource.Speed ), (float)delta, inputVelocity );
			_area.GlobalPosition += _frameVelocity;
		}
	};
};
