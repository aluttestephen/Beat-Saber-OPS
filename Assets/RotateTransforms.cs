using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// RotateTransforms.cs
/// Attach to an empty GameObject called "RingManager".
/// Drag your hollow square ring GameObjects into the targetTransforms list.
/// They will rotate around the Z axis with an angular offset per ring,
/// creating the Beat Saber tunnel effect.
/// </summary>
public class RotateTransforms : MonoBehaviour
{
    [Header("Targets")]
    public List<Transform> targetTransforms;

    [Header("Rotation")]
    public float angularSpeed = 10f;
    public float offset = 15f;          // degrees between each ring's starting rotation
    public float offsetSpeed = 0.5f;    // how fast the offset drifts over time

    private List<float> _rotationOffsets = new List<float>();

    void Awake()
    {
        GenerateRotationOffsets();
        ApplyInitialRotationOffsets();
    }

    void Update()
    {
        RotateAll();
        UpdateGlobalOffset();
        UpdateOffsetList();
    }

    public void UpdateOffsetSpeed(float newSpeed)
    {
        offsetSpeed = newSpeed;
    }

    private void GenerateRotationOffsets()
    {
        _rotationOffsets.Clear();
        for (int i = 0; i < targetTransforms.Count; i++)
            _rotationOffsets.Add(offset * (i + 1));
    }

    private void ApplyInitialRotationOffsets()
    {
        for (int i = 0; i < targetTransforms.Count; i++)
            targetTransforms[i].Rotate(0, 0, _rotationOffsets[i]);
    }

    private void RotateAll()
    {
        float angle = angularSpeed * Time.deltaTime;
        for (int i = 0; i < targetTransforms.Count; i++)
            targetTransforms[i].Rotate(0, 0, angle);
    }

    private void UpdateGlobalOffset()
    {
        offset += offsetSpeed * Time.deltaTime;
    }

    private void UpdateOffsetList()
    {
        for (int i = 0; i < targetTransforms.Count; i++)
            _rotationOffsets[i] = offset * (i + 1);
    }
}