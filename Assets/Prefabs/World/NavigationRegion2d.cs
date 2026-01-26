using Game.Systems;
using Godot;
using Nomad.Core.Events;

namespace Prefabs {
	/*
	===================================================================================

	NavigationRegion2d

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public partial class NavigationRegion2d : NavigationRegion2D {
		private Vector2[] _vertices;

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
			_vertices[ 1 ].X += args.IncrementAmount.X;

			_vertices[ 2 ].X += args.IncrementAmount.X;
			_vertices[ 2 ].Y += args.IncrementAmount.Y;

			_vertices[ 3 ].Y += args.IncrementAmount.Y;

			BakeNavigationPolygon( true );
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
			var arenaSizeChanged = eventFactory.GetEvent<ArenaSizeChangedEventArgs>( nameof( WorldArea ), nameof( WorldArea.ArenaSizeChanged ) );
			arenaSizeChanged.Subscribe( this, OnArenaSizeChanged );

			_vertices = NavigationPolygon.GetVertices();
		}
	};
};
