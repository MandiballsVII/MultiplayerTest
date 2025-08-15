using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rb;

    private void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        rb = gameObject.GetComponent<Rigidbody2D>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        ExploteBullet();
        //AudioManager.instance.PlaySFX("GunShotHit");
    }

    public void ExploteBullet()
    {
        rb.velocity = Vector3.zero;
        animator.SetTrigger("Explote");
    }

    public void DestroyBullet()
    {
        Destroy(gameObject);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("BasicEnemy"))
        {
            ExploteBullet();
            //AudioManager.instance.PlaySFX("GunShotHit");
            //collision.gameObject.GetComponent<EnemyManager>().EnemyDeath();
        }
    }
}
