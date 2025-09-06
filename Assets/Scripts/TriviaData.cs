using UnityEngine;

[CreateAssetMenu]
public class TriviaData : ScriptableObject
{
    public GameObject prefab;
    public float preferredRotation;
    public float preferredScale;
    public TriviaQuestion[] questions;
}

/// <summary>
/// The answer expects a child go name from the prefab in the trivia data
/// </summary>
[System.Serializable]
public struct TriviaQuestion {
    public string question;
    public string answer;
}
