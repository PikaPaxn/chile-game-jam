using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinigamesCoordinator : MonoBehaviour
{
    [Header("Minigames List")]
    public MinigameController[] minigames;
    MinigameController _currentMinigameType;
    MinigameController _currentMinigame;

    [Header("UI Refs")]
    public Slider timeLeftSlider;
    public GameObject wonGO;
    public GameObject loseGO;
    public TextMeshProUGUI counterText;

    enum CoordinatorStates { Idle, WaitingForGame, PlayingGame }
    CoordinatorStates _currentState;
    float _stateChangedTime;

    CoordinatorStates CurrentState {
        get { return _currentState; } 
        set {
            _stateChangedTime = Time.time;
            _currentState = value;
        }
    }

    void Start() {
        ResetUI();
        CurrentState = CoordinatorStates.Idle;
    }

    void Update() {
        switch(CurrentState) {
            case CoordinatorStates.Idle: IdleUpdate(); break;
            case CoordinatorStates.WaitingForGame: WaitingForGameUpdate(); break;
            case CoordinatorStates.PlayingGame: PlayingGameUpdate(); break;
        }
    }

    void IdleUpdate() {
        if (Time.time - _stateChangedTime >= 2f) {
            ResetUI();
            ChooseMinigame();
            StartCoroutine(StartMinigameWithAnimation());
            CurrentState = CoordinatorStates.WaitingForGame;
        }
    }

    void WaitingForGameUpdate() {

    }

    void PlayingGameUpdate() {
        // Update time
        bool timeDone = false;
        if (_currentMinigame.UseTime) {
            var timeLeft = _currentMinigame.TimeLeft01();
            timeLeftSlider.value = timeLeft;
            timeDone = timeLeft <= 0;
        }

        // Check if won
        if (_currentMinigame.HasWon) {
            wonGO.SetActive(true);
            CurrentState = CoordinatorStates.Idle;
        }

        // Check if lose
        if (!_currentMinigame.IsPlaying || timeDone) {
            Debug.Log($"Did you won? {_currentMinigame.HasWon}");

            // Call the callback
            if (timeDone) {
                _currentMinigame.TimeEnded();
            }

            // Check if won
            if (_currentMinigame.HasWon) {
                wonGO.SetActive(true);
            } else {
                loseGO.SetActive(true);
            }

            CurrentState = CoordinatorStates.Idle;
        }
    }

    IEnumerator StartMinigameWithAnimation() {
        if (counterText) {
            counterText.text = "3";
            yield return new WaitForSeconds(1f);
            counterText.text = "2";
            yield return new WaitForSeconds(1f);
            counterText.text = "1";
            yield return new WaitForSeconds(1f);
            counterText.text = _currentMinigame.instructions;
        }

        StartChoosenMinigame();
        CurrentState = CoordinatorStates.PlayingGame;

        if (counterText) {
            yield return new WaitForSeconds(1f);
            counterText.text = "";
        }
    }

    void ChooseMinigame() {
        // Get random game
        var minigame = minigames.RandomPick(_currentMinigame);

        // If is not in the scene, instanciate it
        if (_currentMinigameType != minigame || !_currentMinigame.IsInstanced) {
            if (_currentMinigame != null) {
                Destroy(_currentMinigame.gameObject);
            }
            _currentMinigameType = minigame;
            _currentMinigame = Instantiate(minigame);
        }
    }

    void StartChoosenMinigame() {
        // Start a new minigame
        _currentMinigame.StartGame();
        ResetUI();
    }

    // Didn't change for compatibility with current testing
    public void StartMinigame() {
        ChooseMinigame();
        StartChoosenMinigame();        
    }

    void ResetUI() {
        wonGO.SetActive(false);
        loseGO.SetActive(false);
        if (counterText) {
            counterText.text = "";
        }
    }
}
