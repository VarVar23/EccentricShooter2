using UnityEngine;

public class GunAnimation : MonoBehaviour
{
    [SerializeField] private Gun[] _guns;
    [SerializeField] private Animator[] _animators;
    private int _index;

    private void Start()
    {
        foreach (var gun in _guns)
            gun.ShootAction += Shoot;
    }

    public void SetIndex(int indexWeapon)
    {
        _index = indexWeapon;
    }

    private void Shoot()
    {
        _animators[_index].SetTrigger("shoot");
        Debug.Log("Выстрел");
    }

    private void OnDestroy()
    {
        foreach (var gun in _guns)
            gun.ShootAction -= Shoot;
    }
}