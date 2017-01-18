// Jared White
// October 10, 2016

using UnityEngine;
using System.Collections;

/// <summary>
/// Manager to determine interactions between overlapping sprites in the scene,
/// and take care of wrapping calculations
/// </summary>
public class CollisionDetection : MonoBehaviour
{
    // Fields
    #region Fields
    private static float minScreenX;
    private static float maxScreenX;
    private static float minScreenY;
    private static float maxScreenY;

    // Circle-Rectangle collision fields
    private static float circleDistanceX;
    private static float circleDistanceY;
    private static float radius;
    private static float rectWidth;
    private static float rectHeight;
    private static float cornerSquareDistance;

    // Wrapping fields
    private static Vector3 finalCoords = Vector3.zero;
    #endregion


    /// <summary>
    /// Side of screen. Means nothing - just for reference.
    /// </summary>
    public enum Side
    {
        Left,
        Top,
        Right,
        Bottom,
        None
    }


    // Properties
    #region Properties
    /// <summary>
    /// The left side of the screen
    /// </summary>
    public static float Left
    {
        get { return minScreenX; }
    }

    /// <summary>
    /// The right side of the screen
    /// </summary>
    public static float Right
    {
        get { return maxScreenX; }
    }

    /// <summary>
    /// The top side of the screen
    /// </summary>
    public static float Top
    {
        get { return minScreenY; }
    }

    /// <summary>
    /// The bottom side of the screen
    /// </summary>
    public static float Bottom
    {
        get { return maxScreenY; }
    }
    #endregion


    // Use this for initialization
    void Start ()
    {
        RecalculateScreenSize();
    }
	
	// Update is called once per frame
	void Update ()
    {
	    
	}


    /// <summary>
    /// Redetermine where the current minimum and maximum X and Y values of
    /// the screen are.
    /// </summary>
    public static void RecalculateScreenSize()
    {
        // Set the screen size locations for wrapping (Left, Right, Top, Bottom)
        minScreenX = Camera.main.ScreenToWorldPoint(new Vector3(0, 0)).x;
        maxScreenX = Camera.main.ScreenToWorldPoint(
            new Vector3(Screen.width, 0)).x;

        minScreenY = Camera.main.ScreenToWorldPoint(new Vector3(0, 0)).y;
        maxScreenY = Camera.main.ScreenToWorldPoint(
            new Vector3(0, Screen.height)).y;
    }


    #region Collision Detection Methods
    /// <summary>
    /// Automatically detect collision between two objects based on their
    /// SpriteInfo's bounding shape
    /// </summary>
    /// <param name="objA">First Object</param>
    /// <param name="objB">Second Object</param>
    /// <returns>True if both objects are overlapping each other</returns>
    public static bool DetectCollision(SpriteInfo objA, SpriteInfo objB)
    {
        if (objA.boundingShape == SpriteInfo.BoundingShape.Box)
        {
            if (objB.boundingShape == SpriteInfo.BoundingShape.Box)
            {
                return AABBCollision(objA, objB);
            }
            else if (objB.boundingShape == SpriteInfo.BoundingShape.Circle)
            {
                return CircleRectCollision(objB, objA);
            }
        }
        else if (objA.boundingShape == SpriteInfo.BoundingShape.Circle)
        {
            if (objB.boundingShape == SpriteInfo.BoundingShape.Box)
            {
                return CircleRectCollision(objA, objB);
            }
            else if (objB.boundingShape == SpriteInfo.BoundingShape.Circle)
            {
                return CircleCollision(objA, objB);
            }
        }

        return false;
    }


    /// <summary>
    /// <para>Rectangle-to-Rectangle:</para>
    /// Determine if 2 rectangular objects are colliding under Axis-Aligned
    /// Bounding Box collision detection methods
    /// </summary>
    /// <param name="objA">The first object</param>
    /// <param name="objB">The second object</param>
    /// <returns>True if objA and objB are intersecting each other within their
    /// bounding boxes.</returns>
    public static bool AABBCollision(SpriteInfo objA, SpriteInfo objB)
    {
        if (objB.MinX < objA.MaxX
            && objB.MaxX > objA.MinX
            && objB.MinY < objA.MaxY
            && objB.MaxY > objA.MinY)
        {
            return true;
        }

        return false;
    }


    #region Circle-to-Rectangle Collisions
    // Psuedocode acquired from:
    // http://stackoverflow.com/questions/401847/circle-rectangle-collision-detection-intersection
    #region Psuedocode
    //public bool Intersects(CircleType circle, RectType rect)
    //{
    //    circleDistance.x = abs(circle.x - rect.x);
    //    circleDistance.y = abs(circle.y - rect.y);

    //    if (circleDistance.x > (rect.width / 2 + circle.r)) { return false; }
    //    if (circleDistance.y > (rect.height / 2 + circle.r)) { return false; }

    //    if (circleDistance.x <= (rect.width / 2)) { return true; }
    //    if (circleDistance.y <= (rect.height / 2)) { return true; }

    //    cornerDistance_sq = (circleDistance.x - rect.width / 2) ^ 2 +
    //                         (circleDistance.y - rect.height / 2) ^ 2;

    //    return (cornerDistance_sq <= (circle.r ^ 2));
    //}
    #endregion


    /// <summary>
    /// Circle-to-Rectangle:
    /// Detects overlap between a circular object and a rectangular object
    /// </summary>
    /// <param name="circleObj">Circular Object</param>
    /// <param name="rectPosition">Rectangular object's position</param>
    /// <param name="width">Width of rectangle</param>
    /// <param name="height">Height of rectangle</param>
    /// <returns>True if both are intersecting each other</returns>
    public static bool CircleRectCollision(SpriteInfo circleObj,
        Vector3 rectPosition, float width, float height)
    {
        // Set up necessary fields
        radius = circleObj.Radius;
        rectWidth = width / 2;
        rectHeight = height / 2;

        // #1: Acquire the distance between the center of the circular object
        // and center of rectangular object
        circleDistanceX = Mathf.Abs(
                circleObj.transform.position.x - rectPosition.x);
        circleDistanceY = Mathf.Abs(
                circleObj.transform.position.y - rectPosition.y);


        // #2: Elminate the cases where circle is too far away that the radius
        // cannot be within the rectangular object's bounds
        if (circleDistanceX > rectWidth + radius
            || circleDistanceY > rectHeight + radius)
        {
            return false;
        }


        // #3: Confirm the cases where the circle is close enough that
        // intersection is guaranteed - i.e. center's within rectangular bounds
        if (circleDistanceX <= rectWidth || circleDistanceY <= rectHeight)
        {
            return true;
        }


        // #4: Determine whether the circle is intersecting the bounds of the
        // rectangle's corner (Square magnitude)
        cornerSquareDistance =
            (circleDistanceX - rectWidth) * (circleDistanceX - rectWidth)
            + (circleDistanceY - rectHeight) * (circleDistanceY - rectHeight);
        return (cornerSquareDistance <= radius * radius);
    }


    /// <summary>
    /// Circle-to-Rectangle:
    /// Detects overlap between a circular object and a rectangular object
    /// </summary>
    /// <param name="circlePosition">Circle object's position</param>
    /// <param name="circleRadius">Radius of circle</param>
    /// <param name="rectObj"
    /// <returns>True if both are intersecting each other</returns>
    public static bool CircleRectCollision(Vector3 circlePosition,
        float circleRadius, SpriteInfo rectObj)
    {
        // Setup necessary fields
        radius = circleRadius;
        rectWidth = rectObj.SpriteRenderer.bounds.extents.x;
        rectHeight = rectObj.SpriteRenderer.bounds.extents.y;

        // #1: Acquire the distance between the center of the circular object
        // and center of rectangular object
        circleDistanceX = Mathf.Abs(
                circlePosition.x - rectObj.transform.position.x);
        circleDistanceY = Mathf.Abs(
                circlePosition.y - rectObj.transform.position.y);


        // #2: Elminate the cases where circle is too far away that the radius
        // cannot be within the rectangular object's bounds
        if (circleDistanceX > rectWidth + radius
            || circleDistanceY > rectHeight + radius)
        {
            return false;
        }


        // #3: Confirm the cases where the circle is close enough that
        // intersection is guaranteed - i.e. center's within rectangular bounds
        if (circleDistanceX <= rectWidth || circleDistanceY <= rectHeight)
        {
            return true;
        }


        // #4: Determine whether the circle is intersecting the bounds of the
        // rectangle's corner (Square magnitude)
        cornerSquareDistance =
            (circleDistanceX - rectWidth) * (circleDistanceX - rectWidth)
            + (circleDistanceY - rectHeight) * (circleDistanceY - rectHeight);
        return (cornerSquareDistance <= radius * radius);
    }


    /// <summary>
    /// Circle-to-Rectangle:
    /// Detects overlap between a circular object and a rectangular object
    /// </summary>
    /// <param name="circlePosition">Circle object's position</param>
    /// <param name="circleRadius">Radius of circle</param>
    /// <param name="rectPosition">Rectangular object's position</param>
    /// <param name="width">Width of rectangle</param>
    /// <param name="height">Height of rectangle</param>
    /// <returns>True if both are intersecting each other</returns>
    public static bool CircleRectCollision(Vector3 circlePosition,
        float circleRadius, Vector3 rectPosition, float width, float height)
    {
        // Setup necessary fields
        radius = circleRadius;
        rectWidth = width / 2;
        rectHeight = height / 2;

        // #1: Acquire the distance between the center of the circular object
        // and center of rectangular object
        circleDistanceX = Mathf.Abs(circlePosition.x - rectPosition.x);
        circleDistanceY = Mathf.Abs(circlePosition.y - rectPosition.y);


        // #2: Elminate the cases where circle is too far away that the radius
        // cannot be within the rectangular object's bounds
        if (circleDistanceX > rectWidth + radius
            || circleDistanceY > rectHeight + radius)
        {
            return false;
        }


        // #3: Confirm the cases where the circle is close enough that
        // intersection is guaranteed - i.e. center's within rectangular bounds
        if (circleDistanceX <= rectWidth || circleDistanceY <= rectHeight)
        {
            return true;
        }


        // #4: Determine whether the circle is intersecting the bounds of the
        // rectangle's corner (Square magnitude)
        cornerSquareDistance =
            (circleDistanceX - rectWidth) * (circleDistanceX - rectWidth)
            + (circleDistanceY - rectHeight) * (circleDistanceY - rectHeight);
        return (cornerSquareDistance <= radius * radius);
    }


    /// <summary>
    /// Circle-to-Rectangle:
    /// Detects overlap between a circular object and a rectangular object
    /// </summary>
    /// <param name="circleObj">Circular Object</param>
    /// <param name="rectObj">Square Object</param>
    /// <returns>True if both are intersecting each other</returns>
    public static bool CircleRectCollision(SpriteInfo circleObj, SpriteInfo rectObj)
    {
        // Setup fields for collision detection by narrowing down the size of
        // the "width" so collision detection doesn't need to be redone 4 times
        // in 4 quadrants - only needs to be done once.
        radius = circleObj.Radius;
        rectWidth = rectObj.SpriteRenderer.bounds.extents.x;
        rectHeight = rectObj.SpriteRenderer.bounds.extents.y;


        // #1: Acquire the distance between the center of the circular object
        // and center of rectangular object
        circleDistanceX = Mathf.Abs(
                circleObj.transform.position.x - rectObj.transform.position.x);
        circleDistanceY = Mathf.Abs(
                circleObj.transform.position.y - rectObj.transform.position.y);
        


        // #2: Elminate the cases where circle is too far away that the radius
        // cannot be within the rectangular object's bounds
        if (circleDistanceX > rectWidth + radius
            || circleDistanceY > rectHeight + radius)
        {
            return false;
        }


        // #3: Confirm the cases where the circle is close enough that
        // intersection is guaranteed - i.e. center's within rectangular bounds
        if (circleDistanceX <= rectWidth || circleDistanceY <= rectHeight)
        {
            return true;
        }


        // #4: Determine whether the circle is intersecting the bounds of the
        // rectangle's corner (Square magnitude)
        cornerSquareDistance =
            (circleDistanceX - rectWidth) * (circleDistanceX - rectWidth)
            + (circleDistanceY - rectHeight) * (circleDistanceY - rectHeight);
        return (cornerSquareDistance <= radius * radius);
    }
    #endregion


    /// <summary>
    /// <para>Circle-to-Circle:</para>
    /// Determine if 2 objects are colliding under Circlular collision
    /// detection method
    /// </summary>
    /// <param name="circleObjA">The first circular object</param>
    /// <param name="circleObjB">The second circular object</param>
    /// <returns>True if both circles intersect each other</returns>
    public static bool CircleCollision(SpriteInfo circleObjA,
        SpriteInfo circleObjB)
    {
        return (Vector3.Distance(circleObjA.transform.position,
            circleObjB.transform.position)
            <= circleObjA.Radius + circleObjB.Radius);
    }


    #region Point-to-Shape Collision
    /// <summary>
    /// Determine if a point lies within a circular object
    /// </summary>
    /// <param name="point">Point</param>
    /// <param name="circleObj">Circular object</param>
    /// <returns>True if point is inside of the circular object</returns>
    public static bool PointCircleCollision(Vector3 point, SpriteInfo circleObj)
    {
        return ((circleObj.transform.position - point).sqrMagnitude
            <= circleObj.Radius * circleObj.Radius);
    }
    
    /// <summary>
    /// Determine if a point lies within a rectangular object
    /// </summary>
    /// <param name="point">Point</param>
    /// <param name="rectObj">Circular object</param>
    /// <returns>True if point is inside of the rectangular object</returns>
    public static bool PointRectCollision(Vector3 point, SpriteInfo rectObj)
    {
        return (point.x >= rectObj.SpriteRenderer.bounds.min.x
            && point.x <= rectObj.SpriteRenderer.bounds.max.x
            && point.y >= rectObj.SpriteRenderer.bounds.min.y
            && point.y <= rectObj.SpriteRenderer.bounds.max.y);
    }
    #endregion

    #endregion


    /// <summary>
    /// Sends an object to the other side of the screen if beyond the screen's
    /// bounds, and returns the new coordinates.
    /// Should only be used to change the final position of an object - not for
    /// use with calculations.
    /// </summary>
    /// <param name="gameObject">Object to wrap to other side of screen</param>
    /// <returns>Vector3 location of final coordinates</returns>
    public static Vector3 Wrap(GameObject gameObject)
    {
        //Vector3 finalCoords = new Vector3();

        // Wrap the x coordinate from left to right side of the screen
        if (gameObject.transform.position.x < minScreenX)
        {
            finalCoords.x = maxScreenX
                - (minScreenX - gameObject.transform.position.x);
        }
        // Wrap the x coordinate from right to left side of the screen
        else if (gameObject.transform.position.x > maxScreenX)
        {
            finalCoords.x = minScreenX
                + (gameObject.transform.position.x - maxScreenX);
        }

        // Wrap the y coordinate from top to bottom side of the screen
        if (gameObject.transform.position.y < minScreenY)
        {
            finalCoords.y = maxScreenY
                - (minScreenY - gameObject.transform.position.y);
        }
        // Wrap the y coordinate from bottom to top side of the screen
        else if (gameObject.transform.position.y > maxScreenY)
        {
            finalCoords.y = minScreenY
                + (gameObject.transform.position.y - maxScreenY);
        }

        gameObject.transform.position = finalCoords;
        return finalCoords;
    }


    /// <summary>
    /// Determine if an object is completely outside of the screen's bounds
    /// </summary>
    public static bool OutOfBounds(SpriteInfo obj)
    {
        return (obj.transform.position.x
                - obj.SpriteRenderer.bounds.extents.x > Right
            || obj.transform.position.x
                + obj.SpriteRenderer.bounds.extents.x < Left
            || obj.transform.position.y
                - obj.SpriteRenderer.bounds.extents.y > Bottom
            || obj.transform.position.y
                + obj.SpriteRenderer.bounds.extents.y < Top);
    }
}
