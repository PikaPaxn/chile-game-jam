using UnityEngine;
using UnityEngine.UI;

public class AnimateUIUV : MonoBehaviour
{
    [SerializeField] private string textureRef = "_MainTex";
    [SerializeField] private Vector2 speed = Vector2.one;
    private Material _scrollingMaterial;
    private Image _image;

    private void Start() {
        _image = GetComponent<Image>();
        _scrollingMaterial = new Material(_image.material);
        _image.material = _scrollingMaterial;
    }

    private void Update() {
        var offset = speed * Time.time;
        _scrollingMaterial.SetTextureOffset(textureRef, offset);
    }
}
