using System.Collections.Generic;
using UnityEngine;

public class VegaGame : MinigameController {
    [Header("Object References")]
    public VegaBox[] boxes;
    public VegaSprites[] icons;
    List<VegaSprites> _chosenIcons;

    public GameObject itemPrefab;
    public Transform itemsParent;

    [Header("Items config")]
    public int itemsToSort = 5;
    int _correctItems;
    List<VegaSprites> _currentList;
    [SerializeField] float distanceBetweenItems;
    [SerializeField] float currentItemScale = 1.2f;
    [SerializeField] float inactiveItemScale = 0.8f;


    public override void StartGame() {
        base.StartGame();
        // Clear the lists
        _chosenIcons = new();
        _currentList = new();
        _correctItems = 0;
        
        // Initialize everything
        InitializeBoxes();
        InitializeQueue();
    }

    void InitializeBoxes() {
        foreach (var box in boxes) {
            // Pick icon with no repetition
            var currentIcon = icons.RandomPick(_chosenIcons.ToArray());
            _chosenIcons.Add(currentIcon);

            box.Initialize(currentIcon);
        }
    }

    void InitializeQueue() {
        // Initialize the Queue
        itemsParent.DestroyChildren();
        VegaItem.ItemCallbacks itemCallbacks = new();
        itemCallbacks.onCorrectBox = ItemOnCorrectBox;
        itemCallbacks.onWrongBox = ItemOnWrongBox;

        for (int i = 0; i < itemsToSort; i++) {
            var currentIcon = _chosenIcons.RandomPick();
            _currentList.Add(currentIcon);

            var itemGo = Instantiate(itemPrefab, itemsParent);
            var vegaItem = itemGo.GetComponent<VegaItem>();
            vegaItem.Initialize(currentIcon, itemCallbacks);
            vegaItem.enabled = i == 0;
            itemGo.transform.localPosition = Vector3.zero + distanceBetweenItems * i * Vector3.right;
            itemGo.transform.localScale = Vector3.one * ((i == 0) ? currentItemScale : inactiveItemScale);
        }

    }

    void ReorderItems() {
        for (int i = 0; i < itemsParent.childCount; i++) {
            var child = itemsParent.GetChild(i);
            if (!child.gameObject.activeInHierarchy)
                break;

            child.localPosition = Vector3.zero + distanceBetweenItems * i * Vector3.right;
            if (i == 0) {
                child.localScale = Vector3.one * currentItemScale;
                child.GetComponent<VegaItem>().enabled = true;
            }
        }
    }

    void ItemOnCorrectBox() {
        _correctItems++;
        if (_correctItems >= itemsToSort) {
            Won();
        }

        ReorderItems();
    }

    void ItemOnWrongBox() {
        Lose();
    }
}

[System.Serializable]
public class VegaSprites {
    public Sprite box;
    public Sprite fruit;
}
