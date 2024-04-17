using UnityEngine;

public class Skins : MonoBehaviour
{
    [SerializeField] private Material[] _materials;

    public int Length() => _materials.Length;

    public Material GetMaterial(int index)
    {
        if (index >= _materials.Length || index < 0) return _materials[0];

        return _materials[index];
    }
}