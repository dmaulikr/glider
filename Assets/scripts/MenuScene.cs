using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScene : MonoBehaviour {

    private CanvasGroup fadeGroup;
    private float fadeInSpeed = 0.33f;
    public RectTransform menuContainer;
    public Transform levelPanel;
    
    public Transform colorPanel;
    public Transform trailPanel;

    public Button tiltControlButton;
    public Color tiltControlEnabled;
    public Color tiltControlDisabled;
    public Text colorBuySetText;
    public Text trailBuySetText;

    public Text goldText;

    private MenuCamera menuCam;

    private int[] colorCost = new int[] { 0, 5, 5, 10, 10, 10, 15, 15, 10,50 };
    private int[] trailCost = new int[] { 0, 20, 40, 40, 60, 60, 80, 80, 100,150 };
    private int selectedTrailIndex;
    private int selectedcolorIndex;

    private int activeColorIndex;
    private int activeTrailIndex;
    private Vector3 desiredMenuPosition;
    private GameObject currentTrail;

    public AnimationCurve enteringLevelZoomCurve;
    private bool isEnteringLevel = false;
    private float zoomDuration = 3.0f;
    private float zoomTransition;

    private Texture previousTrail;
    private GameObject lastPriviewObject;
    public Transform trailPreviewObject;
    public RenderTexture trailPreviewTexture;


    private void Start()
    {
       

        //check if we have an accelerator, 

        if(SystemInfo.supportsAccelerometer)
        {
            //is currentsly enabled
            tiltControlButton.GetComponent<Image>().color = (SaveManager.Instance.state.usingAccelerometer) ? tiltControlEnabled : tiltControlDisabled;
        }

        else
        {
            tiltControlButton.gameObject.SetActive(false);
        }

        menuCam = FindObjectOfType<MenuCamera>();

        

        //position camera to focused menu
        SetCameraTo(Manager.Instance.menuFocus);

        //tell how much gold text to be displayed
        UpdateGoldText();

        fadeGroup = FindObjectOfType<CanvasGroup>();

        //start loading with a white screen
        fadeGroup.alpha = 1;

        InitShop();

        InitLevel();

        //set player preferences for ocolor and trial

        OnColorSelect(SaveManager.Instance.state.activeColor);
        SetColor(SaveManager.Instance.state.activeColor);

        OnTrailSelect(SaveManager.Instance.state.activeTrail);
        SetTrail(SaveManager.Instance.state.activeTrail);

        //make the buttons bigger for selected items

        colorPanel.GetChild(SaveManager.Instance.state.activeColor).GetComponent<RectTransform>().localScale = Vector3.one * 1.125f;
        trailPanel.GetChild(SaveManager.Instance.state.activeTrail).GetComponent<RectTransform>().localScale = Vector3.one * 1.125f;

        //create the trail preview
        lastPriviewObject = GameObject.Instantiate(Manager.Instance.playerTrails[SaveManager.Instance.state.activeTrail]) as GameObject;
        lastPriviewObject.transform.SetParent(trailPreviewObject);
        lastPriviewObject.transform.localPosition =  Vector3.zero;

    }

    private void Update()
    {
        fadeGroup.alpha = 1 - Time.timeSinceLevelLoad * fadeInSpeed;
        // menu navigation(smooth)

        menuContainer.anchoredPosition3D = Vector3.Lerp(menuContainer.anchoredPosition3D, desiredMenuPosition, 0.1f);
    
        if(isEnteringLevel)
        {
            zoomTransition += (1 / zoomDuration) * Time.deltaTime;
            menuContainer.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 5, enteringLevelZoomCurve.Evaluate(zoomTransition));

            Vector3 newDesiredPosition = desiredMenuPosition * 5;
            RectTransform rt = levelPanel.GetChild(Manager.Instance.currentLevel).GetComponent<RectTransform>();
            newDesiredPosition -= rt.anchoredPosition3D * 5;

            //this line will oveerrde teh previous update

            menuContainer.anchoredPosition3D = Vector3.Lerp(desiredMenuPosition, newDesiredPosition, enteringLevelZoomCurve.Evaluate(zoomTransition));

            //fade to white screen , this will override the frst line of updtae

            fadeGroup.alpha = zoomTransition;

            if(zoomTransition>=1)
            {
                //enter the level
                SceneManager.LoadScene("game");
            }
        }
    }

    private void InitShop()
    {
        if (colorPanel == null || trailPanel == null)
            Debug.Log("A");

        int i = 0;
        foreach(Transform t in colorPanel)
        {
            int curIndex = i;
            Button b = t.GetComponent<Button>();
            b.onClick.AddListener(() => OnColorSelect(curIndex));

            //set te color of image based on own/not

            Image img = t.GetComponent<Image>();
            img.color = SaveManager.Instance.IsColorOwned(i) ? Manager.Instance.playerColors[curIndex]
                : Color.Lerp(Manager.Instance.playerColors[curIndex], new Color (0,0,0,1), 0.25f) ;
            i++;
        }

        i = 0;
        foreach (Transform t in trailPanel)
        {
            int curIndex = i;
            Button b = t.GetComponent<Button>();
            b.onClick.AddListener(() => OnTrailSelect(curIndex));

            RawImage img = t.GetComponent<RawImage>();
            img.color = SaveManager.Instance.IsTrailOwned(i) ? Color.white : new Color(0.7f, 0.7f, 0.7f);
            i++;
        }

        previousTrail = trailPanel.GetChild(SaveManager.Instance.state.activeTrail).GetComponent<RawImage>().texture;
    }

    private void InitLevel()
    {
        if (levelPanel == null)
            Debug.Log("please assign level panel");

        int i = 0;
        foreach (Transform t in levelPanel)
        {
            int curIndex = i;
            Button b = t.GetComponent<Button>();
            b.onClick.AddListener(() => OnLevelSelect(curIndex));
            Image img = t.GetComponent<Image>();
            //is it unlocked
            if (i <= SaveManager.Instance.state.completedLevel)
            {
                if (i == SaveManager.Instance.state.completedLevel)
                {
                    img.color = Color.white;
                
                }
                else
                {
                    //level comp.
                    img.color = Color.green;
                }
            }
            else
            {
                //level isnt unloacked,disable button
                b.interactable = false;
                img.color = Color.grey;

            }


            i++;
        }


    }

    private void SetCameraTo(int menuIndex)
    {
        NavigateTo(menuIndex);
        menuContainer.anchoredPosition3D = desiredMenuPosition;

     }

    private void NavigateTo(int menuIndex)
    {
        switch (menuIndex)
        {
        //0 & default main menu
            default:
            case 0:
                desiredMenuPosition = Vector3.zero;
                menuCam.BackToMainMenu();
                break;
                // 1 - play menu
            case 1:
                desiredMenuPosition = Vector3.right * 1280;
                menuCam.MoveToLevel();
                break;

                // 2 - shop menu
            case 2:
                desiredMenuPosition = Vector3.left * 1280;
                menuCam.MoveToShop();
                break;

        }

    }

     private void SetColor(int index)
    {

        //set the active index of color

        activeColorIndex = index;

        SaveManager.Instance.state.activeColor = index;

        Manager.Instance.playerMaterial.color = Manager.Instance.playerColors[index];

       
        colorBuySetText.text = "Current";

        SaveManager.Instance.Save();
    }

    private void SetTrail(int index)
    {
        activeTrailIndex = index;
        SaveManager.Instance.state.activeTrail = index;
        if (currentTrail != null)
        {
            Destroy(currentTrail);
        }

        currentTrail = Instantiate(Manager.Instance.playerTrails[index]) as GameObject;
        currentTrail.transform.SetParent(FindObjectOfType<MenuPlayer>().transform);

        currentTrail.transform.localPosition = Vector3.zero;
        currentTrail.transform.localRotation = Quaternion.Euler(0, 0, 90);
        currentTrail.transform.localScale = Vector3.one * 0.01f;


        trailBuySetText.text = "Current";

        SaveManager.Instance.Save();

    }

    private void UpdateGoldText()
    {
        goldText.text = SaveManager.Instance.state.gold.ToString();
    }

    private void OnLevelSelect(int curIndex)
    {
        Manager.Instance.currentLevel = curIndex;
        isEnteringLevel = true;

        Debug.Log("Selecting level " + curIndex);
    }

    public void OnPlayClick()
    {
        NavigateTo(1);

    }

    public void OnShopClick()
    {
        NavigateTo(2);
        Debug.Log("shop button click");
    }

    public void OnBackClick()
    {
        NavigateTo(0);
        Debug.Log("back button click");
    }

    private void OnTrailSelect(int curIndex)
    {
        Debug.Log("Selecting trail button: " + curIndex);

        if (selectedTrailIndex == curIndex)
            return;

        //preview trail
        //get the image of the preview button

        trailPanel.GetChild(selectedTrailIndex).GetComponent<RawImage>().texture = previousTrail;

        //keep the new trail previw image in the previous as backup
        previousTrail = trailPanel.GetChild(curIndex).GetComponent<RawImage>().texture;

        trailPanel.GetChild(curIndex).GetComponent<RawImage>().texture = trailPreviewTexture;

        //change the pysical object of the trail preview
        if(lastPriviewObject!=null)
        {
            Destroy(lastPriviewObject);
        }

        lastPriviewObject = GameObject.Instantiate(Manager.Instance.playerTrails[curIndex]) as GameObject;
        lastPriviewObject.transform.SetParent(trailPreviewObject);
        lastPriviewObject.transform.localPosition = Vector3.zero;


        //make the icons bigger

        trailPanel.GetChild(curIndex).GetComponent<RectTransform>().localScale = Vector3.one * 1.125f;

        //put the previous one on normal scale

        trailPanel.GetChild(selectedTrailIndex).GetComponent<RectTransform>().localScale = Vector3.one;

        selectedTrailIndex = curIndex;

        if (SaveManager.Instance.IsTrailOwned(curIndex))
        {
            //trail is owned;
            if (activeTrailIndex == curIndex)
            {
                trailBuySetText.text = "Current";
            }
            else
            {
                trailBuySetText.text = "Select";
            }

        }
        else
        {
            //trail is noy owned
            trailBuySetText.text = "Buy: " + trailCost[curIndex].ToString();
        }
    }

    private void OnColorSelect(int curIndex)
    {
        Debug.Log("Selecting color button: " + curIndex);

        if(selectedcolorIndex ==  curIndex)
        {
            return;
        }

        //make the icons bigger

        colorPanel.GetChild(curIndex).GetComponent<RectTransform>().localScale = Vector3.one * 1.125f;

        //put the previous one on normal scale

        colorPanel.GetChild(selectedcolorIndex).GetComponent<RectTransform>().localScale = Vector3.one;


        selectedcolorIndex = curIndex;

        if (SaveManager.Instance.IsColorOwned(curIndex))
        {
            //is it alreadya cuurrnt color?
    
            if(activeColorIndex == curIndex)
            {
                colorBuySetText.text = "Current";
            }
            else
            {
                colorBuySetText.text = "Select";
            }


        }
        else
        {
            colorBuySetText.text = "Buy: " + colorCost[curIndex].ToString();
        }
    }

    public void OnColorBuySet()
    {
        Debug.Log("buy/set color");
        if (SaveManager.Instance.IsColorOwned(selectedcolorIndex))
        {
            SetColor(selectedcolorIndex);

            
        }
        else
        {
            if (SaveManager.Instance.BuyColor(selectedcolorIndex, colorCost[selectedcolorIndex]))
            {
                //success
                SetColor(selectedcolorIndex);

                //change the color of the button
                colorPanel.GetChild(selectedcolorIndex).GetComponent<Image>().color  = Manager.Instance.playerColors[selectedcolorIndex]; 

                //update the gold text

                UpdateGoldText();
               
            }
            else
            {
                Debug.Log("NOT enough code");
            }
        }

    }

    public void OnTrailBuySet()
    {
        Debug.Log("buy/set trail");
        if (SaveManager.Instance.IsTrailOwned(selectedTrailIndex))
        {
            SetTrail(selectedTrailIndex);
        }
        else
        {
            if (SaveManager.Instance.BuyTrail(selectedTrailIndex, trailCost[selectedTrailIndex]))
            {
                //success
                SetTrail(selectedTrailIndex);

                //change the color of the button
                trailPanel.GetChild(selectedTrailIndex).GetComponent<RawImage>().color = Color.white;

                //update the gold text

                UpdateGoldText();
            }
            else
            {
                Debug.Log("NOT enough code");
            }
        }
    }

    public void OnTiltControl()
    {
        //toggle the acc. bool
        SaveManager.Instance.state.usingAccelerometer = !SaveManager.Instance.state.usingAccelerometer;

        //make sure to sav ethe players preferences

        SaveManager.Instance.Save();

        //change the dsiplay image of the tilt control

        tiltControlButton.GetComponent<Image>().color = (SaveManager.Instance.state.usingAccelerometer) ? tiltControlEnabled : tiltControlDisabled;


    }

}
