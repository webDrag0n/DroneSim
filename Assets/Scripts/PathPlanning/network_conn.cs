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

    public GameObject sphere_prefab; // ���Ԥ����
    public GameObject cube_prefab; // �������Ԥ����
    public GameObject path_point_prefab; // path prefab

    bool flag = true;
    bool pathFlag = false;
    bool pathReady = false;
    int command_count = 0;

    public float[][] dict; // �ȵ�pointָ�������յ�target point��start point������
    public float[][][] path; // ������ɵ�·������һά�����߶���������ά��ʾһ���߶ε������˵������

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

            yield return new WaitForSeconds(0.1f); // ���Ʒ���Ƶ�ʣ����ⷢ�͹���
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
        // ��Ӧ�ó����˳�ǰִ��һЩ����
        UnityEngine.Debug.Log("Ӧ�ó��򼴽��˳�����������Python����");
        // ��������Python����
        Kill_All_Python_Process();
        if (server != null)
        {
            server.Close(); // �رշ�����
        }
    }

    public void InitServer()
    {
        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        EndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12578);
        server.Bind(point);
        server.Listen(10);
        print("�ȴ��ͻ�������");
        server.BeginAccept(AcceptCallBack, server);
    }

    public void AcceptCallBack(IAsyncResult ar)
    {

        try
        {
            print("�ͻ��˽���");
            Socket server = ar.AsyncState as Socket;
            Socket client = server.EndAccept(ar);
            ClientState clientState = new ClientState();
            clientState.socket = client;
            //�����ӽ����Ŀͻ��˱�������
            clients.Add(client, clientState);
            //���մ˿ͻ��˷�������Ϣ
            client.BeginReceive(clientState.data, 0, 1024, 0, ReceiveCallBack, clientState);
            //���������µĿͻ��˽���
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
                print("�ͻ��˹ر�");
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
        input = input.Trim('[', ']'); // ȥ����ͷ�ͽ�β�ķ�����
        string[] subArrays = input.Split(new[] { "], [" }, StringSplitOptions.None); // �ָ�������

        float[][] result = new float[subArrays.Length][];
        for (int i = 0; i < subArrays.Length; i++)
        {
            string[] elements = subArrays[i].Split(new[] { ", " }, StringSplitOptions.None); // �ָ�Ԫ��
            result[i] = elements.Select(float.Parse).ToArray(); // ת��Ϊfloat���Ͳ�����������
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
        // ��ȡ��������
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // ���������ƶ�����
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
        process.BeginOutputReadLine(); // �첽��ȡ���
        process.WaitForExit();
        UnityEngine.Debug.Log("Python �ű�ִ�����");
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
                UnityEngine.Debug.Log($"�޷���ֹ���̣�{ex.Message}");
            }
        }
    }
}
