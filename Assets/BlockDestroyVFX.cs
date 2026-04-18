using UnityEngine;

public class BlockDestroyVFX : MonoBehaviour
{
    public static BlockDestroyVFX Instance;

    [Header("particle settings")]
    [Range(10,60)]
    public int particleCount = 24;
    public float burstSpeed = 4f;
    public float particleSize = 0.08f;
    public float lifetime = 0.5f;

    void Awake()
    {
        Instance = this;
    }

    public void PlayHitEffect(Vector3 worldPosition, Color color)
    {
        
        GameObject vfxGO = new GameObject("HitVFX");
        vfxGO.transform.position = worldPosition;

        ParticleSystem ps = vfxGO.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.duration = 0.1f;
        main.loop = false; 
        main.startLifetime = lifetime;
        main.startSpeed = burstSpeed;
        main.startSize = particleSize; 
        main.startColor = color;
        main.gravityModifier = 0.3f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.enabled = true;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, particleCount)
        });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;

        shape.radius = 0.1f;

        var renderer = vfxGO.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Additive"));
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        var sizeOverLife = ps.sizeOverLifetime;
        sizeOverLife.enabled = true;
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut (0f,1f,1f, 0f));

        ps.Play();

        Destroy(vfxGO, lifetime + 0.2f);


    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
