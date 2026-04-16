using UnityEngine;
using System.IO.Ports;
using System.Threading;
using Newtonsoft.Json;

public class SerialReceiver : MonoBehaviour
{
    public string arduinoPort = "COM7";
    public int baudRate = 115200;

    private SerialPort arduinoSerial;
    private Thread arduinoThread;

    public static PlayerData[] players = new PlayerData[2]
    {
        new PlayerData(),
        new PlayerData()
    };

    void Start()
    {
        arduinoSerial = new SerialPort(arduinoPort, baudRate);
        arduinoSerial.Open();
        arduinoThread = new Thread(ReadArduino);
        arduinoThread.Start();
        Debug.Log("Serial opened on " + arduinoPort);
    }

    void ReadArduino()
    {
        while (arduinoSerial.IsOpen)
        {
            try
            {
                string line = arduinoSerial.ReadLine();
                var data = JsonConvert.DeserializeObject<ArduinoPacket>(line);
                if (data != null && data.id >= 1 && data.id <= 2)
                {
                    int i = data.id - 1;
                    players[i].roll     = data.roll;
                    players[i].pitch    = data.pitch;
                    players[i].yaw      = data.yaw;
                    players[i].swingMag = data.swingMag;
                    Debug.Log($"P{data.id} roll:{data.roll} pitch:{data.pitch}");
                }
            }
            catch { }
        }
    }

    void OnDestroy()
    {
        arduinoThread?.Abort();
        arduinoSerial?.Close();
    }
}

[System.Serializable]
public class ArduinoPacket
{
    public int id;
    public float roll, pitch, yaw, swingMag;
}

public class PlayerData
{
    public float roll, pitch, yaw, swingMag;
    public float handX, handY;
    public bool swinging;
}