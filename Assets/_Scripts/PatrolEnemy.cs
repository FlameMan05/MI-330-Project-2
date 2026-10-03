using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolEnemy : MonoBehaviour
{
    public Transform[] waypoints;
    public bool reverseWaypointsAtEnds = true;
    public float waypointTolerance = .05f;

    public float speed = 2.0f;

    private int nextWaypoint = 0;
    private int dir;

    // Start is called before the first frame update
    void Start()
    {
        dir = 1;
        if (waypoints.Length != 0) {
            //jump to first waypoint
            transform.position = waypoints[0].position;
            if (waypoints.Length > 1)
            {
                nextWaypoint = 1;
            }
        }
        Debug.Log("Start complete");
    }

    // Update is called once per frame
    void Update()
    {
        // check if I'm close enough to my target position (next waypoint)
        if (Vector2.Distance(transform.position, waypoints[nextWaypoint].position) <= waypointTolerance)
        {
            nextWaypoint += dir;
            if (nextWaypoint < 0)
            {
                nextWaypoint = 1; //must be reversing if < 0
                dir = 1;
            } else if (nextWaypoint >= waypoints.Length)
            {
                if (reverseWaypointsAtEnds)
                {
                    nextWaypoint -= 2; //-1 is where it is now so -1
                    dir = -1; // reverse direction
                } else
                {
                    nextWaypoint = 0;
                }
            }
        }

        // this enemy is dumb--no pathfinding. can get hung up
        // will ignore colliders in the environment -- object didn't allow for rigidbody
        // add waypoints to prevent this
        Vector3 newPosition = Vector3.MoveTowards(transform.position, waypoints[nextWaypoint].position, speed * Time.deltaTime);
        Debug.Log($"Current position: {transform.position.x}, {transform.position.y}, {transform.position.z}");
        Debug.Log($"Movement vector: {newPosition.x}, {newPosition.y}, {newPosition.z}");
        transform.position = newPosition;
    }
}
