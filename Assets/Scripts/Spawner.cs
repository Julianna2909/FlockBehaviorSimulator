using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private Boid boidPrefab;
    [SerializeField] private int spawnCount;
    [SerializeField] private float spawnRadius;

    public Boid[] Spawn()
    {
        var boids = new Boid[spawnCount];
        for (var i = 0; i < boids.Length; i++)
        {
            var pos = transform.position + Random.insideUnitSphere * spawnRadius;
            var boid = Instantiate(boidPrefab);
            boid.transform.position = pos;
            boid.transform.forward = Random.insideUnitSphere;
            boids[i] = boid;
        }

        return boids;
    }
}