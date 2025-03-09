using UnityEngine;

public interface ISOContainer<T> where T : ScriptableObject
{
    public abstract T ScriptableObject { get; set; }
}
