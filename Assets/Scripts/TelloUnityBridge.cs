using UnityEngine;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TelloUnityBridge : MonoBehaviour
{
    [Header("UI Elements")]
    public RawImage videoDisplay;

    [Header("Network Settings")]
    public string pythonIP = "127.0.0.1";
    public int pythonPort = 5005;
    public int videoPort = 5006;

    // Sockets i Threads
    private UdpClient commandClient;
    private UdpClient videoReceiver;
    private Thread videoThread;
    
    // Gestió del Frame de Vídeo
    private byte[] latestFrameBytes;
    private bool frameReceived = false;
    private Texture2D videoTexture;
    private object lockObject = new object();

    void Start()
    {
        // Inicialitzem el client per enviar ordres
        commandClient = new UdpClient();

        // Inicialitzem la textura (mida coincident amb el Python, 320x240)
        videoTexture = new Texture2D(320, 240, TextureFormat.RGB24, false);
        videoDisplay.texture = videoTexture;

        // Obrim el receptor de vídeo en un fil independent de Unity
        videoReceiver = new UdpClient(videoPort);
        videoThread = new Thread(new ThreadStart(ReceiveVideo));
        videoThread.IsBackground = true;
        videoThread.Start();
    }

    void Update()
    {
        // 1. Control per teclat (Exemple bàsic)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.tKey.wasPressedThisFrame) SendCommand("takeoff");
            if (Keyboard.current.lKey.wasPressedThisFrame) SendCommand("land");
        }

        // WASD per moure's en pla, fletxes amunt/avall per pujar o baixar,
        // i fletxes esquerra/dreta per girar.
        int fb = 0;
        int lr = 0;
        int ud = 0;
        int yaw = 0;

        if (Keyboard.current != null)
        {
            fb += Keyboard.current.wKey.isPressed ? 40 : 0;
            fb -= Keyboard.current.sKey.isPressed ? 40 : 0;
            lr += Keyboard.current.dKey.isPressed ? 40 : 0;
            lr -= Keyboard.current.aKey.isPressed ? 40 : 0;
            ud += Keyboard.current.upArrowKey.isPressed ? 40 : 0;
            ud -= Keyboard.current.downArrowKey.isPressed ? 40 : 0;
            yaw += Keyboard.current.rightArrowKey.isPressed ? 40 : 0;
            yaw -= Keyboard.current.leftArrowKey.isPressed ? 40 : 0;
        }

        SendCommand($"rc {lr} {fb} {ud} {yaw}");

        // 2. Renderitzar el vídeo a la Main Thread de Unity
        if (frameReceived)
        {
            lock (lockObject)
            {
                videoTexture.LoadImage(latestFrameBytes);
                videoTexture.Apply();
                frameReceived = false;
            }
        }
    }

    private void ReceiveVideo()
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, videoPort);
        while (true)
        {
            try
            {
                byte[] receivedData = videoReceiver.Receive(ref remoteEndPoint);
                lock (lockObject)
                {
                    latestFrameBytes = receivedData;
                    frameReceived = true;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error rebent vídeo: " + e.Message);
            }
        }
    }

    public void SendCommand(string command)
    {
        try
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(command);
            commandClient.Send(data, data.Length, pythonIP, pythonPort);
        }
        catch (Exception e)
        {
            Debug.LogError("Error enviant ordre: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        // Tanquem sockets al sortir
        if (commandClient != null) commandClient.Close();
        if (videoReceiver != null) videoReceiver.Close();
        if (videoThread != null && videoThread.IsAlive) videoThread.Abort();
    }
}