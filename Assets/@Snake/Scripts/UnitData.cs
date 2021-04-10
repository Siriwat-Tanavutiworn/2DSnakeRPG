using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName =("Datas/UnitData"), order = 1)]
public class UnitData : ScriptableObject
{
    public UnitType _unitType;

    [Header("Status")]
    public float _HP;
    public float _ATK;
    public float _DEF;

    public enum UnitType
    {
        Red, Green, Blue
    }


}
