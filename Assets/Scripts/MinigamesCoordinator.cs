using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinigamesCoordinator : MonoBehaviour
{

    private static readonly WaitForSeconds _waitForSeconds1 = new(1f);
    [Header("Minigames List")]
    public MinigameController[] minigames;
    MinigameController _currentMinigameType;
    MinigameController _currentMinigame;

    [Header("UI Refs")]
    public Slider timeLeftSlider;
    Animator _timeSliderAnimator;
    public GameObject wonGO;
    public GameObject loseGO;
    public TextMeshProUGUI instructions;

    enum CoordinatorStates { Idle, WaitingForGame, PlayingGame }
    CoordinatorStates _currentState;
    float _stateChangedTime;

    [Header("Transitions")]
    public GameObject[] transitions;


    CoordinatorStates CurrentState
    {
        get { return _currentState; }
        set
        {
            _stateChangedTime = Time.time;
            _currentState = value;
        }
    }

    void Start()
    {
        ResetUI();
        CurrentState = CoordinatorStates.Idle;
        _timeSliderAnimator = timeLeftSlider.GetComponent<Animator>();
    }

    void Update()
    {
        switch (CurrentState)
        {
            case CoordinatorStates.Idle: IdleUpdate(); break;
            case CoordinatorStates.WaitingForGame: WaitingForGameUpdate(); break;
            case CoordinatorStates.PlayingGame: PlayingGameUpdate(); break;
        }
    }

    void IdleUpdate()
    {
        if (Time.time - _stateChangedTime >= 2f)
        {
            ResetUI();

            StartCoroutine(StartMinigameWithAnimation());
            CurrentState = CoordinatorStates.WaitingForGame;
        }
    }

    void WaitingForGameUpdate()
    {

    }

    void PlayingGameUpdate()
    {
        if (_currentMinigame.IsPaused)
            return;

        // Update time
        bool timeDone = false;
        if (_currentMinigame.UseTime)
        {
            var timeLeft = _currentMinigame.TimeLeft01();
            timeLeftSlider.value = timeLeft;
            timeDone = timeLeft <= 0;
        }

        // Check if won
        if (_currentMinigame.HasWon)
        {
            wonGO.SetActive(true);
            CurrentState = CoordinatorStates.Idle;
        }

        // Check if lose
        if (!_currentMinigame.IsPlaying || timeDone)
        {
            Debug.Log($"Did you won? {_currentMinigame.HasWon}");

            // Call the callback
            if (timeDone)
            {
                _currentMinigame.TimeEnded();
            }

            // Check if won
            if (_currentMinigame.HasWon)
            {
                wonGO.SetActive(true);
            }
            else
            {
                loseGO.SetActive(true);
            }

            CurrentState = CoordinatorStates.Idle;
        }
    }

    IEnumerator StartMinigameWithAnimation()
    {
        // Play transition
        if (transitions.Length > 0)
        {
            var transition = transitions.RandomPick();
            transition.SetActive(true);
        }
        yield return _waitForSeconds1;

        ChooseMinigame();
        StartChoosenMinigame();
        if (instructions)
        {
            instructions.gameObject.SetActive(true);
            instructions.text = _currentMinigame.instructions != "" ? _currentMinigame.instructions : "Preparate!";
            yield return _waitForSeconds1;
            instructions.text = "";
        }
        else
        {
            Debug.LogWarning("No instructions text assigned in MinigamesCoordinator");
        }

    }

    void ChooseMinigame()
    {
        // Get random game
        var minigame = minigames.RandomPick(_currentMinigameType);

        // If is not in the scene, instanciate it
        if (_currentMinigameType != minigame || !_currentMinigame.IsInstanced)
        {
            if (_currentMinigame != null)
            {
                Destroy(_currentMinigame.gameObject);
            }
            _currentMinigameType = minigame;
            _currentMinigame = Instantiate(minigame);
        }
    }

    void StartChoosenMinigame()
    {
        // Start a new minigame
        _currentMinigame.StartGame();
        CurrentState = CoordinatorStates.PlayingGame;
        ResetUI();
        if (_timeSliderAnimator) {
            _timeSliderAnimator.SetBool("UsesTime", _currentMinigame.UseTime);
        }
    }

    // Didn't change for compatibility with current testing
    public void StartMinigame()
    {
        ChooseMinigame();
        StartChoosenMinigame();
    }

    void ResetUI()
    {
        wonGO.SetActive(false);
        loseGO.SetActive(false);
    }

    internal void PauseMinigame(bool pause) {
        _currentMinigame.Pausing(pause);
    }
}
