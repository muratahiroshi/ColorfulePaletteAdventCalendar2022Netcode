using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Netcode;

public class ScoreBoard : NetworkBehaviour
{
    private struct ClientInfo
    {
        public ulong ClientId;
        public string PlayerName;
        public int Score;
    }
    private Dictionary<ulong, ClientInfo> _clientScoreTable = new Dictionary<ulong, ClientInfo>();
    private NetworkVariable<Unity.Collections.FixedString4096Bytes> _scoreInfo =
        new NetworkVariable<Unity.Collections.FixedString4096Bytes>();
    private NetworkVariable<Unity.Collections.FixedString4096Bytes> _log =
        new NetworkVariable<Unity.Collections.FixedString4096Bytes>();

    private List<string> _clientLog = new();
    

    public void addClientScore(ulong clientId, string playerName, int amount)
    {
        Debug.Log("addClientScore");
        if (_clientScoreTable.TryGetValue(key: clientId, out var value) == true)
        {
            value.Score += amount;
            _clientScoreTable[clientId] = value;
        }
        else
        {
            _clientScoreTable.Add(clientId, 
                new ClientInfo(){ClientId = clientId,PlayerName = playerName, Score = amount}
            );
        }
        
        var scoreInfo = "";
        foreach ( var (_, clientInfo)  in _clientScoreTable.OrderByDescending( c => c.Value.Score ) )
        {
            scoreInfo += string.Format("{0} : {1}ポイント\n", clientInfo.PlayerName, clientInfo.Score);
        }
        _scoreInfo.Value = scoreInfo;
    }

    public void addClientLog(string log)
    {
        _clientLog.Add(log);
        while (_clientLog.Count > 20)
        {
            _clientLog.RemoveAt(0);
        }

        string dispLog = "";
        foreach(string l in Enumerable.Reverse(_clientLog).ToList())
        {
            dispLog += l + "\n";
        }

        _log.Value = dispLog;
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 60, 300, 500));
        GUILayout.Label("Score\n" + _scoreInfo.Value.Value + "\nLog\n" + _log.Value.Value);
        GUILayout.EndArea();
    }
}
