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
	
	public partial class Camera2d : Camera2D {
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
			LimitRight += (int)args.IncrementAmount.X;
			LimitBottom += (int)args.IncrementAmount.Y;
		}

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();

			var eventFactory = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			var arenaSizeChanged = eventFactory.GetEvent<ArenaSizeChangedEventArgs>( nameof( WorldArea.ArenaSizeChanged ) );
			arenaSizeChanged.Subscribe( this, OnArenaSizeChanged );
		}
	};
};