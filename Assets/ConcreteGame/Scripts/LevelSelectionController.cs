using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectionController : MonoBehaviour
{
    public GameObject loadingPanel;
    public Button[] levelButtons;

    private void Start()
    {
        InitializeLevels();
    }

    private void InitializeLevels()
    {
        foreach (var button in levelButtons)
        {
            LevelButtonData buttonData = button.GetComponent<LevelButtonData>();
            if (buttonData != null)
            {
                buttonData.UpdateButtonState();
                
                int levelNum = buttonData.levelNumber;
                button.onClick.AddListener(() => OnLevelButtonClick(levelNum));
            }
        }
    }

    private void OnLevelButtonClick(int levelNumber)
    {
        StartCoroutine(LoadLevelWithDelay(levelNumber));
    }

    private IEnumerator LoadLevelWithDelay(int levelNumber)
    {
        loadingPanel.SetActive(true);
        
        yield return new WaitForSeconds(1f);
        
        string sceneName;
        if (levelNumber <= 5)
        {
            sceneName = $"Level {levelNumber}";
        }
        else
        {
            int randomLevel = Random.Range(1, 6);
            sceneName = $"Level {randomLevel}";
        }
        
        SceneManager.LoadScene(sceneName);
    }

    public static void UnlockNextLevel(int completedLevel)
    {
        int currentUnlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);
        if (completedLevel >= currentUnlocked)
        {
            PlayerPrefs.SetInt("UnlockedLevel", completedLevel + 1);
            PlayerPrefs.Save();
        }
    }

    public static void SaveLevelStars(int levelNumber, int starCount)
    {
        string key = $"Level{levelNumber}_Stars";
        int currentStars = PlayerPrefs.GetInt(key, 0);
        if (starCount > currentStars)
        {
            PlayerPrefs.SetInt(key, starCount);
            PlayerPrefs.Save();
        }
    }
}