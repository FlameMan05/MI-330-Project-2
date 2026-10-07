using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Key : MonoBehaviour
{
    // the child object with the visible sprite
    public GameObject KeySpriteChild;

    // the sound effect to play
    public AudioClip SfxClip;

    private BoxCollider2D MyCollider;

    private void Start()
    {
        MyCollider = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if it's a player that collided with me
        if (collision.gameObject.transform.CompareTag("Player"))
        {
            //play sound effect (if any is set)
            if (SfxClip != null)
            {
                AudioSource.PlayClipAtPoint(SfxClip, transform.position);
            }

            //turn off my collider
            //don't have to check nullity because we're in a trigger enter
            MyCollider.enabled = false;

            //stop showing my sprite
            KeySpriteChild.SetActive(false);

            //make me a child of the player
            transform.SetParent(collision.gameObject.transform);

            //and then I lurk invisibly until the player hits a door
        }
    }

}
