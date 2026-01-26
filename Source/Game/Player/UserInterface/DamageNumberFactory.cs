using Game.Common;
using Game.Systems;
using Godot;
using Nomad.Core.Events;
using Nomad.Core.Memory;

namespace Game.Player.UserInterface {
	/*
	===================================================================================

	DamageNumberFactory

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed partial class DamageNumberFactory : Node {
		private readonly BasicObjectPool<DamageNumberLabel> _pool;

		/*
		===============
		DamageNumberFactory
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public DamageNumberFactory() {
			Name = nameof( DamageNumberFactory );
			_pool = new BasicObjectPool<DamageNumberLabel>( CreateLabel, 512 );
		}

		/*
		===============
		Add
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="position"></param>
		/// <param name="value"></param>
		public void Add( Vector2 position, float value ) {
			var label = _pool.Rent();

			label.Show( value, position );
		}

		/*
		===============
		OnDamageLabelVisibilityChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="label"></param>
		private void OnDamageLabelVisibilityChanged( DamageNumberLabel label ) {
			if ( !label.Visible ) {
				_pool.Return( label );
			}
		}

		/*
		===============
		CreateLabel
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		private DamageNumberLabel CreateLabel() {
			var label = new DamageNumberLabel();
			AddChild( label );
			label.Connect( DamageNumberLabel.SignalName.VisibilityChanged, Callable.From( () => OnDamageLabelVisibilityChanged( label ) ) );

			return label;
		}

		/*
		===============
		OnWaveCompleted
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnWaveCompleted( in WaveChangedEventArgs args ) {
			foreach ( var child in GetChildren() ) {
				if ( child is DamageNumberLabel label ) {
					label.Visible = false;
				}
			}
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

			var waveCompleted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager ), nameof( WaveManager.WaveCompleted ) );
			waveCompleted.Subscribe( this, OnWaveCompleted );
		}
	};
};
