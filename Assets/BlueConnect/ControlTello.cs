using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System;

public class ControlTello : MonoBehaviour
{
    public Socket server, client;
    EndPoint localaddr, telloaddr, epSender;
    Thread thread;
    Thread thread1;
    WaitForSeconds time0 = new WaitForSeconds(0f);
    WaitForSeconds time1 = new WaitForSeconds(0f);
    byte[] result = new byte[1024];
    byte[] receiveDatas = new byte[1024];
    bool isOK = true;

    // Start is called before the first frame update
    void Start()
    {
        print("\r\n\r\nTello Unity Demo.\r\n");
        print("Tello: command takeoff land flip forward back left right \r\n       up down cw ccw speed speed?\r\n");
        print("end -- quit demo.\r\n");
        server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        localaddr = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 9000);
        telloaddr = new IPEndPoint(IPAddress.Parse("192.168.10.1"), 8889);
        IPEndPoint temp = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 8890);
        epSender = (EndPoint)temp;
        server.Bind(localaddr);
        client.Bind(temp);
        //server.Listen(10);
        print("等待客户端连接");
        StartCoroutine("sendCommands");
        //thread = new Thread(receiveData);
        //thread.Start();
        server.BeginReceiveFrom(result, 0, result.Length, SocketFlags.None, ref telloaddr, new AsyncCallback(receiveData), telloaddr);
        //thread1 = new Thread(getStatus);
        //thread1.Start();

        //client.BeginReceiveFrom(receiveDatas, 0, receiveDatas.Length, SocketFlags.None, ref epSender, new AsyncCallback(getStatus), epSender);
    }

    // Update is called once per frame
    IEnumerator sendCommands()
    {
        while (true)
        {
            bool flag = false;
            if (isOK)
            {
                if (Input.GetKeyDown(KeyCode.C))
                {
                    sendData("command");
                    //server.SendTo(Encoding.UTF8.GetBytes("command"), telloaddr);
                    print("command");
                    flag = true;
                }
                else if (Input.GetKeyDown(KeyCode.Z))
                {
                    print("end");
                    thread.Abort();

                }
                else if (Input.GetKeyDown(KeyCode.J))
                {
                    sendData("takeoff");
                    //server.SendTo(Encoding.UTF8.GetBytes("takeoff"), telloaddr);
                    print("takeoff");
                    flag = true;
                }
                else if (Input.GetKeyDown(KeyCode.K))
                {
                    sendData("land");
                    //server.SendTo(Encoding.UTF8.GetBytes("land"), telloaddr);
                    print("land");
                    flag = true;
                }
                else if (Input.GetKeyDown(KeyCode.W))
                {
                    server.SendTo(Encoding.UTF8.GetBytes("forward 50"), telloaddr);
                    print("forward");
                    flag = true;
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    server.SendTo(Encoding.UTF8.GetBytes("back 50"), telloaddr);
                    print("back");
                    flag = true;
                }
                else if (Input.GetKeyDown(KeyCode.A))
                {
                    server.SendTo(Encoding.UTF8.GetBytes("left 50"), telloaddr);
                    print("left");
                    flag = true;
                }
                else if (Input.GetKeyDown(KeyCode.D))
                {
                    server.SendTo(Encoding.UTF8.GetBytes("right 50"), telloaddr);
                    print("right");
                    flag = true;
                }
                else if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    server.SendTo(Encoding.UTF8.GetBytes("up 50"), telloaddr);
                    print("up");
                    flag = true;
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    server.SendTo(Encoding.UTF8.GetBytes("down 50"), telloaddr);
                    print("down");
                    flag = true;
                }
                else if (Input.GetKeyDown(KeyCode.Space))
                {
                    sendData("emergency");
                    //server.SendTo(Encoding.UTF8.GetBytes("emergency"), telloaddr);
                    print("emergency");
                    flag = true;
                }
                else if (Input.GetKeyDown(KeyCode.Q))
                {
                    server.SendTo(Encoding.UTF8.GetBytes("ccw 90"), telloaddr);
                    print("ccw");
                    flag = true;
                }
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    server.SendTo(Encoding.UTF8.GetBytes("cw 90"), telloaddr);
                    print("cw");
                    flag = true;
                }
            }
            if (flag)
            {
                isOK = false;
                yield return time0;
            }
            else
            {
                yield return time1;
            }
        }
    }

    public void sendData(string text)
    {
        byte[] sendData = Encoding.UTF8.GetBytes(text);
        server.BeginSendTo(sendData, 0, sendData.Length, SocketFlags.None, telloaddr, new AsyncCallback(sendCallBack), server);
    }

    public void sendCallBack(IAsyncResult ar)
    {
        try
        {
            Socket handler = (Socket)ar.AsyncState;
            int bytesSent = handler.EndSend(ar);
        }catch(Exception e)
        {
            print(e);
        }
    }

    public void receiveData(IAsyncResult iar)
    {
        int count = server.EndReceiveFrom(iar, ref telloaddr);
        if(count > 0)
        {
            isOK = true;
            string str = Encoding.UTF8.GetString(result, 0, count);
            print(str);
        }
        server.BeginReceiveFrom(result, 0, result.Length, SocketFlags.None, ref telloaddr, new AsyncCallback(receiveData), telloaddr);
    }
    public void getStatus(IAsyncResult iar)
    {
        int recv = client.EndReceiveFrom(iar, ref epSender);
        string str = Encoding.UTF8.GetString(receiveDatas, 0, recv);
        print(str);
        client.BeginReceiveFrom(receiveDatas, 0, receiveDatas.Length, SocketFlags.None, ref epSender, new AsyncCallback(getStatus), epSender);
    }
}
