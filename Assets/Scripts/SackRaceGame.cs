using UnityEngine;

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

    [Header("Escenario")]
    [SerializeField] private Transform world;
    [SerializeField] private Transform goal;
    [SerializeField] private float worldMoveSpeed = 3f;

    [Header("Canvas")]
    [SerializeField] private GameObject leftButton;
    [SerializeField] private GameObject rightButton;

    private bool waitingForLeft = true;
    private SackGameState playerState = SackGameState.WaitingForInput;

    private void Start()
    {
        leftButton.SetActive(true);
        rightButton.SetActive(false);
    }

    public override void StartGame()
    {
        base.StartGame();
        player.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        world.position = Vector3.zero;
        waitingForLeft = true;
        errorCount = 0;
        leftButton.SetActive(true);
        rightButton.SetActive(false);
        playerState = SackGameState.WaitingForInput;
    }

    private void Update()
    {
        if (_currentState == State.End)
            return;

        HandleInput();
        HandleWorldMovement();
        CheckGoalReached();
    }

    private void FixedUpdate()
    {
        if (_currentState == State.End)
            return;
        CheckPhysics();
    }

    private void ChangeState(SackGameState newState)
    {
        playerState = newState;
        switch (newState)
        {
            case SackGameState.WaitingForInput:
                EnableButton(waitingForLeft ? leftButton : rightButton);
                break;
            case SackGameState.Jumping:
                player.linearVelocity = Vector2.zero;
                player.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                DisableButtons();
                waitingForLeft = !waitingForLeft;
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
        return (waitingForLeft && Input.GetButtonDown("DirectionLeft")) ||
               (!waitingForLeft && Input.GetButtonDown("DirectionRight"));
    }

    private void RegisterError()
    {
        errorCount++;
        if (errorCount >= maxErrors)
        {
            DisableButtons();
            Lose();
            player.transform.Rotate(Vector3.forward, 90);
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
