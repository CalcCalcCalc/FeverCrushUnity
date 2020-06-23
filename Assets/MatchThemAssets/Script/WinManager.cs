using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinManager : MonoBehaviour
{
    // public GameObject main;
    public GameObject Score;
    public GameObject PlayAgainButton;
    public GameObject BackButton;
    public GameObject NameInputText;
    private string currentScore;

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

    // Start is called before the first frame update
    void Start()
    {
        currentScore = Main.getScore().ToString();
        var scoretext = Score.GetComponent(typeof(Text)) as Text;
        scoretext.text = currentScore;
    }

    public void SubmitHighscore() 
    {
        var inputText = NameInputText.GetComponent(typeof(Text)) as Text;
        var currentName = inputText.text;
        if (currentName == "") {
            currentName = names[(int)System.Math.Floor(UnityEngine.Random.value * 10)];
        };
        var tempScore = "";
        for (var i = 0; i < 10; i++)
        {
            var place = string.Concat("place_",i);
            if (PlayerPrefs.HasKey(place))
            {
                var pref = PlayerPrefs.GetString(place);
                if (tempScore == ""){
                    var splitPref = pref.Split('_');
                    var prefScore = splitPref[1];                
                    if (Int32.Parse(prefScore) <= Int32.Parse(currentScore))
                    {
                        PlayerPrefs.SetString(place, String.Concat(currentName,"_",currentScore));
                        tempScore = pref;
                    }
                }
                else 
                {
                    PlayerPrefs.SetString(place, tempScore);
                    tempScore = pref;
                }
            }
        }
    }
}
