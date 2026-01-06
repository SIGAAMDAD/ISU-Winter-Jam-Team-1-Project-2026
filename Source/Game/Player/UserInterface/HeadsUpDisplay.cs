using Game.Systems;
using Godot;
using Nomad.Core.Events;

namespace Game.Player.UserInterface {
	public partial class HeadsUpDisplay : CanvasLayer {
		private HealthBar _healthBar;
		private WaveCounter _waveCounter;
		private AnnouncementLabel _announcementLabel;

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();

			var eventFactory = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			_healthBar = new HealthBar( GetNode<ProgressBar>( "HealthBar" ), eventFactory );
			_waveCounter = new WaveCounter( GetNode<Label>( "WaveCounter" ), eventFactory );
			_announcementLabel = new AnnouncementLabel( GetNode<Label>( "AnnouncementLabel" ), eventFactory );
		}
	};
};
