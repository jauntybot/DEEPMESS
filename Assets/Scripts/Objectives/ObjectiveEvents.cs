using UnityEngine;

public static class ObjectiveEvents {

    public static GridElementDestroyedEvent GridElementDestroyedEvent = new GridElementDestroyedEvent();

}

public class GridElementDestroyedEvent : ObjectiveEvent {
    public GridElement element;
    public GridElement.DamageType damageType;
    public GridElement source;

}
