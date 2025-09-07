using UnityEngine;
using UnityEngine.Events;
public class OnEnableTrigger : MonoBehaviour
{
    public UnityEvent onEnable;

    private void OnEnable() {
        onEnable.Invoke();
    }
}
