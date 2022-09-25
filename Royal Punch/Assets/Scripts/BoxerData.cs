using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BoxerData", menuName = "SO/BoxerData", order = 0)]
public class BoxerData : ScriptableObject 
{
    [SerializeField] private int _health;   
    [SerializeField] private int _power;

    public int Health { get => _health; set => _health = value; }
    public int Power { get => _power; set => _power = value; }
}
