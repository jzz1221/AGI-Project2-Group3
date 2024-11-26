using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieScript : MonoBehaviour
{
    private float speed;
    private GameObject player;
    private GameObject spawner;
    private bool active;
    public bool isRemoved { get; private set; } // Tracks if the zombie is removed

    public GameObject plane;

    public Transform zombieTransform;

    void Start()
    {
        active = true;
        isRemoved = false;

        plane.SetActive(false);
        if (zombieTransform != null && player != null)
        {
            zombieTransform.LookAt(player.transform);
            zombieTransform.Rotate(0, 180, 0);
        }
    }

    void Update()
    {
        if (active)
        {
            AttackPlayer();
        }
    }

    void AttackPlayer()
    {
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
    }

    public void ActivatePlane()
    {
        plane.SetActive(true);
    }

    public void SetPlayer(GameObject p)
    {
        player = p;
    }

    public void SetSpawner(GameObject s)
    {
        spawner = s;
    }

    public void SetSpeed(float s)
    {
        speed = s;
    }

    public void RemoveZombie()
    {
        spawner.GetComponent<ZombieSpawnerScript>().UpdateZombiesLeft();
        active = false;
        isRemoved = true; // Mark the zombie as removed
        StartCoroutine(DestroyZombie());
    }

    private IEnumerator DestroyZombie()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}
