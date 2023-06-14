using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    public GameObject pacman;

    public GameObject leftWarpNode;
    public GameObject rightWarpNode;

    public AudioSource siren;
    public AudioSource munch1;
    public AudioSource munch2;
    public AudioSource powerPelletAudio;
    public AudioSource respawningAudio;
    public AudioSource ghostEatenAudio;

    public int currentMunch = 0;

    public int score;
    public Text scoreText;

    public GameObject ghostNodeLeft;
    public GameObject ghostNodeRight;
    public GameObject ghostNodeCenter;
    public GameObject ghostNodeStart;

    public GameObject redGhost;
    public GameObject pinkGhost;
    public GameObject blueGhost;
    public GameObject orangeGhost;

    public EnemyController redGhostController;
    public EnemyController pinkGhostController;
    public EnemyController blueGhostController;
    public EnemyController orangeGhostController;

    public int totalPellets;
    public int pelletLeft;
    public int pelletCollectedOnThisLife;

    public bool hadDeathOnThisLevel = false;

    public bool gameIsRunning = true;

    public List<NodeController> nodeControllers = new List<NodeController>();

    public bool newGame;
    public bool clearLevel;


    public AudioSource startGameAudio;
    public AudioSource death;

    public int lives;
    public int currentLevel;

    public Image blackBackground;

    public Text gameOverText;
    public Text livesText;

    public bool isPowerPelletRunning = false;
    public float currentPowerPelletTime = 0;
    public float powerPelletTimer = 8f;
    public int powerPelletMultiplyer = 1;



    public enum GhostModeEnum
    {
        chase,
        scatter
    }

    public GhostModeEnum currentGhostMode;


    public int[] ghostModeTimers = new int[] { 7, 20, 7, 20, 5, 20, 5 };
    public int ghostModeTimerIndex;
    public float ghostModeTimer = 0;
    public bool runningTimer;
    public bool completedTimer;




    // Start is called before the first frame update
    void Awake()
    {
        newGame = true;
        clearLevel = false;
        blackBackground.enabled = false;

        redGhostController = redGhost.GetComponent<EnemyController>();
        pinkGhostController = pinkGhost.GetComponent<EnemyController>();
        blueGhostController = blueGhost.GetComponent<EnemyController>();
        orangeGhostController = orangeGhost.GetComponent<EnemyController>();


        ghostNodeStart.GetComponent<NodeController>().isGhostStartingNode = true;

        pacman = GameObject.Find("Player");
        StartCoroutine(Setup());


    }


    public IEnumerator Setup()
    {

        gameOverText.enabled = false;
        ghostModeTimerIndex = 0;
        ghostModeTimer = 0;
        completedTimer = false;
        runningTimer = true;

        //プレーヤーがレベルをクリアしたとき、背景が表し、ゲームを0.1秒休止させる
        if (clearLevel)
        {
            blackBackground.enabled = true;

            yield return new WaitForSeconds(0.1f);

        }
        blackBackground.enabled = false;


        pelletCollectedOnThisLife = 0;
        currentGhostMode = GhostModeEnum.scatter;
        gameIsRunning = false;
        currentMunch = 0;

        float waitTimer = 1f;



        if (newGame || clearLevel)
        {

            pelletLeft = totalPellets;
            waitTimer = 4f;

            //新たなゲーム、または今のレベルをクリアしたとき、豆が全部復活
            for (int i = 0; i < nodeControllers.Count; i++)
            {
                nodeControllers[i].RespawnPellet();
            }

        }
       
        
        if (newGame)
        {
            startGameAudio.Play();
            score = 0;
            scoreText.text = "Score:" + score.ToString();
            SetLives(3);
            currentLevel = 1;
        }


        pacman.GetComponent<PlayerController>().Setup();

        redGhostController.Setup();
        pinkGhostController.Setup();
        blueGhostController.Setup();
        orangeGhostController.Setup();


        newGame = false;
        clearLevel = false;

        yield return new WaitForSeconds(waitTimer);

        StartGame();

    }

    void StartGame()
    {
        gameIsRunning = true;
        siren.Play();
    }

    void StopGame()
    {
        gameIsRunning = false;
        siren.Stop();
        powerPelletAudio.Stop();
        respawningAudio.Stop();
        pacman.GetComponent<PlayerController>().Stop();
    }
    void SetLives(int newLives)
    {
        lives = newLives;
        livesText.text = "Lives:" + lives;

    }


    // Update is called once per frame
    void Update()
    {
        if (!gameIsRunning)
        {

            return;
        }

        if(redGhostController.ghostNodeState == EnemyController.GhostNodeStatesEnum.respawning
            || pinkGhostController.ghostNodeState == EnemyController.GhostNodeStatesEnum.respawning
            || blueGhostController.ghostNodeState == EnemyController.GhostNodeStatesEnum.respawning
            || orangeGhostController.ghostNodeState == EnemyController.GhostNodeStatesEnum.respawning)
        {
            if (!respawningAudio.isPlaying)
            {
                respawningAudio.Play();
            }
        }
        else
        {
            if (respawningAudio.isPlaying)
            {
                respawningAudio.Stop();
            }

        }

        if(!completedTimer && runningTimer)
        {
            ghostModeTimer += Time.deltaTime;
            if(ghostModeTimer >= ghostModeTimers[ghostModeTimerIndex])
            {
                ghostModeTimer = 0;
                ghostModeTimerIndex++;
                if(currentGhostMode == GhostModeEnum.chase)
                {
                    currentGhostMode = GhostModeEnum.scatter;
                }
                else
                {
                    currentGhostMode = GhostModeEnum.chase;
                }

                if(ghostModeTimerIndex == ghostModeTimers.Length)
                {
                    completedTimer = true;
                    runningTimer = false;
                    currentGhostMode = GhostModeEnum.chase;
                }
            }
        }

        if (isPowerPelletRunning)
        {
            currentPowerPelletTime += Time.deltaTime;
            if (currentPowerPelletTime >= powerPelletTimer)
            {
                isPowerPelletRunning = false;
                currentPowerPelletTime = 0;
                powerPelletAudio.Stop();
                siren.Play();
                powerPelletMultiplyer = 1;
            }
        }
    }


    public void GotPelletFromNodeController(NodeController nodeController)
    {
        nodeControllers.Add(nodeController);
        totalPellets++;
        pelletLeft++;
    }

    public void AddToScore(int amount)
    {
        score += amount;
        scoreText.text = "Score: " + score.ToString();

    }

    public IEnumerator CollectedPellet(NodeController nodeController)
    {
        if(currentMunch == 0)
        {
            munch1.Play();
            currentMunch = 1;
        }
        else if(currentMunch == 1)
        {
            munch2.Play();
            currentMunch = 0;
        }

        pelletLeft--;
        pelletCollectedOnThisLife++;

        int requiredBluePellets = 0;
        int requiredOrangePellets = 0;

        if (hadDeathOnThisLevel)
        {
            requiredBluePellets = 12;
            requiredOrangePellets = 32;
        }
        else
        {
            requiredBluePellets = 30;
            requiredOrangePellets = 60;
        }

        if(pelletCollectedOnThisLife >= requiredBluePellets && !blueGhost.GetComponent<EnemyController>().leftHomeBefore)
        {

            blueGhost.GetComponent<EnemyController>().readyToLeaveHome = true;

        }

        if (pelletCollectedOnThisLife >= requiredOrangePellets && !orangeGhost.GetComponent<EnemyController>().leftHomeBefore)
        {

            orangeGhost.GetComponent<EnemyController>().readyToLeaveHome = true;

        }


        AddToScore(100);



        //豆がまだあるかどうか
        if(pelletLeft == 0)
        {
            currentLevel++;
            clearLevel = true;
            StopGame();
            yield return new WaitForSeconds(1);
            StartCoroutine(Setup());
        }

        //パワーペレット
        if (nodeController.isPowerPellet)
        {
            siren.Stop();
            powerPelletAudio.Play();
            isPowerPelletRunning = true;
            currentPowerPelletTime = 0;

            redGhostController.SetFrightened(true);
            pinkGhostController.SetFrightened(true);
            blueGhostController.SetFrightened(true);
            orangeGhostController.SetFrightened(true);

        }
    }

    public IEnumerator PauseGame(float timeToPause)
    {
        gameIsRunning = false;

        yield return new WaitForSeconds(timeToPause);

        gameIsRunning = true;

    }


    public void GhostEaten()
    {
        ghostEatenAudio.Play();
        AddToScore(1000 * powerPelletMultiplyer);
        powerPelletMultiplyer++;
        StartCoroutine(PauseGame(1));
    }


    public IEnumerator PlayerEaten()
    {
        hadDeathOnThisLevel = true;
        StopGame();
        yield return new WaitForSeconds(1);

        redGhostController.SetVisible(false);
        pinkGhostController.SetVisible(false);
        blueGhostController.SetVisible(false);
        orangeGhostController.SetVisible(false);

        pacman.GetComponent<PlayerController>().Death();
        death.Play();
        yield return new WaitForSeconds(3);

        SetLives(lives -1 );
        if (lives <= 0)
        {
            newGame = true;

            gameOverText.enabled = true;

            yield return new WaitForSeconds(3);
        }

        StartCoroutine(Setup());

    }
}
