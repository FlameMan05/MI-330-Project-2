using UnityEngine;

namespace cowsins2D
{
    public class SimpleEnemyVisual : MonoBehaviour
    {
        [
            SerializeField,
            Tooltip(
                "SpriteRenderer to flip. If left unassigned, it will look for one on this object or its children."
            )
        ]
        private SpriteRenderer spriteRenderer;

        [
            SerializeField,
            Tooltip("Check if your original sprite asset faces left by default instead of right.")
        ]
        private bool spriteFacesLeft = false;

        [
            SerializeField,
            Tooltip(
                "If true, scales the entire transform's X axis instead of using SpriteRenderer.flipX."
            )
        ]
        private bool useTransformScale = false;

        private SimpleEnemy simpleEnemy;

        private void Awake()
        {
            simpleEnemy = GetComponentInParent<SimpleEnemy>();
        }

        private void Start()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void Update()
        {
            if (simpleEnemy == null)
                return;

            HandleSpriteFlip();
        }

        private void HandleSpriteFlip()
        {
            Transform target = simpleEnemy.CurrentTargetLocation;
            if (target == null)
                return;

            // Determine direction based on where the enemy is heading relative to its current position
            float deltaX = target.position.x - transform.position.x;

            if (Mathf.Abs(deltaX) > 0.01f)
            {
                bool movingRight = deltaX > 0;
                ApplyFlip(movingRight);
            }
        }

        private void ApplyFlip(bool movingRight)
        {
            if (useTransformScale)
            {
                Vector3 scale = transform.localScale;
                float targetXScale = Mathf.Abs(scale.x);

                if (spriteFacesLeft)
                    scale.x = movingRight ? -targetXScale : targetXScale;
                else
                    scale.x = movingRight ? targetXScale : -targetXScale;

                transform.localScale = scale;
            }
            else if (spriteRenderer != null)
            {
                // If the sprite faces left by default, flip it when moving right. Otherwise, flip it when moving left.
                spriteRenderer.flipX = spriteFacesLeft ? movingRight : !movingRight;
            }
        }
    }
}
