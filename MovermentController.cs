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

        transform.position = Vector2.MoveTowards(transform.position, currentNode.transform.position, speed * Time.deltaTime); // �ꥢ�륿����˺Ϥ碌��

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

        //���ޤ�����Ȥ����F�ڤΥΩ`�ɤ�����Фˤ��뤫�ɤ�����_�J����
        if((transform.position.x == currentNode.transform.position.x && transform.position.y == currentNode.transform.position.y) || reverseDirection)
        {
            if (isGhost)
            {
                GetComponent<EnemyController>().ReachedCenterofNode(currentNodeController);
            }
            //һ���¤����ޤ����顢�Ҥ���ܞ����
            if (currentNodeController.isWarpLeftNode && canWarp)
            {
                currentNode = gameManager.rightWarpNode;
                direction = "left";
                lastMovingDirection = "left";
                transform.position = currentNode.transform.position;
                canWarp = false;

            }
            //һ���¤����ޤ����顢�����ܞ����
            else if(currentNodeController.isWarpRightNode && canWarp)
            {
                currentNode = gameManager.leftWarpNode;
                direction = "right";
                lastMovingDirection = "right";
                transform.position = currentNode.transform.position;
                canWarp = false;

            }
            //���Έ��ϡ��ΤΥΩ`�ɤ�̽�����ƄӤ���
            else
            {
                //���`���ȤǤϤʤ��������`�ȥΩ`�ɤˤ����H�ˡ�����ˡ��¤��ƄӤ������Ȥ���ֹ�ޤ�
                if(currentNodeController.isGhostStartingNode && direction == "down" && 
                    (!isGhost || GetComponent<EnemyController>().ghostNodeState != EnemyController.GhostNodeStatesEnum.respawning))
                {
                    direction = lastMovingDirection;
                }

                //���λ�ä���Τ��ƄӤǤ���Ω`�ɤ��{�٤�
                GameObject newNode = currentNodeController.GetNodeFromDirection(direction);
                if (newNode != null)
                {
                    currentNode = newNode;
                    lastMovingDirection = direction;
                }
                // �ƄӤ������Ȥ�����ƄӤǤ��ʤ����Ϥˡ����ä���ͬ��������ƄӤ��A����
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
