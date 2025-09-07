using UnityEngine;
using UnityEngine.EventSystems;

public class VegaItem : DragAndDrop
{
    VegaSprites _myIcon;
    ItemCallbacks _callbacks;

    public void Initialize(VegaSprites icon, ItemCallbacks callbacks = null) {
        _myIcon = icon;
        GetComponent<SpriteRenderer>().sprite = icon.fruit;
        _callbacks = callbacks;

        if (GetComponent<BoxCollider2D>() != null)
            Destroy(GetComponent<BoxCollider2D>());
        gameObject.AddComponent<BoxCollider2D>();
    }

    public override bool ShouldStay() {
        var hit = Physics2D.OverlapPointAll(transform.position);
        //Debug.Log($"Kyaaaa... I got dropped at {transform.position} and hit {hit}");

        // We hit nothing, ignore
        if (hit.Length < 2) {
            return false;
        }

        // Check if we hit the box
        var box = hit[1].GetComponent<VegaBox>();
        if (box == null) {
            return false;
        }

        // Check if right
        var success = box.CheckIcon(_myIcon);
        if (success) {
            gameObject.SetActive(false);
            transform.SetAsLastSibling();
            _callbacks?.onCorrectBox.Invoke();
        } else {
            _callbacks?.onWrongBox.Invoke();
        }

        return true;
    }

    public class ItemCallbacks {
        public System.Action onCorrectBox;
        public System.Action onWrongBox;
    }
}
