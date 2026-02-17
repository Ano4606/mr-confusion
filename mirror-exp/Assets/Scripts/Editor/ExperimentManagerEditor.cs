using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ExperimentManager))]
public class ExperimentManagerEditor : Editor
{
    private string[] avatarOptions = new string[]
    {
        "participant-black-female",
        "participant-white-female",
        "participant-asian-female",
        "participant-black-male",
        "participant-white-male",
        "participant-asian-male"
    };

    public override void OnInspectorGUI()
    {
        ExperimentManager manager = (ExperimentManager)target;

        // Debug Mode Section
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Mode", EditorStyles.boldLabel);
        
        manager.debugMode = EditorGUILayout.Toggle(
            new GUIContent("Debug Mode", "Skip avatar selection and use debug avatar directly"),
            manager.debugMode
        );

        if (manager.debugMode)
        {
            EditorGUI.indentLevel++;
            
            // Avatar dropdown
            int currentIndex = System.Array.IndexOf(avatarOptions, manager.debugAvatarName);
            if (currentIndex == -1) currentIndex = 0;
            
            int newIndex = EditorGUILayout.Popup(
                new GUIContent("Debug Avatar", "Avatar to use in debug mode"),
                currentIndex,
                avatarOptions
            );
            
            manager.debugAvatarName = avatarOptions[newIndex];
            
            manager.skipEmbodiment = EditorGUILayout.Toggle(
                new GUIContent("Skip Embodiment", "Skip embodiment phase in debug mode"),
                manager.skipEmbodiment
            );
            
            EditorGUI.indentLevel--;
            
            // Info box
            EditorGUILayout.HelpBox(
                "Debug Mode Active:\n" +
                "• Avatar selection will be skipped\n" +
                "• Selected avatar: " + manager.debugAvatarName + "\n" +
                "• Embodiment: " + (manager.skipEmbodiment ? "Skipped" : "Enabled"),
                MessageType.Info
            );
        }

        EditorGUILayout.Space();
        
        // Draw default inspector for remaining fields
        DrawDefaultInspector();
        
        // Mark as dirty if changed
        if (GUI.changed)
        {
            EditorUtility.SetDirty(manager);
        }
    }
}
