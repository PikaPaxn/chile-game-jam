using UnityEngine;
using UnityEngine.UI;

public class BalanceMeter : MonoBehaviour
{
    public enum Axis { Horizontal, Vertical }

    [Header("Tipo de barra")]
    [SerializeField] private Axis axis = Axis.Horizontal;

    [Header("UI")]
    [Tooltip("Rect del riel (fondo de la barra)")]
    [SerializeField] private RectTransform trackRect;
    [Tooltip("Rect del puntero/knob que se mueve")]
    [SerializeField] private RectTransform knobRect;
    [Tooltip("Opcional: imagen que representa la zona segura")]
    [SerializeField] private RectTransform safeZoneRect;

    [Header("Zona segura (centrada)")]
    [Range(0.1f, 0.9f)]
    [SerializeField] private float safeZoneWidth01 = 0.35f; // % del ancho/alto del track

    [Header("Control del jugador")]
    [Tooltip("Sensibilidad del input del jugador")]
    [SerializeField] private float inputPower = 2.2f;

    [Header("Física del puntero")]
    [Tooltip("Aceleración base aplicada (input + sismo)")]
    [SerializeField] private float accel = 6f;
    [Tooltip("Amortiguación de velocidad")]
    [SerializeField] private float damping = 6f;

    [Header("Deriva sísmica")]
    [Tooltip("Fuerza base del sismo")]
    [SerializeField] private float quakeAmplitude = 0.9f;
    [Tooltip("Frecuencia del ruido (más alto = cambios más rápidos)")]
    [SerializeField] private float quakeFrequency = 0.7f;
    [Tooltip("Escala de dificultad en el tiempo (0=inicio, 1=fin)")]
    [SerializeField] private AnimationCurve quakeOverTime = AnimationCurve.Linear(0, 0.7f, 1, 1.2f);

    [Header("Gracia al salir de la zona (segundos)")]
    [SerializeField] private float graceTime = 0.0f;

    // Estado
    float _pos;          // -1..1 (0 centrado)
    float _vel;
    float _outsideTimer;
    int _seed;

    public float Position01 => Mathf.InverseLerp(-1f, 1f, _pos);
    public bool IsInsideSafe { get; private set; } = true;

    void Awake()
    {
        _seed = Random.Range(0, 99999);
        RefreshSafeZoneVisual();
        SnapKnob();
    }

    public void ResetMeter(float startPos = 0f)
    {
        _pos = Mathf.Clamp(startPos, -1f, 1f);
        _vel = 0f;
        _outsideTimer = 0f;
        IsInsideSafe = true;
        SnapKnob();
    }

    public void Tick(float progress01)
    {
        // 1) Input del jugador
        float axisInput = axis == Axis.Horizontal
            ? Input.GetAxisRaw("Horizontal")   // flechas o A/D
            : Input.GetAxisRaw("Vertical");    // flechas o W/S

        float playerForce = axisInput * inputPower;

        // 2) Deriva del “sismo” (Perlin: suave y oscilante)
        float t = Time.time * quakeFrequency;
        float noise = Mathf.PerlinNoise(_seed, t) * 2f - 1f;  // -1..1
        float quakeForce = noise * quakeAmplitude * Mathf.Max(0.05f, quakeOverTime.Evaluate(progress01));

        // 3) Dinámica simple (aceleración + amortiguación)
        float dt = Time.deltaTime;
        _vel += (playerForce + quakeForce) * accel * dt;
        _vel *= Mathf.Exp(-damping * dt); // amortiguación estable
        _pos = Mathf.Clamp(_pos + _vel * dt, -1f, 1f);

        // 4) ¿Está dentro de la zona segura?
        float halfSafe = Mathf.Clamp01(safeZoneWidth01);
        IsInsideSafe = Mathf.Abs(_pos) <= halfSafe;

        if (IsInsideSafe) _outsideTimer = 0f;
        else              _outsideTimer += dt;

        // 5) Mover el knob en la UI
        SnapKnob();
    }

    /// <summary>
    /// ¿Falló definitivamente (tras la gracia)?
    /// </summary>
    public bool HasFailed()
    {
        return !IsInsideSafe && _outsideTimer >= graceTime;
    }

    void SnapKnob()
    {
        if (trackRect == null || knobRect == null) return;

        // Mapeo -1..1 a píxeles del track
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
        if (Application.isPlaying == false) RefreshSafeZoneVisual();
    }
#endif
}
