using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class LevelButtonData : MonoBehaviour
{
    public int levelNumber;
    public GameObject lockIcon;
    public Image[] stars;
    public Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void UpdateButtonState()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        bool isUnlocked = levelNumber <= unlockedLevel;
        
        button.interactable = isUnlocked;
        lockIcon.SetActive(!isUnlocked);
        
        GetComponent<Image>().color = isUnlocked ? new Color(0.3f, 0.5f, 0.7f) : new Color(0.2f, 0.2f, 0.2f);
        
        if (isUnlocked && levelNumber <= 5)
        {
            int starCount = PlayerPrefs.GetInt($"Level{levelNumber}_Stars", 0);
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].color = i < starCount ? Color.yellow : new Color(0.3f, 0.3f, 0.3f);
            }
        }
    }
}