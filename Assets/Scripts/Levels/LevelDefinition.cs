using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[CreateAssetMenu(menuName = "Grid/Level Definition")]
public class LevelDefinition : ScriptableObject
{

    public List<Content> initSpawns;

}

[System.Serializable]
public class Content {

    public Vector2 coord;
    public GridElement gridElement;
}

