using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorWithKey : MonoBehaviour
{
    public GameObject OpenSpriteChild;
    public GameObject ClosedSpriteChild;
    public AudioClip OpenSfxClip;

    private BoxCollider2D MyCollider;

    private void Start()
    {
        MyCollider = GetComponent<BoxCollider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //if it's a player that collided with me
        if (collision.gameObject.transform.CompareTag("Player"))
        {
            //check if they have a key
            Key key = collision.gameObject.transform.GetComponentInChildren<Key>();
            if (key != null)
            {
                //play the sound
                if (OpenSfxClip != null)
                {
                    AudioSource.PlayClipAtPoint(OpenSfxClip, transform.position);
                }

                //swap sprites to open
                ClosedSpriteChild.SetActive(false);
                OpenSpriteChild.SetActive(true);

                //disable my collider
                //don't have to check nullity because we're in a collision enter
                MyCollider.enabled = false;

                //destroy the key
                Destroy(key.gameObject);
            }
            //otherwise... none shall pass!
        }
    }
}
