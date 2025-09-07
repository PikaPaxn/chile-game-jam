using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PauseGame : MonoBehaviour {
    [SerializeField] GameObject pauseButton;
    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject selectableItemOnPause;

    GameObject _oldSelectable;
    float _oldTimeScale = 1f;
    MinigamesCoordinator _coordinator;
    
    void Start() {
        UnPause();
        var pauseAction = InputSystem.actions.FindAction("Pause");
        pauseAction.started += GamepadPause;
        _coordinator = FindFirstObjectByType<MinigamesCoordinator>();
    }

    void GamepadPause(InputAction.CallbackContext ctx) {
        TogglePause();
    }

    public void Pause() {
        if (_coordinator) _coordinator.PauseMinigame(true);

        pauseButton.SetActive(false);
        pausePanel.SetActive(true);
        _oldSelectable = EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(selectableItemOnPause);
        _oldTimeScale = Time.timeScale;
        Time.timeScale = 0;
    }

    public void UnPause() {
        pauseButton.SetActive(true);
        pausePanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(_oldSelectable);
        Time.timeScale = _oldTimeScale;
        if (_coordinator) _coordinator.PauseMinigame(false);
    }

    bool IsPaused => Time.timeScale == 0;

    public void TogglePause() {
        if (IsPaused) UnPause();
        else Pause();
    }

}
