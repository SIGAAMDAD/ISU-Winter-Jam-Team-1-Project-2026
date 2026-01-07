using Game.Common;
using Game.Player.Upgrades;
using Game.Systems;
using Godot;
using Nomad.Core.Events;

namespace Game.Player {
	/*
	===================================================================================
	
	PlayerManager
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class PlayerManager : CharacterBody2D {
		public IGameEvent<EntityTakeDamageEventArgs> TakeDamage => _stats.TakeDamage;
		public IGameEvent<StatChangedEventArgs> StatChanged => _stats.StatChanged;

		private PlayerStats _stats;
		private PlayerController _controller;
		private PlayerAnimator _animator;
		private PlayerAudioPlayer _audioPlayer;

		/*
		===============
		Damage
		===============
		*/
		public void Damage( float damage ) {
			_stats.Damage( damage * _stats.DamageResistance );
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

			_stats = new PlayerStats( this, GetNode<UpgradeManager>( "/root/World/CanvasLayer/UpgradeManager" ), GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>() );
			_animator = new PlayerAnimator( this );
			_controller = new PlayerController( this, _animator, _stats );
			_audioPlayer = new PlayerAudioPlayer( this, _controller, _animator );
		}

		/*
		===============
		_Process
		===============
		*/
		public override void _Process( double delta ) {
			base._Process( delta );

			float _delta = (float)delta;
			_controller.Update( _delta, out bool inputWasActive );
			_animator.Update( _delta, inputWasActive );
		}
	};
};