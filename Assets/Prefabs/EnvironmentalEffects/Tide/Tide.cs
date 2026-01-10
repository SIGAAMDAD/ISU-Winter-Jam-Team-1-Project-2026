using Game.Player;
using Game.Systems;
using Godot;
using Nomad.Core.Events;
using System;

namespace Game.Mobs {
	/*
	===================================================================================
	
	Tide
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed partial class Tide : EffectBase {
		private Vector2 _velocity = Vector2.Zero;
		private IGameEvent<PlayerTakeDamageEventArgs> _damagePlayer;

		/*
		===============
		OnPlayerEntered
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="player"></param>
		protected override void OnPlayerEntered( PlayerManager player ) {
			_damagePlayer.Publish( new PlayerTakeDamageEventArgs( 25.0f ) );
			// TODO: add particle effect
			_effectFinished.Publish( _effectId );
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
			_damagePlayer = eventFactory.GetEvent<PlayerTakeDamageEventArgs>( nameof( PlayerStats.TakeDamage ) );
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
		public override void _PhysicsProcess( double delta ) {
			base._PhysicsProcess( delta );

			Vector2 targetVelocity = Vector2.Down * 2.15f;
			_velocity += ( targetVelocity - _velocity ) * (float)( 1.0f - Math.Exp( -8.0f * delta ) );
			GlobalPosition += _velocity;
		}
	};
};