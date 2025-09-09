using UnityEngine;

public class ChainPoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.transform.parent != transform.parent)
        {
            if (coll.gameObject.TryGetComponent(out BuildPoint buildPoint))
            {
                if (transform.GetComponentInParent<MoveObject>().isSnapped == false)
                {
                    if (LevelController.Instance.currentBuilding == transform.parent.gameObject)
                    {
                        transform.GetComponentInParent<MoveObject>().SetSnap(transform, coll.transform);
                    }
                }
            }
        }
    }
}
