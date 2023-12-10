using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Projectile : MonoBehaviour
{
    public float Speed = 4.5f;
    public LayerMask enemyLayer;

    // Update is called once per frame
    private void Update()
    {
        transform.position += transform.right * Time.deltaTime * Speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the fireball hits an object on the "Player2Layer"
        if (collision.gameObject.layer == LayerMask.NameToLayer("Fireball")){
            Destroy(gameObject);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Player2Layer"))
        {
            // Deal damage to the enemy (you can customize this part)
            Collider2D[] hitenemy = Physics2D.OverlapCircleAll(transform.position, 1.0f, enemyLayer);
            foreach(Collider2D Player2Layer in hitenemy){
                Debug.Log("Hit " + Player2Layer.name);

                // Send the damage over to the player 
                Player2Layer.GetComponent<HeroKnight_2>().p2TakeDamage((double)5.0);
            }

            Destroy(gameObject);
        }
    }

}
