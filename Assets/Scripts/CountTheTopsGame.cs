using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CountTheTopsGame : MinigameController
{
    [Header("Game Flow")]
    [SerializeField, Min(0f)] private float stopSpawningBeforeEnd = 4f;
    
    [Header("Spawner / Movimiento")]
    [SerializeField] private GameObject trompoPrefab;
    [SerializeField] private Transform spawnParent;     // organiza la jerarquía
    [SerializeField] private float minSpawnInterval = 0.35f;
    [SerializeField] private float maxSpawnInterval = 1.10f;
    [SerializeField] private float minSpeed = 2.5f;
    [SerializeField] private float maxSpeed = 5.5f;

    [Tooltip("Rango vertical en coordenadas de viewport (0-1).")]
    [SerializeField] private Vector2 viewportYRange = new Vector2(0.25f, 0.80f);

    [Tooltip("X de spawn/despawn en coordenadas de viewport (fuera de la pantalla).")]
    [SerializeField] private float spawnViewportX = 1.15f;
    [SerializeField] private float despawnViewportX = -0.15f;

    [Tooltip("Profundidad para ViewportToWorldPoint (0 para 2D ortográfico).")]
    [SerializeField] private float worldDepth = 0f;

    [Header("UI")]
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private TMP_Text timerText;

    [Header("Inputs")]
    [SerializeField] private bool allowSpace = true;
    [SerializeField] private bool allowFire1 = true;
    private InputAction input;

    [Header("Eventos")]
    public UnityEvent onWin;
    public UnityEvent onLose;

    [Header("Dev / Debug")]
    [SerializeField] private int playerCount;
    [SerializeField] private int actualCount;

    private Coroutine _spawnLoop;

    private void Start() {
        input = InputSystem.actions.FindAction("AnyButton");
    }

    public override void StartGame()
    {
        base.StartGame();
        playerCount = 0;
        actualCount = 0;
        UpdateUI();

        if (_spawnLoop != null) StopCoroutine(_spawnLoop);
        _spawnLoop = StartCoroutine(SpawnLoop());
    }

    private void Update()
    {
        if (_currentState != State.Playing) return;

        HandleInputs();
        UpdateTimerUI();

        if (TimeLeft01() <= 0f)
        {
            FinishAndEvaluate();
        }
    }

    private void HandleInputs()
    {
        // Evita doble conteo si se hace clic sobre UI: el botón UI llamará a OnPressCount() aparte
        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        //bool pressed = false;
        //if (!pressed && allowSpace && Input.GetKeyDown(KeyCode.Space))
        //    pressed = true;

        //// if (!pressed && allowFire1 && !overUI && Input.GetButtonDown("Fire1"))
        //if (!pressed && allowFire1 && Input.GetButtonDown("Fire1"))
        //    pressed = true;

        bool pressed = input.GetButtonDown() && !overUI;

        if (pressed)
            IncrementPlayerCount();
    }

    private void IncrementPlayerCount()
    {
        if (_currentState != State.Playing) return;
        playerCount++;
        UpdateUI();
    }

    // Llama este método desde un botón UI
    public void OnPressCount()
    {
        if (_currentState != State.Playing) return;
        IncrementPlayerCount();
    }

    private void UpdateUI()
    {
        if (counterText) counterText.text = playerCount.ToString();
    }

    private void UpdateTimerUI()
    {
        if (!timerText) return;
        float remaining = Mathf.Max(0f, timeLimit * TimeLeft01());
        timerText.text = remaining.ToString("0.0") + "s";
    }

    private IEnumerator SpawnLoop()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[CuentaTrompos] No hay Camera.main en la escena.");
            yield break;
        }

        while (_currentState == State.Playing)
        {
            if (RemainingSeconds() <= stopSpawningBeforeEnd) break;
            
            float wait = Random.Range(minSpawnInterval, maxSpawnInterval);
            
            // Para despertar justo cuando llegue la ventana y no spawnear tarde.
            float untilStop = Mathf.Max(0f, RemainingSeconds() - stopSpawningBeforeEnd);
            yield return new WaitForSeconds(Mathf.Min(wait, untilStop));

            if (_currentState != State.Playing || TimeLeft01() <= 0f || RemainingSeconds() <= stopSpawningBeforeEnd) break;

            float vy = Random.Range(viewportYRange.x, viewportYRange.y);
            float vz = cam.orthographic ? worldDepth : Mathf.Max(0.5f, worldDepth == 0 ? 10f : worldDepth); // distancia útil en perspectiva
            Vector3 spawnWorld = cam.ViewportToWorldPoint(new Vector3(spawnViewportX, vy, vz+10));

            GameObject go = Instantiate(trompoPrefab, spawnWorld, Quaternion.identity, spawnParent);
            var mover = go.GetComponent<TrompoMover>();
            if (mover == null) mover = go.AddComponent<TrompoMover>();

            mover.Speed = Random.Range(minSpeed, maxSpeed);
            mover.DespawnViewportX = despawnViewportX;
            mover.WorldDepthLock = vz;
            mover.UseWorldDepthLock = cam.orthographic == false; // en 3D/perspectiva, fija la Z a la misma distancia
            mover.RandomizeSpin();

            actualCount++;
            
            if (IsPaused) {
                yield return new WaitWhile(() => { return IsPaused; });
            }
        }
    }

    private void FinishAndEvaluate()
    {
        if (_spawnLoop != null) StopCoroutine(_spawnLoop);
        _spawnLoop = null;

        bool win = playerCount == actualCount;

        _currentState = State.End;
        if (win)
        {
            Won();
            onWin?.Invoke();
        }
        else
        {
            Lose();
            onLose?.Invoke();
        }
    }
    
    private float RemainingSeconds() => Mathf.Max(0f, timeLimit * TimeLeft01());
}
