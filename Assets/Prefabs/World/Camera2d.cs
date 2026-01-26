using Game.Systems;
using Godot;
using Nomad.Core.Events;

namespace Prefabs {
	/*
	===================================================================================

	Camera2d

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed partial class Camera2d : Camera2D {
		public void Shake( float duration, float intensity ) {
		}

		/*
		===============
		OnArenaChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnArenaSizeChanged( in ArenaSizeChangedEventArgs args ) {
			LimitRight = (int)args.Size.X;
			LimitBottom = (int)args.Size.Y;
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
		}
	};
};
