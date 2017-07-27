using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PreLoader : MonoBehaviour {

    private CanvasGroup fadeGroup;
    private float loadTime;
    private float minLogoTime = 3.0f;  // min time of that scene
    private void Start()
    {
        //grab the only canvasGroup in the scene
        fadeGroup = FindObjectOfType<CanvasGroup>();

        //start loading with a white screen
        fadeGroup.alpha = 1;

        //preload the game
        if (Time.time < minLogoTime)
            loadTime = minLogoTime;
        else
            loadTime = Time.time;

    }

    private void Update()
    {
        if(Time.time<minLogoTime)
        {
            fadeGroup.alpha = 1 - Time.time;

        }

        if (Time.time > minLogoTime && loadTime != 0)
        {
            fadeGroup.alpha = Time.time - minLogoTime;
            if(fadeGroup.alpha>=1)
            {
                SceneManager.LoadScene("menu");
                
            }
        }


    }


}
