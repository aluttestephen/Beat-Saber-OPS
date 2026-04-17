using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

public class SerialReceiver : MonoBehaviour
{
    private float[] _pitch = {0f, 0f, 0f};
    private float[] _yaw = {0f, 0f, 0f};
    private float[] _roll = {0f, 0f, 0f};

    private Vector3[] _wrist = {Vector3.zero, Vector3.zero, Vector3.zero};

    public string arduinoPort = "COM7";
    public int baudRate = 115200;

    private SerialPort arduinoSerial;
    private Thread arduinoThread;

    private Thread udpThread;
    private UdpClient udpClient;

    public int udpPort = 5005;

    public static PlayerData[] players = new PlayerData[2]
    {
        new PlayerData(),
        new PlayerData()
    };

    void ParseLine (string line)
    {
        if (line.StartsWith("A:"))
        {
            ParseIMU(line.Substring(2), 1);
        }
        else if (line.StartsWith("B:"))
        {
            ParseIMU(line.Substring(2), 2);
        }
        else if (line.StartsWith("H:"))
        {
            ParseHandLandmarks(line.Substring(2));
        }
    }
    void ParseIMU(string data, int player)
    {
        string[] parts = data.Split(',');
        if (parts.Length < 3) return;
        if (float.TryParse(parts[0], out float p)) _pitch[player] = p; 
        if (float.TryParse(parts[1], out float y)) _yaw[player] = y; 
        if (float.TryParse(parts[2], out float r)) _roll[player] = r; 
    }
    
    void ParseHandLandmarks (string data)
    {
        int colon = data.IndexOf(':');
        if (colon < 0) return;
        if (!int.TryParse(data.Substring(0,colon), out int player)) return;
        string[] parts = data.Substring(colon + 1).Split(',');
        if (parts.Length < 3) return; 
        if (float.TryParse(parts[0], out float x) && float.TryParse(parts[1], out float y) && float.TryParse(parts[2], out float z))
        {
            _wrist[player] = new Vector3(x,y,z);
        }
    }
    void Start()
    {
        arduinoSerial = new SerialPort(arduinoPort, baudRate);
        arduinoSerial.Open();
        arduinoThread = new Thread(ReadArduino);
        arduinoThread.Start();
        Debug.Log("Serial opened on " + arduinoPort);

        udpClient = new UdpClient(udpPort);
        udpThread = new Thread(ReadUDP);
        udpThread.Start();
        Debug.Log("UDP Listening on port" + udpPort);
    }

    void ReadArduino()
    {
        while (arduinoSerial.IsOpen)
        {
            try
            {
                string line = arduinoSerial.ReadLine();
                ParseLine(line);
            }
            catch { }
        }
    }

    void ReadUDP()
    {
        IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, udpPort);
            while (true)
            {
                try
                {
                    byte[] data = udpClient.Receive(ref endpoint);
                    string jsonString = Encoding.UTF8.GetString(data).Trim();
                    
                    // Deserialize the JSON
                    HandData hand = JsonConvert.DeserializeObject<HandData>(jsonString);
                    
                    if (hand != null)
                    {
                        // Assign to your wrist array (using id as index)
                        int id = Mathf.Clamp(hand.id, 0, 2);
                        _wrist[id] = new Vector3(hand.x, hand.y, 0); 
                        
                        // You can also store velocity or swing state here
                        players[id-1].handX = hand.x;
                        players[id-1].handY = hand.y;
                    }
                }
                catch (System.Exception e) {
                    Debug.LogWarning("UDP Error: " + e.Message);
                }
            }
    }

    public float GetPitch(int player) => _pitch[Mathf.Clamp(player,1,2)];
    public float GetYaw(int player) => _yaw[Mathf.Clamp(player,1,2)];
    public float GetRoll(int player) => _roll[Mathf.Clamp(player,1,2)];

    public Vector3 GetWristPosition(int player) => _wrist[Mathf.Clamp(player,1,2)];
    void OnDestroy()
    {
        arduinoThread?.Abort();
        arduinoSerial?.Close();
        udpThread?.Abort();
        udpClient?.Close();
    }


}

[System.Serializable]
public class HandData
{
    public int id;
    public float x;
    public float y;
    public float vel;
    public float angle;
    public float dir_x;
    public float dir_y;
    public bool swing;    
}

[System.Serializable]
public class ArduinoPacket
{
    public int id;
    public float roll, pitch, yaw;
}

public class PlayerData
{
    public float roll, pitch, yaw;
    public float handX, handY;
}