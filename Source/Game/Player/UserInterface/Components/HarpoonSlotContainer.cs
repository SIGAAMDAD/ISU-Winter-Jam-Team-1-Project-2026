using Game.Player.Upgrades;
using Game.Player.Weapons;
using Game.Systems;
using Godot;
using Nomad.Core.Events;

namespace Game.Player.UserInterface.Components {
	/*
	===================================================================================

	HarpoonSlotContainer

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public partial class HarpoonSlotContainer : HBoxContainer {
		[Export]
		private Texture2D _icon;
		[Export]
		private HarpoonType _type;
		[Export]
		private ProjectileResource _resource;

		private ProgressBar _progressBar;

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

			var harpoonCooldownChanged = eventFactory.GetEvent<HarpoonCooldownChangedEventArgs>( nameof( PlayerAttackController ), nameof( PlayerAttackController.HarpoonCooldownChanged ) );
			harpoonCooldownChanged.Subscribe( this, OnHarpoonCooldownChanged );

			var statChanged = eventFactory.GetEvent<StatChangedEventArgs>( nameof( PlayerStats ), nameof( PlayerStats.StatChanged ) );
			statChanged.Subscribe( this, OnStatChanged );

			_progressBar = GetNode<ProgressBar>( nameof( ProgressBar ) );
			_progressBar.MaxValue = _resource.CooldownTime;

			var icon = GetNode<TextureRect>( "Icon" );
			icon.Texture = _icon;
		}

		/*
		===============
		OnStatChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnStatChanged( in StatChangedEventArgs args ) {
			if ( args.StatId == PlayerStats.ATTACK_SPEED ) {
				_progressBar.MaxValue = args.Value * _resource.CooldownTime;
			}
		}

		/*
		===============
		OnHarpoonCooldownChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnHarpoonCooldownChanged( in HarpoonCooldownChangedEventArgs args ) {
			if ( _type == args.Type ) {
				_progressBar.Value = args.Progress;
			}
		}
	};
};
