using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour
{
   public bool isSnapped;

   public float removeDistance;
    
   public List<BuildPoint> buildPoints;
   public List<ChainPoint> chainPoints;

   private List<MoveObject> snapedObjects;
   
   private Camera cam;
   
   private Vector3 dragStartMouseWorld;
   private Vector3 offset;
   
   void Start()
   {
      cam = Camera.main;
      snapedObjects = new List<MoveObject>();
   }

   void OnMouseDown()
   {
      LevelController.Instance.currentBuilding = gameObject;
        
      Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
      mouseWorld.z = transform.position.z;

      offset = transform.position - mouseWorld;
      dragStartMouseWorld = mouseWorld;
   }
   
   void OnMouseDrag()
   {
      Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
      mouseWorld.z = transform.position.z;

      if (isSnapped)
      {
         RemoveSnaps(mouseWorld);
      }
      else
      {
         transform.position = mouseWorld + offset;
      }
   }

   public void SetSnap(Transform mySnap, Transform otherSnap)
   {
      isSnapped = true;
      
      Vector3 worldOffset = otherSnap.position - mySnap.position;

      transform.position += worldOffset;
   }

   public void AddConnectedBuilding(MoveObject building)
   {
      snapedObjects.Add(building);
      if (snapedObjects.Count > 0)
      {
         isSnapped = true;
      }
   }

   public void RemoveConnectedBuilding(MoveObject building)
   {
      snapedObjects.Remove(building);
      if (snapedObjects.Count <= 0)
      {
         isSnapped = false;
      }
   }

   private void RemoveSnaps(Vector3 currentMouseWorld)
   {
      float distance = Vector3.Distance(dragStartMouseWorld, currentMouseWorld);

      if (distance > removeDistance)
      {
         isSnapped = false;
      }
   }
}
