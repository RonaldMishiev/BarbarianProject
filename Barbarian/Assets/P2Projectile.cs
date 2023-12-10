using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P2Projectile : MonoBehaviour
{
    public float Speed = 6f;
    public LayerMask playerLayer;

    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(transform.position, transform.position + -transform.right * Time.deltaTime * Speed, Color.red);
        transform.position += -transform.right * Time.deltaTime * Speed;
    }

    private void OnCollisionEnter2D(Collision2D collision){

        // Check if the fireball hits an object on the P1 layer
        if (collision.gameObject.layer == LayerMask.NameToLayer("Fireball")){
            Destroy(gameObject);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Player1Layer")){
            Debug.Log("Collision detected with: " + collision.gameObject.name);
            

            Collider2D[] hitenemy = Physics2D.OverlapCircleAll(transform.position, 3f, playerLayer);
            foreach(Collider2D Player1Layer in hitenemy){
                Debug.Log("Hit " + Player1Layer.name);

                // Send the damage over to the player 
                Player1Layer.GetComponent<HeroKnight>().p1TakeDamage((double)5.0);
            }

            Destroy(gameObject);
        }
    }
}
