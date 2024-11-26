using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieScript : MonoBehaviour
{   
    private float speed;
    private GameObject player;
    private GameObject spawner;
    private bool active;

    void Start()
    {
        active = true;
    }

    void Update()
    {
        if(active) {
            AttackPlayer();
        }
    }

    void AttackPlayer() {
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, speed*Time.deltaTime);
    }

    public void SetPlayer(GameObject p) {
        player = p;
    }

    public void SetSpawner(GameObject s) {
        spawner = s;
    }

    public void SetSpeed(float s) {
        speed = s;
    }

    public void RemoveZombie() {
        spawner.GetComponent<ZombieSpawnerScript>().UpdateZombiesLeft();
        active = false;
        StartCoroutine(DestroyZombie());
    }

    private IEnumerator DestroyZombie() {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}
