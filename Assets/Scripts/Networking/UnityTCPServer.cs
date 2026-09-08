using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public class UnityTCPServer : MonoBehaviour
{
    [Header("Network")]
    [SerializeField] private int port = 5005;

    [Header("Camera Frame")]
    [SerializeField] private int frameWidth = 640;
    [SerializeField] private int frameHeight = 480;

    [Header("Frame Streaming")]
    [SerializeField] private bool streamFrames = true;
    [SerializeField] private float streamRateHz = 30f;

    private TcpListener server;
    private TcpClient client;
    private NetworkStream stream;

    private Thread serverThread;
    private Thread receiveThread;
    private Thread sendThread;

    private volatile bool running;
    private volatile bool clientConnected;

    private readonly object commandLock = new object();

    private float pendingPanDelta;
    private float pendingTiltDelta;
    private bool commandAvailable;

    private readonly object frameLock = new object();

    private byte[] latestFrame;
    private bool newFrameAvailable;

    private uint frameId;

    private CameraFrameCapture frameCapture;

    private void Start()
    {
        frameCapture =
            FindFirstObjectByType<CameraFrameCapture>();

        if (frameCapture == null)
        {
            Debug.LogError(
                "[TCP] CameraFrameCapture not found."
            );
        }

        StartServer();
    }

    private void Update()
    {
        ApplyPendingCommand();
    }

    private void StartServer()
    {
        try
        {
            server = new TcpListener(
                IPAddress.Any,
                port
            );

            server.Start();

            running = true;

            serverThread = new Thread(
                WaitForClient
            );

            serverThread.IsBackground = true;
            serverThread.Start();

            Debug.Log(
                $"[TCP] Unity server started on port {port}"
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"[TCP] Failed to start server: {e.Message}"
            );
        }
    }

    private void WaitForClient()
    {
        try
        {
            while (running)
            {
                Debug.Log(
                    "[TCP] Waiting for Python client..."
                );

                TcpClient newClient =
                    server.AcceptTcpClient();

                if (!running)
                    break;

                client = newClient;
                client.NoDelay = true;

                stream = client.GetStream();

                clientConnected = true;

                Debug.Log(
                    "[TCP] Python client connected!"
                );

                receiveThread = new Thread(
                    ReceiveCommands
                );

                receiveThread.IsBackground = true;
                receiveThread.Start();

                sendThread = new Thread(
                    SendFrames
                );

                sendThread.IsBackground = true;
                sendThread.Start();

                break;
            }
        }
        catch (Exception e)
        {
            if (running)
            {
                Debug.LogError(
                    $"[TCP] Server error: {e.Message}"
                );
            }
        }
    }

    private void ReceiveCommands()
    {
        byte[] buffer = new byte[8];

        try
        {
            while (running && clientConnected)
            {
                if (!ReceiveExact(buffer, 8))
                    break;

                float panDelta =
                    ReadBigEndianFloat(buffer, 0);

                float tiltDelta =
                    ReadBigEndianFloat(buffer, 4);

                lock (commandLock)
                {
                    pendingPanDelta = panDelta;
                    pendingTiltDelta = tiltDelta;
                    commandAvailable = true;
                }

                Debug.Log(
                    $"[TCP] Command: " +
                    $"Pan={panDelta:F4}°, " +
                    $"Tilt={tiltDelta:F4}°"
                );
            }
        }
        catch (Exception e)
        {
            if (running)
            {
                Debug.LogWarning(
                    $"[TCP] Receive error: {e.Message}"
                );
            }
        }

        clientConnected = false;
    }

    private void ApplyPendingCommand()
    {
        float pan;
        float tilt;

        lock (commandLock)
        {
            if (!commandAvailable)
                return;

            pan = pendingPanDelta;
            tilt = pendingTiltDelta;

            commandAvailable = false;
        }

        VirtualPanTiltCamera camera =
            FindFirstObjectByType<VirtualPanTiltCamera>();

        if (camera == null)
        {
            Debug.LogWarning(
                "[TCP] VirtualPanTiltCamera not found."
            );

            return;
        }

        camera.ApplyPanTiltDelta(
            pan,
            tilt
        );
    }

    public void SubmitFrame(byte[] rgbFrame)
    {
        if (!clientConnected)
            return;

        if (rgbFrame == null)
            return;

        int expectedSize =
            frameWidth *
            frameHeight *
            3;

        if (rgbFrame.Length != expectedSize)
        {
            Debug.LogWarning(
                $"[TCP] Invalid frame size: " +
                $"{rgbFrame.Length}, expected {expectedSize}"
            );

            return;
        }

        lock (frameLock)
        {
            latestFrame = rgbFrame;
            newFrameAvailable = true;
        }
    }

    private void SendFrames()
    {
        double nextFrameTime =
            Stopwatch.GetTimestamp() /
            (double)Stopwatch.Frequency;

        double interval =
            1.0 / streamRateHz;

        while (running && clientConnected)
        {
            if (!streamFrames)
            {
                Thread.Sleep(10);
                continue;
            }

            byte[] frame = null;

            lock (frameLock)
            {
                if (newFrameAvailable)
                {
                    frame = latestFrame;
                    newFrameAvailable = false;
                }
            }

            if (frame != null)
            {
                try
                {
                    SendFramePacket(frame);

                    frameId++;
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogWarning(
                        $"[TCP] Frame send error: {e.Message}"
                    );

                    clientConnected = false;
                    break;
                }
            }
            else
            {
                Thread.Sleep(1);
            }

            double now =
                Stopwatch.GetTimestamp() /
                (double)Stopwatch.Frequency;

            if (now < nextFrameTime)
            {
                int sleepMilliseconds =
                    Math.Max(
                        1,
                        (int)Math.Round(
                            (nextFrameTime - now) * 1000.0
                        )
                    );

                Thread.Sleep(sleepMilliseconds);
            }

            nextFrameTime += interval;
        }
    }

    private void SendFramePacket(byte[] rgbFrame)
    {
        byte[] packet =
            new byte[12 + rgbFrame.Length];

        packet[0] = (byte)(frameId >> 24);
        packet[1] = (byte)(frameId >> 16);
        packet[2] = (byte)(frameId >> 8);
        packet[3] = (byte)frameId;

        double timestamp =
            Stopwatch.GetTimestamp() /
            (double)Stopwatch.Frequency;

        byte[] timestampBytes =
            BitConverter.GetBytes(timestamp);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(timestampBytes);
        }

        Buffer.BlockCopy(
            timestampBytes,
            0,
            packet,
            4,
            8
        );

        Buffer.BlockCopy(
            rgbFrame,
            0,
            packet,
            12,
            rgbFrame.Length
        );

        stream.Write(
            packet,
            0,
            packet.Length
        );
    }

    private bool ReceiveExact(
        byte[] buffer,
        int size
    )
    {
        int received = 0;

        while (received < size && running)
        {
            int count = stream.Read(
                buffer,
                received,
                size - received
            );

            if (count <= 0)
                return false;

            received += count;
        }

        return received == size;
    }

    private float ReadBigEndianFloat(
        byte[] data,
        int offset
    )
    {
        byte[] temp = new byte[4];

        temp[0] = data[offset + 3];
        temp[1] = data[offset + 2];
        temp[2] = data[offset + 1];
        temp[3] = data[offset];

        return BitConverter.ToSingle(
            temp,
            0
        );
    }

    public bool IsClientConnected()
    {
        return clientConnected;
    }

    public uint GetFrameId()
    {
        return frameId;
    }

    private void OnApplicationQuit()
    {
        StopServer();
    }

    private void OnDestroy()
    {
        StopServer();
    }

    private void StopServer()
    {
        running = false;
        clientConnected = false;

        try
        {
            stream?.Close();
            client?.Close();
            server?.Stop();
        }
        catch
        {
        }

        stream = null;
        client = null;
        server = null;
    }
}