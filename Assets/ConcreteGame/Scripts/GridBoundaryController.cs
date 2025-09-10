using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class GridBoundaryController : MonoBehaviour
{
    public static GridBoundaryController Instance { get; private set; }

    public bool showGrid = true;

    [Header("Углы области")]
    [SerializeField] private Transform bottomLeft;
    [SerializeField] private Transform topRight;

    [Header("Сетка")]
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private Color gridColor = Color.green;
    [SerializeField] private float lineWidth = 0.02f;
    [SerializeField] private Material lineMaterial;

    [Header("Анимация")]
    [SerializeField] private float animationDuration = 2f;
    [SerializeField] private Ease animationEase = Ease.Linear;

    private float minX, maxX, minY, maxY;
    private float animationProgress = 0f;

    private readonly List<LineRenderer> gridLines = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        UpdateBounds();
    }

    private void Start()
    {
        GenerateGrid();
        AnimateGrid();
    }

    private void UpdateBounds()
    {
        if (bottomLeft == null || topRight == null) return;

        float rawMinX = Mathf.Min(bottomLeft.position.x, topRight.position.x);
        float rawMaxX = Mathf.Max(bottomLeft.position.x, topRight.position.x);
        float rawMinY = Mathf.Min(bottomLeft.position.y, topRight.position.y);
        float rawMaxY = Mathf.Max(bottomLeft.position.y, topRight.position.y);

        float gSize = Mathf.Max(0.1f, gridSize);

        minX = Mathf.Floor(rawMinX / gSize) * gSize;
        maxX = Mathf.Ceil(rawMaxX / gSize) * gSize;
        minY = Mathf.Floor(rawMinY / gSize) * gSize;
        maxY = Mathf.Ceil(rawMaxY / gSize) * gSize;
    }

    private void GenerateGrid()
    {
        // Очистить старые линии
        foreach (var line in gridLines)
        {
            if (line != null) Destroy(line.gameObject);
        }
        gridLines.Clear();

        float gSize = Mathf.Max(0.1f, gridSize);

        // Вертикальные линии
        for (float x = minX; x <= maxX; x += gSize)
        {
            CreateLine(new Vector3(x, minY, 0), new Vector3(x, maxY, 0));
        }

        // Горизонтальные линии
        for (float y = minY; y <= maxY; y += gSize)
        {
            CreateLine(new Vector3(minX, y, 0), new Vector3(maxX, y, 0));
        }
    }

    private void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject go = new GameObject("GridLine");
        go.transform.parent = transform;

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPositions(new Vector3[] { start, end });
        lr.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Sprites/Default"));
        lr.startColor = gridColor;
        lr.endColor = gridColor;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.useWorldSpace = true;

        gridLines.Add(lr);
    }

    public void AnimateGrid()
    {
        animationProgress = 0f;

        DOTween.To(() => animationProgress, x => animationProgress = x, 1f, animationDuration)
            .SetEase(animationEase)
            .OnUpdate(UpdateLineVisibility);
    }

    private void UpdateLineVisibility()
    {
        int visibleCount = Mathf.FloorToInt(gridLines.Count * animationProgress);

        for (int i = 0; i < gridLines.Count; i++)
        {
            gridLines[i].enabled = (i < visibleCount);
        }
    }

    public Vector3 GetBottomLeft()
    {
        return bottomLeft.position;
    }

    public Vector3 GetTopRight()
    {
        return topRight.position;
    }
    
    public Vector3 ClampToBounds(Vector3 pos)
    {
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        return pos;
    }

    public float GridSize => gridSize;

    public bool IsInsideBounds(Vector3 pos)
    {
        return pos.x >= minX && pos.x <= maxX &&
               pos.y >= minY && pos.y <= maxY;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGrid) return;
        if (bottomLeft == null || topRight == null) return;

        UpdateBounds();

        float gSize = Mathf.Max(0.1f, gridSize);
        Gizmos.color = gridColor;

        // Вертикальные линии
        for (float x = minX; x <= maxX; x += gSize)
        {
            Vector3 from = new Vector3(x, minY, 0);
            Vector3 to = new Vector3(x, maxY, 0);
            Gizmos.DrawLine(from, to);
        }

        // Горизонтальные линии
        for (float y = minY; y <= maxY; y += gSize)
        {
            Vector3 from = new Vector3(minX, y, 0);
            Vector3 to = new Vector3(maxX, y, 0);
            Gizmos.DrawLine(from, to);
        }
    }
#endif
}
