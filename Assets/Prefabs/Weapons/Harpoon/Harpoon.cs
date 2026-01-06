using Game.Common;
using Game.Mobs;
using Game.Player;
using Godot;

namespace Prefabs {
	/*
	===================================================================================
	
	Harpoon
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	/// 
	/// NOTE: might want to make a factory for this thing
	
	public partial class Harpoon : Sprite2D {
		public float Damage = 10.0f;
		public float Speed = 30.0f;
		public PlayerDirection Direction = PlayerDirection.North;

		private Vector2 _frameVelocity = Vector2.Zero;

		/*
		===============
		OnBodyHit
		===============
		*/
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
		public override void _Process( double delta ) {
			base._Process( delta );

			Vector2 inputVelocity = Direction switch {
				PlayerDirection.North => Vector2.Up,
				PlayerDirection.East => Vector2.Right,
				PlayerDirection.West => Vector2.Left,
				PlayerDirection.South => Vector2.Down
			};

			EntityUtils.CalcSpeed( ref _frameVelocity, Speed, (float)delta, inputVelocity );
			GlobalPosition += _frameVelocity;
		}
	};
};
