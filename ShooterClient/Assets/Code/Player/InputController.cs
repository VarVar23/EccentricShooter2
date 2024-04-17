using System;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public PlayerGun PlayerGun;

    [SerializeField] private PlayerCharacter _player;
    [SerializeField] private PlayerChangeWeapon _playerChangeWeapons;
    [SerializeField] private float _restartDelay = 3;
    [SerializeField] private float _mouseSens;

    private MultiplayerManager _multiplayerManager;
    private SpawnPoints _spawnPoints;
    private float _rawHorizontal;
    private float _rawVertical;
    private float _mouseX;
    private float _mouseY;
    private float _mouseScroll;
    private bool _space;
    private bool _isShoot;
    private bool _hold = false;
    private bool _hideCursor;


    private void Start()
    {
        _multiplayerManager = MultiplayerManager.Instance;
        _spawnPoints = FindObjectOfType<SpawnPoints>();
        _hideCursor = true;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            _hideCursor = !_hideCursor;
            Cursor.lockState = _hideCursor ? CursorLockMode.Locked : CursorLockMode.None;
        }

        if (_hold) return;

        _rawHorizontal = Input.GetAxisRaw("Horizontal");
        _rawVertical = Input.GetAxisRaw("Vertical");

        _mouseX = 0;
        _mouseY = 0;
        _mouseScroll = 0;

        if (_hideCursor)
        {
            _mouseX = Input.GetAxis("Mouse X");
            _mouseY = Input.GetAxis("Mouse Y");
            _mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        }

        _space = Input.GetKeyDown(KeyCode.Space);

        _isShoot = Input.GetMouseButton(0);

        _player.RotateX(-_mouseY * _mouseSens);
        _player.SetInput(_rawHorizontal, _rawVertical, _mouseX * _mouseSens);
        _playerChangeWeapons.ChangeWeapon(_mouseScroll);

        if (_space) _player.Jump();
        if (Input.GetKeyDown(KeyCode.LeftControl)) _player.Squat(true);
        if (Input.GetKeyUp(KeyCode.LeftControl)) _player.Squat(false);

        if (_isShoot && PlayerGun.TryShoot(out ShootInfo info))
        {
            SendShoot(ref info);
        }

        SendMove();
    }

    private void SendShoot(ref ShootInfo info)
    {
        info.key = _multiplayerManager.GetSessionId();
        string json = JsonUtility.ToJson(info);

        _multiplayerManager.SendMessage("shoot", json);
    }

    private void SendMove()
    {
        _player.GetMoveInfo(out Vector3 position, out Vector3 velocity, out float rotateX, out float rotateY, out bool squat);

        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            {"pX", position.x },
            {"pY", position.y },
            {"pZ", position.z },

            {"vX", velocity.x},
            {"vY", velocity.y},
            {"vZ", velocity.z},

            { "rX", rotateX },
            { "rY", rotateY },

            { "s", squat }
        };

        _multiplayerManager.SendMessage("move", data);
    }

    public void Restart(int spawnIndex)
    {
        _multiplayerManager.SpawnPoints.GetPoint(spawnIndex, out Vector3 position, out Vector3 rotation);
        StartCoroutine(Hold());

        _player.transform.position = position;
        rotation = new Vector3(0, rotation.y, 0);
        _player.transform.eulerAngles = rotation;
        _player.SetInput(0, 0, 0);

        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            {"pX", position.x },
            {"pY", position.y },
            {"pZ", position.z },

            {"vX", 0},
            {"vY", 0},
            {"vZ", 0},

            { "rX", 0 },
            { "rY", rotation.y },

            { "s", false}
        };

        _multiplayerManager.SendMessage("move", data);
    }

    private System.Collections.IEnumerator Hold()
    {
        _hold = true;
        yield return new WaitForSeconds(_restartDelay);

        _hold = false;
    }
}

[Serializable]
public struct ShootInfo
{
    public string key;

    public float dX;
    public float dY;
    public float dZ;

    public float pX;
    public float pY;
    public float pZ;
}

[Serializable]
public struct RestartInfo
{
    public float x;
    public float z;
}