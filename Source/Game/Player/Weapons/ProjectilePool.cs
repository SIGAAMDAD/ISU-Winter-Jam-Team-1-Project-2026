using Game.Common;
using Godot;

namespace Game.Player.Weapons {
#if false
	/*
	===================================================================================

	ProjectilePool

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed partial class ProjectilePool : Node {
		private const int MAX_ACTIVE_PROJECTILES = 512;

		private readonly ProjectileNode[] _projectileNodes = new ProjectileNode[ MAX_ACTIVE_PROJECTILES ];
		private readonly Projectile[] _projectiles = new Projectile[ MAX_ACTIVE_PROJECTILES ];
		private readonly Vector2[] _positions = new Vector2[ MAX_ACTIVE_PROJECTILES ];
		private readonly Vector2[] _velocities = new Vector2[ MAX_ACTIVE_PROJECTILES ];
		private readonly Vector2[] _directions = new Vector2[ MAX_ACTIVE_PROJECTILES ];
		private readonly float[] _speeds = new float[ MAX_ACTIVE_PROJECTILES ];
		private readonly bool[] _active = new bool[ MAX_ACTIVE_PROJECTILES ];
		private readonly int[] _freeIndices = new int[ MAX_ACTIVE_PROJECTILES ];
		private int _projectileCount = 0;
		private int _freeIndexCount = 0;

		/*
		===============
		ProjectilePool
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public ProjectilePool() {

		}

		/*
		===============
		CreateProjectile
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="projectile"></param>
		/// <param name="position"></param>
		/// <param name="direction"></param>
		/// <param name="angle"></param>
		public void CreateProjectile( ProjectileResource projectile, Vector2 position, PlayerDirection direction, float angle ) {
			int index;
			if ( _freeIndexCount > 0 ) {
				index = _freeIndices[ --_freeIndexCount ];
			} else if ( _projectileCount < MAX_ACTIVE_PROJECTILES ) {
				index = _projectileCount++;
			} else {
				return;
			}

			Vector2 dirVector = direction switch {
				PlayerDirection.North => Vector2.Up,
				PlayerDirection.East => Vector2.Left,
				PlayerDirection.West => Vector2.Right,
				PlayerDirection.South => Vector2.Down
			};

			Projectile data = _projectiles[ index ];
			_positions[ index ] = position;
			_velocities[ index ] = Vector2.Zero;
			_directions[ index ] = dirVector;
			_speeds[ index ] = projectile.Speed;
			_active[ index ] = true;

			float angleDeg = angle;
			switch ( direction ) {
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
			data.Animation.RotationDegrees = angleDeg;
			data.Animation.GlobalPosition = position;
			data.Animation.Visible = true;
			data.Animation.GlobalRotation = angle;
		}

		/*
		===============
		RemoveProjectile
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="index"></param>
		public void RemoveProjectile( int index ) {
			if ( index < 0 || index >= _projectileCount || _active[ index ] ) {
				return;
			}

			_active[ index ] = false;
			_projectiles[ index ].Animation.Visible = false;

			_freeIndices[ _freeIndexCount++ ] = index;
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

			for ( int i = 0; i < MAX_ACTIVE_PROJECTILES; i++ ) {
				_projectileNodes[ i ] = new ProjectileNode();
//				_projectiles[ i ] = new Projectile( _projectileNodes[ i ] );
				AddChild( _projectileNodes[ i ] );
			}
		}

		/*
		===============
		_PhysicsProcess
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public override void _PhysicsProcess( double delta ) {
			int count = _projectileCount;
			float fixedDelta = (float)delta;

			for ( int i = 0; i < count; i++ ) {
				if ( !_active[ i ] ) {
					continue;
				}

				ref Vector2 velocity = ref _velocities[ i ];
				ref Vector2 direction = ref _directions[ i ];
				float speed = _speeds[ i ];

				velocity = direction * speed;
				ref Vector2 position = ref _positions[ i ];
				position += velocity * fixedDelta;

				_projectiles[ i ].Animation.GlobalPosition += position;
			}
		}
	};
#endif
};
