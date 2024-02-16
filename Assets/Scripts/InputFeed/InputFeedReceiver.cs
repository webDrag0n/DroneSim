using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class InputFeedReceiver : MonoBehaviour
{
    public int port = 12346; // The same port as used in the Python script
    public GameObject controlledObject;
    
    private VTOLController controller;
    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;

    private void Start()
    {
        udpClient = new UdpClient(port);
        remoteEndPoint = new IPEndPoint(IPAddress.Any, port);

        controller = controlledObject.GetComponent<VTOLController>();
    }

    private void Update()
    {
        try
        {
            byte[] data = udpClient.Receive(ref remoteEndPoint);
            string key = Encoding.ASCII.GetString(data);

            // Process the received key input
            switch (key)
            {
                case "W":
                    // Handle 'W' key press
                    controller.MoveForwardBack(1);
                    break;
                case "A":
                    // Handle 'A' key press
                    controller.MoveLeftRight(-1);
                    break;
                case "S":
                    // Handle 'S' key press
                    controller.MoveForwardBack(-1);
                    break;
                case "D":
                    // Handle 'D' key press
                    controller.MoveLeftRight(1);
                    break;
                case "0":
                    // Handle no keys pressed
                    // Add code for stopping the movement here
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }
}
