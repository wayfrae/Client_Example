using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public class Client : MonoBehaviour
{
    private const int MAX_USER = 10;
    private const int PORT = 26000;
    private const int WEB_PORT = 26001;
    private const string SERVER_IP = "127.0.0.1";
    private const int BYTE_SIZE = 1440;

    private byte reliableChannel, unreliableChannel;
    private int hostId;
    private byte error;
    private bool isStarted;
    int connectionId;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Init();
    }

#pragma warning disable CS0618 // Type or member is obsolete
    public void Init()
    {

        NetworkTransport.Init();

        ConnectionConfig cc = new ConnectionConfig();

        reliableChannel = cc.AddChannel(QosType.ReliableFragmented);
        cc.PacketSize = 1440;
        cc.FragmentSize = 900;
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology hostTopology = new HostTopology(cc, MAX_USER);

        hostId = NetworkTransport.AddHost(hostTopology, 0);

#if UNITY_WEBGL && !UNITY_EDITOR
        //web client
        connectionId = NetworkTransport.Connect(hostId, SERVER_IP, WEB_PORT, 0, out error);   
        Debug.Log("Connecting from web");
#else
        //standalone client
        connectionId = NetworkTransport.Connect(hostId, SERVER_IP, PORT, 0, out error);
        Debug.Log("Connecting from standalone");
#endif

        Debug.Log(string.Format("Attempting to connect on {0}...", SERVER_IP));
        isStarted = true;
    }

    private void Update()
    {
        UpdateMessagePump();
    }

    public void UpdateMessagePump()
    {
        if (!isStarted)
        {
            return;
        }

        int recHostId;
        int channelId;

        byte[] recBuffer = new byte[BYTE_SIZE];
        int dataSize;

        NetworkEventType type = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, BYTE_SIZE, out dataSize, out error);

        switch (type)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log("Successfully connected to server!");
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("You have been disconnected from the server.");
                break;
            case NetworkEventType.DataEvent:
                Debug.Log(string.Format("Data was received from client {0}.", connectionId));
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream memoryStream = new MemoryStream(recBuffer);
                GameMessage message = formatter.Deserialize(memoryStream) as Message_Hazards;
                OnData(connectionId, channelId, recHostId, message);
                break;
            case NetworkEventType.BroadcastEvent:
            default:
                Debug.Log("Unexpected Network Event Type");
                break;
        }
    }

    private void OnData(int connId, int channelId, int recHostId, GameMessage message)
    {
        switch (message.Code)
        {
            case OperationCode.None:
                break;
            case OperationCode.Move:
                break;
            case OperationCode.Shoot:
                break;
            case OperationCode.CreateAccount:
                break;
            case OperationCode.Spawn:
                break;
            case OperationCode.Hazards:
                DisplayHazards(connId, channelId, recHostId, (Message_Hazards)message);
                break;
        }
    }

    private void DisplayHazards(int connId, int channelId, int recHostId, Message_Hazards message)
    {
        var gameControllerObj = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        message.hazards = message.hazards.Replace("]", "");
        message.hazards = message.hazards.Replace("[", "");
        string[] splitString = Regex.Split(message.hazards, @"(?<=},)");
        List<Hazard> hazards = new List<Hazard>();
        foreach (string json in splitString)
        {
            var s = json.TrimEnd(',');
            hazards.Add(JsonUtility.FromJson<Hazard>(s));
        }        
        gameControllerObj.SpawnAsteroids(hazards);
    }

    public void SendMessageTest()
    {
        SendServer(new Message_Spawn());
    }

    #region Send
    public void SendServer(GameMessage message)
    {
        byte[] buffer = new byte[BYTE_SIZE];
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream memoryStream = new MemoryStream(buffer);
        formatter.Serialize(memoryStream, message);

        NetworkTransport.Send(hostId, connectionId, reliableChannel, buffer, BYTE_SIZE, out error);
    }
    #endregion

    public void TESTFUNCTIONCREATEACCOUNT()
    {
        Message_CreateAccount message = new Message_CreateAccount();
        message.Username = "Hello";
        message.Password = "World";
        message.Email = "email@email.com";

        SendServer(message);
    }

    public void Shutdown()
    {
        isStarted = false;
        NetworkTransport.Shutdown();
    }
}
