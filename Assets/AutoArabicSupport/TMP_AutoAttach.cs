using UnityEditor;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class TMP_AutoAttach
{
    private static HashSet<int> processedTextComponents = new HashSet<int>();
    private static double lastFullScanTime;
    private static double fullScanInterval = 30.0;
    private static bool isProcessing = false;

    static TMP_AutoAttach()
    {
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
        EditorApplication.update += OnEditorUpdate;
        ObjectFactory.componentWasAdded += OnComponentAdded;
        EditorSceneManager.sceneOpened += OnSceneOpened;
        EditorSceneManager.sceneSaved += OnSceneSaved;
        PrefabStage.prefabStageOpened += OnPrefabStageOpened;
        PrefabUtility.prefabInstanceUpdated += OnPrefabInstanceUpdated;
        
        EditorApplication.delayCall += () => PerformFullScan(true);
    }

    private static void OnEditorUpdate()
    {
        if (EditorApplication.isPlaying || EditorApplication.isCompiling || isProcessing)
            return;

        double currentTime = EditorApplication.timeSinceStartup;
        if (currentTime - lastFullScanTime > fullScanInterval)
        {
            PerformFullScan(false);
        }
    }

    private static void OnComponentAdded(Component component)
    {
        if (component is TMP_Text tmpText)
        {
            ProcessSingleComponent(tmpText);
        }
    }

    private static void OnHierarchyChanged()
    {
        if (!EditorApplication.isPlaying && !isProcessing)
        {
            ProcessAllComponents(false);
        }
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        EditorApplication.delayCall += () => PerformFullScan(false);
    }

    private static void OnSceneSaved(Scene scene)
    {
        ProcessAllComponents(false);
    }

    private static void OnPrefabStageOpened(PrefabStage prefabStage)
    {
        EditorApplication.delayCall += () => PerformFullScan(false);
    }

    private static void OnPrefabInstanceUpdated(GameObject instance)
    {
        TMP_Text[] textComponents = instance.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text textComponent in textComponents)
        {
            ProcessSingleComponent(textComponent);
        }
    }

    private static void PerformFullScan(bool clearCache)
    {
        if (isProcessing) return;
        
        isProcessing = true;
        
        if (clearCache)
        {
            processedTextComponents.Clear();
        }
        
        lastFullScanTime = EditorApplication.timeSinceStartup;
        ProcessAllComponents(true);
        
        isProcessing = false;
    }

    private static void ProcessAllComponents(bool logChanges)
    {
        if (EditorApplication.isPlaying) return;

        TMP_Text[] allTextComponents = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        bool changesMade = false;

        foreach (TMP_Text textComponent in allTextComponents)
        {
            if (textComponent == null) continue;
            
            int instanceID = textComponent.GetInstanceID();
            
            if (!processedTextComponents.Contains(instanceID))
            {
                if (AttachArabicHandler(textComponent))
                {
                    changesMade = true;
                }
                processedTextComponents.Add(instanceID);
            }
            else if (textComponent.GetComponent<ArabicTextHandler>() == null)
            {
                if (AttachArabicHandler(textComponent))
                {
                    changesMade = true;
                }
            }
        }

        if (changesMade && logChanges)
        {
            Debug.Log($"ArabicTextHandler attached to {allTextComponents.Length} TMP components");
        }
    }

    private static void ProcessSingleComponent(TMP_Text textComponent)
    {
        if (textComponent == null || EditorApplication.isPlaying) return;

        int instanceID = textComponent.GetInstanceID();
        
        if (!processedTextComponents.Contains(instanceID) || textComponent.GetComponent<ArabicTextHandler>() == null)
        {
            AttachArabicHandler(textComponent);
            processedTextComponents.Add(instanceID);
        }
    }

    private static bool AttachArabicHandler(TMP_Text textComponent)
    {
        if (textComponent.GetComponent<ArabicTextHandler>() != null)
            return false;

        Undo.RecordObject(textComponent.gameObject, "Add Arabic Text Handler");
        
        ArabicTextHandler arabicHandler = textComponent.gameObject.AddComponent<ArabicTextHandler>();
        arabicHandler.textComponent = textComponent;
        
        EditorUtility.SetDirty(textComponent.gameObject);
        
        if (!EditorApplication.isPlaying)
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(textComponent.gameObject);
        }
        
        return true;
    }
}
