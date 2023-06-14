using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    public enum GhostNodeStatesEnum
    {
        respawning,
        leftNode,
        rightNode,
        centerNode,
        startNode,
        movingInNodes
    }

    public GhostNodeStatesEnum ghostNodeState;

    public enum GhostTypeEnum
    {
        red,
        blue,
        pink,
        orange
    }

    public GhostTypeEnum ghostType;
    public GhostNodeStatesEnum respawnState;
    public GhostNodeStatesEnum startGhostNodeState;

    public GameObject ghostNodeLeft;
    public GameObject ghostNodeRight;
    public GameObject ghostNodeCenter;
    public GameObject ghostNodeStart;

    public MovermentController movermentController;

    public GameObject startingNode;

    public bool readyToLeaveHome = false;

    public GameManager gameManager;

    public bool testRespawn = false;

    public bool isFrightened = false;
    public bool leftHomeBefore = false;

    public bool isVisible = true;

    public GameObject[] scatterNodes;
    public int scatterNodeIndex;

    public SpriteRenderer ghostSprite;
    public SpriteRenderer eyesSprite;

    public Animator animator;

    public Color color;





    // Start is called before the first frame update
    void Awake()
    {
        animator = GetComponent<Animator>();

        ghostSprite = GetComponent<SpriteRenderer>();

        scatterNodeIndex = 0;

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        movermentController = GetComponent<MovermentController>();

        

        if (ghostType == GhostTypeEnum.red)
        {

            startGhostNodeState = GhostNodeStatesEnum.startNode;
            respawnState = GhostNodeStatesEnum.centerNode;
            startingNode = ghostNodeStart;

        }
        else if (ghostType == GhostTypeEnum.pink)
        {

            startGhostNodeState = GhostNodeStatesEnum.centerNode;
            startingNode = ghostNodeCenter;
            respawnState = GhostNodeStatesEnum.centerNode;

        }
        else if (ghostType == GhostTypeEnum.blue)
        {
            startGhostNodeState = GhostNodeStatesEnum.leftNode;
            startingNode = ghostNodeLeft;
            respawnState = GhostNodeStatesEnum.leftNode;

        }
        else if (ghostType == GhostTypeEnum.orange)
        {

            startGhostNodeState = GhostNodeStatesEnum.rightNode;
            startingNode = ghostNodeRight;
            respawnState = GhostNodeStatesEnum.rightNode;

        }

    }


    public void Setup()
    {



        ghostNodeState = startGhostNodeState;
        readyToLeaveHome = false;
        SetVisible(true);
        //ゴーストの位置を初期化する
        movermentController.currentNode = startingNode;
        transform.position = startingNode.transform.position;

        movermentController.direction = "";
        movermentController.lastMovingDirection = "";

        //追い散らすの範囲を0に初期化する
        scatterNodeIndex = 0;


        //Set isFrightened
        isFrightened = false;

        leftHomeBefore = false;

        //家から出る準備の初期化(blue & pink)
        if (ghostType == GhostTypeEnum.red)
        {
            readyToLeaveHome = true;
            leftHomeBefore = true;
        }
        else if (ghostType == GhostTypeEnum.pink)
        {
            readyToLeaveHome = true;
        }

        animator.SetBool("moving", false);

    }

    // Update is called once per frame
    void Update()
    {
        if(ghostNodeState != GhostNodeStatesEnum.movingInNodes || !gameManager.isPowerPelletRunning)
        {
            isFrightened = false;
        }

        if (isVisible)
        {
            if(ghostNodeState != GhostNodeStatesEnum.respawning)
            {
                ghostSprite.enabled = true;
            }
            else
            {
                ghostSprite.enabled = false;
            }
            
            eyesSprite.enabled = true;
        }
        else
        {
            ghostSprite.enabled = false;
            eyesSprite.enabled = false;
        }

        if (isFrightened)
        {
            animator.SetBool("frightened", true);
            eyesSprite.enabled = false;
            ghostSprite.color = new Color(255, 255, 255, 255);
        }
        else
        {
            animator.SetBool("frightened", false);
            animator.SetBool("frightenedBlinking", false);
            ghostSprite.color = color;
        }


        if (!gameManager.gameIsRunning)
        {
            return;
        }

        if(gameManager.powerPelletTimer - gameManager.currentPowerPelletTime <= 3)
        {
            animator.SetBool("frightenedBlinking", true);
        }
        else
        {
            animator.SetBool("frightenedBlinking", false);
        }

        
        animator.SetBool("moving", true);

        if (testRespawn == true)
        {
            readyToLeaveHome = false;
            ghostNodeState = GhostNodeStatesEnum.respawning;
            testRespawn = false;
        }

        if (movermentController.currentNode.GetComponent<NodeController>().isSideNode)
        {
            movermentController.SetSpeed(1);
        }
        else
        {
            if (isFrightened)
            {
                movermentController.SetSpeed(1);
            }
            else if (ghostNodeState == GhostNodeStatesEnum.respawning)
            {
                movermentController.SetSpeed(7);
            }
            else
            {
                movermentController.SetSpeed(2.5f);
            }
        }

    }

    public void SetFrightened(bool newIsFrightened)
    {
        isFrightened = newIsFrightened;
    }

    public void ReachedCenterofNode(NodeController nodeController)
    {

        if (ghostNodeState == GhostNodeStatesEnum.movingInNodes)
        {

            leftHomeBefore = true;

            //Scatter mode
            if (gameManager.currentGhostMode == GameManager.GhostModeEnum.scatter)
            {

                DetermineScatterModeDirection();


            }
            //Fightened mode
            else if (isFrightened)
            {

                string direction = GetRandomDirection();
                movermentController.SetDirection(direction);

            }
            //Chase mode
            else
            {
                //次の遷移先を決める
                if (ghostType == GhostTypeEnum.red)
                {
                    DetermineRedGhostDirection();
                }
                else if (ghostType == GhostTypeEnum.pink)
                {
                    DeterminePinkGhostDirection();
                }
                else if (ghostType == GhostTypeEnum.blue)
                {
                    DetermineBlueGhostDirection();
                }
                else if (ghostType == GhostTypeEnum.orange)
                {
                    DetermineOrangeGhostDirection();
                }
            }


        }
        else if (ghostNodeState == GhostNodeStatesEnum.respawning)
        {

            string direction = "";

            if (transform.position == ghostNodeStart.transform.position)
            {
                direction = "down";
            }
            else if (transform.position == ghostNodeCenter.transform.position)
            {
                if (respawnState == GhostNodeStatesEnum.centerNode)
                {
                    if (respawnState == GhostNodeStatesEnum.centerNode)
                    {
                        ghostNodeState = respawnState;
                    }
                    else if (respawnState == GhostNodeStatesEnum.leftNode)
                    {
                        direction = "left";
                    }
                    else if (respawnState == GhostNodeStatesEnum.rightNode)
                    {
                        direction = "right";
                    }
                }
            }
            else if ((transform.position == ghostNodeLeft.transform.position) ||
                (transform.position == ghostNodeRight.transform.position))
            {
                ghostNodeState = respawnState;
            }
            else
            {
                //一番早い帰り道を探す
                direction = GetClosestDirection(ghostNodeStart.transform.position);
            }



            movermentController.SetDirection(direction);
        }
        else
        {
            //家から出る準備ができたら
            if (readyToLeaveHome)
            {
                //真ん中に移動する
                if (ghostNodeState == GhostNodeStatesEnum.leftNode)
                {
                    ghostNodeState = GhostNodeStatesEnum.centerNode;
                    movermentController.SetDirection("right");
                }
                else if (ghostNodeState == GhostNodeStatesEnum.rightNode)
                {
                    ghostNodeState = GhostNodeStatesEnum.centerNode;
                    movermentController.SetDirection("left");
                }
                //出発点に移動する
                else if (ghostNodeState == GhostNodeStatesEnum.centerNode)
                {
                    ghostNodeState = GhostNodeStatesEnum.startNode;
                    movermentController.SetDirection("up");
                }
                //ゲームの中で移動する
                else if (ghostNodeState == GhostNodeStatesEnum.startNode)
                {
                    ghostNodeState = GhostNodeStatesEnum.movingInNodes;
                    movermentController.SetDirection("left");
                }
            }
        }
    }


    string GetRandomDirection()
    {
        List<string> possilbleDirections = new List<string>();
        NodeController nodeController = movermentController.currentNode.GetComponent<NodeController>();

        if (nodeController.canMoveDown && movermentController.lastMovingDirection != "up")
        {
            possilbleDirections.Add("down");
        }
        if (nodeController.canMoveUp && movermentController.lastMovingDirection != "down")
        {
            possilbleDirections.Add("up");
        }
        if (nodeController.canMoveRight && movermentController.lastMovingDirection != "left")
        {
            possilbleDirections.Add("right");
        }
        if (nodeController.canMoveLeft && movermentController.lastMovingDirection != "right")
        {
            possilbleDirections.Add("left");
        }

        string direction = "";
        int randomDirectionIndex = Random.Range(0, possilbleDirections.Count - 1);
        direction = possilbleDirections[randomDirectionIndex];

        return direction;

    }



    void DetermineRedGhostDirection()
    {
        string direction = GetClosestDirection(gameManager.pacman.transform.position);
        movermentController.SetDirection(direction);

    }

    void DeterminePinkGhostDirection()
    {
        string pacmanDirection = gameManager.pacman.GetComponent<MovermentController>().lastMovingDirection;
        float distanceBetweenNodes = 0.33f;

        Vector2 target = gameManager.pacman.transform.position;
        if (pacmanDirection == "left")
        {
            target.x -= distanceBetweenNodes * 2;
        }
        else if (pacmanDirection == "right")
        {
            target.x += distanceBetweenNodes * 2;
        }
        else if (pacmanDirection == "up")
        {
            target.y += distanceBetweenNodes * 2;
        }
        else if (pacmanDirection == "down")
        {
            target.y -= distanceBetweenNodes * 2;
        }

        string direction = GetClosestDirection(target);
        movermentController.SetDirection(direction);
    }

    void DetermineBlueGhostDirection()
    {

        string pacmanDirection = gameManager.pacman.GetComponent<MovermentController>().lastMovingDirection;
        float distanceBetweenNodes = 0.33f;

        Vector2 target = gameManager.pacman.transform.position;
        if (pacmanDirection == "left")
        {
            target.x -= distanceBetweenNodes * 2;
        }
        else if (pacmanDirection == "right")
        {
            target.x += distanceBetweenNodes * 2;
        }
        else if (pacmanDirection == "up")
        {
            target.y += distanceBetweenNodes * 2;
        }
        else if (pacmanDirection == "down")
        {
            target.y -= distanceBetweenNodes * 2;
        }

        GameObject redGhost = gameManager.redGhost;
        float xDistance = target.x - redGhost.transform.position.x;
        float yDistance = target.y - redGhost.transform.position.y;

        Vector2 blueTarget = new Vector2(target.x + xDistance, target.y + yDistance);

        string direction = GetClosestDirection(blueTarget);
        movermentController.SetDirection(direction);

    }

    void DetermineOrangeGhostDirection()
    {
        float distance = Vector2.Distance(gameManager.pacman.transform.position, transform.position);
        float distanceBetweenNodes = 0.33f;

        if (distance < 0)
        {
            distance *= -1;
        }

        //直線距離は８以内の場合に、Redと同じくプレーヤーを追いかける
        if (distance <= distanceBetweenNodes * 8)
        {
            DetermineRedGhostDirection();
        }
        //他の場合に、追い散らす
        else
        {
            DetermineScatterModeDirection();
        }
    }

    string GetClosestDirection(Vector2 target)
    {

        float shortestDistance = 0;
        string lastMovingDirection = movermentController.lastMovingDirection;
        string newDirection = "";

        NodeController nodeController = movermentController.currentNode.GetComponent<NodeController>();

        if (nodeController.canMoveUp && lastMovingDirection != "down")
        {
            GameObject nodeup = nodeController.nodeUp;
            //Get distance with pacman
            float distance = Vector2.Distance(nodeup.transform.position, target);

            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "up";
            }
        }

        if (nodeController.canMoveDown && lastMovingDirection != "up")
        {
            GameObject nodeDown = nodeController.nodeDown;
            //Get distance with pacman
            float distance = Vector2.Distance(nodeDown.transform.position, target);

            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "down";
            }
        }

        if (nodeController.canMoveUp && lastMovingDirection != "down")
        {
            GameObject nodeup = nodeController.nodeUp;
            //Get distance with pacman
            float distance = Vector2.Distance(nodeup.transform.position, target);

            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "up";
            }
        }

        if (nodeController.canMoveLeft && lastMovingDirection != "right")
        {
            GameObject nodeLeft = nodeController.nodeLeft;
            //Get distance with pacman
            float distance = Vector2.Distance(nodeLeft.transform.position, target);

            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "left";
            }
        }

        if (nodeController.canMoveRight && lastMovingDirection != "left")
        {
            GameObject nodeRight = nodeController.nodeRight;
            //Get distance with pacman
            float distance = Vector2.Distance(nodeRight.transform.position, target);

            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "right";
            }
        }

        return newDirection;


    }

    public void SetVisible(bool newIsVisible)
    {
        isVisible = newIsVisible;
    }

    void DetermineScatterModeDirection()
    { 
        if (transform.position == scatterNodes[scatterNodeIndex].transform.position)
        {
            scatterNodeIndex++;

            if (scatterNodeIndex == scatterNodes.Length )
            {
                scatterNodeIndex = 0;
            }
        }
        string direction = GetClosestDirection(scatterNodes[scatterNodeIndex].transform.position);
        movermentController.SetDirection(direction);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player" && ghostNodeState != GhostNodeStatesEnum.respawning)
        {
            //食べられた
            if (isFrightened)
            {
                gameManager.GhostEaten();
                ghostNodeState = GhostNodeStatesEnum.respawning;

            }
            //プレーヤーを食べる
            else
            {
                StartCoroutine(gameManager.PlayerEaten());
            }
        }
    }


}
