using UnityEngine;
using System.Collections;

/// <summary>
/// Level manager.
/// 
/// </summary>

public class LevelManager : MonoBehaviour {
	//Public to allow manipulation from within the editor
	public float tileSize = 1.0f;
	public int chunkSize = 10;
	public int numChunksX = 1;
	public int numChunksZ = 1;
	public int heightVariability = 4;
	public float terrainBumpiness = 0.1f;
	public GameObject chunkPrefab;
	public int biomeThickness = 1;
	public GameObject waterTablePrefab;
	public float waterTableLevel = 2f;
	public float layerHeight = 0.5f;

	//Used to plot the vertices in the mesh
	private int xSize = 1;
	private int zSize = 1;
	private WorldGrid worldGrid;
	private float[,] sharedVertHeights;
	private int[] intVertHeights;
	private GameObject waterTable;

	private GameObject[,] chunkArray;
    private int xCursor = 30;
    private int zCursor = 30;
    private int targetEditHeight = 10;

    public enum TerrainEdit { up, down, level };

	// Use this for initialization
	void Start () {
		xSize = chunkSize * numChunksX;
		zSize = chunkSize * numChunksZ;
		worldGrid = new WorldGrid (xSize, zSize);
		worldGrid.CreateHeightMap (heightVariability, terrainBumpiness);
		BuildSharedVertHeights ();
		BuildChunks ();
		BuildWaterTable ();
	}

	private void BuildWaterTable(){
		//Planes are size 10 units by default, so reduce to a size of one to begin with.
		float waterXScale = 0.1f;
		float waterZScale = 0.1f;
		waterXScale = waterXScale * xSize * tileSize;
		waterZScale = waterZScale * zSize * tileSize;
		Vector3 waterPos = new Vector3 ((float)(xSize * tileSize / 2), (float)(waterTableLevel * tileSize * 0.5f), (float)(zSize * tileSize / 2));
		waterTable = Instantiate (waterTablePrefab, waterPos, Quaternion.identity) as GameObject;
		waterTable.transform.localScale = new Vector3 (waterXScale, 1.0f, waterZScale);
	}

	//Shared vert heights are the vertices on the corner of four tiles
	void BuildSharedVertHeights(){
		int numTiles = xSize * zSize;
		int numTris = numTiles * 2;
		int numVerts = numTiles * 6;
		
		//Shared vert array is one bigger in both dimensions
		sharedVertHeights = new float[xSize + 1, zSize + 1];
		//intHeightArray is used for deciding which texture applies to which tri
		intVertHeights = new int[numVerts];
		//Loop through every vert
		int x, z;

		x = z = 0;
		
		for (x = 0; x < xSize+1; x++) {
			for (z = 0; z < zSize+1; z++) {
				//find the highest point adjacent to that tile
				float yHeight = GetHighestAdjacentTile(x, z);
				sharedVertHeights[x, z] = yHeight;
			}
		}
	}

    //Loop through potentially changed parts of the world map, and raise any tiles that are surrounded by higher tiles
    void RemoveDivots(int xFocus, int zFocus)
    {
        int boundedX = BoundIntToLevel(xFocus, true);
        int boundedZ = BoundIntToLevel(zFocus, false);

        int minX = xFocus - chunkSize;
        int minZ = zFocus - chunkSize;
        int maxX = xFocus + chunkSize + 1;
        int maxZ = zFocus + chunkSize + 1;

        minX = BoundIntToLevel(minX, true);
        minZ = BoundIntToLevel(minZ, false);
        maxX = BoundIntToLevel(maxX, true);
        maxZ = BoundIntToLevel(maxZ, false);

        worldGrid.RemoveDivots(minX, minZ, maxX, maxZ);

    }

	//Only rebuild the affected verts after editing terrain
	void RebuildSharedVertHeights(int xFocus, int zFocus){
		int numTiles = xSize * zSize;
		int numTris = numTiles * 2;
		int numVerts = numTiles * 6;

        int boundedX = BoundIntToLevel( xFocus, true);
        int boundedZ = BoundIntToLevel(zFocus, false);

        int minX = xFocus - chunkSize;
		int minZ = zFocus - chunkSize;
		int maxX = xFocus + chunkSize + 1;
		int maxZ = zFocus + chunkSize + 1;

        minX = BoundIntToLevel(minX, true);
        minZ = BoundIntToLevel(minZ, false);
        maxX = BoundIntToLevel(maxX, true);
        maxZ = BoundIntToLevel(maxZ, false);

		//intHeightArray is used for deciding which texture applies to which tri
		intVertHeights = new int[numVerts];
		//Loop through every vert
		int x, z;
		
		x = minX;
		z = minZ;
		
		for (x = minX; x < maxX; x++) {
			for (z = minZ; z < maxZ; z++) {
				//find the highest point adjacent to that tile
				float yHeight = GetHighestAdjacentTile(x, z);
				sharedVertHeights[x, z] = yHeight;
			}
		}
	}

	//Make sure an integer is within the x and y limits of the level size
    public int BoundIntToLevel(int coord, bool xCoord)
    {
        int boundedCoord = coord;
        if (xCoord == true)
        {
            if (boundedCoord < 1) boundedCoord = 1;
            if (boundedCoord > xSize - 1) boundedCoord = xSize - 1;
        }
        else
        {
            if (boundedCoord < 1) boundedCoord = 1;
            if (boundedCoord > zSize - 1) boundedCoord = zSize - 1;
        }
        return boundedCoord;
    }

	//Look at the adjecent 4 tiles to find the highest
	//The vert array is one larger in the x and z direction than the tile array
	//Vert x, y is adjacent to tiles x,y  x-1, y   x, y-1    x-1, y-1
	//Tile x, y is bounded by the four verts  x, y   x+1, y    x, y+1   x+1, y+1
	private float GetHighestAdjacentTile(int x, int z){
		float height = 0.0f;
		
		//If the bottom left is in the bounds
		if ((x > 0) && (z > 0) && (x < xSize) && (z < zSize)) {
			height = ReturnHighest (height, x, z);
			
			//These make it check all adjacent, rather than just one.
			height = ReturnHighest(height, x-1, z);
			height = ReturnHighest(height, x, z-1);
			height = ReturnHighest(height, x-1, z-1);
		}
		return height * tileSize * layerHeight;
	}
	
	private float ReturnHighest(float h, int x, int z){
		if (h > worldGrid.GetTile(x, z))
			return h;
		else 
			return worldGrid.GetTile(x, z);
	}

	//Build the invividual chunks that form the terrain
	private void BuildChunks(){
		chunkArray = new GameObject[numChunksX,numChunksZ];
		int x, z;
		x = z = 0;
		for (x = 0; x < numChunksX; x++) {
			for (z = 0; z < numChunksZ; z++) {
			//	Vector3 chunkPos = new Vector3((float)(x * chunkSize * tileSize), 0f, (float)(z*chunkSize*tileSize));
				Vector3 chunkPos = new Vector3(0f, 0f, 0f);
				GameObject newChunk = Instantiate(chunkPrefab, chunkPos, this.transform.rotation) as GameObject;
				TerrainChunkManager chunkManager = newChunk.GetComponent("TerrainChunkManager") as TerrainChunkManager;
				chunkManager.Initialise(chunkSize, chunkSize, x*chunkSize, z*chunkSize, tileSize, worldGrid, sharedVertHeights, biomeThickness);
				chunkArray[x, z] = newChunk;
			}
		}
	}

	public TerrainChunkManager GetChunkManagerAtCoord(int x, int z){
		if ((x > numChunksX) || (z > numChunksZ))
			return null;
		else {
			return chunkArray [x, z].GetComponent ("TerrainChunkManager") as TerrainChunkManager;
		}
	}

	//Edit terrain at a certain location. Update the height map.
	//Update the appropriate chunk and the chunks surrounding it.
    //Need to add brush size and target height as variables. Brush size pretty easy, subtract from min and add to max.
    //Add target height to mouse manager. Store during on mouse down.
	public void EditTerrain(int x, int z, TerrainEdit terrainEdit){
		int xChunk = (int)Mathf.Floor (x / chunkSize);
		int zChunk = (int)Mathf.Floor (z / chunkSize);
        bool worldEdited = false;

        int brushSize = 2;

        int brushX = x;
        int brushZ = z;

        for (brushX = x - brushSize; brushX < x + brushSize; brushX++)
        {
            for (brushZ = z - brushSize; brushZ < z + brushSize; brushZ++)
            {
                switch (terrainEdit)
                {
                    case TerrainEdit.up:
                        {
                            worldGrid.SmoothTerrainTopDown(x, z, WorldGrid.Directions.C, 20);
                            worldEdited = true;
                            break;
                        }
                    case TerrainEdit.down:
                        {
                            worldGrid.SmoothTerrainBottomUp(x, z, WorldGrid.Directions.C, 10);
                            worldEdited = true;
                            break;
                        }
                    case TerrainEdit.level:
                        {
                            //Bit of a hack. Because of the way terrain is represented (favouring the heighest adjacent tile) make the brush size for raising terrain
                            //one smaller than for lowering terrain.
                            if ((brushX >( x - brushSize)) && (brushX < (x + brushSize)) && (brushZ > (z - brushSize)) && (brushZ < (z + brushSize)))
                            {
                                if (worldGrid.GetTile(brushX, brushZ) < targetEditHeight)
                                {
                                    worldGrid.SmoothTerrainTopDown(brushX, brushZ, WorldGrid.Directions.C, targetEditHeight + 1);
                                    worldEdited = true;
                                }
                            }
                            else if (worldGrid.GetTile(brushX, brushZ) > targetEditHeight)
                            {
                                worldGrid.SmoothTerrainBottomUp(brushX, brushZ, WorldGrid.Directions.C, targetEditHeight - 1);
                                worldEdited = true;
                            }
                            break;
                        }
                }
            }
        }
        if (worldEdited == true)
        {
         //   RemoveDivots(x, z);
            RebuildSharedVertHeights(x, z);

            //Update the central chunk
            GetChunkManagerAtCoord(xChunk, zChunk).RebuildChunk();
            //Update the four cartesian chunks if they are within bounds
            if (xChunk > 0)
                GetChunkManagerAtCoord(xChunk - 1, zChunk).RebuildChunk();
            if (xChunk < numChunksX - 1)
                GetChunkManagerAtCoord(xChunk + 1, zChunk).RebuildChunk();
            if (zChunk > 0)
                GetChunkManagerAtCoord(xChunk, zChunk - 1).RebuildChunk();
            if (zChunk < numChunksZ - 1)
                GetChunkManagerAtCoord(xChunk, zChunk + 1).RebuildChunk();
            //Update the diagonal chunks if they are within bounds
            if ((xChunk > 0) && (zChunk > 0))
                GetChunkManagerAtCoord(xChunk - 1, zChunk - 1).RebuildChunk();
            if ((xChunk > 0) && (zChunk < numChunksZ - 1))
                GetChunkManagerAtCoord(xChunk - 1, zChunk + 1).RebuildChunk();
            if ((xChunk < numChunksX - 1) && (zChunk > 0))
                GetChunkManagerAtCoord(xChunk + 1, zChunk - 1).RebuildChunk();
            if ((xChunk < numChunksX - 1) && (zChunk < numChunksZ - 1))
                GetChunkManagerAtCoord(xChunk + 1, zChunk + 1).RebuildChunk();
        }
	}
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown ("u")) {
			EditTerrain(xCursor, zCursor, TerrainEdit.up);
		}
        if (Input.GetKeyDown("j"))
        {
            EditTerrain(xCursor, zCursor, TerrainEdit.down);
        }
        if (Input.GetKeyDown("y"))
        {
            EditTerrain(xCursor, zCursor, TerrainEdit.level);
        }

        if (Input.GetKeyDown("h")) xCursor++;
        if (Input.GetKeyDown("f")) xCursor--;
        if (Input.GetKeyDown("t")) zCursor++;
        if (Input.GetKeyDown("g")) zCursor--;
        if (Input.GetKeyDown("i")) targetEditHeight++;
        if (Input.GetKeyDown("k")) targetEditHeight--;
        xCursor = BoundIntToLevel(xCursor, true);
        zCursor = BoundIntToLevel(zCursor, true);
    }

    //Takes the initial click position to determine the height to level to, and the current cursor position to determine
    //the current tile to edit
    public void LevelTerrainAtPosition(Vector3 pos, Vector3 posInitial)
    {
        targetEditHeight = (int)Mathf.Floor((posInitial.y / (tileSize * layerHeight)));
        int gridCoordX = (int)(pos.x / tileSize);
        int gridCoordZ = (int)(pos.z / tileSize);
        EditTerrain(gridCoordX, gridCoordZ, TerrainEdit.level);
    }
}
