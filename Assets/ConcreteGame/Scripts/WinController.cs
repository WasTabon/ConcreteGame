using UnityEngine;

public class WinController : MonoBehaviour
{
    public static WinController Instance;

    [Header("UI Elements")]
    [SerializeField] private RectTransform _winPanel;
    [SerializeField] private RectTransform _winText;
    [SerializeField] private RectTransform _continueButton;
    
    [Header("Star Backgrounds")]
    [SerializeField] private RectTransform _starBlack1;
    [SerializeField] private RectTransform _starBlack2;
    [SerializeField] private RectTransform _starBlack3;
    
    [Header("Stars")]
    [SerializeField] private RectTransform _star1;
    [SerializeField] private RectTransform _star2;
    [SerializeField] private RectTransform _star3;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
       
    }
}
