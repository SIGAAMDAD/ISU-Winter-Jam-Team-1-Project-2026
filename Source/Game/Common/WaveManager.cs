using Game.Systems;
using Godot;
using Nomad.Core.Events;

namespace Game.Common {
	public partial class WaveManager : Node {
		public int CurrentWave => _currentWave;
		private int _currentWave = 1;

		private IGameEvent<WaveChangedEventArgs> _waveChanged;

		public override void _Ready() {
			base._Ready();

			var eventFactory = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			_waveChanged = eventFactory.GetEvent<WaveChangedEventArgs>( "WaveChanged" );

			// start the wave
			_waveChanged.Publish( new WaveChangedEventArgs( _currentWave, _currentWave ) );
		}
	};
};