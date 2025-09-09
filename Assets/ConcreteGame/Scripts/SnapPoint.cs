using UnityEngine;

public class SnapPoint : MonoBehaviour
{
    public DragAndDrop2D Owner { get; private set; }
    public bool IsSnapped => _snappedTo != null;

    private BuildingPoint _snappedTo;

    void Awake()
    {
        Owner = GetComponentInParent<DragAndDrop2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BuildingPoint buildingPoint = other.GetComponent<BuildingPoint>();
        if (buildingPoint != null && !IsSnapped && !buildingPoint.IsSnapped)
        {
            if (buildingPoint.Owner == this.Owner)
                return;

            Debug.Log("Snap Set");
            
            _snappedTo = buildingPoint;
            buildingPoint.SetSnap(this);

            Owner.OnSnapConnected(this, buildingPoint, Owner.gameObject);
        }
    }

    public void BreakSnap()
    {
        if (_snappedTo != null)
        {
            var other = _snappedTo;
            _snappedTo = null;
            other.ClearSnap();
            Owner.gameObject.GetComponentInChildren<BuildingPoint>().ClearSnap();
            
            Owner.OnSnapDisconnected(this, other);
        }
    }

    public BuildingPoint GetSnappedTo() => _snappedTo;
}