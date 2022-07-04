using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Upgrade : ScriptableObject
{
    public string upgradeName;

    public virtual void OnUpgrade(int stage)
    {
        
    }

    public virtual Currency GetCost(int stage)
    {
        return new Currency("0");
    }

    public virtual string GetDescription(int stage)
    {
        return "";
    }

    public virtual void Setup(int stage)
    {

    }

    public virtual string GetImprovementText(int stage)
    {
        return "";
    }
}
