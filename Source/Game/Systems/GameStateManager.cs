using System;
using Godot;
using Nomad.Core.Events;
using Nomad.Core.Logger;

namespace Game.Systems {
	/*
	===================================================================================

	GameStateManager

	===================================================================================
	*/
	/// <summary>
	/// <para>Handles state across the game's ui and level scenes</para>
	/// <para>This class is basically an automated event bus (something that does all the heavy lifting with event subscription) for different game states.</para>
	/// <para>This class handles both global <see cref="GameState"/> management, and also allows custom dynamically defined states that are cleared upon new scene instantiation.</para>
	/// <para>To register an event catcher for global game events (part of the <see cref="GameState"/> enum), use the <see cref="GameStateManager.SubscribeToGameStateEvent(GameState, StateEventType, Action{GameState}?)"/>
	/// method.</para>
	/// </summary>

	//
	// NOTE: keep this file under ideally 800 lines, and avoid breaches of SRP
	//
	// IDEAS FOR EXTENDING THIS:
	// - use ObjectPool for scene-based state variables for more efficient memory management
	// - allow the creator of a dynamic state variable to have a custom state validator callback when changing the state's internals
	// - allow the creator of a dynamic state variable to modify an internal Dictionary hosting variable for an object's state
	// - thread safety... maybe
	// - let the event callback do the state validation itself (so that we don't have manual state validation here)
	//

	public partial class GameStateManager : Node {
		/// <summary>
		/// The singleton handler for the global <see cref="GameStateManager"/>, access is filtered directly through this
		/// </summary>
		public static GameStateManager Instance {
			get {
				// create the instance if we haven't already
				lock ( _instanceLock ) {
					_instance ??= new GameStateManager();
					return _instance;
				}
			}
		}

		/// <summary>
		/// The game's current state, can only be changed through <see cref="SetState"/>
		/// </summary>
		/// <remarks>
		/// The <see cref="GameStateChanged"/> event is fire whenever this variable is changed
		/// </remarks>
		public GameState GameState { get; private set; } = GameState.TitleScreen;

		public static IGameEvent<GameStateChangedEventArgs> GameStateChanged => Instance._stateChanged;
		private readonly IGameEvent<GameStateChangedEventArgs> _stateChanged;

		private readonly ILoggerCategory _category;
		private readonly ILoggerService _logger;

		/// <summary>
		/// The internal singleton handle.
		/// </summary>
		private static GameStateManager _instance;
		private static readonly object _instanceLock = new object();

		/*
		===============
		GameStateManager
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private GameStateManager() {
			Name = nameof( GameStateManager );

			var serviceLocator = ( (Node)Engine.GetMainLoop().Get( SceneTree.PropertyName.Root ) ).GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator;

			var eventFactory = serviceLocator.GetService<IGameEventRegistryService>();
			_stateChanged = eventFactory.GetEvent<GameStateChangedEventArgs>( nameof( GameStateChanged ) );

			_logger = serviceLocator.GetService<ILoggerService>();
			_category = _logger.CreateCategory( nameof( GameStateManager ), LogLevel.Info, true );
		}

		/*
		===============
		PauseGame
		===============
		*/
		/// <summary>
		/// </summary>
		/// <remarks>
		/// A lose wrapper around calling <see cref="SetGameState"/> with <see cref="GameState.Paused"/>, effectively the same as writing
		/// "Instance.SetGameSet( GameState.Paused )"
		/// </remarks>
		/// <returns>True if state was changed to <see cref="GameState.Paused"/></return>
		public bool PauseGame() {
			// make sure we actually have valid state to do the transition
			switch ( GameState ) {
				case GameState.TitleScreen:
					_logger.PrintWarning( in _category, "GameStateManager.PauseGame: attempted to pause game from title screen." );
					break;
				case GameState.Paused:
					_logger.PrintWarning( in _category, "GameStateManager.PauseGame: game is already paused." );
					break;
				case GameState.Level:
					SetGameState( GameState.Paused );
					return true;
				default: // uh-oh
					throw new ArgumentOutOfRangeException( "GameStateManager has an invalid game state!" );
			}
			return false;
		}

		/*
		===============
		UnPauseGame
		===============
		*/
		/// <summary>
		/// </summary>
		/// <remarks>
		/// A lose wrapper around calling <see cref="SetGameState"/> with <see cref="GameState.Level"/>, effectively the same as writing
		/// "SetGameSet( GameState.Level )" when the pause menu is active
		/// </remarks>
		/// <returns>True if state was changed to <see cref="GameState.Level"/></return>
		public bool UnPauseGame() {
			// make sure we actually have valid state to do the transition
			switch ( GameState ) {
				case GameState.TitleScreen:
					_logger.PrintWarning( in _category, "GameStateManager.UnPauseGame: attempted to unpause game from title screen." );
					break;
				case GameState.Level:
					_logger.PrintWarning( in _category, "GameStateManager.UnPauseGame: game isn't paused, but function is called." );
					break;
				case GameState.Paused:
					SetGameState( GameState.Level );
					return true;
				default: // uh-oh
					throw new ArgumentOutOfRangeException( "GameStateManager has an invalid game state!" );
			}
			return false;
		}

		/*
		===============
		ActivateTitleScreen
		===============
		*/
		/// <summary>
		/// </summary>
		/// <remarks>
		/// A lose wrapper around calling <see cref="SetGameState"/> with <see cref="GameState.Titlescreen"/>, effectively the same as writing
		/// "SetGameSet( GameState.Titlescreen )" when the pause menu is active
		/// </remarks>
		/// <returns>True if state was changed to <see cref="GameState.TitleScreen"/></return>
		public bool ActivateTitleScreen() {
			// make sure we actually have valid state to do the transition
			switch ( GameState ) {
				case GameState.Level:
					_logger.PrintWarning( in _category, "GameStateManager.ActivateTitleScreen: attempted to activate title screen from a level without using the pause menu." );
					break;
				case GameState.TitleScreen:
					_logger.PrintWarning( in _category, "GameStateManager.ActivateTitleScreen: title screen state reactivated." );
					break;
				case GameState.Paused:
					SetGameState( GameState.TitleScreen );
					return true;
				default: // uh-oh
					throw new ArgumentOutOfRangeException( "GameStateManager has an invalid game state!" );
			}
			return false;
		}

		/*
		===============
		ActivateTitleScreen
		===============
		*/
		/// <summary>
		/// </summary>
		/// <remarks>
		/// A lose wrapper around calling <see cref="SetGameState"/> with <see cref="GameState.Level"/>, effectively the same as writing
		/// "SetGameSet( GameState.Level )" when the pause menu is active
		/// </remarks>
		/// <returns>True if state was changed to <see cref="GameState.Level"/></return>
		public bool ActivateLevel() {
			// make sure we actually have valid state to do the transition
			switch ( GameState ) {
				case GameState.Paused:
					_logger.PrintWarning( in _category, "GameStateManager.ActivateLevel: attempted to activate level state from pause menu, use GameStateManager.UnPauseGame instead." );
					break;
				case GameState.Level:
					_logger.PrintWarning( in _category, "GameStateManager.ActivateLevel: level state reactivated." );
					break;
				case GameState.TitleScreen:
					SetGameState( GameState.Level );
					return true;
				default: // uh-oh
					throw new ArgumentOutOfRangeException( "GameStateManager has an invalid game state!" );
			}
			return false;
		}

		/*
		===============
		SetGameState
		===============
		*/
		/// <summary>
		/// Sets the <see cref="GameState"/>, and fires the <see cref="GameStateChanged"/> event.
		/// </summary>
		/// <remarks>
		/// The <see cref="GameStateChanged"/> event is only triggered if no errors occurred.
		/// </remarks>
		/// <param name="state">The new <see cref="GameState"/>, should ideally be different from the current gamestate.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="state"/> isn't a valid <see cref="GameState"/>.</exception>
		private void SetGameState( GameState state ) {
			if ( state < GameState.TitleScreen || state >= GameState.Count ) {
				throw new ArgumentOutOfRangeException( $"Provided state '{Enum.GetName( typeof( GameState ), state )}' is not a valid GameState" );
			} else if ( GameState == state ) {
				_logger.PrintWarning( in _category, $"GameStateManager.SetGameState: same game state." );
			}

			_logger.PrintLine( in _category, $"GameStateManager.SetState: changing state to '{Enum.GetName( typeof( GameState ), state )}'..." );

			// notify the system
			TriggerGameStateChange( state );
		}

		/*
		===============
		TriggerGameStateChange
		===============
		*/
		/// <summary>
		/// Publishes and notifies all <see cref="global::GameState"/> subscribers of a game state change.
		/// </summary>
		/// <remarks>
		/// Thread safety is handled from <see cref="SetGameState"/>, no need to lock the lock again.
		/// </remarks>
		/// <param name="newState">The new state that we're changing to.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="newState"/> isn't a valid <see cref="global::GameState"/>.</exception>
		private void TriggerGameStateChange( GameState newState ) {
			if ( newState < GameState.TitleScreen || newState >= GameState.Count ) {
				throw new ArgumentOutOfRangeException( nameof( newState ) );
			}

			// establish the new state BEFORE we notify the rest of the system to avoid state corruption
			GameState oldState = GameState;
			GameState = newState;

			// notify the system now that we've established the new state
			_stateChanged.Publish( new GameStateChangedEventArgs( oldState, newState ) );
		}
	};
};