using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
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
    public GameObject Audio;

    private Color pointedcolor;
    private Color talismancolor;
    private Color defaultcolor;

    void Start()
    {
        active = true;
        isRemoved = false;
        talismancolor = Color.yellow;
        pointedcolor = Color.gray;
        defaultcolor = Color.white;
        defaultcolor.a = 0;
        pointedcolor.a = 0.7f;
        talismancolor.a = 1f;

        Audio.GetComponent<AudioTrigger>().PlayAudio();

        //plane.SetActive(false);
        
        plane.GetComponent<MeshRenderer>().material.color = defaultcolor;
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

    public void PointPlane()
    {
        plane.GetComponent<MeshRenderer>().material.color = pointedcolor;
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
        plane.GetComponent<Renderer>().material.color = talismancolor;
        Destroy(Audio);
        active = false;
        isRemoved = true; // Mark the zombie as removed
        StartCoroutine(DestroyZombie());
    }

    public void PushZombie(int distance)
    {
        if (player == null) return; // 确保玩家对象存在
        
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;

        Vector3 pushDirection = -directionToPlayer;
        Vector3 newPosition = transform.position + pushDirection * (distance*2);
        
        transform.position = newPosition;
        plane.GetComponent<MeshRenderer>().material.color = talismancolor;

        // 可选：更新僵尸朝向玩家的方向
        /*zombieTransform.LookAt(player.transform);
        zombieTransform.Rotate(0, 180, 0);*/
    }

    private IEnumerator DestroyZombie()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}
