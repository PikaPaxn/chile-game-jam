using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class BalanceMeter : MonoBehaviour {
    public enum Axis { Horizontal, Vertical }
    public enum MovementMode { AutoDrift, PerlinQuake }

    [Header("Tipo de barra")]
    [SerializeField] private Axis axis = Axis.Horizontal;

    [Header("UI")]
    [SerializeField] private RectTransform trackRect;
    [SerializeField] private RectTransform knobRect;
    [SerializeField] private RectTransform safeZoneRect;

    [Header("Zona segura (centrada)")]
    [Range(0.05f, 0.95f)]
    [SerializeField] private float safeZoneWidth01 = 0.35f; // % del ancho/alto total

    [Header("Modo de movimiento")]
    [SerializeField] private MovementMode movementMode = MovementMode.AutoDrift;

    [Header("Feedback de input")]
    [SerializeField] private Image knobImage;        // si es vacío, se toma del knobRect
    [SerializeField] private Color pressColor = Color.red;
    [SerializeField] private float pressScale = 1.2f;
    [SerializeField, Range(0.05f, 0.6f)] private float pressLerpDuration = 0.15f;

    [Header("Auto-Drift")]
    [Tooltip("Velocidad inicial (normalizada por segundo).")]
    [SerializeField] private float startSpeed = 0.06f;
    [Tooltip("Aceleración base (u/s^2). Se multiplica por la curva en el tiempo.")]
    [SerializeField] private float autoAccelBase = 0.12f;
    [Tooltip("0→inicio, 1→fin del minijuego")]
    [SerializeField] private AnimationCurve autoAccelOverTime = AnimationCurve.EaseInOut(0, 0.3f, 1, 2.0f);
    [Tooltip("Zona muerta alrededor del centro para evitar flips de dirección")]
    [SerializeField] private float centerDeadZone = 0.02f;
    [Tooltip("Límite de velocidad")]
    [SerializeField] private float maxSpeed = 2.0f;
    [Tooltip("Rozamiento opcional (0 = sin rozamiento)")]
    [SerializeField] private float friction = 0f;

    [Header("Input del jugador (suma velocidad, NO aceleración)")]
    [Tooltip("Cuánta velocidad por segundo agrega tu input")]
    [SerializeField] private float inputSpeed = 1.4f;
    private InputAction _direction;

    //Modo Terremoto
    [Header("Perlin Quake (si eliges ese modo)")]
    [SerializeField] private float quakeAmplitude = 0.9f;
    [SerializeField] private float quakeFrequency = 0.7f;
    [SerializeField] private AnimationCurve quakeOverTime = AnimationCurve.Linear(0, 0.7f, 1, 1.2f);

    [Header("Gracia al salir de la zona (segundos)")]
    [SerializeField] private float graceTime = 0.15f;

    // estados
    private float _pos;             // -1..1 (0 centrado)
    private float _vel;
    private float _outsideTimer;
    private int _seed;
    private float _lastDir = 1f;
    private float _pressTimer;      // cuenta regresiva del pulso
    private Color _knobBaseColor;   // color original del knob
    private Vector3 _knobBaseScale; // escala original del knob
    private float _prevAxisRaw;     // -1 o +1 (dirección usada)

    public float Position01 => Mathf.InverseLerp(-1f, 1f, _pos);
    public bool IsInsideSafe { get; private set; } = true;

    private void Awake()
    {
        _seed = Random.Range(0, 99999);
        if (knobImage == null && knobRect != null) knobImage = knobRect.GetComponent<Image>();
        if (knobImage != null)
        {
            _knobBaseColor = knobImage.color;
            _knobBaseScale = knobImage.rectTransform.localScale;
        }
        
        RefreshSafeZoneVisual();
        SnapKnob();
    }

    private void Start() {
        _direction = InputSystem.actions.FindAction("Directions");
    }

    public void ResetMeter(float startPos = 0f)
    {
        _pos = Mathf.Clamp(startPos, -1f, 1f);
        _outsideTimer = 0f;
        IsInsideSafe = true;

        // Dirección inicial aleatoria, velocidad pequeña
        _lastDir = Random.value < 0.5f ? -1f : 1f;
        _vel = startSpeed * _lastDir;

        SnapKnob();
    }

    public void Tick(float progress01)
    {
        float dt = Time.deltaTime;

        // input del jugador: agrega velocidad (no aceleración)
        float axisInput = axis == Axis.Horizontal ? _direction.ReadValue<Vector2>().x
                                                  : _direction.ReadValue<Vector2>().y;
        
        
        // detectar "keyUp" del stick (solo cuando pasa de no-0 a 0)
        bool axisUp = !Mathf.Approximately(_prevAxisRaw, 0f) && Mathf.Approximately(axisInput, 0f); 
        if (axisUp) PulseOnRelease(); 
        // detectar "keyDown" del stick (solo cuando pasa de 0 a no-0)
        bool axisDown = Mathf.Approximately(_prevAxisRaw, 0f) && !Mathf.Approximately(axisInput, 0f); 
        if (axisDown) PulseOnPress();
        
        _vel += inputSpeed * axisInput * dt;

        // movimiento auto
        if (movementMode == MovementMode.AutoDrift)
        {
            // dirección = signo de la posición, con dead zone para evitar jitter
            float dir = Mathf.Abs(_pos) < centerDeadZone ? _lastDir : Mathf.Sign(_pos);
            if (dir != 0f) _lastDir = dir;

            float accel = autoAccelBase * Mathf.Max(0f, autoAccelOverTime.Evaluate(progress01));
            _vel += dir * accel * dt; // acelera hacia el lado en el que está el knob
        }
        else
        {
            // perlin (modo viejo)
            float t = Time.time * quakeFrequency;
            float noise = Mathf.PerlinNoise(_seed, t) * 2f - 1f;
            float quakeForce = noise * quakeAmplitude * Mathf.Max(0.05f, quakeOverTime.Evaluate(progress01));
            _vel += quakeForce * dt;
        }

        // rozamiento y límite velocidad
        if (friction > 0f) _vel *= Mathf.Exp(-friction * dt);
        _vel = Mathf.Clamp(_vel, -maxSpeed, maxSpeed);

        // integración de posición
        _pos = Mathf.Clamp(_pos + _vel * dt, -1f, 1f);

        // safe zone
        float halfSafe = Mathf.Clamp01(safeZoneWidth01);
        IsInsideSafe = Mathf.Abs(_pos) <= halfSafe;
        _outsideTimer = IsInsideSafe ? 0f : _outsideTimer + dt;
        
        // lerp del color del knob
        if (_pressTimer > 0f && knobImage != null)
        {
            _pressTimer = Mathf.Max(0f, _pressTimer - dt);
            float tLerp = 1f - (_pressTimer / pressLerpDuration); // 0→1
            knobImage.color = Color.Lerp(pressColor, _knobBaseColor, tLerp);
            Vector3 big = _knobBaseScale * pressScale;
            knobImage.rectTransform.localScale = Vector3.Lerp(big, new Vector3(1f, 1f, 1f), tLerp);
        }

        // visual
        SnapKnob();
        _prevAxisRaw = axisInput;
    }
    
    void PulseOnPress()
    {
        _pressTimer = 0f; //cancela los lerps
        if (knobImage != null)
        {
            knobImage.color = pressColor;
            knobImage.rectTransform.localScale = new Vector3(pressScale, pressScale, 1f);
        }
    } 
    void PulseOnRelease() { 
        _pressTimer = pressLerpDuration; // reinicia el pulso
    }

    public bool HasFailed()
    {
        return !IsInsideSafe && _outsideTimer >= graceTime;
    }

    void SnapKnob()
    {
        if (trackRect == null || knobRect == null) return;

        if (axis == Axis.Horizontal)
        {
            float usable = trackRect.rect.width * 0.5f;
            Vector2 p = knobRect.anchoredPosition;
            p.x = _pos * usable;
            knobRect.anchoredPosition = p;
        }
        else
        {
            float usable = trackRect.rect.height * 0.5f;
            Vector2 p = knobRect.anchoredPosition;
            p.y = _pos * usable;
            knobRect.anchoredPosition = p;
        }
    }

    void RefreshSafeZoneVisual()
    {
        if (trackRect == null || safeZoneRect == null) return;

        if (axis == Axis.Horizontal)
        {
            Vector2 size = safeZoneRect.sizeDelta;
            size.x = trackRect.rect.width * safeZoneWidth01;
            safeZoneRect.sizeDelta = size;
        }
        else
        {
            Vector2 size = safeZoneRect.sizeDelta;
            size.y = trackRect.rect.height * safeZoneWidth01;
            safeZoneRect.sizeDelta = size;
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (knobRect != null)
        {
            knobRect.anchorMin = knobRect.anchorMax = new Vector2(0.5f, 0.5f);
        }
        if (safeZoneRect != null)
        {
            safeZoneRect.anchorMin = safeZoneRect.anchorMax = new Vector2(0.5f, 0.5f);
        }
        if (!Application.isPlaying) RefreshSafeZoneVisual();
    }
#endif
}
