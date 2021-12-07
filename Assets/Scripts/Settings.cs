using UnityEngine;

[CreateAssetMenu]
public class Settings : ScriptableObject
{
    [Header("Boid Behavior")] public float minSpeed = 2;
    public float maxSpeed = 5;
    public float perceptionRadius = 2.5f;
    public float avoidanceRadius = 1;
    public float maxSteerForce = 3;

    public float alignWeight = 1;
    public float cohesionWeight = 1;
    public float seperateWeight = 1;

    [Header("Collisions")] public LayerMask obstacleMask;
    public float boundsRadius = .27f;
    public float avoidCollisionWeight = 10;
    public float collisionAvoidDistance = 5;

    [Header("Directions")] public int viewDirectionCount = 300;
    public ComputeShader compute;

    public const int threadGroupSize = 512;

    public Vector3[] Directions { get; private set; }

    public void GenerateDirections()
    {
        Directions = new Vector3[viewDirectionCount];

        var goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        var angleIncrement = Mathf.PI * 2 * goldenRatio;

        var buffer = new ComputeBuffer(viewDirectionCount, 3 * sizeof(float));
        buffer.SetData(Directions);
        compute.SetBuffer(0, "directions", buffer);
        compute.SetFloat("goldenRatio", goldenRatio);
        compute.SetFloat("angleIncrement", angleIncrement);
        compute.SetInt("directionsCount", viewDirectionCount);

        var groupCount = Mathf.CeilToInt(viewDirectionCount / (float) threadGroupSize);
        compute.Dispatch(0, groupCount, 1, 1);

        buffer.GetData(Directions);
        buffer.Release();
    }
}