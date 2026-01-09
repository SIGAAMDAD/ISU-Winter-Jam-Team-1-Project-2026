using Game.Common;
using Game.Player.Weapons;
using Game.Systems;
using Godot;
using Nomad.Core.Events;
using System;

namespace Prefabs {
	/*
	===================================================================================
	
	WorldArea
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class WorldArea : Area2D {
		[Export]
		private CollisionPolygon2D _bounds;
		[Export]
		private CollisionShape2D _shape;
		[Export]
		private TextureRect _water;

		private RectangleShape2D _rectangleShape;
		private Vector2[] _polygon;

		private Vector2 _incrementSize;

		public IGameEvent<ArenaSizeChangedEventArgs> ArenaSizeChanged => _arenaSizeChanged;
		private IGameEvent<ArenaSizeChangedEventArgs> _arenaSizeChanged;

		/*
		===============
		OnAreaShapeExited
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="areaRid"></param>
		/// <param name="area"></param>
		/// <param name="areaShapeIndex"></param>
		/// <param name="localShapeIndex"></param>
		private void OnAreaShapeExited( Rid areaRid, Area2D area, int areaShapeIndex, int localShapeIndex ) {
			if ( area is not null && area.GetParent() is Projectile projectile ) {
				// make sure we don't have any memory leaks
				projectile.QueueFree();
			}
		}

		/*
		===============
		OnWaveCompleted
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnWaveCompleted( in WaveChangedEventArgs args ) {
			Vector2 size = _rectangleShape.Size;
			size.X += _incrementSize.X;
			size.Y += _incrementSize.Y;
			_rectangleShape.Size = size;
			_shape.GlobalPosition += size / 2.0f;

			_polygon[ 1 ].X += _incrementSize.X;

			_polygon[ 2 ].X += _incrementSize.X;
			_polygon[ 2 ].Y += _incrementSize.Y;

			_polygon[ 3 ].Y += _incrementSize.Y;

			_arenaSizeChanged.Publish( new ArenaSizeChangedEventArgs( size, _incrementSize ) );
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

			var eventFactory = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			var waveCompleted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager.WaveCompleted ) );
			waveCompleted.Subscribe( this, OnWaveCompleted );

			_arenaSizeChanged = eventFactory.GetEvent<ArenaSizeChangedEventArgs>( nameof( ArenaSizeChanged ) );

			if ( _shape.Shape is RectangleShape2D rect ) {
				_rectangleShape = rect;
			} else {
				throw new InvalidCastException( "The world's collision shape must be a RectangleShape2D!" );
			}

			_polygon = _bounds.Polygon;
			_incrementSize = ( _water.Size - _rectangleShape.Size ) / 20.0f;

			Connect( SignalName.AreaShapeExited, Callable.From<Rid, Area2D, int, int>( OnAreaShapeExited ) );
		}
	};
};