using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieScript : MonoBehaviour
{   
    private int speed;
    private GameObject player;
    private GameObject spawner;

    void Start()
    {
        
    }

    void Update()
    {
        AttackPlayer();
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

    public void SetSpeed(int s) {
        speed = s;
    }

    public void RemoveZombie() {
        spawner.GetComponent<ZombieSpawnerScript>().UpdateZombiesLeft();
        Destroy(gameObject);
    }
}
