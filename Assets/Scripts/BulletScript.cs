using UnityEngine;
using System.Collections;
using UnityEngine.Networking;


public class BulletScript : NetworkBehaviour
{

    [SerializeField]
    GameObject fragment;

    public int range;
    public Vector3 direction;
    public float velocity;

    void Start()
    {
        if (!base.isServer)
            return;
        
        GetComponent<Rigidbody>().velocity += direction * velocity;

        Destroy(gameObject, range); // Destroys the object after range seconds.
    }

    void OnCollisionEnter(Collision collision)
    {
        //If it hits a player, kill it
        if (collision.collider.tag == "Player")
        {
            Player target = GameManager.GetPlayer(collision.collider.name);
            target.Die();
        }

        //Create fragments
        for (int i = 0; i < 7; i++)
        {
            Vector3 fragmentPosition = transform.position;
            fragmentPosition += Random.rotation.eulerAngles.normalized * 0.1f;

            Instantiate(fragment, fragmentPosition, transform.rotation);
        }
        Destroy(gameObject);
    }
}
