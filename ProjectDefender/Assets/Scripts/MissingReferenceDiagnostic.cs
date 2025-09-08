using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

public class MissingReferenceDiagnostic : EditorWindow
{
    private Vector2 scrollPosition;
    private List<DiagnosticResult> results = new List<DiagnosticResult>();
    private bool showOnlyProblems = true;
    private bool autoFix = false;
    
    [System.Serializable]
    public class DiagnosticResult
    {
        public GameObject gameObject;
        public string path;
        public List<string> issues = new List<string>();
        public bool hasProblems;
        public bool expanded = true;
    }
    
    [MenuItem("Tools/Diagnose Missing References")]
    static void ShowWindow()
    {
        var window = GetWindow<MissingReferenceDiagnostic>("Reference Diagnostic");
        window.minSize = new Vector2(500, 400);
        window.Show();
    }
    
    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        
        // Header
        EditorGUILayout.LabelField("Missing Reference Diagnostic Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Controls
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Scan Current Scene", GUILayout.Height(30)))
        {
            ScanScene();
        }
        
        if (GUILayout.Button("Scan Selected Objects", GUILayout.Height(30)))
        {
            ScanSelected();
        }
        
        if (GUILayout.Button("Clear Results", GUILayout.Height(30)))
        {
            results.Clear();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Options
        showOnlyProblems = EditorGUILayout.Toggle("Show Only Problems", showOnlyProblems);
        autoFix = EditorGUILayout.Toggle("Auto-Remove Missing Scripts", autoFix);
        
        EditorGUILayout.Space();
        
        // Statistics
        if (results.Count > 0)
        {
            int problemCount = results.Count(r => r.hasProblems);
            EditorGUILayout.HelpBox($"Found {problemCount} GameObjects with issues out of {results.Count} scanned.", 
                problemCount > 0 ? MessageType.Warning : MessageType.Info);
        }
        
        // Results
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        foreach (var result in results)
        {
            if (showOnlyProblems && !result.hasProblems)
                continue;
                
            DrawDiagnosticResult(result);
        }
        
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
    
    void DrawDiagnosticResult(DiagnosticResult result)
    {
        if (result.gameObject == null) return;
        
        // Create a box for each GameObject
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Header with GameObject name
        EditorGUILayout.BeginHorizontal();
        
        result.expanded = EditorGUILayout.Foldout(result.expanded, "", true);
        
        // Color code based on problem status
        GUI.color = result.hasProblems ? Color.yellow : Color.white;
        
        if (GUILayout.Button(result.gameObject.name, EditorStyles.label))
        {
            Selection.activeGameObject = result.gameObject;
            EditorGUIUtility.PingObject(result.gameObject);
        }
        
        GUI.color = Color.white;
        
        // Show path
        EditorGUILayout.LabelField(result.path, EditorStyles.miniLabel, GUILayout.MaxWidth(200));
        
        // Quick fix button
        if (result.hasProblems && GUILayout.Button("Fix", GUILayout.Width(40)))
        {
            FixGameObject(result.gameObject);
            ScanScene(); // Rescan after fix
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Show issues if expanded
        if (result.expanded && result.issues.Count > 0)
        {
            EditorGUI.indentLevel++;
            foreach (var issue in result.issues)
            {
                // Color code different types of issues
                if (issue.Contains("MISSING SCRIPT"))
                    GUI.color = Color.red;
                else if (issue.Contains("NULL"))
                    GUI.color = new Color(1f, 0.5f, 0f); // Orange
                else if (issue.Contains("WARNING"))
                    GUI.color = Color.yellow;
                else
                    GUI.color = Color.white;
                    
                EditorGUILayout.LabelField("• " + issue, EditorStyles.wordWrappedLabel);
                GUI.color = Color.white;
            }
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }
    
    void ScanScene()
    {
        results.Clear();
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (var go in allObjects)
        {
            ScanGameObject(go);
        }
        
        Debug.Log($"Diagnostic complete: Scanned {allObjects.Length} GameObjects");
    }
    
    void ScanSelected()
    {
        results.Clear();
        
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("No objects selected!");
            return;
        }
        
        foreach (var go in Selection.gameObjects)
        {
            ScanGameObject(go);
            // Also scan children
            foreach (Transform child in go.GetComponentsInChildren<Transform>(true))
            {
                if (child.gameObject != go)
                    ScanGameObject(child.gameObject);
            }
        }
        
        Debug.Log($"Diagnostic complete: Scanned {Selection.gameObjects.Length} selected objects and their children");
    }
    
    void ScanGameObject(GameObject go)
    {
        var result = new DiagnosticResult
        {
            gameObject = go,
            path = GetGameObjectPath(go)
        };
        
        // Check for missing scripts
        Component[] components = go.GetComponents<Component>();
        int missingCount = 0;
        
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] == null)
            {
                missingCount++;
                result.issues.Add($"MISSING SCRIPT at component index {i}");
                result.hasProblems = true;
                
                if (autoFix)
                {
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                }
            }
        }
        
        // Check for null references in MonoBehaviours
        MonoBehaviour[] monoBehaviours = go.GetComponents<MonoBehaviour>();
        
        foreach (var mb in monoBehaviours)
        {
            if (mb == null) continue;
            
            SerializedObject so = new SerializedObject(mb);
            SerializedProperty sp = so.GetIterator();
            
            while (sp.NextVisible(true))
            {
                if (sp.propertyType == SerializedPropertyType.ObjectReference)
                {
                    if (sp.objectReferenceValue == null && sp.objectReferenceInstanceIDValue != 0)
                    {
                        result.issues.Add($"NULL REFERENCE: {mb.GetType().Name}.{sp.displayName} (was pointing to deleted object)");
                        result.hasProblems = true;
                    }
                    else if (sp.objectReferenceValue == null && IsFieldRequired(mb, sp.name))
                    {
                        result.issues.Add($"WARNING: {mb.GetType().Name}.{sp.displayName} is null (might be required)");
                        result.hasProblems = true;
                    }
                }
            }
        }
        
        // Check for specific Unity components with common issues
        CheckSpecificComponents(go, result);
        
        results.Add(result);
    }
    
    void CheckSpecificComponents(GameObject go, DiagnosticResult result)
    {
        // Check Renderer materials
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (renderer.sharedMaterials.Contains(null))
            {
                result.issues.Add("NULL MATERIAL: Renderer has missing materials");
                result.hasProblems = true;
            }
        }
        
        // Check Audio Source
        AudioSource audioSource = go.GetComponent<AudioSource>();
        if (audioSource != null && audioSource.clip == null)
        {
            result.issues.Add("INFO: AudioSource has no AudioClip assigned");
        }
        
        // Check for components that require other components
        var components = go.GetComponents<Component>();
        foreach (var comp in components)
        {
            if (comp == null) continue;
            
            var requirements = comp.GetType().GetCustomAttributes(typeof(RequireComponent), true);
            foreach (RequireComponent req in requirements)
            {
                if (go.GetComponent(req.m_Type0) == null)
                {
                    result.issues.Add($"MISSING REQUIREMENT: {comp.GetType().Name} requires {req.m_Type0.Name}");
                    result.hasProblems = true;
                }
            }
        }
    }
    
    bool IsFieldRequired(MonoBehaviour mb, string fieldName)
    {
        // Check if field has [RequireField] or similar attributes
        var type = mb.GetType();
        var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (field != null)
        {
            // Check for common Unity attributes that indicate required fields
            var attributes = field.GetCustomAttributes(true);
            foreach (var attr in attributes)
            {
                string attrName = attr.GetType().Name;
                if (attrName.Contains("Required") || attrName.Contains("NotNull"))
                    return true;
            }
            
            // Check if field name suggests it's required
            if (fieldName.ToLower().Contains("target") || 
                fieldName.ToLower().Contains("required") ||
                fieldName.ToLower().Contains("main"))
                return true;
        }
        
        return false;
    }
    
    void FixGameObject(GameObject go)
    {
        int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        
        // Clear null references
        MonoBehaviour[] monoBehaviours = go.GetComponents<MonoBehaviour>();
        foreach (var mb in monoBehaviours)
        {
            if (mb == null) continue;
            
            SerializedObject so = new SerializedObject(mb);
            SerializedProperty sp = so.GetIterator();
            bool changed = false;
            
            while (sp.NextVisible(true))
            {
                if (sp.propertyType == SerializedPropertyType.ObjectReference)
                {
                    if (sp.objectReferenceValue == null && sp.objectReferenceInstanceIDValue != 0)
                    {
                        sp.objectReferenceInstanceIDValue = 0;
                        changed = true;
                    }
                }
            }
            
            if (changed)
                so.ApplyModifiedProperties();
        }
        
        Debug.Log($"Fixed {go.name}: Removed {removed} missing scripts and cleared null references");
    }
    
    string GetGameObjectPath(GameObject go)
    {
        string path = go.name;
        Transform parent = go.transform.parent;
        
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        
        return path;
    }
}