using UnityEngine;
using UnityEngine.UI;
public class RayuelaGame : MinigameController
{
    [Header("Game Object References")]
    [SerializeField] Slider forceSlider;
    [SerializeField] RectTransform arena;
    [SerializeField] RectTransform arenaTarget;
    [SerializeField] RectTransform sliderTarget;
    [SerializeField] Transform rayuela;
    Vector3 _rayuelaInitialPos;
    bool _rayuelaPendingPos = false;
    float _rayuelaTargetPos;

    [Header("Game Config")]
    [SerializeField] float forceSpeed;
    [Tooltip("Minimo y Maximo de fuerza pedida (0-1)")]
    [SerializeField] Vector2 forceContraints;
    [SerializeField, Min(.05f)] float forceMargin = .2f;
    float _currentForce;
    float _targetForce;
    [SerializeField, Min(.1f)] float rayuelaTimeToFlight = 2f;
    [SerializeField] AnimationCurve rayuelaFlight = AnimationCurve.EaseInOut(0, 0, 1, 1);
    float _currentRayuelaTime;

    enum RayuelaState { Idle, Charging, Throwing, Done }
    RayuelaState _currentRayuelaState;

    void OnEnable() {
        if (rayuela.localPosition != Vector3.zero) {
            _rayuelaInitialPos = rayuela.localPosition;
            _rayuelaPendingPos = false;
        } else {
            _rayuelaPendingPos = true;
        }
    }

    public override void StartGame() {
        base.StartGame();

        // Reset
        if (!_rayuelaPendingPos) {
            rayuela.localPosition = _rayuelaInitialPos;
        }
        forceSlider.value = _currentForce = 0;
        _currentRayuelaState = RayuelaState.Idle;

        // Generate target
        _targetForce = forceContraints.Range();
        arenaTarget.anchorMin = sliderTarget.anchorMin = new Vector2(0, _targetForce);
        arenaTarget.anchorMax = sliderTarget.anchorMax = new Vector2(1, _targetForce);       
        var targetHeight = (forceSlider.transform as RectTransform).rect.height * forceMargin;
        sliderTarget.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
        arenaTarget.anchoredPosition = new Vector2(arenaTarget.anchoredPosition.x, 0);
        sliderTarget.anchoredPosition = new Vector2(sliderTarget.anchoredPosition.x, 0);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_rayuelaPendingPos && rayuela.localPosition != Vector3.zero) {
            _rayuelaInitialPos = rayuela.localPosition;
            _rayuelaPendingPos = false;
        }

        //TODO: Change to use new Input System
        if (_currentRayuelaState == RayuelaState.Idle && Input.GetButtonDown("Jump")) {
            AddForce();
            _currentRayuelaState = RayuelaState.Charging;
        } else if (_currentRayuelaState == RayuelaState.Charging) {
            if (Input.GetButtonUp("Jump")) {
                CalcRayuelaTargetPos();
                _currentRayuelaTime = 0;
                _currentRayuelaState = RayuelaState.Throwing;
            } else if (Input.GetButton("Jump")) {
                AddForce();
            }
        } else if (_currentRayuelaState == RayuelaState.Throwing) {
            _currentRayuelaTime = Mathf.Clamp(_currentRayuelaTime + Time.deltaTime, 0, rayuelaTimeToFlight);
            var currentY = Mathf.Lerp(_rayuelaInitialPos.y, _rayuelaTargetPos, rayuelaFlight.Evaluate(_currentRayuelaTime / rayuelaTimeToFlight));
            rayuela.localPosition = new Vector3(rayuela.localPosition.x, currentY, rayuela.localPosition.z);

            if (_currentRayuelaTime >= rayuelaTimeToFlight) {
                if (_targetForce - forceMargin <= _currentForce && _currentForce <= _targetForce + forceMargin) {
                    Won();
                } else {
                    Lose();
                }
                _currentRayuelaState = RayuelaState.Done;
            }
        }
    }

    void AddForce() {
        _currentForce = Mathf.Clamp01(_currentForce + forceSpeed * Time.deltaTime);
        forceSlider.value = _currentForce;
    }

    void CalcRayuelaTargetPos() {
        var arenaRect = arena.rect;
        _rayuelaTargetPos = (arena.localPosition.y - arenaRect.height / 2f) + arenaRect.height * _currentForce;
    }
}
