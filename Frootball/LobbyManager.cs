using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LobbyManager : MonoBehaviour
{
    #region Singleton
    public static LobbyManager instance;

    
    #endregion
    
    public GameObject playerChooserPrefab;
    public GameObject playerChooserUIPrefab;

    public Transform[] blueSpawnPoints;
    public Transform[] redSpawnPoints;
    List<Transform> takenSpawnpoints;

    public int maxPlayerCount = 4;
    public int maxPlayersPerTeam = 2;

    [HideInInspector]
    public PlayerSpawnSettings playerSpawnSettings;
    
    List<Controlls> usedControlls;

    public enum LobbyState { Team_Select, Character_Select, Map_Select };
    public LobbyState lobbyState = LobbyState.Team_Select;

    [Header("Players")]
    public Color[] playerColors;
    public Color blueTeamColor;
    public Color redTeamColor;
    public bool useTeamColors = true;
    public List<Color> availableColors = new List<Color>();

    [Header("Controlls")]
    public Controlls key = Controlls.Keyboard;
    public Controlls joy1 = Controlls.Joy_One;
    public Controlls joy2 = Controlls.Joy_Two;
    public Controlls joy3 = Controlls.Joy_Three;
    public Controlls joy4 = Controlls.Joy_Four;
    
    public float inputDetectionTime = 0.08f;
    float inputTimer = 0f;

    public Animator transitionAnimator;

    public TeamSelector teamSelector;
    public Transform characterSelectScreen;
    public CharacterSelectMaker characterSelectMaker;
    public CharacterSelector[] characterSelectors;

    bool noPlayers = false;

    bool loading = false;

    private void Awake()
    {
        instance = this;
        playerSpawnSettings = FindObjectOfType<PlayerSpawnSettings>();
        teamSelector = TeamSelector.instance;
    }

    void Start()
    {
        takenSpawnpoints = new List<Transform>();
        usedControlls = new List<Controlls>();

        if (teamSelector.players.Count <= 0)
        {
            noPlayers = true;
        }

        availableColors.AddRange(playerColors);
    }

    public void LoadCharacterSelector()
    {
        if (loading) return;

        loading = true;
        StartCoroutine(LoadCharacterSelectScreen());
    }

    IEnumerator LoadCharacterSelectScreen()
    {
        yield return new WaitForSeconds(0.25f);
        transitionAnimator.SetTrigger("End");
        yield return new WaitForSeconds(1f);

        lobbyState = LobbyState.Character_Select;
        characterSelectScreen.gameObject.SetActive(true);

        List<PlayerTeamChooser> bluePlayers = TeamSelector.instance.GetPlayersByTeam(Team.Blue);
        List<PlayerTeamChooser> redPlayers = TeamSelector.instance.GetPlayersByTeam(Team.Red);

        characterSelectMaker.Spawn(teamSelector.players.Count, bluePlayers.Count, redPlayers.Count);
        
        characterSelectors = characterSelectScreen.GetComponentsInChildren<CharacterSelector>();
        
        List<CharacterSelector> blueSelectors = GetCharacterSelectorsByTeam(characterSelectors, Team.Blue);
        List<CharacterSelector> redSelectors = GetCharacterSelectorsByTeam(characterSelectors, Team.Red);

        for (int i = 0; i < bluePlayers.Count; i++)
        {
            blueSelectors[i].LoadPlayer(bluePlayers[i]);
        }

        for (int i = 0; i < redPlayers.Count; i++)
        {
            redSelectors[i].LoadPlayer(redPlayers[i]);
        }

        teamSelector.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.1f);

        transitionAnimator.SetTrigger("Start");

        UpdateColors();

        loading = false;
    }

    public void LoadTeamSelector()
    {
        if (loading) return;

        loading = true;
        StartCoroutine(LoadingTeamSelector());
    }

    IEnumerator LoadingTeamSelector()
    {
        yield return new WaitForSeconds(0.25f);
        transitionAnimator.SetTrigger("End");
        yield return new WaitForSeconds(1f);

        lobbyState = LobbyState.Team_Select;

        characterSelectScreen.gameObject.SetActive(false);
        foreach(Transform c in characterSelectMaker.transform)
        {
            Destroy(c.gameObject);
        }

        foreach (PlayerTeamChooser player in teamSelector.players)
        {
            teamSelector.ChooserUnready(player.controlls);
        }

        teamSelector.gameObject.SetActive(true);
        
        transitionAnimator.SetTrigger("Start");

        loading = false;
    }

    List<CharacterSelector> GetCharacterSelectorsByTeam(CharacterSelector[] selectors, Team team)
    {
        List<CharacterSelector> teamSelectors = new List<CharacterSelector>();

        foreach(CharacterSelector cs in selectors)
        {
            if(cs.team == team)
            {
                teamSelectors.Add(cs);
            }
        }

        return teamSelectors;

    }

    public void CheckCharSelectorsReady()
    {
        int ready = 0;

        for(int i = 0; i < characterSelectors.Length; i++)
        {
            if(characterSelectors[i].state == CharacterSelector.CharacterSelectState.Ready)
            {
                ready++;
            }
        }

        if(ready == characterSelectors.Length)
        {
            SavePlayerSettings();
            SceneLoader.instance.Load("SampleScene");
        }
    }

    public void SavePlayerSettings()
    {
        foreach (CharacterSelector selector in characterSelectors)
        {
            playerSpawnSettings.playerSettings.Add(selector.playerSettings);
        }
        
    }

    void SpawnTeamChooser(Controlls controlls)
    {
        if (teamSelector.players.Count < maxPlayerCount)
        {
            if (!usedControlls.Contains(controlls))
            {
                teamSelector.SpawnChooser(controlls);
                usedControlls.Add(controlls);

                noPlayers = false;
            }
        }
    }

    void RemoveTeamChooser(Controlls controlls)
    {
        teamSelector.RemoveChooser(controlls);
        usedControlls.Remove(controlls);

        if (teamSelector.players.Count <= 0)
        {
            noPlayers = true;
        }
    }

    public void SetColor(PlayerTeamChooser ptc)
    {
        ptc.color = availableColors[0];
        availableColors.Remove(ptc.color);
        UpdateColors();
    }

    public void SetColor(Color c)
    {
        availableColors.Remove(c);
        UpdateColors();
    }

    public void FreeColor(PlayerTeamChooser ptc)
    {
        availableColors.Add(ptc.color);
        UpdateColors();
    }

    public void FreeColor(Color c)
    {
        availableColors.Add(c);
        UpdateColors();
    }

    public void UpdateColors()
    {
        List<Color> newList = new List<Color>();

        foreach(Color c in playerColors)
        {
            if (availableColors.Contains(c))
            {
                newList.Add(c);
            }
        }
        
        availableColors = newList;
    }

    public List<CharacterSelector> GetSelectorsByControls(Controlls ctr)
    {
        List<CharacterSelector> selectors = new List<CharacterSelector>();

        foreach (CharacterSelector s in characterSelectors)
        {
            if(s.controlls == ctr)
            {
                selectors.Add(s);
            }
        }

        return selectors;
    }

    private void Update()
    {
        Activity();
    }

    void Activity()
    {
        if (noPlayers)
        {
            if (UniversalInput.Back() && lobbyState == LobbyState.Team_Select && !SceneLoader.instance.loading)
            {
                SceneLoader.instance.Load("MainMenu");
            }
        }

        if (Time.time >= inputTimer)
        {
            DetectActivity(key);
            DetectActivity(joy1);
            DetectActivity(joy2);
            DetectActivity(joy3);
            DetectActivity(joy4);
        }
    }

    void DetectActivity(Controlls ctr)
    {
        if (Input.GetButtonDown("Select_" + ctr.ToString()) || Input.GetButtonDown("Ready_" + ctr.ToString()))
        {
            inputTimer = Time.time + inputDetectionTime;

            if(lobbyState == LobbyState.Team_Select)
            {
                if (!usedControlls.Contains(ctr))
                {
                    SpawnTeamChooser(ctr);
                }
                else
                {
                    if(teamSelector.GetChooser(ctr).controledChooser == null)
                    {
                        teamSelector.ChooserReady(ctr);
                    }
                }
            }
        }

        if (Input.GetButtonDown("Back_" + ctr.ToString()))
        {
            inputTimer = Time.time + inputDetectionTime;

            if (lobbyState == LobbyState.Team_Select && !noPlayers)
            {
                if (usedControlls.Contains(ctr) && !teamSelector.GetChooser(ctr).ready)
                {
                    if(teamSelector.GetChooser(ctr).controledChooser == null)
                    {
                        RemoveTeamChooser(ctr);
                    }
                }
                else
                {
                    if (teamSelector.GetChooser(ctr).controledChooser == null)
                    {
                        teamSelector.ChooserUnready(ctr);
                    }
                }
            }
        }
    }
    
}


    
