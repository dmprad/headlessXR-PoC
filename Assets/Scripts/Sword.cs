using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public Transform rightController;
    public Transform hmd;
    public Vector3 offset = new Vector3(0f, 0f, 0.1f);
    public Vector3 offset2 = new Vector3(0f, 0f, 0.1f);

    public Health health;

    private HashSet<EnemyChase1> enemiesHitThisContact = new HashSet<EnemyChase1>();


    private void Start()
    {

        //StartCoroutine(SpawnWhenReady());


        /*if (rightController != null)
        {
            this.transform.position = rightController.position + rightController.forward * offset.z + rightController.right * offset.x
                                            + Vector3.up * offset.y;
        }
        
        else
        {
            this.transform.position = hmd.position + hmd.forward * offset2.z + hmd.right * offset2.x
                                            + hmd.up * offset2.y;
        }  */
    
    }

    private IEnumerator SpawnWhenReady()
    {
        while (rightController == null || rightController.position == Vector3.zero)
            yield return null;

        transform.position =
            rightController.position +
            rightController.right * offset.x +
            rightController.up * offset.y +
            rightController.forward * offset.z;

        //transform.rotation = rightController.rotation;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    /*void OnCollisionEnter(Collision collision)
    {
        TryHit(collision.collider);
    }               */

    /*void OnCollisionExit(Collision collision)
{
    ClearHit(collision.collider);
} */

    void OnTriggerEnter(Collider other)
    {
        TryHit(other);
        Debug.Log("Hit by: " + other.gameObject.name);
    }

    void OnTriggerExit(Collider other)
    {
        ClearHit(other);
    }

    void TryHit(Collider other)
    {
        if (health == null)
            return;

        EnemyChase1 enemy = other.GetComponentInParent<EnemyChase1>();
        if (enemy == null)
            return;

        if (enemy.isDead)
            return;

        
        /*if (enemiesHitThisContact.Contains(enemy))
            return;

        enemiesHitThisContact.Add(enemy);
        health.DamageEnemyFromSword();       */

        if (!enemiesHitThisContact.Add(enemy))
            return;

        health.DamageEnemyFromSword();
        Debug.Log("Hit event from: " + other.name);
    }

    void ClearHit(Collider other)
    {
        EnemyChase1 enemy = other.GetComponentInParent<EnemyChase1>();
        if (enemy == null)
            return;

        if (enemiesHitThisContact.Contains(enemy))
            enemiesHitThisContact.Remove(enemy);
    }

   

}
