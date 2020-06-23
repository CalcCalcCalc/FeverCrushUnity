using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public bool ResetPrefs;
    public string[] names = {
        "Alfred",
        "Bertie",
        "Carlton",
        "Denny",
        "Egbert",
        "Fancesca",
        "Gertrude",
        "Hubert",
        "Isabella",
        "Jim"
    };

    public void Start() 
    {
        if (ResetPrefs)
        {
            PlayerPrefs.DeleteAll();

            for (var i = 0; i < 10; i++)
            {
                var place = string.Concat("place_",i);
                if (!PlayerPrefs.HasKey(place))
                {
                    PlayerPrefs.SetString(place, string.Concat(names[i], "_", (-(i-9) + 1) * 1000));
                }
            }

            for (var i = 0; i < 10; i++)
            {
                var place = string.Concat("place_",i);
                Debug.Log(PlayerPrefs.GetString(place));
            }
        }

    }

    public void LoadScene(string scenename)
    {
        Debug.Log("sceneName to load: " + scenename);
        SceneManager.LoadScene(scenename);
    }


}
