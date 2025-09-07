using UnityEngine;

public class ObjectVibration : MonoBehaviour
{
    [Range(0f, 1f)]
    public float intensity = 0.5f;

    public float frequency = 25f;
    public float amplitude = 0.1f;

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        if (intensity > 0f)
        {
            float offsetX = Mathf.Sin(Time.time * frequency) * amplitude * intensity;
            float offsetY = Mathf.Cos(Time.time * frequency * 1.1f) * amplitude * intensity;

            transform.localPosition = initialPosition + new Vector3(offsetX, offsetY, 0);
        }
        else
        {
            transform.localPosition = initialPosition;
        }
    }
}
