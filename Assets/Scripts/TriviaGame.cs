using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TriviaGame : MinigameController
{
    [SerializeField] TextMeshProUGUI questionText;
    [SerializeField] Transform mapParent;
    [SerializeField] TriviaData[] data;

    public override void StartGame() {
        base.StartGame();
        mapParent.DestroyChildren();

        // Invoke new question
        var currentData = data.RandomPick();
        SetupData(currentData);
    }

    void SetupData(TriviaData data) {
        var currentQuestion = data.questions.RandomPick();
        questionText.text = currentQuestion.question;

        // Instantiate prefab
        var go = Instantiate(data.prefab, mapParent);
        go.transform.rotation = Quaternion.Euler(data.preferredRotation * Vector3.forward);
        go.transform.localScale = data.preferredScale * Vector3.one;

        // Setup buttons
        var buttons = GetComponentsInChildren<Button>();
        foreach (var button in buttons) {
            button.onClick.RemoveAllListeners();
            if (button.name == currentQuestion.answer) {
                button.onClick.AddListener(() => { Won(); });
            } else {
                button.onClick.AddListener(() => { Lose(); });
            }
        }
    }


}
