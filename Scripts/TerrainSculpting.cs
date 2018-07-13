using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TerrainSculpting : MonoBehaviour
{


    [Header("hexagonMesh Settings")]
    List<int> edgeMidPoints;
    Mesh hexagonMesh;
    float landHeight = 0.13f;
    Vector3[] vertices;
    HexClass hexVerts;

    public void AlterHex(List<int> coastlineMidPoints)
    {
        hexagonMesh = GetComponent<MeshFilter>().mesh;
        vertices = hexagonMesh.vertices;
        hexVerts = new HexClass();
        Vector3 Peakheight = new Vector3(0, 0, landHeight);
        //Set the height of the centre point of the hex to be of the set height value
        vertices[hexVerts.peak] += Peakheight;
        for (int i = 0; i < hexVerts.NumberOfRings; i++)
        {

            int[] currentRing = new int[24];
            //Retreive the index of all the vertexes within the current ring 
            currentRing = hexVerts.Hexagon[i];
            for (int j = 0; j < currentRing.Length; j++)
            {
                int currentVert = currentRing[j];
                Vector3 height = new Vector3(0, 0, landHeight);
                //Set every vert to be of the height of the set height value                  
                vertices[currentVert] += height;
            }
            //if the current ring is one of the last 4
            if (i >= hexVerts.NumberOfRings - 4)
            {
                List<int> listPoints = new List<int>();
                //find the height value necessary to lower the current rings height so that it smooths down to the coastline
                float heightDifference = landHeight / (hexVerts.NumberOfRings - (hexVerts.NumberOfRings - 4));
                int currentVert;
                int divideNumber;

                Vector3 height;
                //For every side of the hexagon that borders a sea tile
                for (int j = 0; j < coastlineMidPoints.Count; j++)
                {
                    //if the verts on the end on this side for this particular mid point is currently not within a list structure to
                    //be reduced in height then add it
                    if (listPoints.Contains(currentRing[coastlineMidPoints[j] - 2]) == false)
                    {
                        listPoints.Add(currentRing[coastlineMidPoints[j] - 2]);
                    }

                    if (coastlineMidPoints[j] != 22)
                    {
                        if (listPoints.Contains(currentRing[coastlineMidPoints[j] + 2]) == false)
                        {
                            listPoints.Add(currentRing[coastlineMidPoints[j] + 2]);
                        }
                    }
                    else
                    {
                        //if the verts on the end on this side for this particular mid point is currently not within a list structure to
                        //be reduced in height then add it(since the final edge point will loop back to the original vert in the structure)
                        if (listPoints.Contains(currentRing[0]) == false)
                        {
                            listPoints.Add(currentRing[0]);
                        }
                    }

                    //For every vert on the current side that isnt on either end
                    for (int z = coastlineMidPoints[j] - 1; z <= coastlineMidPoints[j] + 1; z++)
                    {
                        //Find the height value which will lower this ring to create a smooth coastline
                        //by using the current ring in the structure and the overall height value
                        currentVert = currentRing[z];
                        divideNumber = i - (hexVerts.NumberOfRings - 5);
                        height = new Vector3(0, 0, ((heightDifference * divideNumber)));

                        //if the current ring isn't the most outer ring
                        if(i!=hexVerts.NumberOfRings-1)
                        {
                            //generate a random seed value
                            float Seed = Random.Range(1.0f, 100f);
                            float xCoord = (float)(i / hexVerts.NumberOfRings) * scale * Seed;
                            float yCoord = (float)(z / coastlineMidPoints.Count) * scale * Seed;
                            //use the x and y co-ordinates to generate a noise value
                            float sample = Mathf.PerlinNoise(xCoord, yCoord);
                            //affect the height by mutliplying it with the noise value
                            height *= (sample);
                        }

                        vertices[currentVert] -= height;
                    }

                }

                //For every end point of the borders are to be affected
                for (int j = 0; j < listPoints.Count; j++)
                {
                    //Find the height value which will lower this ring to create a smooth coastline
                    //by using the current ring in the structure and the overall height value
                    currentVert = listPoints[j];
                    divideNumber = i - (hexVerts.NumberOfRings - 5);
                    height = new Vector3(0, 0, ((heightDifference * divideNumber)));
                    vertices[currentVert] -= height;
                }

            }
        }
        //Apply the height changes to the current mesh
        hexagonMesh.vertices = vertices;
        hexagonMesh.RecalculateBounds();

    }

   
    float scale = 10.0f;

    //creates hills on the hexagon mesh
    public void AddNoise(int width, int gridHeight, Vector2 currentPos, GameObject UI)
    {
        //retrieves the mesh filter component
        hexagonMesh = GetComponent<MeshFilter>().mesh;
        //sets up a list storing the points of each ring that act as the centre of the borders
        edgeMidPoints = new List<int> { 2, 6, 10, 14, 18, 22 };
        //retrieve all vertex points from the mesh
        vertices = hexagonMesh.vertices;
        //create a new hexagon structure for the mesh
        hexVerts = new HexClass();
 

        //retrieve a height value using perlin noise
        float xCoord = (float)(currentPos.x / width) * scale;
        float yCoord = (float)(currentPos.y / gridHeight) * scale;
        float sample = Mathf.PerlinNoise(xCoord, yCoord);

        //reduce the height value to make it more realistic
        sample = (sample / 3) * 2;
        //set a vector value to act as the centre points height
        Vector3 Peakheight = new Vector3(0, 0, (sample - landHeight));
        //half the heighvalue
        Peakheight /= 2;

        //value representing the amount of rings the hill will take up in the hill structure
        int ringMax = hexVerts.NumberOfRings - 5;
        //value representing how the height of every ring of the hill change in height
        float hillRatio = 0.8f;
        //update the UI's position so it doesn't get hidden by the hill
        UI.transform.position += (((new Vector3(0, (sample - landHeight),0)) / ringMax) * (ringMax))/2;
        //update the peak vertex height to be the peak height
        vertices[hexVerts.peak] += (Peakheight/ringMax) * (ringMax *0.85f);

        //for every ring the hill will alter
        for (int i = 0; i < hexVerts.NumberOfRings-5; i++)
        {
            //retrieve every vertex in the current ring
            int[] currentRing = new int[24];
            currentRing = hexVerts.Hexagon[i];
            //calculate the current rings height value
            Vector3 ringHeight = (Peakheight / ringMax) * (ringMax - i);
            //affect the height value by multiplying it by the hill ratio
            ringHeight *= hillRatio;
            //increase the hill ratio tio increase the slope
            hillRatio += 0.1f;
            //for every vertex in the ring
            for (int j = 0; j < currentRing.Length; j++)
            {
                //retrieve the current vertex
                int currentVert = currentRing[j];
                //add the height value to the vertexes position
                vertices[currentVert] += ringHeight;
            }
        }

        //Apply the height changes to the current mesh
        hexagonMesh.vertices = vertices;
        hexagonMesh.RecalculateBounds();

    }

    GameObject parent;
    //places biome specific prefabs on athe current tile based on a rng value
    public void AddPrefab(List<GameObject> prefab, int RNGHigherValue)
    {
        //retrieve a parentg object
        parent = GameObject.Find("WorldObj");
        //retrieve the mesh filter component
        hexagonMesh = GetComponent<MeshFilter>().mesh;
        //sets up a list storing the points of each ring that act as the centre of the borders
        edgeMidPoints = new List<int> { 2, 6, 10, 14, 18, 22 };
        //retrieve all vertex points from the mesh
        vertices = hexagonMesh.vertices;
        //create a new hexagon structure for the mesh
        hexVerts = new HexClass();
        //retreive the position of the peak vertex
        Vector3 worldPt = transform.TransformPoint(vertices[hexVerts.peak]);
        //for every ring within the hexagon
        for (int i = 0; i < hexVerts.NumberOfRings - 1; i++)
        {
            //retrieve every vertex in the current ring
            int[] currentRing = new int[24];
            currentRing = hexVerts.Hexagon[i];

             //for every vertex in the current ring
            for (int j = 0; j < currentRing.Length; j++)
            {
                //retrieve the current vertex
                int currentVert = currentRing[j];
                //retrieve the current vertexes position
                 worldPt = transform.TransformPoint(vertices[currentVert]);

                //generate a random number
                int rng = Random.Range(0, 101);
                //if the generated number is within the range
                if(rng> RNGHigherValue)
                {
                    //retrieve one of biome objects at random
                    int randomObj = Random.Range(0, prefab.Count);
                    var currentPrefab = prefab[randomObj];
                    //place the chosen biome object at the current vertexes world position
                   var temp = Instantiate(currentPrefab, worldPt, Quaternion.identity);
                    //set the objects parent
                   temp.transform.parent = parent.transform;
                }
             
            }
        }
        //Apply the height changes to the current mesh
        hexagonMesh.vertices = vertices;
        hexagonMesh.RecalculateBounds();
    }


    //function which places a city object on a hex tile
    public void PlaceCity(GameObject cityObj, string name)
    {
        //retreives partent objeect for city
        parent = GameObject.Find("WorldObj");
        //retreieves information about the mesh
        hexagonMesh = GetComponent<MeshFilter>().mesh;
        vertices = hexagonMesh.vertices;
        hexVerts = new HexClass();
        //calculates a height value which the city must be places
        float scale = cityObj.transform.lossyScale.y /2 + landHeight;
        //sets the spawn position of the city
        Vector3 spawnPos = transform.TransformPoint(vertices[hexVerts.peak]);
        spawnPos = new Vector3(spawnPos.x, spawnPos.y + scale, spawnPos.z);
        //spawnt the city and set its parent object
        GameObject cityObject = Instantiate(cityObj, spawnPos, Quaternion.identity);
        cityObject.transform.parent = parent.transform;
        //set the citys UI to be the name passed
        GameObject cityNameUI = cityObject.transform.GetChild(1).GetChild(0).GetChild(0).gameObject;
        cityNameUI.GetComponent<Text>().text = name;
    }
}

