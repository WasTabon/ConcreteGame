using System;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Random = UnityEngine.Random;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance;

    [SerializeField] private int _neededBuldings;
    private int _buildingsCount;

    [SerializeField] private TextMeshProUGUI _buildingsCountText;
    [SerializeField] private RectTransform _startGameButton;
    
    [SerializeField] private GridBoundaryController grid;

    private GameObject _currentObject;
    private GameObject _currentButton;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _startGameButton.DOScale(Vector3.zero, 0f);
        _buildingsCountText.text = $"0/{_neededBuldings}";
    }

    public void DenyBuild()
    {
        _currentObject.SetActive(false);
    }

    public void AllowBuild()
    {
        _currentButton.SetActive(false);
        _buildingsCount++;
        _buildingsCountText.text = $"{_buildingsCount}/{_neededBuldings}";

        if (_buildingsCount >= _neededBuldings)
        {
            ShowStartGameButton();
        }
    }

    private void ShowStartGameButton()
    {
        _buildingsCountText.rectTransform.DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete((() =>
            {
                _startGameButton.DOScale(Vector3.one, 0.3f)
                    .SetEase(Ease.InBack);
            }));
    }

    public void StartGame()
    {
        _startGameButton.DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack);
        
        GridBoundaryController.Instance.HideGrid();
        
        GridMovement[] gridMovements = FindObjectsOfType<GridMovement>();

        foreach (var gm in gridMovements)
        {
            gm.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        }
    }
    
    public void SpawnOnGrid(GameObject prefab, GameObject button)
    {
        if (grid == null || prefab == null)
        {
            Debug.LogWarning("GridBoundaryController или Prefab не назначен!");
            return;
        }

        _currentButton = button;
        
        Vector2 spawnPosition = FindFreeSpawnPosition(prefab);
        
        if (spawnPosition != Vector2.zero)
        {
            Vector3 spawnPos = new Vector3(spawnPosition.x, spawnPosition.y, 0);
            GameObject obj = Instantiate(prefab, spawnPos, prefab.transform.rotation);
            _currentObject = obj;
            
            UIController.Instance.CloseBuildingPanel();
        }
        else
        {
            Debug.Log("Не удалось найти свободное место для спавна!");
            // Можно добавить уведомление для игрока о том, что нет места
        }
    }

    private Vector2 FindFreeSpawnPosition(GameObject prefab)
    {
        float gSize = grid.GridSize;

        // Получаем границы сетки
        Vector3 min = grid.GetBottomLeft();
        Vector3 max = grid.GetTopRight();

        // Определяем размер проверки на основе префаба
        Vector2 checkSize = GetPrefabCheckSize(prefab, gSize);

        List<Vector2> allCells = new List<Vector2>();
        List<Vector2> freeCells = new List<Vector2>();

        // Собираем все возможные позиции на сетке
        for (float x = min.x + gSize / 2f; x <= max.x; x += gSize)
        {
            for (float y = min.y + gSize / 2f; y <= max.y; y += gSize)
            {
                Vector2 cellCenter = new Vector2(x, y);
                allCells.Add(cellCenter);
            }
        }

        // Перемешиваем список для случайного порядка проверки
        for (int i = 0; i < allCells.Count; i++)
        {
            Vector2 temp = allCells[i];
            int randomIndex = Random.Range(i, allCells.Count);
            allCells[i] = allCells[randomIndex];
            allCells[randomIndex] = temp;
        }

        // Проверяем каждую позицию на свободность
        foreach (Vector2 cellCenter in allCells)
        {
            if (IsCellFree(cellCenter, checkSize))
            {
                return cellCenter;
            }
        }

        // Если не нашли свободных клеток, возвращаем нулевой вектор
        return Vector2.zero;
    }

    private Vector2 GetPrefabCheckSize(GameObject prefab, float gSize)
    {
        // Получаем размер из коллайдера префаба
        Collider2D prefabCollider = prefab.GetComponent<Collider2D>();
        if (prefabCollider != null)
        {
            return prefabCollider.bounds.size;
        }

        // Получаем размер из SpriteRenderer префаба
        SpriteRenderer prefabSprite = prefab.GetComponent<SpriteRenderer>();
        if (prefabSprite != null && prefabSprite.sprite != null)
        {
            return prefabSprite.bounds.size;
        }

        // Дефолтный размер равный одной клетке сетки
        return Vector2.one * gSize;
    }

    private bool IsCellFree(Vector2 position, Vector2 checkSize)
    {
        // Проверяем наличие коллайдеров в данной позиции
        // Используем размер чуть меньше для избежания ложных срабатываний на границах
        Vector2 adjustedSize = checkSize * 0.95f;
        
        Collider2D[] overlapping = Physics2D.OverlapBoxAll(position, adjustedSize, 0f);
        
        // Проверяем все найденные коллайдеры
        foreach (Collider2D col in overlapping)
        {
            if (col != null)
            {
                // Игнорируем триггеры (если они есть)
                if (col.isTrigger)
                    continue;
                    
                // Если нашли любой не-триггер коллайдер, позиция занята
                return false;
            }
        }

        // Проверяем, что позиция находится в границах сетки
        if (!grid.IsInsideBounds(position))
            return false;

        return true;
    }

    // Дополнительный метод для принудительного поиска места (если нужно)
    public Vector2 FindNearestFreePosition(Vector2 preferredPosition, GameObject prefab)
    {
        float gSize = grid.GridSize;
        Vector2 checkSize = GetPrefabCheckSize(prefab, gSize);

        // Сначала проверяем предпочитаемую позицию
        if (IsCellFree(preferredPosition, checkSize))
        {
            return preferredPosition;
        }

        // Спиральный поиск вокруг предпочитаемой позиции
        int maxRadius = Mathf.Max(
            Mathf.CeilToInt((grid.GetTopRight().x - grid.GetBottomLeft().x) / gSize),
            Mathf.CeilToInt((grid.GetTopRight().y - grid.GetBottomLeft().y) / gSize)
        );

        for (int radius = 1; radius <= maxRadius; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    // Проверяем только клетки на границе текущего радиуса
                    if (Mathf.Abs(x) == radius || Mathf.Abs(y) == radius)
                    {
                        Vector2 testPosition = preferredPosition + new Vector2(x * gSize, y * gSize);
                        
                        if (IsCellFree(testPosition, checkSize))
                        {
                            return testPosition;
                        }
                    }
                }
            }
        }

        return Vector2.zero; // Не найдено свободного места
    }
}