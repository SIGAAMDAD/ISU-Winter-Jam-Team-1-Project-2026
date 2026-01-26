using Game.Player;
using Game.Systems;
using Godot;
using Nomad.Core.Events;

namespace Game.Mobs {
	/*
	===================================================================================

	EffectBase

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public partial class EffectBase : AnimatedSprite2D {
		public int EffectId => _effectId;
		protected int _effectId;

		public IGameEvent<int> EffectFinished => _effectFinished;
		protected IGameEvent<int> _effectFinished;

		/*
		===============
		Enable
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void Enable() {
			Visible = true;
			ProcessMode = ProcessModeEnum.Pausable;
		}

		/*
		===============
		Disable
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void Disable() {
			SetDeferred( PropertyName.Visible, false );
			SetDeferred( PropertyName.ProcessMode, (long)ProcessModeEnum.Disabled );
		}

		/*
		===============
		OnPlayerEntered
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="player"></param>
		protected virtual void OnPlayerEntered( PlayerManager player ) {
		}

		/*
		===============
		OnPlayerExited
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="player"></param>
		protected virtual void OnPlayerExited( PlayerManager player ) {
		}

		/*
		===============
		OnBodyEntered
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="bodyRid"></param>
		/// <param name="body"></param>
		/// <param name="bodyShapeIndex"></param>
		/// <param name="localShapeIndex"></param>
		private void OnBodyEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is PlayerManager player ) {
				OnPlayerEntered( player );
			}
		}

		/*
		===============
		OnBodyEntered
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="bodyRid"></param>
		/// <param name="body"></param>
		/// <param name="bodyShapeIndex"></param>
		/// <param name="localShapeIndex"></param>
		private void OnBodyExited( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is PlayerManager player ) {
				OnPlayerExited( player );
			}
		}

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();

			var collisionArea = GetNode<Area2D>( "Area2D" );
			collisionArea.Connect( Area2D.SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnBodyEntered ) );
			collisionArea.Connect( Area2D.SignalName.BodyShapeExited, Callable.From<Rid, Node2D, int, int>( OnBodyExited ) );

			var eventFactory = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			_effectFinished = eventFactory.GetEvent<int>( nameof( EffectBase ), nameof( EffectFinished ) );
		}
	};
};
