using UnityEngine;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine.UI;

public class TelloUnityBridgeMetaQuest : MonoBehaviour
{
    [Header("UI Elements")]
    public RawImage videoDisplay;

    [Header("Network Settings")]
    public string pythonIP = "127.0.0.1";
    public int pythonPort = 5005;
    public int videoPort = 5006;

    [Header("Controller Settings")]
    [Range(1f, 100f)]
    public float stickSpeed = 40f;
    [Range(0f, 0.5f)]
    public float deadzone = 0.2f;

    private UdpClient commandClient;
    private UdpClient videoReceiver;
    private Thread videoThread;

    private byte[] latestFrameBytes;
    private bool frameReceived = false;
    private Texture2D videoTexture;
    private readonly object lockObject = new object();

    void Start()
    {
        commandClient = new UdpClient();

        videoTexture = new Texture2D(320, 240, TextureFormat.RGB24, false);
        if (videoDisplay != null)
        {
            videoDisplay.texture = videoTexture;
        }

        videoReceiver = new UdpClient(videoPort);
        videoThread = new Thread(new ThreadStart(ReceiveVideo));
        videoThread.IsBackground = true;
        videoThread.Start();
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            SendCommand("takeoff");
        }

        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
        {
            SendCommand("land");
        }

        Vector2 leftStick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch);
        Vector2 rightStick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick, OVRInput.Controller.RTouch);

        int fb = ApplyDeadzone(leftStick.y);
        int lr = ApplyDeadzone(leftStick.x);
        int ud = ApplyDeadzone(rightStick.y);
        int yaw = ApplyDeadzone(rightStick.x);

        if (fb == 0 && lr == 0 && ud == 0 && yaw == 0)
        {
            SendCommand("rc 0 0 0 0");
        }
        else
        {
            SendCommand($"rc {lr} {fb} {ud} {yaw}");
        }

        if (frameReceived && videoTexture != null)
        {
            lock (lockObject)
            {
                videoTexture.LoadImage(latestFrameBytes);
                videoTexture.Apply();
                frameReceived = false;
            }
        }
    }

    private int ApplyDeadzone(float axisValue)
    {
        if (Mathf.Abs(axisValue) < deadzone)
        {
            return 0;
        }

        return Mathf.RoundToInt(axisValue * stickSpeed);
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
        if (commandClient != null) commandClient.Close();
        if (videoReceiver != null) videoReceiver.Close();
        if (videoThread != null && videoThread.IsAlive) videoThread.Abort();
    }
}