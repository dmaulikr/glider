using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMotor : MonoBehaviour
{
    private CharacterController controller;

    private float baseSpeed = 10.0f;
    private float rotSpeedX = 3.0f;
    private float rotSpeedY = 1.5f;
    private float deathTime;
    private float deathDuration = 2;

    public GameObject deathExplosion;


    private void Start()
    {
        controller = GetComponent<CharacterController>();

        //Create the trail
        GameObject trail = Instantiate(Manager.Instance.playerTrails[SaveManager.Instance.state.activeTrail]);
        //set the as a child of the model
        trail.transform.SetParent(transform.GetChild(0));

        //fix the rotation of the trail
        trail.transform.localEulerAngles = Vector3.forward * -90f;


    }

    private void Update()
    {
        //if the player is dead
        if (deathTime != 0)
        {
            if (Time.time - deathTime > deathDuration)
            {
                SceneManager.LoadScene("game");
            }

            return;

        }

        //give the player forward velocity
        Vector3 moveVector = transform.forward * baseSpeed;

        //gather player input
        Vector3 inputs = Manager.Instance.GetPlayerInput();

        //get the delat direction

        Vector3 yaw = inputs.x * transform.right * rotSpeedX * Time.deltaTime;
        Vector3 pitch = inputs.y * transform.up * rotSpeedY * Time.deltaTime;
        Vector3 dir = yaw + pitch;
        //make sure to limit the player for doing a loop

        float maxX = Quaternion.LookRotation(moveVector + dir).eulerAngles.x;

        //if he is  not to going too far up/down, add dir to the move vector

        if(maxX < 90 && maxX > 70 || maxX > 270 && maxX < 290)
        {
            // too fa, dont do anything
             
        }
        else
        {
            // add the direction to the current move 
            moveVector += dir;

            //have the player face where he is going 
            transform.rotation = Quaternion.LookRotation(moveVector);
        }

        //move him
        controller.Move(moveVector * Time.deltaTime);
     }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //set deathtime
        deathTime = Time.time;
        GameObject go = Instantiate(deathExplosion) as GameObject;
        go.transform.position = transform.position;
        transform.GetChild(0).gameObject.SetActive(false);
    }
         
}
