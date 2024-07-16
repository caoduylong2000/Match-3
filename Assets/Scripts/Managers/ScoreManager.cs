using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ScoreManager : Singleton<ScoreManager>
{
    private MatchableGrid grid;
    private MatchablePool pool;
    private AudioMixer audioMixer;

    [SerializeField]
    public Transform collectionPoint;

    //UI text element for displaying the score and combo multiplier
    [SerializeField]
    private Text scoreText,
                 comboText;

    [SerializeField]
    private Image comboSlider;

    //actual score, and a combo multiplier
    private int score,
                comboMultiplier;

    public int Score
    {
        get
        {
            return score;
        }
    }
    //how much time has passed since the player last scored
    private float timeSinceLastScore;

    //how match time should we allow before resetting the combo multiplier?
    [SerializeField]
    private float maxComboTime,
                  currentComboTime;

    //Is Combo time current running?
    private bool timerIsActive;

    //get references to other game objects in Start
    private void Start()
    {
        grid = (MatchableGrid)MatchableGrid.Instance;
        pool = (MatchablePool)MatchablePool.Instance;
        audioMixer = AudioMixer.Instance;

        comboText.enabled = false;
        comboSlider.gameObject.SetActive(false);

    }
    //when player hits retry, reset the score and combo
    public void Reset()
    {
        score = 0;
        scoreText.text = score.ToString();
        timeSinceLastScore = maxComboTime;
    }

    public void AddScore(int amount)
    {
        score += amount * IncreaseCombo();
        scoreText.text = score.ToString();

        timeSinceLastScore = 0;

        if(!timerIsActive)
        {
            StartCoroutine(ComboTimer());
        }

        //play score sound
        audioMixer.PlaySound(SoundEffects.score);
    }

    //combo timer coroutine, counts up to max combotime before resetting the combo multiplier
    private IEnumerator ComboTimer()
    {
        timerIsActive = true;
        comboText.enabled = true;
        comboSlider.gameObject.SetActive(true);

        do
        {
            timeSinceLastScore += Time.deltaTime;
            comboSlider.fillAmount = 1 - timeSinceLastScore / currentComboTime;
            yield return null;
        }
        while (timeSinceLastScore < currentComboTime);

        comboMultiplier = 0;
        comboText.enabled = false;
        comboSlider.gameObject.SetActive(false);
        timerIsActive = false;
    }

    private int IncreaseCombo()
    {
        comboText.text = "Combo X" + ++comboMultiplier;

        currentComboTime = maxComboTime - Mathf.Log(comboMultiplier) / 2;

        return comboMultiplier;
    }

    public IEnumerator ResolveMatch(Match toResolve, MatchType powerUsed = MatchType.invalid)
    {
        Matchable powerupFormed = null;
        Matchable matchable;

        Transform target = collectionPoint;

        //if large match is made, create a powerup
        if (powerUsed == MatchType.invalid && toResolve.Count > 3)
        {
            powerupFormed = pool.UpgradeMatchable(toResolve.ToBeUpgraded, toResolve.Type);
            toResolve.RemoveMatchable(powerupFormed);
            target = powerupFormed.transform;
            powerupFormed.SortingOrder = 3;

            //play upgrade sound
            audioMixer.PlaySound(SoundEffects.upgrade);
        }
        else
        {
            //play resolve sound
            audioMixer.PlaySound(SoundEffects.resolve);
        }

        for (int i =0; i != toResolve.Count; ++i)
        {
            matchable = toResolve.Matchables[i];

            //okly allow gems used as powerups to resolve gems
            if (powerUsed != MatchType.match5 && matchable.IsGem)
            {
                continue;
            }

            //remove the matchables from the grid
            grid.RemoveItemAt(matchable.position);

            //move them off to the side of the screen
            if (i == toResolve.Count - 1)
                yield return StartCoroutine(matchable.Resolve(target));
            else
                StartCoroutine(matchable.Resolve(target));
        }

        //update the player's score
        AddScore(toResolve.Count * toResolve.Count);

        // if there was a powerup, reset the sorting order
        if (powerupFormed != null)
            powerupFormed.SortingOrder = 1;
           
    }
}
