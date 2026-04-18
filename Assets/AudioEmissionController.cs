using UnityEngine;
public class AudioEmissionController : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Target")]
    public Renderer targetRenderer;

    [Header("Emission")]
    public Color emissiveColor = Color.white;
    public string emissionPropertyName = "_EmissionColor";

    [Range(1, 500)]
    public float sensitivity = 150f;

    [Range(0f, 1f)]
    public float smoothing = 0.15f;

    public float threshold = 0.001f;
    public float defaultIntensity = 0.5f;

    private Material _material;
    private int _emissionPropertyID;
    private float _currentIntensity;

    void Awake()
    {
        _material = targetRenderer.material;
        _emissionPropertyID = Shader.PropertyToID(emissionPropertyName);
        _material.EnableKeyword("_EMISSION");
    }

    void Update()
    {
        if (audioSource == null || !audioSource.isPlaying) return;

        float[] spectrumData = new float[1024];
        audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

        float audioAverage = 0f;
        foreach (float sample in spectrumData)
            audioAverage += sample;
        audioAverage /= spectrumData.Length;

        float targetIntensity = audioAverage > threshold
            ? audioAverage * sensitivity
            : defaultIntensity;

        _currentIntensity = Mathf.Lerp(_currentIntensity, targetIntensity, smoothing);
        _material.SetColor(_emissionPropertyID, emissiveColor * _currentIntensity);
    }

    void OnDestroy()
    {
        if (_material != null)
            Destroy(_material);
    }
}