using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private Fader loadingScreen;

    [SerializeField]
    private Canvas canvas;

    private bool timeLevel = false;

    private void Start()
    {
        loadingScreen.Hide(false);
        StartCoroutine(loadingScreen.Fade(0));

        Camera camera = GetComponent<Camera>();

        Canvas canvas = GetComponent<Canvas>();
    }

    private IEnumerator Quit()
    {
        yield return StartCoroutine(loadingScreen.Fade(1));
        Application.Quit();
    }

    private IEnumerator StartSurvival()
    {
        yield return StartCoroutine(loadingScreen.Fade(1));
        SceneManager.LoadScene("Survival");
    }

    private IEnumerator StartTimeRush()
    {
        yield return StartCoroutine(loadingScreen.Fade(1));
        SceneManager.LoadScene("TimeRush");
    }

    public void QuitButtonPressed()
    {
        StartCoroutine(Quit());
    }

    public void SurvivalButtonPressed()
    {
        StartCoroutine(StartSurvival());

    }

    public void TimeRushButtonPressed()
    {
        StartCoroutine(StartTimeRush());
    }
}
