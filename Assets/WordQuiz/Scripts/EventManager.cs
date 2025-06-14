using UnityEngine;
using System;
using System.Collections.Generic;

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

    // Note Button Events
    public delegate void NoteButtonSelectedHandler(prog_button button);
    public event NoteButtonSelectedHandler OnNoteButtonSelected;

    public void SelectedNoteButton(prog_button button)
    {
        OnNoteButtonSelected?.Invoke(button);
    }

    // Note Option Events
    public delegate void NoteOptionSelectedHandler(option_note option);
    public event NoteOptionSelectedHandler OnNoteOptionSelected;

    public void SelectNoteOption(option_note option)
    {
        OnNoteOptionSelected?.Invoke(option);
    }

    // Note Level Events
    public delegate void NoteLevelChangedHandler(int level);
    public event NoteLevelChangedHandler OnNoteLevelChanged;

    public void SelectNoteLevel(int level)
    {
        OnNoteLevelChanged?.Invoke(level);
    }

    // Add more events here as needed
} 