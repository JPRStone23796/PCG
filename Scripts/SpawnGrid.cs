using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnGrid : MonoBehaviour {

    [SerializeField]
    GameObject Grid;
    GameObject currentGrid;

    [SerializeField]
    Slider MountainAgentsSlider, landChance, cellularPasses, ActionsSlider;
    [SerializeField]
    GameObject SliderUI, ControlsUI;

    [SerializeField]
    Text MountainAgentsValue, landChanceValue, cellularPassesValue, ActionsSliderValue;

    [SerializeField]
    Toggle byStepToggle;


    [SerializeField]
    Text StepCounterText,StepTaskText;
    [SerializeField]
    GameObject StepCounter;

    //creates the grid once a button is pressed
    public void CreateGrid()
    {
        //if a grid already exists
        if (currentGrid != null)
        {
            //destroy the current map
            Destroy(currentGrid);
        }
        //if the controls UI hasnt been turned off
        if (ControlsOpen == true)
        {
            //set the controls UI to be active
            ControlsUI.gameObject.SetActive(true);
            //if the by step option has been selected
            if(byStepToggle.isOn)
            {
                //set the step counter UI to active
                StepCounter.SetActive(true);
            }
            else
            {
                //deactivate the step counter UI
                StepCounter.SetActive(false);
            }
        }
        //disactivate the map options UI
        SliderUI.gameObject.SetActive(false);
        //create the game object responsible for creating the grid in the world
        currentGrid = Instantiate(Grid, transform.position, Quaternion.identity);
        //retreive the grids Generating island component 
        GeneratingIsland gridCreation = currentGrid.GetComponent<GeneratingIsland>();
        //set the grids generation values
        gridCreation.SetValues((int)cellularPasses.value, (int)landChance.value, (int)MountainAgentsSlider.value, (int)ActionsSlider.value, byStepToggle.isOn,StepCounterText, StepTaskText);
        //create the map using the set values
        gridCreation.MapCreation();
    }



    void Start()
    {
        //set the relevant UI to active, deactivate those unnecessary
        SliderUI.gameObject.SetActive(true);
        ControlsUI.gameObject.SetActive(false);
        StepCounter.SetActive(false);
        //update the text values to the sliders values
        cellularPassesValue.text = cellularPasses.value.ToString();
        MountainAgentsValue.text = MountainAgentsSlider.value.ToString();
        landChanceValue.text = landChance.value.ToString();
        ActionsSliderValue.text = ActionsSlider.value.ToString();
    }



    bool ControlsOpen = true;

    void Update()
    {
        //if the escape key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //if a map has been generated and the slider UI has been deactivated
            if (currentGrid != null && SliderUI.gameObject.activeInHierarchy == false)
            {
                //set the slider UI options to active
                SliderUI.gameObject.SetActive(true);
                //deactivate the controls UI
                ControlsUI.gameObject.SetActive(false);
            }
            //if the grid has been generated but the slider UI is active
            else if (currentGrid != null && SliderUI.gameObject.activeInHierarchy)
            {
                //deactivate the slider UI
                SliderUI.gameObject.SetActive(false);
                //if the controls have not been closed
                if(ControlsOpen)
                {
                    //set the controls UI to active
                    ControlsUI.gameObject.SetActive(true);
                    //if the by step mode has been selected
                    if (byStepToggle.isOn)
                    {
                        //set the step counter UI to be true
                        StepCounter.SetActive(true);
                    }
                    //if the by step mode has not been selected
                    else
                    {
                        //deactivate the step counter UI
                        StepCounter.SetActive(false);
                    }
                }
                //if the controls UI has been closed
                else
                {
                    //deactivate the controls UI 
                    ControlsUI.gameObject.SetActive(false);
                }
            }
        }
        //if the C key has been pressed and the slider UI is deactivated
        if (Input.GetKeyDown(KeyCode.C) && SliderUI.gameObject.activeInHierarchy == false)
        {
            //create a new map using the current map settings
            CreateGrid();
        }

        //if the slider UI is active
        if (SliderUI.gameObject.activeInHierarchy == true)
        {
            //update the slider value text to the slider values
            cellularPassesValue.text = cellularPasses.value.ToString();
            MountainAgentsValue.text = MountainAgentsSlider.value.ToString();
            landChanceValue.text = landChance.value.ToString();
            ActionsSliderValue.text = ActionsSlider.value.ToString();
        }
        //if the comma key is pressed
        if (Input.GetKeyDown(KeyCode.Comma))
        {
            //if the map exists
            if (currentGrid != null)
            {
                //if the controls UI is active
                if(ControlsUI.gameObject.activeInHierarchy)
                {
                    //deactivate the controls UI
                    ControlsUI.gameObject.SetActive(false);
                    //set the value so that the controls UI wont be opened until comma is pressed again
                    ControlsOpen = false;
                }
                else
                {
                    //deactivate the controls UI
                    ControlsUI.gameObject.SetActive(true);
                    //set the value so that the controls UI will be opened 
                    ControlsOpen = true;
                }
                
            }
        }

            //if the current map has been created and the e key is pressed
            if (currentGrid && Input.GetKeyDown(KeyCode.E))
            {
            //either turn the tile UI on or off
                currentGrid.GetComponent<GeneratingIsland>().TurnOffUI();
            }
  

        //if the delete key is pressed
        if(Input.GetKeyDown(KeyCode.Delete))
        {
            //quit the exe
              Application.Quit();
        }
    }






}
