using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class VideoFeedSender : MonoBehaviour
{
    public Camera sourceCamera;
    public string targetIP = "192.168.3.3"; // Replace with the target device's IP
    public int targetPort = 12345; // Specify the port to use

    private Texture2D texture;
    private UdpClient udpClient;
    private IPEndPoint endPoint;

    private void Start()
    {
        texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        udpClient = new UdpClient();
        endPoint = new IPEndPoint(IPAddress.Parse(targetIP), targetPort);
    }

    private void Update()
    {
        // Capture the camera's view
        RenderTexture.active = sourceCamera.targetTexture;
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        texture.Apply();

        // Convert texture to bytes (e.g., JPEG)
        byte[] imageBytes = texture.EncodeToJPG();

        // Send the image over UDP
        udpClient.Send(imageBytes, imageBytes.Length, endPoint);
    }

    private void OnDisable()
    {
        udpClient.Close();
    }
}
