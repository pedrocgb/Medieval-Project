using UnityEngine;
using ObjectPool;

public class ProjectileBase : MonoBehaviour, IPooledObjects
{
    private Rigidbody2D myRigidbody;

    private float damage = 0f;
    private float velocity = 5f;
    private float maxDistance = 3f;
    private float distanceTravelled = 0f;
    private Vector3 lastPosition;

    void Awake()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
    }

    public void Initialize(float Velocity, float MaxDistance, float Damage, float SpreadAngle)
    {
        velocity = Velocity;
        maxDistance = MaxDistance;
        damage = Damage;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, transform.eulerAngles.z + SpreadAngle));
    }

    public void OnObjectSpawn()
    {
        distanceTravelled = 0f;
        lastPosition = transform.position;
    }



    // Update is called once per frame
    void Update()
    {
        CalculateDistance();
    }

    void FixedUpdate()
    {
        MoveProjectile();
    }

    protected void CalculateDistance()
    {
        distanceTravelled += Vector2.Distance(transform.position, lastPosition);
        lastPosition = transform.position;
        if (distanceTravelled >= maxDistance)
        {
            Debug.Log("Max distance reached, despawning projectile.");
            gameObject.SetActive(false);
        }
    }

    protected void MoveProjectile()
    {
        myRigidbody.MovePosition(myRigidbody.position + (Vector2)transform.right * (velocity * Time.fixedDeltaTime));
    }
}
