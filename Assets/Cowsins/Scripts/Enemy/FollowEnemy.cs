using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace cowsins2D
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class FollowEnemy : MonoBehaviour
    {
        [Header("Damage")]
        [SerializeField, Tooltip("Damage dealt to the player when touched.")]
        float damage = 0.1f;

        [Header("Movement Speeds")]

        [
            SerializeField,
            Tooltip("If true, the enemy will always chase the player regardless of distance.")
        ]
        bool alwaysFollow = false;

        [SerializeField, HideIf("alwaysFollow"), Tooltip("Speed at which the enemy patrols between locations.")]
        float patrolSpeed = 2f;

        [SerializeField, Tooltip("Speed at which the enemy chases the player.")]
        float chaseSpeed = 4f;

        [Header("Movement Constraints")]
        [
            SerializeField,
            HideIf("ignoreEnviornment"),
            Tooltip("allows gravity to pull the enemy down.")
        ]
        bool affectedByGravity = true;

        [
            SerializeField,
            HideIf("affectedByGravity"),
            Tooltip(
                "the enemy permanently ignores environment collisions allowing it to move through walls."
            )
        ]
        bool ignoreEnviornment = false;

        [
            SerializeField,
            HideIf("ignoreEnviornment"),
            Tooltip("layers considered as environment/walls/floors.")
        ]
        LayerMask environmentLayer;

        [Header("Jump & Wall Detection Settings")]
        [
            SerializeField,
            ShowIf("affectedByGravity"),
            Tooltip("the enemy can jump when encountering walls.")
        ]
        bool canJump = true;

        [
            SerializeField,
            Tooltip("upward force applied when jumping over walls"),
            ShowIf("affectedByGravity")
        ]
        float jumpForce = 6f;

        [Header("Detection Ranges")]
        [
            SerializeField,
            Tooltip("Range within which the enemy starts following the player."),
            HideIf("alwaysFollow")
        ]
        float detectionRange = 5f;

        [
            SerializeField,
            HideIf("alwaysFollow"),
            Tooltip(
                "Range at which the enemy stops following the player and returns to patrolling."
            )
        ]
        float stopFollowingRange = 8f;


        [Header("Patrol Settings")]
        [
            SerializeField,
            Tooltip(
                "list of transform locations for the enemy to patrol between. (leave empty to stand still)"
            )
        ]
        List<Transform> locations = new List<Transform>();

        [SerializeField]
        bool debug;

        [
            SerializeField,
            Tooltip("distance ahead to cast the box for wall detection."),
            ShowIf("debug")
        ]
        float wallCheckDistance = 0.1f;

        [
            SerializeField,
            Tooltip("Size of the wall check box cast (Width and Height)."),
            ShowIf("debug")
        ]
        Vector2 wallCheckSize = new Vector2(0.2f, 0.8f);

        [
            SerializeField,
            Tooltip("Extra distance added beyond the collider's bottom to check for ground."),
            ShowIf("debug")
        ]
        float groundCheckOffset = 0.1f;

        [SerializeField, Tooltip("Radius for the ground check circle cast."), ShowIf("debug")]
        float groundCheckRadius = 0.2f;

        Rigidbody2D rb;
        Collider2D col;
        EnemyHealth enemyHealth;
        Transform playerTransform;
        int currentIndex = 0;
        [SerializeField,ReadOnly] bool isChasing = false;
        bool cachedIsGrounded = false;
        bool cachedIsWallHit = false;
        Vector2 cachedWallDirection = Vector2.right;
        float reachThreshold = 0.4f;
        public Transform CurrentTargetLocation
        {
            get
            {
                if (isChasing && playerTransform != null)
                    return playerTransform;
                return (locations != null && locations.Count > 0) ? locations[currentIndex] : null;
            }
        }

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            enemyHealth = GetComponent<EnemyHealth>(); // Reference EnemyHealth[cite: 2]
            FindPlayer();
            ApplyRigidbodyConstraints();
            ApplyCollisionIgnore();
        }

        #region Unity Methods

        void Update()
        {
            if (enemyHealth != null && enemyHealth.isDead)
                return; //return if dead

            HandleChasing();
            ApplyCollisionIgnore();
        }

        void FixedUpdate()
        {
            if (enemyHealth != null && enemyHealth.isDead)
                return; //ignore if dead

            float currentSpeed = patrolSpeed;
            Vector2 targetPos = Vector2.zero;
            bool hasTarget = false;

            if (isChasing)
            {
                //move towards the player here
                if (playerTransform != null)
                {
                    targetPos = playerTransform.position;
                    currentSpeed = chaseSpeed;
                    hasTarget = true;
                }
            }
            else
            {
                //stop moving or patrol here
                if (locations != null && locations.Count > 0)
                {
                    Transform targetLocation = locations[currentIndex];
                    targetPos = targetLocation.position;
                    currentSpeed = patrolSpeed;
                    hasTarget = true;

                    float distance = Vector2.Distance((Vector2)transform.position, targetPos);

                    if (distance <= reachThreshold)
                    {
                        currentIndex = (currentIndex + 1) % locations.Count;
                    }
                }
            }

            //early exit if we don't have any valid target locations or player to track
            if (!hasTarget)
            {
                if (affectedByGravity)
                {
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                }
                else
                {
                    rb.linearVelocity = Vector2.zero;
                }
                return;
            }

            //if gravity is disabled or we have triggered environment ignore while chasing, move freely across both X and Y axes
            if (!affectedByGravity && ignoreEnviornment)
            {
                Vector2 directionToTarget = ((Vector2)targetPos - (Vector2)transform.position).normalized;
                rb.linearVelocity = directionToTarget * currentSpeed;
                return;
            }

            //otherwise, use standard gravity-based horizontal movement + jumping behavior
            float direction = Mathf.Sign(targetPos.x - transform.position.x);
            cachedWallDirection = new Vector2(direction, 0f);
            float targetXVelocity = direction * currentSpeed;

            //if we enable jumping
            if (canJump && col != null)
            {
                //check gounded
                Vector2 groundCheckStart = new Vector2(col.bounds.center.x, col.bounds.min.y);
                cachedIsGrounded = Physics2D.CircleCast(
                    groundCheckStart,
                    groundCheckRadius,
                    Vector2.down,
                    groundCheckOffset,
                    environmentLayer
                );

                //check box casat for wall check ahead based on direction
                cachedIsWallHit = Physics2D.BoxCast(
                    col.bounds.center,
                    wallCheckSize,
                    0f,
                    cachedWallDirection,
                    wallCheckDistance,
                    environmentLayer
                );

                bool shouldJump = cachedIsGrounded && cachedIsWallHit && (isChasing || (locations != null && locations.Count > 0));

                //jump if we are up against a wall and grounded
                if (shouldJump)
                {
                    rb.linearVelocity = new Vector2(targetXVelocity, jumpForce);
                    return;
                }

                //stop pushing horizontally if stuck against a wall mid-air so gravity can take over
                if (cachedIsWallHit && !cachedIsGrounded)
                {
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                    return;
                }
            }
            else
            {
                cachedIsGrounded = false;
                cachedIsWallHit = false;
            }

            //set linear velocity
            rb.linearVelocity = new Vector2(targetXVelocity, rb.linearVelocity.y);
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            CheckPlayerTouch(collision.collider);
        }

        void OnTriggerStay2D(Collider2D collision)
        {
            CheckPlayerTouch(collision);
        }

        void OnDrawGizmosSelected()
        {
            // Smaller Detection Range (Yellow)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Larger Stop-Following / Leash Range (Red)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, stopFollowingRange);

            // Ground/Wall checks are only active when affectedByGravity, environment isn't ignored, and canJump are enabled
            if (affectedByGravity && canJump)
            {
                Collider2D targetCol = col != null ? col : GetComponent<Collider2D>();
                if (targetCol != null)
                {
                    // 1. Ground Check CircleCast Visualization (Green if colliding, Cyan if not)
                    Gizmos.color = cachedIsGrounded ? Color.green : Color.cyan;
                    Vector3 groundStart = new Vector3(
                        targetCol.bounds.center.x,
                        targetCol.bounds.min.y,
                        0f
                    );
                    Vector3 groundEnd = groundStart + Vector3.down * groundCheckOffset;

                    Gizmos.DrawWireSphere(groundStart, groundCheckRadius);
                    Gizmos.DrawWireSphere(groundEnd, groundCheckRadius);

                    Gizmos.DrawLine(
                        groundStart + Vector3.left * groundCheckRadius,
                        groundEnd + Vector3.left * groundCheckRadius
                    );
                    Gizmos.DrawLine(
                        groundStart + Vector3.right * groundCheckRadius,
                        groundEnd + Vector3.right * groundCheckRadius
                    );

                    // 2. Wall Check BoxCast Visualization (Green if hitting wall, Cyan if clear)
                    Gizmos.color = cachedIsWallHit ? Color.green : Color.cyan;
                    Vector2 wallDir = Application.isPlaying ? cachedWallDirection : Vector2.right;
                    Vector2 boxCenter =
                        (Vector2)targetCol.bounds.center + (wallDir * wallCheckDistance);

                    // Draw a wire cube representing the BoxCast area
                    Gizmos.DrawWireCube(boxCenter, wallCheckSize);
                }
            }
        }

        void OnDestroy()
        {
            // Reset layer collision ignore state on destroy to prevent persisting across game restarts
            if (ignoreEnviornment)
            {
                int enemyLayerIndex = gameObject.layer;
                for (int i = 0; i < 32; i++)
                {
                    if ((environmentLayer.value & (1 << i)) != 0)
                    {
                        Physics2D.IgnoreLayerCollision(enemyLayerIndex, i, false);
                    }
                }
            }

            if (transform.parent != null)
            {
                Destroy(transform.parent.gameObject);
            }
        }

        #endregion

        #region Private Methods
        void CheckPlayerTouch(Collider2D collision)
        {
            if (enemyHealth != null && enemyHealth.isDead)
                return; // Check if dead

            if (collision.TryGetComponent<IPlayerStats>(out var playerStats)) // Check player stats interface
            {
                playerStats.Damage(damage); // Damage player
            }
        }

        void ApplyCollisionIgnore()
        {
            if (col == null || !ignoreEnviornment)
                return;

            //once it starts chasing, disable environment collision permanently for this enemy
            int enemyLayerIndex = gameObject.layer;
            for (int i = 0; i < 32; i++)
            {
                if ((environmentLayer.value & (1 << i)) != 0)
                {
                    Physics2D.IgnoreLayerCollision(enemyLayerIndex, i, true);
                }
            }
        }

        void ApplyRigidbodyConstraints()
        {
            if (!affectedByGravity)
            {
                rb.gravityScale = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
            else
            {
                rb.gravityScale = 1f;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }

        void FindPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                foreach (var obj in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                {
                    if (obj is IPlayerStats)
                    {
                        playerTransform = obj.transform;
                        break;
                    }
                }
            }
        }

        void HandleChasing()
        {
            if (playerTransform != null)
            {
                float distanceToPlayer = Vector2.Distance(
                    transform.position,
                    playerTransform.position
                );

                if (alwaysFollow)
                {
                    isChasing = true;
                }
                else
                {
                    if (!isChasing && distanceToPlayer <= detectionRange)
                    {
                        isChasing = true;
                    }
                    else if (isChasing && distanceToPlayer > stopFollowingRange)
                    {
                        isChasing = false;
                    }
                }
            }
            else
            {
                isChasing = false;
            }
        }

        #endregion
    }
}
