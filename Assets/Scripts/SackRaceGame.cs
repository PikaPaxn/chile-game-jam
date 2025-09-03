using UnityEngine;

public class SackRaceGame : MinigameController
{
    [Header("Controles")]
    [SerializeField] private KeyCode leftKey = KeyCode.A;
    [SerializeField] private KeyCode rightKey = KeyCode.D;

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

    private bool waitingForLeft = true;

    private void Update()
    {
        if (_currentState == State.End)
            return;

        HandleInput();
        HandleWorldMovement();
        CheckGoalReached();
    }


    private void HandleInput()
    {
        if (IsCorrectKeyPressed() && IsGrounded())
        {
            Jump();
            waitingForLeft = !waitingForLeft;
        }
        else if (Input.anyKeyDown)
        {
            RegisterError();
        }
    }

    private bool IsCorrectKeyPressed()
    {
        return (waitingForLeft && Input.GetKeyDown(leftKey)) ||
               (!waitingForLeft && Input.GetKeyDown(rightKey));
    }

    private void RegisterError()
    {
        errorCount++;
        if (errorCount >= maxErrors)
        {
            Lose();
        }
    }

    private void Jump()
    {
        player.linearVelocity = Vector2.zero;
        player.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }


    private void CheckGoalReached()
    {
        if (player.transform.position.x >= goal.position.x)
        {
            Won();
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.Raycast(player.position, Vector2.down, 2.85f).collider != null;
    }

    private void HandleWorldMovement()
    {
        if (!IsGrounded())
        {
            world.position += Time.deltaTime * worldMoveSpeed * Vector3.left;
        }
    }

    void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(player.position, player.position + Vector2.down * 2.85f);
        }
    }
}
