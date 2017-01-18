// Jared White
// September 29, 2016

using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the functions of an object representing the player - including
/// movement and projectile firing
/// </summary>
public class PlayerController : MonoBehaviour
{
    // Fields
    #region Fields
    public GameObject bullet;
    public float bulletTimer = .2f;
    private float bulletTime;
    private Vector3 bulletScale;

    // Vector fields
    private Vector3 playerPos;
    private Vector3 velocity;
    private Vector3 direction;
    private Vector3 acceleration;


    // Movement fields
    public float maxSpeed = 2.5f;
    public float accelerationRate = .8f;
    public float decelerationRate = 55f;
    public float turnAngle = 4f;
    private float rotationAngle;

    private SpriteInfo spriteInfo;
    private Color color;
    public GameManager gameManager;
    #endregion


    #region Properties
    /// <summary>
    /// The game manager controlling the player
    /// </summary>
    public GameManager GameManager
    {
        get { return gameManager; }
        set
        {
            gameManager = value;
            bullet.GetComponent<ProjectileBehavior>().GameManager = value;
        }
    }

    /// <summary>
    /// Sprite info for the player
    /// </summary>
    public SpriteInfo SpriteInfo
    {
        get { return spriteInfo; }
    }

    /// <summary>
    /// Scale of bullet when initialized
    /// </summary>
    public Vector3 BulletScale
    {
        get { return bulletScale; }
        set { bulletScale = value; }
    }
    
    /// <summary>
    /// Has the cooldown timer for firing a bullet been reached?
    /// </summary>
    public bool BulletPrimed
    {
        get { return gameManager.Timer >= bulletTime; }
    }

    /// <summary>
    /// Velocity of the player
    /// </summary>
    public Vector3 Velocity
    {
        get { return velocity; }
        set { velocity = value; }
    }
    #endregion


    // Use this for initialization
    void Start()
    {
        // Initialize default vector values
        velocity = new Vector3(0, 0, 0);
        direction = new Vector3(0, 1, 0);
        acceleration = new Vector3(0, 0, 0);

        rotationAngle = 0;
        transform.rotation = Quaternion.Euler(0, 0, rotationAngle);


        // Initialize other default values
        if (maxSpeed <= 0)
        {
            maxSpeed = 2.5f;
        }
        
        if (accelerationRate <= 0)
        {
            accelerationRate = 8f;
        }
        if (decelerationRate <= 0)
        {
            decelerationRate = 55f;
        }

        if (turnAngle <= 0)
        {
            turnAngle = 4f;
        }


        if (GetComponent<SpriteInfo>() != null)
        {
            spriteInfo = GetComponent<SpriteInfo>();
        }
        else
        {
            spriteInfo = new SpriteInfo();
            gameObject.AddComponent<SpriteInfo>();
        }


        // Ensure bullet has a projectile behavior
        if (bullet.GetComponent<ProjectileBehavior>() == null)
        {
            bullet.AddComponent<ProjectileBehavior>();
        }

        if (bullet.GetComponent<SpriteInfo>() == null)
        {
            bullet.AddComponent<SpriteInfo>();
        }


        // Initialize timer values
        if (bulletTimer < 0)
        {
            bulletTimer = .6f;
        }

        if (gameManager != null)
        {
            bulletTime = gameManager.Timer;
        }
        else
        {
            bulletTime = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Rotate and Movement
        RotatePlayer();
        Move();
        Wrap();

        // Collision Detection
        DetectCollisions();

        // Fire a bullet
        FireBullet();

        // Finalize Changes
        SetTransform();
    }


    /// <summary>
    /// Force the player to look at a specified angle
    /// </summary>
    /// <param name="angle">Degrees to look in</param>
    public void ForceAngle(float angle)
    {
        direction = Quaternion.Euler(0, 0, angle) * Vector3.up;
        rotationAngle = angle;
        transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
    }


    /// <summary>
    /// Change the direction that the player is currently facing in by
    /// changing the direction vector and updating the rotation angle of
    /// the object the script is attached to.
    /// <para>When the Left and Right arrow keys are pressed, the player should
    /// rotate left and right, respectively.</para>
    /// </summary>
    void RotatePlayer()
    {
        // Rotate counterclockwise if Left Arrow is pressed
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            // Update the direction vector by rotating it
            direction = Quaternion.Euler(0, 0, turnAngle) * direction;

            // Set the angle of the object's rotation
            rotationAngle += turnAngle;
        }


        // Rotate clockwise if Right Arrow is pressed
        if (Input.GetKey(KeyCode.RightArrow))
        {
            // Update the direction vector by rotating it
            direction = Quaternion.Euler(0, 0, -turnAngle) * direction;

            // Set the angle of the object's rotation
            rotationAngle -= turnAngle;
        }
    }


    /// <summary>
    /// Update velocity based upon acceleration and deceleration,
    /// and then update position of player based on the new velocity vector.
    /// <para>Acceleration is applied only while pressing the up arrow or down
    /// arrow, moving forwards or backwards, respectively - otherwise,
    /// deceleration will be applied.</para>
    /// Should be called after a new rotation is applied.
    /// </summary>
    void Move()
    {
        // Update the currently tracked position
        playerPos = transform.position;

        // If the Up Arrow key is pressed, add acceleration
        if (Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow))
        {
            // Keep the acceleration vector time-dependent, not
            // framerate-dependent
            acceleration = accelerationRate * direction * Time.deltaTime * spriteInfo.Radius * 2;

            // Update the velocity by adding the acceleration vector to the
            // velocity vector
            velocity += acceleration;

            // Limit the velocity to a maximum value
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed * spriteInfo.Radius * 2);

            // Update where the player is to be located by adding the new
            // velocity vector to it
            playerPos += velocity * Time.deltaTime;
        }


        // If the Down Arrow key is pressed, add negative acceleration.
        else if (Input.GetKey(KeyCode.DownArrow)
            && !Input.GetKey(KeyCode.UpArrow))
        {
            // Keep the acceleration vector time-dependent, not
            // framerate-dependent
            acceleration = -accelerationRate * direction * Time.deltaTime * spriteInfo.Radius * 2;

            // Update the velocity by adding the acceleration vector to the
            // velocity vector
            velocity += acceleration;

            // Limit the velocity to a maximum value
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed * spriteInfo.Radius * 2);

            // Update where the player is to be located by adding the new
            // velocity vector to it
            playerPos += velocity * Time.deltaTime;
        }


        // Else, apply a time-dependent deceleration as long as up arrow is
        // released and the velocity is non-zero until the player is not moving
        else if (velocity.magnitude >= .0001f)
        {
            acceleration = direction * decelerationRate * Time.deltaTime/* * spriteInfo.Radius * 2*/;
            acceleration = Vector3.ClampMagnitude(acceleration, .99f);

            velocity *= acceleration.magnitude;

            // Update where the player is to be located by adding the new
            // velocity vector to it
            playerPos += velocity * Time.deltaTime;
        }
    }


    /// <summary>
    /// Keep the player on the screen at all times.
    /// </summary>
    void Wrap()
    {
        // Wrap the x coordinate from left to right side of the screen
        if (playerPos.x < CollisionDetection.Left)
        {
            playerPos.x = CollisionDetection.Right
                - (CollisionDetection.Left - playerPos.x);
        }
        // Wrap the x coordinate from right to left side of the screen
        else if (playerPos.x > CollisionDetection.Right)
        {
            playerPos.x = CollisionDetection.Left
                + (playerPos.x - CollisionDetection.Right);
        }

        // Wrap the y coordinate from top to bottom side of the screen
        if (playerPos.y < CollisionDetection.Top)
        {
            playerPos.y = CollisionDetection.Bottom
                - (CollisionDetection.Top - playerPos.y);
        }
        // Wrap the y coordinate from bottom to top side of the screen
        else if (playerPos.y > CollisionDetection.Bottom)
        {
            playerPos.y = CollisionDetection.Top
                + (playerPos.y - CollisionDetection.Bottom);
        }
    }


    /// <summary>
    /// Update the position/transform and rotation of the object
    /// Should be called AFTER the position and rotation values have been update
    /// </summary>
    void SetTransform()
    {
        // Set the updated position
        transform.position = playerPos;

        // Set the updated rotation
        transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
    }


    /// <summary>
    /// If the bullet cooldown timer has passed, then when the spacebar is
    /// pressed, fire a bullet that travels in the direction the player is
    /// currently facing.
    /// </summary>
    void FireBullet()
    {
        if (Input.GetKeyDown(KeyCode.Space) && BulletPrimed)
        {
            bulletTime = gameManager.Timer + bulletTimer;

            gameManager.Bullets.Add(
                Instantiate(
                    bullet,
                    transform.position,
                    Quaternion.Euler(0, 0, rotationAngle)) as GameObject);

            gameManager.Bullets[gameManager.Bullets.Count - 1]
                .transform.localScale = bulletScale;

        }
    }


    /// <summary>
    /// Detect if the player is colliding with a collidable object, and if so,
    /// stop its movement.
    /// </summary>
    void DetectCollisions()
    {
        for (int i = 0; i < gameManager.levelManager.WallTileCount; i++)
        {
            if (CollisionDetection.CircleRectCollision(
                playerPos, spriteInfo.radius,
                gameManager.levelManager.GetWallInfo(i)))
            {
                playerPos = transform.position;
                acceleration = Vector3.zero;
                velocity = Vector3.zero;

                /// Note to self for possible improvement on script:
                /// Make some way to determine if the velocity vector will land the player into the collidable object,
                /// so it can determine whether to keep moving in the x or y axis - perhaps by snapping it to moving in
                /// the axis direction of the closest 90 degree angle from velocity's direction?
                
                gameManager.levelManager.GetWallInfo(i)
                    .SpriteRenderer.color = Color.red;
                break;
            }


            else if (gameManager.levelManager.GetWallInfo(i)
                .SpriteRenderer.color != Color.white)
            {
                color = gameManager.levelManager.GetWallInfo(i)
                    .SpriteRenderer.color;

                color += new Color(
                    1f * Time.deltaTime,
                    1f * Time.deltaTime,
                    1f * Time.deltaTime);

                if (color.r >= 1f)
                {
                    color = new Color(1f, color.g, color.b);
                }
                
                if (color.g >= 1f)
                {
                    color = new Color(color.r, 1f, color.b);
                }

                if (color.b >= 1f)
                {
                    color = new Color(color.r, color.g, 1f);
                }

                gameManager.levelManager.GetWallInfo(i)
                    .SpriteRenderer.color = color;
            }
        }
    }


    /// <summary>
    /// Reset the current time for the bullet cooldown timer so firing is
    /// instantly ready to continue
    /// </summary>
    public void ResetBulletCooldown()
    {
        bulletTime = 0;
    }
}