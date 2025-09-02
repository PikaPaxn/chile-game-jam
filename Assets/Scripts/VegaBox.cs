using UnityEngine;

public class VegaBox : MonoBehaviour
{
    Sprite _myIcon;
    public void Initialize(Sprite icon) {
        _myIcon = icon;

        // If Icon has been created, replace it
        if (transform.childCount > 0 && transform.GetChild(0).name == "Icon") {
            transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = icon;
        } else {
            // If not, create it from scratch.
            // Clear the old icon
            transform.DestroyChildren();

            // Add the icon to the box
            var iconGO = new GameObject("Icon");
            var render = iconGO.AddComponent<SpriteRenderer>();
            render.sprite = icon;

            // Make sure it has the proper scale
            iconGO.transform.localScale = Vector3.one;
            iconGO.transform.SetParent(transform, true);
            iconGO.transform.localPosition = new Vector3(0, 0, -0.1f);
            iconGO.transform.localRotation = Quaternion.identity;
        }
    }

    public bool CheckIcon(Sprite icon) {
        return _myIcon == icon;
    }
}
