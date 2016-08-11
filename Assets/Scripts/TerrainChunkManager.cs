using UnityEngine;
using System.Collections;

/// <summary>
/// Terrain chunk manager.
/// Builds the mesh for the terrain chunk. This is the rendered
/// and physically simuluated element of the terrain
/// </summary>
/// 
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class TerrainChunkManager : MonoBehaviour {
	private int xSize = 10;
	private int zSize = 10;
	private float tileSize = 1.0f;
	private int anchorX = 0;
	private int anchorZ = 0;
	private WorldGrid worldGrid;
	private float[,] sharedVertHeights;
	private int biomeThickness = 1;

	//Required to construct the mesh
	private Vector3[] vertices;
	private Vector3[] normals;
	private Vector2[] uv;
	private int[] triangles;
	private Mesh mesh;

	//See how many tiles fit into the given texture. Each integer will corespond to four coordinates for the uv mapping.
	private int numTileTypes = 4;
	private Vector2[,] uvMappingKey;
	private float textureXSpan;
	private float textureZSpan;

	//For texturing the mesh from a tile map
	public int textureWidth = 128;
	public int textureHeight = 32;
	public int texturePixelSize = 32;

	// Use this for initialization
	void Start () {

	}

	//Allocate UV coordinates to each tile in the texture map.
	private void BuildTextureMap(){
		int mapTileWidth = textureWidth / texturePixelSize;
		int mapTileHeight = textureHeight / texturePixelSize;
		numTileTypes = mapTileWidth * mapTileHeight;
		
		//First element says which tile is access, second says which of the four corners. 0, 1, 2, 3 = bot left, top left, bot right, top right
		uvMappingKey = new Vector2[numTileTypes, 4];
		textureXSpan = (float)texturePixelSize / (float)textureWidth;
		textureZSpan = (float)texturePixelSize / (float)textureHeight;
		
		int i = 0;
		//Starting from the bottom left tile, assign uv coords.
		int x, z;
		x = z = 0;
		for (x = 0; x < mapTileWidth; x++) {
			for (z = 0; z < mapTileHeight; z++) {
				uvMappingKey [i,0] = new Vector2 ((float)(x * textureXSpan), (float)(z * textureZSpan));
				uvMappingKey [i,1] = new Vector2 ((float)((x) * textureXSpan), (float)((z+1) * textureZSpan));
				uvMappingKey [i,2] = new Vector2 ((float)((x+1) * textureXSpan), (float)((z) * textureZSpan));
				uvMappingKey [i,3] = new Vector2 ((float)((x+1) * textureXSpan), (float)((z+1) * textureZSpan));
				i++;
			}
		}
	}

	private void BuildMeshArrays(){
		int numTiles = xSize * zSize;
		int numTris = numTiles * 2;
		int numVerts = numTiles * 6;
		
		vertices = new Vector3[ numVerts ];
		normals = new Vector3[numVerts];
		uv = new Vector2[numVerts];
		
		triangles = new int[numTris * 3];

		mesh = new Mesh ();
	}

	private void BuildMesh(){
		int x, z;
		int localX; int localZ;
		localX = localZ = 0;

		for (z = anchorZ; z < anchorZ + zSize; z++) {
			localX = 0;
			for (x = anchorX; x < anchorX + xSize; x++) {
				//	BuildTileFlat (x, z);
				BuildTileFromSharedVerts(x, z, localX, localZ);
				localX++;
			}
			localZ++;
		}
		
		// Create a new Mesh and populate with the data
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.normals = normals;
		mesh.uv = uv;
		//Built in code likely better
		mesh.RecalculateNormals ();
		
		// Assign our mesh to our filter/renderer/collider
		MeshFilter mesh_filter = GetComponent<MeshFilter>();
		MeshCollider mesh_collider = GetComponent<MeshCollider>();
		
		mesh_filter.mesh = mesh;
		mesh_collider.sharedMesh = null;
		mesh_collider.sharedMesh = mesh;
	}

	//Builds a tile that takes the highest points adjacent to any vertice
	void BuildTileFromSharedVerts(int xGridCoord, int zGridCoord, int localX, int localZ){
		//Verts are stored in a 1d array, so find the first element for vert '0'
	//	int startOffset = (zGridCoord * zSize + xGridCoord) * 6;
		int startOffset = (localZ * zSize + localX) * 6;
		int panelOffset = 0;
		
		float yHeight;

		//1, 5, 2 and 4 need to be at the same height to avoid awkward ridges.
		//eg, if the tile is 'folded' it should be concave, not convex.

		//0 bot left.
		//1, 5 top left
		//3 top right
		//2, 4 bot right
		float yHeightBotLeft = sharedVertHeights[xGridCoord, zGridCoord];
		float yHeightTopLeft = sharedVertHeights[xGridCoord, zGridCoord+1];
		float yHeightBotRight = sharedVertHeights[xGridCoord+1, zGridCoord];
		float yHeightTopRight = sharedVertHeights[xGridCoord+1, zGridCoord+1];

		Vector3 botLeft = new Vector3 (xGridCoord * tileSize, yHeightBotLeft, (zGridCoord) * tileSize);
		Vector3 topLeft = new Vector3 (xGridCoord * tileSize, yHeightTopLeft, (zGridCoord+1) * tileSize);
		Vector3 botRight = new Vector3 ((xGridCoord + 1) * tileSize, yHeightBotRight, (zGridCoord) * tileSize);
		Vector3 topRight = new Vector3 ((xGridCoord + 1) * tileSize, yHeightTopRight, (zGridCoord + 1) * tileSize);

		//If top left and bot right are at the same level, do the usual botleft upperright triangle arrangement
		if (yHeightTopLeft == yHeightBotRight) {
			vertices [startOffset + 0] = botLeft;
			vertices [startOffset + 1] = topLeft;
			vertices [startOffset + 2] = botRight;

			vertices [startOffset + 3] = topRight;
			vertices [startOffset + 4] = botRight;
			vertices [startOffset + 5] = topLeft;
		} else { //Otherwise, rotate tile 90 degrees
			vertices [startOffset + 0] = topLeft;
			vertices [startOffset + 1] = topRight;
			vertices [startOffset + 2] = botLeft;
			
			vertices [startOffset + 3] = botRight;
			vertices [startOffset + 4] = botLeft;
			vertices [startOffset + 5] = topRight;
		}

		//uv mapping for single tile texture
		/*		uv [startOffset + 0] = new Vector2 (0f, 0f);
		uv [startOffset + 1] = new Vector2 (0f, 1f);
		uv [startOffset + 2] = new Vector2 (1f, 0f);
		
		uv [startOffset + 3] = new Vector2 (1f, 1f);
		uv [startOffset + 4] = new Vector2 (1f, 0f);
		uv [startOffset + 5] = new Vector2 (0f, 1f);*/
		
		//Getting uv from mapping key
		//Crudely, saying tile type is equal to terrain height minus 1.
		int tileType = worldGrid.GetTile (xGridCoord, zGridCoord) - 1;
		
		//		UVMapVerts (startOffset, tileType);
		UVMapVertsFromHeightmap (startOffset, tileType);		
		
		for (int i = 0; i < 6; i++) {
			normals [startOffset + i] = Vector3.up;
			triangles [startOffset + i] = startOffset + i;
		}
	}

	private void UVMapVertsFromHeightmap(int startOffset, int tileType){
		int tileTypeLowerLeft = tileType;
		int tileTypeUpperRight = tileType;
		
		//Currently gets the tile type from the vert. A better system would
		//generate this int as the height map is generated.
		
		int type0 = (int)Mathf.Round (vertices [startOffset + 0].y);
		int type1 = (int)Mathf.Round (vertices [startOffset + 1].y);
		int type2 = (int)Mathf.Round (vertices [startOffset + 2].y);
		int type3 = (int)Mathf.Round (vertices [startOffset + 3].y);
		int type4 = (int)Mathf.Round (vertices [startOffset + 4].y);
		int type5 = (int)Mathf.Round (vertices [startOffset + 5].y);
		
		//If one tri is higher than one vert, increment its type
		if ((type0 > type3) && (type1 > type3) && (type2 > type3)){
			tileTypeLowerLeft +=1;
		}
		if ((type3 > type0) && (type4 > type0) && (type5 > type0)) {
			tileTypeUpperRight += 1;
		}

		tileTypeLowerLeft = (int)(tileTypeLowerLeft / biomeThickness);
		tileTypeUpperRight = (int)(tileTypeUpperRight / biomeThickness);

		//Bound the tile types within the texture map
		if (tileTypeLowerLeft < 0)
			tileTypeLowerLeft = 0;
		if (tileTypeLowerLeft > numTileTypes-1)
			tileTypeLowerLeft = numTileTypes-1;
		
		if (tileTypeUpperRight < 0)
			tileTypeUpperRight = 0;
		if (tileTypeUpperRight > numTileTypes-1)
			tileTypeUpperRight= numTileTypes-1;
		
		//Assign the appropriate UV mapping to the correct vert, based on tile type and
		//which corner and tri its part of
		uv [startOffset + 0] = uvMappingKey [tileTypeLowerLeft, 0];
		uv [startOffset + 1] = uvMappingKey [tileTypeLowerLeft, 1];
		uv [startOffset + 2] = uvMappingKey [tileTypeLowerLeft, 2];
		
		uv [startOffset + 3] = uvMappingKey [tileTypeUpperRight, 3];
		uv [startOffset + 4] = uvMappingKey [tileTypeUpperRight, 2];
		uv [startOffset + 5] = uvMappingKey [tileTypeUpperRight, 1];
	}

	public void Initialise(int xSize, int zSize, int anchorX, int anchorZ, float tileSize, WorldGrid worldGrid, float[,] sharedVertHeights, int biomeThickness){
		this.xSize = xSize;
		this.zSize = zSize;
		this.anchorX = anchorX;
		this.anchorZ = anchorZ;
		this.tileSize = tileSize;
		this.worldGrid = worldGrid;
		this.sharedVertHeights = sharedVertHeights;
		this.biomeThickness = biomeThickness;

		BuildTextureMap ();
		BuildMeshArrays ();
		BuildMesh ();
	}

	public void RebuildChunk(){
		BuildMesh ();
	}
}
