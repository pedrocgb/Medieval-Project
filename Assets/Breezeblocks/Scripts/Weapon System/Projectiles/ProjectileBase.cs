using UnityEngine;
using ObjectPool;
using Sirenix.OdinInspector;

public class ProjectileBase : MonoBehaviour, IPooledObjects
{
    #region Variables and Properties
    private Rigidbody2D myRigidbody;
    private Collider2D myCollider;

    private float damage = 0f;
    private float velocity = 5f;
    private float initialVelocity = 5f; 
    private float maxDistance = 3f;
    private float distanceTravelled = 0f;
    private Vector3 lastPosition;
    private RangedStats bow;

    [FoldoutGroup("Settings", expanded: true)]
    [SerializeField] private LayerMask _hitLayer;
    [FoldoutGroup("Settings")]
    [SerializeField, Range(0f, 1f)]
    private float _minStrengthFactorBeforeFall = 0.15f;

    [FoldoutGroup("Stick Settings", expanded: true)]
    [SerializeField] private bool _stickOnHit = true;

    [FoldoutGroup("Stick Settings")]
    [SerializeField] private bool _parentToHit = true;

    [FoldoutGroup("Stick Settings")]
    [SerializeField] private float _stuckLifetime = 5f; // seconds before despawn after sticking

    private bool _isStuck = false;
    private float _stuckTimer = 0f;
    #endregion

    // ======================================================================

    #region  Initialization
    void Awake()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
    }

    public void Initialize(float Velocity, float MaxDistance, float SpreadAngle, float Damage, RangedStats Bow)
    {
        initialVelocity = Velocity;
        velocity = Velocity;
        maxDistance = MaxDistance;
        damage = Damage;
        bow = Bow;

        // Apply spread on Z rotation
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, transform.eulerAngles.z + SpreadAngle));
    }

    public void OnObjectSpawn()
    {
        distanceTravelled = 0f;
        lastPosition = transform.position;
        _isStuck = false;
        _stuckTimer = 0f;

        // Re-enable physics & collider when reused from pool
        if (myRigidbody != null)
            myRigidbody.simulated = true;

        if (myCollider != null)
            myCollider.enabled = true;

        // Clear parent (optional – so it doesn't stay attached to old targets)
        transform.SetParent(null);
    }
    #endregion

    // ======================================================================

    #region  Loop
    void Update()
    {
        if (_isStuck)
        {
            if (_stuckLifetime > 0)
            {
                // Count down while stuck, then despawn
                _stuckTimer += Time.deltaTime;
                if (_stuckTimer >= _stuckLifetime)
                {
                    gameObject.SetActive(false);
                }
            }
            return;
        }

        CalculateDistance();
    }

    void FixedUpdate()
    {
        if (_isStuck)
            return;

        MoveProjectile();
    }
    #endregion

    // ======================================================================

    #region Move Methods
    protected void CalculateDistance()
    {
        distanceTravelled += Vector2.Distance(transform.position, lastPosition);
        lastPosition = transform.position;

        if (distanceTravelled >= maxDistance)
        {
            LandOnGround();
        }
    }

    protected void MoveProjectile()
    {
       if (myRigidbody == null)
            return;

        // 1) Compute how much strength is left based on distance travelled
        float t = Mathf.Clamp01(distanceTravelled / maxDistance);
        float strengthFactor = 1f - t; // 1 → 0 as distance increases

        // 2) If arrow has basically no strength left, just land it
        if (strengthFactor <= _minStrengthFactorBeforeFall)
        {
            LandOnGround();
            return;
        }

        // 3) Set current velocity based on remaining strength
        velocity = initialVelocity * strengthFactor;

        Vector2 currentPos = myRigidbody.position;
        Vector2 dir        = (Vector2)transform.right; // forward direction
        Vector2 step       = dir * (velocity * Time.fixedDeltaTime);
        float   distance   = step.magnitude;

        // Safety: no movement, no raycast
        if (distance <= 0f)
            return;

        Vector2 nextPos = currentPos + step;

        // 4) Raycast to detect hits between current & next position
        RaycastHit2D hit = Physics2D.Raycast(currentPos, dir, distance, _hitLayer);
        if (hit.collider != null)
        {
            HandleHit(hit);
            return;
        }

        // 5) No hit, move normally
        myRigidbody.MovePosition(nextPos);
    }
    #endregion

    // ======================================================================

    #region Hit Methods
    private void HandleHit(RaycastHit2D hit)
    {
        // Damage
        IDamageable target = hit.collider.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(damage, DamageType.Piercing, bow.GetRandomLimb());
        }

        if (_stickOnHit)
        {
            StickToHit(hit);
        }
        else
        {
            // Just despawn if we don't want sticking
            gameObject.SetActive(false);
        }
    }

    private void StickToHit(RaycastHit2D hit)
    {
        _isStuck = true;
        _stuckTimer = 0f;

        // Place the arrow exactly at the hit point
        transform.position = hit.point;

        // Rotate arrow so its "right" points along the impact direction (into the surface)
        // hit.normal points OUT of the surface, so we flip it.
        //Vector2 impactDir = -hit.normal;
        //float angle = Mathf.Atan2(impactDir.y, impactDir.x) * Mathf.Rad2Deg;
        //transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Stop physics & collisions
        if (myRigidbody != null)
            myRigidbody.simulated = false;

        if (myCollider != null)
            myCollider.enabled = false;

        // Parent to the hit object so it moves with it (enemy walking, etc.)
        if (_parentToHit && hit.collider != null)
        {
            transform.SetParent(hit.collider.transform, true);
        }
    }
    
     private void LandOnGround()
    {
        if (_isStuck)
            return;

        _isStuck    = true;
        _stuckTimer = 0f;

        // Arrow "falls" and stays where it is.
        // You can optionally rotate it to lie flat, or slightly tilt it.
        // For now we keep its last rotation.

        if (myRigidbody != null)
            myRigidbody.simulated = false;

        // Keep collider enabled if you want arrows on the ground to block / be hittable.
        // If not, you can disable it:
        // if (myCollider != null)
        //     myCollider.enabled = false;
    }
    #endregion

    // ======================================================================
}
