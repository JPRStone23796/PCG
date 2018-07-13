using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    [SerializeField] int cameraSpeed, scrollSpeed;
    
    void Update()
    {
        //retrieve the horizontal input
        float moveHorizontal = Input.GetAxis("Horizontal");
        //if the camera is not within the following bounds
        if((transform.position.z <=-15 && moveHorizontal > 0) || (transform.position.z >= 15 && moveHorizontal < 0))
        {
            //set the input to 0
            moveHorizontal = 0;
        }
        //retrieve the vertical input
        float moveVertical = Input.GetAxis("Vertical");
        //if the camera is not within the following bounds
        if ((transform.position.x <= -20 && moveVertical < 0) || (transform.position.x >= 10 && moveVertical > 0))
        {
            //set the input to 0
            moveVertical = 0;
        }
        //retreive the scroll input
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        //if the camera is not within the following bounds
        if ((transform.position.y<=1 && scrollInput > 0) || (transform.position.y>=21 && scrollInput < 0))
        {
            //set the input to 0
            scrollInput = 0;
        }
        //if left shift is pressed
        if (Input.GetKey(KeyCode.LeftShift))
        {
            //increase the input values
            moveVertical *= 3f;
            moveHorizontal *= 3f;
        }
        //create a input vector
        Vector3 movement = new Vector3(moveVertical, -(scrollInput*scrollSpeed), -moveHorizontal);
       //apply the input vector to the cameras positin
        transform.position += (movement * cameraSpeed) * Time.deltaTime;
    }
	

}
