using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIController : MonoBehaviour
{
    public static UIController Instance;
    
    [Header("Buildings Settings")]
    [SerializeField] private GameObject backgroundObject;   // родительский прозрачный фон
    [SerializeField] private RectTransform panel;           // дочерняя панель, которая растягивается
    [SerializeField] private RectTransform content;         // контейнер с дочерними элементами
    [SerializeField] private float panelStretchDuration = 0.5f;
    [SerializeField] private float contentStagger = 0.1f;
    [SerializeField] private float contentAnimDuration = 0.25f;
    
    [Header("Main Elements")]
    [SerializeField] private Button openButton;           
    [SerializeField] private RectTransform buttonsBackground; 
    [SerializeField] private float expandDuration = 0.5f; 
    [SerializeField] private float buttonsStagger = 0.15f; 
    [SerializeField] private float buttonAnimDuration = 0.35f; 
    [SerializeField] private float buttonMoveOffset = 30f; 

    private RectTransform[] childButtons;
    private Vector2[] originalPositions;   // фиксируем оригинальные позиции кнопок
    private Vector2 initialBackgroundSize;   
    private Vector2 collapsedBackgroundSize; 
    private bool isExpanded = false;
    private bool isAnimating = false;        
    private Vector2 panelInitialSize;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        childButtons = buttonsBackground.GetComponentsInChildren<RectTransform>();
        childButtons = System.Array.FindAll(childButtons, x =>
            x != buttonsBackground && x.GetComponent<Button>() != null && x.gameObject != openButton.gameObject);

        // сохраняем исходные позиции кнопок
        originalPositions = new Vector2[childButtons.Length];
        for (int i = 0; i < childButtons.Length; i++)
            originalPositions[i] = childButtons[i].anchoredPosition;

        initialBackgroundSize = buttonsBackground.sizeDelta;
        buttonsBackground.pivot = new Vector2(0.5f, 1f);

        RectTransform openBtnRect = openButton.GetComponent<RectTransform>();
        Vector3 btnWorldPos = openBtnRect.position;
        buttonsBackground.position = new Vector3(openBtnRect.position.x, btnWorldPos.y + openBtnRect.rect.height * 0.6f, buttonsBackground.position.z);

        float closedHeight = openBtnRect.sizeDelta.y + 10f;
        collapsedBackgroundSize = new Vector2(initialBackgroundSize.x, closedHeight);

        buttonsBackground.sizeDelta = collapsedBackgroundSize;

        foreach (var btn in childButtons)
            btn.gameObject.SetActive(false);

        openButton.onClick.AddListener(ToggleMenu);
        
        if (panel != null)
        {
            panelInitialSize = panel.sizeDelta;
            panel.sizeDelta = new Vector2(0, panelInitialSize.y);
            panel.gameObject.SetActive(true); // чтобы DOTween мог анимировать
        }

        if (content != null)
        {
            foreach (Transform child in content)
            {
                CanvasGroup cg = child.GetComponent<CanvasGroup>();
                if (cg == null) cg = child.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0;
                child.localScale = Vector3.one * 0.8f;
            }
        }

        if (backgroundObject != null)
            backgroundObject.SetActive(false);
    }

    public void OpenBuildingPanel()
    {
        if (backgroundObject != null)
            backgroundObject.SetActive(true);

        if (panel != null)
        {
            panel.gameObject.SetActive(true); // панель должна быть активной для анимации
            panel.sizeDelta = new Vector2(0, panelInitialSize.y); // сброс ширины перед анимацией

            panel.DOSizeDelta(panelInitialSize, panelStretchDuration).SetEase(Ease.OutQuad)
                .OnComplete(() => AnimateContent(true));
        }
    }

    public void CloseBuildingPanel()
    {
        if (content != null)
        {
            AnimateContent(false, () => {
                // Отключаем панель и фон только после завершения анимации
                if (panel != null)
                    panel.gameObject.SetActive(false);
            
                if (backgroundObject != null)
                    backgroundObject.SetActive(false);
            });
        }
        else
        {
            // Если нет контента, отключаем сразу
            if (panel != null)
                panel.gameObject.SetActive(false);
        
            if (backgroundObject != null)
                backgroundObject.SetActive(false);
        }
    }

    private void AnimateContent(bool show, System.Action onComplete = null)
    {
        if (content == null)
        {
            onComplete?.Invoke();
            return;
        }

        for (int i = 0; i < content.childCount; i++)
        {
            Transform child = content.GetChild(i);
            CanvasGroup cg = child.GetComponent<CanvasGroup>();
            if (cg == null) cg = child.gameObject.AddComponent<CanvasGroup>();

            if (show)
            {
                child.localScale = Vector3.one * 0.8f;
                cg.alpha = 0;
                Sequence seq = DOTween.Sequence();
                seq.Append(cg.DOFade(1, contentAnimDuration))
                   .Join(child.DOScale(1f, contentAnimDuration).SetEase(Ease.OutBack))
                   .SetDelay(i * contentStagger);

                if (i == content.childCount - 1)
                    seq.OnComplete(() => onComplete?.Invoke());
            }
            else
            {
                Sequence seq = DOTween.Sequence();
                seq.Append(cg.DOFade(0, contentAnimDuration))
                   .Join(child.DOScale(0.8f, contentAnimDuration).SetEase(Ease.InBack))
                   .SetDelay(i * contentStagger);

                if (i == content.childCount - 1)
                    seq.OnComplete(() => onComplete?.Invoke());
            }
        }

        // Если нет дочерних элементов
        if (content.childCount == 0)
            onComplete?.Invoke();
    }
    
    private void ToggleMenu()
    {
        if (isAnimating) return; 

        if (isExpanded)
            CollapseMenu();
        else
            ExpandMenu();
    }

    private void ExpandMenu()
    {
        isExpanded = true;
        isAnimating = true;

        buttonsBackground.DOSizeDelta(initialBackgroundSize, expandDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                for (int i = 0; i < childButtons.Length; i++)
                {
                    var btn = childButtons[i];
                    btn.gameObject.SetActive(true);

                    CanvasGroup cg = btn.GetComponent<CanvasGroup>();
                    if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();
                    cg.alpha = 0;

                    // сброс начальных значений: всегда от originalPositions
                    btn.localScale = Vector3.one * 0.8f;
                    btn.anchoredPosition = originalPositions[i] - new Vector2(0, buttonMoveOffset);

                    Sequence seq = DOTween.Sequence();
                    seq.Append(cg.DOFade(1, buttonAnimDuration))
                       .Join(btn.DOScale(1f, buttonAnimDuration).SetEase(Ease.OutBack))
                       .Join(btn.DOAnchorPos(originalPositions[i], buttonAnimDuration).SetEase(Ease.OutQuad))
                       .SetDelay(i * buttonsStagger);

                    if (i == childButtons.Length - 1)
                        seq.OnComplete(() => isAnimating = false);
                }

                if (childButtons.Length == 0)
                    isAnimating = false;
            });
    }

    private void CollapseMenu()
    {
        isExpanded = false;
        isAnimating = true;

        int completed = 0; 

        for (int i = 0; i < childButtons.Length; i++)
        {
            var btn = childButtons[i];
            if (!btn.gameObject.activeSelf) continue;

            CanvasGroup cg = btn.GetComponent<CanvasGroup>();
            if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();

            Sequence seq = DOTween.Sequence();
            seq.Append(cg.DOFade(0, buttonAnimDuration * 0.8f))
               .Join(btn.DOScale(0.8f, buttonAnimDuration * 0.8f).SetEase(Ease.InBack))
               .Join(btn.DOAnchorPos(originalPositions[i] - new Vector2(0, buttonMoveOffset), buttonAnimDuration * 0.8f).SetEase(Ease.InQuad))
               .SetDelay(i * 0.05f)
               .OnComplete(() =>
               {
                   btn.gameObject.SetActive(false);
                   completed++;
                   if (completed == childButtons.Length)
                       isAnimating = false;
               });
        }

        buttonsBackground.DOSizeDelta(collapsedBackgroundSize, expandDuration)
            .SetEase(Ease.InQuad);
    }
}
