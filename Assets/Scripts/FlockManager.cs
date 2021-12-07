using UnityEngine;

public class FlockManager : MonoBehaviour
{
    public struct BoidData
    {
        public Vector3 position;
        public Vector3 direction;

        public Vector3 flockHeading;
        public Vector3 flockCenter;
        public Vector3 separationHeading;
        public int flockSize;
        public static int Size => sizeof(float) * 3 * 5 + sizeof(int);
    }

    [SerializeField] private Spawner spawner;
    [SerializeField] private ComputeShader compute;
    [SerializeField] private Settings settings;

    private Boid[] boids;

    private void Start()
    {
        settings.GenerateDirections();
        boids = spawner.Spawn();
        for (var i = 0; i < boids.Length; i++)
        {
            boids[i].Initialize(settings);
        }
    }

    private void Update() => Simulate();

    private void Simulate()
    {
        var boidCount = boids.Length;
        if (boidCount == 0) return;

        var boidBufferData = CreateBufferData(boidCount);

        var boidBuffer = new ComputeBuffer(boidCount, BoidData.Size);
        boidBuffer.SetData(boidBufferData);

        SetComputeParameters(boidBuffer);
        RunCompute(boidCount);

        boidBuffer.GetData(boidBufferData);

        ApplyBoidsData(boidBufferData);

        boidBuffer.Release();
    }

    private void ApplyBoidsData(BoidData[] boidBufferData)
    {
        for (var i = 0; i < boids.Length; i++)
        {
            boids[i].FlockHeading = boidBufferData[i].flockHeading;
            boids[i].FlockCenter = boidBufferData[i].flockCenter;
            boids[i].AvoidanceHeading = boidBufferData[i].separationHeading;
            boids[i].FlockSize = boidBufferData[i].flockSize;

            boids[i].ApplyNewData();
        }
    }

    private void RunCompute(int boidCount)
    {
        var threadGroups = Mathf.CeilToInt(boidCount / (float) Settings.threadGroupSize);
        compute.Dispatch(0, threadGroups, 1, 1);
    }

    private void SetComputeParameters(ComputeBuffer boidBuffer)
    {
        compute.SetBuffer(0, "boids", boidBuffer);
        compute.SetInt("boidCount", boidBuffer.count);
        compute.SetFloat("perceptionRadius", settings.perceptionRadius);
        compute.SetFloat("avoidanceRadius", settings.avoidanceRadius);
    }

    private BoidData[] CreateBufferData(int boidCount)
    {
        var boidBufferData = new BoidData[boidCount];

        for (var i = 0; i < boids.Length; i++)
        {
            boidBufferData[i].position = boids[i].Position;
            boidBufferData[i].direction = boids[i].Forward;
        }

        return boidBufferData;
    }
}