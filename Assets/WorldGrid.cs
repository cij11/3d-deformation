using UnityEngine;
using System.Collections;

/// <summary>
/// World grid.
/// Stores the underlying world topographical/height data
/// The physical/rendered terrain chunk meshes will be built from this.
/// 
/// Also contains recursive functions for editing terrain. Meshes
/// get updated to the newly calculated heights after editing.
/// </summary>
public class WorldGrid {
	public enum Directions{N, NE, E, SE, S, SW, W, NW, C };

	int xSize;
	int zSize;
	int[,] worldData;
	float xSeed;
	float zSeed;

	public WorldGrid(int xSize, int zSize){
		this.xSize = xSize;
		this.zSize = zSize;
		xSeed = Random.Range (0f, 1000f);
		zSeed = Random.Range (0f, 1000f);

		worldData = new int[xSize, zSize];

		BuildWorldRandom ();
	//	CreateHeightMap (4);
	}
	public void BuildWorldRandom(){
		int x, z;
		for (z = 0; z < zSize; z++) {
			for (x = 0; x < xSize; x++) {
				int tileType = Random.Range(0, 3);
				if(tileType == 0) worldData [x,z] = 1;
				else worldData [x,z] = 0;
			}
		}
	}

    public void RemoveDivots(int startX, int startZ, int endX, int endZ)
    {
        int x, z;
        x = z = 0;

        for (x = startX; x < endX; x++)
        {
            for (z = startZ; z < endZ; z++)
            {
                //check the adjacent 8 tiles. If all four corners shared by higher tiles, elevate.
                bool corner0, corner1, corner2, corner3;
                corner0 = corner1 = corner2 = corner3 = false;
                //Cardinal squares affect 2 verts
                if (worldData[x, z+1] > worldData[x, z])
                {
                    corner1 = true;
                    corner3 = true;
                }
                if (worldData[x+1, z] > worldData[x, z])
                {
                    corner3 = true;
                    corner2 = true;
                }
                if (worldData[x, z-1] > worldData[x, z])
                {
                    corner2 = true;
                    corner0 = true;
                }
                if (worldData[x-1, z] > worldData[x, z])
                {
                    corner0 = true;
                    corner1 = true;
                }
                //Diagonal squares only affect one corner
                if (worldData[x - 1, z + 1] > worldData[x, z]) corner1 = true;
                if (worldData[x + 1, z - 1] > worldData[x, z]) corner2 = true;
                if (worldData[x + 1, z + 1] > worldData[x, z]) corner3 = true;
                if (worldData[x - 1, z - 1] > worldData[x, z]) corner0 = true;

                if (corner0 && corner1 && corner2 && corner3)
                {
                    worldData[x, z] = worldData[x, z] + 1;
                }
            }
        }

    }

	//Create a 2d grid of ints coresponding to the terrain height at that coord
	public void CreateHeightMap(int terrainHeightVariability, float bumpiness){

		int x, z;
		x = z = 0;
		
		for (x = 0; x < xSize; x++) {
			for (z = 0; z < zSize; z++) {
				float xRatio = (float)x * bumpiness;
				float zRatio = (float)z * bumpiness;
				
				//	heightArray[x, z] = terrainBaseHeight + Random.Range(0, terrainHeightVariability+1);
				float perlinRatio = (Mathf.PerlinNoise(xRatio + xSeed, zRatio + zSeed));
				float perlinSquared = perlinRatio; // * perlinRatio * perlinRatio;
			//	float perlinSquared = Mathf.Sqrt(perlinRatio);
				float perlinScaled =  perlinSquared * (float)terrainHeightVariability;
				worldData [x, z] = (int)perlinScaled;
			}
		}
	}

	
	public void RaiseTerrain (int x, int z, int targetHeight)
	{

	}

	//Check the adjacent 8 tiles. If they are two levels below the current tile, elevate them,
	//then check the tiles adjacent.
	//Call direction C to begin with, which recurses in all 8 directions.
	public void SmoothTerrainTopDown (int x, int z, Directions direction, int neighbourHeight)
	{
		if (worldData [x, z] < (neighbourHeight - 1)) {
			worldData [x, z] = neighbourHeight - 1;

			switch (direction) {
			case Directions.N:
				{
					if (z < zSize - 1)
						SmoothTerrainTopDown (x, z + 1, Directions.N, neighbourHeight - 1);
					break;
				}
			case Directions.S:
				{
					if (z > 0)
						SmoothTerrainTopDown (x, z - 1, Directions.S, neighbourHeight - 1);
					break;
				}
			case Directions.E:
				{
					if (x < xSize - 1)
						SmoothTerrainTopDown (x + 1, z, Directions.E, neighbourHeight - 1);
					break;
				}
			case Directions.W:
				{
					if (x > 0)
						SmoothTerrainTopDown (x - 1, z, Directions.W, neighbourHeight - 1);
					break;
				}
			case Directions.NE:
				{
					if (z < zSize - 1)
						SmoothTerrainTopDown (x, z + 1, Directions.N, neighbourHeight - 1);
					if (x < xSize - 1)
						SmoothTerrainTopDown (x + 1, z, Directions.E, neighbourHeight - 1);
					if ((x < xSize - 1) && (z < zSize - 1))
						SmoothTerrainTopDown (x + 1, z + 1, Directions.NE, neighbourHeight - 1);
				
					break;
				}
			case Directions.NW:
				{
					if (z < zSize - 1)
						SmoothTerrainTopDown (x, z + 1, Directions.N, neighbourHeight - 1);
					if (x > 0)
						SmoothTerrainTopDown (x - 1, z, Directions.W, neighbourHeight - 1);
					if ((z < zSize - 1) && (x > 0))
						SmoothTerrainTopDown (x - 1, z + 1, Directions.NW, neighbourHeight - 1);
					break;
				}
			case Directions.SE:
				{
					if (z > 0)
						SmoothTerrainTopDown (x, z - 1, Directions.S, neighbourHeight - 1);
					if (x < xSize - 1)
						SmoothTerrainTopDown (x + 1, z, Directions.E, neighbourHeight - 1);
					if ((z > 0) && (x < xSize - 1))
						SmoothTerrainTopDown (x + 1, z - 1, Directions.SE, neighbourHeight - 1);
					break;
				}
			case Directions.SW:
				{
					if (z > 0)
						SmoothTerrainTopDown (x, z - 1, Directions.S, neighbourHeight - 1);
					if (x > 0)
						SmoothTerrainTopDown (x - 1, z, Directions.W, neighbourHeight - 1);
					if ((z > 0) && (x > 0))
						SmoothTerrainTopDown (x - 1, z - 1, Directions.SW, neighbourHeight - 1);
					break;
				}
			case Directions.C:
				{
					if (z < zSize - 1)
						SmoothTerrainTopDown (x, z + 1, Directions.N, neighbourHeight - 1);
					if (z > 0)
						SmoothTerrainTopDown (x, z - 1, Directions.S, neighbourHeight - 1);
					if (x < xSize - 1)
						SmoothTerrainTopDown (x + 1, z, Directions.E, neighbourHeight - 1);
					if (x > 0)
						SmoothTerrainTopDown (x - 1, z, Directions.W, neighbourHeight - 1);
					if ((x < xSize - 1) && (z < zSize - 1))
						SmoothTerrainTopDown (x + 1, z + 1, Directions.NE, neighbourHeight - 1);
					if ((z < zSize - 1) && (x > 0))
						SmoothTerrainTopDown (x - 1, z + 1, Directions.NW, neighbourHeight - 1);
					if ((z > 0) && (x < xSize - 1))
						SmoothTerrainTopDown (x + 1, z - 1, Directions.SE, neighbourHeight - 1);
					if ((z > 0) && (x > 0))
						SmoothTerrainTopDown (x - 1, z - 1, Directions.SW, neighbourHeight - 1);
					break;
				}
			}
		}
	}

    public void SmoothTerrainBottomUp(int x, int z, Directions direction, int neighbourHeight)
    {
        if (worldData[x, z] > (neighbourHeight + 1))
        {
            worldData[x, z] = neighbourHeight + 1;

            switch (direction)
            {
                case Directions.N:
                    {
                        if (z < zSize - 1)
                            SmoothTerrainBottomUp(x, z + 1, Directions.N, neighbourHeight + 1);
                        break;
                    }
                case Directions.S:
                    {
                        if (z > 0)
                            SmoothTerrainBottomUp(x, z - 1, Directions.S, neighbourHeight + 1);
                        break;
                    }
                case Directions.E:
                    {
                        if (x < xSize - 1)
                            SmoothTerrainBottomUp(x + 1, z, Directions.E, neighbourHeight + 1);
                        break;
                    }
                case Directions.W:
                    {
                        if (x > 0)
                            SmoothTerrainBottomUp(x - 1, z, Directions.W, neighbourHeight + 1);
                        break;
                    }
                case Directions.NE:
                    {
                        if (z < zSize - 1)
                            SmoothTerrainBottomUp(x, z + 1, Directions.N, neighbourHeight + 1);
                        if (x < xSize - 1)
                            SmoothTerrainBottomUp(x + 1, z, Directions.E, neighbourHeight + 1);
                        if ((x < xSize - 1) && (z < zSize - 1))
                            SmoothTerrainBottomUp(x + 1, z + 1, Directions.NE, neighbourHeight + 1);

                        break;
                    }
                case Directions.NW:
                    {
                        if (z < zSize - 1)
                            SmoothTerrainBottomUp(x, z + 1, Directions.N, neighbourHeight + 1);
                        if (x > 0)
                            SmoothTerrainBottomUp(x - 1, z, Directions.W, neighbourHeight + 1);
                        if ((z < zSize - 1) && (x > 0))
                            SmoothTerrainBottomUp(x - 1, z + 1, Directions.NW, neighbourHeight + 1);
                        break;
                    }
                case Directions.SE:
                    {
                        if (z > 0)
                            SmoothTerrainBottomUp(x, z - 1, Directions.S, neighbourHeight + 1);
                        if (x < xSize - 1)
                            SmoothTerrainBottomUp(x + 1, z, Directions.E, neighbourHeight + 1);
                        if ((z > 0) && (x < xSize - 1))
                            SmoothTerrainBottomUp(x + 1, z - 1, Directions.SE, neighbourHeight + 1);
                        break;
                    }
                case Directions.SW:
                    {
                        if (z > 0)
                            SmoothTerrainBottomUp(x, z - 1, Directions.S, neighbourHeight + 1);
                        if (x > 0)
                            SmoothTerrainBottomUp(x - 1, z, Directions.W, neighbourHeight + 1);
                        if ((z > 0) && (x > 0))
                            SmoothTerrainBottomUp(x - 1, z - 1, Directions.SW, neighbourHeight + 1);
                        break;
                    }
                case Directions.C:
                    {
                        if (z < zSize - 1)
                            SmoothTerrainBottomUp(x, z + 1, Directions.N, neighbourHeight + 1);
                        if (z > 0)
                            SmoothTerrainBottomUp(x, z - 1, Directions.S, neighbourHeight + 1);
                        if (x < xSize - 1)
                            SmoothTerrainBottomUp(x + 1, z, Directions.E, neighbourHeight + 1);
                        if (x > 0)
                            SmoothTerrainBottomUp(x - 1, z, Directions.W, neighbourHeight + 1);
                        if ((x < xSize - 1) && (z < zSize - 1))
                            SmoothTerrainBottomUp(x + 1, z + 1, Directions.NE, neighbourHeight + 1);
                        if ((z < zSize - 1) && (x > 0))
                            SmoothTerrainBottomUp(x - 1, z + 1, Directions.NW, neighbourHeight + 1);
                        if ((z > 0) && (x < xSize - 1))
                            SmoothTerrainBottomUp(x + 1, z - 1, Directions.SE, neighbourHeight + 1);
                        if ((z > 0) && (x > 0))
                            SmoothTerrainBottomUp(x - 1, z - 1, Directions.SW, neighbourHeight + 1);
                        break;
                    }
            }
        }
    }

    public int GetTile(int x, int z){
		return worldData [x, z];
	}
	public void SetTile(int x, int z, int newTile){
		worldData [x, z] = newTile;
	}
}