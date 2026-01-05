using Game.Systems;
using Godot;
using Nomad.Core.Events;

namespace Game.Common {
	public partial class WaveManager : Node {
		public int CurrentWave => _currentWave;
		private int _currentWave = 1;

		public IGameEvent<WaveChangedEventArgs> WaveStarted => _waveStarted;
		private IGameEvent<WaveChangedEventArgs> _waveStarted;

		public IGameEvent<WaveChangedEventArgs> WaveCompleted => _waveCompleted;
		private IGameEvent<WaveChangedEventArgs> _waveCompleted;

		public override void _Ready() {
			base._Ready();

			var eventFactory = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			_waveStarted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveStarted ) );

			// start the wave
			_waveStarted.Publish( new WaveChangedEventArgs( _currentWave, _currentWave ) );

			_waveCompleted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveCompleted ) );
		}
	};
};