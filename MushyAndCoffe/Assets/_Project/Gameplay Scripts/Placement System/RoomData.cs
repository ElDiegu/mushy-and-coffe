using System.Collections.Generic;
using System.Linq;
using MushyAndCoffe.ScriptableObjects;
using EventSystem;
using UnityEngine;

namespace MushyAndCoffe.PlacementSystem
{
    /// <summary>
    /// This class stores all Furniture-realted data from the room such as location of placed furniture
    /// and cells occupied by them.
    /// </summary>
    public static class RoomData
    {
        public static Dictionary<Vector3Int, GameObject> CellInfoDictionary { get; private set; } = new Dictionary<Vector3Int, GameObject>();
        public static Dictionary<GameObject, HashSet<Vector3Int>> ObjectInfoDictionary { get; private set; } = new Dictionary<GameObject, HashSet<Vector3Int>>();
        
        public static bool IsPlacementAllowed(FurnitureSO furniture, Vector3Int cell) => IsPlacementAllowed(furniture.GetOccupyingCells(cell));
        
        public static bool IsPlacementAllowed(HashSet<Vector3Int> cells) 
        {
            foreach (var cell in cells) if (CellInfoDictionary.ContainsKey(cell)) return false;
            
            return true;
        }
        
        public static void OccupyCells(HashSet<Vector3Int> cells, GameObject furnitureObject) 
        {
            foreach (var cell in cells) CellInfoDictionary.Add(cell, furnitureObject);
            
            ObjectInfoDictionary.Add(furnitureObject, cells);
        }
        
        public static void OccupyCells(FurnitureSO furniture, Vector3Int cell, GameObject furnitureObject) => OccupyCells(furniture.GetOccupyingCells(cell), furnitureObject);
        
        public static void LiberateCells(HashSet<Vector3Int> cells) 
        {
            ObjectInfoDictionary.Remove(CellInfoDictionary[cells.First()]);
            
            foreach (var cell in cells) if (CellInfoDictionary.ContainsKey(cell)) CellInfoDictionary.Remove(cell);
        }
        
        public static void LiberateCells(GameObject furnitureObject) => LiberateCells(ObjectInfoDictionary[furnitureObject]); 
        
        public static GameObject GetFurnitureAt(Vector3Int cell) => CellInfoDictionary[cell];
        
        public static FurnitureSO GetFurnitureSOAt(Vector3Int cell) => GetFurnitureAt(cell).GetComponent<Furniture>().ScriptableObject as FurnitureSO;
    }
}
