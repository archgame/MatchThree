/// <summary>
/// The main MatchThree script.
/// Contains all methods for creating a basic match three game (like bejeweled).
/// Functions include setting up the initial grid and checking the grid for matches.
/// Script should be attached to the main camera.
/// </summary>
//TODO: install directions for android, Linux, PC, and MAC builds
//TODO: test screen resolution on itch.io and itch.io mobile

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchThree : MonoBehaviour {

    public GameObject prefab;                   //prefab GameObject containing an Image that represents the jewel icon
    public Canvas canvas;                       //existing canvas in the hierarchy
    public Color[] colors;                      //list of colors used to create the jewels //TODO: random colors
    public AudioClip[] hits;                    //hits for falling blocks
    public AudioClip[] taps;                    //tabs for match threes
    private AudioSource audio;                  //audio source

    private const int xSize = 10;               //grid size x
    private const int ySize = 10;               //grid size y
    private const float padding = 0.25f;        //as a percent, 0.25 will create a %25 border for the jewel.
    private const float animationTime = 0.075f; //time in seconds for scaling animation speed.
   
    private int minResolution = 0;              //short end of the screen size
    private int maxSize = 0;                    //maximum grid length from xSize and ySize
    private float spacing = 0;                  //spacing set as 

    private GameObject[,] jewels;               //two-dimensional array containing the jewels.
    private List<GameObject> matchedJewels;     //list to hold jewels that found a match.
    private int score = 0;                      //score
    private int multiplier = 1;                 //multiply used for consecutive matches //TODO: impliment multiplier to score

    [HideInInspector]
    public bool isAnimating = false;            //boolean to disallow input if animation happening
    [HideInInspector]
    public GameObject selected1;                //first jewel swap selection
    [HideInInspector]
    public GameObject selected2;                //second jewel swap selection

    

    // Use this for initialization
    void Start () {
        jewels = new GameObject[xSize,ySize];                   //initialize two-dimensional array
        matchedJewels = new List<GameObject>();                 //initialize list
        minResolution = Mathf.Min(Screen.width, Screen.height); //find minimum screen resolution
        maxSize = Mathf.Max(ySize, xSize); //find maximum grid size //TODO: screen resolution adjusts for mobile and itch.io
        spacing = minResolution / maxSize*1.00f; //set spacing based on minimum screen resolution and maximum grid size
        audio = this.GetComponent<AudioSource>();
        SetupBoard();
    }	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("space") && !isAnimating)
            ResetBoard();
        if (Input.GetKeyDown("escape") || Input.GetKeyDown("q"))
            Quit();
        if (selected1 != null && selected2 != null && !isAnimating)
            StartCoroutine(SwapJewels(selected1, selected2));
    }
    // Reset board
    public void ResetBoard()
    {
        UpdateScore(0);
        SetupBoard();
    }
    // Update score
    private void UpdateScore(int s)
    {
        score = s; //Debug.Log(string.Format("score: {0}", score));
        //TODO: visualize score when updated
        //TODO: persistent high score
    }


    // Setup the board with a random assortment of colored jewels
    public void SetupBoard()
    {
        foreach (Transform child in canvas.transform)
            Destroy(child.gameObject);

        for(int i = 0;i<xSize;i++)
        {
            for(int j =0;j<ySize;j++)
            {
                jewels[i, j] = SetupJewel(i,j);
            }
        }
        CheckForMatch();
    }
    //Sets up jewel based on two-dimensional array value
    public GameObject SetupJewel(int i, int j)
    {
        Vector3 position = SetJewelPosition(j, i);
        GameObject obj = Instantiate(prefab, position, Quaternion.identity) as GameObject;
        obj.transform.SetParent(canvas.transform);
        obj.GetComponent<Image>().color = colors[Random.Range(0, colors.Length)];
        obj.GetComponent<RectTransform>().sizeDelta = SetJewelScale();
        obj.name = string.Format("jewel[{0},{1}]",i,j);
        Interaction interaction = obj.GetComponent<Interaction>();
        interaction.x = i;
        interaction.y = j;
        return obj;
    }
    //set the jewel position based on screen size and grid size
    private Vector3 SetJewelPosition(int x, int y)
    {
        float X = x * (spacing) + (Screen.width - (spacing * (xSize - 1))) / 2.0f;
        float Y = (y * (-spacing) + Screen.height) - (spacing) / 2.0f;
        return new Vector3(X, Y, 0);
    }
    //scale the jewel based on grid size and screen size
    private Vector2 SetJewelScale()
    {
        return new Vector2(spacing * (1 - padding), spacing * (1 - padding));
    }
    //change jewel position
    private void UpdateJewel(int x, int y, int xNew, int yNew)
    {
        audio.clip = taps[Random.Range(0, taps.Length)];
        audio.Play();
        jewels[x, y].transform.position = SetJewelPosition(yNew, xNew);
        jewels[xNew, yNew] = jewels[x, y];
        Interaction interaction = jewels[xNew, yNew].GetComponent<Interaction>();
        interaction.x = xNew;
        interaction.y = yNew;
        jewels[xNew, yNew].name = string.Format("jewel[{0},{1}]", xNew, yNew);
        jewels[x, y] = null; //to confirm the update, we set the previous jewel to null
    }


    // check each jewel for match, starting at the top left and going right and then down
    public void CheckForMatch()
    {
        for (int i = xSize; i-- > 0;)
        {
            for (int j = 0; j < ySize; j++)
            {
                if (i != 0 && j != 0 || i != xSize - 1 && j != ySize - 1
                    || i != xSize - 1 && j != 0 || i != 0 && j != ySize - 1){

                    Color color = jewels[i, j].GetComponent<Image>().color;

                    bool matchedHorizontal = false;
                    if(j != 0 && j != ySize -1){
                        Color colorUp = jewels[i, j + 1].GetComponent<Image>().color;
                        Color colorDown = jewels[i, j - 1].GetComponent<Image>().color;
                        matchedHorizontal = CompareColors(color, colorUp, colorDown);
                    }

                    bool matchedVertical = false;
                    if (i != 0 && i != xSize - 1){                       
                        Color colorLeft = jewels[i - 1, j].GetComponent<Image>().color;
                        Color colorRight = jewels[i + 1, j].GetComponent<Image>().color;
                        matchedVertical = CompareColors(color, colorLeft, colorRight);
                    }

                    //TODO: check for vertical and horizontal matches (star)
                    if (matchedHorizontal)
                    {
                        matchedJewels.Add(jewels[i, j - 1]);
                        matchedJewels.Add(jewels[i, j]);
                        matchedJewels.Add(jewels[i, j + 1]);
                        CheckExtraMatch(i, j, i, j + 1);
                        StartCoroutine(DestroyWait());
                        return;
                    }
                    if (matchedVertical)
                    {
                        matchedJewels.Add(jewels[i - 1, j]);
                        matchedJewels.Add(jewels[i, j]);
                        matchedJewels.Add(jewels[i + 1, j]);
                        CheckExtraMatch(i,j,i-1,j);
                        StartCoroutine(DestroyWait());
                        return;
                    }               
                }
            }
        }
        //TODO: indication when match checking has completed and player input is required.
        Debug.Log("No Matches Found");
    }
    // Compare three colors to test for match three
    private bool CompareColors(Color c, Color c1, Color c2)
    {
        if (c.Equals(c1) && c.Equals(c2))
            return true;
        return false;
    }
    // Recursively check for matches greather than three
    private void CheckExtraMatch(int x1, int y1, int x2, int y2)
    {
        int dX = x2 - x1;
        int dY = y2 - y1;
        int x3 = x2 + dX;
        int y3 = y2 + dY;
        
        if (x3 >= 0 && y3 != ySize) //make sure we aren't going past the edge of the grid
        {
            Color c2 = jewels[x2, y2].GetComponent<Image>().color;
            Color c3 = jewels[x3, y3].GetComponent<Image>().color;
            if (c2.Equals(c3))
            {
                matchedJewels.Add(jewels[x3, y3]);
                CheckExtraMatch(x2, y2, x3, y3);
            }
        }
    }


    // Visualize a match three and begin cascade process
    IEnumerator DestroyWait()
    {
        isAnimating = true;
        yield return new WaitForSeconds(animationTime * 3);
        //TODO: make matched jewels blink
        audio.clip = hits[Random.Range(0, hits.Length)];
        audio.Play();
        foreach (GameObject jewel in matchedJewels)
            jewel.GetComponent<Image>().color = Color.white;

        yield return new WaitForSeconds(animationTime*3);
        for (int i = matchedJewels.Count; i-- > 0;)
        {
            UpdateScore(score + 1);
            Destroy(matchedJewels[i]);
            matchedJewels.RemoveAt(i);
        }

        yield return new WaitForSeconds(animationTime*3); //stutter animation
        for (int i = xSize; i-- > 0;) //search grid starting at bottom row and moving up
        {
            for(int j = 0;j<ySize;j++) //search grid row from left to right
            {
                if(jewels[i,j] == null) //if we find an empty item, we need to fill it
                {
                    bool top = true;
                    for(int k = i;k-- >0;) //search items above empty item and bring closest one down
                    {
                        if(jewels[k,j] != null && top)
                        {
                            UpdateJewel(k, j, i, j);
                            yield return new WaitForSeconds(animationTime / 3.00f); //stutter animation
                            top = false;
                            break;
                        }
                    }
                    if(top) //if there are no items above and we've reach the top, create a new jewel
                    {
                        jewels[i, j] = SetupJewel(i, j);
                        yield return new WaitForSeconds(animationTime / 3.00f); //stutter animation
                    }
                }
            }
        }

        isAnimating = false;
        CheckForMatch(); //after adjustments always check for new matches
    }
    //two jewels selected, swap colors
    IEnumerator SwapJewels(GameObject jewel1, GameObject jewel2)
    {
        isAnimating = true;
        Image image1 = jewel1.GetComponent<Image>();
        Image image2 = jewel2.GetComponent<Image>();
        Color color1 = image1.color;
        Color color2 = image2.color;
        Interaction interaction1 = jewel1.GetComponent<Interaction>();
        Interaction interaction2 = jewel2.GetComponent<Interaction>();
        image1.color = Color.white; //TODO: visualize swap as movement instead of white color change
        image2.color = Color.white;

        yield return new WaitForSeconds(animationTime * 3);
        audio.clip = taps[Random.Range(0, taps.Length)];
        audio.Play();
        image1.color = color2;
        image2.color = color1;
        selected1 = null;
        selected2 = null;

        yield return new WaitForSeconds(animationTime * 3);
        isAnimating = false;
        CheckForMatch();
    }
    //play sound when jewel selected
    public void ClickSound()
    {
        audio.clip = taps[Random.Range(0, taps.Length)];
        audio.Play();
    }

    //exit game
    private void Quit()
    {
        #if UNITY_EDITOR
                // Application.Quit() does not work in the editor so
                // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }

}
