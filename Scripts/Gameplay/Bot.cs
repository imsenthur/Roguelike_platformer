using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Algorithms;
using CLUTCH;
using CLUTCHINPUT;
using UnityEngine.UI;

public class Bot : MonoBehaviour
{
    #region Variables

    public float Botattackdist = 20;
    private float BotFollowRange = 300;
    private bool FollowPlayer;
    public int UpdateRate = 2;
    private DamageHandler playerDamage;

    [System.Serializable]
    public enum CharacterState
    {
        Stand,
        Run,
        Jump,
        GrabLedge,
    };

    public AudioClip mHitWallSfx;
    public AudioClip mJumpSfx;
    public AudioClip mWalkSfx;
    public AudioSource mAudioSource;

    public enum BotAction
	{
		None = 0,
		MoveTo,
	}

    public float mWalkSpeed;
    public float mJumpSpeed;
    protected int mFramesFromJumpStart = 0;
    public float mWalkSfxTimer = 0.0f;
    public const float cWalkSfxTime = 0.25f;

    private bool[] mInputs;
    private bool[] mPrevInputs;

    public BotAction mCurrentAction = BotAction.None;
	public Vector2 mDestination;
	public int mCurrentNodeId = -1;
	public int mFramesOfJumping = 0;
	public int mStuckFrames = 0;
    public int mMaxJumpHeight = 5;
    public int mWidth = 1;
    public int mHeight = 3;
	public const int cMaxStuckFrames = 20;

    public Animator mAnimator;

    public List<Vector2i> mPath = new List<Vector2i>();

    public CharacterState mCurrentState = CharacterState.Stand;


    public Transform playerpos;

    public Map mmapdata;
    /// <summary>
    /// The previous position.
    /// </summary>
    public Vector2 mOldPosition;
    /// <summary>
    /// The current position.
    /// </summary>
    public Vector2 mPosition;
    public Vector2 mScale;

    /// <summary>
    /// The current speed in pixels/second.
    /// </summary>
    public Vector2 mSpeed;

    /// <summary>
    /// The previous speed in pixels/second.
    /// </summary>
    public Vector2 mOldSpeed;

    public Vector2 mAABBOffset;

    /// <summary>
    /// The AABB for collision queries.
    /// </summary>
    public AABB mAABB;

    /// <summary>
    /// The tile map.
    /// </summary>
    public Map mMap;

    /// <summary>
    /// True if the instance is right beside the right wall.
    /// </summary>
    public bool mPushesRightWall = false;
    /// <summary>
    /// True if the instance is right beside the left wall.
    /// </summary>
    public bool mPushesLeftWall = false;
    /// <summary>
    /// True if the instance is on the ground.
    /// </summary>
    public bool mOnGround = false;
    /// <summary>
    /// True if the instance hits the ceiling.
    /// </summary>
    public bool mAtCeiling = false;
    /// <summary>
    /// The previous state of atCeiling.
    /// </summary>
    public bool mWasAtCeiling = false;
    /// <summary>
    /// The previous state of onGround.
    /// </summary>
    public bool mWasOnGround = false;
    /// <summary>
    /// The previous state of pushesRightWall.
    /// </summary>
    public bool mPushedRightWall = false;
    /// <summary>
    /// The previous state of pushesLeftWall.
    /// </summary>
    public bool mPushedLeftWall = false;

    public bool mOnOneWayPlatform = false;

    /// <summary>
    /// Depth for z-ordering the sprites.
    /// </summary>
    public float mSpriteDepth = -1.0f;

    /// <summary>
    /// If the object is colliding with one way platform tile and the distance to the tile's top is less
    /// than this threshold, then the object will be aligned to the one way platform.
    /// </summary>
    public float cOneWayPlatformThreshold = 2.0f;

    public bool mIgnoresOneWayPlatforms = false;


    public float Gametimer;
    public int seconds = 0;

    //private float recoil=0;
#endregion

    void OnDrawGizmos()
    {
        DrawMovingObjectGizmos();
    }

    /// <summary>
    /// Draws the aabb and ceiling, ground and wall sensors .
    /// </summary>
    protected void DrawMovingObjectGizmos()
    {
        //calculate the position of the aabb's center
        var aabbPos = transform.position + (Vector3)mAABBOffset;

        //draw the aabb rectangle
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(aabbPos, mAABB.HalfSize * 2.0f);

        //draw the ground checking sensor
        Vector2 bottomLeft = aabbPos - new Vector3(mAABB.HalfSizeX, mAABB.HalfSizeY, 0.0f) - Vector3.up + Vector3.right;
        var bottomRight = new Vector2(bottomLeft.x + mAABB.HalfSizeX * 2.0f - 2.0f, bottomLeft.y);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(bottomLeft, bottomRight);

        //draw the ceiling checking sensor
        Vector2 topRight = aabbPos + new Vector3(mAABB.HalfSize.x, mAABB.HalfSize.y, 0.0f) + Vector3.up - Vector3.right;
        var topLeft = new Vector2(topRight.x - mAABB.HalfSize.x * 2.0f + 2.0f, topRight.y);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(topLeft, topRight);

        //draw left wall checking sensor
        bottomLeft = aabbPos - new Vector3(mAABB.HalfSize.x, mAABB.HalfSize.y, 0.0f) - Vector3.right;
        topLeft = bottomLeft;
        topLeft.y += mAABB.HalfSize.y * 2.0f;

        Gizmos.DrawLine(topLeft, bottomLeft);

        //draw right wall checking sensor

        bottomRight = aabbPos + new Vector3(mAABB.HalfSize.x, -mAABB.HalfSize.y, 0.0f) + Vector3.right;
        topRight = bottomRight;
        topRight.y += mAABB.HalfSize.y * 2.0f;

        Gizmos.DrawLine(topRight, bottomRight);
    }

    public void TappedOnTile(Vector2i mapPos)
    {
        if (mMap != null)
        {
            while (!(mMap.IsGround(mapPos.x, mapPos.y)))
                --mapPos.y;

            MoveTo(new Vector2i(mapPos.x, mapPos.y + 1));
        }
    }

    public void BotInit()
    {
        mWidth = 1;
        mHeight = 3;
        mScale = Vector2.one;
        mAudioSource = GetComponent<AudioSource>();
        mPosition = transform.position;

        playerpos = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

        mAABB.HalfSize = new Vector2(Constants.cHalfSizesw, Constants.cHalfSizesh);

        mJumpSpeed = Constants.cJumpSpeed;
        mWalkSpeed = Constants.cWalkSpeed;

        mAABBOffset.y = mAABB.HalfSizeY;
        //transform.localScale = new Vector3(mAABB.HalfSizeX / 8.0f, mAABB.HalfSizeY / 8.0f, 1.0f);
    }

    bool IsOnGroundAndFitsPos(Vector2i pos)
    {
        for (int y = pos.y; y < pos.y + mHeight; ++y)
        {
            for (int x = pos.x; x < pos.x + mWidth; ++x)
            {
                if (mMap.IsObstacle(x, y))
                    return false;
            }
        }

        for (int x = pos.x; x < pos.x + mWidth; ++x)
        {
            if (mMap.IsGround(x, pos.y - 1))
                return true;
        }

        return false;
    }
    public void MoveTo(Vector2i destination)
    {
        mStuckFrames = 0;

        Vector2i startTile = mMap.GetMapTileAtPoint(mAABB.Center - mAABB.HalfSize + Vector2.one * Map.cTileSize * 0.5f);

        if (mOnGround && !IsOnGroundAndFitsPos(startTile))
        {
            if (IsOnGroundAndFitsPos(new Vector2i(startTile.x + 1, startTile.y)))
                startTile.x += 1;
            else
                startTile.x -= 1;
        }

        var path = mMap.mPathFinder.FindPath(
                        startTile,
                        destination,
                        Mathf.CeilToInt(mAABB.HalfSizeX / 8.0f),
                        Mathf.CeilToInt(mAABB.HalfSizeY / 8.0f),
                        (short)mMaxJumpHeight);


        mPath.Clear();

        if (path != null && path.Count > 1)
        {
            for (var i = path.Count - 1; i >= 0; --i)
                mPath.Add(path[i]);

            mCurrentNodeId = 1;

            ChangeAction(BotAction.MoveTo);

            mFramesOfJumping = GetJumpFramesForNode(0);
        }
        else
        {
            mCurrentNodeId = -1;

            if (mCurrentAction == BotAction.MoveTo)
                mCurrentAction = BotAction.None;
        }

        //if (!Debug.isDebugBuild)
        //  DrawPathLines();
    }

    public void MoveTo(Vector2 destination)
    {
        MoveTo(mMap.GetMapTileAtPoint(destination));
    }

    #region EnvironmentCheck

    /// <summary>
    /// Determines whether there's ceiling right above the hero.
    /// </summary>
    /// <returns>
    /// <c>true</c> if there is ceiling right above the hero; otherwise, <c>false</c>.
    /// </returns>
    /// <param name='ceilY'>
    /// The position of the bottom of the ceiling tile in world coordinates.
    /// </param>
    public bool HasCeiling(Vector2 position, out float ceilingY)
    {
        //make sure the aabb is up to date with the position
        var center = position + mAABBOffset;

        //init the groundY
        ceilingY = 0.0f;

        //set the Vector2is right below us on our left and right sides
        var topRight = center + mAABB.HalfSize + Vector2.up - Vector2.right;
        var topLeft = new Vector2(topRight.x - mAABB.HalfSize.x * 2.0f + 2.0f, topRight.y);

        //get the indices of a tile below us on our left side
        int tileIndexX, tileIndexY;

        //iterate over all the tiles that the object may collide with from the left to the right
        for (var checkedVector2i = topLeft; checkedVector2i.x < topRight.x + Map.cTileSize; checkedVector2i.x += Map.cTileSize)
        {
            //makre sure that we don't check beyound the top right corner
            checkedVector2i.x = Mathf.Min(checkedVector2i.x, topRight.x);

            mMap.GetMapTileAtPoint(checkedVector2i, out tileIndexX, out tileIndexY);

            if (tileIndexY < 0 || tileIndexY >= mMap.mHeight) return false;
            if (tileIndexX < 0 || tileIndexX >= mMap.mWidth) return false;

            //if below this tile there is another tile, that means we can't possibly
            //hit it without hitting the one below, so we can immidiately skip to the topRight corner check
            if (!mMap.IsObstacle(tileIndexX, tileIndexY - 1))
            {
                //if the tile is not empty, it means we have ceiling right above us
                if (mMap.IsObstacle(tileIndexX, tileIndexY))
                {
                    //calculate the y position of the bottom of the ceiling tile
                    ceilingY = (float)tileIndexY * Map.cTileSize - Map.cTileSize / 2.0f + mMap.position.y;
                    return true;
                }
            }

            //if we checked all the possible tiles and there's nothing right above the aabb
            if (checkedVector2i.x == topRight.x)
                return false;
        }

        //there's nothing right above the aabb
        return false;
    }

    /// <summary>
    /// Determines whether there's ground right below the hero.
    /// </summary>
    /// <returns>
    /// <c>true</c> if there is ground right below the hero; otherwise, <c>false</c>.
    /// </returns>
    /// <param name='groundY'>
    /// The position of the top of the ground tile in world coordinates.
    /// </param>
    public bool HasGround(Vector2 position, out float groundY)
    {
        //make sure the aabb is up to date with the position
        var center = position + mAABBOffset;

        //init the groundY
        groundY = 0.0f;

        //set the Vector2is right below us on our left and right sides
        var bottomLeft = center - mAABB.HalfSize - Vector2.up + Vector2.right;
        var bottomRight = new Vector2(bottomLeft.x + mAABB.HalfSize.x * 2.0f - 2.0f, bottomLeft.y);

        //left side
        //calculate the indices of a tile below us on our left side
        int tileIndexX, tileIndexY;

        //iterate over all the tiles that the object may collide with from the left to the right
        for (var checkedVector2i = bottomLeft; checkedVector2i.x < bottomRight.x + Map.cTileSize; checkedVector2i.x += Map.cTileSize)
        {
            //makre sure that we don't check beyound the bottom right corner
            checkedVector2i.x = Mathf.Min(checkedVector2i.x, bottomRight.x);

            mMap.GetMapTileAtPoint(checkedVector2i, out tileIndexX, out tileIndexY);

            if (tileIndexY < 0 || tileIndexY >= mMap.mHeight) return false;
            if (tileIndexX < 0 || tileIndexX >= mMap.mWidth) return false;

            //if above this tile there is another tile, that means we can't possibly
            //hit it without hitting the one above
            if (!mMap.IsObstacle(tileIndexX, tileIndexY + 1))
            {
                var floorTop = (float)tileIndexY * Map.cTileSize + Map.cTileSize / 2.0f + mMap.position.y;
                //if the tile is not empty, it means we have a floor right below us
                if (mMap.IsObstacle(tileIndexX, tileIndexY))
                {
                    //calculate the y position of the floor tile's top
                    groundY = floorTop;
                    return true;
                }//if there's a one way platform below us, treat it as a floor only if we're falling or standing
                else if ((mMap.IsOneWayPlatform(tileIndexX, tileIndexY) && !mIgnoresOneWayPlatforms) && mSpeed.y <= 0.0f
                        && Mathf.Abs(checkedVector2i.y - floorTop) <= cOneWayPlatformThreshold + mOldPosition.y - position.y)
                {
                    groundY = floorTop;
                    mOnOneWayPlatform = true;
                }
            }

            //if we checked all the possible tiles and there's nothing right below the aabb
            if (checkedVector2i.x == bottomRight.x)
            {
                if (mOnOneWayPlatform)
                    return true;
                return false;
            }
        }

        //there's nothing right beneath the aabb
        return false;
    }

    /// <summary>
    /// Checks if the hero collides with a wall on the right.
    /// </summary>
    /// <returns>
    /// True if the hero collides with the wall on the right, otherwise false.
    /// </returns>
    /// <param name='wallX'>
    /// The X coordinate in world space of the left edge of the wall the hero collides with.
    /// </param>
    public bool CollidesWithRightWall(Vector2 position, out float wallX)
    {
        //make sure the aabb is up to date with the position
        var center = position + mAABBOffset;

        //init the wallX
        wallX = 0.0f;

        //calculate the bottom left and top left vertices of our aabb
        var bottomRight = center + new Vector2(mAABB.HalfSize.x, -mAABB.HalfSize.y) + Vector2.right;
        var topRight = bottomRight + new Vector2(0.0f, mAABB.HalfSize.y * 2.0f);

        //get the bottom right vertex's tile indices
        int tileIndexX, tileIndexY;

        //iterate over all the tiles that the object may collide with from the top to the bottom
        for (var checkedVector2i = bottomRight; checkedVector2i.y < topRight.y + Map.cTileSize; checkedVector2i.y += Map.cTileSize)
        {
            //make sure that we don't check beyound the top right corner
            checkedVector2i.y = Mathf.Min(checkedVector2i.y, topRight.y);

            mMap.GetMapTileAtPoint(checkedVector2i, out tileIndexX, out tileIndexY);

            if (tileIndexY < 0 || tileIndexY >= mMap.mHeight) return false;
            if (tileIndexX < 0 || tileIndexX >= mMap.mWidth) return false;

            //if the tile has another tile on the left, we can't touch the tile's left side because it's blocked
            if (!mMap.IsObstacle(tileIndexX - 1, tileIndexY))
            {
                //if the tile is not empty, then we hit the wall
                if (mMap.IsObstacle(tileIndexX, tileIndexY))
                {
                    //calculate the x position of the left side of the wall
                    wallX = (float)tileIndexX * Map.cTileSize - Map.cTileSize / 2.0f + mMap.position.x;
                    return true;
                }
            }

            //if we checked all the possible tiles and there's nothing right next to the aabb
            if (checkedVector2i.y == topRight.y)
                return false;
        }

        return false;
    }

    /// <summary>
    /// Checks if the hero collides with a wall on the left.
    /// </summary>
    /// <returns>
    /// True if the hero collides with the wall on the left, otherwise false.
    /// </returns>
    /// <param name='wallX'>
    /// The X coordinate in world space of the right edge of the wall the hero collides with.
    /// </param>
    public bool CollidesWithLeftWall(Vector2 position, out float wallX)
    {
        //make sure the aabb is up to date with the position
        var center = position + mAABBOffset;

        //init the wallX
        wallX = 0.0f;

        //calculate the bottom left and top left vertices of our mAABB.
        var bottomLeft = center - mAABB.HalfSize - Vector2.right;
        var topLeft = bottomLeft + new Vector2(0.0f, mAABB.HalfSize.y * 2.0f);

        //get the bottom left vertex's tile indices
        int tileIndexX, tileIndexY;

        //iterate over all the tiles that the object may collide with from the top to the bottom
        for (var checkedVector2i = bottomLeft; checkedVector2i.y < topLeft.y + Map.cTileSize; checkedVector2i.y += Map.cTileSize)
        {
            //make sure that we don't check beyound the top right corner
            checkedVector2i.y = Mathf.Min(checkedVector2i.y, topLeft.y);

            mMap.GetMapTileAtPoint(checkedVector2i, out tileIndexX, out tileIndexY);

            if (tileIndexY < 0 || tileIndexY >= mMap.mHeight) return false;
            if (tileIndexX < 0 || tileIndexX >= mMap.mWidth) return false;

            //if the tile has another tile on the right, we can't touch the tile's right side because it's blocked
            if (!mMap.IsObstacle(tileIndexX + 1, tileIndexY))
            {
                //if the tile is not empty, then we hit the wall
                if (mMap.IsObstacle(tileIndexX, tileIndexY))
                {
                    //calculate the x position of the right side of the wall
                    wallX = (float)tileIndexX * Map.cTileSize + Map.cTileSize / 2.0f + mMap.position.x;
                    return true;
                }
            }

            //if we checked all the possible tiles and there's nothing right next to the aabb
            if (checkedVector2i.y == topLeft.y)
                return false;
        }

        return false;
    }

    public bool ReachedNodeOnXAxis(Vector2 pathPosition, Vector2 prevDest, Vector2 currentDest)
    {
        return (prevDest.x <= currentDest.x && pathPosition.x >= currentDest.x)
            || (prevDest.x >= currentDest.x && pathPosition.x <= currentDest.x)
            || Mathf.Abs(pathPosition.x - currentDest.x) <= Constants.cBotMaxPositionError;
    }

    public bool ReachedNodeOnYAxis(Vector2 pathPosition, Vector2 prevDest, Vector2 currentDest)
    {
        return (prevDest.y <= currentDest.y && pathPosition.y >= currentDest.y)
            || (prevDest.y >= currentDest.y && pathPosition.y <= currentDest.y)
            || (Mathf.Abs(pathPosition.y - currentDest.y) <= Constants.cBotMaxPositionError);
    }

    public void GetContext(out Vector2 prevDest, out Vector2 currentDest, out Vector2 nextDest, out bool destOnGround, out bool reachedX, out bool reachedY)
    {
        prevDest = new Vector2(mPath[mCurrentNodeId - 1].x * Map.cTileSize + mMap.transform.position.x,
                                             mPath[mCurrentNodeId - 1].y * Map.cTileSize + mMap.transform.position.y);
        currentDest = new Vector2(mPath[mCurrentNodeId].x * Map.cTileSize + mMap.transform.position.x,
                                          mPath[mCurrentNodeId].y * Map.cTileSize + mMap.transform.position.y);
        nextDest = currentDest;

        if (mPath.Count > mCurrentNodeId + 1)
        {
            nextDest = new Vector2(mPath[mCurrentNodeId + 1].x * Map.cTileSize + mMap.transform.position.x,
                                          mPath[mCurrentNodeId + 1].y * Map.cTileSize + mMap.transform.position.y);
        }

        destOnGround = false;
        for (int x = mPath[mCurrentNodeId].x; x < mPath[mCurrentNodeId].x + mWidth; ++x)
        {
            if (mMap.IsGround(x, mPath[mCurrentNodeId].y - 1))
            {
                destOnGround = true;
                break;
            }
        }

        Vector2 pathPosition = mAABB.Center - mAABB.HalfSize + Vector2.one * Map.cTileSize * 0.5f;

        reachedX = ReachedNodeOnXAxis(pathPosition, prevDest, currentDest);
        reachedY = ReachedNodeOnYAxis(pathPosition, prevDest, currentDest);

        //snap the character if it reached the goal but overshot it by more than cBotMaxPositionError
        if (reachedX && Mathf.Abs(pathPosition.x - currentDest.x) > Constants.cBotMaxPositionError && Mathf.Abs(pathPosition.x - currentDest.x) < Constants.cBotMaxPositionError * 3.0f && !mPrevInputs[(int)KeyInput.GoRight] && !mPrevInputs[(int)KeyInput.GoLeft])
        {
            pathPosition.x = currentDest.x;
            mPosition.x = pathPosition.x - Map.cTileSize * 0.5f + mAABB.HalfSizeX + mAABBOffset.x;
        }

        if (destOnGround && !mOnGround)
            reachedY = false;
    }

    public void TestJumpValues()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            mFramesOfJumping = GetJumpFrameCount(1);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            mFramesOfJumping = GetJumpFrameCount(2);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            mFramesOfJumping = GetJumpFrameCount(3);
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            mFramesOfJumping = GetJumpFrameCount(4);
        else if (Input.GetKeyDown(KeyCode.Alpha5))
            mFramesOfJumping = GetJumpFrameCount(5);
        else if (Input.GetKeyDown(KeyCode.Alpha6))
            mFramesOfJumping = GetJumpFrameCount(6);
    }

    public int GetJumpFramesForNode(int prevNodeId)
    {
        int currentNodeId = prevNodeId + 1;

        if (mPath[currentNodeId].y - mPath[prevNodeId].y > 0 && mOnGround)
        {
            int jumpHeight = 1;
            for (int i = currentNodeId; i < mPath.Count; ++i)
            {
                if (mPath[i].y - mPath[prevNodeId].y >= jumpHeight)
                    jumpHeight = mPath[i].y - mPath[prevNodeId].y;
                if (mPath[i].y - mPath[prevNodeId].y < jumpHeight || mMap.IsGround(mPath[i].x, mPath[i].y - 1))
                    return GetJumpFrameCount(jumpHeight);
            }
        }

        return 0;
    }

#endregion

    /// <summary>
    /// Updates the moving object's physics, integrates the movement, updates sensors for terrain collisions.
    /// </summary>
    public void UpdatePhysics()
    {
        //assign the previous state of onGround, atCeiling, pushesRightWall, pushesLeftWall
        //before those get recalculated for this frame
        mWasOnGround = mOnGround;
        mPushedRightWall = mPushesRightWall;
        mPushedLeftWall = mPushesLeftWall;
        mWasAtCeiling = mAtCeiling;

        mOnOneWayPlatform = false;

        //save the speed to oldSpeed vector
        mOldSpeed = mSpeed;

        //save the position to the oldPosition vector
        mOldPosition = mPosition;

        //integrate the movement only if we're not tweening
        mPosition.x += Mathf.RoundToInt(mSpeed.x * Time.deltaTime);
        mPosition.y += Mathf.RoundToInt(mSpeed.y * Time.deltaTime);

        var checkAgainLeft = false;


        float groundY, ceilingY;
        float rightWallX = 0.0f, leftWallX = 0.0f;

        //if we overlap a tile on the left then align the hero
        if (mSpeed.x <= 0.0f && CollidesWithLeftWall(mPosition, out leftWallX))
        {
            if (mOldPosition.x - mAABB.HalfSize.x + mAABBOffset.x >= leftWallX)
            {
                mPosition.x = leftWallX + mAABB.HalfSize.x - mAABBOffset.x;
                mSpeed.x = Mathf.Max(mSpeed.x, 0.0f);

                mPushesLeftWall = true;
            }
            else
                checkAgainLeft = true;
        }
        else
            mPushesLeftWall = false;

        var checkAgainRight = false;

        //if we overlap a tile on the right then align the hero
        if (mSpeed.x >= 0.0f && CollidesWithRightWall(mPosition, out rightWallX))
        {
            if (mOldPosition.x + mAABB.HalfSize.x + mAABBOffset.x <= rightWallX)
            {
                mPosition.x = rightWallX - mAABB.HalfSize.x - mAABBOffset.x;
                mSpeed.x = Mathf.Min(mSpeed.x, 0.0f);

                mPushesRightWall = true;
            }
            else
                checkAgainRight = true;
        }
        else
            mPushesRightWall = false;

        //when we hit the ground
        //we can't hit the ground if our speed is positive
        if (HasGround(mPosition, out groundY) && mSpeed.y <= 0.0f
            && mOldPosition.y - mAABB.HalfSize.y + mAABBOffset.y >= groundY - 0.5f)
        {
            //calculate the y position on top of the ground
            mPosition.y = groundY + mAABB.HalfSize.y - mAABBOffset.y;

            //stop falling
            mSpeed.y = 0.0f;

            //we are on the ground now
            mOnGround = true;
        }
        else
            mOnGround = false;

        //check if the hero hit the ceiling
        if (HasCeiling(mPosition, out ceilingY) && mSpeed.y >= 0.0f
            && mOldPosition.y + mAABB.HalfSize.y + mAABBOffset.y + 1.0f <= ceilingY)
        {
            mPosition.y = ceilingY - mAABB.HalfSize.y - mAABBOffset.y - 1.0f;

            //stop going up
            mSpeed.y = 0.0f;

            mAtCeiling = true;
        }
        else
            mAtCeiling = false;

        //if we are colliding with the block but we don't know from which side we had hit him, just prioritize the horizontal alignment
        if (checkAgainLeft && !mOnGround && !mAtCeiling)
        {
            mPosition.x = leftWallX + mAABB.HalfSize.x;
            mSpeed.x = Mathf.Max(mSpeed.x, 0.0f);

            mPushesLeftWall = true;
        }
        else if (checkAgainRight && !mOnGround && !mAtCeiling)
        {
            mPosition.x = rightWallX - mAABB.HalfSize.x;
            mSpeed.x = Mathf.Min(mSpeed.x, 0.0f);

            mPushesRightWall = true;
        }

        //update the aabb
        mAABB.Center = mPosition + mAABBOffset;

        //apply the changes to the transform
        transform.position = new Vector3(Mathf.Round(mPosition.x), Mathf.Round(mPosition.y), mSpriteDepth);
    }


    private void Start()
    {
        mInputs = new bool[(int)KeyInput.Count];
        mPrevInputs = new bool[(int)KeyInput.Count];
        Gametimer = Time.time;

        playerDamage = GameObject.FindGameObjectWithTag("Player").GetComponent<DamageHandler>();
        //PlayerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

        BotInit();
    }

    public void ChangeAction(BotAction newAction)
    {
        mCurrentAction = newAction;
    }
    int GetJumpFrameCount(int deltaY)
    {
        if (deltaY <= 0)
            return 0;
        else
        {
            switch (deltaY)
            {
                case 1:
                    return 1;
                case 2:
                    return 2;
                case 3:
                    return 6;
                case 4:
                    return 9;
                case 5:
                    return 15;
                case 6:
                    return 21;
                default:
                    return 30;
            }
        }
    }

    public void UpdatePrevInputs()
    {
        var count = (byte)KeyInput.Count;

        for (byte i = 0; i < count; ++i)
            mPrevInputs[i] = mInputs[i];
    }

    public void BotUpdate()
	{

		//get the position of the bottom of the bot's aabb, this will be much more useful than the center of the sprite (mPosition)
		int tileX, tileY;
        var position = mAABB.Center;
        position.y -= mAABB.HalfSizeY;

		mMap.GetMapTileAtPoint(position, out tileX, out tileY);
		
		//int characterHeight = Mathf.CeilToInt(mAABB.HalfSizeY*2.0f/Map.cTileSize);

        switch (mCurrentAction)
        {
            case BotAction.None:

                TestJumpValues();

                if (mFramesOfJumping > 0)
                {
                    mFramesOfJumping -= 1;
                    mInputs[(int)KeyInput.Jump] = true;
                }

                break;

            case BotAction.MoveTo:

                Vector2 prevDest, currentDest, nextDest;
                bool destOnGround, reachedY, reachedX;
                GetContext(out prevDest, out currentDest, out nextDest, out destOnGround, out reachedX, out reachedY);
                Vector2 pathPosition = mAABB.Center - mAABB.HalfSize + Vector2.one * Map.cTileSize * 0.5f;

                mInputs[(int)KeyInput.GoRight] = false;
                mInputs[(int)KeyInput.GoLeft] = false;
                mInputs[(int)KeyInput.Jump] = false;
                mInputs[(int)KeyInput.GoDown] = false;

                if (pathPosition.y - currentDest.y > Constants.cBotMaxPositionError && mOnOneWayPlatform)
                    mInputs[(int)KeyInput.GoDown] = true;

                if (reachedX && reachedY)
                {
                    int prevNodeId = mCurrentNodeId;
                    mCurrentNodeId++;

                    if (mCurrentNodeId >= mPath.Count)
                    {
                        mCurrentNodeId = -1;
                        ChangeAction(BotAction.None);
                        break;
                    }

                    if (mOnGround)
                        mFramesOfJumping = GetJumpFramesForNode(prevNodeId);

                    goto case BotAction.MoveTo;
                }
                else if (!reachedX)
                {
                    if (currentDest.x - pathPosition.x > Constants.cBotMaxPositionError)
                        mInputs[(int)KeyInput.GoRight] = true;
                    else if (pathPosition.x - currentDest.x > Constants.cBotMaxPositionError)
                        mInputs[(int)KeyInput.GoLeft] = true;
                }
                else if (!reachedY && mPath.Count > mCurrentNodeId + 1 && !destOnGround)
                {
                    int checkedX = 0;

                    if (mPath[mCurrentNodeId + 1].x != mPath[mCurrentNodeId].x)
                    {
                        mMap.GetMapTileAtPoint(pathPosition, out tileX, out tileY);

                        if (mPath[mCurrentNodeId + 1].x > mPath[mCurrentNodeId].x)
                            checkedX = tileX + mWidth;
                        else
                            checkedX = tileX - 1;
                    }

                    if (checkedX != 0 && !mMap.AnySolidBlockInStripe(checkedX, tileY, mPath[mCurrentNodeId + 1].y))
                    {
                        if (nextDest.x - pathPosition.x > Constants.cBotMaxPositionError)
                            mInputs[(int)KeyInput.GoRight] = true;
                        else if (pathPosition.x - nextDest.x > Constants.cBotMaxPositionError)
                            mInputs[(int)KeyInput.GoLeft] = true;

                        if (ReachedNodeOnXAxis(pathPosition, currentDest, nextDest) && ReachedNodeOnYAxis(pathPosition, currentDest, nextDest))
                        {
                            mCurrentNodeId += 1;
                            goto case BotAction.MoveTo;
                        }
                    }
                }

                if (mFramesOfJumping > 0 &&
                    (!mOnGround || (reachedX && !destOnGround) || (mOnGround && destOnGround)))
                {
                    mInputs[(int)KeyInput.Jump] = true;
                    if (!mOnGround)
                        --mFramesOfJumping;
                }

                if (mPosition == mOldPosition)
                {
                    ++mStuckFrames;
                    if (mStuckFrames > cMaxStuckFrames)
                        MoveTo(mPath[mPath.Count - 1]);
                }
                else
                    mStuckFrames = 0;

                break;
        }
			
        
        if (gameObject.activeInHierarchy)
		    CharacterUpdate();
	}

    public void CharacterUpdate()
    {
        switch (mCurrentState)
        {
            case CharacterState.Stand:

                mWalkSfxTimer = cWalkSfxTime;
                mAnimator.Play("Stand");

                mSpeed = Vector2.zero;

                if (!mOnGround)
                {
                    mCurrentState = CharacterState.Jump;
                    break;
                }

                //if left or right key is pressed, but not both
                if (mInputs[(int)KeyInput.GoRight] != mInputs[(int)KeyInput.GoLeft])
                {
                    mCurrentState = CharacterState.Run;
                }
                else if (mInputs[(int)KeyInput.Jump])
                {
                    mSpeed.y = mJumpSpeed;
                    mAudioSource.PlayOneShot(mJumpSfx);
                    mCurrentState = CharacterState.Jump;
                }

                if (mInputs[(int)KeyInput.GoDown] && mOnOneWayPlatform)
                    mPosition -= Vector2.up * cOneWayPlatformThreshold;

                break;
            case CharacterState.Run:

                mAnimator.Play("Walk");

                mWalkSfxTimer += Time.deltaTime;

                if (mWalkSfxTimer > cWalkSfxTime)
                {
                    mWalkSfxTimer = 0.0f;
                    mAudioSource.PlayOneShot(mWalkSfx);
                }

                //if both or neither left nor right keys are pressed then stop walking and stand

                if (mInputs[(int)KeyInput.GoRight] == mInputs[(int)KeyInput.GoLeft])
                {
                    mCurrentState = CharacterState.Stand;
                    mSpeed = Vector2.zero;
                }
                else if (mInputs[(int)KeyInput.GoRight])
                {
                    mSpeed.x = mWalkSpeed;
                    transform.localScale = new Vector3(-mScale.x, mScale.y, 1.0f);
                }
                else if (mInputs[(int)KeyInput.GoLeft])
                {
                    mSpeed.x = -mWalkSpeed;
                    transform.localScale = new Vector3(mScale.x, mScale.y, 1.0f);
                }

                //if there's no tile to walk on, fall
                if (mInputs[(int)KeyInput.Jump])
                {

                    mSpeed.y = mJumpSpeed;
                    mAudioSource.PlayOneShot(mJumpSfx, 1.0f);
                    mCurrentState = CharacterState.Jump;
                }
                else if (!mOnGround)
                {
                    mCurrentState = CharacterState.Jump;
                    break;
                }

                //don't move left when pushing left wall
                if (mPushesLeftWall)
                    mSpeed.x = Mathf.Max(mSpeed.x, 0.0f);
                //don't move right when pushing right wall
                else if (mPushesRightWall)
                    mSpeed.x = Mathf.Min(mSpeed.x, 0.0f);

                break;
            case CharacterState.Jump:

                mWalkSfxTimer = cWalkSfxTime;

                mAnimator.Play("Jump");

                HandleJumping();


                //if we hit the ground
                if (mOnGround)
                {
                    //if there's no movement change state to standing
                    if (mInputs[(int)KeyInput.GoRight] == mInputs[(int)KeyInput.GoLeft])
                    {
                        mCurrentState = CharacterState.Stand;
                        mSpeed = Vector2.zero;
                    }
                    else	//either go right or go left are pressed so we change the state to walk
                    {
                        mCurrentState = CharacterState.Run;
                        mSpeed.y = 0.0f;
                    }
                }
                break;
        }

        if ((!mWasOnGround && mOnGround)
            || (!mWasAtCeiling && mAtCeiling)
            || (!mPushedLeftWall && mPushesLeftWall)
            || (!mPushedRightWall && mPushesRightWall))
            mAudioSource.PlayOneShot(mHitWallSfx, 0.5f);

        UpdatePhysics();

        if (mWasOnGround && !mOnGround)
            mFramesFromJumpStart = 0;

        UpdatePrevInputs();
    }

    private void HandleJumping()
    {
        //increase the number of frames that we've been in the jump state
        ++mFramesFromJumpStart;

        mFramesFromJumpStart = 100;

        //if we hit the ceiling, we don't want to compensate pro jumping, we can prevent by faking a huge mFramesFromJumpStart
        if (mAtCeiling)
            mFramesFromJumpStart = 100;

        //if we're jumping/falling then apply the gravity
        //this should be applied at the beginning of the jump routine
        //because this way we can assure that when we hit the ground 
        //the speed.y will not change after we zero it
        mSpeed.y += Constants.cGravity * Time.deltaTime;

        mSpeed.y = Mathf.Max(mSpeed.y, Constants.cMaxFallingSpeed);

        if (!mInputs[(int)KeyInput.Jump] && mSpeed.y > 0.0f)
        {
            mSpeed.y = Mathf.Min(mSpeed.y, 200.0f);
            mFramesFromJumpStart = 100;
        }

        //in air movement
        //if both or none horizontal movement keys are pressed
        if (mInputs[(int)KeyInput.GoRight] == mInputs[(int)KeyInput.GoLeft])
        {
            mSpeed.x = 0.0f;
        }
        else if (mInputs[(int)KeyInput.GoRight])	//if right key is pressed then accelerate right
        {
            transform.localScale = new Vector3(-mScale.x, mScale.y, 1.0f);
            mSpeed.x = mWalkSpeed;

            //..W
            //.H.     <- to not get stuck in these kind of situations we beed to advance
            //..W			the hero forward if he doesn't push a wall anymore
            if (mPushedRightWall && !mPushesRightWall)
                mPosition.x += 1.0f;
        }
        else if (mInputs[(int)KeyInput.GoLeft])	//if left key is pressed then accelerate left
        {
            transform.localScale = new Vector3(mScale.x, mScale.y, 1.0f);
            mSpeed.x = -mWalkSpeed;

            //W..
            //.H.     <- to not get stuck in these kind of situations we need to advance
            //W..			the hero forward if he doesn't push a wall anymore
            if (mPushedLeftWall && !mPushesLeftWall)
                mPosition.x -= 1.0f;
        }

        //if we just started falling and want to jump, then jump anyway
        if (mInputs[(int)KeyInput.Jump] && (mOnGround || (mSpeed.y < 0.0f && mFramesFromJumpStart < Constants.cJumpFramesThreshold)))
            mSpeed.y = mJumpSpeed;
    }

    Vector2 GameCordstoTilecords(Vector2 Gamecords)
    {
        Vector2 Tilecords = Vector2.zero;
        Tilecords.x = Mathf.RoundToInt((Gamecords.x - 32) / 16);
        Tilecords.y = Mathf.RoundToInt((Gamecords.y - 8) / 16);
        return Tilecords;
    }

    void Checkifplayerisinrange()
    {
        if (playerpos != null)
        {
            if (Mathf.Abs(this.gameObject.transform.position.x - playerpos.position.x) <= Botattackdist && Mathf.Abs(this.gameObject.transform.position.y - playerpos.position.y) <= Botattackdist)
            {
                playerDamage.Takedamageplayer();
            }
        }
    }

    void PlayerFollow()
    {
        //if (Mathf.Abs(this.gameObject.transform.position.x - playerpos.position.x) <= BotFollowRange && Mathf.Abs(this.gameObject.transform.position.y - playerpos.position.y) <= BotFollowRange)
        //{
        if (playerpos != null)
        {
            int x = Mathf.RoundToInt(GameCordstoTilecords(playerpos.position).x);
            int y = Mathf.RoundToInt(GameCordstoTilecords(playerpos.position).y);
            TappedOnTile(new Vector2i(x+2, y));
        }
        //}
    }

    void MakeZombieChasePlayer()
    {
        if (Time.time > Gametimer + 1)
        {
            Gametimer = Time.time;
            seconds++;
        }

        if (seconds == UpdateRate)
        {
            seconds = 0;
            FollowPlayer = true;
        }

        if (FollowPlayer)
        {
            PlayerFollow();
            FollowPlayer = false;
        }
    }

    private void Update()
    {
        MakeZombieChasePlayer();
        Checkifplayerisinrange();
        BotUpdate();
    }
}