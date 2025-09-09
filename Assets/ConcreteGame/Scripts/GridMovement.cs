using UnityEngine;
using System.Collections.Generic;

public class GridMovement : MonoBehaviour
{
    [SerializeField] private float gridSize = 1f;
    private Camera mainCamera;
    private bool isDragging = false;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // --- ПК (мышь) ---
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverThisObject(Input.mousePosition))
                isDragging = true;
        }
        if (Input.GetMouseButtonUp(0))
            isDragging = false;

        // --- Телефон (тач) ---
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began && IsPointerOverThisObject(touch.position))
                isDragging = true;

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                isDragging = false;
        }

        // --- Двигаем, если держим ---
        if (isDragging)
            DragToGrid();
    }

    private void DragToGrid()
    {
        Vector3 screenPos = Input.touchCount > 0 ?
            (Vector3)Input.GetTouch(0).position :
            Input.mousePosition;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);

        float gSize = GridBoundaryController.Instance != null ? GridBoundaryController.Instance.GridSize : gridSize;

        // --- Размер объекта ---
        Bounds bounds = GetBounds(gSize);

        Vector3 size = bounds.size;

        float snappedX = Mathf.Round((worldPos.x - size.x / 2f) / gSize) * gSize + size.x / 2f;
        float snappedY = Mathf.Round((worldPos.y - size.y / 2f) / gSize) * gSize + size.y / 2f;

        worldPos.x = snappedX;
        worldPos.y = snappedY;
        worldPos.z = transform.position.z;

        // Проверка: можно ли сюда встать?
        if (CanPlaceAt(worldPos, gSize))
        {
            transform.position = worldPos;
        }
    }

    private Bounds GetBounds(float gSize)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Collider2D col = GetComponent<Collider2D>();

        if (sr != null) return sr.bounds;
        if (col != null) return col.bounds;

        return new Bounds(transform.position, Vector3.one * gSize);
    }

    /// <summary>
    /// Проверяем, помещается ли объект в пределах сетки и не пересекается ли с другими
    /// </summary>
    private bool CanPlaceAt(Vector3 targetPos, float gSize)
    {
        Bounds bounds = GetBounds(gSize);
        Vector3 offset = targetPos - transform.position;
        bounds.center += offset; // смещаем bounds в потенциальное место

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

                // --- 1. Проверяем границы ---
                if (!GridBoundaryController.Instance.IsInsideBounds(cellCenter))
                    return false;

                // --- 2. Проверяем занятость ---
                Collider2D hit = Physics2D.OverlapPoint(cellCenter);
                if (hit != null && hit.GetComponent<GridMovement>() != null && hit.gameObject != gameObject)
                {
                    return false; // клетка занята другим объектом
                }
            }
        }

        return true;
    }

    private bool IsPointerOverThisObject(Vector3 screenPos)
    {
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);

        RaycastHit2D hit = Physics2D.Raycast(worldPos2D, Vector2.zero);
        return hit.collider != null && hit.collider.transform == transform;
    }
}
