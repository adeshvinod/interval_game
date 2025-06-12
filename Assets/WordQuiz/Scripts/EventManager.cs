using UnityEngine;
using System;

[CreateAssetMenu(fileName = "EventManager", menuName = "Managers/Event Manager")]
public class EventManager : ScriptableObject
{
    // Interval Option Events
    public delegate void IntervalOptionSelectedHandler(interval_option option);
    public event IntervalOptionSelectedHandler OnIntervalOptionSelected;

    public void SelectIntervalOption(interval_option option)
    {
        OnIntervalOptionSelected?.Invoke(option);
    }

    // Interval Button Events
    public delegate void IntervalButtonSelectedHandler(prog_button button);
    public event IntervalButtonSelectedHandler OnIntervalButtonSelected;

    public void SelectedIntervalButton(prog_button button)
    {
        OnIntervalButtonSelected?.Invoke(button);
    }

    // Add more events here as needed
} 