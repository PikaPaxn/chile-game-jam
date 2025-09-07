using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;

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
    public GameObject gameOverUI;
    public GameObject kites;

    enum CoordinatorStates { Idle, WaitingForGame, PlayingGame }
    CoordinatorStates _currentState;
    float _stateChangedTime;

    [Header("Transitions")]
    public GameObject[] transitions;
    public int lives = 3;
    private bool _gameOverShown = false;
    public String[] loseTexts = { "Has perdido!", "Inténtalo de nuevo!", "Casi lo logras!" };

    [Header("Camera")]
    public CameraController cameraController;

    CoordinatorStates CurrentState
    {
        get { return _currentState; }
        set
        {
            _stateChangedTime = Time.time;
            _currentState = value;
        }
    }

    void Awake()
    {
        _timeSliderAnimator = timeLeftSlider.GetComponent<Animator>();
    }

    void Start()
    {
        ResetUI();
        CurrentState = CoordinatorStates.Idle;
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

    void PlayTransition()
    {
        if (transitions.Length > 0)
        {
            var transition = transitions.RandomPick();
            transition.SetActive(true);
        }
        else
        {
            Debug.LogWarning("No transitions assigned in MinigamesCoordinator");
        }
    }

    void IdleUpdate()
    {
        if (Time.time - _stateChangedTime >= 2f)
        {
            ResetUI();

            if (lives <= 0 && !_gameOverShown)
            {
                StartCoroutine(GameOverSequence());
                _gameOverShown = true;
                return;
            }
            else if (lives > 0)
            {
                StartCoroutine(StartMinigameWithAnimation());
                CurrentState = CoordinatorStates.WaitingForGame;
            }

        }
    }

    void WaitingForGameUpdate()
    {

    }

    IEnumerator GameOverSequence()
    {
        // Show Game Over UI
        gameOverUI.SetActive(true);
        yield return new WaitForSeconds(2f); // Wait for 3 seconds
        SceneManager.LoadScene("MainMenu");
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
                loseGO.GetComponent<TextMeshProUGUI>().text = loseTexts.RandomPick();
                lives--;
                if (lives <= 0)
                {
                    Debug.Log("Game Over!");
                    // Show Game Over UI

                }
            }

            CurrentState = CoordinatorStates.Idle;
        }
    }

    IEnumerator StartMinigameWithAnimation()
    {
        PlayTransition();

        // Play kites lives animation
        if (kites != null)
        {
            var childsCount = kites.transform.childCount;
            kites.SetActive(false);
            kites.SetActive(true);
            // Enable all kites
            for (int i = 0; i < childsCount; i++)
            {
                kites.transform.GetChild(i).gameObject.SetActive(true);
            }
            if (lives == 2)
            {
                // Disable half
                for (int i = 0; i < childsCount; i++)
                {
                    if (i < childsCount / 2) kites.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
            else if (lives == 1)
            {
                // Disable all but one
                for (int i = 0; i < childsCount; i++)
                {
                    if (i != 0) kites.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
            else if (lives <= 0)
            {
                for (int i = 0; i < childsCount; i++)
                {
                    kites.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }
        yield return _waitForSeconds1;

        ChooseMinigame();
        StartChoosenMinigame();
        cameraController.ZoomIn();
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
        if (_timeSliderAnimator)
        {
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

    internal void PauseMinigame(bool pause)
    {
        _currentMinigame.Pausing(pause);
    }
}
