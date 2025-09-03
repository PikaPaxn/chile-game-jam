using UnityEngine;

public class MinigameController : MonoBehaviour
{
    [Header("General Minigame Config")]
    [Tooltip("How long should the minigame be, in seconds")]
    public float timeLimit;

    // State of the game
    protected enum State { Idle, Playing, End }
    protected State _currentState = State.Idle;

    // Manage if the game has ended
    float _startTime;
    bool _hasWon;

    /// <summary>
    /// When overriding remember to call base.StartGame() to preserve the time logic
    /// </summary>
    public virtual void StartGame() {
        _hasWon = false;
        _startTime = Time.time;
        _currentState = State.Playing;
    }

    /// <summary>
    /// Call it so the coordinator knows you won the minigame
    /// </summary>
    public void Won() {
        Debug.Log("You won the Minigame!", gameObject);
        _hasWon = true;
        _currentState = State.End;
    }

    /// <summary>
    /// Call it if you can lose the minigame other than by time
    /// </summary>
    public void Lose() {
        Debug.Log("You lost the Minigame! :(", gameObject);
        _hasWon = false;
        _currentState = State.End;
    }

    public bool IsPlaying => _currentState == State.Playing;

    // Coordinator references
    public bool HasWon => _hasWon;
    public bool IsInstanced => gameObject.scene.name != null;
    public bool UseTime => timeLimit > 0;

    /// <summary>
    /// Returns how much time is left
    /// </summary>
    /// <returns>1 is all time is left, 0 is time has expired</returns>
    public float TimeLeft01() {
        float timePassed = Time.time - _startTime;
        return 1f - Mathf.InverseLerp(0, timeLimit, timePassed);
    }
}