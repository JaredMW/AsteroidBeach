// Jared White
// October 19, 2016

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Makes an object behave as an asteroid - either drifting across the screen
/// or chasing towards a target. Can spawn children when destroyed.
/// </summary>
public class AsteroidBehavior : MonoBehaviour
{
    // Fields
    #region Fields
    // Setup fields
    public Asteroid asteroidType;
    public int maxSpawnCount = 4;

    // Movement vectors
    public Vector3 asteroidPos;
    public Vector3 velocity;
    public Vector3 acceleration;
    public Vector3 direction;

    // Movement fields
    public float minDriftSpeed = .65f;
    public float maxDriftSpeed = 2.3f;

    public float maxChaserSpeed = 5.1f;
    public float maxChaserAcceleration = 15f;

    public float minRotateSpeed = -2.3f;
    public float maxRotateSpeed = 2.3f;

    public static float minChildRotation = -20f;
    public static float maxChildRotation = 20f;

    private float rotationAngle;
    private float angleToRotate;
    private float speed;
    private CollisionDetection.Side spawnDirection
        = CollisionDetection.Side.None;

    /// <summary>Prefabs of Large-Type Asteroids</summary>
    private static List<GameObject> normalAsteroids;
    /// <summary>Prefabs of Small-Type Asteroids</summary>
    private static List<GameObject> smallAsteroids;
    /// <summary>Prefabs of Chaser-Type Asteroids</summary>
    private static List<GameObject> chaserAsteroids;

    // Scaling lists
    public static List<Vector3> largeAsteroidScales = new List<Vector3>();
    public static List<Vector3> smallAsteroidScales = new List<Vector3>();
    public static List<Vector3> chaserAsteroidScales = new List<Vector3>();
    private static List<float> largeAsteroidRadii = new List<float>();
    private static List<float> smallAsteroidRadii = new List<float>();
    private static List<float> chaserAsteroidRadii = new List<float>();

    public static PlayerController player;
    #endregion


    /// <summary>
    /// A type of asteroid
    /// </summary>
    public enum Asteroid
    {
        /// <summary>Regular-sized asteroid</summary>
        Large,
        /// <summary>Small-sized child asteroid</summary>
        Small,
        /// <summary>Child asteroid that chases the player</summary>
        Chaser
    }


    // Properties
    #region Properties
    /// <summary>
    /// The type of asteroid this asteroid represents
    /// </summary>
    public Asteroid AsteroidType
    {
        get { return asteroidType; }
        set { asteroidType = value; }
    }

    /// <summary>
    /// Get/set the prefabs of large asteroids.
    /// </summary>
    public static List<GameObject> LargeAsteroids
    {
        get { return normalAsteroids; }
        set { normalAsteroids = value; }
    }

    /// <summary>
    /// Get/set the prefabs of small asteroids.
    /// </summary>
    public static List<GameObject> SmallAsteroids
    {
        get { return smallAsteroids; }
        set { smallAsteroids = value; }
    }
    
    /// <summary>
    /// Get/set the prefabs of chaser asteroids.
    /// </summary>
    public static List<GameObject> ChaserAsteroids
    {
        get { return chaserAsteroids; }
        set { chaserAsteroids = value; }
    }



    /// <summary>
    /// The radii of the large asteroid prefabs
    /// </summary>
    public static List<float> LargeRadii
    {
        get { return largeAsteroidRadii; }
        set { largeAsteroidRadii = value; }
    }

    /// <summary>
    /// The radii of the small asteroid prefabs
    /// </summary>
    public static List<float> SmallRadii
    {
        get { return smallAsteroidRadii; }
        set { smallAsteroidRadii = value; }
    }

    /// <summary>
    /// The radii of the chaser asteroid prefabs
    /// </summary>
    public static List<float> ChaserRadii
    {
        get { return chaserAsteroidRadii; }
        set { chaserAsteroidRadii = value; }
    }


    /// <summary>
    /// The velocity of this asteroid
    /// </summary>
    public Vector3 Velocity
    {
        get { return velocity; }
        set { velocity = value; }
    }

    /// <summary>
    /// Set the initial direction this asteroid should move in on Start.
    /// (Will only affect spawn
    /// </summary>
    public CollisionDetection.Side SpawnDirection
    {
        get { return spawnDirection; }
        set { spawnDirection = value; }
    }
    #endregion


    // Use this for initialization
    void Start ()
    {
	    if (maxSpawnCount < 2)
        {
            maxSpawnCount = 4;
        }

        if (minDriftSpeed < 0f)
        {
            minDriftSpeed = 0f;
        }

        if (maxDriftSpeed < minDriftSpeed)
        {
            maxDriftSpeed = minDriftSpeed + 5f;
        }

        if (maxRotateSpeed < minRotateSpeed)
        {
            maxRotateSpeed = minRotateSpeed + 3f;
        }

        if (maxChaserSpeed <= 0f)
        {
            maxChaserSpeed = 10f;
        }

        acceleration = Vector3.zero;

        // Start with an initial randomized direction if a large asteroid
        if (asteroidType == Asteroid.Large)
        {
            if (direction == Vector3.zero
               && spawnDirection == CollisionDetection.Side.None)
            {
                direction = Vector3.right;
            }
            else
            {
                direction.Normalize();
            }

            // Initialize default values
            if (spawnDirection == CollisionDetection.Side.None)
            {
                rotationAngle = Random.Range(0f, 359.99f);
            }
            else
            {
                direction = Vector3.right;

                rotationAngle = Random.Range(-70f, 70f);

                if (spawnDirection == CollisionDetection.Side.Top)
                {
                    rotationAngle += 270f;
                }
                if (spawnDirection == CollisionDetection.Side.Left)
                {
                    rotationAngle += 180f;
                }
                if (spawnDirection == CollisionDetection.Side.Bottom)
                {
                    rotationAngle += 90f;
                }
            }

            if (direction == Vector3.zero)
            {
                direction = Vector3.right;
            }
            direction = Quaternion.Euler(0f, 0f, rotationAngle) * direction;

            angleToRotate = Random.Range(minRotateSpeed, maxRotateSpeed);

            speed = Random.Range(minDriftSpeed, maxDriftSpeed);
            velocity = direction * speed;
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        switch (asteroidType)
        {
            case Asteroid.Large:
            case Asteroid.Small:
                RotateAsteroid();
                MoveAsteroid();
                SetTransform();
                break;


            case Asteroid.Chaser:
                if (player != null)
                {
                    ApplySeekForce(player.transform.position);
                }
                MoveAsteroid();
                SetTransform();
                break;
        }
	}


    /// <summary>
    /// Destroy this asteroid. If destroyed by a bullet, spawn children if it
    /// is a large asteroid. Return SpriteInfos of destroyed asteroids.
    /// </summary>
    /// <param name="byBullet">Was this asteroid destroyed by a bullet?</param>
    /// <param name="chaserChance">The chance that the spawned children will be
    /// chaser asteroids</param>
    /// <returns>The SpriteInfo components of all destroyed asteroids</returns>
    public List<SpriteInfo> DestroyAsteroid(bool byBullet = true,
        float chaserChance = .1f)
    {
        List<SpriteInfo> children = new List<SpriteInfo>();
        if (asteroidType == Asteroid.Large && byBullet)
        {
            children = SpawnChildren(
                Random.Range(2, maxSpawnCount + 1),
                chaserChance);
        }
        
        Destroy(gameObject);
        return children;
    }


    /// <summary>
    /// Rotate the asteroid, NOT the direction of the velocity, but by the
    /// drift rotation speed - constantly.
    /// </summary>
    void RotateAsteroid()
    {
        rotationAngle += angleToRotate;
    }

    /// <summary>
    /// Apply a force towards a target by adding the difference between this
    /// object's position and a target's position to the acceleration.
    /// <para>Exponentially increase size of acceleration as asteroid
    /// approaches the target so that the asteroid does not enter an orbital
    /// pattern while approaching target.</para>
    /// </summary>
    /// <param name="target">Target to move towards</param>
    void ApplySeekForce(Vector3 target)
    {
        acceleration +=
            Vector3.ClampMagnitude(
                (target * target.sqrMagnitude
                - transform.position * transform.position.sqrMagnitude),
                maxChaserAcceleration);
    }

    /// <summary>
    /// Move the asteroid forwards based on current velocity & acceleration
    /// </summary>
    void MoveAsteroid()
    {
        asteroidPos = transform.position;

        if (asteroidType == Asteroid.Chaser)
        {
            velocity += acceleration * Time.deltaTime
                * GetComponent<SpriteInfo>().Radius * 2;
            direction = velocity.normalized;

            velocity = Vector3.ClampMagnitude(velocity, maxChaserSpeed);
            rotationAngle
                = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }

        asteroidPos += velocity * Time.deltaTime
            * GetComponent<SpriteInfo>().Radius * 2;
            
        // Reset acceleration
        acceleration = Vector3.zero;
    }

    /// <summary>
    /// Finalize the transformation of the asteroid
    /// </summary>
    void SetTransform()
    {
        transform.position = asteroidPos;
        transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
    }

    /// <summary>
    /// Make smaller children appear from this asteroid's location with this
    /// asteroid's speed.
    /// <para>There's a chance, out of 100% (1f), that the children spawned
    /// will be chaser asteroids.</para>
    /// </summary>
    /// <param name="numChildren">Number of children to spawn</param>
    /// <param name="chaserChance">Percent chance the children will be chaser
    /// asteroids</param>
    List<SpriteInfo> SpawnChildren(int numChildren, float chaserChance = .1f)
    {
        //GameObject child;
        List<SpriteInfo> children = new List<SpriteInfo>();
        int childIndex = 0;

        // Spawn chaser children based on uneven random distribution of chance
        #region Chaser Child Asteroid Spawn
        if (chaserAsteroids != null && chaserAsteroids.Count > 0
            && Random.Range(0f, 1f) <= chaserChance)
        {
            for (int c = 0; c < numChildren; c++)
            {
                childIndex = Random.Range(0, chaserAsteroids.Count);
                children.Add((Instantiate(
                    chaserAsteroids[childIndex],
                    asteroidPos,
                    Quaternion.Euler(
                        0,
                        0,
                        rotationAngle + Random.Range(
                            minChildRotation,
                            maxChildRotation))) as GameObject)
                    .GetComponent<SpriteInfo>());

                children[c].transform.localScale
                    = chaserAsteroidScales[childIndex];
                children[c].Radius = chaserAsteroidRadii[childIndex];

                children[c].GetComponent<AsteroidBehavior>().direction
                    = Quaternion.Euler(
                        0,
                        0,
                        Random.Range(minChildRotation, maxChildRotation))
                    * direction;

                children[c].GetComponent<AsteroidBehavior>().velocity
                    = children[c].GetComponent<AsteroidBehavior>().direction
                    * (velocity.magnitude * 2f);

                children[c].GetComponent<AsteroidBehavior>().maxChaserSpeed
                    = velocity.magnitude * 2.5f;
            }
        }
        #endregion

        // Otherwise, spawn regular children
        #region Small Child Asteroid Spawn
        else if (smallAsteroids != null && smallAsteroids.Count > 0)
        {
            for (int c = 0; c < numChildren; c++)
            {
                childIndex = Random.Range(0, smallAsteroids.Count);
                children.Add((Instantiate(
                    smallAsteroids[childIndex],
                    asteroidPos,
                    Quaternion.Euler(
                        0,
                        0,
                        rotationAngle + Random.Range(
                            minChildRotation,
                            maxChildRotation))) as GameObject)
                    .GetComponent<SpriteInfo>());

                children[c].transform.localScale
                    = smallAsteroidScales[childIndex];
                children[c].Radius = smallAsteroidRadii[childIndex];
            
                children[c].GetComponent<AsteroidBehavior>().direction
                    = Quaternion.Euler(
                        0,
                        0,
                        Random.Range(minChildRotation, maxChildRotation))
                    * direction;

                children[c].GetComponent<AsteroidBehavior>().velocity
                    = children[c].GetComponent<AsteroidBehavior>().direction
                    * (velocity.magnitude * 2f);
            }
        }
        #endregion

        return children;
    }


    /// <summary>
    /// Rescale all of the asteroids to be the size they should be for a grid
    /// with a specified tile size.
    /// </summary>
    /// <param name="unitsPerTile">Length of 1 grid tile</param>
    public static void RescaleAsteroids(float unitsPerTile)
    {
        // Reset scaling lists
        largeAsteroidScales.Clear();
        LargeRadii.Clear();
        smallAsteroidScales.Clear();
        SmallRadii.Clear();
        chaserAsteroidScales.Clear();
        ChaserRadii.Clear();

        Vector3 asteroidScale = new Vector3();

        for (int i = 0; i < normalAsteroids.Count; i++)
        {
            asteroidScale = LevelManager.GetScaleSpriteToTileSize(
                normalAsteroids[i].GetComponent<SpriteInfo>(),
                unitsPerTile);
            asteroidScale.x
                = (asteroidScale.x * GameManager.AsteroidToTileSizeRatio);
            asteroidScale.y
                = (asteroidScale.y * GameManager.AsteroidToTileSizeRatio);

            largeAsteroidScales.Add(asteroidScale);
            largeAsteroidRadii.Add(
                unitsPerTile * GameManager.AsteroidToTileSizeRatio);
        }


        // Reset small asteroid scales & radii
        for (int i = 0; i < smallAsteroids.Count; i++)
        {
            asteroidScale
                = LevelManager.GetScaleSpriteToTileSize(
                    smallAsteroids[i].GetComponent<SpriteInfo>(),
                    unitsPerTile);
            asteroidScale.x
                = (asteroidScale.x * GameManager.AsteroidToTileSizeRatio)
                * GameManager.SmallToLargeAsteroidRatio;
            asteroidScale.y
                = (asteroidScale.y * GameManager.AsteroidToTileSizeRatio)
                * GameManager.SmallToLargeAsteroidRatio;

            smallAsteroidScales.Add(asteroidScale);
            smallAsteroidRadii.Add(unitsPerTile
                * GameManager.AsteroidToTileSizeRatio
                * GameManager.SmallToLargeAsteroidRatio);
        }


        // Reset chaser asteroid scales & radii
        for (int i = 0; i < chaserAsteroids.Count; i++)
        {
            asteroidScale
                = LevelManager.GetScaleSpriteToTileSize(
                    chaserAsteroids[i].GetComponent<SpriteInfo>(),
                    unitsPerTile);
            asteroidScale.x
                = (asteroidScale.x * GameManager.AsteroidToTileSizeRatio)
                * GameManager.SmallToLargeAsteroidRatio;
            asteroidScale.y
                = (asteroidScale.y * GameManager.AsteroidToTileSizeRatio)
                * GameManager.SmallToLargeAsteroidRatio;

            chaserAsteroidScales.Add(asteroidScale);
            chaserAsteroidRadii.Add(unitsPerTile
                * GameManager.AsteroidToTileSizeRatio
                * GameManager.SmallToLargeAsteroidRatio);
        }
    }
    
}
