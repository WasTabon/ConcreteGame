using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class WinController : MonoBehaviour
{
    public static WinController Instance;

    [Header("UI Elements")]
    [SerializeField] private RectTransform _winPanel;
    [SerializeField] private RectTransform _background;
    [SerializeField] private RectTransform _winText;
    [SerializeField] private Button _continueButton;
    
    [Header("Star Backgrounds (always visible)")]
    [SerializeField] private RectTransform _starBlack1;
    [SerializeField] private RectTransform _starBlack2;
    [SerializeField] private RectTransform _starBlack3;
    
    [Header("Stars (children of StarBackgrounds)")]
    [SerializeField] private RectTransform _star1;
    [SerializeField] private RectTransform _star2;
    [SerializeField] private RectTransform _star3;

    private Vector2 _originalBackgroundPos;
    private Vector2 _originalTextScale;
    private Vector2 _originalButtonScale;
    private Color _originalButtonColor;
    private Image _continueButtonImage;

    private void Awake()
    {
        Instance = this;
        _continueButtonImage = _continueButton.GetComponent<Image>();
    }

    private void Start()
    {
        SetupScene();
    }

    private void SetupScene()
    {
        _winPanel.gameObject.SetActive(false);
        
        _originalBackgroundPos = _background.anchoredPosition;
        _originalTextScale = _winText.localScale;
        _originalButtonScale = _continueButton.transform.localScale;
        _originalButtonColor = _continueButtonImage.color;
        
        _background.anchoredPosition = new Vector2(_originalBackgroundPos.x - Screen.width, _originalBackgroundPos.y);
        
        _winText.localScale = Vector3.zero;
        _continueButton.transform.localScale = Vector3.zero;
        
        _starBlack1.localScale = Vector3.one;
        _starBlack2.localScale = Vector3.one;
        _starBlack3.localScale = Vector3.one;
        
        _star1.localScale = Vector3.zero;
        _star2.localScale = Vector3.zero;
        _star3.localScale = Vector3.zero;
    }

    public void ShowWinAnimation(int starCount)
    {
        starCount = Mathf.Clamp(starCount, 0, 3);
        
        _winPanel.gameObject.SetActive(true);
        
        Sequence winSequence = DOTween.Sequence();
        
        winSequence.AppendCallback(() => {
            _continueButton.interactable = false;
            _continueButtonImage.color = new Color(_originalButtonColor.r * 0.6f, _originalButtonColor.g * 0.6f, _originalButtonColor.b * 0.6f, _originalButtonColor.a * 0.7f);
        });
        
        winSequence.Append(_background.DOAnchorPos(_originalBackgroundPos, 0.8f).SetEase(Ease.OutBack));
        
        winSequence.Append(_winText.DOScale(_originalTextScale, 0.6f).SetEase(Ease.OutBounce));
        
        winSequence.Append(_continueButton.transform.DOScale(_originalButtonScale, 0.4f).SetEase(Ease.OutQuad));
        
        winSequence.Append(_starBlack1.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutQuad));
        winSequence.Join(_starBlack2.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutQuad).SetDelay(0.1f));
        winSequence.Join(_starBlack3.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutQuad).SetDelay(0.2f));
        
        if (starCount >= 1)
        {
            winSequence.Append(_star1.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
        }
        
        if (starCount >= 2)
        {
            winSequence.Append(_star2.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
        }
        
        if (starCount >= 3)
        {
            winSequence.Append(_star3.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
        }
        
        winSequence.AppendCallback(() => {
            _continueButton.interactable = true;
            _continueButtonImage.DOColor(_originalButtonColor, 0.3f);
            _continueButton.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 10, 1);
        });
    }

    public void HideWinPanel()
    {
        _winPanel.gameObject.SetActive(false);
        SetupScene();
    }
}