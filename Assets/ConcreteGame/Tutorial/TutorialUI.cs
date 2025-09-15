using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject tutorialPanel;   // панель туториала
    [SerializeField] private TextMeshProUGUI tutorialText; // текст
    [SerializeField] private Button nextButton;           // кнопка Next/Finish
    [SerializeField] private TextMeshProUGUI nextButtonText; // текст на кнопке

    [Header("Tutorial Content")]
    [TextArea]
    [SerializeField] private List<string> tutorialSteps = new List<string>();

    private int currentStep = 0;
    private const string TutorialShownKey = "TutorialShown";

    private void Start()
    {
        // Проверяем — если туториал уже показывали, скрываем
        if (PlayerPrefs.GetInt(TutorialShownKey, 0) == 1)
        {
            tutorialPanel.SetActive(false);
            return;
        }

        // Иначе показываем первый шаг
        tutorialPanel.SetActive(true);
        currentStep = 0;
        tutorialText.text = tutorialSteps[currentStep];
        nextButtonText.text = "Next";

        nextButton.onClick.AddListener(OnNextClicked);
    }

    private void OnNextClicked()
    {
        MusicController.Instance.PlayClickSound();

        currentStep++;

        if (currentStep < tutorialSteps.Count)
        {
            tutorialText.text = tutorialSteps[currentStep];
            
            // Если это последний шаг — меняем текст кнопки
            if (currentStep == tutorialSteps.Count - 1)
            {
                nextButtonText.text = "Finish";
            }
        }
        else
        {
            // Туториал окончен
            tutorialPanel.SetActive(false);
            PlayerPrefs.SetInt(TutorialShownKey, 1);
            PlayerPrefs.Save();
        }
    }
}