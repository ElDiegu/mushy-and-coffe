using System.Collections.Generic;
using MushyAndCoffe.Enums;
using MushyAndCoffe.Extensions;
using MushyAndCoffe.Managers;
using MushyAndCoffe.ScriptableObjects;
using EventSystem;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using Unity.VisualScripting;

namespace MushyAndCoffe.PlacementSystem
{
    public class PlacementManager : Singleton<PlacementManager>
    {
        [Header("System Parameters")]
        [SerializeField] private FurnitureSO selectedFurniture = null;
        [SerializeField] private Grid grid;
        [SerializeField] private Camera usedCamera;
        private Physics physics;
        public PlacementMode placementMode;

        [Header("Preview")]
        [SerializeField] private GameObject previewObject = null;
        [SerializeField] private GameObject cellSelected;
        [SerializeField] private LayerMask placementLayerMask, furnitureLayerMask;
        [SerializeField] private GameObject cellSelector = null;

        private void Update()
        {
            var input = InputManager.Instance.GetInput();

            if (input.LeftClick) 
                switch (placementMode)
                {
                    case PlacementMode.Placement:
                        PlaceObject();
                        break;
                    case PlacementMode.Selection:
                        if (selectedFurniture == null) SelectPlacedObject();
                        else PlaceObject();
                        break;
                }

            if (input.RotateObject) RotateObject(90);

            if (input.Delete) DeleteFurniture();

            UpdatePreview();
        }

        public void ChangePlacementMode(PlacementMode placementMode)
        {
            this.placementMode = placementMode;
            selectedFurniture = null;
            DeletePreview();
        }

        private void PlaceObject()
        {
            if (previewObject == null || !previewObject.activeSelf) return;

            bool hit = physics.MouseRaycastFromCamera(Input.mousePosition, usedCamera, 100f, placementLayerMask, out Vector3 clickLocation);

            if (!hit) return;

            var cell = grid.WorldToCell(previewObject.transform.position);
            var furnitureSO = previewObject.GetComponent<Furniture>().ScriptableObject as FurnitureSO;

            if (!RoomData.IsPlacementAllowed(furnitureSO, cell)) return;

            var furnitureObject = Instantiate(previewObject, previewObject.transform.position, previewObject.transform.rotation);

            RoomData.OccupyCells(furnitureSO, cell, furnitureObject);

            if (placementMode == PlacementMode.Selection)
            {
                selectedFurniture = null;
                DeletePreview();
            }
        }

        private void RotateObject(float degrees)
        {
            // We can't rotate the object if it doesn't exists
            if (previewObject == null) return;

            // We can't rotate wall objects
            if ((previewObject.GetComponent<Furniture>().ScriptableObject as FurnitureSO).Surface == FurnitureSurface.Wall) return;

            var objectRotation = previewObject.transform.rotation;

            if (objectRotation.y + degrees >= 360) objectRotation.y = 0f;
            else objectRotation = Quaternion.Euler(objectRotation.eulerAngles.x, objectRotation.eulerAngles.y + degrees, objectRotation.eulerAngles.z);

            previewObject.transform.rotation = objectRotation;
        }

        private void SelectPlacedObject()
        {
            var hitDetected = physics.MouseRaycastFromCamera(Input.mousePosition, usedCamera, 100f, furnitureLayerMask, out RaycastHit hit);

            Debug.Log(hitDetected);

            if (!hitDetected) return;

            previewObject = hit.collider.gameObject;
            selectedFurniture = previewObject.GetComponent<Furniture>().ScriptableObject as FurnitureSO;

            RoomData.LiberateCells(previewObject);
        }

        private void DeleteFurniture()
        {
            selectedFurniture = null;
            DeletePreview();
            // TODO: add one item to the inventory when deleting
        }

#region Object preview
        private void UpdatePreview()
        {
            if (selectedFurniture == null) return;

            bool hitDetected = physics.MouseRaycastFromCamera(Input.mousePosition, usedCamera, 100f, placementLayerMask, out RaycastHit hit);

            var clickLocation = hit.point;

            if (!hitDetected)
            {
                if (previewObject != null) SetPreviewState(false);
                return;
            }

            // If we're in placement mode, have a selected furniture and we don't have a preview, we instantiate it
            if (previewObject == null && placementMode == PlacementMode.Placement && selectedFurniture != null)
                previewObject = Instantiate(selectedFurniture.Prefab, clickLocation, new Quaternion());

            //ChangeAlphaVisibility();
            
            // We hide floor furniture when over wall and wall furniture when over floor
            if (hit.normal == Vector3.up && selectedFurniture.Surface == FurnitureSurface.Floor)
                SetPreviewState(true);
            else if (hit.normal != Vector3.up && selectedFurniture.Surface == FurnitureSurface.Wall)
                SetPreviewState(true);
            else SetPreviewState(false);
            
            if (selectedFurniture.Surface == FurnitureSurface.Wall) previewObject.transform.rotation = Quaternion.LookRotation(hit.normal);

            var cell = grid.WorldToCell(clickLocation);
            
            var cellDetected = DetermineCell(cell, out cell);
            
            if (!cellDetected) SetPreviewState(false);
            else previewObject.transform.position = grid.GetCellCenterWorld(cell) - new Vector3(0, 0.5f, 0f);
            
            // TODO: Show cell marker
            // if (previewObject.activeSelf) cellSelector.transform.position = grid.GetCellCenterWorld(cell);
        }

        private bool DetermineCell(Vector3Int referenceCell, out Vector3Int cell) 
        {
            cell = referenceCell;
            
            if (RoomData.IsPlacementAllowed(selectedFurniture, referenceCell)) return true;
            
            if (!RoomData.IsPlacementAllowed(selectedFurniture, referenceCell + Vector3Int.up)) return false;
            
            if (RoomData.GetFurnitureSOAt(referenceCell).Stack == FurnitureStackType.NoStack) return false;
            
            if (selectedFurniture.Stack != FurnitureStackType.Small) return false;
            
            cell = referenceCell + Vector3Int.up;
            return true;
        }
        
        private void DeletePreview()
        {
            cellSelector.transform.SetParent(null, false);
            if (previewObject != null) Destroy(previewObject);
        }

        private void SetPreviewState(bool state)
        {
            if (previewObject != null) previewObject.SetActive(state);
            if (cellSelector != null) cellSelector.SetActive(state);
        }

        private void ChangeAlphaVisibility()
        {   
            Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                Material[] materials = renderer.materials;
                foreach (Material material in materials)
                {
                    material.SetFloat("_Transparency", 0.9f);
                }
            }
        }
        public void ChangePreviewObject(GameObject newPreviewObject)
        {
            selectedFurniture = newPreviewObject.GetComponent<Furniture>().ScriptableObject as FurnitureSO;
            DeletePreview();
        }
#endregion

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            bool hit = physics.MouseRaycastFromCamera(Input.mousePosition, usedCamera, 100f, placementLayerMask, out RaycastHit raycastHit);

            if (hit)
            {
                Gizmos.DrawSphere(raycastHit.point, 0.2f);
                Gizmos.DrawLine(raycastHit.point, raycastHit.point + raycastHit.normal);
            }
        }
#endif
    }

    public enum PlacementMode
    {
        Placement,
        Selection
    }
    
    [CustomEditor(typeof(PlacementManager))]
    public class PlacementManagerEditor : Editor 
    { 
        public PlacementMode newMode;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            newMode = (PlacementMode)EditorGUILayout.EnumPopup(newMode);
            if (GUILayout.Button("Change mode")) (target as PlacementManager).ChangePlacementMode(newMode);
        }
    }
}
