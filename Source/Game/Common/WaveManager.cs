using Game.Systems;
using Godot;
using Nomad.Core.Events;

namespace Game.Common {
	public partial class WaveManager : Node {
		public int CurrentWave => _currentWave;
		private int _currentWave = 0;

		private IGameEvent<WaveChangedEventArgs> _waveChanged;

		public override void _Ready() {
			base._Ready();

			var eventFactory = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			_waveChanged = eventFactory.GetEvent<WaveChangedEventArgs>( "WaveChanged" );
			_waveChanged.Publish( new WaveChangedEventArgs( 0, 0 ) );
		}
	};
};