using System.Collections.Generic;
using UnityEngine;

public class VegaGame : MinigameController
{
    public VegaBox[] boxes;
    public Sprite[] icons;
    List<Sprite> _chosenIcons;

    public int itemsToSort = 5;
    List<Sprite> _currentList;
    public GameObject itemPrefab;
    public Transform itemsParent;

    public override void StartGame() {
        base.StartGame();
        // Clear the lists
        _chosenIcons = new();
        _currentList = new();
        InitializeBoxes();
        InitializeQueue();
    }

    void InitializeBoxes() {
        foreach(var box in boxes) {
            // Pick icon with no repetition
            var currentIcon = icons.RandomPick(_chosenIcons.ToArray());
            _chosenIcons.Add(currentIcon);

            box.Initialize(currentIcon);
        }
    }

    void InitializeQueue() {
        // Initialize the Queue
        itemsParent.DestroyChildren();
        for (int i = 0; i < itemsToSort; i++) {
            var currentIcon = icons.RandomPick();
            _currentList.Add(currentIcon);

            var itemGo = Instantiate(itemPrefab, itemsParent);
            itemGo.GetComponent<VegaItem>().Initialize(currentIcon);
            itemGo.transform.localPosition = Vector3.zero + Vector3.right * i;
        }

    }
}
