using UnityEngine;
using UnityEngine.UI;

public class PauseGame : MonoBehaviour {
    [SerializeField] GameObject pauseButton;
    [SerializeField] GameObject pausePanel;

    float _oldTimeScale = 1f;

    void Start() {
        UnPause();
    }

    public void Pause() {
        pauseButton.SetActive(false);
        pausePanel.SetActive(true);
        _oldTimeScale = Time.timeScale;
        Time.timeScale = 0;
    }

    public void UnPause() {
        pauseButton.SetActive(true);
        pausePanel.SetActive(false);
        Time.timeScale = _oldTimeScale;
    }

    bool IsPaused => Time.timeScale == 0;

    public void TogglePause() {
        if (IsPaused) UnPause();
        else Pause();
    }

}
