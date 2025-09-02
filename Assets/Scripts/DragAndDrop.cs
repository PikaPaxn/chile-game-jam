using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Basic class to drag and drop a 2d world items
/// It has a flag to know if it should go back to its original position when droping the item
/// If the flag is true, you can override ShouldStay() to change the drop behaviour.
/// </summary>
public class DragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /// <summary>
    /// Flag to return to its original position on drop.
    /// </summary>
    [Tooltip("Flag to return to its original position on drop.")]
    public bool shouldReturnToPos = true;
    Vector3 _startPosition;

    public void OnBeginDrag(PointerEventData eventData) {
        _startPosition = transform.position;
    }

    public void OnDrag(PointerEventData eventData) {
        Vector3 newPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        newPos.z = _startPosition.z;
        transform.position = newPos; 
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (shouldReturnToPos && !ShouldStay()) {
            transform.position = _startPosition;
        }
    }

    public virtual bool ShouldStay() => false;
}
