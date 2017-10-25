using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Algorithms;
using CLUTCH;
using CLUTCHINPUT;
using UnityEngine.UI;

[System.Serializable]
public enum TileType
{
    Empty,
    Block,
    OneWay
}

[System.Serializable]
public partial class Map : MonoBehaviour 
{
	
	/// <summary>
	/// The map's position in world space. Bottom left corner.
	/// </summary>
	public Vector3 position;
	/// <summary>
	/// The base tile sprite prefab that populates the map.
	/// Assigned in the inspector.
	/// </summary>
	public SpriteRenderer tilePrefab;
    public bool BotTakenControl;
	
	/// <summary>
	/// The path finder.
	/// </summary>
	public PathFinderFast mPathFinder;
	
	/// <summary>
	/// The nodes that are fed to pathfinder.
	/// </summary>
	[HideInInspector]
	public byte[,] mGrid;
	
	/// <summary>
	/// The map's tile data.
	/// </summary>
	[HideInInspector]
	public TileType[,] tiles;

	/// <summary>
	/// The map's sprites.
	/// </summary>
	private SpriteRenderer[,] tilesSprites;
	
	/// <summary>
	/// A parent for all the sprites. Assigned from the inspector.
	/// </summary>
	public Transform mSpritesContainer;
	
	/// <summary>
	/// The size of a tile in pixels.
	/// </summary>
	static public int cTileSize = 16;
	
	/// <summary>
	/// The width of the map in tiles.
	/// </summary>
	public int mWidth = 50;
	/// <summary>
	/// The height of the map in tiles.
	/// </summary>
	public int mHeight = 42;

    public Texture2D levelmap;
    public Camera gameCamera;
    public GameObject[] Bots;
    public GameObject Player;
    public Bot BotBava;

    bool[] inputs;
    bool[] prevInputs;

    public TileType GetTile(int x, int y) 
	{
        if (x < 0 || x >= mWidth
            || y < 0 || y >= mHeight)
            return TileType.Block;

		return tiles[x, y]; 
	}

    public bool IsOneWayPlatform(int x, int y)
    {
        if (x < 0 || x >= mWidth
            || y < 0 || y >= mHeight)
            return false;

        return (tiles[x, y] == TileType.OneWay);
    }

    public bool IsGround(int x, int y)
    {
        if (x < 0 || x >= mWidth
           || y < 0 || y >= mHeight)
            return false;

        return (tiles[x, y] == TileType.OneWay || tiles[x, y] == TileType.Block);
    }

    public bool IsObstacle(int x, int y)
    {
        if (x < 0 || x >= mWidth
            || y < 0 || y >= mHeight)
            return true;

        return (tiles[x, y] == TileType.Block);
    }

    public bool IsNotEmpty(int x, int y)
    {
        if (x < 0 || x >= mWidth
            || y < 0 || y >= mHeight)
            return true;

        return (tiles[x, y] != TileType.Empty);
    }

	public void InitPathFinder()
	{
		mPathFinder = new PathFinderFast(mGrid, this);
		
		mPathFinder.Formula                 = HeuristicFormula.Manhattan;
		//if false then diagonal movement will be prohibited
        mPathFinder.Diagonals               = false;
		//if true then diagonal movement will have higher cost
        mPathFinder.HeavyDiagonals          = false;
		//estimate of path length
        mPathFinder.HeuristicEstimate       = 6;
        mPathFinder.PunishChangeDirection   = false;
        mPathFinder.TieBreaker              = false;
        mPathFinder.SearchLimit             = 1000000;
        mPathFinder.DebugProgress           = false;
        mPathFinder.DebugFoundPath          = false;
	}
	
	public void GetMapTileAtPoint(Vector2 point, out int tileIndexX, out int tileIndexY)
	{
		tileIndexY =(int)((point.y - position.y + cTileSize/2.0f)/(float)(cTileSize));
		tileIndexX =(int)((point.x - position.x + cTileSize/2.0f)/(float)(cTileSize));
	}
	
	public Vector2i GetMapTileAtPoint(Vector2 point)
	{
		return new Vector2i((int)((point.x - position.x + cTileSize/2.0f)/(float)(cTileSize)),
					(int)((point.y - position.y + cTileSize/2.0f)/(float)(cTileSize)));
	}
	
	public Vector2 GetMapTilePosition(int tileIndexX, int tileIndexY)
	{
		return new Vector2(
				(float) (tileIndexX * cTileSize) + position.x,
				(float) (tileIndexY * cTileSize) + position.y
			);
	}

	public Vector2 GetMapTilePosition(Vector2i tileCoords)
	{
		return new Vector2(
			(float) (tileCoords.x * cTileSize) + position.x,
			(float) (tileCoords.y * cTileSize) + position.y
			);
	}
	
	public bool CollidesWithMapTile(AABB aabb, int tileIndexX, int tileIndexY)
	{
		var tilePos = GetMapTilePosition (tileIndexX, tileIndexY);
		
		return aabb.Overlaps(tilePos, new Vector2( (float)(cTileSize)/2.0f, (float)(cTileSize)/2.0f));
	}

    public bool AnySolidBlockInRectangle(Vector2 start, Vector2 end)
    {
        return AnySolidBlockInRectangle(GetMapTileAtPoint(start), GetMapTileAtPoint(end));
    }

    public bool AnySolidBlockInStripe(int x, int y0, int y1)
    {
        int startY, endY;

        if (y0 <= y1)
        {
            startY = y0;
            endY = y1;
        }
        else
        {
            startY = y1;
            endY = y0;
        }

        for (int y = startY; y <= endY; ++y)
        {
            if (GetTile(x, y) == TileType.Block)
                return true;
        }

        return false;
    }

    public bool AnySolidBlockInRectangle(Vector2i start, Vector2i end)
    {
        int startX, startY, endX, endY;

        if (start.x <= end.x)
        {
            startX = start.x;
            endX = end.x;
        }
        else
        {
            startX = end.x;
            endX = start.x;
        }

        if (start.y <= end.y)
        {
            startY = start.y;
            endY = end.y;
        }
        else
        {
            startY = end.y;
            endY = start.y;
        }

        for (int y = startY; y <= endY; ++y)
        {
            for (int x = startX; x <= endX; ++x)
            {
                if (GetTile(x, y) == TileType.Block)
                    return true;
            }
        }

        return false;
    }

    public void SetTile(int x, int y, TileType type)
    {
        if (x <= 1 || x >= mWidth - 2 || y <= 1 || y >= mHeight - 2)
            return;

        tiles[x, y] = type;

        if (type == TileType.Block)
        {
            tilesSprites[x, y] = Instantiate<SpriteRenderer>(tilePrefab);
            tilesSprites[x, y].transform.parent = transform;
            tilesSprites[x, y].gameObject.layer = 8;
            tilesSprites[x, y].transform.position = position + new Vector3(cTileSize * x, cTileSize * y, 10.0f);
            DamageHandler damage = tilesSprites[x, y].gameObject.GetComponent<DamageHandler>();
            damage.x = x;
            damage.y = y;
            damage.mapdata = this;

            ClickableTile clickedthis = tilesSprites[x, y].gameObject.GetComponent<ClickableTile>();
            clickedthis.TileX = x;
            clickedthis.TileY = y;
            
            mGrid[x, y] = 0;
            AutoTile(type, x, y);
            tilesSprites[x, y].enabled = true;
        }
        else if (type == TileType.OneWay)
        {
            tilesSprites[x, y] = Instantiate<SpriteRenderer>(tilePrefab);
            tilesSprites[x, y].transform.parent = transform;
            tilesSprites[x, y].gameObject.layer = 8;
            tilesSprites[x, y].transform.position = position + new Vector3(cTileSize * x, cTileSize * y, 10.0f);

            DamageHandler damage = tilesSprites[x, y].gameObject.GetComponent<DamageHandler>();
            damage.x = x;
            damage.y = y;
            damage.mapdata = this;

            mGrid[x, y] = 1;
            tilesSprites[x, y].enabled = true;

            tilesSprites[x, y].transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            tilesSprites[x, y].transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
            tilesSprites[x, y].sprite = mDirtSprites[2];
            
        }
        else
        {
            mGrid[x, y] = 1;
        }
        
        AutoTile(type, x - 1, y);
        AutoTile(type, x + 1, y);
        AutoTile(type, x, y - 1);
        AutoTile(type, x, y + 1);
       
    }

    public void Start()
    {
        //Application.targetFrameRate = 60;

        //Player = GameObject.FindGameObjectWithTag("Player");
        //BotBava = Player.GetComponent<Bot>();


        //set the position
        position = transform.position;
        mWidth = levelmap.width;
        mHeight = levelmap.height;

        tiles = new TileType[mWidth, mHeight];
        tilesSprites = new SpriteRenderer[levelmap.width, levelmap.height];

        mGrid = new byte[Mathf.NextPowerOfTwo((int)mWidth), Mathf.NextPowerOfTwo((int)mHeight)];
        InitPathFinder();

        //Camera.main.orthographicSize = Camera.main.pixelHeight / 6f;

        //get raw pixels
        Color32[] allpixels = levelmap.GetPixels32();

        for (int y = 0; y < mHeight; ++y)
        {
            for (int x = 0; x < mWidth; ++x)
            {

                //SpawnTileat(allpixels[(y * width) + x], x, y);
                Color32 c = allpixels[(y * mWidth) + x];

                //check transparent pixels
                if (c.a <= 0)
                {
                    SetTile(x, y, TileType.Empty);
                }
                else if ((c.a == 255) && (c.r == 255) && (c.g == 0) && (c.b == 255))
                {
                    SetTile(x, y, TileType.OneWay);
                }
                else
                {
                    SetTile(x, y, TileType.Block);
                }
            }
        }

        for (int y = 0; y < mHeight; ++y)
        {
            tiles[1, y] = TileType.Block;
            tiles[mWidth - 2, y] = TileType.Block;
        }

        for (int x = 0; x < mWidth; ++x)
        {
            tiles[x, 1] = TileType.Block;
            tiles[x, mHeight - 2] = TileType.Block;
        }

       // BotBava.mMap = this;
        //BotBava.mPosition = new Vector2(2 * Map.cTileSize, (mHeight / 2) * Map.cTileSize + BotBava.mAABB.HalfSizeY);

    }



    void Update()
    {
        //cameraBoundUpper = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, Camera.main.pixelWidth));
        //cameraBoundLower = Camera.main.ScreenToWorldPoint(new Vector3(0, 0));
    }

    void AutoTile(TileType type, int x, int y)
    {
        if (x >= mWidth || x < 0 || y >= mHeight || y < 0)
            return;

        if (tiles[x, y] != TileType.Block)
            return;

        int tileOnLeft = tiles[x - 1, y] == tiles[x, y] ? 1 : 0;
        int tileOnRight = tiles[x + 1, y] == tiles[x, y] ? 1 : 0;
        int tileOnTop = tiles[x, y + 1] == tiles[x, y] ? 1 : 0;
        int tileOnBottom = tiles[x, y - 1] == tiles[x, y] ? 1 : 0;

        float scaleX = 1.0f;
        float scaleY = 1.0f;
        float rot = 0.0f;
        int id = 0;

        int sum = tileOnLeft + tileOnRight + tileOnTop + tileOnBottom;

        switch (sum)
        {
            case 0:
                break;

            case 1:
                if (tileOnRight == 1)
                {
                    scaleX = 1;
                    rot = -1;
                }
                else if (tileOnTop == 1)
                {
                    scaleY = -1;
                    scaleX = 1.0f;
                    rot = 0;

                }
                else if (tileOnLeft == 1)
                {
                    scaleX = 1.0f;
                    scaleY = 1.0f;
                    rot = 1;
                }

                break;
            case 2:

                if (tileOnLeft + tileOnBottom == 2)
                {
                    scaleY = 1.0f;
                    scaleX = 1.0f;
                    rot = -1;
                }
                else if (tileOnRight + tileOnBottom == 2)
                {
                    scaleY = 1.0f;
                    scaleX = 1.0f;
                    rot = 1;
                }
                else if (tileOnTop + tileOnLeft == 2)
                {
                    scaleY = 1.0f;
                    scaleX = 1.0f;
                    rot = -1;
                }
                else if (tileOnTop + tileOnRight == 2)
                {
                    scaleY = 1.0f;
                    scaleX = 1.0f;
                    rot = 1;
                }
                else if (tileOnTop + tileOnBottom == 2)
                {
                    scaleY = 1.0f;
                    scaleX = 1.0f;
                    rot = 0;
                    id = 1;
                }
                else if (tileOnRight + tileOnLeft == 2)
                {
                    scaleY = 1.0f;
                    scaleX = 1.0f;
                    rot = 0;
                }
                break;
            case 3:
                if (tileOnLeft == 0)
                {
                    scaleY = 1.0f;
                    scaleX = 1.0f;
                    rot = 1;

                }
                else if (tileOnRight == 0)
                {
                    scaleY = 1.0f;
                    scaleX = 1.0f;
                    rot = -1;
                }
                else if (tileOnBottom == 0)
                {
                    scaleY = -1.0f;
                    scaleX = 1.0f;
                    rot = 0;
                }
                else if (tileOnTop == 0)
                {
                    scaleY = 1.0f;
                    scaleX = 1.0f;
                    rot = 0;
                }

                break;

            case 4:
                id = 1;
                //tile.GetComponent<SpriteRenderer>().sprite = basetilesprite;
                break;
        }
        if (tilesSprites[x, y]!= null)
        {
            tilesSprites[x, y].transform.localScale = new Vector3(scaleX, scaleY, 1.0f);
            tilesSprites[x, y].transform.eulerAngles = new Vector3(0.0f, 0.0f, rot * 90.0f);
            tilesSprites[x, y].sprite = mDirtSprites[0 + id];
        }
    }

    public List<Sprite> mDirtSprites;
}
