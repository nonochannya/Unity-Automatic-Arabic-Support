using TMPro;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public enum ProcessingMode
{
    OnceAtAwake,
    RealTimeDetection,
    OnDemandOnly  // New mode for typing effects
}

public class ArabicTextHandler : MonoBehaviour
{
    [Header("Settings")]
    public TMP_Text textComponent;
    public ProcessingMode processingMode = ProcessingMode.RealTimeDetection;
    [Header("Timing")]
    public float startupDelay = 0.1f; // Delay before starting to monitor changes
    private float startupTimer = 0f;
    private bool startupComplete = false;
    
    [Header("Arabic Processing Options")]
    public bool showTashkeel = true;
    public bool useHinduNumbers = false;

    [Header("Error Handling")]
    public bool skipErrorCausingCharacters = true;
    public bool logErrors = true;

    [Header("Typing Effect Support")]
    [Tooltip("These are meant to be changed by methods, not from the inspector!")]
    public bool pauseProcessing = false;  // Flag to pause processing during typing effects
    public bool autoProcessAfterTyping = true;  // Auto-process after typing completes

    [Header("Debug Info")]
    [SerializeField] private bool containsArabic = false;
    [SerializeField] private string lastProcessedText = "";
    public string lastError = "";

    [NonSerialized] public string previousRawText = "";
    [NonSerialized] public string lastFixedText = "";
    [NonSerialized] public bool isProcessing = false;

    // Track if we're in a typing effect
    private bool inTypingEffect = false;
    private string pendingText = "";

    [Header("Additional Settings")]
    [Tooltip("Do not touch unless you need to (=^･ω･^=)")]
    [TextArea(2, 5)]
    public string types = "ArabicBlock\nArabicSupplement\nArabicExtended_A\nArabicPresentationForms_A\nArabicPresentationForms_B";
    public bool UseTheleftUnicodes = false;
    
    void Awake()
    {
        if (textComponent == null)
            textComponent = GetComponent<TMP_Text>();
        
        previousRawText = textComponent.text;
        
        if (processingMode == ProcessingMode.OnceAtAwake)
        {
            ProcessArabicText();
        }
    }

    void Start()
    {
        startupTimer = 0f;
        startupComplete = false;
        
        if (processingMode == ProcessingMode.RealTimeDetection)
        {
            // Don't process immediately, wait for startup delay
            previousRawText = textComponent.text;
        }
    }

    void Update()
    {
        // Handle startup delay
        if (!startupComplete)
        {
            startupTimer += Time.deltaTime;
            if (startupTimer >= startupDelay)
            {
                startupComplete = true;
                // Process once after startup delay
                if (processingMode == ProcessingMode.RealTimeDetection)
                {
                    ProcessArabicText();
                }
            }
            return;
        }
        
        if (processingMode == ProcessingMode.RealTimeDetection && !isProcessing && !pauseProcessing)
        {
            CheckForTextChanges();
        }
    }
    void CheckForTextChanges()
    {
        string currentText = textComponent.text;
        
        // Only process if the current text is different from our stored raw text
        // AND it's not the same as our last fixed output (to prevent loops)
        if (currentText != previousRawText && currentText != lastFixedText)
        {
            // If we're in a typing effect, store the text but don't process yet
            if (inTypingEffect)
            {
                pendingText = currentText;
                return;
            }
            
            previousRawText = currentText;
            ProcessArabicText();
        }
    }
    
    void ProcessArabicText()
    {
        if (textComponent == null || isProcessing) return;
        
        isProcessing = true;
        lastError = ""; // Clear previous errors
        
        string originalText = previousRawText;
        containsArabic = ContainsArabicCharacters(originalText);
        
        if (containsArabic)
        {
            try
            {
                string processedText = SafeArabicFix(originalText);
                
                // Store the fixed text and apply it
                lastFixedText = processedText;
                textComponent.text = processedText;
                lastProcessedText = processedText;
            }
            catch (Exception e)
            {
                lastError = e.Message;
                if (logErrors)
                {
                    Debug.LogWarning($"ArabicTextHandler: Failed to process text '{originalText}'. Error: {e.Message}");
                }
                
                // Fallback: use original text
                lastFixedText = originalText;
                textComponent.text = originalText;
            }
        }
        else
        {
            // If no Arabic, just use the raw text
            lastFixedText = originalText;
            textComponent.text = originalText;
        }
        
        isProcessing = false;
    }
    
    // New methods for typing effect integration
    public void StartTypingEffect()
    {
        inTypingEffect = true;
        pauseProcessing = true;
        pendingText = "";
    }
    
    public void EndTypingEffect()
    {
        inTypingEffect = false;
        
        if (autoProcessAfterTyping && !string.IsNullOrEmpty(pendingText))
        {
            previousRawText = pendingText;
            pauseProcessing = false;
            ProcessArabicText();
        }
    }
    
    // Method to set text directly without processing (for typing effects)
    public void SetTextWithoutProcessing(string text)
    {
        if (textComponent == null) return;
        textComponent.text = text;
    }
    
    // ... (rest of the methods remain the same) ...
    
    string SafeArabicFix(string text)
    {
        try
        {
            // Try the normal fix first
            return ArabicSupport.ArabicFixer.Fix(text, showTashkeel: showTashkeel, useHinduNumbers: useHinduNumbers);
        }
        catch (IndexOutOfRangeException)
        {
            if (skipErrorCausingCharacters)
            {
                // Try to process the text by removing problematic characters one by one
                return ProcessTextSafely(text);
            }
            else
            {
                throw; // Re-throw if we don't want to skip characters
            }
        }
        catch (Exception)
        {
            throw; // Re-throw other exceptions
        }
    }
    
    string ProcessTextSafely(string text)
    {
        // Try processing character by character if the full text fails
        string result = "";
        
        for (int i = 0; i < text.Length; i++)
        {
            string currentChar = text[i].ToString();
            
            try
            {
                // Try to process this single character
                if (ContainsArabicCharacters(currentChar))
                {
                    string processedChar = ArabicSupport.ArabicFixer.Fix(currentChar, showTashkeel: showTashkeel, useHinduNumbers: useHinduNumbers);
                    result += processedChar;
                }
                else
                {
                    result += currentChar;
                }
            }
            catch (Exception)
            {
                // If this character causes problems, just add it as-is
                result += currentChar;
                if (logErrors)
                {
                    Debug.LogWarning($"ArabicTextHandler: Skipped problematic character: {currentChar} (Unicode: {((int)text[i]).ToString("X4")})");
                }
            }
        }
        
        return result;
    }
    
    bool ContainsArabicCharacters(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        
        foreach (char c in text)
        {
            // Using the exact Unicode ranges you specified:
            // 0600-06FF: Arabic block
            // 0750-077F: Arabic Supplement  
            // 08A0-08FF: Arabic Extended-A
            // FB50-FDFF: Arabic Presentation Forms-A
            // FE70-FEFF: Arabic Presentation Forms-B
            if(UseTheleftUnicodes)
                return true; // yes I'm super lasy
            
            if ((c >= 0x0600 && c <= 0x06FF) ||
                (c >= 0x0750 && c <= 0x077F) ||
                (c >= 0x08A0 && c <= 0x08FF) ||
                (c >= 0xFB50 && c <= 0xFDFF) ||
                (c >= 0xFE70 && c <= 0xFEFF))
            {
                return true;
            }
        }
        return false;
    }
    
    // Public method to manually trigger processing
    public void ForceProcessText()
    {
        ProcessArabicText();
    }
    
    // Method to change processing mode at runtime
    public void SetProcessingMode(ProcessingMode mode)
    {
        processingMode = mode;
        if (mode == ProcessingMode.OnceAtAwake)
        {
            ProcessArabicText();
        }
    }
    
    // Method to reset the script state if it gets stuck
    public void ResetState()
    {
        isProcessing = false;
        previousRawText = textComponent.text;
        lastFixedText = "";
        lastError = "";
        inTypingEffect = false;
        pauseProcessing = false;
        pendingText = "";
    }
}

#if UNITY_EDITOR
// Custom editor to make it work in edit mode
[CustomEditor(typeof(ArabicTextHandler))]
public class ArabicTextHandlerEditor : Editor
{
    private string previousEditorText = "";
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        ArabicTextHandler handler = (ArabicTextHandler)target;
        
        GUILayout.Space(10);
        
        // Show error if any
        if (!string.IsNullOrEmpty(handler.lastError))
        {
            EditorGUILayout.HelpBox($"Last Error: {handler.lastError}", MessageType.Warning);
        }
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Force Process Text"))
        {
            handler.ForceProcessText();
            EditorUtility.SetDirty(handler);
        }
        
        if (GUILayout.Button("Reset State"))
        {
            handler.ResetState();
            EditorUtility.SetDirty(handler);
        }
        GUILayout.EndHorizontal();
        
        // Real-time processing in edit mode
        if (handler.processingMode == ProcessingMode.RealTimeDetection && 
            handler.textComponent != null && !handler.isProcessing && !handler.pauseProcessing)
        {
            string currentText = handler.textComponent.text;
            if (currentText != previousEditorText && 
                currentText != handler.lastFixedText)
            {
                previousEditorText = currentText;
                handler.previousRawText = currentText;
                handler.ForceProcessText();
                EditorUtility.SetDirty(handler.textComponent);
            }
        }
    }
}
#endif