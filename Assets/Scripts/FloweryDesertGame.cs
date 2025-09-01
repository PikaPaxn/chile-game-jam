using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class FloweryDesertGame : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private int targetClicks = 25;

    [Header("UI")]
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private TMP_Text timerText;

    [Header("Feedback")]
    [SerializeField] private ParticleSystem rainParticles;
    [SerializeField] private AudioSource sfxTap;
    [SerializeField] private AudioSource sfxWin;
    [SerializeField] private AudioSource sfxLose;

    [Header("Events")]
    public UnityEvent onWin;
    public UnityEvent onLose;
    
    [Header("DevFeedback")]
    [SerializeField] private float remaining;
    [SerializeField] private int clicks;

    private enum State { Idle, Playing, Ended }
    private State state = State.Idle;

    

    void OnEnable()
    {
        StartGame();
    }

    public void StartGame()
    {
        clicks = 0;
        remaining = Mathf.Max(0.1f, this.GetMinigameController().time-0.1f); //TODO: Fix this problem 0.1f
        state = State.Playing;

        UpdateUI();
        SetRain(false);
    }

    void Update()
    {
        if (state != State.Playing) return;

        if (Input.GetButtonDown("Fire1"))
        {
            clicks++;
            if (sfxTap != null) sfxTap.Play();
            
        }

        remaining -= Time.deltaTime;
        if (remaining <= 0f)
        {
            remaining = 0f;
            FinishGame();
        }

        UpdateUI();
    }

    private void FinishGame()
    {
        if (state == State.Ended) return;
        state = State.Ended;

        bool win = clicks >= targetClicks;

        SetRain(win);

        if (win)
        {
            if (sfxWin != null) sfxWin.Play();
            onWin?.Invoke();
        }
        else
        {
            if (sfxLose != null) sfxLose.Play();
            onLose?.Invoke();
        }

    }

    private void UpdateUI()
    {
        if (counterText != null)
            counterText.text = $"Clicks: {clicks} / {targetClicks}";

        if (timerText != null)
            timerText.text = $"Tiempo: {remaining:0.0}s";
        
    }

    private void SetRain(bool on)
    {
        if (rainParticles == null) return;
        if (on && !rainParticles.isPlaying) rainParticles.Play();
        else if (!on && rainParticles.isPlaying) rainParticles.Stop();
    }
}
