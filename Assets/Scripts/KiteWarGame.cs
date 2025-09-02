using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class KiteWarGame : MinigameController
{
    [Header("Rondas (cantidad de flechas por ronda, en orden)")]
    [SerializeField] private int[] roundLengths = new int[] { 1, 3, 5 };

    [Header("Tiempo por input (segundos)")]
    [Tooltip("Ventana base para presionar cada flecha. Se reduce por ronda con roundTimeMultiplier.")]
    [SerializeField, Range(0.15f, 2f)] private float perKeyTime = 0.7f;

    [Tooltip("Multiplicador por ronda. Ej: 0.9f = cada ronda 10% menos de tiempo.")]
    [SerializeField, Range(0.6f, 1.0f)] private float roundTimeMultiplier = 0.9f;

    [Tooltip("Pausa corta entre rondas (segundos)")]
    [SerializeField, Range(0f, 5f)] private float betweenRoundsDelay = 0.6f;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text roundText;

    [Tooltip("Contenedor donde se dibuja la secuencia de flechas")]
    [SerializeField] private Transform sequenceContainer;

    [Tooltip("Prefab de un ítem de flecha (Image). El script pintará su sprite y color.")]
    [SerializeField] private GameObject arrowItemPrefab;

    [Header("Sprites flechas")]
    [SerializeField] private Sprite spriteUp;
    [SerializeField] private Sprite spriteDown;
    [SerializeField] private Sprite spriteLeft;
    [SerializeField] private Sprite spriteRight;

    [Header("Colores UI")]
    [SerializeField] private Color nextColor = Color.white;
    [SerializeField] private Color hitColor = new Color(0.3f, 1f, 0.3f);
    [SerializeField] private Color failColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private Color idleColor = new Color(1f, 1f, 1f, 0.4f);

    [Header("Feedback / Audio")]
    [SerializeField] private AudioSource sfxOk;
    [SerializeField] private AudioSource sfxWrong;
    [SerializeField] private AudioSource sfxRound;
    [SerializeField] private AudioSource sfxWin;
    [SerializeField] private AudioSource sfxLose;

    [Header("Animators (opcional)")]
    [SerializeField] private Animator playerKiteAnim;
    [SerializeField] private Animator enemyKiteAnim;

    [Header("Eventos")]
    public UnityEvent onPlayerHit;
    public UnityEvent onEnemyHit;
    public UnityEvent onPlayerWin;
    public UnityEvent onEnemyWin;
    
    private enum Direction { Up, Down, Left, Right }

    private readonly List<Direction> _currentSequence = new();
    private readonly List<Image> _uiArrows = new();

    private int _roundIndex;     // ronda actual
    private int _seqIndex;       // índice dentro de la secuencia
    private float _keyDeadline;  // tiempo límite para la tecla actual
    private bool _waitingNextRound;
    

    private void OnEnable()
    {
        StartGame();
    }

    public override void StartGame()
    {
        _roundIndex = 0;
        _waitingNextRound = false;

        ClearSequenceUI();
        UpdateRoundText();
        UpdateTimerUI();

        base.StartGame();

        StartRound();
    }

    private void Update()
    {
        if (_currentState != State.Playing) return;

        UpdateTimerUI();
        if (TimeLeft01() <= 0f) 
        {
            Lose();
            return;
        }

        if (_waitingNextRound) return;

        if (Time.time > _keyDeadline)
        {
            FailInput();
            return;
        }

        Direction? input = ReadInput();
        if (!input.HasValue) return;

        Direction expected = _currentSequence[_seqIndex];
        if (input.Value == expected)
        {
            MarkUI(_seqIndex, hitColor);
            _seqIndex++;

            if (sfxOk) sfxOk.Play();

            if (_seqIndex >= _currentSequence.Count)
            {
                RoundSuccess();
            }
            else
            {
                MarkUI(_seqIndex, nextColor);
                _keyDeadline = Time.time + CurrentPerKeyTime();
            }
        }
        else
        {
            FailInput();
        }
    }


    private void StartRound()
    {
        _waitingNextRound = false;

        ClearSequenceUI();

        int length = Mathf.Clamp(roundLengths[Mathf.Clamp(_roundIndex, 0, roundLengths.Length - 1)], 1, 10);
        GenerateSequence(length);

        BuildSequenceUI();

        _seqIndex = 0;
        if (_uiArrows.Count > 0) MarkUI(0, nextColor);

        _keyDeadline = Time.time + CurrentPerKeyTime();

        if (sfxRound) sfxRound.Play();
    }

    private void RoundSuccess()
    {
        if (playerKiteAnim) playerKiteAnim.SetTrigger("player_attack");
        onPlayerHit?.Invoke();

        _roundIndex++;
        if (_roundIndex >= roundLengths.Length)
        {
            if (playerKiteAnim) playerKiteAnim.SetTrigger("cut_enemy");
            if (sfxWin) sfxWin.Play();
            onPlayerWin?.Invoke();
            Won();
            return;
        }

        // Preparar siguiente ronda con un pequeño delay
        StartCoroutine(NextRoundAfterDelay());
    }

    private IEnumerator NextRoundAfterDelay()
    {
        _waitingNextRound = true;
        yield return new WaitForSeconds(betweenRoundsDelay);

        UpdateRoundText();
        StartRound();
    }

    private void FailInput()
    {
        if (_seqIndex < _uiArrows.Count) MarkUI(_seqIndex, failColor);

        if (enemyKiteAnim) enemyKiteAnim.SetTrigger("enemy_attack");
        if (sfxWrong) sfxWrong.Play();
        onEnemyHit?.Invoke();

        Lose();
    }

    private void Lose()
    {
        if (_currentState == State.End) return;

        if (enemyKiteAnim) enemyKiteAnim.SetTrigger("cut_player");
        if (sfxLose) sfxLose.Play();
        onEnemyWin?.Invoke();

        _currentState = State.End; // cerrar loop
    }


    private Direction? ReadInput()
    {
        if (Input.GetButtonDown("DirectionUp")) return Direction.Up;
        if (Input.GetButtonDown("DirectionDown")) return Direction.Down;
        if (Input.GetButtonDown("DirectionLeft")) return Direction.Left;
        if (Input.GetButtonDown("DirectionRight")) return Direction.Right;

        return null;
    }

    private float CurrentPerKeyTime()
    {
        float factor = Mathf.Pow(roundTimeMultiplier, _roundIndex);
        return Mathf.Max(0.12f, perKeyTime * factor);
    }

    private void GenerateSequence(int length)
    {
        _currentSequence.Clear();
        
        Direction? last = null;
        int sameCount = 0;

        for (int i = 0; i < length; i++)
        {
            Direction direction = (Direction)Random.Range(0, 4);
            if (last.HasValue && direction == last.Value)
            {
                sameCount++;
                if (sameCount >= 1) // evita repetición
                {
                    direction = (Direction)(((int)direction + Random.Range(1, 4)) % 4);
                    sameCount = 0;
                }
            }
            else sameCount = 0;

            last = direction;
            _currentSequence.Add(direction);
        }
    }

    private void BuildSequenceUI()
    {
        ClearSequenceUI();
        foreach (var direction in _currentSequence)
        {
            var arrowGo = Instantiate(arrowItemPrefab, sequenceContainer);
            var img = arrowGo.GetComponent<Image>();
            img.sprite = DirectionToSprite(direction);
            img.color = idleColor;
            _uiArrows.Add(img);
        }
    }

    private void ClearSequenceUI()
    {
        foreach (Transform t in sequenceContainer) Destroy(t.gameObject);
        _uiArrows.Clear();
    }

    private void MarkUI(int index, Color color)
    {
        if (index < 0 || index >= _uiArrows.Count) return;
        _uiArrows[index].color = color;
    }

    private Sprite DirectionToSprite(Direction direction)
    {
        return direction switch
        {
            Direction.Up => spriteUp,
            Direction.Down => spriteDown,
            Direction.Left => spriteLeft,
            Direction.Right => spriteRight,
            _ => spriteUp
        };
    }
    
    private void UpdateTimerUI()
    {
        if (!timerText) return;
        float totalLeft = TimeLeft01();                // 0..1
        float secondsLeft = Mathf.Max(0f, timeLimit * totalLeft);
        timerText.text = $"Tiempo: {secondsLeft:0.0}s";
    }

    private void UpdateRoundText()
    {
        if (!roundText) return;
        roundText.text = $"Ronda: {_roundIndex + 1}/{roundLengths.Length}";
    }
}
