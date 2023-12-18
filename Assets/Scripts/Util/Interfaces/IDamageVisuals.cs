using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageVisuals {

    GameObject splatterPrefab { get; set;}
    void Splatter(Vector2 dir);

}
