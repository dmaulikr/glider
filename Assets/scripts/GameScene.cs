using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameScene : MonoBehaviour
{
    private CanvasGroup fadeGroup;
    private float fadeInDuration = 2;
    private bool gameStarted;

    public Transform arrow;
    private Transform playerTransfrom;
    public Objective objective;

    private void Start()
    {
        playerTransfrom = FindObjectOfType<PlayerMotor>().transform;
        
        //load up the level
        SceneManager.LoadScene(Manager.Instance.currentLevel.ToString(), LoadSceneMode.Additive);

        //get the only canvas grp in the scene 
        fadeGroup = FindObjectOfType<CanvasGroup>();

        fadeGroup.alpha = 1;

    }
    private void Update()
    {
        if (objective != null)
        {
            //if we have an objective
            //rotate the arrow
            Vector3 dir = playerTransfrom.InverseTransformPoint(objective.GetCurrentRing().position);
            float a = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            a += 180;
            arrow.transform.localEulerAngles = new Vector3(0, 180, a);
        }

        if (Time.timeSinceLevelLoad <=fadeInDuration)
        {
            fadeGroup.alpha = 1 - (Time.timeSinceLevelLoad / fadeInDuration);
        }

        //if th eintitial fade in is completed, andthe game is not started yet

        else if(!gameStarted)
        {
            fadeGroup.alpha = 0;
            gameStarted = true;
        }
    }

	public void CompleteLevel()
    {
        //complete level,save progress
        SaveManager.Instance.CompleteLevel(Manager.Instance.currentLevel);

        //focus the level selection when we return to the menu scene
        Manager.Instance.menuFocus = 1;

        ExitScene();
    }

    public void ExitScene()
    {
        SceneManager.LoadScene("menu");
    }
}
