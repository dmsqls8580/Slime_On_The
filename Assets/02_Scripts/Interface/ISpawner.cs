using UnityEngine;

public interface ISpawner
{
    public Transform Transform { get; set; }
    public int SpawnCount { get; set; }
    public float SpawnRadius { get; set; }
    public void RemoveObject(GameObject gameObject);
}