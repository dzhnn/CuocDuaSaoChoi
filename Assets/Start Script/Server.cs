using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System;
using UnityEngine;
using System.IO;

public class Server : MonoBehaviour
{
    private int port = 5000;
    private List<ServerClient> clients;
    private List<ServerClient> disconnectList;

    private TcpListener server;
    private bool serverStarted;

    public void Init(IPAddress hostIP)
    {
        DontDestroyOnLoad(gameObject);
        clients = new List<ServerClient>();
        disconnectList = new List<ServerClient>();
        try
        {
            server = new TcpListener(hostIP, port);
            server.Start();

            StartListening();
            serverStarted = true;
        }
        catch (Exception e)
        {
            Debug.Log("Error: " + e.Message);
        }

    }
    private void Update()
    {
        if (!serverStarted)
        {
            return;
        }
        foreach (ServerClient client in clients)
        {
            if (!IsConnected(client.tcp))
            {
                client.tcp.Close();
                disconnectList.Add(client);
                continue;
            }
            else
            {
                NetworkStream stream = client.tcp.GetStream();
                if (stream.DataAvailable)
                {
                    StreamReader reader = new StreamReader(stream, true);
                    string data = reader.ReadLine();

                    if (data != null)
                    {
                        OnIncomingData(client, data);
                    }
                }
            }
        }
        for (int i = 0; i < disconnectList.Count - 1; i++)
        {
            clients.Remove(disconnectList[i]);
            disconnectList.RemoveAt(i);
        }

    }
    
    private void StartListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
    }

    private void AcceptTcpClient(IAsyncResult result)
    {
        TcpListener listener = (TcpListener)result.AsyncState;

        string allUsers = "";
        foreach (ServerClient sclient in clients)
        {
            allUsers += sclient.clientName + '|';
        }
        ServerClient sc = new ServerClient(listener.EndAcceptTcpClient(result));
        clients.Add(sc);

        StartListening();

        Broadcast("SWHO|" + allUsers, clients[clients.Count-1]);
    }

    private bool IsConnected(TcpClient c)
    {
        try
        {
            if (c != null && c.Client != null && c.Client.Connected)
            {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                {
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        catch
        {
            return false;
        }
    }

    private void Broadcast(string data, ServerClient client)
    {
        List<ServerClient> list = new List<ServerClient> { client };
        BroadcastS(data, list);
    }

    private void BroadcastS(string data, List<ServerClient> clients)
    {
        foreach (ServerClient client in clients)
        {
            try
            {
                StreamWriter writer = new StreamWriter(client.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.Log("Write error: " + e.Message);
            }
        }

    }
    
    private void OnIncomingData(ServerClient c, string data)
    {
        Debug.Log(c.clientName + " sent Server:" + data);
        string[] aData = data.Split('|');

        GameClient gc;

        switch (aData[0])
        {
                //LOG IN
            case "CWHO":
                c.clientName = aData[1];
                BroadcastS("SCNN|" + c.clientName, clients);
                break;

            case "HStart":
                BroadcastS("CStart|", clients);
                break;

            case "CENDTURN":
                gc = Client.Instance.GetGameClientByName(c.clientName);
                if (gc.myTurn == false)
                {
                    Broadcast("Not your turn|", c);
                }
                else BroadcastS("SENDTURN|", clients);
                break;

                //XÚC XẮC VÀ DI CHUYỂN
            case "CROLL":
                gc = Client.Instance.GetGameClientByName(c.clientName);
                if (gc.myTurn == false)
                {
                    Broadcast("Not your turn|", c);
                }
                else if (gc.banned > 0)
                {
                    gc.rolled = true;
                }
                else if (gc.rolled)
                {
                    Broadcast("You rolled !!|", c);
                }
                else Broadcast("SRandomDice|" + c.clientName, c);
                break;

            case "CRolled":
                
                gc = Client.Instance.GetGameClientByName(c.clientName);
                if (gc.banned > 0)
                {
                    Debug.Log("Đang bị cấm di chuyển");
                }
                else if (gc.atSchool )// thêm biến đếm số lần max roll???
                {
                    BroadcastS("SDice|" + aData[1], clients);
                    if (aData[1] == "5" || aData[1] == "6")
                    {
                        Tile graduateTile = GameManager.Instance.allTiles.Find(t => t.name == "Graduate");
                        BroadcastS("SGraduate|" + graduateTile.name + "|" + c.clientName, clients);
                        gc.atSchool = false;       
                        return;
                    }
                }
                else
                {
                    BroadcastS("SFindDes|" + aData[1], clients);
                }
                
                break;

            case "CReroll":
                gc = Client.Instance.GetGameClientByName(c.clientName);
                gc.rolled = false;
                break;

            case "CAddPoint": //nhận tên area  //gửi tín hiệu và tên area
                gc = Client.Instance.GetGameClientByName(c.clientName);
                if (gc.myTurn)  //trong lượt thì mới được cộng điểm
                Broadcast("SUpdateScore|++|" + aData[1], c);
                
                break;

            case "CNotiScore": //1 là điểm, 2 là tên gc, 3 là tên area
                BroadcastS("SNotiScore|" + aData[1] + "|" + aData[2] + "|" + aData[3], clients);
                break;

            case "CMinusPoint":
                gc = Client.Instance.GetGameClientByName(c.clientName);
                if (gc.myTurn)
                    Broadcast("SUpdateScore|--", c);
                break;



            //SPECIAL TILE

            case "CTileClicked":
                gc = Client.Instance.GetGameClientByName(c.clientName);
                if (gc.myTurn == false)
                {
                    Broadcast("Not your turn|", c);
                }
                else BroadcastS("SStartMove|" + aData[1] + "|" + c.clientName, clients); //sau khi ng chơi click thì cập nhật di chuyển cho tất cả ng chơi xem
                break;

            case "CSpecialTile":
                BroadcastS("SSpecialTile|" + aData[1] + "|" + c.clientName, clients);
                break;

            case "CToSchool":
                BroadcastS("SToSchool|" + aData[1] + "|" + c.clientName, clients);
                break;

            case "CBanned":
                gc = Client.Instance.GetGameClientByName(c.clientName);
                gc.banned = 2;
                Broadcast("Youre banned"+gc.banned.ToString(), c);
                break;
            
            case "CMeteor":
                gc = Client.Instance.GetGameClientByName(c.clientName);
                if (!gc.myTurn) break; //chỉ gửi 1 lần
                GameManager mng = GameManager.Instance;
                Area randomArea = mng.GetRandomArea();
                BroadcastS("SMeteor|" + randomArea.name, clients);
                break;

            case "CMoveBus":
                BroadcastS("SMoveBus|" + aData[1] + "|" + c.clientName, clients);
                break;



            //ĐIỀU HƯỚNG UI card
            case "CShowPanel":
                Broadcast("SShowPanel|", c);
                break;

            case "CClosePanel":
                Broadcast("SClosePanel|", c);
                break;

            case "CENDGAME":
                BroadcastS("SSort|", clients);
                break;

            case "CLoaded":
                Broadcast("SRank|", c);
                break;

            
            //CARD
            case "CDrawCard":
                Broadcast("SDraw1Card|" + c.clientName, c);
                break;

            case "CCardDrawed":
                BroadcastS("SDeck--|" + aData[1], clients);
                break;

            case "CDeckEmpty":
                BroadcastS("SDeckEmpty|", clients);
                break;

            case "CCardClicked":
                Broadcast("SShowCardOption|" + aData[1] + "|" + c.clientName, c);
                break;

            case "CUse":
                Broadcast("SUse|" + aData[1], c);
                break;

            case "CAddToUsed":
                BroadcastS("SAddToUsed|" + aData[1], clients);
                break;


                //CHAT VÀ ĐỔI SKIN
            case "CMSG":
                BroadcastS("SMSG|" + c.clientName + ": " + aData[1], clients);
                break;

            case "CSkinChange":
                Broadcast("SClosePanel|", c);
                string playerName = aData[1];
                int skinIndex = int.Parse(aData[2]);
         
                BroadcastS("SSkinChange|" + playerName + "|" + skinIndex, clients);


                Broadcast("SDraw1Card|" + c.clientName, c);
                Broadcast("SDraw1Card|" + c.clientName, c);

                break;
        }
    }

}

public class ServerClient
{
    public string clientName;
    public TcpClient tcp;
    public bool isHost;
    public ServerClient(TcpClient tcp)
    {
        this.tcp = tcp;
    }
}
