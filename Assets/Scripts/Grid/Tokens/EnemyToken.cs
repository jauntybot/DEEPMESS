using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyToken : Token {

    
    protected PriorityQueue<Vector2> frontier;
    protected Dictionary<Vector2, Vector2> cameFrom = new Dictionary<Vector2, Vector2>();

    
}
