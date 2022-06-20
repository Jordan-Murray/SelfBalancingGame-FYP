using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Stat
{
    public float characterStat;
    public float upperStatLimit;
    public float lowerStatLimit;
    public string characterStatName;
    public string characterStatOwner;

    public Stat(float _characterStat, float _upperStatLimit, float _lowerStatLimit,string _characterStatName, string _characterStatOwner)
    {
        characterStat = _characterStat;
        upperStatLimit = _upperStatLimit;
        lowerStatLimit = _lowerStatLimit;
        characterStatName = _characterStatName;
        characterStatOwner = _characterStatOwner;
    }

    public string StatToString()
    {
        return "Stat Owner: " + characterStatOwner + 
        ", Stat Name: " + characterStatName + 
        ", Stat:" + characterStat.ToString() + 
        ", Lower Limit: " + lowerStatLimit.ToString() + 
        ", Upper Limit: " + upperStatLimit.ToString();
    }
    
}
