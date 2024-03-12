using System;
using System.Collections.Generic;

public class ObjectiveEvent {}

public static class ObjectiveEventManager {

    static readonly Dictionary<Type, Action<ObjectiveEvent>> s_Events = new Dictionary<Type, Action<ObjectiveEvent>>();

    static readonly Dictionary<Delegate, Action<ObjectiveEvent>> s_EventLookups = new Dictionary<Delegate, Action<ObjectiveEvent>>();

    public static void AddListener<T>(Action<T> evt) where T : ObjectiveEvent {
        if (!s_EventLookups.ContainsKey(evt))
        {
            Action<ObjectiveEvent> newAction = (e) => evt((T) e);
            s_EventLookups[evt] = newAction;
// If event is already in s_Events dictionary amend its Action
            if (s_Events.TryGetValue(typeof(T), out Action<ObjectiveEvent> internalAction))
                s_Events[typeof(T)] = internalAction += newAction;
            else
                s_Events[typeof(T)] = newAction;
        }
    }

    public static void RemoveListener<T>(Action<T> evt) where T : ObjectiveEvent {
        if (s_EventLookups.TryGetValue(evt, out var action))
        {
            if (s_Events.TryGetValue(typeof(T), out var tempAction))
            {
                tempAction -= action;
                if (tempAction == null)
                    s_Events.Remove(typeof(T));
                else
                    s_Events[typeof(T)] = tempAction;
            }

            s_EventLookups.Remove(evt);
        }
    }

    public static void Broadcast(ObjectiveEvent evt) {
        if (s_Events.TryGetValue(evt.GetType(), out var action))
            action.Invoke(evt);
    }

    public static void Clear() {
        s_Events.Clear();
        s_EventLookups.Clear();
    }
}
