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
    
    public void SpawnOnGrid(GameObject prefab, GameObject button)
    {
        if (grid == null || prefab == null)
        {
            Debug.LogWarning("GridBoundaryController или Prefab не назначен!");
            return;
        }

        float gSize = grid.GridSize;

        // Получаем границы сетки через публичные методы/свойства
        Vector3 min = grid.GetBottomLeft(); // <- нужно добавить этот метод в GridBoundaryController
        Vector3 max = grid.GetTopRight();   // <- и этот метод

        List<Vector2> freeCells = new List<Vector2>();

        for (float x = min.x + gSize / 2f; x <= max.x; x += gSize)
        {
            for (float y = min.y + gSize / 2f; y <= max.y; y += gSize)
            {
                Vector2 cellCenter = new Vector2(x, y);

                // Проверка через Physics2D — есть ли GridMovement
                Collider2D hit = Physics2D.OverlapPoint(cellCenter);
                if (hit != null && hit.GetComponent<GridMovement>() != null)
                    continue;

                freeCells.Add(cellCenter);
            }
        }

        if (freeCells.Count == 0)
        {
            Debug.Log("Свободных клеток нет!");
            return;
        }

        Vector2 chosenCell = freeCells[Random.Range(0, freeCells.Count)];

        Vector3 spawnPos = new Vector3(chosenCell.x, chosenCell.y, 0);
        GameObject obj = Instantiate(prefab, spawnPos, prefab.transform.rotation);

        _currentObject = obj;
        
        UIController.Instance.CloseBuildingPanel();
    }
}
