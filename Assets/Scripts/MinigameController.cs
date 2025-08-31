using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MinigameController : MonoBehaviour
{
    public float time;
    float _startTime;
    bool _hasWon;


    public void StartGame() {
        _hasWon = false;
        _startTime = Time.time;
    }

    public void Won() {
        Debug.Log("You won the Minigame!", gameObject);
        _hasWon = true;
    }

    public bool HasWon => _hasWon;

    public bool IsInstanced => gameObject.scene.name != null;


    /// <summary>
    /// Returns how much time is left
    /// </summary>
    /// <returns>1 is all time is left, 0 is time has expired</returns>
    public float TimeLeft01() {
        float timePassed = Time.time - _startTime;
        return 1f - Mathf.InverseLerp(0, time, timePassed);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MinigameController))]
public class MinigameControllerEditor : Editor {
    public override VisualElement CreateInspectorGUI() {
        return base.CreateInspectorGUI();
    }
}
#endif