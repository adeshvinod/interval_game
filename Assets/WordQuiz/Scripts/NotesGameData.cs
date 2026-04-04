using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NotesGameData", menuName = "Game/Notes Game Data")]
public class NotesGameData : ScriptableObject
{
    public enum NoteLevel
    {
        level1,
        level2,
        level3,
        level4,
        Custom
    }

    // Event to notify when level changes
    public event Action<NoteLevel> OnLevelChanged;
    // Event to notify when questionList changes
    public event Action OnQuestionListChanged;

    [Header("Level Settings")]
    [SerializeField] private NoteLevel _currentLevel = NoteLevel.level1;
    public NoteLevel currentLevel 
    { 
        get => _currentLevel;
        set
        {
            if (_currentLevel != value)
            {
                _currentLevel = value;
                OnLevelChanged?.Invoke(_currentLevel);
            }
        }
    }

    [Header("Score Data")]
    public int score = 0;
    public int highScore = 0;
    public float health = 100f;
    public float timer = 10f;

    [Header("Level High Scores")]
    public int level1HighScore = 0;
    public int level2HighScore = 0;
    public int level3HighScore = 0;
    public int level4HighScore = 0;

    [Header("Question History")]
    public float[] accuracies = new float[78];
    public float[] reactiontimes = new float[78];
    public int[] questioncounter = new int[78];

    [Header("Current Question Data")]
    public int currentQuestion_Answer_node;

    [Header("Game Configuration")]
    public List<(int, int)> questionList = new List<(int, int)>();
    public Dictionary<(int, int), int> Coordinate_system = new Dictionary<(int, int), int>()  //coordinate system of the fretboard
    {   
        {(0,0),0},
        {(0,1),1},
        {(0,2),2},
        {(0,3),3},
        {(0,4),4},
        {(0,5),5},
        {(0,6),6},
        {(0,7),7},
        {(0,8),8},
        {(0,9),9},
        {(0,10),10},
        {(0,11),11},
        {(0,12),12},

        {(1,0),13},
        {(1,1),14},
        {(1,2),15},
        {(1,3),16},
        {(1,4),17},
        {(1,5),18},
        {(1,6),19},
        {(1,7),20},
        {(1,8),21},
        {(1,9),22},
        {(1,10),23},
        {(1,11),24},
        {(1,12),25},

        {(2,0),26},
        {(2,1),27},
        {(2,2),28},
        {(2,3),29},
        {(2,4),30},
        {(2,5),31},
        {(2,6),32},
        {(2,7),33},
        {(2,8),34},
        {(2,9),35},
        {(2,10),36},
        {(2,11),37},
        {(2,12),38},

        {(3,0),39},
        {(3,1),40},
        {(3,2),41},
        {(3,3),42},
        {(3,4),43},
        {(3,5),44},
        {(3,6),45},
        {(3,7),46},
        {(3,8),47},
        {(3,9),48},
        {(3,10),49},
        {(3,11),50},
        {(3,12),51},

        {(4,0),52},
        {(4,1),53},
        {(4,2),54},
        {(4,3),55},
        {(4,4),56},
        {(4,5),57},
        {(4,6),58},
        {(4,7),59},
        {(4,8),60},
        {(4,9),61},
        {(4,10),62},
        {(4,11),63},
        {(4,12),64},

        {(5,0),65},
        {(5,1),66},
        {(5,2),67},
        {(5,3),68},
        {(5,4),69},
        {(5,5),70},
        {(5,6),71},
        {(5,7),72},
        {(5,8),73},
        {(5,9),74},
        {(5,10),75},
        {(5,11),76},
        {(5,12),77}
    };
    
   
    private void OnValidate()
    {
        // Update questionList whenever currentLevel changes in the inspector
        SelectLevel((int)_currentLevel);
        // Trigger the event when level changes in inspector
        OnLevelChanged?.Invoke(_currentLevel);
    }

    public void ResetGameData()
    {
        score = 0;
        health = 100f;
        timer = 10f;

        // Reset arrays
        for (int i = 0; i < 78; i++)
        {
            accuracies[i] = 0;
            reactiontimes[i] = 0;
            questioncounter[i] = 0;
        }

        // Reset current question data
        currentQuestion_Answer_node = 0;
    }

    public void UpdateHighScore()
    {
        if (score > highScore)
        {
            highScore = score;
        }
    }

    public void LoadHighScores()
    {
        savedData data = SaveSystem.Loaddata();
        if (data != null)
        {
            level1HighScore = data.l1_notes_highscore;
            level2HighScore = data.l2_notes_highscore;
            level3HighScore = data.l3_notes_highscore;
            level4HighScore = data.l4_notes_highscore;
        }
    }

    public void SelectLevel(int level)
    {
        questionList.Clear();
        currentLevel = (NoteLevel)level;
        
        switch (currentLevel)
        {
            case NoteLevel.level1:
                // Level 1: strings 4-5, all frets
                for (int i = 4; i <= 5; i++)
                {
                    for(int j = 0; j <= 12; j++)
                    {
                        questionList.Add((i, j));
                    }
                }
                break;

            case NoteLevel.level2:
                // Level 2: all strings, first 5 frets
                for (int i = 0; i <= 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        questionList.Add((i, j));
                    }
                }
                break;

            case NoteLevel.level3:
                // Level 3: all strings, first 8 frets
                for (int i = 0; i <= 5; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        questionList.Add((i, j));
                    }
                }
                break;

            case NoteLevel.level4:
                // Level 4: all strings, all frets
                for (int i = 0; i <= 5; i++)
                {
                    for (int j = 0; j <= 12; j++)
                    {
                        questionList.Add((i, j));
                    }
                }
                break;

            case NoteLevel.Custom:
                // Custom level starts with empty questionList
                questionList.Clear();
                break;
        }
        OnQuestionListChanged?.Invoke();
    }

    public void ToggleNoteInQuestionList((int, int) coordinates)
    {
        if (currentLevel != NoteLevel.Custom) return;

        if (questionList.Contains(coordinates))
        {
            questionList.Remove(coordinates);
        }
        else
        {
            questionList.Add(coordinates);
        }
        OnQuestionListChanged?.Invoke();
    }
} 