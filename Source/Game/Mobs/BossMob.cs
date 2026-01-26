using Game.Systems;
using Godot;
using Nomad.Core.Events;

namespace Game.Mobs {
	/*
	===================================================================================

	BossMob

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public partial class BossMob : MobBase {
		[Export]
		private int _minSpawnWave = 10;

		public IGameEvent<BossHealthChangedEventArgs> HealthChanged => _healthChanged;
		private IGameEvent<BossHealthChangedEventArgs> _healthChanged;

		/*
		===============
		Damage
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="amount"></param>
		public override void Damage( float amount ) {
			base.Damage( amount );

			_healthChanged.Publish( new BossHealthChangedEventArgs( _health ) );
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
			_healthChanged = eventFactory.GetEvent<BossHealthChangedEventArgs>( nameof( BossMob ), nameof( HealthChanged ) );
		}
	};
};
