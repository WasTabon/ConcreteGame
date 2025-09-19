using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

public class LevelButtonPrefabReplacer : EditorWindow
{
    private GameObject levelButtonPrefab;
    
    [MenuItem("Tools/Replace Level Buttons with Prefab")]
    public static void ShowWindow()
    {
        GetWindow<LevelButtonPrefabReplacer>("Level Button Prefab Replacer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Level Button Prefab Replacer", EditorStyles.boldLabel);
        
        levelButtonPrefab = (GameObject)EditorGUILayout.ObjectField("Level Button Prefab", levelButtonPrefab, typeof(GameObject), false);
        
        if (GUILayout.Button("Create Prefab from First Button"))
        {
            CreatePrefabFromExistingButton();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Replace All Buttons with Prefab"))
        {
            if (levelButtonPrefab != null)
            {
                ReplaceAllButtonsWithPrefab();
            }
            else
            {
                Debug.LogError("Please assign a Level Button Prefab first!");
            }
        }
    }

    private void CreatePrefabFromExistingButton()
    {
        GameObject firstButton = GameObject.Find("LevelButton_1");
        if (firstButton == null)
        {
            Debug.LogError("Could not find LevelButton_1 in the scene!");
            return;
        }

        GameObject prefabTemplate = Instantiate(firstButton);
        prefabTemplate.name = "LevelButtonPrefab";
        
        LevelButtonData buttonData = prefabTemplate.GetComponent<LevelButtonData>();
        if (buttonData != null)
        {
            buttonData.levelNumber = 0;
        }

        string prefabPath = "Assets/LevelButtonPrefab.prefab";
        GameObject createdPrefab = PrefabUtility.SaveAsPrefabAsset(prefabTemplate, prefabPath);
        DestroyImmediate(prefabTemplate);
        
        levelButtonPrefab = createdPrefab;
        
        Debug.Log($"Prefab created at {prefabPath}");
        EditorGUIUtility.PingObject(createdPrefab);
    }

    private void ReplaceAllButtonsWithPrefab()
    {
        List<ButtonReplacementData> replacementData = new List<ButtonReplacementData>();
        
        for (int i = 1; i <= 30; i++)
        {
            GameObject existingButton = GameObject.Find($"LevelButton_{i}");
            if (existingButton != null)
            {
                ButtonReplacementData data = new ButtonReplacementData();
                data.levelNumber = i;
                data.parent = existingButton.transform.parent;
                data.siblingIndex = existingButton.transform.GetSiblingIndex();
                
                LevelButtonData buttonData = existingButton.GetComponent<LevelButtonData>();
                if (buttonData != null)
                {
                    data.lockOverlayActive = buttonData.lockOverlay != null && buttonData.lockOverlay.activeSelf;
                    
                    if (buttonData.starWhites != null)
                    {
                        data.starScales = new Vector3[buttonData.starWhites.Length];
                        for (int j = 0; j < buttonData.starWhites.Length; j++)
                        {
                            if (buttonData.starWhites[j] != null)
                            {
                                data.starScales[j] = buttonData.starWhites[j].transform.localScale;
                            }
                        }
                    }
                }
                
                replacementData.Add(data);
                DestroyImmediate(existingButton);
            }
        }

        foreach (var data in replacementData)
        {
            GameObject newButton = (GameObject)PrefabUtility.InstantiatePrefab(levelButtonPrefab);
            newButton.name = $"LevelButton_{data.levelNumber}";
            newButton.transform.SetParent(data.parent, false);
            newButton.transform.SetSiblingIndex(data.siblingIndex);
            
            LevelButtonData newButtonData = newButton.GetComponent<LevelButtonData>();
            if (newButtonData != null)
            {
                newButtonData.levelNumber = data.levelNumber;
                
                TextMeshProUGUI levelText = newButton.GetComponentInChildren<TextMeshProUGUI>();
                if (levelText != null && levelText.name == "LevelNumber")
                {
                    levelText.text = data.levelNumber.ToString();
                }
                
                if (newButtonData.lockOverlay != null)
                {
                    newButtonData.lockOverlay.SetActive(data.lockOverlayActive);
                }
                
                if (newButtonData.starWhites != null && data.starScales != null)
                {
                    for (int j = 0; j < Mathf.Min(newButtonData.starWhites.Length, data.starScales.Length); j++)
                    {
                        if (newButtonData.starWhites[j] != null)
                        {
                            newButtonData.starWhites[j].transform.localScale = data.starScales[j];
                        }
                    }
                }
            }
        }

        GameObject controller = GameObject.FindObjectOfType<LevelSelectionController>()?.gameObject;
        if (controller != null)
        {
            LevelSelectionController controllerScript = controller.GetComponent<LevelSelectionController>();
            if (controllerScript != null)
            {
                Button[] allButtons = FindObjectsOfType<Button>();
                List<Button> levelButtons = new List<Button>();
                
                foreach (Button btn in allButtons)
                {
                    if (btn.name.StartsWith("LevelButton_"))
                    {
                        levelButtons.Add(btn);
                    }
                }
                
                controllerScript.levelButtons = levelButtons.ToArray();
                EditorUtility.SetDirty(controllerScript);
            }
        }
        
        Debug.Log($"Successfully replaced {replacementData.Count} level buttons with prefabs!");
    }
    
    private class ButtonReplacementData
    {
        public int levelNumber;
        public Transform parent;
        public int siblingIndex;
        public bool lockOverlayActive;
        public Vector3[] starScales;
    }
}