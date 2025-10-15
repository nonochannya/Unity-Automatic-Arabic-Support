using UnityEditor;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

[InitializeOnLoad]
public class TMP_AutoAttach
{
    // Keep track of TMP_Text components we've already processed
    private static HashSet<int> processedTextComponents = new HashSet<int>();
    
    // Time to clear the cache (in seconds)
    private static double cacheClearInterval = 60.0;
    private static double lastCacheClearTime;
    
    static TMP_AutoAttach()
    {
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
        
        // Initialize with existing TMP_Text components
        InitializeCache();
    }

    private static void InitializeCache()
    {
        processedTextComponents.Clear();
        lastCacheClearTime = EditorApplication.timeSinceStartup;
        
        TMP_Text[] existingTextComponents = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (TMP_Text textComponent in existingTextComponents)
        {
            if (PrefabUtility.IsPartOfPrefabAsset(textComponent.gameObject))
                processedTextComponents.Add(textComponent.GetInstanceID());
            else 
                processedTextComponents.Add(textComponent.GetInstanceID());
        }
    }

    private static void OnHierarchyChanged()
    {
        // Don't process in play mode
        if (EditorApplication.isPlaying)
            return;
        
        // Check if we should clear the cache
        double currentTime = EditorApplication.timeSinceStartup;
        if (currentTime - lastCacheClearTime > cacheClearInterval)
        {
            InitializeCache();
        }
        
        // Get all TMP_Text components in the scene
        TMP_Text[] allTextComponents = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        bool changesMade = false;
        
        foreach (TMP_Text textComponent in allTextComponents)
        {
            int instanceID = textComponent.GetInstanceID();
            
            // Skip if we've already processed this component
            if (processedTextComponents.Contains(instanceID))
                continue;
                
            // Check if the component already has ArabicTextHandler
            if (textComponent.GetComponent<ArabicTextHandler>() == null)
            {
                // Register undo operation
                Undo.RecordObject(textComponent.gameObject, "Add Arabic Text Handler");
                
                // Add the component and set the reference
                ArabicTextHandler arabicHandler = textComponent.gameObject.AddComponent<ArabicTextHandler>();
                arabicHandler.textComponent = textComponent;
                
                // Mark as dirty to ensure changes are saved
                EditorUtility.SetDirty(textComponent.gameObject);
                
                changesMade = true;
            }
            
            // Mark this component as processed
            processedTextComponents.Add(instanceID);
        }
        
        if (changesMade)
        {
            Debug.Log("ArabicTextHandler components added to TextMeshPro objects.");
        }
    }
}