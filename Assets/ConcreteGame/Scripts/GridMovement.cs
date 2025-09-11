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
    
    [Header("Настройки проскока")]
    [SerializeField] private bool allowPassthrough = true; // включить проскок через препятствия

    private Camera mainCamera;
    private bool isDragging = false;
    private bool hasCollision = false;
    private int fingerId = -1; // отслеживание тача
    private Tween moveTween;   // твины движения

    // запрещённые направления (по колизиям)
    private bool blockLeft, blockRight, blockUp, blockDown;

    // --- DEBUG ---
    private Bounds debugBounds;   // последний проверочный бокс
    private bool drawDebug;       // включить/выключить рисование

    void Start()
    {
        mainCamera = Camera.main;

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
        
        // Постоянно проверяем соседей для определения блокировок и показа кнопок
        CheckNeighbors();
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

        Vector3 targetPos = new Vector3(snappedX, snappedY, transform.position.z);

        // Новая логика: если включен проскок и позиция пальца свободна, двигаемся туда
        if (allowPassthrough && CanMoveToPositionWithPassthrough(targetPos, gSize))
        {
            moveTween?.Kill();
            moveTween = transform.DOMove(targetPos, 0.1f).SetEase(Ease.OutQuad);
        }
        // Если проскок отключен, используем старую логику
        else if (!allowPassthrough && CanMoveToPosition(targetPos, gSize))
        {
            moveTween?.Kill();
            moveTween = transform.DOMove(targetPos, 0.1f).SetEase(Ease.OutQuad);
        }
    }

    /// <summary>
    /// Новый метод для проверки возможности движения с проскоком через препятствия
    /// </summary>
    private bool CanMoveToPositionWithPassthrough(Vector3 targetPos, float gSize)
    {
        // Проверяем, что в целевой позиции нет коллизий
        Bounds bounds = GetBounds(gSize);
        bounds.center = targetPos;

        // --- DEBUG ---
        debugBounds = bounds;
        drawDebug = true;

        // Проверяем коллизии в целевой позиции
        Collider2D[] hits = Physics2D.OverlapBoxAll(bounds.center, bounds.size * 0.9f, 0f);

        foreach (var hit in hits)
        {
            if (hit != null && hit.gameObject != gameObject)
                return false; // В целевой позиции есть препятствие
        }

        // Проверяем границы
        if (GridBoundaryController.Instance != null && !GridBoundaryController.Instance.IsInsideBounds(bounds.center))
            return false;

        return true; // Целевая позиция свободна, можно двигаться
    }

    private void CheckNeighbors()
    {
        // Сбрасываем все блокировки
        blockLeft = blockRight = blockUp = blockDown = false;

        float gSize = GridBoundaryController.Instance != null
            ? GridBoundaryController.Instance.GridSize
            : gridSize;

        Vector3 currentPos = transform.position;
        bool foundNeighbor = false;

        // Проверяем все 4 направления на наличие соседей на расстоянии одной клетки
        Vector3[] directions = {
            Vector3.right * gSize,    // право
            Vector3.left * gSize,     // лево  
            Vector3.up * gSize,       // вверх
            Vector3.down * gSize      // вниз
        };

        for (int i = 0; i < directions.Length; i++)
        {
            Vector3 checkPosition = currentPos + directions[i];
            
            // Проверяем есть ли объект в этой позиции
            Bounds checkBounds = GetBounds(gSize);
            checkBounds.center = checkPosition;
            
            Collider2D[] hits = Physics2D.OverlapBoxAll(checkBounds.center, checkBounds.size * 0.9f, 0f);
            
            foreach (var hit in hits)
            {
                if (hit != null && hit.gameObject != gameObject)
                {
                    foundNeighbor = true;
                    
                    // Блокируем направление к найденному соседу только если проскок отключен
                    if (!allowPassthrough)
                    {
                        switch (i)
                        {
                            case 0: blockRight = true; break;  // блокируем движение вправо
                            case 1: blockLeft = true; break;   // блокируем движение влево
                            case 2: blockUp = true; break;     // блокируем движение вверх
                            case 3: blockDown = true; break;   // блокируем движение вниз
                        }
                    }
                    break;
                }
            }
        }

        // Также проверяем текущую позицию на перекрытие с другими объектами
        Bounds currentBounds = GetBounds(gSize);
        Collider2D[] currentHits = Physics2D.OverlapBoxAll(currentBounds.center, currentBounds.size * 0.9f, 0f);
        
        foreach (var hit in currentHits)
        {
            if (hit != null && hit.gameObject != gameObject)
            {
                foundNeighbor = true;
                break;
            }
        }

        // Обновляем состояние кнопок
        if (foundNeighbor && !hasCollision)
        {
            hasCollision = true;
            ShowBuildButtons();
        }
        else if (!foundNeighbor && hasCollision)
        {
            hasCollision = false;
            HideBuildButtons();
        }
    }

    private bool CanMoveToPosition(Vector3 targetPos, float gSize)
    {
        Vector3 delta = targetPos - transform.position;

        // Нормализуем дельту к направлению движения на сетке
        float deltaX = Mathf.Round(delta.x / gSize);
        float deltaY = Mathf.Round(delta.y / gSize);

        // Проверяем блокировки основных направлений
        if (deltaX > 0 && blockRight) return false;  // движение вправо
        if (deltaX < 0 && blockLeft) return false;   // движение влево
        if (deltaY > 0 && blockUp) return false;     // движение вверх
        if (deltaY < 0 && blockDown) return false;   // движение вниз

        // Для диагонального движения проверяем оба направления
        if (deltaX > 0 && deltaY > 0 && (blockRight || blockUp)) return false;     // вправо-вверх
        if (deltaX < 0 && deltaY > 0 && (blockLeft || blockUp)) return false;      // влево-вверх
        if (deltaX > 0 && deltaY < 0 && (blockRight || blockDown)) return false;   // вправо-вниз
        if (deltaX < 0 && deltaY < 0 && (blockLeft || blockDown)) return false;    // влево-вниз

        // Проверяем, что в целевой позиции нет коллизий
        Bounds bounds = GetBounds(gSize);
        bounds.center = targetPos;

        // --- DEBUG ---
        debugBounds = bounds;
        drawDebug = true;

        Collider2D[] hits = Physics2D.OverlapBoxAll(bounds.center, bounds.size * 0.9f, 0f);

        foreach (var hit in hits)
        {
            if (hit != null && hit.gameObject != gameObject)
                return false;
        }

        // Проверяем границы
        if (GridBoundaryController.Instance != null && !GridBoundaryController.Instance.IsInsideBounds(bounds.center))
            return false;

        return true;
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

    private bool IsPointerOverThisObject(Vector3 screenPos)
    {
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);

        // Получаем все объекты в этой точке
        RaycastHit2D[] hits = Physics2D.RaycastAll(worldPos2D, Vector2.zero);
    
        // Ищем самый верхний объект (по Z-coordinate или по порядку рендеринга)
        RaycastHit2D topHit = default;
        float highestZ = float.MinValue;
    
        foreach (var hit in hits)
        {
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                float zPos = hit.collider.transform.position.z;
                if (zPos > highestZ)
                {
                    highestZ = zPos;
                    topHit = hit;
                }
            }
        }
    
        // Проверяем, является ли самый верхний объект нашим
        return topHit.collider != null && topHit.collider.transform == transform;
    }
    
    private bool IsPointerOverThisObjectWithLayer(Vector3 screenPos)
    {
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);

        // Проверяем только объекты на том же слое
        int layerMask = 1 << gameObject.layer;
        RaycastHit2D hit = Physics2D.Raycast(worldPos2D, Vector2.zero, Mathf.Infinity, layerMask);
    
        return hit.collider != null && hit.collider.transform == transform;
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

    // --- Рисование проверочного бокса ---
    private void OnDrawGizmos()
    {
        if (!drawDebug) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(debugBounds.center, debugBounds.size);
    }
}