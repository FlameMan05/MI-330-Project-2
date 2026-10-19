using UnityEngine;
using System.Collections;  

namespace cowsins2D
{
    public class SidewaysSpike : MonoBehaviour
    {
        [SerializeField] private float distance;
        [SerializeField, Tooltip("Time to move left")] private float leftTime = 3f; // Time to move up
        [SerializeField, Tooltip("Time to stay idle after moving left")] private float idleTimeLeft = 2f; // Time to stay idle after moving up
        [SerializeField, Tooltip("Time to move right")] private float rightTime = 0.3f; // Time to move down
        [SerializeField, Tooltip("Time to stay idle after moving down")] private float idleTimeRight = 1f; // Time to stay idle after moving down

        private Vector3 initialPosition;
        private Vector3 targetPosition;
        private bool isMovingLeft = true;

        void Start()
        {
            initialPosition = transform.position;
            targetPosition = initialPosition + Vector3.left * distance; // Move up one unit
            StartCoroutine(MoveObject());
        }

        private IEnumerator MoveObject()
        {
            // Perform movement operations
            // while (true) allows us to loop through this process.
            while (true)
            {
                float timer = 0f;
                // Define where to go and where it came from depending on the current state of the sideways spike
                Vector3 startPos = isMovingLeft ? initialPosition : targetPosition;
                Vector3 endPos = isMovingLeft ? targetPosition : initialPosition;
                float moveTime = isMovingLeft ? leftTime : rightTime;
                float idleTime = isMovingLeft ? idleTimeLeft : idleTimeRight;

                // Lerp the movement from the startPos, and the endPos.
                while (timer < moveTime)
                {
                    timer += Time.deltaTime;
                    float t = Mathf.Clamp01(timer / moveTime);
                    transform.position = Vector3.Lerp(startPos, endPos, t);
                    yield return null;
                }

                // Wait for an interval
                yield return new WaitForSeconds(idleTime);

                // Switch sides
                isMovingLeft = !isMovingLeft;
            }
        }
    }
}