using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 class HexClass
{
    //list representing all vertex points within the hexagons mesh
    public List<int[]> Hexagon;
    //value representing the vertex point which acted as the very centre of the hexagon
    public int peak;
    //value representing the amount of rings within the hexagonal structure
    public  int NumberOfRings;
    //constructor
    public HexClass()
    {
        //set up the hexagon list
        Hexagon = new List<int[]>();
        //peak is vertex 0
        peak = 0;
        //the structure contains each ring
        NumberOfRings = 8;
        //each ring will have the following vertexes
        int[,] rings = new int[8, 24] { { 49, 35, 34, 20, 1, 2, 5, 180, 177, 163, 162, 156, 131, 130, 129, 122, 121, 99, 97, 82, 81, 67, 65, 52}
                                       ,{ 50, 36, 33, 19, 18, 3, 4, 179, 178, 164, 161, 155, 154, 133, 132, 120, 119, 100, 98, 83, 84, 68, 66, 51}
                                       ,{ 54, 38, 37, 21, 22, 7, 6, 181, 182, 166, 165, 153, 152, 135, 134, 118, 117, 102, 101, 85, 86, 70, 69, 53}
                                       ,{ 56, 40, 39, 23, 24, 9, 8, 183, 184, 168, 167, 151, 150, 137, 136, 116, 114, 104, 103, 87, 88, 72, 71, 55}
                                       ,{ 58, 42, 41, 25, 26, 11, 10, 185, 186, 170, 169, 149, 146, 139, 138, 115, 113, 106, 105, 89, 90, 74, 73, 57}
                                       ,{ 60, 44, 43, 27, 28, 13, 12, 187, 188, 172, 171, 148, 147, 141, 140, 123, 124, 108, 107, 91, 92, 76, 75, 59}
                                       ,{ 62, 46, 45, 29, 30, 15, 14, 189, 190, 174, 173, 157, 158, 143, 142, 125, 126, 110, 109, 93, 94, 78, 77, 61}
                                       ,{ 64, 48, 47, 31, 32, 17, 16, 191, 192, 176, 175, 159, 160, 145, 144, 127, 128, 112, 111, 95, 96, 80, 79, 63}

        };
        //for every ring
        for (int i=0;i<8;i++)
        {
            // create a new integer array made up of 24 points
            int[] newRing = new int[24];
            //for every vertex in a ring
            for (int j = 0; j < 24; j++)
            {
                //add the current vertex to the new ring array
                newRing[j] = rings[i, j];
            }
            //add the new ring array to the list of vertexes
            Hexagon.Add(newRing);
        }
    }
	
}


