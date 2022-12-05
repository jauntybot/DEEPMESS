using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[CreateAssetMenu(menuName = "Grids/Grid Definition")]
public class GridDefinition : ScriptableObject
{

    public InitContents contents;

}

[System.Serializable]
public class InitContents {
    public enum Contents { Empty, Wall, Enemy }
    public Contents[,] defintion;
}