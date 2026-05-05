using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

/// <summary>
/// Main game logic for the Chords challenge scene (challengeChords).
///
/// Flow:
///  - 72 shuffled questions = 6 chord types × 12 root note values
///  - Levels 1-2: root revealed; user fills remaining chord tones
///  - Levels 3-4: 2 random chord tones pre-revealed; user fills the rest
///  - Wrong tap → -wrongAnswerDamage% health; timer expiry → -timerExpiryDamage% health
///  - Chord complete → +score; game over when health ≤ 0 or all 72 answered
/// </summary>
public class chordsGameManager : MonoBehaviour
{
    public static chordsGameManager instance;

    // ── Inspector references ─────────────────────────────────────────────────
    [SerializeField] private ChordsGameData chordsGameData;
    [SerializeField] private GameSettings   gameSettings;

    [SerializeField] private TextMeshProUGUI questionText;      // shows chord name
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI timerText;

    [SerializeField] private GameObject gameRunningPanel;
    [SerializeField] private GameObject gameoverPanel;
    [SerializeField] private GameObject wholeFretboard;         // hidden during scorecard

    [SerializeField] private AudioSource correctAnswerAudio;
    [SerializeField] private AudioSource wrongAnswerAudio;

    // ── Tunable damage values (tweak in Inspector) ───────────────────────────
    [Header("Damage Values")]
    [SerializeField] private float wrongAnswerDamage  = 8f;
    [SerializeField] private float timerExpiryDamage  = 20f;

    // ── Runtime state ────────────────────────────────────────────────────────
    public GameStatus gameStatus = GameStatus.Playing;

    private prog_button[] progbuttons_;

    // 72-question pool
    private List<(int chordIndex, int rootNote)> questionPool = new List<(int, int)>();
    private int currentPoolIndex = 0;

    // Current question
    private int   currentChordIndex   = 0;
    private int   currentRootNote     = 0;
    private prog_button revealedRootButton = null;

    // Buttons the user still needs to tap (sibling indices for de-dup)
    private HashSet<int> remainingAnswerSiblings = new HashSet<int>();

    // Tracks (chordIndex, rootNote) pairs where at least one wrong event occurred
    private HashSet<(int, int)> failedQuestionKeys = new HashSet<(int, int)>();

    private Coroutine healthAnimCoroutine;
    private Coroutine scoreAnimCoroutine;

    private static readonly Dictionary<int, string> IntervalNames = new Dictionary<int, string>()
    {
        {0,"R"},{1,"b2"},{2,"M2"},{3,"b3"},{4,"M3"},{5,"P4"},
        {6,"b5"},{7,"P5"},{8,"b6"},{9,"M6"},{10,"b7"},{11,"M7"}
    };

    private static readonly Dictionary<int, string> NoteNamesFlats = new Dictionary<int, string>()
    {
        {0,"A"},{1,"Bb"},{2,"B"},{3,"C"},{4,"Db"},{5,"D"},
        {6,"Eb"},{7,"E"},{8,"F"},{9,"Gb"},{10,"G"},{11,"Ab"}
    };

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    void Awake()
    {
        instance = this;

        if (chordsGameData == null)
            chordsGameData = Resources.Load<ChordsGameData>("ChordsGameData");
        if (gameSettings == null)
            gameSettings = Resources.Load<GameSettings>("GameSettings");
    }

    void Start()
    {
        var pb = GameObject.Find("Prog_buttons");
        if (pb != null)
            progbuttons_ = pb.GetComponentsInChildren<prog_button>();
        else
            Debug.LogError("chordsGameManager: Prog_buttons not found");

        // Auto-find wholefretboard if not wired in Inspector
        if (wholeFretboard == null)
            wholeFretboard = GameObject.Find("wholefretboard");

        chordsGameData.ResetGameData();
        failedQuestionKeys.Clear();

        UpdateHealthDisplay();
        UpdateScoreDisplay();

        BuildAndShuffleQuestionPool();
        NextQuestion();
    }

    void Update()
    {
        if (gameStatus == GameStatus.Playing)
        {
            chordsGameData.timer -= Time.deltaTime;
            timerText.text = Mathf.CeilToInt(chordsGameData.timer).ToString();

            if (chordsGameData.timer <= 0f)
                HandleTimerExpired();
        }
        else if (gameStatus == GameStatus.Next)
        {
            chordsGameData.timer = 20f;
        }

        if (chordsGameData.health <= 0f && gameStatus != GameStatus.Gameover)
            TriggerGameOver();
    }

    // ── Question pool ─────────────────────────────────────────────────────────

    private void BuildAndShuffleQuestionPool()
    {
        questionPool.Clear();

        if (chordsGameData.activeChordTypes == null || chordsGameData.activeChordTypes.Count == 0)
        {
            Debug.LogError("chordsGameManager: activeChordTypes is empty — select a level first");
            return;
        }

        foreach (int chordIdx in chordsGameData.activeChordTypes)
            for (int rootNote = 0; rootNote < 12; rootNote++)
                questionPool.Add((chordIdx, rootNote));

        // Fisher-Yates shuffle
        for (int i = questionPool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = questionPool[i];
            questionPool[i] = questionPool[j];
            questionPool[j] = tmp;
        }

        currentPoolIndex = 0;
        chordsGameData.totalQuestions = questionPool.Count;
        Debug.Log($"Question pool built: {questionPool.Count} questions");
    }

    // ── Core question flow ────────────────────────────────────────────────────

    public void NextQuestion()
    {
        if (currentPoolIndex >= questionPool.Count)
        {
            TriggerGameOver();
            return;
        }

        var q = questionPool[currentPoolIndex];
        currentChordIndex = q.chordIndex;
        currentRootNote   = q.rootNote;

        gameStatus            = GameStatus.Playing;
        chordsGameData.timer  = 20f;

        SetupQuestion();
    }

    private void SetupQuestion()
    {
        if (progbuttons_ == null) return;

        int[] formula = ChordsGameData.AllChords[currentChordIndex];

        // Reset all buttons
        foreach (var btn in progbuttons_)
        {
            btn.SetActiveCircle(0);
            btn.text.text = "";
        }

        remainingAnswerSiblings.Clear();

        bool isHardLevel = chordsGameData.currentChordLevel == ChordLevel.level3 ||
                           chordsGameData.currentChordLevel == ChordLevel.level4;

        bool ok = isHardLevel ? SetupHardLevelQuestion(formula) : SetupStandardQuestion(formula);
        if (!ok) return;

        string chordName = ChordsGameData.ChordNames[currentChordIndex];
        string rootName  = NoteNamesFlats[currentRootNote];
        questionText.text = $"{chordName}";

        Debug.Log($"Q{currentPoolIndex+1}/{questionPool.Count}: {rootName} {chordName} | {remainingAnswerSiblings.Count} answers needed");
    }

    /// <summary>Levels 1 & 2: reveal one random root position.</summary>
    private bool SetupStandardQuestion(int[] formula)
    {
        var rootButtons = new List<prog_button>();
        foreach (var btn in progbuttons_)
            if (btn.notevalue == currentRootNote)
                rootButtons.Add(btn);

        if (rootButtons.Count == 0)
        {
            Debug.LogWarning($"No root buttons for note {currentRootNote} — skipping");
            currentPoolIndex++;
            NextQuestion();
            return false;
        }

        revealedRootButton = rootButtons[Random.Range(0, rootButtons.Count)];
        revealedRootButton.SetActiveCircle(3);
        revealedRootButton.text.text = "R";

        foreach (var btn in progbuttons_)
        {
            if (btn == revealedRootButton) continue;
            int interval = ChordsGameData.GetInterval(currentRootNote, btn.notevalue);
            if (System.Array.IndexOf(formula, interval) >= 0)
                remainingAnswerSiblings.Add(btn.transform.GetSiblingIndex());
        }

        if (remainingAnswerSiblings.Count == 0)
        {
            Debug.LogWarning($"No answer buttons for chord — auto-advancing");
            currentPoolIndex++;
            NextQuestion();
            return false;
        }

        return true;
    }

    /// <summary>
    /// Levels 3 & 4: randomly pre-reveal 2 chord tones (may include root or any interval).
    /// User fills the remaining tones.
    /// </summary>
    private bool SetupHardLevelQuestion(int[] formula)
    {
        // Collect all chord-tone buttons on the board
        var allChordButtons = new List<prog_button>();
        foreach (var btn in progbuttons_)
        {
            int interval = ChordsGameData.GetInterval(currentRootNote, btn.notevalue);
            if (System.Array.IndexOf(formula, interval) >= 0)
                allChordButtons.Add(btn);
        }

        // Need at least 3 chord-tone buttons to have something left after pre-revealing 2
        if (allChordButtons.Count < 3)
        {
            Debug.LogWarning($"Too few chord tones on board for hard level — falling back to standard reveal");
            return SetupStandardQuestion(formula);
        }

        // Shuffle
        for (int i = allChordButtons.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = allChordButtons[i];
            allChordButtons[i] = allChordButtons[j];
            allChordButtons[j] = tmp;
        }

        // All chord tones start as answers
        foreach (var btn in allChordButtons)
            remainingAnswerSiblings.Add(btn.transform.GetSiblingIndex());

        // Pre-reveal first 2 (after shuffle, these are random chord tones)
        revealedRootButton = null;
        for (int i = 0; i < 2; i++)
        {
            var btn = allChordButtons[i];
            int interval = ChordsGameData.GetInterval(currentRootNote, btn.notevalue);
            if (interval == 0)
            {
                btn.SetActiveCircle(3);
                btn.text.text = "R";
                revealedRootButton = btn;
            }
            else
            {
                btn.SetActiveCircle(1);
                btn.text.text = IntervalNames.ContainsKey(interval) ? IntervalNames[interval] : "";
            }
            remainingAnswerSiblings.Remove(btn.transform.GetSiblingIndex());
        }

        if (remainingAnswerSiblings.Count == 0)
        {
            Debug.LogWarning($"All chord tones pre-revealed — auto-advancing");
            currentPoolIndex++;
            NextQuestion();
            return false;
        }

        return true;
    }

    // ── Button input ──────────────────────────────────────────────────────────

    /// <summary>Called by prog_button.ButtonSelected_chords() in the game scene.</summary>
    public void SelectedButton(prog_button btn)
    {
        if (gameStatus != GameStatus.Playing) return;

        // Tapping a pre-revealed button just plays audio/pulse
        if (btn.circleIndex == 1 || btn.circleIndex == 3)
        {
            audioManager.PlayNote(btn.notevalue, btn.x_coord, btn.stringnum,
                gameSettings.transposedNotes_audio[btn.stringnum]);
            StartCoroutine(btn.PulseScale());
            return;
        }

        int siblingIndex  = btn.transform.GetSiblingIndex();
        int interval      = ChordsGameData.GetInterval(currentRootNote, btn.notevalue);
        int[] formula     = ChordsGameData.AllChords[currentChordIndex];
        bool isChordTone  = System.Array.IndexOf(formula, interval) >= 0;

        audioManager.PlayNote(btn.notevalue, btn.x_coord, btn.stringnum,
            gameSettings.transposedNotes_audio[btn.stringnum]);

        if (isChordTone)
        {
            // +10 per correct note
            int oldScore = chordsGameData.score;
            chordsGameData.score += 10;
            AnimateScoreChange(oldScore, chordsGameData.score);

            if (interval == 0)
            {
                btn.SetActiveCircle(3);
                btn.text.text = "R";
            }
            else
            {
                btn.SetActiveCircle(1);
                btn.text.text = IntervalNames.ContainsKey(interval) ? IntervalNames[interval] : "";
            }
            remainingAnswerSiblings.Remove(siblingIndex);

            if (remainingAnswerSiblings.Count == 0)
                ChordComplete();
        }
        else
        {
            // Wrong tap: -5 score
            int oldScore = chordsGameData.score;
            chordsGameData.score = Mathf.Max(0, chordsGameData.score - 5);
            AnimateScoreChange(oldScore, chordsGameData.score);

            btn.SetActiveCircle(2);
            btn.text.text = IntervalNames.ContainsKey(interval) ? IntervalNames[interval] : "";
            wrongAnswerAudio.Play();
            failedQuestionKeys.Add((currentChordIndex, currentRootNote));
            TakeDamage(wrongAnswerDamage);
        }
    }

    // ── Chord completion ──────────────────────────────────────────────────────

    private void ChordComplete()
    {
        chordsGameData.correctAnswers++;
        correctAnswerAudio.Play(); // All notes found — chord complete

        gameStatus = GameStatus.Next;
        currentPoolIndex++;

        Invoke(nameof(NextQuestion), 1.5f);
    }

    // ── Timer ─────────────────────────────────────────────────────────────────

    private void HandleTimerExpired()
    {
        failedQuestionKeys.Add((currentChordIndex, currentRootNote));
        TakeDamage(timerExpiryDamage);

        // Deduct 5 points per missing note
        int missingNotes = remainingAnswerSiblings.Count;
        if (missingNotes > 0)
        {
            int oldScore = chordsGameData.score;
            chordsGameData.score = Mathf.Max(0, chordsGameData.score - 5 * missingNotes);
            AnimateScoreChange(oldScore, chordsGameData.score);
        }

        chordsGameData.timer = 20f;

        if (chordsGameData.health > 0f)
        {
            wrongAnswerAudio.Play();
            RevealAnswers();
            gameStatus = GameStatus.Next;
            currentPoolIndex++;
            Invoke(nameof(NextQuestion), 2.5f);
        }
    }

    /// <summary>Show all remaining correct positions in faded state so user learns.</summary>
    private void RevealAnswers()
    {
        if (progbuttons_ == null) return;
        int[] formula = ChordsGameData.AllChords[currentChordIndex];

        foreach (var btn in progbuttons_)
        {
            if (btn.circleIndex == 1 || btn.circleIndex == 3) continue;
            int interval = ChordsGameData.GetInterval(currentRootNote, btn.notevalue);
            if (System.Array.IndexOf(formula, interval) >= 0)
            {
                btn.SetActiveCircle(1);
                btn.text.text = IntervalNames.ContainsKey(interval) ? IntervalNames[interval] : "";
            }
        }
    }

    // ── Game over ─────────────────────────────────────────────────────────────

    private bool gameoverFired = false;

    private void TriggerGameOver()
    {
        if (gameoverFired) return;
        gameoverFired = true;
        gameStatus    = GameStatus.Gameover;

        StopAllCoroutines();
        CancelInvoke();

        // Disable all buttons
        foreach (var btn in progbuttons_)
        {
            btn.SetActiveCircle(0);
            var b = btn.GetComponent<Button>();
            if (b != null) b.interactable = false;
        }

        // Copy failed questions to chordsGameData for gameover_chords to read
        chordsGameData.wrongChordIndices.Clear();
        chordsGameData.wrongRootNotes.Clear();
        foreach (var (ci, rn) in failedQuestionKeys)
        {
            chordsGameData.wrongChordIndices.Add(ci);
            chordsGameData.wrongRootNotes.Add(rn);
        }

        chordsGameData.UpdateHighScore();

        // Show gameover panel first (gameover_chords.OnEnable caches Prog_buttons,
        // then hides the fretboard itself — order matters)
        if (gameRunningPanel  != null) gameRunningPanel.SetActive(false);
        if (gameoverPanel     != null) gameoverPanel.SetActive(true);
    }

    // ── Health / score display ────────────────────────────────────────────────

    private void TakeDamage(float amount)
    {
        float from = chordsGameData.health;
        chordsGameData.health = Mathf.Max(0f, chordsGameData.health - amount);
        if (healthAnimCoroutine != null) StopCoroutine(healthAnimCoroutine);
        healthAnimCoroutine = StartCoroutine(AnimateHealth(from, chordsGameData.health));
    }

    private void UpdateHealthDisplay()
    {
        if (healthText != null)
            healthText.text = Mathf.RoundToInt(chordsGameData.health) + "%";
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
            scoreText.text = chordsGameData.score.ToString();
    }

    private void AnimateScoreChange(int fromScore, int toScore)
    {
        if (scoreAnimCoroutine != null) StopCoroutine(scoreAnimCoroutine);
        scoreAnimCoroutine = StartCoroutine(AnimateScore(fromScore, toScore));
    }

    private IEnumerator AnimateScore(int fromScore, int toScore)
    {
        float duration = 0.5f;
        float elapsed  = 0f;
        Color normal   = Color.white;
        Color glow     = toScore >= fromScore
                         ? new Color(1f, 0.92f, 0.016f)  // golden yellow (increase)
                         : new Color(1f, 0.2f, 0.2f);     // red (decrease)
        Vector3 normSc = Vector3.one;
        Vector3 bigSc  = new Vector3(1.5f, 1.5f, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float p = Mathf.Sin(t * Mathf.PI);

            scoreText.text                     = Mathf.RoundToInt(Mathf.Lerp(fromScore, toScore, t)).ToString();
            scoreText.transform.localScale     = Vector3.Lerp(normSc, bigSc, p);
            scoreText.color                    = Color.Lerp(normal, glow, p);
            yield return null;
        }

        scoreText.text                 = toScore.ToString();
        scoreText.transform.localScale = normSc;
        scoreText.color                = normal;
        scoreAnimCoroutine             = null;
    }

    private IEnumerator AnimateHealth(float fromHealth, float toHealth)
    {
        float duration = 0.55f;
        float elapsed  = 0f;
        Color normal   = Color.white;
        Color damage   = new Color(1f, 0.2f, 0.2f);
        Vector3 normSc = Vector3.one;
        Vector3 bigSc  = new Vector3(1.4f, 1.4f, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t  = Mathf.Clamp01(elapsed / duration);
            float p  = Mathf.Sin(t * Mathf.PI);

            healthText.text                     = Mathf.RoundToInt(Mathf.Lerp(fromHealth, toHealth, t)) + "%";
            healthText.transform.localScale     = Vector3.Lerp(normSc, bigSc, p);
            healthText.color                    = Color.Lerp(normal, damage, p);
            yield return null;
        }

        healthText.text                 = Mathf.RoundToInt(toHealth) + "%";
        healthText.transform.localScale = normSc;
        healthText.color                = normal;
        healthAnimCoroutine             = null;
    }
}
