using Game.Common;
using Game.Mobs;
using Godot;

namespace Game.Player.Weapons {
	/*
	===================================================================================
	
	Projectile
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class Projectile : Sprite2D {
		public float Damage = 10.0f;
		public float Speed = 30.0f;
		public PlayerDirection Direction = PlayerDirection.North;

		private Vector2 _frameVelocity = Vector2.Zero;

		/*
		===============
		OnBodyHit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="bodyRid"></param>
		/// <param name="body"></param>
		/// <param name="bodyShapeIndex"></param>
		/// <param name="localShapeIndex"></param>
		private void OnBodyHit( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is MobBase mob ) {
				mob.Damage( Damage );
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

			var collisionArea = GetNode<Area2D>( "Area2D" );
			collisionArea.Connect( Area2D.SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnBodyHit ) );
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
		public override void _PhysicsProcess( double delta ) {
			base._PhysicsProcess( delta );

			Vector2 inputVelocity = Vector2.Right.Rotated( GlobalRotation );

			EntityUtils.CalcSpeed( ref _frameVelocity, Speed,(float)delta, inputVelocity );
			GlobalPosition += _frameVelocity;
		}
	};
};