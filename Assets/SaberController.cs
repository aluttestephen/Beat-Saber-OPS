using UnityEngine;

public class SaberController : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("1 = re player, 2 = blue player")]
    public int playerID = 1;

    [Header("Serial Data Source")]
    [Tooltip("Drag the GameObject that has SerialReceiver.cs on it")]
    public SerialReceiver serialReceiver;

    [Header("Position (MediaPipe wrist)")]
    [Tooltip ("Use Mediapipe wrist landmark to move saber in place")]
    public bool useHandPosition = true; 

    [Tooltip("How fast saber position follow wrist 0=instant, 1=never")]
    [Range(0f, 0.95f)]
    public float handPosLerp = 0.15f;

    [Tooltip("Scale factor: converts normalized MediaPipe coords (0-1) to real life units")]
    public Vector3 handPosScale = new Vector3(4f,2f,0f);

    [Tooltip("Offset so tha saber sits in front of the player and not at the origin")]
    public Vector3 handPosOffset = new Vector3(-2f,0.5f,1.5f);

    [Header("Rotation (Arduino IMU)")]
    [Tooltip("Smoothing for IMU rotation")]
    [Range (0f, 0.5f)]
    public float rotationSlerp = 0.1f;

    [Tooltip("Flip yaw if IMU is mounted upside-down, i.e. insures correct saber orientation")]
    public bool invertYaw = false;

    [Tooltip("Flip pitch if Saber tilts wrong way")]
    public bool invertPitch = false; 

    [Header("Appearance")]
    public Color saberColor = Color.red;

    private Quaternion _targetRotation = Quaternion.identity; //Quaternion (x,y,z,w) ensures smooth rotation
    private Vector3 _targetPosition;
    private Renderer _renderer;

    void Start(){
        _targetPosition = transform.position;

        _renderer = GetComponentInChildren<Renderer>();
        if(_renderer != null)
        {
            _renderer.material.color = saberColor;
            if (_renderer.material.HasProperty("_EmissionColor"))
            {
                _renderer.material.EnableKeyword("_EMISION");
                _renderer.material.SetColor("_EmissionColor", saberColor *2f);
            }
        }

        if (serialReceiver == null)
        {
            serialReceiver =  FindAnyObjectByType<SerialReceiver>();
        }

        if (serialReceiver == null)
        {
            Debug.LogWarning($"[SaberController p{playerID}] No SerialReceiver found. Saber will not move.");
        }
        }

    void Update()
    {
        if (serialReceiver == null) return;

        UpdateRotationFromIMU();

        if (useHandPosition)
        {
            UpdatePositionFromHandLandmarks();
        }

        //Smoothens Rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, 1f-rotationSlerp);

        //Smoothens Translation
        transform.position = Vector3.Lerp(transform.position, _targetPosition, 1f - handPosLerp);
    }

    void UpdateRotationFromIMU()
    {
        float pitch = serialReceiver.GetPitch(playerID);
        float yaw = serialReceiver.GetYaw(playerID);
        float roll = serialReceiver.GetRoll(playerID);

        if (invertPitch) pitch = -pitch;
        if (invertYaw) yaw = -yaw;

        _targetRotation = Quaternion.Euler(pitch, yaw, roll);
    }

    void UpdatePositionFromHandLandmarks()
    {
        Vector3 wrist = serialReceiver.GetWristPosition(playerID);

        if (wrist == Vector3.zero)
        {
            return;
        } 

        float worldX = (wrist.x - 0.5f) * handPosScale.x + handPosOffset.x;
        float worldY = (0.5f - wrist.y) * handPosScale.y + handPosOffset.y;
        float worldZ = handPosOffset.z;

        _targetPosition = new Vector3(worldX, worldY, worldZ);
    }

    public Vector3 GetTipPosition() {
        return transform.position + transform.up * 0.5f;
    }

    public Vector3 GetSwingDirection(){
    return transform.up;
    }

    public bool MatchesBlockType(int blockType){
    if (playerID == 1) return blockType == 0;  //red
    if (playerID == 2) return blockType == 1; //blue

    return false;

    }

    public bool IsSwinging()
    {
        float roll  = serialReceiver.GetRoll(playerID);
        float pitch = serialReceiver.GetPitch(playerID);
        float yaw   = serialReceiver.GetYaw(playerID);

        float mag = Mathf.Sqrt(roll * roll + pitch * pitch + yaw * yaw);
        return mag > 2.5f;
    }
}   

