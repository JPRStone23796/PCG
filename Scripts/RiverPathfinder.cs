using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverPathfinder : MonoBehaviour
{
    GameObject[,] Grid;
    bool[,] islandLandMass;
    Vector2 Goal, CurrentGrid;
    public Vector2 CurrentGridPosition;
    List<CalculatePath> Open = new List<CalculatePath>();
    List<CalculatePath> Closed = new List<CalculatePath>();
    Queue<Vector2> Path = new Queue<Vector2>();
    int currentPos;
    Vector2[] AIpath;
    Vector2 CurrentParent;
    CreateGrid grid;
    GeneratingIsland islandState;

    public bool pathfound, pathNotPossible,waterFound;
    int PathEnd;



    //public function which will return a list of cells which connects the two positions provided
    public List<Vector2> CreatePath(Vector2 coastPos, Vector2 startPos)
    {
        //retrieve the create grid compenent
        grid = GetComponent<CreateGrid>();
        //vector representing where the path should reach
        Goal = coastPos;
        //vector representing where the path should start
        CurrentGridPosition = startPos;
        //retreive the map structure
        Grid = grid.HexGrid;
        //initialise a value representing if a water tile is found
        waterFound = false;
        //value representing if a path is  possible
        pathNotPossible = false;
        //retrieve the generating island component
        islandState = GetComponent<GeneratingIsland>();
        //retreive the state of every tile in the map structure
        islandLandMass = islandState.hexMapIsland();
        //create a list containing all cells that have not yet been considered
        Open = new List<CalculatePath>();
        //create a list containing all cells that have been calculated
        Closed = new List<CalculatePath>();

        //find a path between the start point and the goal point
        FindingPath();

        //create a list to store the path created
        List<Vector2> lakePath = new List<Vector2>();
       //if a path was created and its length is greater than 1
        if(pathfound && AIpath.Length>1)
        {
            //for every point in the path found
            for(int i=0;i<AIpath.Length;i++)
            {  
                //Add the current path point the list of points
                lakePath.Add(AIpath[i]);
            }
        }
        //remove all points from the open and closed lists
        resetValues();
        //return the list of points making up the path
        return lakePath;
    }
    

    void FindingPath()
    {
        //add the starting point of the path to the open list, set its parent to be a zero vector 
        Open.Add(new CalculatePath(CurrentGridPosition, Vector2.zero));
        //find the path between the starting position and end position
        AIMovement();
        //if a path is found
        if(pathfound)
        {
            //retrieve all path points 
            FindPath();
        }
     
    }

    //reponsible for finding the path between the start and the goal positions
    void AIMovement()
    {
        //while a path has not be found and path is still possible
        while (pathfound == false && pathNotPossible==false )
        { 
            //if all cells have been considered
            if (Open.Count == 0)
            {
                //the path is not possible
                pathNotPossible = true;
            }
            //if the path is still possible
            if (pathNotPossible==false)
            {
                CalculatePoint();
                CalculateNeighbours();
            }
        }
    }

    void CalculatePoint()
    {
        //value representing the lowest fcost value
        float Fcost = 100;
        //for every cell remaining in the open list
        for (int i = 0; i < Open.Count; i++)
        {
            //calculate the current cells fcost by adding the cells distance from the start point and end point together
            var CurrentFCost = CalculateCost(Open[i].ReturningPosition(), CurrentGridPosition) + CalculateCost(Open[i].ReturningPosition(), Goal);
            //if the current cells fcost is the lowest
            if (CurrentFCost < Fcost)
            {
                //the current cell becomes the selected next step
                Fcost = CurrentFCost;
                CurrentGrid = Open[i].ReturningPosition();
                currentPos = i;
            }
        }
    }

    //calculates the G cost and H cost for each cell
    float CalculateCost(Vector2 GridPosition, Vector2 StartPosition)
    {
        //finds the difference between the cells current position and the other vector passed
        var x = Mathf.Abs(GridPosition.x - StartPosition.x);
        var y = Mathf.Abs(GridPosition.y - StartPosition.y);
        //adds the x and y difference together and returns it
        return x + y;
    }

    void CalculateNeighbours()
    {
        //add neighbours to open list if they aren't already
        NeighbourCheck();
        //add the current cell to the closed list
        Closed.Add(Open[currentPos]);
        //if the current cell has an h cost of less than or equal to one
        if (CalculateCost(Open[currentPos].ReturningPosition(), Goal) <= 1)
        {
            //value representing the final point of the path
            PathEnd = Closed.Count - 1;
            //path has been found
            pathfound = true;

        }
        //if the path has already found another water tile, either another lake or sea
        else if(waterFound)
        {
            //value representing the final point of the path
            PathEnd = Closed.Count - 1;
            //path has been found
            pathfound = true;
        }
        //remove the last point from the open list
        Open.Remove(Open[currentPos]);

    }

    void NeighbourCheck()
    {
        //retrieve the x and y co-ordinates of the current cell
        int xPos = (int)CurrentGrid.x;
        int yPos = (int)CurrentGrid.y;
        //value representing each of the cells neighbours
        Vector2 pos = Vector2.zero;



        //////// Check each of the cells honeycomb neighbours and if they are not a mountain tile, are of a land state or already in the lists then add the cell to the open list
        if (xPos > 0)
        {
            if (grid.HexGrid[xPos - 1, yPos].transform.GetChild(0).gameObject.tag != "Mountain" && islandLandMass[xPos - 1, yPos] )
            {
                pos = new Vector2(xPos - 1, yPos);
                var current = new CalculatePath(pos, CurrentGrid);
                if (CheckList(pos, Closed) == false && CheckList(pos, Open) == false)
                {

                    Open.Add(current);
                }
            }
            else if (grid.HexGrid[xPos - 1, yPos].transform.GetChild(0).gameObject.tag != "Mountain" && islandLandMass[xPos - 1, yPos]==false)
            {
                waterFound = true;
            }
        }
        if (xPos < (grid.gridWidth - 1))
        {
            if (grid.HexGrid[xPos + 1, yPos].transform.GetChild(0).gameObject.tag != "Mountain" && islandLandMass[xPos + 1, yPos])
            {
                pos = new Vector2(xPos + 1, yPos);
                var current = new CalculatePath(pos, CurrentGrid);
                if (CheckList(pos, Closed) == false && CheckList(pos, Open) == false)
                {

                    Open.Add(current);
                }
            }
            else if (grid.HexGrid[xPos + 1, yPos].transform.GetChild(0).gameObject.tag != "Mountain" && islandLandMass[xPos + 1, yPos]==false)
            {
                waterFound = true;
            }
            
        }
        if (yPos % 2 != 0)
        {
            if (yPos > 0)
            {
                if (grid.HexGrid[xPos, yPos - 1].transform.GetChild(0).gameObject.tag != "Mountain" && islandLandMass[xPos, yPos-1])
                {
                    pos = new Vector2(xPos, yPos-1);
                    var current = new CalculatePath(pos, CurrentGrid);
                    if (CheckList(pos, Closed) == false && CheckList(pos, Open) == false)
                    {

                        Open.Add(current);
                    }
                }
                else if (grid.HexGrid[xPos, yPos - 1].transform.GetChild(0).gameObject.tag != "Mountain" && islandLandMass[xPos, yPos - 1] == false)
                {
                    waterFound = true;
                }
            }
           


            if (xPos < (grid.gridWidth - 1) && yPos > 0)
            {
                if (grid.HexGrid[xPos + 1, yPos - 1].transform.GetChild(0).gameObject.tag != "Mountain" && islandLandMass[xPos + 1, yPos-1])
                {
                    pos = new Vector2(xPos + 1, yPos - 1);
                    var current = new CalculatePath(pos, CurrentGrid);
                    if (CheckList(pos, Closed) == false && CheckList(pos, Open) == false)
                    {

                        Open.Add(current);
                    }
                }
                else if (grid.HexGrid[xPos + 1, yPos - 1].transform.GetChild(0).gameObject.tag != "Mountain" && islandLandMass[xPos + 1, yPos - 1] == false)
                {
                    waterFound = true;
                }
            }
           


            if (xPos < (grid.gridWidth - 1) && yPos < (grid.gridHeight - 1))
            {
                if (grid.HexGrid[xPos + 1, yPos + 1].transform.GetChild(0).gameObject.tag != "Mountain" && islandLandMass[xPos + 1, yPos +1])
                {
                    pos = new Vector2(xPos + 1, yPos +1);
                    var current = new CalculatePath(pos, CurrentGrid);
                    if (CheckList(pos, Closed) == false && CheckList(pos, Open) == false)
                    {

                        Open.Add(current);
                    }
                }
                else if (grid.HexGrid[xPos + 1, yPos + 1].transform.GetChild(0).gameObject.tag != "Mountain" && islandLandMass[xPos + 1, yPos + 1] == false)
                {
                    waterFound = true;
                }
            }
           


            if (yPos < (grid.gridHeight - 1))
            {
                if (grid.HexGrid[xPos, yPos + 1].transform.GetChild(0).gameObject.tag != "Mountain" && islandLandMass[xPos, yPos + 1])
                {
                    pos = new Vector2(xPos, yPos + 1);
                    var current = new CalculatePath(pos, CurrentGrid);
                    if (CheckList(pos, Closed) == false && CheckList(pos, Open) == false)
                    {

                        Open.Add(current);
                    }
                }
                else if (grid.HexGrid[xPos, yPos + 1].transform.GetChild(0).gameObject.tag != "Mountain" && islandLandMass[xPos, yPos + 1] == false)
                {
                    waterFound = true;
                }
            }
           
        }
        if (yPos % 2 == 0)
        {
            if (yPos > 0 && xPos > 0)
            {
                if (grid.HexGrid[xPos - 1, yPos - 1].transform.GetChild(0).gameObject.tag != "Mountain" && islandLandMass[xPos - 1, yPos - 1])
                {
                    pos = new Vector2(xPos - 1, yPos -1);
                    var current = new CalculatePath(pos, CurrentGrid);
                    if (CheckList(pos, Closed) == false && CheckList(pos, Open) == false)
                    {

                        Open.Add(current);
                    }
                }
                else if (grid.HexGrid[xPos - 1, yPos - 1].transform.GetChild(0).gameObject.tag != "Mountain" && islandLandMass[xPos - 1, yPos - 1] == false)
                {

                    waterFound = true;
                }
            }
            


            if (yPos > 0)
            {
                if (grid.HexGrid[xPos, yPos - 1].transform.GetChild(0).gameObject.tag != "Mountain" && islandLandMass[xPos, yPos - 1])
                {
                    pos = new Vector2(xPos, yPos-1);
                    var current = new CalculatePath(pos, CurrentGrid);
                    if (CheckList(pos, Closed) == false && CheckList(pos, Open) == false)
                    {

                        Open.Add(current);
                    }
                }
                else if (grid.HexGrid[xPos, yPos - 1].transform.GetChild(0).gameObject.tag != "Mountain" && islandLandMass[xPos, yPos - 1] == false)
                {
                    waterFound = true;
                }
            }
               


            if (yPos < (grid.gridHeight - 1))
            {
                if (grid.HexGrid[xPos, yPos + 1].transform.GetChild(0).gameObject.tag != "Mountain" && islandLandMass[xPos, yPos + 1])
                {
                    pos = new Vector2(xPos, yPos+1);
                    var current = new CalculatePath(pos, CurrentGrid);
                    if (CheckList(pos, Closed) == false && CheckList(pos, Open) == false)
                    {

                        Open.Add(current);
                    }
                }
                else if (grid.HexGrid[xPos, yPos + 1].transform.GetChild(0).gameObject.tag != "Mountain" && islandLandMass[xPos, yPos + 1] == false)
                {
                    waterFound = true;
                }
            }
           


            if (xPos > 0 && yPos < (grid.gridHeight - 1))
            {
                if (grid.HexGrid[xPos - 1, yPos + 1].transform.GetChild(0).gameObject.tag != "Mountain" && islandLandMass[xPos - 1, yPos+1] )
                {
                    pos = new Vector2(xPos - 1, yPos+1);
                    var current = new CalculatePath(pos, CurrentGrid);
                    if (CheckList(pos, Closed) == false && CheckList(pos, Open) == false)
                    {

                        Open.Add(current);
                    }
                }
                else if (grid.HexGrid[xPos - 1, yPos + 1].transform.GetChild(0).gameObject.tag != "Mountain" && islandLandMass[xPos - 1, yPos + 1] == false)
                {
                    waterFound = true;
                }
            }
            
        }
        ////////

    
    }

    //checks if the current cell isnt already in the closed or open lists
    bool CheckList(Vector2 pos, List<CalculatePath> path)
    {
        //set a value to represent if the cell is within the passed list
        bool exist = false;
        //for every item in the list
        for (int i = 0; i < path.Count; i++)
        {
            //if the current position is within the current list
            if (path[i].ReturningPosition() == pos)
            {
                //the current cell is within the list
                exist = true;
                break;
            }
        }
        //return whether the cell is within the list
        return exist;
    }

    //creates a list of everypoint of the path
    void FindPath()
    {
        //add the first point to the path queue
        Path.Enqueue(Closed[PathEnd].ReturningPosition());
        //retreive the current cells parent cell
        CurrentParent = Closed[PathEnd].ReturningParent();

        //while the current parent isnt equal to a zero vector
        while (CurrentParent != Vector2.zero)
        {
            //retrieve the parent cell
            var currentPosition = ParentSearch(CurrentParent);
            //if the parent cells position is equal to a zero vector
            if (Closed[currentPosition].ReturningParent() == Vector2.zero)
            {
                //break from the loop
                break;
            }
            //add the parent cell to the path
            Path.Enqueue(Closed[currentPosition].ReturningPosition());
            //set the current cell to be the parent cell
            CurrentParent = Closed[currentPosition].ReturningParent();
        }
        //set the paths size to be the size of the queue
        int PathSize = Path.Count;
        //create an array storing the positions of every point in the queue
        AIpath = new Vector2[PathSize];
        //for every point in the path
        for (int i = 0; i < PathSize; i++)
        {
            //add the current point to the array
            AIpath[i] = Path.Dequeue();

        }
        //remove all point from the path list
        Path.Clear();
    }
    // return a index value of a parent position
    int ParentSearch(Vector2 Parent)
    {
        //set up a value to store an index
        int pos = 0;
        //for every point in the closes list
        for (int i = 0; i < Closed.Count; i++)
        {
            //if the current position is equal to the parent position
            if (Closed[i].ReturningPosition() == Parent)
            {
                //the index becomes equal to the current parents
                pos = i;
                //break from the loop
                break;
            }
        }
        //return the positions index
        return pos;
    }

    //reset the values of the path found value and clear the open and closes lists
    void resetValues()
    {
        Open.Clear();
        Closed.Clear();
        pathfound = false;
    }

    public Vector2 returnGoal()
    {
        return Goal;
    }

}

//class storing a cells position as well as their parent
class CalculatePath
{
    Vector2 GridPosition, ParentPos;

    //set the values of the grid
    public CalculatePath(Vector2 Position, Vector2 Parent)
    {
        GridPosition = Position;
        ParentPos = Parent;
    }

    //return the current cells position
    public Vector2 ReturningPosition()
    {
        return GridPosition;
    }

    //return the current cells parent
    public Vector2 ReturningParent()
    {
        return ParentPos;
    }
}