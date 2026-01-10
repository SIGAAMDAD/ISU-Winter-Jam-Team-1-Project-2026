using Game.Mobs;
using Game.Player;
using Game.Systems;
using Godot;
using Nomad.Core.Events;

namespace Prefabs {
	/*
	===================================================================================
	
	WhirlPool
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class WhirlPool : EffectBase {
		private readonly Timer _activeTimer = new Timer() {
			WaitTime = 7.5f,
			OneShot = true,
			Autostart = true
		};

		private float _baseSpeed = 0.0f;

		private IGameEvent<StatChangedEventArgs> _statChanged;

		/*
		===============
		OnKill
		===============
		*/
		/// <summary>
		/// Called when the whirlpool's timer ends.
		/// </summary>
		private void OnKill() {
			QueueFree();
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
				
				float newSpeed = _baseSpeed * 0.5f;
				_statChanged.Publish( new StatChangedEventArgs( PlayerStats.SPEED, newSpeed ) );
			}
		}

		/*
		===============
		OnBodyShapeExited
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="bodyRid"></param>
		/// <param name="body"></param>
		/// <param name="bodyShapeIndex"></param>
		/// <param name="localShapeIndex"></param>
		private void OnBodyShapeExited( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is PlayerManager player ) {
				_statChanged.Publish( new StatChangedEventArgs( PlayerStats.SPEED, _baseSpeed ) );
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
			
			var area2D = GetNode<Area2D>( "Area2D" );
			area2D.Connect( Area2D.SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnBodyShapeEntered ) );
			area2D.Connect( Area2D.SignalName.BodyShapeExited, Callable.From<Rid, Node2D, int, int>( OnBodyShapeExited ) );

			_activeTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnKill ) );
			
			var eventFactory = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			_statChanged = eventFactory.GetEvent<StatChangedEventArgs>( nameof( PlayerStats.StatChanged ) );
		}
	};
};