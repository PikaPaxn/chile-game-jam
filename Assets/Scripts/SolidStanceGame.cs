using UnityEngine;
using UnityEngine.Events;

public class SolidStanceGame : MinigameController
{
    public enum Mode { HorizontalBar, TwoBars}
    [Header("Modo")]
    [SerializeField] private Mode mode = Mode.HorizontalBar;

    [Header("Barras")]
    [SerializeField] private BalanceMeter horizontalMeter;
    [SerializeField] private BalanceMeter verticalMeter; // opcional si usas dos barras

    [Header("Animación del personaje")]
    [Tooltip("Si lo asignas, se rotará con la barra (roll en Z por la horizontal, pitch en X por la vertical).")]
    [SerializeField] private Transform tiltTransform;
    [SerializeField] private float tiltDegrees = 15f;
    [Tooltip("Opcional: Animator para parámetros 'leanX'/'leanY'")]
    [SerializeField] private Animator animator;

    [Header("Eventos")]
    public UnityEvent onLose;

    void Update()
    {
        if (_currentState != State.Playing) return;

        float progress01 = 1f - TimeLeft01(); // 0→inicio, 1→fin

        // Avanzar barras
        if (horizontalMeter != null) horizontalMeter.Tick(progress01);
        if (mode == Mode.TwoBars && verticalMeter != null) verticalMeter.Tick(progress01);

        // ¿Perdió?
        bool failH = (horizontalMeter != null && horizontalMeter.HasFailed());
        bool failV = (mode == Mode.TwoBars && verticalMeter != null && verticalMeter.HasFailed());

        if (failH || failV)
        {
            _currentState = State.End; // HasWon queda false
            onLose?.Invoke();
            return;
        }

        // ¿Ganó por aguantar el tiempo?
        if (TimeLeft01() <= 0f)
        {
            Won(); // llama a base.Won()
            return;
        }

        // Feedback de inclinación/animación
        ApplyTiltAndAnim();
    }

    public override void StartGame()
    {
        base.StartGame();

        if (horizontalMeter != null) horizontalMeter.ResetMeter(0f);
        if (mode == Mode.TwoBars && verticalMeter != null) verticalMeter.ResetMeter(0f);

        ApplyTiltAndAnim();
    }

    void ApplyTiltAndAnim()
    {
        if (tiltTransform == null && animator == null) return;

        float leanX = 0f;
        float leanY = 0f;

        if (horizontalMeter != null)
            leanX = Mathf.Clamp(horizontalMeter.Position01 * 2f - 1f, -1f, 1f); // de 0..1 a -1..1

        if (mode == Mode.TwoBars && verticalMeter != null)
            leanY = Mathf.Clamp(verticalMeter.Position01 * 2f - 1f, -1f, 1f);

        if (tiltTransform != null)
        {
            // Horizontal → roll (Z). Vertical → pitch (X)
            Quaternion target =
                Quaternion.Euler(-leanY * tiltDegrees, 0f, -leanX * tiltDegrees);
            tiltTransform.rotation = Quaternion.Slerp(
                tiltTransform.rotation, target, 10f * Time.deltaTime);
        }

        if (animator != null)
        {
            animator.SetFloat("leanX", leanX);
            animator.SetFloat("leanY", leanY);
        }
    }
}
