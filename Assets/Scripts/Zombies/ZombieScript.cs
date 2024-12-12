using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class ZombieScript : MonoBehaviour
{
    private float speed;
    private float attackingDistance;
    private GameObject player;
    private GameObject spawner;

    private ZombieAnimationScript animation;
    private bool active;
    private bool attacking;
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
        attacking = false;
        talismancolor = Color.yellow;
        pointedcolor = Color.gray;
        defaultcolor = Color.white;
        defaultcolor.a = 0;
        pointedcolor.a = 0.7f;
        talismancolor.a = 1f;
        attackingDistance = 2f;
        animation = gameObject.GetComponentInChildren<ZombieAnimationScript>();

        Audio.GetComponent<AudioTrigger>().PlayAudio();

        animation.SetWalkingTrue();

        //plane.SetActive(false);

        plane.GetComponent<MeshRenderer>().material.color = defaultcolor;

        // 如果外部未设置player，则尝试在场景中查找
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
            {
                SetPlayer(foundPlayer);
            }
        }

        // 尝试转向玩家
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
            if (!attacking)
            {
                MoveToPlayer();
            }
            else
            {
                AttackPlayer();
            }
        }
    }

    void MoveToPlayer()
    {
        // 确保 player 存在
        if (player == null) return;

        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
        float distance = Vector3.Distance(player.transform.position, gameObject.transform.position);
        if (distance < attackingDistance)
        {
            animation.SetWalkingFalse();
            animation.SetAttackingTrue();
            attacking = true;
        }
    }

    void AttackPlayer()
    {
        Debug.Log("Attacking");
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(10);
            }
        }
        RemoveZombie();
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
        if (spawner != null)
        {
            spawner.GetComponent<ZombieSpawnerScript>().UpdateZombiesLeft();
        }
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
        Vector3 newPosition = transform.position + pushDirection * (distance * 2);

        transform.position = newPosition;
        plane.GetComponent<MeshRenderer>().material.color = talismancolor;
    }

    private IEnumerator DestroyZombie()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}
