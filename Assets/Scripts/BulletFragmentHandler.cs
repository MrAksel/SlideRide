using UnityEngine;
using System.Collections;

public class BulletFragmentHandler : MonoBehaviour
{
    public float lifetime = 2f;

    void Start()
    {
        Destroy(gameObject, lifetime); // Destroys this after lifetime seconds
    }
}
