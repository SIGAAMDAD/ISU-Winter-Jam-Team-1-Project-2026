using Godot;
using Nomad.Core.Events;
using Prefabs;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Game.Mobs {
	/*
	===================================================================================

	SpatialPartition

	===================================================================================
	*/
	/// <summary>
	/// Spawns mobs spaced out as to improve performance (reduces the chance of piling).
	/// </summary>

	public sealed class SpatialPartition {
		private Vector2 _worldSize;
		private readonly float _cellSize;
		private readonly int _gridWidth;
		private readonly int _gridHeight;

		private readonly Dictionary<Vector2I, List<Node2D>> _grid = new();
		private readonly Dictionary<Node2D, Vector2I> _mobToCell = new();

		/*
		===============
		SpatialPartition
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="worldSize"></param>
		/// <param name="minDistance"></param>
		public SpatialPartition( Vector2 worldSize, float minDistance, IGameEventRegistryService eventFactory ) {
			_worldSize = worldSize;
			_cellSize = minDistance; // Slightly larger than min distance
			_gridWidth = Mathf.CeilToInt( worldSize.X / _cellSize );
			_gridHeight = Mathf.CeilToInt( worldSize.Y / _cellSize );

			var arenaSizeChanged = eventFactory.GetEvent<ArenaSizeChangedEventArgs>( nameof( WorldArea ), nameof( WorldArea.ArenaSizeChanged ) );
			arenaSizeChanged.Subscribe( this, OnArenaSizeChanged );
		}

		/*
		===============
		IsPositionAvailable
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="worldPosition"></param>
		/// <param name="minDistance"></param>
		/// <param name="spawnerPosition"></param>
		/// <returns></returns>
		public bool IsPositionAvailable( Vector2 worldPosition, float minDistance, Vector2 spawnerPosition = default ) {
			// Convert to grid coordinates relative to spawner
			Vector2I cell = WorldToCell( worldPosition, spawnerPosition );

			// Check 3x3 area around the cell
			for ( int dx = -1; dx <= 1; dx++ ) {
				for ( int dy = -1; dy <= 1; dy++ ) {
					Vector2I checkCell = new Vector2I( cell.X + dx, cell.Y + dy );
					if ( _grid.TryGetValue( checkCell, out var mobsInCell ) ) {
						foreach ( var mob in mobsInCell ) {
							if ( mob.GlobalPosition.DistanceTo( worldPosition ) < minDistance ) {
								return false;
							}
						}
					}
				}
			}

			return true;
		}

		/*
		===============
		Add
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="mob"></param>
		/// <param name="position"></param>
		/// <param name="spawnerPosition"></param>
		public void Add( Node2D mob, Vector2 position, Vector2 spawnerPosition = default ) {
			Vector2I cell = WorldToCell( position, spawnerPosition );

			if ( !_grid.ContainsKey( cell ) ) {
				_grid[ cell ] = new List<Node2D>( 1024 );
			}

			_grid[ cell ].Add( mob );
			_mobToCell[ mob ] = cell;
		}

		/*
		===============
		Remove
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="mob"></param>
		public void Remove( Node2D mob ) {
			if ( _mobToCell.TryGetValue( mob, out var cell ) ) {
				if ( _grid.TryGetValue( cell, out var mobsInCell ) ) {
					mobsInCell.Remove( mob );
					if ( mobsInCell.Count == 0 ) {
						_grid.Remove( cell );
					}
				}

				_mobToCell.Remove( mob );
			}
		}

		/*
		===============
		Clear
		===============
		*/
		/// <summary>
		///
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Clear() {
			_grid.Clear();
			_mobToCell.Clear();
		}

		/*
		===============
		GetOccupiedCellCount
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public int GetOccupiedCellCount() {
			return _grid.Count;
		}

		/*
		===============
		GetTotalMobCount
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public int GetTotalMobCount() {
			int count = 0;
			foreach ( var cell in _grid.Values ) {
				count += cell.Count;
			}
			return count;
		}

		/*
		===============
		OnArenaSizeChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnArenaSizeChanged( in ArenaSizeChangedEventArgs args ) {
			_worldSize = args.Size;
		}

		/*
		===============
		WorldToCell
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="worldPosition"></param>
		/// <param name="spawnerPosition"></param>
		/// <returns></returns>
		private Vector2I WorldToCell( Vector2 worldPosition, Vector2 spawnerPosition ) {
			// Calculate local position relative to spawner
			Vector2 localPos = worldPosition - spawnerPosition + _worldSize / 2;

			int x = Math.Clamp( Mathf.FloorToInt( localPos.X / _cellSize ), 0, _gridWidth - 1 );
			int y = Math.Clamp( Mathf.FloorToInt( localPos.Y / _cellSize ), 0, _gridHeight - 1 );

			return new Vector2I( x, y );
		}
	}
};
