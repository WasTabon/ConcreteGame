using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GridMovement : MonoBehaviour
{
    [SerializeField] private float _animSpeed = 0.15f;
    [SerializeField] private bool _isBuilded;

    [Header("UI кнопки")]
    [SerializeField] private RectTransform _yesButton;
    [SerializeField] private RectTransform _noButton;

    [SerializeField] private float gridSize = 1f;

    private Camera mainCamera;
    private bool isDragging = false;
    private bool hasNeighbor = false;
    private int fingerId = -1; // отслеживание тача
    private Tween moveTween;   // твины движения

    void Start()
    {
        mainCamera = Camera.main;

        // Скрыть кнопки при старте
        _yesButton.localScale = Vector3.zero;
        _noButton.localScale = Vector3.zero;

        _yesButton.gameObject.GetComponent<Button>().onClick.AddListener(AlLowBuild);
        _noButton.gameObject.GetComponent<Button>().onClick.AddListener(DenyBuild);
    }

    void Update()
    {
        if (_isBuilded) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
#else
        HandleTouchInput();
#endif

        if (isDragging)
            DragToGrid();
    }

    // --- ПК управление ---
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverThisObject(Input.mousePosition))
                isDragging = true;
        }
        if (Input.GetMouseButtonUp(0))
            isDragging = false;
    }

    // --- Мобильное управление ---
    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    if (IsPointerOverThisObject(touch.position))
                    {
                        isDragging = true;
                        fingerId = touch.fingerId;
                    }
                }

                if (touch.fingerId == fingerId &&
                   (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
                {
                    isDragging = false;
                    fingerId = -1;
                }
            }
        }
    }

    private void DragToGrid()
    {
        Vector3 screenPos = Input.touchCount > 0 && fingerId != -1
            ? (Vector3)Input.GetTouch(fingerId).position
            : Input.mousePosition;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);

        float gSize = GridBoundaryController.Instance != null
            ? GridBoundaryController.Instance.GridSize
            : gridSize;

        Bounds bounds = GetBounds(gSize);
        Vector3 size = bounds.size;

        float snappedX = Mathf.Round((worldPos.x - size.x / 2f) / gSize) * gSize + size.x / 2f;
        float snappedY = Mathf.Round((worldPos.y - size.y / 2f) / gSize) * gSize + size.y / 2f;

        worldPos.x = snappedX;
        worldPos.y = snappedY;
        worldPos.z = transform.position.z;

        if (CanPlaceAt(worldPos, gSize))
        {
            // плавное движение вместо телепорта
            moveTween?.Kill();
            moveTween = transform.DOMove(worldPos, 0.1f).SetEase(Ease.OutQuad);

            CheckNeighbors();
        }
    }

    private void CheckNeighbors()
    {
        float gSize = GridBoundaryController.Instance != null ? GridBoundaryController.Instance.GridSize : gridSize;
        Bounds bounds = GetBounds(gSize);

        int widthInCells = Mathf.CeilToInt(bounds.size.x / gSize);
        int heightInCells = Mathf.CeilToInt(bounds.size.y / gSize);

        float startX = Mathf.Round(bounds.min.x / gSize) * gSize;
        float startY = Mathf.Round(bounds.min.y / gSize) * gSize;

        bool foundNeighbor = false;

        for (int ix = 0; ix < widthInCells && !foundNeighbor; ix++)
        {
            for (int iy = 0; iy < heightInCells && !foundNeighbor; iy++)
            {
                Vector3 cellCenter = new Vector3(
                    startX + ix * gSize + gSize / 2f,
                    startY + iy * gSize + gSize / 2f,
                    transform.position.z);

                Vector3[] directions = new Vector3[]
                {
                    new Vector3(gSize, 0, 0),   // вправо
                    new Vector3(-gSize, 0, 0),  // влево
                    new Vector3(0, gSize, 0),   // вверх
                    new Vector3(0, -gSize, 0),  // вниз
                };

                foreach (var dir in directions)
                {
                    Vector3 checkPos = cellCenter + dir;
                    Collider2D[] hits = Physics2D.OverlapPointAll(checkPos);

                    foreach (var hit in hits)
                    {
                        if (hit != null && hit.GetComponent<GridMovement>() != null && hit.gameObject != gameObject)
                        {
                            foundNeighbor = true;
                            break;
                        }
                    }

                    if (foundNeighbor) break;
                }
            }
        }

        if (foundNeighbor && !hasNeighbor)
        {
            hasNeighbor = true;
            ShowBuildButtons();
        }
        else if (!foundNeighbor && hasNeighbor)
        {
            hasNeighbor = false;
            HideBuildButtons();
        }
    }

    private void ShowBuildButtons()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Join(_yesButton.DOScale(Vector3.one, _animSpeed).SetEase(Ease.OutBack));
        sequence.Join(_noButton.DOScale(Vector3.one, _animSpeed).SetEase(Ease.OutBack));
    }

    private void HideBuildButtons()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Join(_yesButton.DOScale(Vector3.zero, _animSpeed).SetEase(Ease.InBack));
        sequence.Join(_noButton.DOScale(Vector3.zero, _animSpeed).SetEase(Ease.InBack));
    }

    private Bounds GetBounds(float gSize)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Collider2D col = GetComponent<Collider2D>();

        if (sr != null) return sr.bounds;
        if (col != null) return col.bounds;

        return new Bounds(transform.position, Vector3.one * gSize);
    }

    private bool CanPlaceAt(Vector3 targetPos, float gSize)
    {
        Bounds bounds = GetBounds(gSize);
        Vector3 offset = targetPos - transform.position;
        bounds.center += offset;

        int widthInCells = Mathf.CeilToInt(bounds.size.x / gSize);
        int heightInCells = Mathf.CeilToInt(bounds.size.y / gSize);

        float startX = Mathf.Round(bounds.min.x / gSize) * gSize;
        float startY = Mathf.Round(bounds.min.y / gSize) * gSize;

        for (int ix = 0; ix < widthInCells; ix++)
        {
            for (int iy = 0; iy < heightInCells; iy++)
            {
                Vector3 cellCenter = new Vector3(
                    startX + ix * gSize + gSize / 2f,
                    startY + iy * gSize + gSize / 2f,
                    targetPos.z);

                if (!GridBoundaryController.Instance.IsInsideBounds(cellCenter))
                    return false;

                Collider2D[] hits = Physics2D.OverlapPointAll(cellCenter);
                foreach (var hit in hits)
                {
                    if (hit != null && hit.GetComponent<GridMovement>() != null && hit.gameObject != gameObject)
                        return false;
                }
            }
        }

        return true;
    }

    private bool IsPointerOverThisObject(Vector3 screenPos)
    {
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(worldPos2D, Vector2.zero);

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.transform == transform)
                return true;
        }

        return false;
    }

    public void AlLowBuild()
    {
        _isBuilded = true;
        HideBuildButtons();
        LevelController.Instance.AllowBuild();
    }

    public void DenyBuild()
    {
        LevelController.Instance.DenyBuild();
    }
}
