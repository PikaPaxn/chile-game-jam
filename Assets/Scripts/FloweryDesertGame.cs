using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class FloweryDesertGame : MinigameController
{
    [Header("Config")]
    [SerializeField] private int targetClicks = 25;

    [Header("UI")]
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private TMP_Text timerText;

    [Header("Feedback")]
    [SerializeField] private ParticleSystem rainParticles;

    [SerializeField] private GameObject flowersGroup;
    [SerializeField] private AudioSource sfxTap;
    [SerializeField] private AudioSource sfxWin;
    [SerializeField] private AudioSource sfxLose;

    [Header("Events")]
    public UnityEvent onWin;
    public UnityEvent onLose;
    
    [Header("DevFeedback")]
    [SerializeField] private float _remaining;
    [SerializeField] private int _clicks;

    private enum State { Idle, Playing, Ended }
    private State _state = State.Idle;
    // Input
    InputAction anyButton;
    
    [Header("Rain configuration")]
    // Exponente de la curva (2 = cuadrática clásica). 
    // Sube a 2.5–3 para que “cueste más” al inicio.
    [SerializeField, Range(1f, 4f)] private float power = 2f;

    // Si quieres mantener 5 niveles discretos, activa esto.
    [SerializeField] private bool quantizeTo5Levels = false;
    [SerializeField] private int maxRainParticles = 100;
    
    void Start() {
        anyButton = InputSystem.actions.FindAction("AnyButton");
    }

    void OnEnable()
    {
        //StartGame();
    }

    public override void StartGame()
    {
        _clicks = 0;
        _remaining = Mathf.Max(0.1f, timeLimit-0.1f); //TODO: Fix this problem 0.1f
        _state = State.Playing;

        UpdateUI();
        SetRain(true);
        SetRainParticlesEmissionRate(0);
        
        base.StartGame();
    }

    void Update()
    {
        if (_state != State.Playing) return;

        if (anyButton.GetButtonDown())
        {
            _clicks++;
            if (sfxTap != null) sfxTap.Play();
            
            UpdateRainIntensity();

        }
        
        
        _remaining -= Time.deltaTime;
        if (_remaining <= 0f)
        {
            _remaining = 0f;
            FinishGame();
        }

        UpdateUI();
        
        
    }

    private void FinishGame()
    {
        if (_state == State.Ended) return;
        _state = State.Ended;

        bool win = _clicks >= targetClicks;

        SetRain(win);

        if (win)
        {
            if (sfxWin != null) sfxWin.Play();
            onWin?.Invoke();
            
            ActivateFlowers();
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
            counterText.text = $"Clicks: {_clicks} / {targetClicks}";

        if (timerText != null)
            timerText.text = $"Tiempo: {_remaining:0.0}s";
        
    }

    private void SetRain(bool on)
    {
        if (rainParticles == null) return;
        if (on && !rainParticles.isPlaying) rainParticles.Play();
        else if (!on && rainParticles.isPlaying) rainParticles.Stop();
    }

    private void SetRainParticlesEmissionRate(int rate)
    {
        if (rainParticles == null) return;
        var rainParticlesEmission = rainParticles.emission;
        rainParticlesEmission.rateOverTime = rate;
    }
    

    private void UpdateRainIntensity()
    {
        if (rainParticles == null) return;

        // Progreso real según clicks
        float progress = (targetClicks > 0) ? (float)_clicks / targetClicks : 1f;
        progress = Mathf.Clamp01(progress);

        // Curva cuadrática (ease-in): p^power
        float curve = Mathf.Pow(progress, power);  // p^2 si power=2

        float rate;
        if (quantizeTo5Levels)
        {
            // Mantiene 5 niveles, pero posicionados según la curva cuadrática
            // curve está en 0..1; lo llevamos a 0..5 y redondeamos
            int level = Mathf.Clamp(Mathf.RoundToInt(curve * 5f), 0, 5);
            rate = (maxRainParticles / 5f) * level;
        }
        else
        {
            // Cuadrática continua
            rate = curve * maxRainParticles;
        }

        SetRainParticlesEmissionRate(Mathf.RoundToInt(rate));
    }

    private void ActivateFlowers(bool activate = true)
    {
        flowersGroup.SetActive(activate);
    }
}
