using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ZombieSpawnerScript : MonoBehaviour
{
    [SerializeField] int zombieAmount;
    [SerializeField] int radius;
    [SerializeField] int delay;
    [SerializeField] float zombieSpeed;
    [SerializeField] GameObject zombie;
    [SerializeField] GameObject player;
    private bool zombiesDead = false;
    private int zombiesLeft;

    // Start is called before the first frame update
    void Start()
    {
        zombiesLeft = zombieAmount;
        StartCoroutine(SpawnZombies());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator SpawnZombies() {
        int amountLeft = zombieAmount;

        while(amountLeft > 0) {
            GameObject z = Instantiate(zombie, transform.position+GenereatePosition()*radius, Quaternion.identity);
            z.GetComponent<ZombieScript>().SetPlayer(player);
            z.GetComponent<ZombieScript>().SetSpawner(gameObject);
            z.GetComponent<ZombieScript>().SetSpeed(zombieSpeed);
            amountLeft -= 1;
            yield return new WaitForSeconds(delay);
        }
    }

    private Vector3 GenereatePosition() {
        int x = UnityEngine.Random.Range(0, 2);
        int z = UnityEngine.Random.Range(0, 2);
        while(x == 0 && z == 0) {
            x = UnityEngine.Random.Range(0, 2);
            z = UnityEngine.Random.Range(0, 2);
        }
        int xSign = UnityEngine.Random.Range(0, 2);
        int zSign = UnityEngine.Random.Range(0, 2);
        if(xSign == 1) {
            x = -x;
        }
        if(zSign == 1) {
            z = -z;
        }

        return new Vector3((float)x, 0, (float)z).normalized;
    }

    public void UpdateZombiesLeft() {
        zombiesLeft -= 1;
        if(zombiesLeft == 0) {
            zombiesDead = true;
        }
    }

    public bool CheckZombiesDead() {
        return zombiesDead;
    }
}