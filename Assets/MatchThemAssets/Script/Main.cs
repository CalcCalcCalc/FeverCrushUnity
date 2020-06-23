using System;
using UnityEngine;
using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Core;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
///  This class is the main entry point of the game it should be attached to a gameobject and be instanciate in the scene
/// Author : Pondomaniac Games
/// </summary>
public class Main : MonoBehaviour
{

    public GameObject _indicator;//The indicator to know the selected tile
    public GameObject[,] _arrayOfShapes;//The main array that contain all games tiles
    private GameObject _currentIndicator;//The current indicator to replace and destroy each time the player change the selection
    private GameObject _FirstObject;//The first object selected
    private GameObject _SecondObject;//The second object selected
    public GameObject[] _listOfGems;//The list of tiles we cant to see in the game you can remplace them in unity's inspector and choose all what you want
    public GameObject _emptyGameobject;//After destroying object they are replaced with this one so we will replace them after with new ones
    public GameObject _particleEffect;//The object we want to use in the effect of shining stars 
    public GameObject _particleEffectWhenMatch;//The gameobject of the effect when the objects are matching
    public bool _canTransitDiagonally = false;//Indicate if we can switch diagonally
    public GameObject Score;//Score
    public int _scoreIncrement;//The amount of point to increment each time we find matching tiles
    public static int _scoreTotal = 0;//The score 
    public GameObject Time;// Time
    private int _totalSeconds = 60;
    public bool infiniteTime;
    private ArrayList _currentParticleEffets = new ArrayList();//the array that will contain all the matching particle that we will destroy after
    public AudioClip MatchSound;//the sound effect when matched tiles are found
    public int _gridWidth;//the grid number of cell horizontally
    public int _gridHeight;//the grid number of cell vertically
    //inside class
    private Vector2 _firstPressPos;
    private Vector2 _secondPressPos;
    private Vector2 _currentSwipe;
    private TextMesh _scoreMesh;
    private TextMesh _timeMesh;
    public bool active;
    private TweenInfo[] previousTweenInfos;

    // Use this for initialization
    void Start()
    {
        if (active){
            _scoreTotal = 0;
            //Initializing the array with _gridWidth and _gridHeight passed in parameter
            _arrayOfShapes = new GameObject[_gridWidth, _gridHeight];
            //Creating the gems from the list of gems passed in parameter
            for (int i = 0; i <= _gridWidth - 1; i++)
            {
                for (int j = 0; j <= _gridHeight - 1; j++)
                {
                    var gameObject = GameObject.Instantiate(_listOfGems[UnityEngine.Random.Range(0, _listOfGems.Length)] as GameObject, new Vector3(i, j, 0), transform.rotation) as GameObject;
                    _arrayOfShapes[i, j] = gameObject;
                }
            }            
            //Adding the star effect to the gems and call the DoShapeEffect continuously
            InvokeRepeating("DoShapeEffect", 1f, 0.21F);
            //start timer
            if (!infiniteTime)
            {
                InvokeRepeating("Countdown", 0f, 1F);
            } 
            else 
            {
                var timeMesh = Time.GetComponent(typeof(TextMesh)) as TextMesh;
                timeMesh.text = "       ∞";
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
      if (active){
        bool shouldTransit = false;
        var direction = Swipe();
        if (direction != Direction.NONE)
        {
            //Detecting if the player clicked on the left mouse button and also if there is no animation playing
            if ( HOTween.GetTweenInfos() == null)
            {
                Destroy(_currentIndicator);
                //The 3 following lines is to get the clicked GameObject and getting the RaycastHit2D that will help us know the clicked object
                //Ray ray   = Camera.main.ScreenPointToRay (Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(_firstPressPos), Vector2.zero);
                if (hit.transform != null)
                {  
                    //To know if the user already selected a tile or not yet
                    if (_FirstObject == null)
                    {
                        _FirstObject = hit.transform.gameObject;
                        if (direction != Direction.STATIONARY)
                        {
                            Vector3 hit2Position = hit.transform.position;
                            switch (direction)
                            {
                               case Direction.UP:
                                    hit2Position.y++;break;
                                case Direction.DOWN:
                                    hit2Position.y--;  break;
                                case Direction.LEFT:
                                    hit2Position.x--; break;
                                case Direction.RIGHT:
                                    hit2Position.x++; break;

                            }

                            RaycastHit2D hit2 =  Physics2D.Raycast(hit2Position, Vector2.zero);
                            if (hit2.transform != null )
                            {
                                _SecondObject = hit2.transform.gameObject;
                                shouldTransit = true;
                            }
                        }
                    }
                    else
                    {
                        _SecondObject = hit.transform.gameObject;
                        shouldTransit = true;
                    }
                    _currentIndicator = GameObject.Instantiate(_indicator, new Vector3(hit.transform.gameObject.transform.position.x, hit.transform.gameObject.transform.position.y, -1), transform.rotation) as GameObject;
                    //If the user select the second tile we will swap the two tile and animate them
                    if (shouldTransit)
                    {
                        //Getting the position between the 2 tiles
                        var distance =_FirstObject.transform.position - _SecondObject.transform.position;
                        //Testing if the 2 tiles are next to each others otherwise we will not swap them 
                        if (Mathf.Abs(distance.x) <= 1 && Mathf.Abs(distance.y) <= 1)
                        {   //If we dont want the player to swap diagonally
                            if (!_canTransitDiagonally)
                            {
                                if (distance.x != 0 && distance.y != 0)
                                {
                                    Destroy(_currentIndicator);
                                    _FirstObject = null;
                                    _SecondObject = null;
                                    return;
                                }
                            }
                            //Animate the transition
                            DoSwapMotion(_FirstObject.transform, _SecondObject.transform);
                            //Swap the object in array
                            DoSwapTile(_FirstObject, _SecondObject, ref _arrayOfShapes);
                        }
                        else
                        {
                            _FirstObject = null;
                            _SecondObject = null;
                        }
                        Destroy(_currentIndicator);
                    }
                }
            }
        }
        //If no animation is playing
        if (HOTween.GetTweenInfos() == null)
        {
            //instead of an array, let's do a queue.
            var MatchGroups = FindMatch(_arrayOfShapes);
            //If we find a matched tiles
            if (MatchGroups.Count > 0)
            {
                Debug.Log(previousTweenInfos);
                foreach(Stack Matches in MatchGroups)
                {
                    var isBomb = Matches.Pop();
                    if (!(isBomb is bool))
                    {
                        throw new InvalidOperationException("Last object in match stack must be a bool");
                    }
                    //Update the score
                    _scoreTotal += Matches.Count * _scoreIncrement;
                    // int e = Matches.get(Matches.size() - 1);
                    // Debug.Log(e);
                    //TODO Explode this bomb

                    foreach (GameObject go in Matches)
                    {
                        //Playing the matching sound
                        GetComponent<AudioSource>().PlayOneShot(MatchSound);
                        //Creating and destroying the effect of matching
                        var destroyingParticle = GameObject.Instantiate(_particleEffectWhenMatch as GameObject, new Vector3(go.transform.position.x, go.transform.position.y, -2), transform.rotation) as GameObject;
                        Destroy(destroyingParticle, 1f);
                        Debug.Log(go.name);
                        //Replace the matching tile with an empty one
                        _arrayOfShapes[(int)go.transform.position.x, (int)go.transform.position.y] = GameObject.Instantiate(_emptyGameobject, new Vector3((int)go.transform.position.x, (int)go.transform.position.y, -1), transform.rotation) as GameObject;
                        //Destroy the ancient matching tiles
                        Destroy(go, 0.1f);
                    }   
                }
                _FirstObject = null;
                _SecondObject = null;
                //Moving the tiles down to replace the empty ones
                DoEmptyDown(ref _arrayOfShapes);
            }
            //If no matching tiles are found remake the tiles at their places
            else if (_FirstObject != null && _SecondObject != null)
            {
                //Animate the tiles
                DoSwapMotion(_FirstObject.transform, _SecondObject.transform);
                //Swap the tiles in the array
                DoSwapTile(_FirstObject, _SecondObject, ref _arrayOfShapes);
                _FirstObject = null;
                _SecondObject = null;

            }
        }      
        else
        {
            previousTweenInfos = HOTween.GetTweenInfos();
        }
        //Update the score
        _scoreMesh = (Score.GetComponent(typeof(TextMesh)) as TextMesh);
        _scoreMesh.text = _scoreTotal.ToString();
      }

    }

    // Find Match-3 Tile
    private Stack FindMatch(GameObject[,] cells)
    {//creating an arraylist to store the matching tiles
        Stack stack = new Stack();
        var isBomb = false;
        //Checking the vertical tiles
        for (var x = 0; x <= cells.GetUpperBound(0); x++)
        {
            for (var y = 0; y <= cells.GetUpperBound(1); y++)
            {
                var thiscell = cells[x, y];
                if (thiscell.name == "Empty(Clone)") continue;
                var gem = thiscell.GetComponent<Gem>();
                if (gem.pill) continue;
                //If it's an empty tile continue
                int matchCount = 0;
                int yn = cells.GetUpperBound(1);
                int yplus1;
                //Getting the number of tiles of the same kind
                for (yplus1 = y + 1; yplus1 <= yn; yplus1++)
                {
                    if (cells[x, yplus1].name == "Empty(Clone)") break;
                    var gemxyplus1 = cells[x, yplus1].GetComponent<Gem>();
                    if (thiscell.name != cells[x, yplus1].name || gemxyplus1.pill) break;
                    matchCount++;
                }
                //If we found more than 2 tiles close we add them in the array of matching tiles
                if (matchCount >= 2)
                {
                    Stack matchGroup = new Stack();
                    yplus1 = Mathf.Min(cells.GetUpperBound(1), yplus1 - 1);
                    for (var y3 = y; y3 <= yplus1; y3++)
                    {
                        var gemxy3 = cells[x,y3].GetComponent<Gem>();
                        if (!stack.Contains(cells[x, y3]))
                        {
                            if (gemxy3.bomb || gemxy3.pill) continue;
                            matchGroup.Push(cells[x, y3]);
                        }
                    }
                    if (gem.bomb)
                    {
                        isBomb = true;                            
                    }
                    switch (matchCount) 
                    {
                        case 3:
                            gem.bomb = true;
                            break;
                        case 4:
                            gem.pill = true;
                            break;
                        default:
                            break;
                    }
                    if (isBomb) 
                    {
                        matchGroup.Push(true);
                    } else {
                        matchGroup.Push(false);
                    }
                    stack.Push(matchGroup);
                }
            }
        }
        //Checking the horizontal tiles , in the following loops we will use the same concept as the previous ones
        for (var y = 0; y < cells.GetUpperBound(1) + 1; y++)
        {
            for (var x = 0; x < cells.GetUpperBound(0) + 1; x++)
            {
                var thiscell = cells[x, y];
                if (thiscell.name == "Empty(Clone)") continue;
                var gem = thiscell.GetComponent<Gem>();
                if (gem.pill) continue;
                int matchCount = 0;
                int x2 = cells.GetUpperBound(0);
                int x1;
                for (x1 = x + 1; x1 <= x2; x1++)
                {
                    if (cells[x2, y].name == "Empty(Clone)") break;
                    var gemx1y = cells[x1, y].GetComponent<Gem>();
                    if (thiscell.name != cells[x1, y].name || gemx1y.pill) break;
                    matchCount++;
                }
                if (matchCount >= 2)
                {
                    Stack matchGroup = new Stack();
                    x1 = Mathf.Min(cells.GetUpperBound(0), x1 - 1);
                    for (var x3 = x; x3 <= x1; x3++)
                    {
                        var gemx3y = cells[x3,y].GetComponent<Gem>();
                        if (!stack.Contains(cells[x3, y]))
                        {
                            if (gemx3y.bomb || gemx3y.pill) continue;
                            matchGroup.Push(cells[x3, y]);
                        }
                        Transform cellx3yTran = cells[x3,y].GetComponent<Transform>();
                        var cellx3ypos = (cellx3yTran.position.x, cellx3yTran.position.y);
                        // if(cellx3ypos == cellx3ypos) Debug.Log("yes you can");
                        // Debug.Log(cellx3ypos);
                        // if()
                        // {
                            switch (matchCount) 
                            {
                                case 3:
                                    Debug.Log("4 in a row!");
                                    if (previousTweenInfos != null)
                                    {
                                        Debug.Log("Just Stopped animating");
                                        foreach(TweenInfo animInfo in previousTweenInfos)
                                        {
                                            foreach(Transform target in animInfo.targets)
                                            {
                                                var animxy = (target.position.x, target.position.y);
                                                // Debug.Log(animxy);
                                                if (cellx3ypos == animxy) 
                                                {
                                                    // Debug.Log(cellx3ypos);
                                                }
                                            }
                                        }
                                    }
                                    gemx3y.bomb = true;
                                    break;
                                case 4:
                                    gemx3y.pill = true;
                                    break;
                                default:
                                    break;
                            }
                        // }
                    }
                    if (gem.bomb)
                    {
                        isBomb = true;
                    }
                    if (isBomb) 
                    {
                        matchGroup.Push(true);
                    } 
                    else 
                    {
                        matchGroup.Push(false);
                    }
                    stack.Push(matchGroup);
                }
            }
        }
        return stack;
    }

    // Swap Motion Animation, to animate the switching arrays
    void DoSwapMotion(Transform a, Transform b)
    {
        Vector3 posA = a.localPosition;
        Vector3 posB = b.localPosition;
        TweenParms parms = new TweenParms().Prop("localPosition", posB).Ease(EaseType.EaseOutQuart);
        HOTween.To(a, 0.25f, parms).WaitForCompletion();
        parms = new TweenParms().Prop("localPosition", posA).Ease(EaseType.EaseOutQuart);
        HOTween.To(b, 0.25f, parms).WaitForCompletion();
    }


    // Swap Two Tile, it swaps the position of two objects in the grid array
    void DoSwapTile(GameObject a, GameObject b, ref GameObject[,] cells)
    {
        GameObject cell = cells[(int)a.transform.position.x, (int)a.transform.position.y];
        cells[(int)a.transform.position.x, (int)a.transform.position.y] = cells[(int)b.transform.position.x, (int)b.transform.position.y];
        cells[(int)b.transform.position.x, (int)b.transform.position.y] = cell;
    }

    // Do Empty Tile Move Down
    private void DoEmptyDown(ref GameObject[,] cells)
    {//replace the empty tiles with the ones above
        for (int x = 0; x <= cells.GetUpperBound(0); x++)
        {
            for (int y = 0; y <= cells.GetUpperBound(1); y++)
            {
                var thisCell = cells[x, y];
                if (thisCell.name == "Empty(Clone)")
                {
                    for (int y2 = y; y2 <= cells.GetUpperBound(1); y2++)
                    {
                        if (cells[x, y2].name != "Empty(Clone)")
                        {
                            cells[x, y] = cells[x, y2];
                            cells[x, y2] = thisCell;
                            break;
                        }
                    }
                }
            }
        }
        //Instantiate new tiles to replace the ones destroyed
        for (int x = 0; x <= cells.GetUpperBound(0); x++)
        {
            for (int y = 0; y <= cells.GetUpperBound(1); y++)
            {
                if (cells[x, y].name == "Empty(Clone)")
                {
                    Destroy(cells[x, y]);
                    cells[x, y] = GameObject.Instantiate(_listOfGems[UnityEngine.Random.Range(0, _listOfGems.Length)] as GameObject, new Vector3(x, cells.GetUpperBound(1) + 2, 0), transform.rotation) as GameObject;
                }
            }
        }

        for (int x = 0; x <= cells.GetUpperBound(0); x++)
        {
            for (int y = 0; y <= cells.GetUpperBound(1); y++)
            {
                TweenParms parms = new TweenParms().Prop("position", new Vector3(x, y, -1)).Ease(EaseType.EaseOutQuart);
                HOTween.To(cells[x, y].transform, .4f, parms);
            }
        }
    }
    //Instantiate the star objects
    void DoShapeEffect()
    {
        foreach (GameObject row in _currentParticleEffets)
            Destroy(row);
        for (int i = 0; i <= 2; i++)
            _currentParticleEffets.Add(GameObject.Instantiate(_particleEffect, new Vector3(UnityEngine.Random.Range(0, _arrayOfShapes.GetUpperBound(0) + 1), UnityEngine.Random.Range(0, _arrayOfShapes.GetUpperBound(1) + 1), -1), new Quaternion(0, 0, UnityEngine.Random.Range(0, 1000f), 100)) as GameObject);
    }

    void Countdown() 
    {
        var timeMesh = Time.GetComponent(typeof(TextMesh)) as TextMesh;
        _totalSeconds -= 1;
        if (_totalSeconds <= 0) 
        {
            var lowestHighscore = PlayerPrefs.GetString("place_9");
            // Debug.Log(lowestHighscore);
            var lowestScore = lowestHighscore.Split('_')[1];
            if (_scoreTotal > Int32.Parse(lowestScore))
            {
                SceneManager.LoadScene("HighScore");
            } 
            else 
            {
                SceneManager.LoadScene("GameOver");
            }
        }
        var extraZero = _totalSeconds % 60 < 10 ? "0" : "";
        timeMesh.text = string.Concat(_totalSeconds/60,":",extraZero,_totalSeconds%60);
    }

    enum Direction
    {       NONE,
            STATIONARY,
            UP,
            DOWN,
            LEFT,
            RIGHT,
    }

    private Direction Swipe()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //save began touch 2d point
            _firstPressPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }
        if (Input.GetMouseButtonUp(0))
        {
            //save ended touch 2d point
            _secondPressPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            //create vector from the two points
            _currentSwipe = new Vector2(_secondPressPos.x - _firstPressPos.x, _secondPressPos.y - _firstPressPos.y);

            //normalize the 2d vector
            _currentSwipe.Normalize();

            //swipe upwards
            if (_currentSwipe.y > 0 && _currentSwipe.x > -0.5f && _currentSwipe.x < 0.5f)
        {
                // Debug.Log("up swipe");
                return Direction.UP;
            }
            //swipe down
            if (_currentSwipe.y < 0 && _currentSwipe.x > -0.5f && _currentSwipe.x < 0.5f)
        {
                // Debug.Log("down swipe");
                return Direction.DOWN;
            }
            //swipe left
            if (_currentSwipe.x < 0 && _currentSwipe.y > -0.5f && _currentSwipe.y < 0.5f)
        {
                // Debug.Log("left swipe");
                return Direction.LEFT;
            }
            //swipe right
            if (_currentSwipe.x > 0 && _currentSwipe.y > -0.5f && _currentSwipe.y < 0.5f)
        {
                // Debug.Log("right swipe");
                return Direction.RIGHT;
            }
            return Direction.STATIONARY;
        }
        return Direction.NONE;
    }

    public static int getScore(){
        return _scoreTotal;
    }
}
