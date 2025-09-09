using UnityEngine;

public class BuildingPoint : MonoBehaviour
{
    public DragAndDrop2D Owner { get; private set; }
    public bool IsSnapped => _snappedTo != null;

    private SnapPoint _snappedTo;

    void Awake()
    {
        Owner = GetComponentInParent<DragAndDrop2D>();
    }

    public void SetSnap(SnapPoint snapPoint)
    {
        _snappedTo = snapPoint;
    }

    public void ClearSnap()
    {
        _snappedTo = null;
    }

    public SnapPoint GetSnappedTo() => _snappedTo;
}