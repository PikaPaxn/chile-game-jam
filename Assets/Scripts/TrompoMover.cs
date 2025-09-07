using UnityEngine;

public class TrompoMover : MonoBehaviour
{
    [field: SerializeField] public float Speed { get; set; } = 4f;
    [field: SerializeField] public float DespawnViewportX { get; set; } = -0.1f;

    [Header("Spin")]
    [SerializeField] private Vector3 spinEulerPerSecond = new Vector3(0f, 0f, 360f);

    [Header("3D / Perspectiva (opcional)")]
    public bool UseWorldDepthLock = false;
    public float WorldDepthLock = 10f;

    public void RandomizeSpin()
    {
        // Pequeña variación para que no todos roten igual
        float sign = Random.value < 0.5f ? -1f : 1f;
        float z = Random.Range(240f, 520f) * sign;
        spinEulerPerSecond = new Vector3(0f, 0f, z);
    }

    private void Update()
    {
        // Movimiento a la izquierda en espacio global
        transform.position += Vector3.left * Speed * Time.deltaTime;

        // Spin
        transform.Rotate(spinEulerPerSecond * Time.deltaTime, Space.Self);

        // Mantener z fija (útil en perspectiva para que no se acerque/aleje)
        if (UseWorldDepthLock)
        {
            Vector3 p = transform.position;
            p = new Vector3(p.x, p.y, WorldDepthLock);
            transform.position = p;
        }

        // Si sale de la pantalla a la izquierda, destruir
        Camera cam = Camera.main;
        if (cam)
        {
            Vector3 vp = cam.WorldToViewportPoint(transform.position);
            if (vp.x <= DespawnViewportX)
            {
                Destroy(gameObject);
            }
        }
    }
}