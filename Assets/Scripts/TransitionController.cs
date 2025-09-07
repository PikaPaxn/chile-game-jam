using UnityEngine;

public class TransitionController : MonoBehaviour
{

    public void OnTransitionComplete()
    {
        gameObject.SetActive(false);
    }
}
