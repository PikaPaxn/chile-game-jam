using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class KiteWarGame : MinigameController
{
    [Header("Rondas (cantidad de flechas por ronda, en orden)")]
    [SerializeField] private int[] roundLengths = { 1, 3, 5 };

    [Header("Tiempo por RONDA (segundos)")]
    [Tooltip("Ventana base para completar TODA la secuencia de la ronda.")]
    [SerializeField, Range(0.5f, 10f)] private float perRoundTime = 3.0f;

    [Tooltip("Multiplicador por ronda. Ej: 0.9f = cada ronda 10% menos de tiempo total.")]
    [SerializeField, Range(0.6f, 1.0f)] private float roundTimeMultiplier = 0.9f;

    [Tooltip("Pausa corta entre rondas (segundos)")]
    [SerializeField, Range(0f, 5f)] private float betweenRoundsDelay = 0.6f;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text roundText;
    
    [Header("Round Progress UI")]
    [SerializeField] private Slider roundProgress;
    [SerializeField] private bool flashOnHurry = true;
    [SerializeField, Range(0f, 0.9f)] private float hurryThreshold = 0.2f;
    [SerializeField] private Animator roundBarAnim;  // animador opcional para parpadeo
    
    [Header("Round Progress Sliders Colors")]
    [SerializeField] private Image roundProgressFill;
    [SerializeField] private Color sliderColorFilled  = new(0.20f, 0.80f, 0.20f);
    [SerializeField] private Color sliderColorHalf = new(1.00f, 0.85f, 0.20f);
    [SerializeField] private Color liderColorEmpty    = new(0.95f, 0.25f, 0.20f);
    
    [Tooltip("Contenedor donde se dibuja la secuencia de flechas")]
    [SerializeField] private Transform sequenceContainer;
    
    [Tooltip("Prefab de un ítem de flecha (Image). El script pintará su sprite y color.")]
    [SerializeField] private GameObject arrowItemPrefab;

    [Header("Sprites flechas")]
    [SerializeField] private Sprite spriteUp, spriteDown, spriteLeft, spriteRight;

    [Header("Colores UI")]
    [SerializeField] private Color nextColor = Color.white;
    [SerializeField] private Color hitColor  = new(0.3f, 1f, 0.3f);
    [SerializeField] private Color failColor = new(1f, 0.3f, 0.3f);
    [SerializeField] private Color idleColor = new(1f, 1f, 1f, 0.4f);

    [Header("Feedback / Audio")]
    [SerializeField] private AudioSource sfxOk, sfxWrong, sfxRound, sfxWin, sfxLose;

    [Header("Animators (opcional)")]
    [SerializeField] private Animator playerKiteAnim, enemyKiteAnim;

    [Header("Eventos")]
    public UnityEvent onPlayerHit, onEnemyHit, onPlayerWin, onEnemyWin;

    private enum Direction { Up, Down, Left, Right }

    private readonly List<Direction> _currentSequence = new();
    private readonly List<Image> _uiArrows = new();

    private int _roundIndex;        // ronda actual
    private int _seqIndex;          // índice dentro de la secuencia
    private bool _waitingNextRound; // en delay entre rondas
    private float _roundDeadline;
    
    private float _roundDuration;   // ← tiempo límite de la ronda

    private void OnEnable()
    {
        //StartGame();
    }

    public override void StartGame()
    {
        _roundIndex = 0;
        _waitingNextRound = false;

        ClearSequenceUI();
        UpdateRoundText();
        UpdateTimerUI(0f);

        base.StartGame();

        StartRound();
    }

    private void Update()
    {
        if (_currentState != State.Playing) return;

        if (TimeLeft01() <= 0f) 
        {
            LocalLose();
            return;
        }

        if (_waitingNextRound) return;

        float roundTimeLeft = Mathf.Max(0f, _roundDeadline - Time.time);
        UpdateRoundUI(roundTimeLeft);
        UpdateTimerUI(roundTimeLeft);

        if (roundTimeLeft <= 0f)
        {
            FailInput(); // Se acabó el tiempo de la ronda sin completar la secuencia
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

        int length = Mathf.Clamp(roundLengths[Mathf.Clamp(_roundIndex, 0, roundLengths.Length - 1)], 1, 20);
        GenerateSequence(length);
        BuildSequenceUI();

        _seqIndex = 0;
        if (_uiArrows.Count > 0) MarkUI(0, nextColor);

        _roundDuration = CurrentRoundTime();
        _roundDeadline = Time.time + CurrentRoundTime();

        if (sfxRound) sfxRound.Play();
        
        UpdateRoundText();
        UpdateRoundProgressUI(1f);
        UpdateTimerUI(_roundDeadline - Time.time);

        if (roundProgressFill != null)
            roundProgressFill.color = sliderColorFilled;
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

        StartCoroutine(NextRoundAfterDelay());
    }

    private IEnumerator NextRoundAfterDelay()
    {
        _waitingNextRound = true;
        // UpdateRoundProgressUI(0f);
        // UpdateTimerUI(0f); // pausa entre rondas
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

        LocalLose();
    }

    private void LocalLose()
    {
        if (_currentState == State.End) return;
        
        // UpdateRoundProgressUI(0f);
        if (roundBarAnim) roundBarAnim.SetBool("hurry", false);

        if (enemyKiteAnim) enemyKiteAnim.SetTrigger("cut_player");
        if (sfxLose) sfxLose.Play();
        onEnemyWin?.Invoke();

        _currentState = State.End;
        
        Lose();
    }


    private Direction? ReadInput()
    {
        if (Input.GetButtonDown("DirectionUp")) return Direction.Up;
        if (Input.GetButtonDown("DirectionDown")) return Direction.Down;
        if (Input.GetButtonDown("DirectionLeft")) return Direction.Left;
        if (Input.GetButtonDown("DirectionRight")) return Direction.Right;
        return null;
    }

    private float CurrentRoundTime()
    {
        // reduce el tiempo total permitido por ronda
        float factor = Mathf.Pow(roundTimeMultiplier, _roundIndex);
        return Mathf.Max(0.25f, perRoundTime * factor);
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
                if (sameCount >= 1) // evita 2 iguales seguidas
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
    private void UpdateTimerUI(float roundSecondsLeft)
    {
        if (!timerText) return;

        if (roundSecondsLeft > 0f)
            timerText.text = $"{roundSecondsLeft:0.0}s";
        else
        {
            float totalLeft01 = TimeLeft01();
            float secondsLeft = Mathf.Max(0f, timeLimit * totalLeft01);
            timerText.text = $"{secondsLeft:0.0}s";
        }
    }

    private void UpdateRoundText()
    {
        if (!roundText) return;
        roundText.text = $"Ronda: {_roundIndex + 1}/{roundLengths.Length}";
    }
    
    private void UpdateRoundUI(float roundTimeLeft)
    {
        UpdateTimerUI(roundTimeLeft);
        if (_roundDuration > 0f)
        {
            float t01 = roundTimeLeft / _roundDuration;      // 1 → 0
            UpdateRoundProgressUI(t01);
        }
    }
    
    private void UpdateRoundProgressUI(float t01)
    {
        t01 = Mathf.Clamp01(t01);

        if (roundProgress != null)
            roundProgress.value = t01;
        
        if (roundProgressFill != null)
        {
            Color c;
            if (t01 >= 0.5f)
            {
                // 1..0.5 => sliderColorFilled -> sliferColorHalf
                float k = Mathf.InverseLerp(0.5f, 1f, t01);   // k=1 en 1.0, k=0 en 0.5
                c = Color.Lerp(sliderColorHalf, sliderColorFilled, k);
            }
            else
            {
                // 0.5..0 => sliferColorHalf -> sliderColorEmpty
                float k = Mathf.InverseLerp(0f, 0.5f, t01);   // k=1 en 0.5, k=0 en 0
                c = Color.Lerp(liderColorEmpty, sliderColorHalf, k);
            }
            roundProgressFill.color = c;
        }

        if (roundBarAnim != null && flashOnHurry)
            roundBarAnim.SetBool("hurry", t01 <= hurryThreshold);
    }
}
