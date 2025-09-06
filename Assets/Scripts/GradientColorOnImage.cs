using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class GradientColorOnImage : MonoBehaviour
{
    [Header("Colores")]
    public Color centerColor = new Color(0f, 1f, 0f, 0.35f);
    public Color edgeColor   = new Color(1f, 0f, 0f, 0.35f);

    [Header("Orientación")]
    public bool vertical = false;

    [Header("Calidad")]
    [Range(16, 1024)] public int resolution = 256;

    [Header("Mejora")]
    public bool useHSV = true;               // ← usar interpolación en HSV (recomendado)
    [Range(0.2f, 4f)]
    public float curvePower = 1f;            // ← 1=linear; >1 ensancha el verde central
    
    Image _img;
    Texture2D _tex;
    Sprite _sprite;

    void OnEnable()
    {
        _img = GetComponent<Image>(); Build();
    }

    void OnValidate()
    {
        _img = GetComponent<Image>(); Build();
    }

    void Build()
    {
        if (_img == null) return;

        int w = vertical ? 1 : Mathf.Max(2, resolution);
        int h = vertical ? Mathf.Max(2, resolution) : 1;

        if (_tex == null || _tex.width != w || _tex.height != h)
        {
            if (_tex != null)
            {
#if UNITY_EDITOR
                if (Application.isPlaying) Destroy(_tex); 
                else DestroyImmediate(_tex);
#else
                Destroy(_tex);
#endif
            }
            _tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
        }

        // pintar gradiente: distancia al centro -> más rojo
        if (vertical)
        {
            for (int y = 0; y < h; y++)
            {
                float v = (float)y / (h - 1);     // 0..1
                float d = Mathf.Abs(2f * v - 1f); // 0 en centro, 1 en bordes
                float t = Mathf.Clamp01(Mathf.Pow(d, curvePower));
                
                Color c = useHSV ? LerpHSV(centerColor, edgeColor, t)
                    : Color.Lerp(centerColor, edgeColor, t);
                
                _tex.SetPixel(0, y, c);
            }
        }
        else
        {
            for (int x = 0; x < w; x++)
            {
                float u = (float)x / (w - 1);     // 0..1
                float d = Mathf.Abs(2f * u - 1f); // 0 en centro, 1 en bordes
                float t = Mathf.Clamp01(Mathf.Pow(d, curvePower));
                
                Color c = useHSV ? LerpHSV(centerColor, edgeColor, t)
                    : Color.Lerp(centerColor, edgeColor, t);
                
                _tex.SetPixel(x, 0, c);
            }
        }
        _tex.Apply();

        // asignar sprite
        if (_sprite != null)
        {
#if UNITY_EDITOR
            if (Application.isPlaying) Destroy(_sprite); else DestroyImmediate(_sprite);
#else
            Destroy(_sprite);
#endif
        }
        _sprite = Sprite.Create(_tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100);
        _img.sprite = _sprite;
        _img.type = Image.Type.Simple;
        _img.preserveAspect = false;
        _img.color = Color.white; // no multiplicar
    }
    
    static Color LerpHSV(Color a, Color b, float t)
    {
        Color.RGBToHSV(a, out float ha, out float sa, out float va);
        Color.RGBToHSV(b, out float hb, out float sb, out float vb);

        // mover hue por el camino corto (maneja salto 0..1)
        float dh = hb - ha;
        if (dh >  0.5f) dh -= 1f;
        if (dh < -0.5f) dh += 1f;

        float h = ha + dh * t;
        if (h < 0f) h += 1f;
        if (h > 1f) h -= 1f;

        float s = Mathf.Lerp(sa, sb, t);
        float v = Mathf.Lerp(va, vb, t);

        Color c = Color.HSVToRGB(h, s, v);
        c.a = Mathf.Lerp(a.a, b.a, t);
        return c;
    }
}
