using UnityEngine;
using UnityEngine.InputSystem;

enum SackGameState
{
    WaitingForInput,
    Jumping,
    Falling
}

public class SackRaceGame : MinigameController
{
    [Header("Reglas del juego")]
    [SerializeField] private int maxErrors = 3;
    private int errorCount = 0;

    [Header("Jugador")]
    [SerializeField] private Rigidbody2D player;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private Sprite idle, jumping, lose;

    [Header("Escenario")]
    [SerializeField] private Transform world;
    [SerializeField] private Transform goal;
    [SerializeField] private float worldMoveSpeed = 3f;

    [Header("Canvas")]
    [SerializeField] private GameObject leftButton;
    [SerializeField] private GameObject rightButton;
    private InputAction input;


    private bool waitingForLeft = true;
    private SackGameState playerState = SackGameState.WaitingForInput;

    private void Start() {
        leftButton.SetActive(true);
        rightButton.SetActive(false);
        input = InputSystem.actions.FindAction("Saco");
    }

    public override void StartGame()
    {
        base.StartGame();
        player.transform.SetPositionAndRotation(new Vector3(0, -3.5f, 0), Quaternion.identity);
        player.GetComponent<SpriteRenderer>().sprite = idle;
        world.position = new Vector3(0, world.position.y, world.position.z);
        waitingForLeft = true;
        errorCount = 0;
        leftButton.SetActive(true);
        rightButton.SetActive(false);
        playerState = SackGameState.WaitingForInput;
    }

    private void Update()
    {
        HandleWorldMovement();
        if (_currentState == State.End)
            return;

        HandleInput();
        CheckGoalReached();
    }

    private void FixedUpdate()
    {
        CheckPhysics();
    }

    private void ChangeState(SackGameState newState)
    {
        playerState = newState;
        switch (newState)
        {
            case SackGameState.WaitingForInput:
                if (_currentState != State.End)
                {
                    EnableButton(waitingForLeft ? leftButton : rightButton);
                }
                player.GetComponent<SpriteRenderer>().sprite = idle;
                break;
            case SackGameState.Jumping:
                player.linearVelocity = Vector2.zero;
                player.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                DisableButtons();
                waitingForLeft = !waitingForLeft;
                player.GetComponent<SpriteRenderer>().sprite = jumping;
                break;
            case SackGameState.Falling:
                break;
        }
    }

    private void HandleInput()
    {
        if (playerState != SackGameState.WaitingForInput)
            return;

        if (IsCorrectKeyPressed())
        {
            ChangeState(SackGameState.Jumping);

        }
        else if (Input.anyKeyDown)
        {
            RegisterError();
        }
    }

    private void EnableButton(GameObject button)
    {
        button.SetActive(true);
    }

    private void DisableButtons()
    {
        leftButton.SetActive(false);
        rightButton.SetActive(false);
    }

    private bool IsCorrectKeyPressed()
    {
        return (waitingForLeft && input.GetAxisDown() < 0) ||
               (!waitingForLeft && input.GetAxisDown() > 0);
    }

    private void RegisterError()
    {
        errorCount++;
        if (errorCount >= maxErrors)
        {
            DisableButtons();
            Lose();
            player.GetComponent<SpriteRenderer>().sprite = lose;
        }
    }

    private void CheckGoalReached()
    {
        if (player.transform.position.x >= goal.position.x)
        {
            Won();
            DisableButtons();
        }
    }

    private void CheckPhysics()
    {
        float speed = player.linearVelocity.magnitude;
        if (speed < 0.1f && playerState == SackGameState.Jumping)
        {
            ChangeState(SackGameState.WaitingForInput);
        }
        else if (speed < 0.1f && playerState == SackGameState.Falling)
        {
            ChangeState(SackGameState.WaitingForInput);
        }
        else if (player.linearVelocity.y < 0 && playerState == SackGameState.Jumping)
        {
            ChangeState(SackGameState.Falling);
        }
    }


    private void HandleWorldMovement()
    {
        if (playerState != SackGameState.WaitingForInput)
        {
            world.position += Time.deltaTime * worldMoveSpeed * Vector3.left;
        }
    }
}
