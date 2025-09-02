using System.Collections.Generic;
using UnityEngine;

public class VegaGame : MinigameController
{
    public GameObject[] boxes;
    public Sprite[] icons;
    List<Sprite> _chosenIcons;

    public int itemsToSort = 5;
    List<Sprite> _currentList;

    public override void StartGame() {
        base.StartGame();
        // Clear the lists
        _chosenIcons = new();
        _currentList = new();
        InitializeBoxes();
    }

    void InitializeBoxes() {
        foreach(var box in boxes) {
            // Pick icon with no repetition
            var currentIcon = icons.RandomPick(_chosenIcons.ToArray());
            _chosenIcons.Add(currentIcon);

            // If Icon has been created, replace it
            if (box.transform.childCount > 0 && box.transform.GetChild(0).name == "Icon") {
                box.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = currentIcon;
            } else {
                // If not, create it from scratch.
                // Clear the old icon
                box.transform.DestroyChildren();

                // Add the icon to the box
                var iconGO = new GameObject("Icon");
                var render = iconGO.AddComponent<SpriteRenderer>();
                render.sprite = currentIcon;

                // Make sure it has the proper scale
                iconGO.transform.localScale = Vector3.one;
                iconGO.transform.SetParent(box.transform, true);
                iconGO.transform.localPosition = new Vector3(0, 0, -0.1f);
                iconGO.transform.localRotation = Quaternion.identity;
            }
        }
    }
}
