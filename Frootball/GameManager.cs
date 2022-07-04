using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager instance;

    private void Awake()
    {
        instance = this;
    }
    #endregion
    
    public Transform[] blueSpawnPoints;
    public Transform[] redSpawnPoints;
    public List<GameObject> players;
    
    public GameMode gameMode = GameMode.Story;

    [HideInInspector]
    public PlayerSpawnSettings playerSpawnSettings;

    public PauseMenu pauseMenu;
    public bool paused;
    public bool started = false;

    public Team winner;

    public EndScreen winScreen, drawScreen;
    public Camera endCamera;

    public TextMeshProUGUI countdownText;
    public int countdownTime = 5;

    public List<GameObject> disableOnEnd = new List<GameObject>();
    
    public Animator transitionAnim;

    GameObject ball;
    GameObject endScreenParent;

    public GameObject levelScoreUI;

    void Start()
    {
        playerSpawnSettings = FindObjectOfType<PlayerSpawnSettings>();
        pauseMenu = FindObjectOfType<PauseMenu>();
        SpawnPlayers();


        ball = GameObject.FindGameObjectWithTag("Ball");
        disableOnEnd.Add(ball);
        ball.SetActive(false);
        pauseMenu.Hide(true);

        transitionAnim.SetTrigger("Start");
    }

    void SpawnPlayers()
    {
        List<Transform> takenSpawnpoints = new List<Transform>();

        for(int i = 0; i < playerSpawnSettings.playerSettings.Count; i++)
        {
            PlayerSettings settings = playerSpawnSettings.playerSettings[i];
            Transform[] spawnPoints = (settings.team == Team.Blue) ? blueSpawnPoints : redSpawnPoints;

            GameObject prefabToSpawn = (settings.controlls == Controlls.AI) ? playerSpawnSettings.AIprefab : playerSpawnSettings.playerPrefab;

            for(int j = 0; j < spawnPoints.Length; j++)
            {
                if (!takenSpawnpoints.Contains(spawnPoints[j]))
                {
                    GameObject player = Instantiate(prefabToSpawn, spawnPoints[j].position, Quaternion.identity);
                    takenSpawnpoints.Add(spawnPoints[j]);

                    players.Add(player);
                    disableOnEnd.Add(player);

                    CharacterManager pm = player.GetComponent<CharacterManager>();
                    pm.LoadPlayerSettings(settings);
                    pm.playerIndexInTeam = j;
                    
                    if (settings.controlls == Controlls.AI)
                    {
                        player.GetComponent<AIController>().botDifficultyPreset = settings.difficultyPreset;
                    }
                    break;
                }
            }
        }

        Destroy(playerSpawnSettings.gameObject);
    }

    public void Countdown()
    {
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        started = false;
        
        countdownText.gameObject.SetActive(true);
        
        yield return new WaitForSecondsRealtime(0.1f);

        for(int i = countdownTime; i >= 0; i--)
        {
            countdownText.text = "" + i;

            countdownText.transform.localScale = Vector3.one * 2f;
            countdownText.transform.DOScale(Vector3.one * 0.03f, 1f).SetUpdate(true);
            yield return new WaitForSecondsRealtime(1.1f);
        }

        ball.SetActive(true);
        started = true;

        countdownText.gameObject.SetActive(false);
        pauseMenu.Hide(false);

    }

    public void End()
    {
        transitionAnim.SetTrigger("End");

        EndScreen endScreen = (winner == Team.None) ? drawScreen : winScreen;
        StartCoroutine(Ending(endScreen));
    }

    IEnumerator Ending(EndScreen endScreen)
    {
        yield return new WaitForSeconds(1.5f);

        Camera.main.gameObject.SetActive(false);
        endCamera.gameObject.SetActive(true);

        endScreenParent = endScreen.gameObject; 

        endScreen.gameObject.SetActive(true);
        endScreen.SetupUI(winner, ScoreManager.instance.blueScore, ScoreManager.instance.redScore);
        endScreen.LoadPlayers(players, winner);

        foreach (GameObject g in disableOnEnd)
        {
            g.SetActive(false);
        }

        yield return new WaitForSeconds(0.1f);

        transitionAnim.SetTrigger("Start");
    }

    public void Score()
    {
        transitionAnim.SetTrigger("End");
        
        StartCoroutine(ShowScore());
    }

    IEnumerator ShowScore()
    {
        yield return new WaitForSeconds(1.5f);

        Camera.main.gameObject.SetActive(false);
        endCamera.gameObject.SetActive(true);

        endScreenParent.SetActive(false);
        levelScoreUI.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        transitionAnim.SetTrigger("Start");
    }
    
    private void Update()
    {
        if(pauseMenu.paused)
        {
            paused = true;
        } else
        {
            paused = false;
        }
    }

    public void ResetGame(Transform ball)
    {
        //Reset players
        for (int i = 0; i < players.Count; i++)
        {
            RespawnPlayer(i);
        }
        //Reset ball
        ResetBall(ball);
    }

    public void ResetBall(Transform ball)
    {
        ball.gameObject.SetActive(false);
        ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        ball.transform.position = new Vector3(0, 40, 0);
        ball.GetComponent<TrailRenderer>().Clear();

        ball.gameObject.SetActive(true);
    }

    public void RespawnPlayer(int playerIndex, float time = 0f)
    {
        StartCoroutine(Respawn(playerIndex, time));
    }

    IEnumerator Respawn(int playerIndex, float time)
    {
        yield return new WaitForSeconds(time);
        CharacterManager player = players[playerIndex].GetComponent<CharacterManager>();
        Transform[] spawnPoints = (player.team == Team.Blue) ? blueSpawnPoints : redSpawnPoints;
        player.transform.position = spawnPoints[player.playerIndexInTeam].position;
    }
}

public enum GameMode {
    Story,
    Local
}
