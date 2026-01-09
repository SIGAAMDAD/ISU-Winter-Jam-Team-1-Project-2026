using Godot;

namespace Prefabs {
	public readonly record struct ArenaSizeChangedEventArgs(
		Vector2 Size,
		Vector2 IncrementAmount
	);
};