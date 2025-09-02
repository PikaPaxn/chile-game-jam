using UnityEngine;
using UnityEngine.EventSystems;

public class VegaItem : DragAndDrop
{
    Sprite _myIcon;
    
    public void Initialize(Sprite icon) {
        _myIcon = icon;
        GetComponent<SpriteRenderer>().sprite = icon;
    }

    public override bool ShouldStay() {
        var hit = Physics2D.OverlapPointAll(transform.position);
        Debug.Log($"Kyaaaa... I got dropped at {transform.position} and hit {hit}");

        // We hit nothing, ignore
        if (hit.Length < 2) {
            return false;
        }

        // Check if we hit the box
        var box = hit[1].GetComponent<VegaBox>();
        if (box == null) {
            return false;
        }

        var success = box.CheckIcon(_myIcon);
        Debug.Log($"I got hit! Was it right? {success}");
        return true;
    }
}
