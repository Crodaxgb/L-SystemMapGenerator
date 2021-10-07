using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MapGenerator : MonoBehaviour
{
    public int height, width;
    public string seed;

    public bool useRandomSeed;
    [Range(0, 100)]
    public int randomFillPercent;

    //1 means cell is alive (wall) 0 means dead (empty)
    int[,] currentMap, nextMap;
    public int maxIterationCount;

    private void Start()
    {

        GenerateMap();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GenerateMap();
        }
    }

    void GenerateMap()
    {
        currentMap = new int[width, height];
        nextMap = new int[width, height];
        RandomFillMap();

        for(int iteration=0; iteration<maxIterationCount;iteration++)
        {
            ApplyCellularRule();
        }
        MeshGenerator meshGenerator = GetComponent<MeshGenerator>();
        meshGenerator.GenerateMesh(currentMap, 1);
    }


    void RandomFillMap()
    {
        if(useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());


        for (int  rowIterator= 0; rowIterator < width; rowIterator++)
        {
            for (int columnIterator = 0; columnIterator < height; columnIterator++)
            {
                //If the current iteration is at the sides/borders turn the cells into walls
                if(rowIterator == 0 || rowIterator == width - 1 || columnIterator == 0 || columnIterator == height - 1)
                {
                    currentMap[rowIterator, columnIterator] = 1;
                    //This is initial state so assign for the next map too
                    nextMap[rowIterator, columnIterator] = 1;
                }
                else
                {
                    //If its less than the fill percent fill it with wall.
                    //Increasing the fill percent will increase the wall counts in total.
                    currentMap[rowIterator, columnIterator] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
                }
            }
        }
    }


    private void ApplyCellularRule()
    {
        for (int rowIterator = 0; rowIterator < width; rowIterator++)
        {
            for (int columnIterator = 0; columnIterator < height; columnIterator++)
            {
                int neighborWallTiles = GetSurroundingWallCount(rowIterator, columnIterator);

                if(neighborWallTiles > 4)
                {
                    nextMap[rowIterator, columnIterator] = 1;
                }
                else if(neighborWallTiles < 4)
                {
                    nextMap[rowIterator, columnIterator] = 0;
                }
            }
        }
        CopyArrayValues();
    }


    private int GetSurroundingWallCount(int xPosition, int yPosition)
    {
        int wallCount = 0;

        for(int rowIterator = xPosition - 1;  rowIterator<= xPosition + 1; rowIterator ++)
        {
            for (int columnIterator = yPosition - 1; columnIterator <= yPosition + 1; columnIterator++)
            {
                //Check for the iterators, if they overflow the boundaries don't count them
                if(rowIterator >= 0 && rowIterator < width && columnIterator>= 0 && columnIterator < height)
                {
                    //If the current iteration is not on the current cell that  its neighborhood is being calculated
                    if(rowIterator != xPosition || columnIterator != yPosition)
                    {
                        wallCount += currentMap[rowIterator, columnIterator];
                    }
                }
                else
                {
                    //that statement means we're iterating a border cell so it is always wall
                    wallCount++;
                }
            }
        }


        return wallCount;
    }



    private void CopyArrayValues()
    {
        
        //Assigns next state as the current state
        for (int rowIterator = 0; rowIterator < width; rowIterator++)
        {
            for (int columnIterator = 0; columnIterator < height; columnIterator++)
            {

                currentMap[rowIterator, columnIterator] = nextMap[rowIterator, columnIterator];

            }
        }
    }
    /*
    private void OnDrawGizmos()
    {
        if(currentMap != null)
        {
            for (int rowIterator = 0; rowIterator < width; rowIterator++)
            {
                for (int columnIterator = 0; columnIterator < height; columnIterator++)
                {
                    Gizmos.color = (currentMap[rowIterator, columnIterator] == 1)?Color.black:Color.white;
                    Vector3 position = new Vector3(-width / 2 + rowIterator + 0.5f, 0, -height / 2 + columnIterator + 0.5f);
                    Gizmos.DrawCube(position, Vector3.one);

                }
            }
        }
    }*/
}
