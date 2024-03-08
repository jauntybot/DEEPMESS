using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GridElementAction {

    public class UpdateElement : GridElementAction {
        public Vector2 coord;
        public Action movement;
    }

}
