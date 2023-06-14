using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovermentController : MonoBehaviour
{

    public GameManager gameManager;

    public GameObject currentNode;
    public float speed = 2.5f;

    public string direction = "";
    public string lastMovingDirection = "";

    public bool canWarp = true;

    public bool isGhost = false;


    // Start is called before the first frame update
    private void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.gameIsRunning)
        {
            return;
        }


        NodeController currentNodeController = currentNode.GetComponent<NodeController>();

        transform.position = Vector2.MoveTowards(transform.position, currentNode.transform.position, speed * Time.deltaTime); // リアルタイムに合わせる

        bool reverseDirection = false;
        if (
            (direction == "left" && lastMovingDirection == "right") ||
            (direction == "right" && lastMovingDirection == "left") ||
            (direction == "up" && lastMovingDirection == "down") ||
            (direction  == "down" && lastMovingDirection == "up")
            )
            {
            reverseDirection = true;
        }

        //いまがいるところが現在のノードの真ん中にいるかどうかを確認する
        if((transform.position.x == currentNode.transform.position.x && transform.position.y == currentNode.transform.position.y) || reverseDirection)
        {
            if (isGhost)
            {
                GetComponent<EnemyController>().ReachedCenterofNode(currentNodeController);
            }
            //一番奥に入りましたら、右に逆転する
            if (currentNodeController.isWarpLeftNode && canWarp)
            {
                currentNode = gameManager.rightWarpNode;
                direction = "left";
                lastMovingDirection = "left";
                transform.position = currentNode.transform.position;
                canWarp = false;

            }
            //一番奥に入りましたら、左に逆転する
            else if(currentNodeController.isWarpRightNode && canWarp)
            {
                currentNode = gameManager.leftWarpNode;
                direction = "right";
                lastMovingDirection = "right";
                transform.position = currentNode.transform.position;
                canWarp = false;

            }
            //他の場合、次のノードを探し、移動する
            else
            {
                //ゴーストではない、スタートノードにいる際に、さらに、下に移動したいとき、止まる
                if(currentNodeController.isGhostStartingNode && direction == "down" && 
                    (!isGhost || GetComponent<EnemyController>().ghostNodeState != EnemyController.GhostNodeStatesEnum.respawning))
                {
                    direction = lastMovingDirection;
                }

                //今の位置から次に移動できるノードを調べる
                GameObject newNode = currentNodeController.GetNodeFromDirection(direction);
                if (newNode != null)
                {
                    currentNode = newNode;
                    lastMovingDirection = direction;
                }
                // 移動したいところへ移動できない場合に、さっきと同じ方向で移動し続ける
                else
                {
                    direction = lastMovingDirection;
                    newNode = currentNodeController.GetNodeFromDirection(direction);
                    if (newNode != null)
                    {
                        currentNode = newNode;

                    }
                }
            }
           
        }
        else
        {

            canWarp = true;

        }
    }


    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void SetDirection(string newDirection)
    {
        direction = newDirection;

    }
}
