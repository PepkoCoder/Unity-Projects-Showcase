using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    GameState gameState = GameState.MENU;
    GameState lastGameState = GameState.STACK_MODE;
    InteractState interactState = InteractState.FREE;
    NotificationType notificationType = NotificationType.NONE;

    [Header("Spawning")]
    public GameObject prefabToSpawn;
    public GameObject spawnEffect;
    public int maxPeople = 10;
    public float spawnTime = 1f;
    float spawnTimer = 0;

    [Header("Notifications")]
    public GameObject notificationPanel;
    public TextMeshProUGUI notificationText;
    public GameObject levelnotificationPanel;
    public TextMeshProUGUI levelNotificationText;
    public GameObject unlockNotificationPanel;
    public TextMeshProUGUI unlockNotificationText;
    public Image unlockedUnitIcon;

    [Header("Units")]
    [Space]
    public int unlockNewUnitAfterX = 4;
    public GameObject unitPrefab;
    public GameObject unitFloatPrefab;

    [Header("UI Stuff")]
    public GameObject shopUI;
    public GameObject upgradesUI;
    public ResourcesUIManager resourcesUIManager;
    public StackingUIManager stackingUIManager;

    [SerializeField] Frame currFrame;

    [Header("Transitions")]
    public GameObject transitionGraphic;
    public float timeToFadeIn = 1f;

    float counter = 0;

    //People Cash-In
    Person markedPerson;
    List<NotificationType> notificationsWaiting = new List<NotificationType>();

    private void Start()
    {
        SettingsSetup();
        GameSetup();
        
    }

    private void Update()
    {
        SetGoldText();

        if(notificationsWaiting.Count > 0)
        {
            if(GetGameState() != GameState.NOTIFICATION_UI && GetGameState() != GameState.ATTACK_VIEW_MODE)
            {
                if(notificationsWaiting[0] == NotificationType.GOLD)
                {
                    OpenGoldNotification();
                    notificationsWaiting.RemoveAt(0);
                }
                else if (notificationsWaiting[0] == NotificationType.UNLOCK)
                {
                    OpenUnlockNotification();
                    notificationsWaiting.RemoveAt(0);
                }
            }
        }

        if(GetGameState() == GameState.STACK_MODE)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TransitionToField(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TransitionToField(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TransitionToField(2);
            }
        }
        else if(GetGameState() == GameState.NOTIFICATION_UI)
        {
            if (Input.GetMouseButtonDown(0))
            {
                ChangeGameState(lastGameState);
                SoundManager.instance.Play("Click3");

                switch (notificationType)
                {
                    case NotificationType.GOLD:
                        CloseGoldNotification();
                        notificationType = NotificationType.NONE;
                        break;

                    case NotificationType.UNLOCK:
                        CloseUnlockNotification();
                        notificationType = NotificationType.NONE;
                        break;
                }
                
            }
        }
    }

    #region General
    
    public void SettingsSetup()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }

    public void GameSetup()
    {
        currFrame.GetUnitTreeHolder().Setup();

        StartCoroutine("Load");

        CloseUI(shopUI);
        CloseUI(upgradesUI);
    }

    public void FirstTimeOpen()
    {
        ResourceManager.instance.AddGold(new Currency("1"));
    }

    public void ChangeGameState(GameState state)
    {
        lastGameState = gameState;
        gameState = state;
    }

    public GameState GetGameState()
    {
        return gameState;
    }

    public void ChangeNotificationType(NotificationType type)
    {
        notificationType = type;
    }
    
    private void OnApplicationQuit()
    {
        SavingManager.instance.Save();
    }
    
    private void OnApplicationPause(bool pause)
    {
        if (pause)
            SavingManager.instance.Save();

        if (!pause)
            SceneManager.LoadScene("GameScene");
    }

    IEnumerator Load()
    {
        yield return 0;
        SavingManager.instance.Load();
        SoundManager.instance.Play("Music");
        
        if (SavingManager.instance.FirstTimeOpening())
        {
            FirstTimeOpen();
        }
        else
        {
            ChangeGameState(GameState.STACK_MODE);
        }
    }
    
    public void EnqueueUnlockNotification(Unit unit)
    {
        notificationsWaiting.Add(NotificationType.UNLOCK);
        unlockNotificationText.text = "Congratulations! \n You unlocked the <color=#6ead57>" + unit.unitName + "</color>";
        unlockedUnitIcon.sprite = unit.sprite;
    }

    public void OpenUnlockNotification()
    {
        unlockNotificationPanel.SetActive(true);

        ChangeNotificationType(NotificationType.UNLOCK);
        ChangeGameState(GameState.NOTIFICATION_UI);

        SoundManager.instance.Play("Unlock");
    }

    public void CloseUnlockNotification()
    {
        unlockNotificationPanel.GetComponent<Animator>().SetTrigger("Close");
    }

    public void EnqueueGoldNotification(Currency gold)
    {
        notificationsWaiting.Add(NotificationType.GOLD);
        string gold_string = gold.ToString();
        notificationText.text = "<size=18> Welcome back sire!</size> While you were away, our people have gathered \n " +
                                "<color=#fee761><size=18>" + gold_string + " <sprite=0></size></color> ";
    }

    public void OpenGoldNotification()
    {
        notificationPanel.SetActive(true);

        SoundManager.instance.Play("Notification");

        ChangeGameState(GameState.NOTIFICATION_UI);
        
        notificationType = NotificationType.GOLD;
    }

    public void CloseGoldNotification()
    {
        notificationPanel.GetComponent<Animator>().SetTrigger("Close");
    }

    #endregion

    #region Stacking Mode

    public void SetGoldText()
    {
        resourcesUIManager.SetResourceText(ResourceManager.instance.GetGold().ToString(), ResourceManager.instance.GetCredits().ToString());
        resourcesUIManager.SetGoldPerSecondText(ResourceManager.instance.GetGoldPerSecond().ToString() + " <sprite=0>/s");
    }

    public void ChangeInteractState(InteractState state)
    {
        interactState = state;
    }

    public InteractState GetInteractState()
    {
        return interactState;
    }

    public void OpenUI(GameObject ui)
    {
        ui.SetActive(true);
        ui.transform.DOMoveY(0, 0.3f);
        ChangeGameState(GameState.MENU);
    }

    public void CloseUI(GameObject ui)
    {
        ui.transform.DOMoveY(-11, 0.3f).OnComplete(() => ui.SetActive(false));
        ChangeGameState(GameState.STACK_MODE);
    }

    public Vector2 GetRandomPos()
    {
        Field currentField = currFrame.GetFieldHolders()[0].field;
        return new Vector2(Random.Range(currentField.min.x, currentField.max.x), Random.Range(currentField.min.y, currentField.max.y));
    }

    public void BuyNew(Unit unit)
    {
        if (Time.time > spawnTimer && currFrame.GetPeople().Count < maxPeople)
        {
            UnitTreeHolder unitTreeHolder = currFrame.GetUnitTreeHolder();

            if(ResourceManager.instance.GetGold() >= unitTreeHolder.GetCost(unit))
            {
                spawnTimer = Time.time + spawnTime;

                GameObject spawnd = Spawn(unit, GetRandomPos(), true);

                ResourceManager.instance.RemoveGold(unitTreeHolder.GetCost(unit));
                unitTreeHolder.IncreaseCost(unit);
            }
        }
    }

    public void BuyNewForCredits(Unit unit)
    {
        if (currFrame.GetPeople().Count < maxPeople)
        {
            
            if (ResourceManager.instance.GetCredits() >= unit.creditsCost)
            {
                spawnTimer = Time.time + spawnTime;

                GameObject spawnd = Spawn(unit, GetRandomPos(), true);
                ResourceManager.instance.RemoveCredits(unit.creditsCost);
            }
        }
    }

    public GameObject Spawn(Unit unit, Vector2 pos, bool bought = false, int frameIndex = -1)
    {
        Frame spawnFrame = currFrame;

        Field spawnField = spawnFrame.GetFieldForUnit(unit);
        Field currentField = spawnFrame.GetCurrentField();

        if (spawnField.GetPeople().Count >= maxPeople + 1) return null;
        
        GameObject spawned = Create(unit, pos, spawnFrame);
        Person sPerson = spawned.GetComponent<Person>();

        ResourceManager.instance.AddGoldPerSecond(sPerson.curUnit.amountToGive);

        if(spawnFrame.GetFieldIndex(currentField) == spawnFrame.GetFieldIndex(spawnField))
        {
            //Effects
            Instantiate(spawnEffect, new Vector3(pos.x, pos.y, -9), Quaternion.identity);
            SoundManager.instance.Play("Spawn");

            spawned.transform.DOPunchScale(Vector3.one * 0.3f, 0.2f, 10, 0);
        }
        else if(spawnFrame.GetFieldIndex(currentField) < spawnFrame.GetFieldIndex(spawnField))
        {
            //Effects
            Instantiate(spawnEffect, new Vector3(pos.x, pos.y, -9), Quaternion.identity);
            SoundManager.instance.Play("Spawn");

            SpriteRenderer sp = Instantiate(unitFloatPrefab, new Vector3(pos.x, pos.y, -9), Quaternion.identity).GetComponent<SpriteRenderer>();
            sp.sprite = unit.sprite;
        }

        spawnFrame.GetUnitTreeHolder().IncreaseSpawns(unit);
        
        //Check if this is the first time spawning this unit
        if(spawnFrame.GetUnitTreeHolder().GetSpawns(unit) == 1)
        {
            StartCoroutine(FreezeFrame(0.1f));
            EnqueueUnlockNotification(unit);

            if(unit.score >= unlockNewUnitAfterX)
            {
                stackingUIManager.UnlockNextUnit();
            }
        }

        return spawned;
    }

    public GameObject Create(Unit unit, Vector2 pos, Frame frame)
    {
        GameObject spawned = Instantiate(unitPrefab, pos, Quaternion.identity);
        spawned.name = "Person " + frame.GetPeople().Count;

        Person sPerson = spawned.GetComponent<Person>();

        PersonController personController = spawned.GetComponent<PersonController>();
        if (personController != null)
        {
            personController.SetParent(frame, frame.GetFieldForUnit(unit));
        }
        
        sPerson.Setup(unit);
        frame.AddPerson(sPerson);

        return spawned;
    }

    public void AddPerson(Person person)
    {
        currFrame.AddPerson(person);
    }

    public void RemovePerson(Person person)
    {
        currFrame.RemovePerson(person);
    }

    public Person FindClosestPerson(Person curPerson)
    {
        Person closestPerson = null;

        if (currFrame.GetPeople().Count <= 0) return null;

        foreach(Person p in currFrame.GetPeople())
        {
            if (p == curPerson) continue;

            if (closestPerson == null) closestPerson = p;

            float personDistance = Vector2.Distance(p.transform.position, curPerson.transform.position);
            float closestDistance = Vector2.Distance(closestPerson.transform.position, curPerson.transform.position);

            if (personDistance < closestDistance && personDistance <= curPerson.combineDistance)
            {
                closestPerson = p;
            }
        }

        if (closestPerson == curPerson || closestPerson == null) return null;

        float closestDist = Vector2.Distance(closestPerson.transform.position, curPerson.transform.position);

        if (closestDist > curPerson.combineDistance) return null;

        return closestPerson;
    }

    public Person FindClosestPerson(Vector2 position, float maxDistance)
    {
        Person closestPerson = currFrame.GetPeople()[0];

        if (currFrame.GetPeople().Count <= 0) return null;

        foreach (Person p in currFrame.GetPeople())
        {
            float personDistance = Vector2.Distance(p.transform.position, position);
            float closestDistance = Vector2.Distance(closestPerson.transform.position, position);

            if (personDistance < closestDistance && personDistance <= maxDistance)
            {
                closestPerson = p;
            }
        }

        float closestDist = Vector2.Distance(closestPerson.transform.position, position);
        
        if (closestDist > maxDistance) return null;

        return closestPerson;
    }
    
    public void ChangeToField(int index)
    {
        currFrame.SetCurrentField(index);
    }

    public Frame GetFrame()
    {
        return currFrame;
    }

    public void SelectPerson(Person person)
    {
        markedPerson = person;
        interactState = InteractState.UNIT_SELECTED;
    }

    public bool Overpopulated()
    {
        return currFrame.GetPeople().Count >= maxPeople;
    }

    public bool EnoughGold(Currency amount)
    {
        return ResourceManager.instance.GetGold() >= amount;
    }

    IEnumerator FreezeFrame(float seconds)
    {
        Time.timeScale = 0.3f;
        yield return new WaitForSeconds(seconds);
        Time.timeScale = 1;
    }

    #endregion

    #region Transitions
    
    public void TransitionToField(int index)
    {
        if (GetGameState() == GameState.TRANSITION) return;
        ChangeGameState(GameState.TRANSITION);

        transitionGraphic.SetActive(true);

        SoundManager.instance.Play("Transition_In");

        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(transitionGraphic.transform.DOScale(18f, timeToFadeIn * 0.8f).OnComplete(() => ChangeToField(index)))
            .AppendCallback(() => SoundManager.instance.Play("Transition_Out"))
            .Append(transitionGraphic.transform.DOScale(0f, timeToFadeIn * 0.8f).OnComplete(() => transitionGraphic.SetActive(false)))
            .AppendCallback(() => ChangeGameState(GameState.STACK_MODE))
            .Insert(0f, transitionGraphic.transform.DORotate(new Vector3(0f, 0f, 180f), timeToFadeIn * 0.8f))
            .Insert(1f, transitionGraphic.transform.DORotate(new Vector3(0f, 0f, 0f), timeToFadeIn * 0.8f));

        mySequence.Play();

        transitionGraphic.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    #endregion
}

[System.Serializable]
public class UnitTreeHolder
{
    public new string name;
    public UnitTree unitTree;

    [Header("Unlocking Units")]
    public int unitsNeededToUnlockNext = 5;
    [SerializeField] Currency[] cost;
    [SerializeField] bool autoGenerateCost = false;
    [SerializeField] int[] spawns;
    [SerializeField] bool[] unlocked;

    public void Setup()
    {
        cost = new Currency[unitTree.unitsHierarchy.Length];

        for(int i = 0; i < unitTree.unitsHierarchy.Length; i++)
        {
            unitTree.unitsHierarchy[i].cost.ToFloat();
            unitTree.unitsHierarchy[i].amountToGive.ToFloat();
        }

        for (int i = 0; i < cost.Length; i++)
        {
            unitTree.unitsHierarchy[i].cost.ToFloat();
            cost[i] = unitTree.unitsHierarchy[i].cost;
        }

        spawns = new int[unitTree.unitsHierarchy.Length];
        unlocked = new bool[unitTree.unitsHierarchy.Length];

        UnlockAll();
    }

    public int GetUnitIndex(Unit unit)
    {
        int index = 0;
        for (int i = 0; i < unitTree.unitsHierarchy.Length; i++)
        {
            if (unitTree.unitsHierarchy[i].name == unit.name)
            {
                index = i;
            }
        }

        return index;
    }
 
    public Currency[] GetCosts()
    {
        return cost;
    }

    public void SetCosts(Currency[] costs)
    {
        cost = costs;
    }

    public Currency GetCost(Unit unit)
    {
        return cost[GetUnitIndex(unit)];
    }

    public void IncreaseCost(Unit unit)
    {
        Currency curCost = cost[GetUnitIndex(unit)];
        float curCostFloat = curCost.GetFloat();

        if(curCost.GetMultiplier() == "")
        {
            if (curCostFloat <= 10f)
            {
                curCost += 2f;
            }
            else if (curCostFloat > 10f && curCostFloat <= 100f)
            {
                curCost += 10f;
            }
            else
            {
                curCost += 50f;
            }
        }
        else
        {
            if (curCostFloat <= 10f)
            {
                curCost += 0.125f;
            }
            else if (curCostFloat > 10f && curCostFloat <= 100f)
            {
                curCost += 1.25f;
            }
            else
            {
                curCost += 12.5f;
            }
        }
       
    }

    public int GetSpawns(Unit unit)
    {
        return spawns[GetUnitIndex(unit)];
    }

    public int[] GetSpawns()
    {
        return spawns;
    }

    public void SetSpawns(int[] _spawns)
    {
        spawns = _spawns;
    }

    public bool[] GetUnlocks()
    {
        return unlocked;
    }

    public void SetUnlocks(bool[] unlocks)
    {
        unlocked = unlocks;
    }

    public bool CanBuy(Unit unit)
    {
        int index = GetUnitIndex(unit);
        return (index == 0) ? true : spawns[GetUnitIndex(unit)] > 0;
    }

    public void IncreaseSpawns(Unit unit)
    {
        spawns[GetUnitIndex(unit)]++;
        CheckIfCanUnlockNext();
    }

    public bool GetUnitUnlocked(Unit unit)
    {
        return unlocked[GetUnitIndex(unit)];
    }

    public int GetLastUnlockedIndex()
    {
        int i = 0;
        for (i = 0; i < unlocked.Length; i++)
        {
            if (unlocked[i] == false)
            {
                return i - 1;
            }
        }

        return i;
    }

    public void UnlockNextUnit()
    {
        if (GetLastUnlockedIndex() + 1 >= unlocked.Length) return;

        unlocked[GetLastUnlockedIndex()+1] = true;
    }

    public void CheckIfCanUnlockNext()
    {
        int index = GetLastUnlockedIndex();

        if (index >= unlocked.Length) return;

        if(spawns[index] >= unitsNeededToUnlockNext)
        {
            UnlockNextUnit();
        }
    }

    public bool IsLastUnlocked(Unit unit)
    {
        return GetUnitIndex(unit) == GetLastUnlockedIndex();
    }

    public void UnlockAll()
    {
        for(int i = 0; i < unlocked.Length; i++)
        {
            unlocked[i] = true;
        }
    }
}

public enum GameState
{
    MENU,
    TRANSITION,
    STACK_MODE,
    BUILD_MODE,
    NOTIFICATION_UI,
    WORLD_VIEW_MODE,
    ATTACK_VIEW_MODE
}

public enum InteractState
{
    UNIT_SELECTED,
    BUILDING_SELECTED,
    HOLDING_UNIT,
    FREE
}

public enum NotificationType
{
    NONE,
    GOLD,
    UNLOCK,
    ATTACK,
    UPGRADE
}
