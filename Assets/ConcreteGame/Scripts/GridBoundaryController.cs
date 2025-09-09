using UnityEngine;

public class GridBoundaryController : MonoBehaviour
{
    public static GridBoundaryController Instance { get; private set; }

    [Header("Углы области")]
    [SerializeField] private Transform bottomLeft;   // левый нижний угол
    [SerializeField] private Transform topRight;     // правый верхний угол

    [Header("Сетка")]
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private Color gridColor = Color.green;
    [SerializeField] private Material lineMaterial; // материал для отрисовки (Unlit/Color)

    private float minX, maxX, minY, maxY;

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

    private void UpdateBounds()
    {
        if (bottomLeft == null || topRight == null) return;

        minX = Mathf.Min(bottomLeft.position.x, topRight.position.x);
        maxX = Mathf.Max(bottomLeft.position.x, topRight.position.x);
        minY = Mathf.Min(bottomLeft.position.y, topRight.position.y);
        maxY = Mathf.Max(bottomLeft.position.y, topRight.position.y);
    }

    // --- Рисуем сетку в Scene View ---
    private void OnDrawGizmos()
    {
        if (bottomLeft == null || topRight == null) return;

        float gSize = Mathf.Max(0.1f, gridSize);

        Gizmos.color = gridColor;

        // вертикальные линии
        for (float x = bottomLeft.position.x; x <= topRight.position.x; x += gSize)
        {
            Vector3 from = new Vector3(x, bottomLeft.position.y, 0);
            Vector3 to = new Vector3(x, topRight.position.y, 0);
            Gizmos.DrawLine(from, to);
        }

        // горизонтальные линии
        for (float y = bottomLeft.position.y; y <= topRight.position.y; y += gSize)
        {
            Vector3 from = new Vector3(bottomLeft.position.x, y, 0);
            Vector3 to = new Vector3(topRight.position.x, y, 0);
            Gizmos.DrawLine(from, to);
        }
    }

    // --- Рисуем сетку в Game View ---
    private void OnRenderObject()
    {
        if (lineMaterial == null) return;

        lineMaterial.SetPass(0);
        GL.PushMatrix();
        GL.Begin(GL.LINES);
        GL.Color(gridColor);

        float gSize = Mathf.Max(0.1f, gridSize);

        // Начальные позиции сетки (выравниваем к кратному gSize)
        float startX = Mathf.Floor(minX / gSize) * gSize;
        float startY = Mathf.Floor(minY / gSize) * gSize;

        // вертикальные линии
        for (float x = startX; x <= maxX; x += gSize)
        {
            GL.Vertex3(x, minY, 0);
            GL.Vertex3(x, maxY, 0);
        }

        // горизонтальные линии
        for (float y = startY; y <= maxY; y += gSize)
        {
            GL.Vertex3(minX, y, 0);
            GL.Vertex3(maxX, y, 0);
        }

        GL.End();
        GL.PopMatrix();
    }

    /// <summary>
    /// Ограничивает позицию внутри границ
    /// </summary>
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

}
