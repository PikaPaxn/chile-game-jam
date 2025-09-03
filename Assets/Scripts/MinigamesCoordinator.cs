using UnityEngine;
using UnityEngine.UI;

public class MinigamesCoordinator : MonoBehaviour
{
    [Header("Minigames List")]
    public MinigameController[] minigames;
    MinigameController _currentMinigameType;
    MinigameController _currentMinigame;
    bool _minigameDone = false;

    [Header("UI Refs")]
    public Slider timeLeftSlider;
    public GameObject wonGO;
    public GameObject loseGO;

    void Start() {
        wonGO.SetActive(false);
        loseGO.SetActive(false);
    }

    void Update() {
        if (_currentMinigame != null && !_minigameDone) {
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
                _minigameDone = true;
            }

            // Check if lose
            if (!_currentMinigame.IsPlaying || timeDone) {
                Debug.Log($"Did you won? {_currentMinigame.HasWon}");

                // Check if won
                if (_currentMinigame.HasWon) {
                    wonGO.SetActive(true);
                } else {
                    loseGO.SetActive(true);
                }

                _minigameDone = true;
            }
        }
    }

    public void StartMinigame() {
        // Get random game
        var minigame = minigames[Random.Range(0, minigames.Length)];

        // If is not in the scene, instanciate it
        if (_currentMinigameType != minigame || !_currentMinigame.IsInstanced) {
            if (_currentMinigame != null) {
                Destroy(_currentMinigame.gameObject);
            }
            _currentMinigameType = minigame;
            _currentMinigame = Instantiate(minigame);
        }

        // Start a new minigame
        _currentMinigame.StartGame();
        _minigameDone = false;
        wonGO.SetActive(false);
        loseGO.SetActive(false);
    }
}
