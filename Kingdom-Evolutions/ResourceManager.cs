using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    #region Singleton
    public static ResourceManager instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion
    [Header("Gold")]
    [SerializeField] Currency gold = new Currency("0");
    [SerializeField] Currency goldPerSecond = new Currency("0");
    [SerializeField] float goldPerCitizenPoint = 1;
    [SerializeField] float passiveGoldPercentage = 0.2f;
    [SerializeField] long maxPassiveTime = 3600;
    [Header("Credits")]
    [SerializeField] int credits = 0;

    float timer = 0;

    private void Update()
    {
        RepeatEverySecond();
    }

    private void RepeatEverySecond()
    {
        if (Time.time >= timer)
        {
            timer = Time.time + 1f;

            AddGold(goldPerSecond);
        }
    }

    public void AddGold(Currency goldToAdd)
    {
        gold += goldToAdd;
    }

    public void RemoveGold(Currency goldToRemove)
    {
        gold -= goldToRemove;
    }

    public void SetGold(string gold)
    {
        this.gold = new Currency(gold);
    }

    public Currency GetGold()
    {
        return gold;
    }

    public void SetGoldPerecond(string gps)
    {
        goldPerSecond = new Currency(gps);
    }

    public void AddGoldPerSecond(Currency goldToAdd)
    {
        goldPerSecond += goldToAdd;
    }

    public void RemoveGoldPerSecond(Currency goldToRemove)
    {
        goldPerSecond -= goldToRemove;
    }

    public void AddPassiveGold(long seconds)
    {
        Currency goldToAdd = new Currency("0");

        if (seconds > maxPassiveTime)
        {
            goldToAdd = goldPerSecond * maxPassiveTime * passiveGoldPercentage;
            AddGold(goldToAdd);
        }
        else
        {
            goldToAdd = goldPerSecond * seconds * passiveGoldPercentage;
            AddGold(goldToAdd);
        }

        if (goldToAdd != 0)
            GameManager.instance.EnqueueGoldNotification(goldToAdd);
    }

    public Currency GetGoldPerSecond()
    {
        return goldPerSecond;
    }

    public Currency AddUnitResource(Unit unit)
    {
        Currency addAmount = unit.amountToGive;
        AddGold(addAmount);

        return addAmount;
    }

    public int GetCredits()
    {
        return credits;
    }

    public void SetCredits(int creditsToSet)
    {
        credits = creditsToSet;
    }

    public void AddCredits(int creditsToAdd)
    {
        credits += creditsToAdd;
    }

    public void RemoveCredits(int creditsToRemove)
    {
        credits -= creditsToRemove;
    }

    public void SetMaxPassiveTime(long maxPassiveTime)
    {
        this.maxPassiveTime = maxPassiveTime;
    }

    public long GetMaxPassiveTimeInHours()
    {
        return maxPassiveTime / 3600;
    }
}