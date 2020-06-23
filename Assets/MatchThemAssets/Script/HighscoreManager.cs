using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighscoreManager : MonoBehaviour
{
    public GameObject[] Name;
    public GameObject[] Score;
    private Text _nameTextComponent;
    private Text _scoreTextComponent;

    // Start is called before the first frame update
    void Start()
    {
        for (var i = 0; i < Name.Length; i++)
        {
            var place = string.Concat("place_",i);
            if (PlayerPrefs.HasKey(place))
            {
                var pref = PlayerPrefs.GetString(place);
                var splitPref = pref.Split('_');
                var name = splitPref[0];
                var score = splitPref[1];
                _nameTextComponent = Name[i].GetComponent(typeof(Text)) as Text;
                _nameTextComponent.text = name;
                _scoreTextComponent = Score[i].GetComponent(typeof(Text)) as Text;
                _scoreTextComponent.text = score;
            }
        }
    }
}
