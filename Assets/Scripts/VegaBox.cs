using UnityEngine;

public class VegaBox : MonoBehaviour
{
    VegaSprites _myIcon;
    public void Initialize(VegaSprites icon) {
        _myIcon = icon;
        GetComponent<SpriteRenderer>().sprite = icon.box;
    }

    public bool CheckIcon(VegaSprites icon) => _myIcon == icon;
}
