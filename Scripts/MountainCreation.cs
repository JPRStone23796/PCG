using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainCreation : MonoBehaviour {

    public GameObject Hex, Sphere;
    Mesh mesh;
    List<int> BorderMidPoints;
    Vector3 heightValue, rngBase, Decrement;
    Vector3[] vertices;
    HexClass hexVerts;

    public bool ShowVerts;

    public float PeakLow, PeakHigh, SlopeRNG, SmoothRNG;

    List<int> tilesBorders;

    //sets which of the current cells borders is beside another mountain tile
    public void SetBorders(List<int> points)
    {
        //create a new list
        tilesBorders = new List<int>();
        //set the new list to the one passed
        tilesBorders = points;
    }



    void Awake()
    {
        //initialise values used to generate
        PeakLow = 0.715f;
        PeakHigh = 1f;
        SlopeRNG = 0.95f;
        SmoothRNG = 0.55f;
    }

    //responsible for creating the mountain structure
   public void Create()
    {
        //retrieve the mesh filter component
        mesh = GetComponent<MeshFilter>().mesh;
        //sets up a list storing the points of each ring that act as the centre of the borders
        BorderMidPoints = new List<int> { 2, 6, 10, 14, 18, 22 };
        //retrieve all vertex points from the mesh
        vertices = mesh.vertices;
        //calculate the height decrease between each ring
        Decrement = Vector3.forward / 3 * 2;
        // set the base height decrease for each ring
        rngBase = (Decrement / 8);
        //create a new hexagon structure for the mesh
        hexVerts = new HexClass();
        //set the mountains peak to be the highest height possible
        vertices[hexVerts.peak] += ((Decrement / 8) * 9);
        //set the height of the range of mountain, connected it to neighbours
        SetRange(tilesBorders);
        //set the slopes of each mountain
        SetSlopes(tilesBorders);
        //set every other vertex based on the slope values
        SmoothSlopes();
        //Apply the height changes to the current mesh
        mesh.vertices = vertices;
        mesh.RecalculateBounds();

    }

    //set the mountain ranges
    void SetRange(List<int> Point)
    {
        //for every border point that connects to another mountain
        for(int x=0;x<Point.Count;x++)
        {
            //for every ring in the mountain
            for (int i = 0; i < hexVerts.NumberOfRings; i++)
            {
                //retrieve every vertex in the current ring
                int[] currentRing = new int[24];
                currentRing = hexVerts.Hexagon[i];
                //for every vertex in the current ring
                for (int j = 0; j < currentRing.Length; j++)
                {
                    //generate a value that will affect each vertexes height
                    float rng = Random.Range(PeakLow, PeakHigh);
                    //if the current vertex is part of a range
                    if (j == Point[x])
                    {
                        //if the current ring isnt the last
                        if (i < hexVerts.NumberOfRings - 1)
                        {
                            //retrieve the current vertex
                            int currentVert = currentRing[j];
                            //add half the maximum height to the vertex
                            vertices[currentVert] += (Decrement / 2);
                            // reset the height value vector
                            heightValue = Vector3.zero;
                            //calculate the remaining height value to be added
                            heightValue += (rngBase * 4) * rng;
                            //add the remaining height value to the vertex
                            vertices[currentVert] += heightValue;
                        }
                        //if the current ring is the last
                        else
                        {
                            //retrieve the current vertex
                            int currentVert = currentRing[j];
                            //set the vertex height to be the maximum height, ensuring the range connects to the other mountain
                            vertices[currentVert] += Decrement;
                        }
                    }
                }

            }
        }
        
    }

    //set the mountain slopws
    void SetSlopes(List<int> Point)
    {

             //for every ring in the mountain
            for (int i = 0; i < hexVerts.NumberOfRings - 1; i++)
            {
                //retrieve all vertexes in the current ring
                int[] currentRing = new int[24];
                currentRing = hexVerts.Hexagon[i];
                //for every vertex in the current ring
                for (int j = 0; j < currentRing.Length; j++)
                {
                    //generate a value that will affect each vertexes height
                    float rng = Random.Range(-SlopeRNG, (SlopeRNG));
                    //retrieve the current vertex
                    int currentVert = currentRing[j];
                    //calculate a height value based on the current ring
                    heightValue = (Decrement / 8) * (8 - (i + 1));
                    //add a height value generated
                    heightValue += rngBase * rng;
                    //if the vertex is one of the border points
                    if (BorderMidPoints.Contains(j) && !(Point.Contains(j)))
                    {
                        //add the height value to the vertex's height
                        vertices[currentVert] += heightValue;
                    }

                }
            }
       
    }




    // alter every other vertex
    void SmoothSlopes()
    {
        //for every midpoint 
        for(int listPos=0; listPos<BorderMidPoints.Count;listPos++)
        {
            //for every ring in the hexagon structure
            for (int i = 0; i < hexVerts.NumberOfRings-1; i++)
            {
                //retrieve every vertex in the current ring
                int[] currentRing = new int[24];
                currentRing = hexVerts.Hexagon[i];

                //if the current border pos isnt the last
                if (listPos < BorderMidPoints.Count - 1)
                {
                    //retrieve the border start and end points
                    int BorderStart = BorderMidPoints[listPos];
                    int BorderEnd = BorderMidPoints[listPos + 1];
                    //retrieve the current and next border midpoint 
                    int currentPoint = currentRing[BorderStart];
                    int nextPoint = currentRing[BorderEnd];

                    //calculate a height average of the current and next border point
                    Vector3 HeightAverage = (vertices[currentPoint] + vertices[nextPoint]) / 2;
                    //create a height vector using the value calculated
                    HeightAverage = new Vector3(0, 0, HeightAverage.z);
                    //calculate the difference in height between the average and the next point
                    Vector3 HeightDifference = HeightAverage - vertices[nextPoint];
                    //create a height vector using the value calculated
                    HeightDifference = new Vector3(0, 0, HeightDifference.z);
                   //for every vertex between the border start and end
                    for (int z = (BorderStart + 1); z < BorderEnd; z++)
                    {
                        //retrieve the current vertex
                        int currentVert = currentRing[z];
                        //generate a random value to apply to the heightdifference value
                        float rng = Random.Range(-SmoothRNG, SmoothRNG);
                        //add the height average value to the height vector
                        heightValue = HeightAverage;
                        //add a random value height based on the height difference value
                        heightValue += HeightDifference * rng;
                        //apply the height vector to the vertex
                        vertices[currentVert] += heightValue;
                    }


                }
                else
                {
                    //retrieve the border start and end points
                    int BorderStart = BorderMidPoints[listPos];
                    int BorderEnd = BorderMidPoints[0];
                    //retrieve the current and next border midpoint 
                    int currentPoint = currentRing[BorderStart];
                    int nextPoint = currentRing[BorderEnd];
                    //calculate a height average of the current and next border point
                    Vector3 HeightAverage = (vertices[currentPoint] + vertices[nextPoint]) / 2;
                    //create a height vector using the value calculated
                    HeightAverage = new Vector3(0, 0, HeightAverage.z);
                    //create a height vector using the value calculated
                    Vector3 HeightDifference = HeightAverage - vertices[nextPoint];
                    //create a height vector using the value calculated
                    HeightDifference = new Vector3(0, 0, HeightDifference.z);
                    //for every vertex in the current ring
                    for (int j = 0; j < currentRing.Length; j++)
                    {
                        //if the vertex is between these ranges
                        if (j < BorderEnd || j > BorderStart)
                        {
                            //retrieve the current vertex
                            int currentVert = currentRing[j];
                            //generate a random value to apply to the heightdifference value
                            float rng = Random.Range(-SmoothRNG, SmoothRNG);
                            //add the height average value to the height vector
                            heightValue = HeightAverage;
                            //add a random value height based on the height difference value
                            heightValue += HeightDifference * rng;
                            //apply the height vector to the vertex
                            vertices[currentVert] += heightValue;
                        }

                    }
                }

            }
        }

    }
}
