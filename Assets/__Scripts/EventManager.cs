using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : PersistentSingleton<EventManager>
{   
    private Dictionary<string, Action> eventDictionary = new();

    public static void AddListener(string eventName, Action listener)
    {
        Action thisEvent;

        if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            // Entry exists - add listener
            thisEvent += listener;
            Instance.eventDictionary[eventName] = thisEvent;
        }
        else
        {
            // Entry doesn't exist - create new entry.
            thisEvent += listener;
            Instance.eventDictionary.Add(eventName, thisEvent);
        }
    }

    public static void RemoveListener(string eventName, Action listener)
    {
        if (Instance == null) return;

        Action thisEvent;

        if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent -= listener;
            Instance.eventDictionary[eventName] = thisEvent;
        }
    }

    public static void InvokeEvent(string eventName)
    {
        if (Instance.eventDictionary.TryGetValue(eventName, out Action thisEvent))
        {
            thisEvent.Invoke();
        }
    }
}
