using UnityEngine;

public class GunRay : MonoBehaviour
{
    [SerializeField] private Transform _center;
    [SerializeField] private Transform _point;
    [SerializeField] private float _pointSize;
    [SerializeField] private LayerMask _layerMask;
    private Transform _camera;

    private void Awake() => _camera = Camera.main.transform;

    private void Update()
    {
        Ray ray = new Ray(_center.position, _center.forward);

        if(Physics.Raycast(ray, out RaycastHit hit, 50f, _layerMask, QueryTriggerInteraction.Ignore))
        {
            _center.localScale = new Vector3(1, 1, hit.distance);
            float distance = Vector3.Distance(_camera.position, hit.point);
            _point.localScale = Vector3.one * distance * _pointSize;
            _point.position = hit.point;
        }
    }
}