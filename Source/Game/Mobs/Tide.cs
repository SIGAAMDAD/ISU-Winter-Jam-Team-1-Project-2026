using Game.Player;
using Godot;
using System;

namespace Game.Mobs {
	public sealed partial class Tide : AnimatedSprite2D {
		private Vector2 _velocity = Vector2.Zero;

		public override void _Ready() {
			base._Ready();

			var collisionArea = GetNode<Area2D>( "CollisionBody" );
			collisionArea.Connect( Area2D.SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnBodyEntered ) );
		}

		/*
		===============
		_Process
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="delta"></param>
		public override void _Process( double delta ) {
			base._Process( delta );

			Vector2 targetVelocity = Vector2.Down * 2.15f;
			_velocity += ( targetVelocity - _velocity ) * (float)( 1.0f - Math.Exp( -8.0f * delta ) );
			GlobalPosition += _velocity;
		}

		/*
		===============
		OnBodyEntered
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="bodyRid"></param>
		/// <param name="body"></param>
		/// <param name="bodyShapeIndex"></param>
		/// <param name="localShapeIndex"></param>
		private void OnBodyEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is PlayerManager player ) {
				player.Damage( 20.0f );
				GetParent().CallDeferred( MethodName.RemoveChild, this );
				CallDeferred( MethodName.QueueFree );
			}
		}
	};
};