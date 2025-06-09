using UnityEngine;

public class materialController : MonoBehaviour
{
    [Header("String 1")]
    public Renderer string1Renderer;
    public bool isEmissionOn1 = false;
    public Color emissionColor1 = Color.white;
    public float emissionIntensity1 = 1.0f;

    [Header("String 2")]
    public Renderer string2Renderer;
    public bool isEmissionOn2 = false;
    public Color emissionColor2 = Color.white;
    public float emissionIntensity2 = 1.0f;

    [Header("String 3")]
    public Renderer string3Renderer;
    public bool isEmissionOn3 = false;
    public Color emissionColor3 = Color.white;
    public float emissionIntensity3 = 1.0f;

    [Header("String 4")]
    public Renderer string4Renderer;
    public bool isEmissionOn4 = false;
    public Color emissionColor4 = Color.white;
    public float emissionIntensity4 = 1.0f;

    [Header("String 5")]
    public Renderer string5Renderer;
    public bool isEmissionOn5 = false;
    public Color emissionColor5 = Color.white;
    public float emissionIntensity5 = 1.0f;

    [Header("String 6")]
    public Renderer string6Renderer;
    public bool isEmissionOn6 = false;
    public Color emissionColor6 = Color.white;
    public float emissionIntensity6 = 1.0f;

    // Store material instances and previous states
    private Material[] materialInstances = new Material[6];
    private bool[] previousEmissionStates = new bool[6];
    private float[] previousIntensities = new float[6];
    private float pulseSpeed = 8f;  // Same as the original sine wave speed

    void Start()
    {
        // Initialize all strings
        InitializeString(0, string1Renderer, isEmissionOn1, emissionIntensity1);
        InitializeString(1, string2Renderer, isEmissionOn2, emissionIntensity2);
        InitializeString(2, string3Renderer, isEmissionOn3, emissionIntensity3);
        InitializeString(3, string4Renderer, isEmissionOn4, emissionIntensity4);
        InitializeString(4, string5Renderer, isEmissionOn5, emissionIntensity5);
        InitializeString(5, string6Renderer, isEmissionOn6, emissionIntensity6);
    }

    void InitializeString(int index, Renderer renderer, bool isOn, float intensity)
    {
        if (renderer == null)
        {
            Debug.LogError($"String {index + 1} renderer is not assigned!");
            return;
        }

        materialInstances[index] = new Material(renderer.material);
        renderer.material = materialInstances[index];
        previousEmissionStates[index] = isOn;
        previousIntensities[index] = intensity;
        UpdateStringEmission(index);
    }

    void Update()
    {
        // Check each string for changes
        CheckAndUpdateString(0, string1Renderer, isEmissionOn1, emissionIntensity1, emissionColor1);
        CheckAndUpdateString(1, string2Renderer, isEmissionOn2, emissionIntensity2, emissionColor2);
        CheckAndUpdateString(2, string3Renderer, isEmissionOn3, emissionIntensity3, emissionColor3);
        CheckAndUpdateString(3, string4Renderer, isEmissionOn4, emissionIntensity4, emissionColor4);
        CheckAndUpdateString(4, string5Renderer, isEmissionOn5, emissionIntensity5, emissionColor5);
        CheckAndUpdateString(5, string6Renderer, isEmissionOn6, emissionIntensity6, emissionColor6);
    }

    // New method to pulse a specific string's intensity
    public void PulseStringIntensity(int stringIndex)
    {
        if (stringIndex < 0 || stringIndex >= 6 || materialInstances[stringIndex] == null) return;

        // Calculate pulsing intensity between 1 and 40
        float pulseValue = 1f + (Mathf.Sin(Time.time * pulseSpeed) + 1); // This will oscillate between 1 and 40
        
        // Update the specific string's intensity
        switch (stringIndex)
        {
            case 0:
                emissionIntensity1 = pulseValue;
                break;
            case 1:
                emissionIntensity2 = pulseValue;
                break;
            case 2:
                emissionIntensity3 = pulseValue;
                break;
            case 3:
                emissionIntensity4 = pulseValue;
                break;
            case 4:
                emissionIntensity5 = pulseValue;
                break;
            case 5:
                emissionIntensity6 = pulseValue;
                break;
        }
    }

    // New method to reset all strings to base intensity
    public void ResetAllStringsToBaseIntensity()
    {
        emissionIntensity1 = 1f;
        emissionIntensity2 = 1f;
        emissionIntensity3 = 1f;
        emissionIntensity4 = 1f;
        emissionIntensity5 = 1f;
        emissionIntensity6 = 1f;
    }

    void CheckAndUpdateString(int index, Renderer renderer, bool isOn, float intensity, Color color)
    {
        if (renderer == null || materialInstances[index] == null) return;

        if (isOn != previousEmissionStates[index] || intensity != previousIntensities[index])
        {
           // Debug.Log($"String {index + 1} emission changed - On: {isOn}, Intensity: {intensity}");
            previousEmissionStates[index] = isOn;
            previousIntensities[index] = intensity;
            UpdateStringEmission(index, isOn, intensity, color);
        }
    }

    void UpdateStringEmission(int index, bool? isOn = null, float? intensity = null, Color? color = null)
    {
        if (materialInstances[index] == null) return;

        bool currentIsOn = isOn ?? previousEmissionStates[index];
        float currentIntensity = intensity ?? previousIntensities[index];
        Color currentColor = color ?? Color.white;

        if (currentIsOn)
        {
            materialInstances[index].EnableKeyword("_EMISSION");
            Color finalEmissionColor = currentColor * currentIntensity;
            materialInstances[index].SetColor("_EmissionColor", finalEmissionColor);
            materialInstances[index].globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        }
        else
        {
            materialInstances[index].DisableKeyword("_EMISSION");
            materialInstances[index].SetColor("_EmissionColor", Color.black);
        }
    }

    void OnDestroy()
    {
        // Clean up all material instances
        for (int i = 0; i < materialInstances.Length; i++)
        {
            if (materialInstances[i] != null)
            {
                Destroy(materialInstances[i]);
            }
        }
    }
}