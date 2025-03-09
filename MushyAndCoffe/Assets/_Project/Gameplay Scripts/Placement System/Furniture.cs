using MushyAndCoffe.ScriptableObjects;
using UnityEngine;

public class Furniture : MonoBehaviour, ISOContainer<FurnitureSO>
{
    [field: SerializeField]
    public FurnitureSO ScriptableObject { get ; set; }
}
