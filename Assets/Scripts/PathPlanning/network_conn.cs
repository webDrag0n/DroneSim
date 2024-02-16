using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Linq;

public class network_conn : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();
    string script_path;
    public Socket server;

    public GameObject sphere_prefab; // 球的预制体
    public GameObject cube_prefab; // 立方体的预制体
    public GameObject path_point_prefab; // path prefab

    bool flag = true;
    bool pathFlag = false;
    bool pathReady = false;
    int command_count = 0;

    public float[][] dict; // 等到point指令发出后会收到target point和start point的坐标
    public float[][][] path; // 最后生成的路径，第一维是总线段数，后两维表示一个线段的两个端点的坐标

    private void Awake()
    {
        Kill_All_Python_Process();
        Thread thread1 = new Thread(InitServer);
        thread1.Start();
        script_path = Application.dataPath + "/Scripts/PathPlanning/rl_agent.py";
        Thread thread2 = new Thread(StartPythonScript);
        thread2.Start();
    }

    private void Start()
    {
        StartCoroutine("sendCommands"); // hell
    }

    IEnumerator sendCommands()
    {
        while (true)
        {
            if (flag)
            {
                flag = false;
                Vector3 position = transform.position;
                string message = $"{position.x},{position.y},{position.z}";
                if(command_count == 1)
                {
                    message = "block";
                }else if(command_count == 2)
                {
                    GenerateCube();
                    message = "ball";
                }else if(command_count == 3)
                {
                    GenerateSphere();
                    message = "obb";
                }
                else if (command_count == 4)
                {
                    GenerateObb();
                    message = "point";
                }
                else if (command_count == 5)
                {
                    GeneratePoint();
                    message = "path";
                }
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                foreach (var client in clients.Keys)
                {
                    client.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, sendCallBack, client);
                }
            }

            if (pathReady)
            {
                pathReady = false;
                UnityEngine.Debug.Log(path.Length);
                for (int i = 0; i < path.Length; i++)
                {
                    //UnityEngine.Debug.Log(path[i][0][0]);
                    if (i == 0)
                    {
                        Vector3 start_pos = new Vector3();
                        start_pos.x = path[0][0][0];
                        start_pos.y = path[0][0][2];
                        start_pos.z = path[0][0][1];
                        UnityEngine.Debug.Log(start_pos);
                        GameObject start_object = Instantiate(path_point_prefab, start_pos, Quaternion.identity);
                    }

                    Vector3 next_pos = new Vector3();
                    next_pos.x = path[i][1][0];
                    next_pos.y = path[i][1][2];
                    next_pos.z = path[i][1][1];

                    UnityEngine.Debug.Log(next_pos);
                    GameObject path_object = Instantiate(path_point_prefab, next_pos, Quaternion.identity);
                }
            }

            yield return new WaitForSeconds(0.1f); // 控制发送频率，避免发送过快
        }
    }

    public void sendCallBack(IAsyncResult ar)
    {
        try
        {
            Socket handler = (Socket)ar.AsyncState;
            int bytesSent = handler.EndSend(ar);
        }
        catch (Exception e)
        {
            print(e);
        }
    }

    void OnApplicationQuit()
    {
        // 在应用程序退出前执行一些代码
        UnityEngine.Debug.Log("应用程序即将退出，清理所有Python进程");
        // 结束所有Python进程
        Kill_All_Python_Process();
        if (server != null)
        {
            server.Close(); // 关闭服务器
        }
    }

    public void InitServer()
    {
        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        EndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12578);
        server.Bind(point);
        server.Listen(10);
        print("等待客户端连接");
        server.BeginAccept(AcceptCallBack, server);
    }

    public void AcceptCallBack(IAsyncResult ar)
    {

        try
        {
            print("客户端接入");
            Socket server = ar.AsyncState as Socket;
            Socket client = server.EndAccept(ar);
            ClientState clientState = new ClientState();
            clientState.socket = client;
            //将连接进来的客户端保存起来
            clients.Add(client, clientState);
            //接收此客户端发来的信息
            client.BeginReceive(clientState.data, 0, 1024, 0, ReceiveCallBack, clientState);
            //继续监听新的客户端接入
            server.BeginAccept(AcceptCallBack, server);
        }
        catch (SocketException e)
        {
            print(e);
        }
    }

    public void ReceiveCallBack(IAsyncResult ar)
    {
        try
        {
            ClientState state = ar.AsyncState as ClientState;
            Socket client = state.socket;
            int count = client.EndReceive(ar);
            if (count == 0)
            {
                client.Close();
                clients.Remove(client);
                print("客户端关闭");
                return;
            }

            flag = true;
            string recv_string = Encoding.UTF8.GetString(state.data, 0, count);
            UnityEngine.Debug.Log("Received sum: " + recv_string);
            if (recv_string.Equals("OK"))
            {
                pathFlag = false;
                pathReady = true;
            }
            if (pathFlag)
            {
                flag = false;
                StringToList(recv_string);

            }
            if (!recv_string.Equals("start") && command_count < 6)
            {
                if(command_count == 5)
                {
                    path = new float[int.Parse(recv_string)][][];
                    pathFlag = true;
                }
                else
                {
                    StringToList(recv_string);

                }
            }
            command_count++;
            client.BeginReceive(state.data, 0, 1024, 0, ReceiveCallBack, state);
        }
        catch (SocketException e)
        {
            print(e);
        }
    }

    public void StringToList(string input)
    {
        input = input.Trim('[', ']'); // 去除开头和结尾的方括号
        string[] subArrays = input.Split(new[] { "], [" }, StringSplitOptions.None); // 分割子数组

        float[][] result = new float[subArrays.Length][];
        for (int i = 0; i < subArrays.Length; i++)
        {
            string[] elements = subArrays[i].Split(new[] { ", " }, StringSplitOptions.None); // 分割元素
            result[i] = elements.Select(float.Parse).ToArray(); // 转换为float类型并存入结果数组
        }
        UnityEngine.Debug.Log(result);
        if(command_count < 6)
        {
            dict = result;
        }
        else
        {
            path[command_count - 6] = result;
        }
        //UnityEngine.Debug.Log($"command:{command_count}");
    }

    public void GenerateSphere()
    {
        foreach (float[] data in dict)
        {
            float x = data[0];
            float y = data[1];
            float z = data[2];
            float radius = data[3];

            Vector3 position = new Vector3(x, z, y);
            GameObject sphere = Instantiate(sphere_prefab, position, Quaternion.identity);
            sphere.transform.localScale = new Vector3(radius * 2, radius * 2, radius * 2);
        }
    }

    public void GeneratePoint()
    {
        foreach (float[] data in dict)
        {
            float x = data[0];
            float y = data[1];
            float z = data[2];
            float radius = 0.1f;

            Vector3 position = new Vector3(x, z, y);
            GameObject sphere = Instantiate(sphere_prefab, position, Quaternion.identity);
            sphere.transform.localScale = new Vector3(radius * 2, radius * 2, radius * 2);
        }
    }

    public void GenerateObb()
    {
        foreach (float[] data in dict)
        {
            float x = data[0];
            float y = data[1];
            float z = data[2];
            float halfWidth = data[3];
            float halfHeight = data[4];
            float halfDepth = data[5];
            float rotX = data[6] - 90f;
            float rotY = data[7];
            float rotZ = data[8];

            Vector3 position = new Vector3(x, z, y);
            Quaternion rotation = Quaternion.Euler(rotY, rotX, rotZ);
            Vector3 scale = new Vector3(halfWidth * 2, halfHeight * 2, halfDepth * 2);

            GameObject cube = Instantiate(cube_prefab, position, rotation);
            //GameObject cube = Instantiate(cube_prefab, position, Quaternion.identity);
            cube.transform.localScale = scale;
        }
    }

    public void GenerateCube()
    {
        foreach (float[] data in dict)
        {
            float minX = data[0];
            float minY = data[1];
            float minZ = data[2];
            float maxX = data[3];
            float maxY = data[4];
            float maxZ = data[5];

            //Vector3 position = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2);
            //Vector3 scale = new Vector3(maxX - minX, maxY - minY, maxZ - minZ);
            Vector3 position = new Vector3((minX + maxX) / 2, (minZ + maxZ) / 2, (minY + maxY) / 2);
            Vector3 scale = new Vector3(maxX - minX, maxZ - minZ, maxY - minY);
            GameObject cube = Instantiate(cube_prefab, position, Quaternion.identity);
            cube.transform.localScale = scale;
        }
    }

    void Update()
    {
        // 获取键盘输入
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // 根据输入移动方块
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput) * moveSpeed * Time.deltaTime;
        transform.Translate(movement);
    }

    public void StartPythonScript()
    {
        Process process = new Process();
        process.StartInfo.FileName = "python";
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.Arguments = script_path;
        print(process.StartInfo.Arguments);
        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                UnityEngine.Debug.Log($"Python Output: {e.Data}");
            }
        };
        process.Start();
        process.BeginOutputReadLine(); // 异步读取输出
        process.WaitForExit();
        UnityEngine.Debug.Log("Python 脚本执行完成");
    }

    void Kill_All_Python_Process()
    {
        foreach (Process process in Process.GetProcessesByName("python"))
        {
            try
            {
                process.Kill();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log($"无法终止进程：{ex.Message}");
            }
        }
    }
}
