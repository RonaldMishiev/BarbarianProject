using UnityEngine;
using System.Collections;

public class HeroKnight_2 : MonoBehaviour {

    [SerializeField] float      m_speed = 4.0f;
    [SerializeField] float      m_jumpForce = 7.5f;
    [SerializeField] float      m_rollForce = 6.0f;
    [SerializeField] bool       m_noBlood = false;
    [SerializeField] GameObject m_slideDust;

    private Animator            m_animator;
    private Rigidbody2D         m_body2d;
    private Sensor_HeroKnight   m_groundSensor;
    private Sensor_HeroKnight   m_wallSensorR1;
    private Sensor_HeroKnight   m_wallSensorR2;
    private Sensor_HeroKnight   m_wallSensorL1;
    private Sensor_HeroKnight   m_wallSensorL2;
    private bool                m_isWallSliding = false;
    private bool                m_grounded = false;
    private bool                m_rolling = false;
    private int                 m_facingDirection = 1;
    private int                 m_currentAttack = 0;
    private float               m_timeSinceAttack = 0.0f;
    private float               m_delayToIdle = 0.0f;
    private float               m_rollDuration = 8.0f / 14.0f;
    private float               m_rollCurrentTime;
    public double               maxHealth = 100;
    double currHealth;
    bool isblocking;

    public Transform AttackPointP2;
    public float attackRange = 0.9f;
    public LayerMask enemyLayer;

    public Transform fireballPose;
    public GameObject ProjectilePrefab;

    private bool canShootFireball = true;
    private float fireballCooldown = 5.0f;
    private float timeSinceLastFireball = 0.0f;


    // Use this for initialization
    void Start ()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL1 = transform.Find("WallSensor_L1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<Sensor_HeroKnight>();
        currHealth = maxHealth;
        isblocking = false;
    }

    // Update is called once per frame
    void Update ()
    {
        // Increase timer that controls attack combo
        m_timeSinceAttack += Time.deltaTime;

        // Increase timer that checks roll duration
        if(m_rolling)
            m_rollCurrentTime += Time.deltaTime;

        // Disable rolling if timer extends duration
        if(m_rollCurrentTime > m_rollDuration)
            m_rolling = false;

        //Check if character just landed on the ground
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }

        //Check if character just started falling
        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        // -- Handle input and movement --
        float inputX = Input.GetAxis("Player2_Horizontal");

        // Swap direction of sprite depending on walk direction
        if (Input.GetKeyDown(KeyCode.LeftArrow))//inputX > 0)
        {
            Debug.Log(inputX);
            GetComponent<SpriteRenderer>().flipX = false;
            m_facingDirection = 1;
        }
            
        else if (Input.GetKeyDown(KeyCode.RightArrow))//inputX < 0)
        {
            Debug.Log(inputX);
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
        }

        // Move
        if (!m_rolling )
            m_body2d.velocity = new Vector2(inputX * m_speed, m_body2d.velocity.y);

        //Set AirSpeed in animator
        m_animator.SetFloat("AirSpeedY", m_body2d.velocity.y);

        // -- Handle Animations --
        //Wall Slide
        m_isWallSliding = (m_wallSensorR1.State() && m_wallSensorR2.State()) || (m_wallSensorL1.State() && m_wallSensorL2.State());
        m_animator.SetBool("WallSlide", m_isWallSliding);


        //Attack
        if(Input.GetKeyDown("l") && m_timeSinceAttack > 0.25f && !m_rolling)
        {
            m_currentAttack++;

            // Loop back to one after third attack
            if (m_currentAttack > 2)
                m_currentAttack = 1;

            // Reset Attack combo if time since last attack is too large
            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;

            // Call one of three attack animations "Attack1", "Attack2", "Attack3"
            m_animator.SetTrigger("Attack" + m_currentAttack);
            Collider2D[] hitenemy = Physics2D.OverlapCircleAll(AttackPointP2.position, attackRange, enemyLayer);
            foreach(Collider2D Player1Layer in hitenemy){
                Debug.Log("Hit " + Player1Layer.name);

                // Send the damage over to the player 
                Player1Layer.GetComponent<HeroKnight>().p1TakeDamage((double)20.0);
            }

            // Reset timer
            m_timeSinceAttack = 0.0f;
        }

        // Fireball cooldown
        timeSinceLastFireball += Time.deltaTime;

        if (timeSinceLastFireball >= fireballCooldown)
        {
            canShootFireball = true;
        }

        if (Input.GetKeyDown(".") && !m_rolling && canShootFireball){
            Debug.Log("Fireball shot!");
            m_animator.SetTrigger("Attack3");
            // Set the rotation based on the facing direction
            Quaternion rotation = m_facingDirection == 1 ? Quaternion.identity : Quaternion.Euler(0, 180, 0);
            Vector3 spawnPosition = m_facingDirection == 1 ? fireballPose.position : new Vector3(fireballPose.position.x + 5.0f, fireballPose.position.y, fireballPose.position.z);
            Instantiate(ProjectilePrefab, spawnPosition, rotation);

            canShootFireball = false;
            timeSinceLastFireball = 0.0f;
        }


        // Block
        else if (Input.GetKeyDown("k") && !m_rolling)
        {
            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", true);
            isblocking = true;
        }

        else if (Input.GetKeyUp("k")){ 
            m_animator.SetBool("IdleBlock", false);
            isblocking = false;
        }    
        
        // Roll
        else if (Input.GetKeyDown("right shift") && !m_rolling && !m_isWallSliding)
        {
            m_rolling = true;
            m_animator.SetTrigger("Roll");
            m_body2d.velocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.velocity.y);
        }
            

        //Jump
        else if (Input.GetKeyDown(KeyCode.UpArrow) && m_grounded && !m_rolling)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
            m_groundSensor.Disable(0.2f);
        }

        //Run
        else if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            // Reset timer
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }

        //Idle
        else
        {
            // Prevents flickering transitions to idle
            m_delayToIdle -= Time.deltaTime;
                if(m_delayToIdle < 0)
                    m_animator.SetInteger("AnimState", 0);
        }
    }

    public void p2TakeDamage(double damage){
        if (isblocking == true && !m_rolling){
            currHealth -= (double)damage * 0.1;
        }
        else{
            if (!m_rolling)
            currHealth -= damage;
        }
        // play hurt animation
        if (!m_rolling && isblocking == false){
            m_animator.SetTrigger("Hurt");
        }
        // check if player is dead
        if (currHealth <= 0){
            Die();
        }
    }

    void Die(){

        //Play death animation
        if (!m_rolling)
        {
            m_animator.SetBool("noBlood", m_noBlood);
            m_animator.SetTrigger("Death");
        }
        //Diable the player until next game
    }

    // Animation Events
    // Called in slide animation.
    void AE_SlideDust()
    {
        Vector3 spawnPosition;

        if (m_facingDirection == 1)
            spawnPosition = m_wallSensorR2.transform.position;
        else
            spawnPosition = m_wallSensorL2.transform.position;

        if (m_slideDust != null)
        {
            // Set correct arrow spawn position
            GameObject dust = Instantiate(m_slideDust, spawnPosition, gameObject.transform.localRotation) as GameObject;
            // Turn arrow in correct direction
            dust.transform.localScale = new Vector3(m_facingDirection, 1, 1);
        }
    }
}
