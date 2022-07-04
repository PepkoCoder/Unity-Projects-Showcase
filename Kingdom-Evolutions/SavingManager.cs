using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System;
using System.Text.RegularExpressions;

public class SavingManager : MonoBehaviour
{
    #region Singleton
    public static SavingManager instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    [SerializeField] List<UnitTree> unitTrees;
    [SerializeField] bool loadSave;

    bool firstTimeOpening = true;

    DateTime loadTime;

    [System.Serializable]
    struct ResourcesData
    {
        public string gold;
        public string goldPerSecond;
        public int credits;
    }
    
    [System.Serializable]
    struct UnitsData
    {
        public string[] units;
    }

    [System.Serializable]
    struct UnitTreeHolderData
    {
        public string[] cost;
        public int[] spawns;
        public bool[] unlocked;
    }

    [System.Serializable]
    struct TimeData
    {
        public string time;
    }

    [System.Serializable]
    struct UpgradeData
    {
        public int[] upgradeIndexes;
        public int peopleBuyIndex;
        public int upgradeBuyIndex;
    }

    public void Save()
    {
        SaveResources();
        SaveUnits();
        SaveUnitTreeHolders();
        SaveTime();
        SaveUpgrades();
    }

    void SaveResources()
    {
        ResourcesData resourcesData = new ResourcesData();
        resourcesData.gold = ResourceManager.instance.GetGold().ToString();
        resourcesData.goldPerSecond = ResourceManager.instance.GetGoldPerSecond().ToString();
        resourcesData.credits = ResourceManager.instance.GetCredits();
        
        string path = Application.persistentDataPath + "/resources.save";

        FileStream fileStream = new FileStream(path, FileMode.Create);

        BinaryFormatter binaryFormatter = new BinaryFormatter();
        binaryFormatter.Serialize(fileStream, resourcesData);

        fileStream.Close();
    }

    void SaveUnits()
    {
        Frame f = GameManager.instance.GetFrame();

        List<string> units = new List<string>();

        for (int i = 0; i < f.GetFieldHolders().Length; i++)
        {
            Person[] people = f.GetFieldHolders()[i].field.GetPeople().ToArray();

            for (int j = 0; j < people.Length; j++)
            {
                units.Add(people[j].curUnit.unitName);
            }
        }

        UnitsData unitsData = new UnitsData();
        unitsData.units = units.ToArray();

        string path = Application.persistentDataPath + "/units.save";

        FileStream fileStream = new FileStream(path, FileMode.Create);

        BinaryFormatter binaryFormatter = new BinaryFormatter();
        binaryFormatter.Serialize(fileStream, unitsData);

        fileStream.Close();
    }

    void SaveUnitTreeHolders()
    {
        Frame frame = GameManager.instance.GetFrame();
        
        Currency[] cost = frame.GetUnitTreeHolder().GetCosts();
        string[] costs = new string[cost.Length];

        for (int j = 0; j < cost.Length; j++)
        {
            costs[j] = cost[j].ToString();
        }
        
        int[] spawns = frame.GetUnitTreeHolder().GetSpawns();
        bool[] unlocked = frame.GetUnitTreeHolder().GetUnlocks();

        UnitTreeHolderData unitTreeHolderData = new UnitTreeHolderData();
        unitTreeHolderData.cost = costs;
        unitTreeHolderData.spawns = spawns;
        unitTreeHolderData.unlocked = unlocked;

        string path = Application.persistentDataPath + "/UnitTreeHolder_" + frame.GetUnitTreeHolder().name + ".save";

        FileStream fileStream = new FileStream(path, FileMode.Create);

        BinaryFormatter binaryFormatter = new BinaryFormatter();
        binaryFormatter.Serialize(fileStream, unitTreeHolderData);

        fileStream.Close();
    }

    void SaveTime()
    {
        TimeData timeData = new TimeData();
        timeData.time = DateTime.Now.ToString();

        string path = Application.persistentDataPath + "/time.save";

        FileStream fileStream = new FileStream(path, FileMode.Create);

        BinaryFormatter binaryFormatter = new BinaryFormatter();
        binaryFormatter.Serialize(fileStream, timeData);

        fileStream.Close();
    }

    void SaveUpgrades()
    {
        List<int> indexes = new List<int>();

        foreach(UpgradeUI upgradeUI in UpgradeManager.instance.upgradeUIs)
        {
            indexes.Add(upgradeUI.currIndex);
        }

        UpgradeData upgradeData = new UpgradeData();
        upgradeData.upgradeIndexes = indexes.ToArray();
        upgradeData.peopleBuyIndex = FindObjectOfType<StackingUIManager>().unlockedPeopleIndex;
        upgradeData.upgradeBuyIndex = FindObjectOfType<StackingUIManager>().unlockedUpgradeIndex;

        string path = Application.persistentDataPath + "/upgrades.save";

        FileStream fileStream = new FileStream(path, FileMode.Create);

        BinaryFormatter binaryFormatter = new BinaryFormatter();
        binaryFormatter.Serialize(fileStream, upgradeData);

        fileStream.Close();
    }

    public void Load()
    {
        if (loadSave)
        {
            if(PlayerPrefs.GetInt("FIRST_TIME_OPENING", 1) == 1)
            {
                PlayerPrefs.SetInt("FIRST_TIME_OPENING", 0);
                firstTimeOpening = true;
            }
            else
            {
                firstTimeOpening = false;

                LoadResources();
                LoadUnits();
                LoadUnitTreeHolders();
                LoadUpgrades();
                LoadTime();
            }
        }
        else
        {
            PlayerPrefs.SetInt("FIRST_TIME_OPENING", 1);
        }
    }

    void LoadResources()
    {
        string expectedFileLocation = Application.persistentDataPath + "/resources.save";

        if (File.Exists(expectedFileLocation))
        {
            FileStream fileStream = new FileStream(expectedFileLocation, FileMode.Open);

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            ResourcesData resourcesData = (ResourcesData)binaryFormatter.Deserialize(fileStream);

            ResourceManager resourceManager = ResourceManager.instance;
            
            resourceManager.SetGold(resourcesData.gold);
            resourceManager.SetGoldPerecond(resourcesData.goldPerSecond);
            resourceManager.SetCredits(resourcesData.credits);

            fileStream.Close();
        }
    }

    void LoadUnits()
    {
        Frame f = GameManager.instance.GetFrame();

        string expectedFileLocation = Application.persistentDataPath + "/units.save";

        if (File.Exists(expectedFileLocation))
        {
            FileStream fileStream = new FileStream(expectedFileLocation, FileMode.Open);

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            UnitsData unitsData = (UnitsData)binaryFormatter.Deserialize(fileStream);
            
            fileStream.Close();

            string[] unitsSplit = unitsData.units;
            foreach (string unit in unitsSplit)
            {
                Unit u = GetUnit(unit);
                if (u != null)
                {
                    GameManager.instance.Create(u, GameManager.instance.GetRandomPos(), f);
                }
            }
        }
    }

    void LoadUnitTreeHolders()
    {

        Frame frame = GameManager.instance.GetFrame();

        string expectedFileLocation = Application.persistentDataPath + "/UnitTreeHolder_" + frame.GetUnitTreeHolder().name + ".save";

        if (File.Exists(expectedFileLocation))
        {
            FileStream fileStream = new FileStream(expectedFileLocation, FileMode.Open);

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            UnitTreeHolderData unitTreeHolderData = (UnitTreeHolderData)binaryFormatter.Deserialize(fileStream);

            Currency[] cost = new Currency[unitTreeHolderData.cost.Length];

            for (int j = 0; j < cost.Length; j++)
            {
                cost[j] = new Currency(unitTreeHolderData.cost[j]);
            }

            frame.GetUnitTreeHolder().SetCosts(cost);

            frame.GetUnitTreeHolder().SetSpawns(unitTreeHolderData.spawns);

            frame.GetUnitTreeHolder().SetUnlocks(unitTreeHolderData.unlocked);

            fileStream.Close();
        }
    }

    void LoadTime()
    {
        string expectedFileLocation = Application.persistentDataPath + "/time.save";

        if (File.Exists(expectedFileLocation))
        {
            FileStream fileStream = new FileStream(expectedFileLocation, FileMode.Open);

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            TimeData timeData = (TimeData)binaryFormatter.Deserialize(fileStream);

            DateTime last = DateTime.Parse(timeData.time);
            ResourceManager.instance.AddPassiveGold((long)(DateTime.Now - last).TotalSeconds);

            fileStream.Close();
        }
    }
    
    void LoadUpgrades()
    {
        string expectedFileLocation = Application.persistentDataPath + "/upgrades.save";

        if (File.Exists(expectedFileLocation))
        {
            FileStream fileStream = new FileStream(expectedFileLocation, FileMode.Open);

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            UpgradeData upgradeData = (UpgradeData)binaryFormatter.Deserialize(fileStream);

            UpgradeManager.instance.LoadUpgrades(upgradeData.upgradeIndexes);
            FindObjectOfType<StackingUIManager>().unlockedPeopleIndex = upgradeData.peopleBuyIndex;
            FindObjectOfType<StackingUIManager>().unlockedUpgradeIndex = upgradeData.upgradeBuyIndex;
            FindObjectOfType<StackingUIManager>().Setup();

            fileStream.Close();
        }
    }

    public bool FirstTimeOpening()
    {
        return firstTimeOpening;
    }

    Unit GetUnit(string name)
    {
        foreach(UnitTree unitTree in unitTrees)
        {
            foreach(Unit unit in unitTree.unitsHierarchy)
            {
                if(unit.unitName.Equals(name))
                {
                    return unit;
                }
            }
        }

        return null;
    }
}
