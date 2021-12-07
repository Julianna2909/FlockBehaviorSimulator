using UnityEngine;

public class Boid : MonoBehaviour
{
    private Settings settings;

    private Vector3 velocity;
    private Transform cachedTransform;

    public Vector3 FlockHeading { get; set; }
    public Vector3 FlockCenter { get; set; }
    public Vector3 AvoidanceHeading { get; set; }
    public int FlockSize { get; set; }

    public Vector3 Position
    {
        get => cachedTransform.position;
        set => cachedTransform.position = value;
    }

    public Vector3 Forward
    {
        get => cachedTransform.forward;
        set => cachedTransform.forward = value;
    }

    private void Awake() => cachedTransform = transform;

    public void Initialize(Settings settings)
    {
        this.settings = settings;
        var startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = Forward * startSpeed;
    }

    public void ApplyNewData()
    {
        var acceleration = AdjustForFlock() + AdjustForCollision();

        velocity += acceleration * Time.deltaTime;
        velocity = ClampMagnitude(velocity, settings.minSpeed, settings.maxSpeed);

        Position += velocity * Time.deltaTime;
        Forward = velocity.normalized;
    }

    private Vector3 ClampMagnitude(Vector3 vector, float min, float max)
    {
        var magnitude = vector.magnitude;
        var direction = vector / magnitude;
        magnitude = Mathf.Clamp(magnitude, min, max);
        vector = direction * magnitude;
        return vector;
    }

    private Vector3 AdjustForCollision()
    {
        if (!IsHeadingForCollision()) return Vector3.zero;

        var collisionAvoidDirection = GetCollisionAvoidDirection();
        var collisionAvoidForce = SteerTowards(collisionAvoidDirection) * settings.avoidCollisionWeight;
        return collisionAvoidForce;

    }

    private Vector3 AdjustForFlock()
    {
        if (FlockSize == 0) return Vector3.zero;

        var distanceToCenter = FlockCenter - Position;

        var alignmentForce = SteerTowards(FlockHeading) * settings.alignWeight;
        var cohesionForce = SteerTowards(distanceToCenter) * settings.cohesionWeight;
        var separationForce = SteerTowards(AvoidanceHeading) * settings.seperateWeight;

        return alignmentForce + cohesionForce + separationForce;

    }

    private Vector3 GetCollisionAvoidDirection()
    {
        Vector3[] rayDirections = settings.Directions;

        for (var i = 0; i < rayDirections.Length; i++)
        {
            var direction = cachedTransform.TransformDirection(rayDirections[i]);
            var ray = new Ray(Position, direction);
            if (!Physics.SphereCast(ray, settings.boundsRadius, settings.collisionAvoidDistance,
                settings.obstacleMask))
            {
                return direction;
            }
        }

        return Forward;
    }

    private bool IsHeadingForCollision()
    {
        return Physics.SphereCast(Position, settings.boundsRadius, Forward, out _,
            settings.collisionAvoidDistance, settings.obstacleMask);
    }

    private Vector3 SteerTowards(Vector3 vector)
    {
        var steerVector = vector.normalized * settings.maxSpeed - velocity;
        return Vector3.ClampMagnitude(steerVector, settings.maxSteerForce);
    }
}