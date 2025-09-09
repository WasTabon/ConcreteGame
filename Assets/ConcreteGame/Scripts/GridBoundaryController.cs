using UnityEngine;
using DG.Tweening;

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
    [SerializeField] private Material lineMaterial;

    [Header("Анимация")]
    [SerializeField] private float animationDuration = 2f;
    [SerializeField] private Ease animationEase = Ease.Linear;

    private float minX, maxX, minY, maxY;
    
    private float animationProgress = 0f;

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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGrid) return;
        if (bottomLeft == null || topRight == null) return;

        UpdateBounds();

        float gSize = Mathf.Max(0.1f, gridSize);
        Gizmos.color = gridColor;

        for (float x = minX; x <= maxX; x += gSize)
        {
            Vector3 from = new Vector3(x, minY, 0);
            Vector3 to = new Vector3(x, maxY, 0);
            Gizmos.DrawLine(from, to);
        }

        for (float y = minY; y <= maxY; y += gSize)
        {
            Vector3 from = new Vector3(minX, y, 0);
            Vector3 to = new Vector3(maxX, y, 0);
            Gizmos.DrawLine(from, to);
        }
    }
#endif

    private void OnRenderObject()
    {
        if (lineMaterial == null) return;

        lineMaterial.SetPass(0);
        GL.PushMatrix();
        GL.Begin(GL.LINES);
        GL.Color(gridColor);

        float gSize = Mathf.Max(0.1f, gridSize);

        int cols = Mathf.CeilToInt((maxX - minX) / gSize);
        int rows = Mathf.CeilToInt((maxY - minY) / gSize);

        int totalCells = cols * rows;

        int visibleCells = Mathf.FloorToInt(totalCells * animationProgress);

        int drawn = 0;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (drawn >= visibleCells) break;

                float x = minX + c * gSize;
                float y = minY + r * gSize;

                Vector3 bl = new Vector3(x, y, 0);            
                Vector3 br = new Vector3(x + gSize, y, 0);       
                Vector3 tr = new Vector3(x + gSize, y + gSize, 0); 
                Vector3 tl = new Vector3(x, y + gSize, 0);
                
                GL.Vertex(bl); GL.Vertex(br);
                GL.Vertex(br); GL.Vertex(tr);
                GL.Vertex(tr); GL.Vertex(tl);
                GL.Vertex(tl); GL.Vertex(bl);

                drawn++;
            }
            if (drawn >= visibleCells) break;
        }

        GL.End();
        GL.PopMatrix();
    }

    public void AnimateGrid()
    {
        animationProgress = 0f;
        DOTween.To(() => animationProgress, x => animationProgress = x, 1f, animationDuration)
            .SetEase(animationEase);
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
}
