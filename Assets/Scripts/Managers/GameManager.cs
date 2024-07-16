using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/*
 * this class will set up the scene and initialize objects
 * 
 * this class inherits form Singleton so many oter script can access it easily through GameManager.Instance
 */
public class GameManager : Singleton<GameManager>
{
    private MatchablePool pool;
    private MatchableGrid grid;
    private Cursor cursor;
    private AudioMixer audioMixer;
    private ScoreManager score;
    private AdsManager adsManager;
    private bool bonusTime;

    [SerializeField]
    private Button rewardButton;

    [SerializeField]
    private Fader loadingScreen,
                  darkener;

    // the dimension of ther matchable grid, set in the inspector
    [SerializeField]
    private Vector2Int dimensions = Vector2Int.one;

    [SerializeField]
    private Text finalScoreText;

    [SerializeField]
    private Movable resultsPage;

    [SerializeField]
    private bool levelIsTimed;

    [SerializeField]
    private LevelTimer timer;

    [SerializeField]
    private float timeLimit;

    //a UI Text for displaying the contents of the grid data
    //for testing and debugging purpose only
    //[SerializeField]
    //private Text gridOutput;

    [SerializeField]
    private bool debugMode;

    private void Start()
    {
        //get references to other inportant game objects
        pool = (MatchablePool)MatchablePool.Instance;
        grid = (MatchableGrid)MatchableGrid.Instance;
        cursor = Cursor.Instance;
        audioMixer = AudioMixer.Instance;
        score = ScoreManager.Instance;

        adsManager = GetComponent<AdsManager>();

        StartCoroutine(Setup());
    }

    // remember to comment this out before building
    private void Update()
    {
        if (debugMode && Input.GetButtonDown("Jump"))
            NoMoreMoves();
    }

    private IEnumerator Setup()
    {
        //disable user input
        cursor.enabled = false;

        //unhide loading screen
        loadingScreen.Hide(false);

        //if level is timed, set the timer
        if (levelIsTimed)
            timer.SetTimer(timeLimit);

        //pool the matchables
        pool.PoolObjects(dimensions.x * dimensions.y);

        // create the grid
        grid.InitializeGrid(dimensions);

        //fade out the loading screen
        StartCoroutine(loadingScreen.Fade(0));

        //start BG Music
        audioMixer.PlayMusic();

        //populated the grid
        yield return StartCoroutine(grid.PopulateGrid(false, true));

        //check for gridlock a offer a player a hint if they need it
        grid.CheckPossibleMoves();

        //enable user input
        cursor.enabled = true;

        //if level is timed, start the timer
        if (levelIsTimed)
        {
            bonusTime = true;
            StartCoroutine(timer.Countdown());
        }

    }

    public void NoMoreMoves()
    {
        //if the level is timed, reward the player for running out of moves
        if (levelIsTimed)
            grid.MatchEverything();
        
        //in survival mode, punish the player for running out of moves
        GameOver();
    }

    public void GameOver()
    {
        // get and update the final score for the results page
        finalScoreText.text = score.Score.ToString();

        //disable the cursor
        cursor.enabled = false;

        //unhide the darkener and fade in
        darkener.Hide(false);
        StartCoroutine(darkener.Fade(0.8f));

        if(bonusTime == true)
            rewardButton.interactable = true;
        else
            rewardButton.interactable = false;

        //move the results page on to the screen
        StartCoroutine(resultsPage.MoveToPosition(new Vector2(Screen.width / 2, Screen.height / 2)));

    }

    private IEnumerator Quit()
    {
        yield return StartCoroutine(loadingScreen.Fade(1));
        SceneManager.LoadScene("Menu");
    }

    public void QuitButtonPressed()
    {
        StartCoroutine(Quit());
    }

    private IEnumerator Retry()
    {
        //fade out the darkener, nand move the resilts page off screen
        StartCoroutine(resultsPage.MoveToPosition(new Vector2(Screen.width / 2, Screen.height / 2) + Vector2.down * 1000));
        yield return StartCoroutine(darkener.Fade(0));
        darkener.Hide(true);

        //reset the cursor, game grid, and score

        if (levelIsTimed)
            timer.SetTimer(timeLimit);

        cursor.Reset();
        score.Reset();
        yield return StartCoroutine(grid.Reset());

        //let the player start playing again
        cursor.enabled = true;

        //if level is timed, start the timer
        if (levelIsTimed)
            StartCoroutine(timer.Countdown());
    }

    public void RetryButtonPressed()
    {
        StartCoroutine(Retry());
        bonusTime = true;
        rewardButton.interactable = true;
    }

    private IEnumerator RewardTime()
    {
        //fade out the darkener, nand move the resilts page off screen
        StartCoroutine(resultsPage.MoveToPosition(new Vector2(Screen.width / 2, Screen.height / 2) + Vector2.down * 1000));
        yield return StartCoroutine(darkener.Fade(0));
        darkener.Hide(true);

        //reset the cursor, game grid, and score

        if (levelIsTimed)
            timer.SetTimer(20);

        cursor.Reset();

        //let the player start playing again
        cursor.enabled = true;

        //if level is timed, start the timer
        if (levelIsTimed)
            StartCoroutine(timer.Countdown());
    }

    public void RewardButtonPressed()
    {
        if (bonusTime == true)
        {
            adsManager.ShowAd();
            StartCoroutine(RewardTime());
            bonusTime = false;

            
        }
        else
        {
        }

    }
}
