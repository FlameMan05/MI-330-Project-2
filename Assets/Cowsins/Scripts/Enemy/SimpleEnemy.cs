using System.Collections.Generic;
using UnityEngine;

namespace cowsins2D
{
    public class SimpleEnemy : MonoBehaviour
    {
        [SerializeField, Tooltip("damage dealt to the player when touched")]
        float damage = 0.1f;

        [SerializeField, Tooltip("speed at which the enemy moves between locations")]
        float moveSpeed = 3f;

        [SerializeField, Tooltip("list of transforms for the enemy to patrol between.")]
        List<Transform> locations = new List<Transform>();

        float reachThreshold = 0.1f;

        EnemyHealth enemyHealth;
        int currentIndex = 0;

        public Transform CurrentTargetLocation =>
            (locations != null && locations.Count > 0) ? locations[currentIndex] : null;

        void Awake()
        {
            enemyHealth = GetComponent<EnemyHealth>();
        }

        void Update()
        {
            if (enemyHealth != null && enemyHealth.isDead)
                return;
            HandleMovement();
        }

        void HandleMovement()
        {
            if (locations == null || locations.Count == 0)
                return;

            Transform targetLocation = locations[currentIndex];
            transform.position = Vector2.MoveTowards(
                transform.position,
                targetLocation.position,
                moveSpeed * Time.deltaTime
            );

            //check if target location is reached to switch to the next one
            if (Vector2.Distance(transform.position, targetLocation.position) <= reachThreshold)
            {
                currentIndex = (currentIndex + 1) % locations.Count;
            }
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            CheckPlayerTouch(collision.collider);
        }

        void OnTriggerStay2D(Collider2D collision)
        {
            CheckPlayerTouch(collision);
        }

        void CheckPlayerTouch(Collider2D collision)
        {
            if (enemyHealth != null && enemyHealth.isDead)
                return;

            //check if the colliding object is the player using the engine's IPlayerStats
            if (collision.TryGetComponent<IPlayerStats>(out var playerStats))
            {
                playerStats.Damage(damage);
            }
        }

        void OnDestroy()
        {
            //also destroy the parent
            if (transform.parent.gameObject != null)
            {
                Destroy(transform.parent.gameObject);
            }
        }
    }
}
