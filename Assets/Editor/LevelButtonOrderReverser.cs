using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class LevelButtonOrderReverser : EditorWindow
{
    [MenuItem("Tools/Reverse Level Buttons Order")]
    public static void ShowWindow()
    {
        GetWindow<LevelButtonOrderReverser>("Level Button Order Reverser");
    }

    private void OnGUI()
    {
        GUILayout.Label("Level Button Order Reverser", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox("This will reverse the order of level buttons in the hierarchy so they appear from Level 1 to Level 30.", MessageType.Info);
        
        if (GUILayout.Button("Reverse Level Buttons Order"))
        {
            ReverseLevelButtonsOrder();
        }
    }

    private void ReverseLevelButtonsOrder()
    {
        List<GameObject> levelButtons = new List<GameObject>();
        
        for (int i = 1; i <= 30; i++)
        {
            GameObject button = GameObject.Find($"LevelButton_{i}");
            if (button != null)
            {
                levelButtons.Add(button);
            }
        }

        if (levelButtons.Count == 0)
        {
            Debug.LogWarning("No level buttons found in the scene!");
            return;
        }

        Transform parentTransform = levelButtons[0].transform.parent;
        
        levelButtons.Reverse();
        
        for (int i = 0; i < levelButtons.Count; i++)
        {
            levelButtons[i].transform.SetSiblingIndex(i);
        }

        Debug.Log($"Successfully reversed the order of {levelButtons.Count} level buttons!");
    }
}