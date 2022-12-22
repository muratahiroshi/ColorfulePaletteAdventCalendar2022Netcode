using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject moveXFloorPrefab;
    [SerializeField] private GameObject scoreBoardPrefab;

    private string _textIpAddress = "127.0.0.1";
    private string _port = "7777";
    private string _playerName = "プレイヤー名" + (new Random().Next(100, 1000));

    public string PlayerName
    {
        get => _playerName;
    }

    private void Awake()
    {
        NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;

        Application.targetFrameRate = 60;
    }

    private void Update()
    {
#if UNITY_SERVER
        if (!NetworkManager.Singleton.IsServer)
        {
            StartServer();
        }
#endif
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 600));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
        }

        GUILayout.EndArea();
    }

    private void StartButtons()
    {
        // GUIStyle style = GUI.skin.GetStyle ("label");
        //
        // // styleのfont size　が経時的に大きくなったり小さくなったりするように設定 
        // // style.fontSize = (int)(30.0f);
        //
        // //ラベルを作って現在の設定で表示
        // GUILayout.Label("Fall Unity Chans", style);
        
        GUILayout.Label("Fall Unity Chans\n");
        
        if (GUILayout.Button("Server"))
        {
            StartServer();
        }

        if (GUILayout.Button("Host"))
        {
            StartHost();
        }

        if (GUILayout.Button("Client"))
        {
            StartClient(_textIpAddress.Trim(), Convert.ToUInt16(_port));
        }

        GUILayout.Label("IpAddress");
        _textIpAddress = GUILayout.TextField(_textIpAddress);

        GUILayout.Label("Port");
        _port = GUILayout.TextField(_port);

        GUILayout.Label("PlayerName");
        _playerName = GUILayout.TextField(_playerName);
    }

    private void StartServer()
    {
        NetworkManager.Singleton.OnServerStarted += OnStartServer;
        NetworkManager.Singleton.StartServer();
    }

    private void StartHost()
    {
        NetworkManager.Singleton.OnServerStarted += OnStartServer;
        NetworkManager.Singleton.StartHost();
    }

    private void StartClient(string ipAddress, ushort port)
    {
        var transport = Unity.Netcode.NetworkManager.Singleton.NetworkConfig.NetworkTransport;

        if (transport is Unity.Netcode.Transports.UTP.UnityTransport unityTransport)
        {
            unityTransport.SetConnectionData(ipAddress, port);
        }

        NetworkManager.Singleton.StartClient();
    }

    private void SpawnMoveXFloorPrefab(Vector3 position, Vector3 scale, int inverseCounter = 1500,
        float move = 0.002f, float initialDirection = 1.0f)
    {
        var gmo = GameObject.Instantiate(moveXFloorPrefab, position, Quaternion.identity);
        gmo.transform.localScale = scale;

        var moveXFloorObject = gmo.GetComponent<MoveXFloor>();
        moveXFloorObject.inverseCounter = inverseCounter;
        moveXFloorObject.move = move;
        moveXFloorObject.initialDirection = initialDirection;

        var netObject = gmo.GetComponent<NetworkObject>();
        netObject.Spawn(true);
    }

    private void OnStartServer()
    {
        // 動く障害物を生成
        SpawnMoveXFloorPrefab(
            new Vector3(-2.5f, 1.0f - 0.75f, 10.0f),
            new Vector3(1.0f, 1.0f, 1.0f),
            90,
            0.05f
        );
        SpawnMoveXFloorPrefab(
            new Vector3(1.5f, 1.0f - 0.75f, 9.0f),
            new Vector3(1.0f, 1.0f, 1.0f),
            250,
            0.01f,
            -1.0f
        );

        SpawnMoveXFloorPrefab(
            new Vector3(-2.5f, 1.0f - 0.55f, 6.5f),
            new Vector3(1.0f, 1.0f, 1.0f),
            250,
            0.01f
        );

        SpawnMoveXFloorPrefab(
            new Vector3(1.5f, 1.0f - 0.25f, 4.5f),
            new Vector3(1.0f, 1.0f, 1.0f),
            250,
            0.016f
        );

        SpawnMoveXFloorPrefab(
            new Vector3(-4.0f, 1.0f - 0.25f, 2.5f),
            new Vector3(1.0f, 1.0f, 1.0f),
            100,
            0.04f
        );

        SpawnMoveXFloorPrefab(
            new Vector3(0.5f, 1.0f, 1.5f),
            new Vector3(1.0f, 1.0f, 1.0f),
            200,
            0.015f
        );
        
        // スコア表示
        var gmo = GameObject.Instantiate(scoreBoardPrefab);
        var netObject = gmo.GetComponent<NetworkObject>();
        netObject.Spawn(true);
    }

    private void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ? "Host" :
            NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
                        NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }
}