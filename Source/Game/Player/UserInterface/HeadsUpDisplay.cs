using Game.Systems;
using Godot;
using Nomad.Core.Events;

namespace Game.Player.UserInterface {
	public partial class HeadsUpDisplay : CanvasLayer {
		private HealthBar _healthBar;
		private WaveCounter _waveCounter;

		private ProgressBar _weaponCooldownMeter;

		/*
		===============
		OnWeaponCooldownFinished
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnWeaponCooldownFinished( in EmptyEventArgs args ) {
		}

		/*
		===============
		OnWeaponCooldownTimeChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnWeaponCooldownTimeChanged( in WeaponCooldownTimeChangedEventArgs args ) {
			_weaponCooldownMeter.Value = args.Progress;
		}

		/*
		===============
		OnUseWeapon
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnUseWeapon( in EmptyEventArgs args ) {
			_weaponCooldownMeter.Value = 0.0f;
		}

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

			_weaponCooldownMeter = GetNode<ProgressBar>( "WeaponCooldownMeter" );

			var weaponCooldownFinished = eventFactory.GetEvent<EmptyEventArgs>( nameof( PlayerController.WeaponCooldownFinished ) );
			weaponCooldownFinished.Subscribe( this, OnWeaponCooldownFinished );

			var weaponCooldownTimeChanged = eventFactory.GetEvent<WeaponCooldownTimeChangedEventArgs>( nameof( PlayerController.WeaponCooldownTimeChanged ) );
			weaponCooldownTimeChanged.Subscribe( this, OnWeaponCooldownTimeChanged );

			var useWeapon = eventFactory.GetEvent<EmptyEventArgs>( nameof( PlayerController.UseWeapon ) );
			useWeapon.Subscribe( this, OnUseWeapon );
		}
	};
};
