namespace Game.Player {
	public sealed class WeaponSlot {
		public Weapon Weapon => _weapon;
		private Weapon _weapon;

		public int Index => _index;
		private readonly int _index;

		public bool HasWeapon => _weapon != null;

		public WeaponSlot( int index ) {
			_index = index;
		}
	};
};