using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class LevelButtonData : MonoBehaviour
{
    public int levelNumber;
    public GameObject lockOverlay;
    public Image[] starBlacks;
    public Image[] starWhites;
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
        lockOverlay.SetActive(!isUnlocked);
        
        GetComponent<Image>().color = isUnlocked ? new Color(0.3f, 0.5f, 0.7f) : new Color(0.2f, 0.2f, 0.2f);
        
        if (isUnlocked && levelNumber <= 5)
        {
            int starCount = PlayerPrefs.GetInt($"Level{levelNumber}_Stars", 0);
            for (int i = 0; i < starWhites.Length; i++)
            {
                if (i < starCount)
                {
                    starWhites[i].transform.localScale = Vector3.one;
                    starWhites[i].color = Color.yellow;
                }
                else
                {
                    starWhites[i].transform.localScale = Vector3.zero;
                }
            }
        }
        else
        {
            foreach (var starWhite in starWhites)
            {
                starWhite.transform.localScale = Vector3.zero;
            }
        }
    }
}