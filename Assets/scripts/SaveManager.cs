using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour {
    public static SaveManager Instance { set; get; }
    public SaveScript state;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
        Load(); 
        //are we using the accelerometer AND can we use it
        if(state.usingAccelerometer && !SystemInfo.supportsAccelerometer)
        {
            //if we cant make sure we're not trying next time
            state.usingAccelerometer = false;
            Save();

        }
        
        
               
    }
    public void Save()
    {
       PlayerPrefs.SetString("save",Helper.Serialize<SaveScript>(state));

    }

    public void Load()
    {
        if (PlayerPrefs.HasKey("save"))
        {
            state = Helper.Deserialize<SaveScript>(PlayerPrefs.GetString("save"));

        }
        else
        {
            state = new SaveScript();
            Save();
            Debug.Log("No save file.Creating a new one");
        }

    }

    //check if the color is owned
    public bool IsColorOwned(int index)
    {
        //check i the bit is set, if so the coloer is owned
        return (state.colorOwned & (1 << index)) != 0;
    }

    //check if the trail is owned
    public bool IsTrailOwned(int index)
    {
        //check i the bit is set, if so the trail is owned
        return (state.trailOwned & (1 << index)) != 0;
    }
    public bool BuyColor(int index,int cost)
    {
        if (state.gold >= cost)
        {
            state.gold -= cost;
            UnlockColor(index);

            Save();
            return true;

        }
        else
        {
            return false;

        }
    }

    public bool BuyTrail(int index, int cost)
    {
        if (state.gold >= cost)
        {
            state.gold -= cost;
            UnlockTrail(index);

            Save();
            return true;

        }
        else
        {
            return false;

        }
    }

    //unlocking the color in the "color owned int"
    public void UnlockColor(int index)
    {
        state.colorOwned |= 1 << index;
        
    }

    public void UnlockTrail(int index)
    {
        state.trailOwned |= 1 << index;

    }

    //comp level
    public void CompleteLevel(int index)
    {
        if (state.completedLevel == index)
        {
            state.completedLevel++;
            Save();
        }
    }
    //reste all saved files

    public void ResetSave()
    {
        PlayerPrefs.DeleteKey("save");
    }
}
