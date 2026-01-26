using System.Text.RegularExpressions;
using Game.Mobs;
using Game.Player;
using Game.Systems;
using Godot;
using Nomad.Core.Events;

namespace Prefabs {
	/*
	===================================================================================

	Squid

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed partial class Squid : MobBase {
		private static readonly StringName @GrabAnimationName = "grab";
		private static readonly StringName @ReleaseAnimationName = "release";

		private AnimatedSprite2D _grabPlayer;
		private ProgressBar _healthBar;
		private PlayerManager _player;

		private float _baseSpeed = 0.0f;

		private readonly Timer _grabTimer = new Timer() {
			WaitTime = 3.5f,
		};

		private IGameEvent<StatChangedEventArgs> _statChanged;

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

			_healthBar.Value -= amount;
		}

		/*
		===============
		OnBodyShapeEntered
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="bodyRid"></param>
		/// <param name="body"></param>
		/// <param name="bodyShapeIndex"></param>
		/// <param name="localShapeIndex"></param>
		private void OnBodyShapeEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is PlayerManager player ) {
				var statProvider = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IPlayerStatsProvider>();
				_baseSpeed = statProvider.Speed;

				_player = player;

				float newSpeed = 0.0f;
				_statChanged.Publish( new StatChangedEventArgs( PlayerStats.SPEED, newSpeed ) );
				_player.Hide();
				_player.GlobalPosition = _grabPlayer.GlobalPosition;
				_grabPlayer.Show();
				_grabPlayer.Play( GrabAnimationName );

				_grabTimer.Start();
			}
		}

		/*
		===============
		OnGrabPlayerTimerTimeout
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnGrabPlayerTimerTimeout() {
			_statChanged.Publish( new StatChangedEventArgs( PlayerStats.SPEED, _baseSpeed ) );
			_grabPlayer.Play( ReleaseAnimationName );
			_grabPlayer.Hide();
			_player.Show();
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

			_healthBar = GetNode<ProgressBar>( "HealthBar" );
			_healthBar.MaxValue = _health;
			_healthBar.Value = _health;

			_grabPlayer = GetNode<AnimatedSprite2D>( "GrabbedPlayer" );

			_grabTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnGrabPlayerTimerTimeout ) );
			AddChild( _grabTimer );

			var captureArea = GetNode<Area2D>( "CaptureArea" );
			captureArea.Connect( Area2D.SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnBodyShapeEntered ) );

			var eventFactory = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			_statChanged = eventFactory.GetEvent<StatChangedEventArgs>( nameof( PlayerStats ), nameof( PlayerStats.StatChanged ) );
		}
	};
};
