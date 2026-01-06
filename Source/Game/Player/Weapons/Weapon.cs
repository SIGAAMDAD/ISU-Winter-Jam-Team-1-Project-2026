using Game.Player.Weapons;
using Nomad.Core.Util;
using System;

namespace Game.Player {
	/*
	===================================================================================
	
	Weapon
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed class Weapon( WeaponResource resource ) : IDisposable {
		public int Tier => _tier;
		private int _tier = 1;

		public InternString Id => _id;
		private readonly InternString _id = new( resource.Id );

		public WeaponType Type => _type;
		private readonly WeaponType _type = resource.Type;

		public float Damage => _damage;
		private float _damage = resource.Damage;

		public float Cooldown => _cooldown;
		private float _cooldown = resource.Cooldown;

		public float Knockback => _knockback;
		private float _knockback = resource.Knockback;

		public float LifeSteal => _lifeSteal;
		private float _lifeSteal = resource.LifeSteal;

		public float SplashRadius => _splashRadius;
		private float _splashRadius = resource.SplashRadius;

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void Dispose() {
		}

		/*
		===============
		Combine
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void Combine( Weapon weapon ) {
			if ( Id != weapon.Id ) {
				return; // not the same type
			}
		}
	};
};