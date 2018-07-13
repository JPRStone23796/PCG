using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneratingIsland : MonoBehaviour {

    bool[,] Hexmap;
    int gridWidth, gridHeight;
    CreateGrid grid;
    [SerializeField]
    Material terrain, Mountain, Sea, Desert, taiga, tundra, beach, artic, tropical, lake;
    bool IsStep = false;



    [SerializeField]
    int ProbabilityOfLand;
    [SerializeField]
    int NumberOfSteps;
    [SerializeField]
    float waitBetweenSteps = 0f;
    [SerializeField]
    int CellBreed, CellDeath, CellResurrect;
    [SerializeField]
    int NumberOfMountainAgents, ActionsAgentsCanTake;
    bool bActionTaken;

    List<int> MountainNeighbours;

    List<int> HexagonNeighbours;

    [SerializeField]
    GameObject cityPrefab;

    List<River> MapsRivers;


    int numberOfBiomes = 6;
    List<tile> land;
    List<Vector2> Mountains, Coastline, Direction;
    public List<GameObject> treePrefabs, grasslandPrefabs, tundraPrefabs, desertPrefabs, tropicalPrefabs;
    [SerializeField]
    Sprite tundraUI, taigaUI, grasslandUI, DesertUI, ArticUI, TropicalUI;







    List<int> LandNeighbours;
    List<Vector2> CoastlandTiles, MountainTiles, possibleMountains;

    Text stepCounter, stepTask;





    public bool[,] hexMapIsland()
    {
        return Hexmap;
    }
    //Based on if the generator is in step mode, will run the generation process the co-routine based on this value
    public void MapCreation()
    {
        //If the generator is in step mode
        if (IsStep)
        {
            //create the map step by step
            StartCoroutine(CreateMapSlowly());
        }
        else
        {
            //produce the map instantly
            StartCoroutine(CreateMap());
        }

    }

    //function which sets the values of the generation process, based on the UI
    public void SetValues(int CASteps, int landProb, int Agents, int actions, bool step, Text StepUI, Text TaskUI)
    {
        IsStep = step;
        ProbabilityOfLand = landProb;
        NumberOfSteps = CASteps;
        NumberOfMountainAgents = Agents;
        ActionsAgentsCanTake = actions;
        stepCounter = StepUI;
        stepTask = TaskUI;
    }

    //Sets the intital state of each cell in the hexagonal grid
    void SetCellStates()
    {
        grid = GetComponent<CreateGrid>();    //Sets Variables based off the public CreateGridVariables
        gridWidth = grid.gridWidth;
        gridHeight = grid.gridHeight;

        Hexmap = new bool[gridWidth, gridHeight];   //Sets the bool Array based off the height and width
        for (int i = 0; i < gridHeight; i++)
        {
            for (int j = 0; j < gridWidth; j++)
            {
                Hexmap[i, j] = RandomBool();    //Randomly sets each store in the array to either true or false in order for the first pass of celluar automata to work
            }
        }


    }
    //returns state value based on a number generatred
    bool RandomBool()
    {
        int RN = 0;
        RN = Random.Range(1, 101);     //returns a random cell state based off a percentage
        //initialise the state value
        bool CellState = false;
        //if the number generated is within the land probablility value
        if (RN > (100 - ProbabilityOfLand))
        {
            //set the cells state to be of a land state
            CellState = true;
        }
        return CellState;

    }
    //Generate the map instantly
    IEnumerator CreateMap()
    {
        //set each cells initial state
        SetCellStates();
        int i = 0;
        //complete the set number of cellular passes
        while (i <= NumberOfSteps - 1)
        {
            CompleteGeneration();    //apply the cellular ruleset to each cell within the grid
            SetGrid();
            i++;
            yield return null;
        }
        //Generate the locations of the mountain
        GenerateMountains();
        //generate the locations of the rivers
        GenerateRivers();
        //create the islands coastlines, rivers and cities
        terrainSort();
        //Create the mountain structures in the map
        CreateMountains();
        //set the biomes within the map
        SetBiomes();
        //turn off the hexes UI
        TurnOffUI();
        yield return null;
    }

    //Generate the map step by step
    IEnumerator CreateMapSlowly()
    {
        //calculate the number of steps to be taken
        int numberOfSteps = 6 + NumberOfSteps;
        int currentSteps = 0;
        stepCounter.text = currentSteps.ToString() + " / " + numberOfSteps.ToString();
        stepTask.text = "Grid Creation";
        yield return new WaitUntil(() => (Input.GetButtonDown("Fire1")));
        //set each cells initial state
        SetCellStates();
        SetGrid();
        int i = 0;
        currentSteps++;
        stepCounter.text = currentSteps.ToString() + " / " + numberOfSteps.ToString();
        stepTask.text = "Set Initial Cell States";
        yield return new WaitForSeconds(0.01f);
        yield return new WaitUntil(() => (Input.GetButtonDown("Fire1")));
        //complete the set number of cellular passes
        while (i <= NumberOfSteps - 1)
        {

            CompleteGeneration();   //apply the cellular ruleset to each cell within the grid
            SetGrid();
            i++;
            currentSteps++;
            stepCounter.text = currentSteps.ToString() + " / " + numberOfSteps.ToString();
            stepTask.text = "Cellular Pass " + (i+1).ToString();
            yield return new WaitForSeconds(0.01f);
            yield return new WaitUntil(() => (Input.GetButtonDown("Fire1")));

        }
        yield return new WaitUntil(() => (Input.GetButtonDown("Fire1")));
        currentSteps++;
        stepCounter.text = currentSteps.ToString() + " / " + numberOfSteps.ToString();
        stepTask.text = "Mountain Placement";
        //Generate the locations of the mountain
        GenerateMountains();
        yield return new WaitForSeconds(0.01f);
        yield return new WaitUntil(() => (Input.GetButtonDown("Fire1")));
        currentSteps++;
        stepCounter.text = currentSteps.ToString() + " / " + numberOfSteps.ToString();
        stepTask.text = "Generate Rivers and Cities";
        //generate the locations of the rivers
        GenerateRivers();
        yield return new WaitForSeconds(0.01f);
        yield return new WaitUntil(() => (Input.GetButtonDown("Fire1")));
        currentSteps++;
        stepCounter.text = currentSteps.ToString() + " / " + numberOfSteps.ToString();
        stepTask.text = "Terrain Sculpting";
        //create the islands coastlines, rivers and cities
        terrainSort();
        yield return new WaitForSeconds(0.01f);
        yield return new WaitUntil(() => (Input.GetButtonDown("Fire1")));
        currentSteps++;
        stepCounter.text = currentSteps.ToString() + " / " + numberOfSteps.ToString();
        stepTask.text = "Create Mountain Structures";
        //Create the mountain structures in the map
        CreateMountains();
        yield return new WaitForSeconds(0.01f);
        yield return new WaitUntil(() => (Input.GetButtonDown("Fire1")));
        currentSteps++;
        stepCounter.text = currentSteps.ToString() + " / " + numberOfSteps.ToString();
        stepTask.text = "Biome Creation";
        //set the biomes within the map
        SetBiomes();
        //turn off the hexes UI
        TurnOffUI();
        yield return null;
    }

    //Set the materials of the grid
    void SetGrid()
    {
        //for every row
        for (int i = 0; i < gridWidth; i++)
        {
            //for every column
            for (int j = 0; j < gridHeight; j++)
            {

                GameObject gridUI = grid.HexGrid[i, j].transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).gameObject;

                //if the cell is of a land state
                if (Hexmap[i, j] == true)
                {
                    //set the current cells material
                    grid.HexGrid[i, j].transform.GetChild(0).gameObject.GetComponent<Renderer>().material = terrain;
                    //retrieve the current cells UI element
                    if (!gridUI.activeInHierarchy)
                    {
                        gridUI.SetActive(true);
                    }
                    //set the UI elements text compenent to display its x and y co-ordinates within the map structure
                    grid.HexGrid[i, j].transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Text>().text = (i.ToString() + "/" + (j.ToString()));
                }
                else //if the UI cell is of a sea state
                {
                    //set the UI element to be inactive
                    gridUI.SetActive(false);
                    //set the current cells material
                    grid.HexGrid[i, j].transform.GetChild(0).gameObject.GetComponent<Renderer>().material = Sea;
                }

            }
        }
    }

    //completes a cellular pass
    void CompleteGeneration()
    {
        //for every row
        for (int i = 0; i < gridWidth; i++)
        {
            // for every column
            for (int j = 0; j < gridHeight; j++)
            {
                //update the cells state based on its neighbours
                Hexmap[i, j] = CheckNeighbours(new Vector2(i, j), Hexmap[i, j]);
            }
        }
    }

    //returns a state value for a cell based on its neighbourhood
    bool CheckNeighbours(Vector2 gridPos, bool CellsState)
    {
        //initialises a value which measures the current cells land neighbours
        int totalCellStateOne = 0;
        //examine the current cells moore's neighbourhood within the map structure and if a neighbour is of a land state, add one on the total cell state variable
        if (gridPos.y > 0)
        {
            if (Hexmap[(int)gridPos.x, (int)gridPos.y - 1] == true) { totalCellStateOne++; }

            if (gridPos.x < (gridHeight - 1))
            {
                if (Hexmap[(int)gridPos.x + 1, (int)gridPos.y - 1] == true) { totalCellStateOne++; }
            }

            if (gridPos.x > 0)
            {
                if (Hexmap[(int)gridPos.x - 1, (int)gridPos.y - 1] == true) { totalCellStateOne++; }
            }
            //Not True hex

        }


        if (gridPos.y < (gridHeight - 1))
        {
            if (Hexmap[(int)gridPos.x, (int)gridPos.y + 1] == true) { totalCellStateOne++; }

            if (gridPos.x < (gridHeight - 1))
            {
                if ((Hexmap[(int)gridPos.x + 1, (int)gridPos.y + 1] == true)) { totalCellStateOne++; }
            }

            if (gridPos.x > 0)
            {
                if ((Hexmap[(int)gridPos.x - 1, (int)gridPos.y + 1] == true)) { totalCellStateOne++; }
            }
            //Not True Hex

        }


        if (gridPos.x > 0)
        {
            if (Hexmap[(int)gridPos.x - 1, (int)gridPos.y] == true) { totalCellStateOne++; }
        }


        if (gridPos.x < (gridHeight - 1))
        {
            if (Hexmap[(int)gridPos.x + 1, (int)gridPos.y] == true) { totalCellStateOne++; }
        }
        //apply the cellular ruleset to the cell and determine its state
        if (totalCellStateOne >= CellBreed && CellsState == true) { CellsState = true; }
        if (totalCellStateOne < CellDeath && CellsState == true) { CellsState = false; }
        else if (totalCellStateOne >= CellResurrect && CellsState == false) { CellsState = true; }

        //return the cells new state
        return CellsState;
    }

    //places mountains throughout the map using software agents
    void GenerateMountains()
    {
        //For every mountain agent to be created 
        for (int i = 0; i < NumberOfMountainAgents; i++)
        {
            //Set the current agent actions        
            int currentAgentActions = ActionsAgentsCanTake;
            //Get an inital direction the agent will travel through the grid structure
            Vector2 Direction = CreateInitialAgentDirection();
            //Get an initial location that the agent will start at within the grid structure
            Vector2 location = CreateInitialAgentLocation();
            //While the agent still has actions
            while (currentAgentActions > 0)
            {
                //Check to see if the agents current direction will keep them within the grid structure or if an action has been taken,
                //if it doesn't or if an action has been taken, create a new direction vector that will
                Direction = AgentDirectionCheck(location, Direction);
                //apply the agents direction to it's current location
                location += Direction;
                //if the current tile is of a land state
                if (Hexmap[(int)location.x, (int)location.y])
                {
                    //retreive the current tile object
                    GameObject currentTile = grid.HexGrid[(int)location.x, (int)location.y].transform.GetChild(0).gameObject;
                    if ((currentTile.tag != "Mountain"))
                    {
                        //Set the current tile to be of the mountain tag
                        currentTile.tag = "Mountain";
                        //change the current hexagons material
                        currentTile.GetComponent<Renderer>().material = Mountain;
                        currentAgentActions--;
                        //action has been taken so a new direction will be generated
                        bActionTaken = true;
                    }
                }
            }
        }
    }

    //Return a random location within the map structure
    Vector2 CreateInitialAgentLocation()
    {
        //generate a random x and y position
        int xDir = Random.Range(0, (gridWidth - 1));
        int yDir = Random.Range(0, (gridHeight - 1));
        //set up the vector using the generated values
        Vector2 location = new Vector2(xDir, yDir);
        //return the location vecttor
        return location;
    }

    //returns a random initial direction for an agent
    Vector2 CreateInitialAgentDirection()
    {
        //set up the direction values
        int xDir = 0;
        int yDir = 0;
        //while a direction hasnt been generated
        while (xDir == 0 && yDir == 0)
        {
            // generate random direction values
            xDir = Random.Range(-1, 2);
            yDir = Random.Range(-1, 2);
        }
        //set up the direction vector
        Vector2 direction = new Vector2(xDir, yDir);
        //return the generate direction vector
        return direction;
    }

    //check to see if an agents is currently moving within the bounds of the map structrue, and if not generatre them a direction which will
    Vector2 AgentDirectionCheck(Vector2 location, Vector2 direction)
    {
        //if the agent isnt within the bounds of the map or if a mountain has been created
        if ((location.x + direction.x < 0) || (location.x + direction.x > (gridHeight - 1)) || bActionTaken)
        {
            bool NewDir = false;
            //while a new direction vector has not been generated
            while (NewDir == false)
            {
                //generate a new x direction 
                int xDir = Random.Range(-1, 2);
                //update the direction vector with the generate x direction value
                direction = new Vector2(xDir, direction.y);
                //if the new direction is within the bounds of the map structure
                if ((location.x + direction.x > 0) && (location.x + direction.x < (gridHeight - 1)))
                {
                    //new direction has been generated
                    NewDir = true;
                }
            }
        }


        //if the agent isnt within the bounds of the map or if a mountain has been created
        if ((location.y + direction.y < 0) || (location.y + direction.y > (gridWidth - 1)) || bActionTaken)
        {
            bool NewDir = false;
            //while a new direction vector has not been generated
            while (NewDir == false)
            {
                //generate a new x direction 
                int yDir = Random.Range(-1, 2);
                //update the direction vector with the generate x direction value
                direction = new Vector2(direction.x, yDir);
                //if the new direction is within the bounds of the map structure
                if ((location.y + direction.y > 0) && (location.y + direction.y < (gridWidth - 1)))
                {
                    //new direction has been generated
                    NewDir = true;
                }
            }
        }

        //return the direction vector
        return direction;
    }

    //attach the mountain creation script to the placed mountain tiles
    void CreateMountains()
    {
        //for every row
        for (int i = 0; i < gridWidth; i++)
        {
            //for every column
            for (int j = 0; j < gridHeight; j++)
            {
                //retreieve the current hexagon
                GameObject currentHex = grid.HexGrid[i, j].transform.GetChild(0).gameObject;
                //if the current hexagon is tagged as a mountain tile and doesnt already have a mountain creation script attached to it
                if ((currentHex.tag == "Mountain") && !currentHex.GetComponent<MountainCreation>())
                {
                    //retreieve which of the currents cell neighbours are also of a mountain state
                    MountainNeighbourhood(i, j);
                    //attach the mountain creation script to the current hexagon
                    currentHex.AddComponent<MountainCreation>();
                    //transfer which of the currents cell neighbours are also of a mountain state
                    currentHex.GetComponent<MountainCreation>().SetBorders(MountainNeighbours);
                    //create the mountain structure 
                    currentHex.GetComponent<MountainCreation>().Create();
                }
            }
        }
    }

   //reviews a cells neighbourhood and adds those which have the mountain tag to a list
    void MountainNeighbourhood(int xPos, int yPos)
    {
        //set up the mountain neighbourhood list
        MountainNeighbours = new List<int>();
        //examine each of the current cells honeycomb neighbourhood and if any are tagged as a mountain, add them to the list of mountain neighbours
        if (xPos > 0)
        {
            if (grid.HexGrid[xPos - 1, yPos].transform.GetChild(0).gameObject.tag == "Mountain")
            {
                MountainNeighbours.Add(2);
            }
        }
        if (xPos < (gridWidth - 1))
        {
            if (grid.HexGrid[xPos + 1, yPos].transform.GetChild(0).gameObject.tag == "Mountain")
            {
                MountainNeighbours.Add(14);
            }
        }
        if (yPos % 2 != 0)
        {
            if (yPos > 0)
            {
                if (grid.HexGrid[xPos, yPos - 1].transform.GetChild(0).gameObject.tag == "Mountain")
                {
                    MountainNeighbours.Add(6);
                }
            }


            if (xPos < (gridWidth - 1) && yPos > 0)
            {
                if (grid.HexGrid[xPos + 1, yPos - 1].transform.GetChild(0).gameObject.tag == "Mountain")
                {
                    MountainNeighbours.Add(10);
                }
            }


            if (xPos < (gridWidth - 1) && yPos < (gridHeight - 1))
            {
                if (grid.HexGrid[xPos + 1, yPos + 1].transform.GetChild(0).gameObject.tag == "Mountain")
                {
                    MountainNeighbours.Add(18);
                }
            }


            if (yPos < (gridHeight - 1))
            {
                if (grid.HexGrid[xPos, yPos + 1].transform.GetChild(0).gameObject.tag == "Mountain")
                {
                    MountainNeighbours.Add(22);
                }
            }
        }
        if (yPos % 2 == 0)
        {
            if (yPos > 0 && xPos > 0)
            {
                if (grid.HexGrid[xPos - 1, yPos - 1].transform.GetChild(0).gameObject.tag == "Mountain")
                {
                    MountainNeighbours.Add(6);
                }
            }


            if (yPos > 0)
            {
                if (grid.HexGrid[xPos, yPos - 1].transform.GetChild(0).gameObject.tag == "Mountain")
                {
                    MountainNeighbours.Add(10);
                }
            }


            if (yPos < (gridHeight - 1))
            {
                if (grid.HexGrid[xPos, yPos + 1].transform.GetChild(0).gameObject.tag == "Mountain")
                {
                    MountainNeighbours.Add(18);
                }
            }


            if (xPos > 0 && yPos < (gridHeight - 1))
            {
                if (grid.HexGrid[xPos - 1, yPos + 1].transform.GetChild(0).gameObject.tag == "Mountain")
                {
                    MountainNeighbours.Add(22);
                }
            }
        }
    }




    //function responsible for sculpting the coastlines based on their neighbourhood
    void terrainSort()
    {
        //for every row
        for (int i = 0; i < gridWidth; i++)
        {
            //for ever column
            for (int j = 0; j < gridHeight; j++)
            {
                //if the current cell is of a land state
                if (Hexmap[i, j] == true)
                {
                    //examine the cells neighbourhood and sculpt a coastline if necessary
                    CheckNeighbourhood(i, j);
                }
            }
        }
    }

    void CheckNeighbourhood(int xPos, int yPos)
    {
        //initialises a value which measures the current cells land neighbours
        HexagonNeighbours = new List<int>();



        //examine the current cells moore's neighbourhood within the map structure and if a neighbour is of a land state, add it to the list of neighbours
        if (xPos > 0)
        {
            if (Hexmap[xPos - 1, yPos] == false)
            {
                HexagonNeighbours.Add(2);
            }
        }
        if (xPos < (gridWidth - 1))
        {
            if (Hexmap[xPos + 1, yPos] == false)
            {
                HexagonNeighbours.Add(14);
            }
        }
        if (yPos % 2 != 0)
        {
            if (yPos > 0)
            {
                if (Hexmap[xPos, yPos - 1] == false)
                {
                    HexagonNeighbours.Add(6);
                }
            }


            if (xPos < (gridWidth - 1) && yPos > 0)
            {
                if (Hexmap[xPos + 1, yPos - 1] == false)
                {
                    HexagonNeighbours.Add(10);
                }
            }


            if (xPos < (gridWidth - 1) && yPos < (gridHeight - 1))
            {
                if (Hexmap[xPos + 1, yPos + 1] == false)
                {
                    HexagonNeighbours.Add(18);
                }
            }


            if (yPos < (gridHeight - 1))
            {
                if (Hexmap[xPos, yPos + 1] == false)
                {
                    HexagonNeighbours.Add(22);
                }
            }
        }

        if (yPos % 2 == 0)
        {
            if (yPos > 0 && xPos > 0)
            {
                if (Hexmap[xPos - 1, yPos - 1] == false)
                {
                    HexagonNeighbours.Add(6);
                }
            }


            if (yPos > 0)
            {
                if (Hexmap[xPos, yPos - 1] == false)
                {
                    HexagonNeighbours.Add(10);
                }
            }


            if (yPos < (gridHeight - 1))
            {
                if (Hexmap[xPos, yPos + 1] == false)
                {
                    HexagonNeighbours.Add(18);
                }
            }


            if (xPos > 0 && yPos < (gridHeight - 1))
            {
                if (Hexmap[xPos - 1, yPos + 1] == false)
                {
                    HexagonNeighbours.Add(22);
                }
            }
        }


        // retrieve the hexagon object
        GameObject gridPiece = grid.HexGrid[xPos, yPos].transform.GetChild(0).gameObject;
        //if the current object doesn't already have the terrain sculpting script attached to it
        if (gridPiece.GetComponent<TerrainSculpting>() == false)
        {
            //attach the terrain sculpting script to the object
            gridPiece.gameObject.AddComponent<TerrainSculpting>();
        }
        //sculpt the current objects mesh using the list of neighbours which are of a land state
        gridPiece.GetComponent<TerrainSculpting>().AlterHex(HexagonNeighbours);

    }






    void SetBiomes()
    {
        //Set Up the different biome types and enter their appropriate values
        biomeType[] worldBiomes = new biomeType[6];
        for(int i=0;i<numberOfBiomes; i++)
        {
            worldBiomes[i] = new biomeType();
        }
        worldBiomes[0].setBiomes("tundra",tundra,tundraUI,1,3,1,3);
        worldBiomes[1].setBiomes("taiga",taiga,taigaUI,3,4,1,3);
        worldBiomes[2].setBiomes("grassland",terrain,grasslandUI,1,6,3,4);
        worldBiomes[3].setBiomes("desert",Desert,DesertUI,1,3,4,6);
        worldBiomes[4].setBiomes("artic",  artic, ArticUI,4,6,1,3);
        worldBiomes[5].setBiomes("tropical", tropical, TropicalUI,3,6,4,6);


        //create new lists to store every land tile, mountain tile and coastline tile
        land = new List<tile>();
        Mountains = new List<Vector2>();
        Coastline = new List<Vector2>();


        //for every row
        {  for (int i = 0; i < gridWidth; i++)
      
            //for every column
            for (int j = 0; j < gridHeight; j++)
            {
                //if the tile is of a land state but isnt also a mountain tile
                if (Hexmap[i, j] == true && !grid.HexGrid[i,j].transform.GetChild(0).gameObject.GetComponent<MountainCreation>())
                {
                    //add this new tile to the list of land tiles
                    tile temp = new tile();
                    temp.tilePos = new Vector2(i, j);
                    temp.tileObj = grid.HexGrid[i, j].transform.GetChild(0).gameObject;
                    land.Add(temp);
                }
                //if the tile is of a land state but is also a mountain tile
                else if(Hexmap[i, j] == true && grid.HexGrid[i, j].transform.GetChild(0).gameObject.GetComponent<MountainCreation>())
                {
                    //add this grid positions of this cell to the mountain list
                    Mountains.Add(new Vector2(i, j));
                }
                //if the tile is of a sea state
                else
                {
                    //add this grid positions of this cell to the coastline list
                    Coastline.Add(new Vector2(i, j));
                }
            }
        }


        //for every land tile
        for(int i=0;i<land.Count;i++)
        {
            //initialise the tiles lowest distance values to a high value
            float lowestDistanceToMountain = 200f;
            float lowestDistanceToWater = 200f;
            //retrieve the current tile
            tile currentTile = land[i];

            //for every mountain tile
            for(int j=0;j<Mountains.Count;j++)
            {

                //calculate distance between the current tile and the current mountain tile using their x and y co-ordinates appropriately
                float xValue = Mathf.Abs(currentTile.tilePos.x - Mountains[j].x);
                float yValue = Mathf.Abs(currentTile.tilePos.y - Mountains[j].y);
                float currentDistance = xValue + yValue;
                //if the current distance is the lowest for this tile
                if (currentDistance<lowestDistanceToMountain)
                {
                    //the lowest distance becomes the current distance
                    lowestDistanceToMountain = currentDistance;
                }
            }

            //for every coastline tile
            for (int j = 0; j < Coastline.Count; j++)
            {
                //calculate distance between the current tile and the current sea tile using their x and y co-ordinates appropriately
                float xValue = Mathf.Abs(currentTile.tilePos.x - Coastline[j].x);
                float yValue = Mathf.Abs(currentTile.tilePos.y - Coastline[j].y);
                float currentDistance = xValue + yValue;
                //if the current distance is the lowest for this tile
                if (currentDistance < lowestDistanceToWater)
                {
                    //the lowest distance becomes the current distance
                    lowestDistanceToWater = currentDistance;
                }
            }

            //set up an index for the current tiles biome
            int BiomeSelected = 0;
            //calculate the distance between the current tile and the maps equator
            Vector2 EquatorPoint = new Vector2((gridWidth / 2) - 1, currentTile.tilePos.y);
            float distanceFromEqautor = (currentTile.tilePos - EquatorPoint).magnitude;

            //for every type of biome
            for (int z=0;z<numberOfBiomes;z++)
            {
                //use the current tiles distance to a mountain and water tile as well as its distance to the equator, and half the size of the equator
                //if the current tile is within the current biome
                if(worldBiomes[z].WithinBiome(lowestDistanceToWater,lowestDistanceToMountain,distanceFromEqautor, (gridWidth/2)))
                {
                    // biome chosen is the current biome
                    BiomeSelected = z;
                }
              
                           
            }
            // if the lowest distance to a mountain tile is less than 3
            if(lowestDistanceToMountain<3)
            {
                //if the current tile doesnt contain a city
                if(currentTile.tileObj.tag != "City")
                {
                    //add noise to the tile, creating hills
                    currentTile.tileObj.GetComponent<TerrainSculpting>().AddNoise(gridWidth, gridHeight, currentTile.tilePos, currentTile.tileObj.transform.GetChild(0).gameObject);
                }

            }
            //update the tiles material to its appropriate biome type
            currentTile.tileObj.gameObject.GetComponent<Renderer>().material = worldBiomes[BiomeSelected].getMaterial();
            //set the biomes UI text to be of the biome types
            currentTile.tileObj.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Text>().text = worldBiomes[BiomeSelected].getType();
            //set the current tiles UI icon to be of the biomes types
            GameObject Icon = currentTile.tileObj.transform.GetChild(0).transform.GetChild(1).gameObject;
            Icon.SetActive(true);
            Icon.GetComponent<Image>().sprite = worldBiomes[BiomeSelected].getIcon();

          
            //if the current tile isnt a lake or a city
            if (currentTile.tileObj.tag!="Lake" && currentTile.tileObj.tag != "City")
            {
                //if the current biome is a taiga
                if (worldBiomes[BiomeSelected].getType() == "taiga")
                {
                    //if the current tile doesn't contain a terrain sculpting compenent
                    if (!(currentTile.tileObj.GetComponent<TerrainSculpting>()))
                    {
                        //add the terrain sculpting component to the object
                        currentTile.tileObj.gameObject.AddComponent<TerrainSculpting>();
                    }
                    //spawn the biomes specific objects on the current tile
                    currentTile.tileObj.GetComponent<TerrainSculpting>().AddPrefab(treePrefabs, 90);
                }
                //if the current biome is a grassland
                if (worldBiomes[BiomeSelected].getType() == "grassland")
                {
                    //if the current tile doesn't contain a terrain sculpting compenent
                    if (!(currentTile.tileObj.GetComponent<TerrainSculpting>()))
                    {
                        //add the terrain sculpting component to the object
                        currentTile.tileObj.gameObject.AddComponent<TerrainSculpting>();
                    }
                    //spawn the biomes specific objects on the current tile
                    currentTile.tileObj.GetComponent<TerrainSculpting>().AddPrefab(grasslandPrefabs, 95);
                }
                //if the current biome is a desert
                if (worldBiomes[BiomeSelected].getType() == "desert")
                {
                    //if the current tile doesn't contain a terrain sculpting compenent
                    if (!(currentTile.tileObj.GetComponent<TerrainSculpting>()))
                    {
                        //add the terrain sculpting component to the object
                        currentTile.tileObj.gameObject.AddComponent<TerrainSculpting>();
                    }
                    //spawn the biomes specific objects on the current tile
                    currentTile.tileObj.GetComponent<TerrainSculpting>().AddPrefab(desertPrefabs, 99);
                }             
            }

        }

        MountainTiles.Clear();
        CoastlandTiles.Clear();
        land.Clear();

    }




    // create a list of direction vectors which will select a neighbouring hexagon
    void SetDirectionVectors()
    {
        Direction = new List<Vector2>();
        Direction.Add(new Vector2(-1, 0));
        Direction.Add(new Vector2(-1, 1));
        Direction.Add(new Vector2(0, -1));
        Direction.Add(new Vector2(0, 1));
        Direction.Add(new Vector2(1, 0));
        Direction.Add(new Vector2(1, 1));
    }




    // function which generates rivers within the map
    void GenerateRivers()
    {
        //set up lists for coastland, mountain, possible mountains tiles
        CoastlandTiles = new List<Vector2>();
        MountainTiles = new List<Vector2>();
        possibleMountains =  new List<Vector2>();
        //set a list containing every item available
        MapsRivers = new List<River>();
        //set up the direction vectors used to view a hexagons neighbours within a honeycomb neighbourhood
        SetDirectionVectors();
        //for every row
        for (int i = 0; i < gridWidth; i++)
        {
            //for every column
            for (int j = 0; j < gridHeight; j++)
            {
                //if the tile is of a sea state
                if (Hexmap[i, j] == false)
                {
                    //create a list of the current tiles land neighbours
                    LandNeighbours = new List<int>();
                    CoastlineNeighbourhood(i, j);
                    //if the tile has a land neighbour
                    if (LandNeighbours.Count > 0)
                    {
                        //add the current tile to the coastland tile list
                        Vector2 temp = new Vector2();
                        temp = new Vector2(i, j);
                        CoastlandTiles.Add(temp);
                    }

                }
                //if the tile is of a land state
                else
                {
                    //if the tile has the mountain tag
                    if(grid.HexGrid[i,j].transform.GetChild(0).gameObject.tag == "Mountain")
                    {
                        //add the current tile to the mountain list
                        Vector2 temp = new Vector2();
                        temp = new Vector2(i, j);
                        MountainTiles.Add(temp);
                    }
                }
            }
        }


        // for every mountain tike
        for(int i=0;i<MountainTiles.Count;i++)
        {
            //set up a value to store the current mountains tile lowest distance to water
            float lowestDistance = 200f;
            //for every coastland tiles
            for(int j=0;j<CoastlandTiles.Count; j++)
            {
                //get the distance between the current mountian and coastland tile
                float distance = Vector2.Distance(CoastlandTiles[j], MountainTiles[i]);
                //if the distance is lower than the lowest distance
                if(distance<lowestDistance)
                {
                    //the lowest distance becomes the current distance
                    lowestDistance = distance;
                }
            }
            //if the mountains lowest distance was 3
            if(lowestDistance>=3)
            {
                //add the current mountain to the list of possible mountains to create rivers from
                possibleMountains.Add(MountainTiles[i]);
            }
        }






        //if their are mountains which rivers are possible from
        if(possibleMountains.Count>0)
        {
            //set up a value representing the number of rivers there will be
            int numberOfRiverAgents = 3;

            //if there are not enough mountains to create 3 rivers
            if(numberOfRiverAgents>possibleMountains.Count)
            {
                //set the amount of rivers to be the same value as the amount of possible rivers
                numberOfRiverAgents = possibleMountains.Count;
            }
            //set up a value representing the number of rivers currently created
            int agentsCreated = 0;

            // while the set number of rivers havent been created
            while(agentsCreated!=numberOfRiverAgents)
            {
                //set up values representing if a coastland tile has been chosen and if a river is possible
                bool CoastlineFound = false;
                bool riverpossible = true;
                //set up a value representing the index of a coastland tile chosen at random
                int CoastlineRNG = 0;
                //randomly select a mountain from the possible mountain list
                int MountainRNG = Random.Range(0, possibleMountains.Count);
                //list containing all the coastline tiles already selected in which a river is not possible
                List<Vector2> coastlandPointNotPossible = new List<Vector2>();
                //while a coastline tile has not been chosen and a river is possible
                while (CoastlineFound == false && riverpossible)
                {
                    //randomly select a coastline tile
                    CoastlineRNG = Random.Range(0, CoastlandTiles.Count);
                    //calculate the distance between the current mountain and coastland tile
                    float distance = Vector2.Distance(CoastlandTiles[CoastlineRNG], possibleMountains[MountainRNG]);
                    //if the distance is within these ranges
                    if (distance >= 3 && distance<20)
                    {
                        //coastline tile has been found
                        CoastlineFound = true;
                    }
                    //if the distance is not within those ranges
                    else
                    {
                        //if the point is not already within the list
                        if (!coastlandPointNotPossible.Contains(CoastlandTiles[CoastlineRNG]))
                        {
                            // add it to the list of coastland tiles not possible
                            coastlandPointNotPossible.Add(CoastlandTiles[CoastlineRNG]);
                        }
                
                    }
                    //  if every coastland tile is not within the selected ranges
                    if (coastlandPointNotPossible.Count == CoastlandTiles.Count)
                    {
                        //the river is not possible
                        riverpossible = false;
                    }

                }

                // if the river is possible
                if (riverpossible)
                {
                    //get the river path finder compenent
                    RiverPathfinder path = GetComponent<RiverPathfinder>();
                    //create a list which will store the points within the grid which will be river
                    List<Vector2> pathPoints = new List<Vector2>();
                    //retrieve a list of cell positions which will act as the path between the chosen mountain and coastland tile
                    pathPoints = path.CreatePath(CoastlandTiles[CoastlineRNG], possibleMountains[MountainRNG]);
                    //if the path contains items
                    if (pathPoints.Count > 0)
                    {
                        //create a new river structure
                        River currentRiver = new River();
                        //set up the list of points within the new river
                        currentRiver.RiverPoints = new List<Vector2>();

                        //for every item in the path list
                        for (int j = 0; j < pathPoints.Count; j++)
                        {
                            //retreive the current items co-ordinates
                            int xPos = (int)pathPoints[j].x;
                            int yPos = (int)pathPoints[j].y;
                            //set the current items hexagonal object to have the Lake tag
                            grid.HexGrid[xPos, yPos].transform.GetChild(0).gameObject.tag = "Lake";
                            //add the current items co-ordinates to the river structure
                            currentRiver.RiverPoints.Add(new Vector2(xPos, yPos));
                            //for every other item in the list
                            if (j % 2 == 0)
                            {
                                //set up a bool representing if an offshoot tile has been selected
                                bool chosen = false;
                                //while an offshoot tile has not been selected
                                while (chosen == false)
                                {
                                    //pick a random neighbour direction
                                    int rng = Random.Range(0, Direction.Count);

                                    //retreive the x and y co-ordinates of the neighbouring cell
                                    Vector2 pos = pathPoints[j] + Direction[rng];
                                    xPos = (int)pos.x;
                                    yPos = (int)pos.y;
                                    //set up values to the store the previous and next points the rivers list
                                    Vector2 previousPos = Vector2.zero;
                                    Vector2 nextPos = Vector2.zero;
                                    //set the values of the previous and next positions appropriately
                                    if (j > 1)
                                    {
                                        previousPos = pathPoints[j - 1];
                                    }
                                    if (j < pathPoints.Count - 1)
                                    {
                                        previousPos = pathPoints[j + 1];
                                    }

                                    //if the selected offshoot tile isnt the previous or next tile
                                    if (pos != previousPos && pos != nextPos)
                                    {
                                        //if the selected offshoot tile is within the bounds of the grids structure
                                        if ((xPos >= 0 && xPos < gridWidth) && (yPos >= 0 && yPos < gridHeight))
                                        {
                                            //set the current tile to have the lake tag
                                            grid.HexGrid[xPos, yPos].transform.GetChild(0).gameObject.tag = "Lake";
                                            //add the current tile to the river structure
                                            currentRiver.RiverPoints.Add(new Vector2(xPos, yPos));
                                            //a tile has now been chosen
                                            chosen = true;
                                        }
                                    }
                                }


                            }
                        }
                        //add the current river structure to the list of rivers
                        MapsRivers.Add(currentRiver);
                    }
                }
                //remove the current possible mountain from the list
                possibleMountains.Remove(possibleMountains[MountainRNG]);
                //increase the amount of rivers by 1
                agentsCreated++;
                
            }
        }
        // set the lake tiles materials
        SetLakes();

    }


    //sets the lake tiles materials
    void SetLakes()
    {
        //for every row
        for (int i = 0; i < gridWidth; i++)
        {
            //for every column
            for (int j = 0; j < gridHeight; j++)
            {
                //retreive the current cells hexagonal object
                GameObject currentHex = grid.HexGrid[i, j].transform.GetChild(0).gameObject;
                //if the hexagonal object has the tag lake
                if ((currentHex.tag == "Lake"))
                {
                    //set its material 
                    grid.HexGrid[i,j].transform.GetChild(0).gameObject.GetComponent<Renderer>().material = lake;
                    //set it to be of a water state
                    Hexmap[i, j] = false;
                }
            }
        }
        //create cities within the map structurw
        PlaceCities();
    }


    void CoastlineNeighbourhood(int xPos, int yPos)
    {
        //initialises a list  which stores the current cells land neighbours
        LandNeighbours = new List<int>();



        //examine the current cells neighbourhood within the map structure and if a neighbour is of a land state, add it to the list of land neighbours
        if (xPos > 0)
        {
            if (Hexmap[xPos - 1, yPos])
            {
                LandNeighbours.Add(2);
            }
        }

        if (xPos < (gridWidth - 1))
        {
            if (Hexmap[xPos + 1, yPos])
            {
                LandNeighbours.Add(14);
            }
        }



        if (yPos % 2 != 0)
        {
            if (yPos > 0)
            {
                if (Hexmap[xPos, yPos - 1])
                {
                    LandNeighbours.Add(6);
                }
            }


            if (xPos < (gridWidth - 1) && yPos > 0)
            {
                if (Hexmap[xPos + 1, yPos - 1])
                {
                    LandNeighbours.Add(10);
                }
            }


            if (xPos < (gridWidth - 1) && yPos < (gridHeight - 1))
            {
                if (Hexmap[xPos + 1, yPos + 1])
                {
                    LandNeighbours.Add(18);
                }
            }


            if (yPos < (gridHeight - 1))
            {
                if (Hexmap[xPos, yPos + 1])
                {
                    LandNeighbours.Add(22);
                }
            }
        }


        if (yPos % 2 == 0)
        {
            if (yPos > 0 && xPos > 0)
            {
                if (Hexmap[xPos - 1, yPos - 1])
                {
                    LandNeighbours.Add(6);
                }
            }


            if (yPos > 0)
            {
                if (Hexmap[xPos, yPos - 1])
                {
                    LandNeighbours.Add(10);
                }
            }


            if (yPos < (gridHeight - 1))
            {
                if (Hexmap[xPos, yPos + 1])
                {
                    LandNeighbours.Add(18);
                }
            }


            if (xPos > 0 && yPos < (gridHeight - 1))
            {
                if (Hexmap[xPos - 1, yPos + 1])
                {
                    LandNeighbours.Add(22);
                }
            }
        }
    }




 
    void PlaceCities()
    {
        //set up a clas which stores possible city names
        CityNames names = new CityNames();
        //for every river created
        for (int i=0;i<MapsRivers.Count;i++)
        {
            //value which stores if a tile to place a city has been selected
            bool selected = false;
            //while a tile hasnt been selected to place a city
            while(selected ==false)
            {
                //select a tile from the current river structure
                int selectedTile = Random.Range(0, MapsRivers[i].RiverPoints.Count);
                //set up the list which will select the current tiles neighbours
                SetDirectionVectors();
                // while a neighbour has not been chosen
                bool directionChosen = false;
                while(directionChosen==false)
                {
                    //select a neighbour of the current cell
                    int cityPlacement = Random.Range(0, Direction.Count);
                    Vector2 selectedCityPlacment = MapsRivers[i].RiverPoints[selectedTile] + Direction[cityPlacement];
                    //retreive the selected neighbours co-ordinates within the grid strucutre
                    int xpos = (int)selectedCityPlacment.x;
                    int ypos = (int)selectedCityPlacment.y;
                    //if the current neighour is within the bounds of the map structure
                    if ((xpos >= 0 && xpos < gridWidth) && (ypos >= 0 && ypos < gridHeight))
                    {
                        //retrieve the selected tiles hexagonal object
                        GameObject currentHex = grid.HexGrid[xpos, ypos].transform.GetChild(0).gameObject;

                        //if the current tile is not a lake or mountain tile, and is of a land state
                        if (currentHex.tag != "Lake" && currentHex.tag != "Mountain" && Hexmap[xpos,ypos])
                        {
                            //a direction has been selected
                            directionChosen = true;
                            //if the terrain sculpting component has not been added
                            if (!currentHex.GetComponent<TerrainSculpting>())
                            {
                                //add the terrain sculpting component
                                currentHex.AddComponent<TerrainSculpting>();
                            }
                            //select an name from the city name structure
                            int cityNameSelection = Random.Range(0, names.returnSize());
                            //retrieve the chosen name and remove it from the city name structure
                            string chosenName = names.SelectCity(cityNameSelection);
                            //place the city object and set its name to the one selected
                            currentHex.GetComponent<TerrainSculpting>().PlaceCity(cityPrefab,chosenName);
                            //set the current tile's tag to be city
                            currentHex.tag = "City";
                            // a city object has been placed
                            selected = true;
                        }
                        //if the current tile is a lake, mountain or is a sea tile
                        else
                        {
                            //remove the neighbour direction from the list
                            Direction.Remove(Direction[cityPlacement]);
                            //if none of the neighbours allow for a city to be placed
                            if (Direction.Count <= 0)
                            {
                                //direction has been chosen but a tile has not been selected
                                directionChosen = true;
                            }
                        }
                    }
                }
            }
        }
    }



    public void TurnOffUI()
    {
        //for every row
        for (int i = 0; i < gridWidth; i++)
        {
            //for eveery column
            for (int j = 0; j < gridHeight; j++)
            {
                //retrieve the current cells UI object
                GameObject currentHex = grid.HexGrid[i, j].transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
                //if the current UI object is active
                if (currentHex.activeInHierarchy)
                {
                    //set the current UI object to be inactive
                    currentHex.SetActive(false);
                }
                else
                {
                    //set the current UI object to be active
                    currentHex.SetActive(true);
                }

            }
        }
    }


}
public class biomeType
{
    //string represting the biomes name
    string biomeName;
    //material which represents the biomes material
    Material biomeMat;
    //sprite which represents the biomes icon
    Sprite icon;
    //values which are used to calculate if a tile is the current biomes range
    int lowerMoistureLimit, upperMoistureLimit, lowerTempurature, UpperTempature;

    //constructor
    public biomeType()
    {
        biomeName = "";
        biomeMat = null;
        icon = null;
    }

    //set the biomes values
    public void setBiomes(string biome, Material mat, Sprite uiIcon,int lowerwaterLimit,int upperWaterLimit, int lowerTempLimit, int upperTempLimit)
    {
        biomeName = biome;
        biomeMat = mat;
        icon = uiIcon;
        lowerMoistureLimit = lowerwaterLimit;
        upperMoistureLimit = upperWaterLimit;
        lowerTempurature = lowerTempLimit;
        UpperTempature = upperTempLimit;
        
    }

    //retrieve the current biomes name
   public string getType()
    {
        return biomeName;
    }

    //retrieve the current biomes material
    public Material getMaterial()
    {
        return biomeMat;
    }
    //retreieve the current biomes icon
    public Sprite getIcon()
    {
        return icon;
    }



    //calculatw whether a tile is within a current biom
    public bool WithinBiome(float distanceFromWater, float distanceFromMountain, float distanceFromEquator, int MaxDistanceFromEquator)
    {
        //calculate a temperate float using the current tiles distance from a mountain and from the equator
        float temperature = (distanceFromMountain/2) + (MaxDistanceFromEquator - distanceFromEquator);
        // calculate a moisture float using the current tiles distance from water
        float moistureLevel = distanceFromWater;
        //value representing if the current tile is within the current biome
        bool bWithinBiome = false;
        //assign temperature an interger value based upon the following ranges
        if(temperature>=0 && temperature<5)
        {
            temperature = 1;
        }
        else if(temperature>=5 && temperature<10)
        {
            temperature = 2;
        }
        else if(temperature >= 10 && temperature < 12)
        {
            temperature = 3;
        }
        else if (temperature >= 12 && temperature < 16)
        {
            temperature = 4;
        }
        else if (temperature >=16)
        {
            temperature = 5;
        }

        //assign moisture level an interger value based upon the following ranges
        if (moistureLevel >= 0 && moistureLevel < 1.5f)
        {
            moistureLevel = 5;
        }
        else if (moistureLevel >= 1.5f && moistureLevel < 3f)
        {
            moistureLevel = 4;
        }
        else if (moistureLevel >= 3 && moistureLevel < 4.5f)
        {
            moistureLevel = 3;
        }
        else if (moistureLevel >= 4.5f && moistureLevel < 6f)
        {
            moistureLevel = 2;
        }
        else if (moistureLevel >= 6f)
        {
            moistureLevel = 1;
        }

        //if the temperature and moisture values are within this current range
        if (temperature>=lowerTempurature && temperature<UpperTempature)
        {
            if(moistureLevel>=lowerMoistureLimit && moistureLevel<upperMoistureLimit)
            {
                //current tile is within biom
                bWithinBiome = true;
            }
        }

        return bWithinBiome;
    }
}


public struct River
{
    //list containing the co-ordinates of every tile that it is made up of
    public List<Vector2> RiverPoints;
}

public struct tile
{
    //representing the tile's co-ordinates within the grid structure
    public Vector2 tilePos;
    //representing the hexagonal object of the tile
    public GameObject tileObj;
}




class CityNames
{
    //list which contains a list of potential city names
    List<string> SelectionOfCityNames;
    public CityNames()
    {
        //set up list and add a variety of names to it
        SelectionOfCityNames = new List<string>();
        SelectionOfCityNames.Add("London");
        SelectionOfCityNames.Add("Chester");
        SelectionOfCityNames.Add("Manchester");
        SelectionOfCityNames.Add("Birmingham");
        SelectionOfCityNames.Add("Leeds");
        SelectionOfCityNames.Add("Bristol");
        SelectionOfCityNames.Add("York");
        SelectionOfCityNames.Add("Liverpool");
        SelectionOfCityNames.Add("Cambridge");
        SelectionOfCityNames.Add("Sheffield");
        SelectionOfCityNames.Add("Leicester");
        SelectionOfCityNames.Add("Nottingham");
        SelectionOfCityNames.Add("Beeston");
    }

    //return the amount of names remaining
    public int returnSize()
    {
        return SelectionOfCityNames.Count;
    }

    //return a city name and then remove it from the list
    public string SelectCity(int index)
    {
        //retreive a city name based on a index value
        string selection = SelectionOfCityNames[index];
        //remove item from list
        SelectionOfCityNames.RemoveAt(index);
        //return city name
        return selection;

    }
}

