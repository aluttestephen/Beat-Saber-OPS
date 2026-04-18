using UnityEngine;

public class SaberTrail : MonoBehaviour
{
    [Header("Trail settings")]
    public Color saberColor = Color.red;


    [Range(0.05f, 0.2f)]
    public float trailTime = 0.15f;

    [Range(0.01f, 0.2f)]
    public float trailWidth = 0.05f;

    private TrailRenderer _trail;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _trail = GetComponent<TrailRenderer>();
        _trail.time = trailTime;
        _trail.startWidth = trailWidth;
        _trail.endWidth = 0f;

        Gradient gradient = new Gradient();
        gradient.SetKeys(new GradientColorKey[]
        {
            new GradientColorKey(saberColor, 0f),new GradientColorKey(saberColor, 1f)
        }, new GradientAlphaKey[]
        {
            new GradientAlphaKey(0.8f, 0f), 
            new GradientAlphaKey(0f, 1f)
        });

        _trail.colorGradient = gradient;

        _trail.material = new Material(Shader.Find("Particles/Additive"));
        if (_trail.material.shader.name == "Hidden/InternalErrorShader")
        {
            _trail.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        }

        _trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _trail.receiveShadows = false;
    }

}
