using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : MonoBehaviour
{
    public bool bomb = false;
    public bool pill = false;
    public SpriteRenderer sr;

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (sr.color != Color.blue && this.bomb) {
            sr.color = Color.blue;
        }
        if (sr.color != Color.red && this.pill) {
            sr.color = Color.red;
        }
    }
}
