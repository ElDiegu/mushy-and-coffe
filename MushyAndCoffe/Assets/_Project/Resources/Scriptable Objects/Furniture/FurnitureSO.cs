using System.Collections.Generic;
using MushyAndCoffe.Enums;
using UnityEngine;

namespace MushyAndCoffe.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Furniture", menuName = "Mushy And Coffe/Furniture")]
    public class FurnitureSO : ScriptableObject
    {
        [field: SerializeField]      
        public int ID { get; private set; }

        [field: SerializeField]
        public string Name { get; private set;}

        [field: SerializeField]
        public string Description { get; private set; }

        [field: SerializeField]
        public Vector3 Size { get; private set; }

        [field: SerializeField]
        public GameObject Prefab { get; set; }
        
        [field: SerializeField]
        public FurnitureSurface Surface { get; private set; }

        [field: SerializeField]
        public Sprite Icon { get; private set; }
        
        [field: SerializeField]
        public FurnitureStackType Stack { get; private set; }
  
        public HashSet<Vector3Int> GetOccupyingCells(Vector3Int pivotCell) 
        {
            HashSet<Vector3Int> cellSet = new HashSet<Vector3Int>() { pivotCell };
            
            for (int i = 0; i < Size.x; i++) 
                for (int j = 0; j < Size.y; j ++)
                    cellSet.Add(pivotCell + new Vector3Int(i, 0, j));
            
            return cellSet;
        }      
    }
}

namespace MushyAndCoffe.Enums 
{
    public enum FurnitureSurface
    {
        Floor,
        Wall,
        Ceiling
    }

    public enum FurnitureStackType
    {
        Normal,
        Small,
        NoStack
    }
}
