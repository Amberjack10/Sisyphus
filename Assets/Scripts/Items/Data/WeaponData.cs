using System;
using UnityEngine;

public enum WeaponType
{
    Melee,
    Range
}

[Serializable]
public class WeaponData : ItemSO
{
    [field: Header("Weapon")]
    [SerializeField] private float _atk;
    [SerializeField] private float _atkRate;
    [SerializeField] private float _critRate;
    [SerializeField] private float _critDamage;
    [SerializeField] private float _range;
    [SerializeField] private int _lifeSteal;
    [SerializeField] private WeaponType _type;

    [field: Header("Projectile")]
    [SerializeField] private string _projectilePath;
    [SerializeField] private int _numberOfProjectile;

    public float Atk => _atk;
    public float AtkRate => _atkRate;
    public float CritRate => _critRate;
    public float CritDamage => _critDamage;
    public float Range => _range;
    public int LifeSteal => _lifeSteal;
    public WeaponType Type => _type;

    public string ProjectilePath => _projectilePath;
    public int NumberOfProjectile => _numberOfProjectile;

    private GameObject _projectile;
    public GameObject Projectile
    {
        get
        {
            if (Type == WeaponType.Melee)
                return null;
            if (_projectile == null && !ProjectilePath.Equals("None")) _projectile = Resources.Load<GameObject>(ProjectilePath);
            return _projectile;
        }
    }
}