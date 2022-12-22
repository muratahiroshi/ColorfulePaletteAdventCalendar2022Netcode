using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    private Rigidbody _rigidbody;
    private Animator _animator;
    private Vector2 _moveInput;
    private bool _isKeySpace;
    private bool _isForceMoving = true;
    private NetworkVariable<Unity.Collections.FixedString64Bytes> _playerName = new();
    public int recastSecond = 5;
    private int _outAreaCount = 0;
    private float firedTime;
    [FormerlySerializedAs("bulletOffsetPosition")] public float bulletSpawnOffsetPosition = 0.9f;

    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float rotationSpeed = 20.0f;
    [SerializeField] private TextMesh playerNameTextMesh;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        _playerName.OnValueChanged += OnChangePlayerName;
        
        // 先に接続済みのプレイヤーオブジェクトはplayerNameがセットされているので代入する。またOnValueChangedは実行されない。
        playerNameTextMesh.text = _playerName.Value.Value;
    }

    [Unity.Netcode.ServerRpc]
    private void SetInputServerRpc(float x, float y, bool space)
    {
        _moveInput = new Vector2(x, y);
        _isKeySpace = space;
    }

    private void Update()
    {
        if (IsOwner)
        {
            SetInputServerRpc(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical"),
                Input.GetKey(KeyCode.Space)
            );
        }
    }

    private void FixedUpdate()
    {
        if (IsServer)
        {
            var moveVector = new Vector3(_moveInput.x, 0, _moveInput.y);
            if (moveVector.magnitude > 1)
            {
                moveVector.Normalize();
            }

            var coefficient = (moveSpeed * moveVector.magnitude - _rigidbody.velocity.magnitude) / Time.fixedDeltaTime;

            _rigidbody.AddForce(moveVector * coefficient);

            // 移動量が0の時は回転計算をしない。方向がリセットされるため。
            if (coefficient > 0)
            {
                transform.localRotation = Quaternion.Lerp(
                    transform.localRotation,
                    Quaternion.LookRotation(moveVector),
                    rotationSpeed * Time.deltaTime
                );
            }

            _animator.SetBool("Running", coefficient > 0);

            if (_isKeySpace)
            {
                var time = Time.time;
                // if (firedTime == null || firedTime + recastSecond >= time)
                if (firedTime == 0 || firedTime + recastSecond <= time)
                {
                    SpawnBulletPrefab();
                    firedTime = time;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if (IsServer)
        {
            if (col.gameObject.CompareTag("OutArea") && _isForceMoving == false)
            {
                var scoreBoard = GameObject.FindWithTag("ScoreBoard").GetComponent<ScoreBoard>();
                MoveToStartPosition();
                scoreBoard.addClientScore(OwnerClientId, _playerName.Value.Value, -1);
                scoreBoard.addClientLog(string.Format(
                    _outAreaCount % 2 == 0 ? "{0}が落ちました" : "{0}がまた落ちました"
                    , _playerName.Value.Value));
                _outAreaCount++;
                _isForceMoving = true;
            }
            if (col.gameObject.CompareTag("EndFloor") && _isForceMoving == false)
            {
                var scoreBoard = GameObject.FindWithTag("ScoreBoard").GetComponent<ScoreBoard>();
                scoreBoard.addClientScore(OwnerClientId, _playerName.Value.Value, 10);
                scoreBoard.addClientLog(
                    string.Format(_outAreaCount > 0 ? "{0}が苦労してゴールしました" : "{0}がゴールしました", _playerName.Value.Value)
                );
                MoveToStartPosition();
                _outAreaCount = 0;
                _isForceMoving = true;
            }
        }
    }
    
    private void OnCollisionExit(Collision col)
    {
        if (IsServer)
        {
            Debug.Log("tag " +  col.gameObject.tag);
            if (col.gameObject.CompareTag("StartFloor"))
            {
                _isForceMoving = false;
            }
        }
    }


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("OnNetworkSpawn IsServer");
            MoveToStartPosition();
        }

        if (IsOwner)
        {
            Debug.Log("OnNetworkSpawn IsOwner");
            var camera = Camera.main.GetComponent<PlayerFollowCamera>();
            camera.Player = transform;

            var gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            SetPlayerNameServerRpc(gameManager.PlayerName);
        }
    }

    [Unity.Netcode.ServerRpc(RequireOwnership = true)]
    private void SetPlayerNameServerRpc(string playerName)
     {
        _playerName.Value = playerName;
    }
    
    void OnChangePlayerName(Unity.Collections.FixedString64Bytes prev, Unity.Collections.FixedString64Bytes current)
    {
        Debug.Log("OnChangePlayerName");
        if (playerNameTextMesh != null)
        {
            playerNameTextMesh.text = current.Value;
        }
    }

    private void MoveToStartPosition()
    {
        transform.position = new Vector3(Random.Range(-4f, 4f), 0.5f, -1);
    }
    
    private void SpawnBulletPrefab()
    {
        var direction = Quaternion.Euler(transform.rotation.eulerAngles) * Vector3.forward;
        
        var gmo = GameObject.Instantiate(bulletPrefab, transform.position + new Vector3(0, 1.0f, 0) + direction * this.bulletSpawnOffsetPosition, Quaternion.identity);
        gmo.transform.localScale = new Vector3(1.0f, 0.5f, 1.0f);
        // gmo.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        
        var bullet = gmo.GetComponent<Bullet>();
        bullet.direction = direction;

        var netObject = gmo.GetComponent<NetworkObject>();
        netObject.Spawn(true);
    }
}