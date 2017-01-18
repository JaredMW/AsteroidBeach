// Jared White
// October 10, 2016

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Manage crucial game-specific functions and behaviors, as well as store
/// pertinent information
/// </summary>
public class GameManager : MonoBehaviour
{
    // Fields
    #region Fields
    public PlayerController player;
    public int lives = 5;
    public int levelGoal = 1; // The "unspoken" asteroid goal for the level
    private float levelScore; // The number of asteroids destroyed this level
    private bool openDoor = false;
    private bool gameOn = true;

    public GameObject doorKey;
    private Vector3 keyScale;

    public float chaserSpawnChance = 0f;
    public float chaserKeyDropChance = .5f; // Chance of getting a key from chasers
    public float smallKeyDropChance = .1f;  // Chance of getting a key from smalls
    //public float lifeDropChance = .008f;    // Chance of a life drop appearing

    public LevelManager levelManager;
    private Color playerColor;

    public List<GameObject> largeAsteroids;
    public List<GameObject> miniAsteroids;
    public List<GameObject> chaserAsteroids;
    private GameObject keyDrop;

    public int maxAsteroidCount = 6;
    public float minAsteroidSpawnTimer = .15f;
    public float maxAsteroidSpawnTimer = 4.5f;
    public float maxAsteroidSplitAngle = 50f;
    
    private float timer;
    private float asteroidTime;

    /// <summary>The time it takes to transition between levels</summary>
    public float transitionTime = 2f;
    private float levelTransitionTimer;


    private int randomRange;
    private int score;
    private string lifePlurality = "ves";
    
    /// <summary>All active bullets in the scene</summary>
    private List<GameObject> bullets;
    /// <summary>SpriteInfo of every currently active asteroid</summary>
    private List<SpriteInfo> asteroidObjects;
    /// <summary>Objects that the player cannot collide with</summary>
    
    private List<int> collidingIndices;
    private LevelManager.LevelType nextLevelType;

    private Vector3 playerToTileScale;
    public const float AsteroidToTileSizeRatio = 1.3f;
    public const float SmallToLargeAsteroidRatio = .34f;
    public const float PlayerToTileSizeRatio = .8f;

    public Text gameScore;
    public Text levelText;
    public Text finalGameScore;
    public Text finalLevel;
    public Text scoreScreen;
    public Image gameOverScreen;
    #endregion


    // Properties
    #region Properties
    /// <summary>
    /// How much time has passed since the start of the game
    /// </summary>
    public float Timer
    {
        get { return timer; }
    }

    /// <summary>
    /// Has the time limit for another asteroid spawning been reached?
    /// </summary>
    public bool AsteroidPrimed
    {
        get
        {
            return (timer >= asteroidTime
                && asteroidObjects.Count < maxAsteroidCount);
        }
    }

    /// <summary>
    /// Can the key be dropped? (Has the level goal limit been reached, is the
    /// door closed, and is there already a dropped key?)
    /// </summary>
    public bool KeyPrimed
    {
        get
        {
            return (keyDrop == null && !openDoor && levelScore >= levelGoal);
        }
    }

    /// <summary>
    /// Is the timer for starting the next level ready yet?
    /// </summary>
    public bool TransitionPrimed
    {
        get { return (timer > levelTransitionTimer); }
    }


    /// <summary>
    /// List of all active bullets in the game scene
    /// </summary>
    public List<GameObject> Bullets
    {
        get { return bullets; }
        set { bullets = value; }
    }

    /// <summary>
    /// The maximum split angle that an asteroid's children will
    /// separate into
    /// </summary>
    public float MaxAsteroidSplitAngle
    {
        get
        {
            return AsteroidBehavior.maxChildRotation
                - AsteroidBehavior.minChildRotation;
        }
        set
        {
            AsteroidBehavior.minChildRotation = -0.5f * value;
            AsteroidBehavior.maxChildRotation = 0.5f * value;
        }
    }
    #endregion


    // Use this for initialization
    void Start()
    {
        // Set timers and time limits to default values
        #region Timer Setup
        timer = 0;
        asteroidTime = 0;
        transitionTime = 0;

        // Asteroid spawn timer
        if (minAsteroidSpawnTimer < 0)
        {
            minAsteroidSpawnTimer = .15f;
        }
        if (maxAsteroidSpawnTimer <= 0)
        {
            maxAsteroidSpawnTimer = 4.5f;
        }
        if (maxAsteroidSpawnTimer > minAsteroidSpawnTimer)
        {
            maxAsteroidSpawnTimer = minAsteroidSpawnTimer + 1f;
        }

        if (maxAsteroidCount <= 0)
        {
            maxAsteroidCount = 40;
        }
        #endregion

        // Assign the player this object as the game manager
        player.GameManager = this;
        playerToTileScale = Vector3.one;
        AsteroidBehavior.player = player;

        scoreScreen.gameObject.SetActive(true);
        gameOverScreen.gameObject.SetActive(false);
        

        // Instantiate bullet list - all active bullets in scene
        bullets = new List<GameObject>();

        // Ensure valid number of lives
        if (lives <= 0)
        {
            lives = 3;
        }

        
        asteroidObjects = new List<SpriteInfo>();
        
        // Ensure all asteroids have proper components. If not a sprite,
        // don't keep track of it.
        #region Asteroid Setup
        foreach (GameObject asteroid in largeAsteroids)
        {
            if (asteroid.GetComponent<SpriteRenderer>() != null)
            {
                // Ensure the asteroid has the necessary scripts
                if (asteroid.GetComponent<SpriteInfo>() == null)
                {
                    asteroid.AddComponent<SpriteInfo>();
                }
                if (asteroid.GetComponent<AsteroidBehavior>() == null)
                {
                    asteroid.AddComponent<AsteroidBehavior>();
                    asteroid.GetComponent<AsteroidBehavior>().AsteroidType
                        = AsteroidBehavior.Asteroid.Large;
                }
            }

            else
            {
                largeAsteroids.Remove(asteroid);
            }
        }
        
        foreach (GameObject asteroid in miniAsteroids)
        {
            if (asteroid.GetComponent<SpriteRenderer>() != null)
            {
                // Ensure the asteroid has the necessary scripts
                if (asteroid.GetComponent<SpriteInfo>() == null)
                {
                    asteroid.AddComponent<SpriteInfo>();
                }
                if (asteroid.GetComponent<AsteroidBehavior>() == null)
                {
                    asteroid.AddComponent<AsteroidBehavior>();
                    asteroid.GetComponent<AsteroidBehavior>().AsteroidType
                        = AsteroidBehavior.Asteroid.Small;
                }
            }

            else
            {
                miniAsteroids.Remove(asteroid);
            }
        }

        foreach (GameObject asteroid in chaserAsteroids)
        {
            if (asteroid.GetComponent<SpriteRenderer>() != null)
            {
                // Ensure the asteroid has the necessary scripts
                if (asteroid.GetComponent<SpriteInfo>() == null)
                {
                    asteroid.AddComponent<SpriteInfo>();
                }
                if (asteroid.GetComponent<AsteroidBehavior>() == null)
                {
                    asteroid.AddComponent<AsteroidBehavior>();
                    asteroid.GetComponent<AsteroidBehavior>().AsteroidType
                        = AsteroidBehavior.Asteroid.Chaser;
                }
            }

            else
            {
                chaserAsteroids.Remove(asteroid);
            }
        }

        // Set the Asteroid class prefabs and other variables
        AsteroidBehavior.LargeAsteroids = largeAsteroids;
        AsteroidBehavior.SmallAsteroids = miniAsteroids;
        AsteroidBehavior.ChaserAsteroids = chaserAsteroids;

        if (maxAsteroidSplitAngle < 0)
        {
            maxAsteroidSplitAngle *= -1;
        }

        MaxAsteroidSplitAngle = maxAsteroidSplitAngle;
        #endregion


        // The indices of objects colliding with another object
        collidingIndices = new List<int>(asteroidObjects.Count);


        // Set up the first level
        #region Level Setup
        if (levelManager == null)
        {
            levelManager = new LevelManager();
        }

        nextLevelType = LevelManager.LevelType.Tutorial;
        NextLevel(nextLevelType);
        #endregion
    }

    
    // Update is called once per frame
    void Update()
    {
        // If game over, just do this
        if (!gameOn)
        {
            // Enable asteroids to move in background
            for (int i = 0; i < asteroidObjects.Count; i++)
            {
                // Remove asteroids that are out of the screen bounds
                if (CollisionDetection.OutOfBounds(asteroidObjects[i]))
                {
                    RemoveAsteroid(i);
                    i--;
                    break;
                }
            }


            return;
        }


        // Update the timer(s)
        if (levelTransitionTimer >= transitionTime)
        {
            timer += Time.deltaTime;
        }
        else
        {
            levelTransitionTimer += Time.deltaTime;
            player.Velocity = Vector3.zero;
        }


        // Check to see if the player made it to the open door and beat level
        if (levelTransitionTimer >= transitionTime
            && openDoor
            && ((player.SpriteInfo.Shape == SpriteInfo.BoundingShape.Box
                && CollisionDetection.PointRectCollision(
                    levelManager.DoorInfo.transform.position,
                    player.SpriteInfo))
            || (player.SpriteInfo.Shape == SpriteInfo.BoundingShape.Circle
                && CollisionDetection.PointCircleCollision(
                    levelManager.DoorInfo.transform.position,
                    player.SpriteInfo))))
        {
            NextLevel();
            player.Velocity = Vector3.zero;
        }


        #region Level Not Won Yet
        else if (levelTransitionTimer >= transitionTime)
        {
            gameScore.text = score.ToString();
            levelText.text = "Level " + levelManager.Level.ToString()
                + "   " + lives + " li" + lifePlurality + " left";

            #region Collisions and Interactions
            // Collisions between the player and asteroids:
            // If an asteroid collides with a player, they lose a life and the
            // on-screen asteroids are reset, and progress towards level end is
            // also reset.
            #region Player-to-Asteroid Collisions
            if (player.SpriteInfo.SpriteRenderer.color == Color.white)
            {
                for (int i = 0; i < asteroidObjects.Count; i++)
                {
                    if (CollisionDetection.DetectCollision(player.SpriteInfo,
                        asteroidObjects[i]))
                    {
                        if (lives > 1)
                        {
                            lives--;
                            RemoveAsteroid(i);
                            i--;
                            player.SpriteInfo.SpriteRenderer.color = Color.red;
                            if (lives == 1)
                            {
                                lifePlurality = "fe";
                            }
                        }

                        // If lives are all gone, game over.
                        else
                        {

                            player.SpriteInfo.SpriteRenderer.color = Color.red;
                            lives = 0;
                            GameOver();
                            return;
                        }
                        break;
                    }
                }
            }

            // Change the player's color slowly back to default white if needed
            if (player.SpriteInfo.SpriteRenderer.color != Color.white)
            {
                playerColor = player.SpriteInfo.SpriteRenderer.color;

                playerColor += new Color(
                    (100f / 255f) * Time.deltaTime,
                    (100f / 255f) * Time.deltaTime,
                    (100f / 255f) * Time.deltaTime);

                if (playerColor.r >= 1)
                {
                    playerColor = new Color(1, playerColor.g, playerColor.b);
                }

                if (playerColor.g >= 1)
                {
                    playerColor = new Color(playerColor.r, 1, playerColor.b);
                }

                if (playerColor.b >= 1)
                {
                    playerColor = new Color(playerColor.r, playerColor.g, 1);
                }

                player.SpriteInfo.SpriteRenderer.color = playerColor;
            }
            #endregion


            // Generate or remove asteroids
            #region Asteroid Interactions
            // Generate asteroids if maximum number hasn't been reached yet, and the
            // timer for asteroid generation is set
            for (int i = 0; i < asteroidObjects.Count; i++)
            {
                // Remove asteroids that are out of the screen bounds
                if (CollisionDetection.OutOfBounds(asteroidObjects[i]))
                {
                    RemoveAsteroid(i);
                    i--;
                    break;
                }

                // If a bullet strikes an asteroid, remove asteroid and increment
                // score
                for (int b = 0; b < bullets.Count; b++)
                {
                    if ((asteroidObjects[i].Shape == SpriteInfo.BoundingShape.Circle
                            && CollisionDetection.PointCircleCollision(
                                bullets[b].transform.position, asteroidObjects[i]))
                        || asteroidObjects[i].Shape == SpriteInfo.BoundingShape.Box
                            && CollisionDetection.PointRectCollision(
                                bullets[b].transform.position, asteroidObjects[i]))
                    {
                        switch (asteroidObjects[i]
                            .GetComponent<AsteroidBehavior>().AsteroidType)
                        {
                            case AsteroidBehavior.Asteroid.Large:
                                score += 20;
                                levelScore += 0.1f;
                                break;

                            case AsteroidBehavior.Asteroid.Small:
                                score += 50;
                                levelScore += 0.75f;

                                if (KeyPrimed
                                    && Random.Range(0f, 1f) < smallKeyDropChance)
                                {
                                    DropKey(asteroidObjects[i].transform.position);
                                }
                                break;

                            case AsteroidBehavior.Asteroid.Chaser:
                                score += 75;
                                levelScore += 1.25f;

                                if (KeyPrimed
                                    && Random.Range(0f, 1f) < chaserKeyDropChance)
                                {
                                    DropKey(asteroidObjects[i].transform.position);
                                }
                                break;

                            default:
                                score += 20;
                                break;
                        }

                        // Remove the asteroid that was hit
                        RemoveAsteroid(i, true);
                        i--;

                        // Remove the bullet that hit the asteroid
                        RemoveBullet(b);
                        b--;
                        break;
                    }
                }
            }
            #endregion


            #region Bullet Interactions
            for (int i = 0; i < bullets.Count; i++)
            {
                // Remove out-of-bounds bullets
                if (CollisionDetection.OutOfBounds(
                    bullets[i].GetComponent<SpriteInfo>()))
                {
                    RemoveBullet(i);
                    i--;

                    break;
                }

                // Remove bullets that collide with walls
                for (int w = 0; w < levelManager.WallTileCount; w++)
                {
                    if (CollisionDetection.PointRectCollision(
                        bullets[i].transform.position,
                        levelManager.GetWallInfo(w)))
                    {
                        RemoveBullet(i);
                        i--;

                        break;
                    }
                }
            }
            #endregion
            #endregion


            // Detect if player has picked up the active key
            if (!openDoor && keyDrop != null
                && CollisionDetection.DetectCollision(
                    player.SpriteInfo, keyDrop.GetComponent<SpriteInfo>()))
            {
                RemoveKey();
                levelManager.OpenDoor();
                openDoor = true;
            }


            // Generate an asteroid if the timer is ready and max spawn count is
            // not yet reached
            if (AsteroidPrimed)
            {
                GenerateAsteroid();
            }
        }
        #endregion
    }


    /// <summary>
    /// Spawn a key drop at a location
    /// </summary>
    void DropKey(Vector3 location)
    {
        keyDrop = (Instantiate(
            doorKey,
            location,
            Quaternion.identity) as GameObject);
        keyDrop.transform.localScale = keyScale;
        keyDrop.GetComponent<SpriteInfo>().Radius
            = levelManager.UnitsPerSquare * .75f;
        keyDrop.GetComponent<SpriteInfo>().Shape
            = SpriteInfo.BoundingShape.Circle;

        // Place it in the scren bounds
        Vector3 newLocation = keyDrop.transform.position;

        if (keyDrop.transform.position.x < CollisionDetection.Left)
        {
            newLocation.x = CollisionDetection.Left
                + keyDrop.GetComponent<SpriteRenderer>().bounds.extents.x;
        }
        else if (keyDrop.transform.position.x > CollisionDetection.Right)
        {
            newLocation.x = CollisionDetection.Right
                - keyDrop.GetComponent<SpriteRenderer>().bounds.extents.x;
        }
        if (keyDrop.transform.position.y < CollisionDetection.Top)
        {
            newLocation.y = CollisionDetection.Top
                + keyDrop.GetComponent<SpriteRenderer>().bounds.extents.y;
        }
        else if (keyDrop.transform.position.y > CollisionDetection.Bottom)
        {
            newLocation.y = CollisionDetection.Bottom
                - keyDrop.GetComponent<SpriteRenderer>().bounds.extents.y;
        }

        keyDrop.transform.position = newLocation;
    }


    /// <summary>
    /// Safely remove the key drop from the scene
    /// </summary>
    void RemoveKey()
    {
        if (keyDrop != null)
        {
            Destroy(keyDrop);
            keyDrop = null;
        }
    }
    
    
    /// <summary>
    /// Advance to the next level, create the next map, and readjust the size of
    /// the player as necessary.
    /// <para>Every other 5 levels alternates between daytime and nighttime, starting
    /// with levels 1-5 being daytime, and 6-10 being nighttime.</para>
    /// </summary>
    void NextLevel()
    {
        // Remove all active asteroids
        for (int i = 0; i < asteroidObjects.Count; i++)
        {
            RemoveAsteroid(i);
        }

        // Remove all active bullets
        for (int b = 0; b < bullets.Count; b++)
        {
            RemoveBullet(b);
        }


        // Set the next level type based on new level number
        if ((levelManager.Level / 5) % 2 == 0)
        {
            nextLevelType = LevelManager.LevelType.Normal;
        }
        //else if ((levelManager.Level / 5) % 2 == 1)
        //{
        //    nextLevelType = LevelManager.LevelType.Night;
        //}
        else
        {
            nextLevelType = LevelManager.LevelType.Normal;
        }
        
        NextLevel(nextLevelType);
    }
    
    /// <summary>
    /// Advance to the next level, create the next map, and readjust the size of
    /// the player as necessary.
    /// </summary>
    /// <param name="nextLevelType">Force set the level type of the next level
    /// generated</param>
    void NextLevel(LevelManager.LevelType nextLevelType)
    {
        // Reset level-specific stats
        timer = 0;
        levelTransitionTimer = 0 + transitionTime;
        levelScore = 0;
        openDoor = false;
        score += (levelManager.Level * 100);

        // Remove all active asteroids and bullets
        for (int i = 0; i < asteroidObjects.Count; i++)
        {
            RemoveAsteroid(i);
            i--;
        }
        for (int b = 0; b < bullets.Count; b++)
        {
            RemoveBullet(b);
            b--;
        }

        // Advance to the level and create the next map
        levelManager.AdvanceLevel(
            nextLevelType,
            Mathf.Clamp(
                ((levelManager.Level + 1) / 3) + 7,
                7,
                20));

        // Set stats for map dependent on level and level type
        if (nextLevelType == LevelManager.LevelType.Tutorial)
        {
            maxAsteroidCount = 5;
            minAsteroidSpawnTimer = .4f;
            maxAsteroidSpawnTimer = 3f;
            chaserSpawnChance = 0;
            //lifeDropChance = 0;
            smallKeyDropChance = .3f;
            levelGoal = 1;
        }

        // Increment difficulty of next level
        else if (levelManager.Level <= 20)
        {
            // Level stats
            levelGoal = 2 + ((levelManager.Level - 1) * 4);
            maxAsteroidCount = 10 + (int)((levelManager.Level - 1) * .8f);
            minAsteroidSpawnTimer = .4f - (levelManager.Level * .015f);
            maxAsteroidSpawnTimer = 1.3f - (levelManager.Level * .05f);

            // Drop and Spawn Rarities
            if (levelManager.Level == 3)
            {
                chaserSpawnChance = .15f; // 150% chaser spawn chance
            }

            else
            {
                if (levelManager.Level == 2)
                {
                    chaserSpawnChance = 0;
                }
                else
                {
                    // 01.00% -> 4.00% chaser spawn chance
                    chaserSpawnChance = .01f * (1f + (levelManager.Level * .15f));
                }

                // 80.00% -> 40.00% chaser key drop chance
                chaserKeyDropChance = .01f * (80f - (levelManager.Level * 2f));
                // 40.00% -> 01.00% small key drop chance
                smallKeyDropChance = .01f * (40f - (levelManager.Level * 1.95f));
                // 01.00% -> 00.50% life drop chance
                //lifeDropChance = .01f * (1f - (levelManager.Level * .025f));
            }
        }

        // Maximum difficulty for everything except minimum number of asteroids
        // needed to enable key drop
        else
        {
            levelGoal = (int)((levelManager.Level - 1) * 1.3f) + 52;

            // These should be the permanent final values for the following:
            maxAsteroidCount = 30;
            minAsteroidSpawnTimer = .1f;
            maxAsteroidSpawnTimer = .3f;

            chaserSpawnChance = .04f;    // 4.00% chaser spawn chance
            chaserKeyDropChance = .4f;  // 40.00% chaser key drop chance
            smallKeyDropChance = .01f;   // 01.00% small key drop chance
            //lifeDropChance = .005f;      // 00.50% life drop chance
        }

        // Three seconds before first asteroid spawns
        asteroidTime = 3f;
        player.ResetBulletCooldown();


        // Scale key
        keyScale = LevelManager.GetScaleSpriteToTileSize(
            doorKey.GetComponent<SpriteInfo>(),
            levelManager.UnitsPerSquare * .75f);
        doorKey.GetComponent<SpriteInfo>().Radius
            = levelManager.UnitsPerSquare * .75f;


        #region Scale Player
        // Resize the player & their bounds based upon the new size of the map
        if (player.SpriteInfo != null && levelManager.LeveHeight > 4)
        {
            playerToTileScale
                = levelManager.GetScaleSpriteToTileSize(player.SpriteInfo);

            playerToTileScale.x *= PlayerToTileSizeRatio;
            playerToTileScale.y *= PlayerToTileSizeRatio;
            

            if (player.SpriteInfo.Shape == SpriteInfo.BoundingShape.Box)
            {
                player.SpriteInfo.Radius = levelManager.UnitsPerSquare *
                    ((playerToTileScale.x + playerToTileScale.y / 2) / 2);
            }
            else
            {
                player.SpriteInfo.Radius = levelManager.UnitsPerSquare
                    * PlayerToTileSizeRatio;
            }

            player.transform.localScale = playerToTileScale;
            player.transform.position = levelManager.GetRandomSpawnLocation();

            player.ForceAngle(Random.Range(0, 4) * 90);
        }
        #endregion


        #region Scale Bullet
        // Resize the bullets shot by the player
        if (player.bullet != null
            && player.bullet.GetComponent<SpriteInfo>() != null
            && levelManager.LeveHeight > 4)
        {
            playerToTileScale = levelManager.GetScaleSpriteToTileSize(
                player.bullet.GetComponent<SpriteInfo>());
            playerToTileScale.x *= PlayerToTileSizeRatio * .35f;
            playerToTileScale.y *= PlayerToTileSizeRatio * .35f;
            player.BulletScale = playerToTileScale;
            player.bullet.GetComponent<SpriteInfo>().Radius
                = levelManager.UnitsPerSquare * PlayerToTileSizeRatio * .35f;
        }
        #endregion


        // Rescale the asteroids and their radii with the new map size
        AsteroidBehavior.RescaleAsteroids(levelManager.UnitsPerSquare);
    }


    /// <summary>
    /// Generate a single asteroid, place it just outside the edge of the
    /// screen, and set its direction/velocity so it moves in a random direction
    /// through the screen space based on which edge the asteroid was made in.
    /// </summary>
    void GenerateAsteroid()
    {
        // Cooldown timer before next asteroid can be generated
        asteroidTime = timer
            + Random.Range(minAsteroidSpawnTimer, maxAsteroidSpawnTimer);
        randomRange = Random.Range(0, AsteroidBehavior.LargeAsteroids.Count);

        
        // Select a random side of the screen for the asteroid to spawn on
        #region Spawn Side Selection
        int screenSpawnSide = Random.Range(0, 4);  // Left, Top, Right, Bottom
        float spawnX, spawnY;
        
        // Left or right
        if (screenSpawnSide == 0 || screenSpawnSide == 2)
        {
            spawnY = Random.Range(
                CollisionDetection.Bottom, CollisionDetection.Top);

            // Left
            if (screenSpawnSide == 0)
            {
                spawnX = CollisionDetection.Left;
            }

            // Right
            else
            {
                spawnX = CollisionDetection.Right;
            }
        }
        // Top or bottom
        else
        {
            spawnX = Random.Range(
                CollisionDetection.Left, CollisionDetection.Right);

            // Top
            if (screenSpawnSide == 1)
            {
                spawnY = CollisionDetection.Top;
            }

            // Bottom
            else
            {
                spawnY = CollisionDetection.Bottom;
            }
        }
        Vector3 spawnLocation = new Vector3(spawnX, spawnY, 0);
        #endregion


        asteroidObjects.Add((Instantiate(
            AsteroidBehavior.LargeAsteroids[randomRange],
            spawnLocation,
            Quaternion.identity) as GameObject).GetComponent<SpriteInfo>());

        asteroidObjects[asteroidObjects.Count - 1].transform.localScale
            = AsteroidBehavior.largeAsteroidScales[randomRange];

        asteroidObjects[asteroidObjects.Count - 1]
            .GetComponent<SpriteInfo>().Radius
                = AsteroidBehavior.LargeRadii[randomRange];

        // Ensure the asteroid's starting velocity is entering towards the
        // screen and not out of bounds so it's not destroyed immediately
        #region Spawn Velocity Correction
        // Left or right
        if (screenSpawnSide == 0 || screenSpawnSide == 2)
        {
            // Left: move right
            if (screenSpawnSide == 0)
            {
                spawnLocation.x -= asteroidObjects[asteroidObjects.Count - 1]
                    .SpriteRenderer.bounds.extents.x;

                asteroidObjects[asteroidObjects.Count - 1]
                    .GetComponent<AsteroidBehavior>().SpawnDirection
                    = CollisionDetection.Side.Right;
            }

            // Right: move left
            else
            {
                spawnLocation.x += asteroidObjects[asteroidObjects.Count - 1]
                    .SpriteRenderer.bounds.extents.x;

                asteroidObjects[asteroidObjects.Count - 1]
                    .GetComponent<AsteroidBehavior>().SpawnDirection
                    = CollisionDetection.Side.Left;
            }
        }
        // Top or bottom
        else
        {
            // Top: move down
            if (screenSpawnSide == 1)
            {
                spawnLocation.y -= asteroidObjects[asteroidObjects.Count - 1]
                    .SpriteRenderer.bounds.extents.y;

                asteroidObjects[asteroidObjects.Count - 1]
                    .GetComponent<AsteroidBehavior>().SpawnDirection
                    = CollisionDetection.Side.Bottom;
            }

            // Bottom: move up
            else
            {
                spawnLocation.y += asteroidObjects[asteroidObjects.Count - 1]
                    .SpriteRenderer.bounds.extents.y;

                asteroidObjects[asteroidObjects.Count - 1]
                    .GetComponent<AsteroidBehavior>().SpawnDirection
                    = CollisionDetection.Side.Top;
            }
        }

        asteroidObjects[asteroidObjects.Count - 1].transform.position
            = spawnLocation;
        #endregion
    }


    /// <summary>
    /// Safely remove an asteroid from the scene, and spawn children if needed
    /// </summary>
    /// <param name="asteroidIndex">Index of asteroid to remove</param>
    /// <param name="byBullet">Was the asteroid detroyed from a bullet
    /// collision?</param>
    void RemoveAsteroid(int asteroidIndex, bool byBullet = false)
    {
        if (byBullet)
        {
            AsteroidBehavior.Asteroid asteroidType
                = asteroidObjects[asteroidIndex]
                .GetComponent<AsteroidBehavior>().AsteroidType;
        }

        asteroidObjects.AddRange(
            asteroidObjects[asteroidIndex].GetComponent<AsteroidBehavior>()
                .DestroyAsteroid(byBullet, chaserSpawnChance));

        asteroidObjects.RemoveAt(asteroidIndex);
    }


    /// <summary>
    /// Safely remove a bullet from the scene
    /// </summary>
    /// <param name="bulletIndex">Index of bullet to remove</param>
    void RemoveBullet(int bulletIndex)
    {
        Destroy(bullets[bulletIndex]);
        bullets.RemoveAt(bulletIndex);
    }


    /// <summary>
    /// When the player has lost all their lives, the game is over.
    /// </summary>
    void GameOver()
    {
        gameOn = false;
        for (int i = 0; i < bullets.Count; i++)
        {
            RemoveBullet(i);
        }


        gameScore.gameObject.SetActive(false);
        scoreScreen.gameObject.SetActive(false);
        gameOverScreen.gameObject.SetActive(true);
        finalGameScore.gameObject.SetActive(true);
        finalLevel.gameObject.SetActive(true);
        finalGameScore.text = score.ToString();
        finalLevel.text = "Level " + levelManager.Level;
    }
}