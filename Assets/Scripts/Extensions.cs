using UnityEngine;
using UnityEngine.InputSystem;

public static class MonoBehaviourExtensions
{
    public static MinigameController GetMinigameController(this MonoBehaviour mono) => mono.GetComponentInParent<MinigameController>();
}

/// Copied from https://discussions.unity.com/t/solved-getbuttondown-getbuttonup-with-the-new-system/787563/6
/// Thanks Fenrisul
/// For these to work the InputAction needs to have a "Press" interaction with "Press and Release" behaviour
public static class InputActionButtonExtensions {
    public static bool GetButton(this InputAction action) => action.ReadValue<float>() > 0;
    public static bool GetButtonDown(this InputAction action) => action.triggered && action.ReadValue<float>() > 0;
    public static bool GetButtonUp(this InputAction action) => action.triggered && action.ReadValue<float>() == 0;
    public static Vector2 GetVector2Down(this InputAction action) {
        if (action.triggered)
            return action.ReadValue<Vector2>();
        return Vector2.zero;
    }
}
