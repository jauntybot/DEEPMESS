using UnityEngine;

public static class ObjectiveEvents {

    public static GridElementDestroyedEvent GridElementDestroyedEvent = new();
    public static GridElementDamagedEvent GridElementDamagedEvent = new();

    public static UnitConditionEvent UnitConditionEvent = new();
    public static OnEquipmentUse OnEquipmentUse = new();
    public static EndTurnEvent EndTurnEvent = new();

    public static FloorPeekEvent FloorPeekEvent = new();
    public static FloorDescentEvent FloorDescentEvent = new();

}

public class GridElementDamagedEvent : ObjectiveEvent {
    public GridElement element = null;
    public int dmg = 0;
    public GridElement.DamageType damageType = GridElement.DamageType.Unspecified;
    public GridElement source = null;
    public EquipmentData sourceEquip = null;

}

public class GridElementDestroyedEvent : ObjectiveEvent {
    public GridElement element = null;
    public GridElement.DamageType damageType = GridElement.DamageType.Unspecified;
    public GridElement source = null;
    public EquipmentData sourceEquip = null;

}

public class OnEquipmentUse : ObjectiveEvent {
    public EquipmentData data = null;
    public GridElement user = null;
    public GridElement target = null;
}

public class UnitConditionEvent : ObjectiveEvent {
    public Unit target;
    public Unit.Status condition = Unit.Status.Normal;
    public bool undo = false;

}

public class EndTurnEvent : ObjectiveEvent {
    public ScenarioManager.Turn toTurn = ScenarioManager.Turn.Null;
}

public class FloorPeekEvent : ObjectiveEvent {
    public bool down = true;
}

public class FloorDescentEvent : ObjectiveEvent {
    public int floorIndex = 0;
    public int enemyDescentsCount = 0;
}
