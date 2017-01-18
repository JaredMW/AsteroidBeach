// Jared White
// October 20, 2016

using UnityEngine;
using System.Collections;

/// <summary>
/// A behavior for an object which tends to travel in a straight line at a
/// constant velocity
/// </summary>
public class ProjectileBehavior : MonoBehaviour
{
    // Fields
    #region fields
    // Vector Fields
    public Vector3 projectilePos;
    public Vector3 velocity;
    public Vector3 direction;
    

    // Movement Fields
    public float speed = 10f;
    public float rotationAngle;

    public GameManager gameManager;
    #endregion


    // Properties
    #region Properties
    /// <summary>
    /// The speed that this projectile moves at
    /// </summary>
    public float ProjectileSpeed
    {
        get { return speed; }
        set
        {
            if (value >= 0)
            {
                speed = value;
            }
        }
    }

    /// <summary>
    /// The game manager
    /// </summary>
    public GameManager GameManager
    {
        get { return gameManager; }
        set { gameManager = value; }
    }

    /// <summary>
    /// The angle of rotation for this projectile
    /// </summary>
    public float RotationAngle
    {
        get { return rotationAngle; }
        set
        {
            direction = Quaternion.Euler(0, 0, value) * direction;
            rotationAngle = value;
        }
    }
    #endregion


    // Use this for initialization
    void Start ()
    {
        // Initialize default vector values
        projectilePos = Vector3.zero;
        direction = Vector3.up;
        direction = transform.localRotation * direction;
        
        // Initialize of other variables
	    if (speed <= 0f)
        {
            speed = 10f;
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        Move();
        SetTransform();
        CollisionDetect();
	}


    /// <summary>
    /// Move this projectile in the direction it is facing by a constant speed
    /// </summary>
    void Move()
    {
        // Update the current starting position of this projectile
        projectilePos = transform.position;

        // Set the current velocity based on speed and elapsed time
        velocity = speed * direction * Time.deltaTime;

        // Adjust the projectile's position by incrementing it by the velocity
        projectilePos += velocity;
    }

    /// <summary>
    /// Finalize the transformation
    /// </summary>
    void SetTransform()
    {
        transform.position = projectilePos;
    }
    
    /// <summary>
    /// Remove bullets that collide with walls
    /// </summary>
    void CollisionDetect()
    {
        for (int w = 0; w < gameManager.levelManager.WallTileCount; w++)
        {
            if (CollisionDetection.PointRectCollision(
                transform.position,
                gameManager.levelManager.GetWallInfo(w)))
            {
                gameManager.Bullets.Remove(gameObject);
                Destroy(gameObject);

                break;
            }
        }
    }
}
