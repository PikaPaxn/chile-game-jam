using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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
    public GameObject kites;
    public int lives = 3;
    public GameObject gameOverUI;


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

            if (lives <= 0)
            {
                StartCoroutine(GameOverSequence());
                return;
            }
            StartCoroutine(StartMinigameWithAnimation());
            CurrentState = CoordinatorStates.WaitingForGame;
        }
    }

    void WaitingForGameUpdate()
    {

    }

    IEnumerator GameOverSequence()
    {
        // Show Game Over UI
        gameOverUI.SetActive(true);
        yield return new WaitForSeconds(3f); // Wait for 3 seconds
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
        // Play transition
        if (transitions.Length > 0)
        {
            var transition = transitions.RandomPick();
            transition.SetActive(true);
        }

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

    internal void PauseMinigame(bool pause) {
        _currentMinigame.Pausing(pause);
    }
}
