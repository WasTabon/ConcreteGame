using System.Collections.Generic;
using UnityEngine;

public class DragAndDrop2D : MonoBehaviour
{
    private Vector3 offset;
    private Camera cam;

    private bool isSnapped = false;
    private List<(Component, Component)> activeSnaps = new List<(Component, Component)>();

    [Header("Snap Settings")]
    public float snapBreakDistance = 1.0f;

    private Vector3 dragStartMouseWorld; // точка начала перетаскивания

    void Start()
    {
        cam = Camera.main;
    }

    void OnMouseDown()
    {
        LevelController.Instance.currentBuilding = gameObject;
        
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = transform.position.z;

        offset = transform.position - mouseWorld;
        dragStartMouseWorld = mouseWorld; // запоминаем стартовую точку
    }

    void OnMouseDrag()
    {
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = transform.position.z;

        if (isSnapped)
        {
            CheckBreakSnaps(mouseWorld);
        }
        else
        {
            transform.position = mouseWorld + offset;
        }
    }

    public void OnSnapConnected(Component myPoint, Component otherPoint, GameObject owner)
    {
        if (LevelController.Instance.currentBuilding != owner) 
            return;
        
        Debug.Log("Called snap", gameObject);
        
        if (!activeSnaps.Exists(s => (s.Item1 == myPoint && s.Item2 == otherPoint) ||
                                     (s.Item1 == otherPoint && s.Item2 == myPoint)))
        {
            activeSnaps.Add((myPoint, otherPoint));
        }

        isSnapped = true;

        Vector3 delta = myPoint.transform.position - otherPoint.transform.position;
        transform.position -= delta;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = transform.position.z;

        offset = transform.position - mouseWorld;
        dragStartMouseWorld = mouseWorld;
    }

    public void OnSnapDisconnected(Component myPoint, Component otherPoint)
    {
        activeSnaps.RemoveAll(s => (s.Item1 == myPoint && s.Item2 == otherPoint) ||
                                   (s.Item1 == otherPoint && s.Item2 == myPoint));

        if (activeSnaps.Count == 0)
        {
            isSnapped = false;
        }
    }

    /// <summary>
    /// Проверяем, насколько далеко утащили мышь от точки старта
    /// </summary>
    private void CheckBreakSnaps(Vector3 currentMouseWorld)
    {
        float distance = Vector3.Distance(dragStartMouseWorld, currentMouseWorld);

        if (distance > snapBreakDistance)
        {
            // разрываем все активные снапы
            for (int i = activeSnaps.Count - 1; i >= 0; i--)
            {
                var (myPoint, otherPoint) = activeSnaps[i];
                if (myPoint is SnapPoint snap) snap.BreakSnap();
                else if (otherPoint is SnapPoint snapOther) snapOther.BreakSnap();
            }

            activeSnaps.Clear();
            isSnapped = false;

            // переносим объект за курсором
            transform.position = currentMouseWorld + offset;
        }
    }
}
