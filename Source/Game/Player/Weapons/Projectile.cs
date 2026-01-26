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

	public partial class Projectile : AnimatedSprite2D {
		[Export]
		protected ProjectileResource _resource;

		public float CooldownTime => _resource.CooldownTime;

		public float DamageScale = 1.0f;
		public PlayerDirection Direction;
		public Vector2 MoveDirection;

		protected Vector2 _frameVelocity = Vector2.Zero;

		private readonly AudioStreamPlayer2D _audioStream = new AudioStreamPlayer2D() {
			Name = nameof( AudioStreamPlayer2D ),
		};

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
				mob.Damage( _resource.Damage * DamageScale );

				OnEnemyHit( mob );
			}
		}

		/*
		===============
		OnAudioStreamFinished
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnAudioStreamFinished() {
			if ( Visible ) {
				_audioStream.Play();
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

			float angleDeg = RotationDegrees;
			switch ( Direction ) {
				case PlayerDirection.South:
					angleDeg -= 180.0f;
					break;
				case PlayerDirection.West:
					angleDeg += 90.0f;
					break;
				case PlayerDirection.East:
					angleDeg -= 90.0f;
					break;
			}
			RotationDegrees = angleDeg;

			var collisionArea = GetNode<Area2D>( "Area2D" );
			collisionArea.Connect( Area2D.SignalName.AreaShapeEntered, Callable.From<Rid, Area2D, int, int>( OnAreaShapeEntered ) );

			_audioStream.Stream = _resource.FlySound;
			_audioStream.Connect( AudioStreamPlayer2D.SignalName.Finished, Callable.From( OnAudioStreamFinished ) );
			AddChild( _audioStream );
			_audioStream.Play();
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

			Vector2 inputVelocity = MoveDirection;

			EntityUtils.CalcSpeed( ref _frameVelocity, new Vector2( _resource.Speed, _resource.Speed ), (float)delta, inputVelocity );
			GlobalPosition += _frameVelocity;
		}
	};
};
