using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Floors/Floor Asset")]
[System.Serializable]
public class FloorAsset : ScriptableObject {
    new public string name;
    public GameObject prefab;
    public Sprite icon;
}