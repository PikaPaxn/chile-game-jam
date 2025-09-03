using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class FlowerBehaviour : MonoBehaviour
{
    [Header("Animación de Crecimiento")]
    [SerializeField, Min(0.01f)] private float growDuration = 1.5f;
    [SerializeField] private AnimationCurve growCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Delay aleatorio")]
    [SerializeField, Min(0f)] private float minDelay = 0f;
    [SerializeField, Min(0f)] private float maxDelay = 1f;
    
    [Header("Idle (después de crecer)")]
    [SerializeField] private bool enableIdle = true;
    [SerializeField, Min(0f)] private float idleDelay = 0.0f;
    [SerializeField, Min(0f)] private float idleSpeed = 1.2f;
    [SerializeField, Range(0f, 0.25f)] private float idleAmpScale = 0.06f;
    [SerializeField, Range(0f, 8f)] private float idleAmpRotDeg = 2.0f;
    [SerializeField, Range(0f, 1f)] private float idleAmpYBias = 0.5f;


    private Vector3 _initialScale;
    private Vector3 _targetScale;
    private Quaternion _targetRotation;
    private Coroutine _growRoutine;
    private Image _image;
    private float _idlePhase;

    private void Awake()
    {
        _image = GetComponent<Image>();
        
        _initialScale = transform.localScale;
        _targetScale = _initialScale;
        
        _targetRotation = transform.localRotation;
        _idlePhase = Random.Range(0f, Mathf.PI * 2f);
        
    }

    private void OnEnable()
    {
        transform.localScale = new Vector3(_targetScale.x, 0f, _targetScale.z);
        SetAlpha(0f);

        _growRoutine = StartCoroutine(GrowAfterDelayThenIdle());
    }

    private void OnDisable()
    {
        if (_growRoutine != null)
        {
            StopCoroutine(_growRoutine);
            _growRoutine = null;
        }
        transform.localRotation = _targetRotation;
        transform.localScale = _targetScale;
        SetAlpha(1f);
    }
    
    private void SetAlpha(float value)
    {
        if (_image != null)
        {
            Color c = _image.color;
            c.a = value;
            _image.color = c;
        }
    }

    private IEnumerator GrowAfterDelayThenIdle()
    {
        
        float delay = Random.Range(Mathf.Min(minDelay, maxDelay), Mathf.Max(minDelay, maxDelay));

        if (delay > 0f) yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        while (elapsed < growDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / growDuration);
            float curveT = growCurve.Evaluate(t);

            float newY = Mathf.Lerp(0f, _targetScale.y, curveT);
            transform.localScale = new Vector3(_targetScale.x, newY, _targetScale.z);
            
            SetAlpha(Mathf.Lerp(0f, 1f, curveT));

            yield return null;
        }

        transform.localScale = _targetScale;
        SetAlpha(1f);

        if (enableIdle)
        {
            if (idleDelay > 0f) yield return new WaitForSeconds(idleDelay);

            Vector3 baseScale = _targetScale;
            Quaternion baseRot = _targetRotation;

            while (true)
            {
                float t = Time.time * (Mathf.PI * 2f) * idleSpeed + _idlePhase;
                float s = Mathf.Sin(t);

                // Escala “breathing”: X e Y con pesos distintos
                // - X oscila con (1 - idleAmpYBias)
                // - Y oscila con idleAmpYBias
                float sx = 1f + idleAmpScale * s * (1f - idleAmpYBias);
                float sy = 1f + idleAmpScale * s * (idleAmpYBias);
                transform.localScale = new Vector3(baseScale.x * sx, baseScale.y * sy, baseScale.z);

                // Sway de rotación muy suave en Z
                float zDeg = idleAmpRotDeg * s;
                transform.localRotation = baseRot * Quaternion.Euler(0f, 0f, zDeg);

                yield return null;
            }
        }

        _growRoutine = null;
        
    }
}