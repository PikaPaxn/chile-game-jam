using UnityEngine;

public static class MonoBehaviourExtensions
{
    public static MinigameController GetMinigameController(this MonoBehaviour mono) => mono.GetComponentInParent<MinigameController>();
}
