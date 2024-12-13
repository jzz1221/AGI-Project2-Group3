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

    private List<GameObject> spawnedZombies = new List<GameObject>(); // Store spawned zombies

    void Start()
    {
        // Do not start spawning automatically
    }

    void Update()
    {
    }

    public void StartSpawning()
    {
        // Reset states
        zombiesLeft = zombieAmount;
        zombiesDead = false;

        // Start spawning coroutine
        StartCoroutine(SpawnZombies());
    }

    public void StopSpawning()
    {
        // Stop all spawning coroutines
        StopAllCoroutines();
    }

    private IEnumerator SpawnZombies()
    {
        int amountLeft = zombieAmount;

        while (amountLeft > 0)
        {
            // If the game is no longer active, break out of the loop
            if (!GameManager.Instance.isGameActive)
            {
                yield break;
            }

            GameObject z = Instantiate(zombie, transform.position + GenereatePosition() * radius, Quaternion.identity);
            z.GetComponent<ZombieScript>().SetPlayer(player);
            z.GetComponent<ZombieScript>().SetSpawner(gameObject);
            z.GetComponent<ZombieScript>().SetSpeed(zombieSpeed);

            // Add to spawned list
            spawnedZombies.Add(z);

            amountLeft -= 1;
            yield return new WaitForSeconds(delay);
        }
    }

    private Vector3 GenereatePosition()
    {
        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2);

        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;

        return new Vector3((float)x, 0, (float)z).normalized;
    }

    public void UpdateZombiesLeft()
    {
        zombiesLeft -= 1;
        if (zombiesLeft == 0)
        {
            zombiesDead = true;
        }
    }

    public bool CheckZombiesDead()
    {
        return zombiesDead;
    }

    public void ResetSpawner()
    {
        // Destroy all currently spawned zombies
        foreach (GameObject z in spawnedZombies)
        {
            if (z != null)
                Destroy(z);
        }
        spawnedZombies.Clear();

        // Reset states
        zombiesDead = false;
        zombiesLeft = zombieAmount;
    }
}
