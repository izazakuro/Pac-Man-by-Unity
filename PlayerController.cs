using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    MovermentController movementcontroller;

    public SpriteRenderer sprite;

    public Animator animator;

    public GameObject startNode;

    public Vector2 startPosition;

    public GameManager gameManager;

    public bool isDead = false;

    // Start is called before the first frame update
    void Awake()
    {

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        startPosition = new Vector2(-0.03f, -0.626f);
        animator = GetComponentInChildren<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        movementcontroller = GetComponent< MovermentController > ();

        startNode = movementcontroller.currentNode;

    }

    public void Setup()
    {
        isDead = false;
        movementcontroller.SetSpeed(3);
        animator.SetBool("dead", false);
        animator.SetBool("moving", false);
        movementcontroller.currentNode = startNode;
        movementcontroller.direction = "left";
        movementcontroller.lastMovingDirection = "left";
        sprite.flipX = false;
        transform.position = startPosition;
        animator.speed = 3;
        
    }

    public void Stop()
    {
        animator.speed = 0;
    }



    // Update is called once per frame
    void Update()
    {

        if (!gameManager.gameIsRunning)
        {
            if (!isDead)
            {
                animator.speed = 0;
            }
            
            return;
        }

        animator.speed = 1;


        animator.SetBool("moving", true);
        if (Input.GetKey(KeyCode.A))
        {
            movementcontroller.SetDirection("left");
        }
        if (Input.GetKey(KeyCode.D))
        {
            movementcontroller.SetDirection("right");

        }
        if (Input.GetKey(KeyCode.W))
        {
            movementcontroller.SetDirection("up");

        }
        if (Input.GetKey(KeyCode.S))
        {
            movementcontroller.SetDirection("down");
        }

        bool flipX = false;
        bool flipY = false;
        if (movementcontroller.lastMovingDirection == "left")
        {
            animator.SetInteger("direction", 0);
        }
        else if (movementcontroller.lastMovingDirection == "right")
        {
            animator.SetInteger("direction", 0);
            flipX = true;

        }
        else if (movementcontroller.lastMovingDirection == "up")
        {
            animator.SetInteger("direction", 1);

        }
        else if (movementcontroller.lastMovingDirection == "down")
        {
            animator.SetInteger("direction", 1);
            flipY = true;
        }

        sprite.flipY = flipY;
        sprite.flipX = flipX;

    }

    public void Death()
    {
        isDead = true;
        animator.SetBool("moving", false);
        animator.speed = 1;
        animator.SetBool("dead", true);
    }
}
