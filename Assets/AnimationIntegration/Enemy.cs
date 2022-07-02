using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    
    [SerializeField] float respawnTime = 5;
    public bool Dead { get; private set; }

    Animator anim;
    Rigidbody[] rigidbodies;
    Collider[] colliders;
    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
        PseudoEnemyManager.Instance.RegisterEnemy(this);

        Respawn();
    }

    void Respawn()
    {
        Dead = false;
        transform.position = new Vector3(Random.Range(-15f, 15f), 0, Random.Range(-15f, 15f));
        anim.enabled = true;
        for (int i = 0; i < rigidbodies.Length; i++)
            rigidbodies[i].isKinematic = true;

        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;
        
    }
    public void Ragdollify()
    {
        Dead = true;
        anim.enabled = false;
        for (int i = 0; i < rigidbodies.Length; i++)
            rigidbodies[i].isKinematic = false;

        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = true;

        StartCoroutine(DelayedRespawn());
    }

    IEnumerator DelayedRespawn()
    {
        yield return new WaitForSeconds(respawnTime);
        Respawn();
    }

}