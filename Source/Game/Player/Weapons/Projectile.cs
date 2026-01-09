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
		public float Piercing = 0.0f;
		public PlayerDirection Direction = PlayerDirection.North;

		private Vector2 _frameVelocity = Vector2.Zero;

		/*
		===============
		OnEnemyHit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="mob"></param>
		protected virtual void OnEnemyHit( MobBase mob ) {
		}

		/*
		===============
		OnBodyHit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="areaRid"></param>
		/// <param name="area"></param>
		/// <param name="bodyShapeIndex"></param>
		/// <param name="localShapeIndex"></param>
		private void OnAreaShapeEntered( Rid areaRid, Area2D area, int bodyShapeIndex, int localShapeIndex ) {
			if ( area is MobBase mob ) {
				mob.Damage( Damage );

				OnEnemyHit( mob );
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
			collisionArea.Connect( Area2D.SignalName.AreaShapeEntered, Callable.From<Rid, Area2D, int, int>( OnAreaShapeEntered ) );
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

			EntityUtils.CalcSpeed( ref _frameVelocity, new Vector2( Speed, Speed ),(float)delta, inputVelocity );
			GlobalPosition += _frameVelocity;
		}
	};
};