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

	public sealed partial class PlayerManager : CharacterBody2D {
		private PlayerStats _stats;
		private PlayerMovementController _movementController;
		private PlayerAttackController _attackController;
		private PlayerAnimator _animator;
		private PlayerAudioPlayer _audioPlayer;

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

			var serviceRegistry = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServicesFactory;
			var eventFactory = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			_animator = new PlayerAnimator( this );
			_movementController = new PlayerMovementController( this, _animator );
			_attackController = new PlayerAttackController( this, _animator, eventFactory );
			_audioPlayer = new PlayerAudioPlayer( this, _attackController, _animator, eventFactory );

			_stats = new PlayerStats( this, eventFactory );
			serviceRegistry.RegisterSingleton<IPlayerStatsProvider>( _stats );
		}

		/*
		===============
		_Process
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="delta"></param>
		public override void _Process( double delta ) {
			base._Process( delta );

			float fixedDelta = (float)delta;
			_movementController.Update( fixedDelta );
		}

		/*
		===============
		_PhysicsProcess
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="delta"></param>
		public override void _PhysicsProcess( double delta ) {
			base._PhysicsProcess( delta );

			float fixedDelta = (float)delta;
			_movementController.FixedUpdate( fixedDelta, out bool inputWasActive );
			_attackController.FixedUpdate( fixedDelta );
			_animator.FixedUpdate( fixedDelta, inputWasActive );
		}
	};
};
