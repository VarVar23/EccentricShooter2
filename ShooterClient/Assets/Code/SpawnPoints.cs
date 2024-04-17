using UnityEngine;

public class SpawnPoints : MonoBehaviour
{
    [SerializeField] private Transform[] _spawnPointsTransform;

    public int Length { get { return _spawnPointsTransform.Length; } }

    public void GetPoint(int index, out Vector3 position, out Vector3 rotation)
    {
        if(index >= _spawnPointsTransform.Length)
        {
            position = Vector3.zero;
            rotation = Vector3.zero;
            return;
        }

        position = _spawnPointsTransform[index].position;
        rotation = _spawnPointsTransform[index].eulerAngles;
    }
}