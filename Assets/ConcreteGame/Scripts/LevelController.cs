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
        
        float gSize = grid.GridSize;

        // Получаем границы сетки
        Vector3 min = grid.GetBottomLeft();
        Vector3 max = grid.GetTopRight();

        List<Vector2> freeCells = new List<Vector2>();

        // Размер коллайдера у префаба
        Collider2D prefabCollider = prefab.GetComponent<Collider2D>();
        Vector2 checkSize = Vector2.one * gSize * 0.9f; // дефолт, если нет коллайдера
        if (prefabCollider != null)
        {
            checkSize = prefabCollider.bounds.size;
        }

        for (float x = min.x + gSize / 2f; x <= max.x; x += gSize)
        {
            for (float y = min.y + gSize / 2f; y <= max.y; y += gSize)
            {
                Vector2 cellCenter = new Vector2(x, y);

                // Проверяем пространство под объект
                Collider2D hit = Physics2D.OverlapBox(cellCenter, checkSize, 0f);
                if (hit != null)
                    continue;

                freeCells.Add(cellCenter);
            }
        }

        if (freeCells.Count == 0)
        {
            Debug.Log("Свободных клеток нет!");
            return;
        }

        // Берём случайную клетку
        Vector2 chosenCell = freeCells[Random.Range(0, freeCells.Count)];
        Vector3 spawnPos = new Vector3(chosenCell.x, chosenCell.y, 0);

        GameObject obj = Instantiate(prefab, spawnPos, prefab.transform.rotation);

        _currentObject = obj;

        UIController.Instance.CloseBuildingPanel();
    }
}
