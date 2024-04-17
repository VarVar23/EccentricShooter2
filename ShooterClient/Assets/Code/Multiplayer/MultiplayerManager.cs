using Colyseus;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerManager : ColyseusManager<MultiplayerManager>
{
    [field: SerializeField] public LossCounter LossCounter { get; private set; }
    [field: SerializeField] public SpawnPoints SpawnPoints { get; private set; }
    [SerializeField] private Skins _skins;

    [SerializeField] private PlayerCharacter _player;
    [SerializeField] private EnemyController _enemy;

    private ColyseusRoom<State> _room;
    private Dictionary<string, EnemyController> _enemies = new Dictionary<string, EnemyController>();

    protected override void Awake()
    {
        base.Awake();
        Instance.InitializeClient();
        Connect();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _room.Leave();
    }

    public void SendMessage(string key, Dictionary<string, object> data)
    {
        _room.Send(key, data);
    }

    public void SendMessage(string key, string data)
    {
        _room.Send(key, data);
    }

    public string GetSessionId() => _room.SessionId;

    private async void Connect()
    {
        SpawnPoints.GetPoint(Random.Range(0, SpawnPoints.Length), out Vector3 spawnPosition, out Vector3 spawnRotation);

        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            { "skins", _skins.Length() },
            { "points", SpawnPoints.Length },
            { "speed", _player.Speed },
            { "hp", _player.MaxHealth },
            { "pX", spawnPosition.x },
            { "pY", spawnPosition.y },
            { "pZ", spawnPosition.z },
            { "rY", spawnRotation.y }
        };

        _room = await Instance.client.JoinOrCreate<State>("state_handler", data);

        _room.OnStateChange += OnChange;
        _room.OnMessage<string>("Shoot", ApplyShoot);
    }

    private void ApplyShoot(string jsonShootInfo)
    {
        var info = JsonUtility.FromJson<ShootInfo>(jsonShootInfo);

        if (!_enemies.ContainsKey(info.key))
        {
            Debug.LogError("Îøèáêà :(");
            return;
        }

        _enemies[info.key].Shoot(info);
    }

    private void OnChange(State state, bool isFirstState)
    {
        if (!isFirstState) return;

        state.players.ForEach((key, player) => {
            if (key == _room.SessionId) CreatePlayer(player);
            else CreateEnemy(key, player);
        });

        _room.State.players.OnAdd += CreateEnemy;
        _room.State.players.OnRemove += RemoveEnemy;
    }

    private void CreatePlayer(Player player)
    {
        var position = new Vector3(player.pX, player.pY, player.pZ);

        Quaternion rotation = Quaternion.Euler(0, player.rY, 0);
        var playerCharacter = Instantiate(_player, position, rotation);
        player.OnChange += playerCharacter.OnChange;

        _room.OnMessage<int>("Restart", playerCharacter.GetComponent<InputController>().Restart);
        playerCharacter.GetComponent<SetSkin>().Set(_skins.GetMaterial(player.skin));
    }

    private void CreateEnemy(string key, Player player)
    {
        var position = new Vector3(player.pX, player.pY, player.pZ);
        var enemy = Instantiate(_enemy, position, Quaternion.identity);

        enemy.Init(key, player);
        enemy.GetComponent<SetSkin>().Set(_skins.GetMaterial(player.skin));

        _room.OnMessage<string>("ChangeWeapon", enemy.ChangeWeapon);

        _enemies.Add(key, enemy);
    }

    private void RemoveEnemy(string key, Player player) 
    {
        if (!_enemies.ContainsKey(key)) return;

        var enemy = _enemies[key];
        _enemies.Remove(key);
    }
}