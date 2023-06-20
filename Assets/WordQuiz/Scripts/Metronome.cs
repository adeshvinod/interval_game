using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Metronome : MonoBehaviour
{
    public int bpm = 80;
    public Slider metronome_slider;
    public Text slider_text;
    public int beat_no = 1;

    double nextTick = 0.0F; // The next tick in dspTime
    double sampleRate = 0.0F;
    bool ticked = false;

    void Start()
    {
        initialize();

    }

    public void initialize()
    {
        beat_no = 1; //for count in
        double startTick = AudioSettings.dspTime;
        sampleRate = AudioSettings.outputSampleRate;

        nextTick = startTick + (60.0 / bpm);
    }

    void LateUpdate()
    {
        if (!ticked && nextTick >= AudioSettings.dspTime)
        {
            ticked = true;
            OnTick();
           
        }

        bpm = Mathf.RoundToInt((float)metronome_slider.value);
        slider_text.text = bpm.ToString();
    }

    // Just an example OnTick here
    void OnTick()
    {
        Debug.Log("Tick  beat no:"+beat_no);
        
        beat_no = (beat_no + 1) % 4;
        if(beat_no==0 && arpeggio_manager.instance.gameMode==arpeggio_manager.GameMode.OpenFretboard)
        {
            if(arpeggio_manager.instance.questionMode==arpeggio_manager.QuestionMode.ChordProgressionQuestions)
           arpeggio_manager.instance.progression_index = (arpeggio_manager.instance.progression_index + 1) % settings_arpgame.instance.myProgression.Count;
            arpeggio_manager.instance.next_question();
            Debug.Log("next question called from metronome.cs");
            
        }
       
        // GetComponent<AudioSource>().Play();
    }

    void FixedUpdate()
    {
        double timePerTick = 60.0f / bpm;
        double dspTime = AudioSettings.dspTime;

        while (dspTime >= nextTick)
        {
            ticked = false;
            nextTick += timePerTick;
        }

    }
}