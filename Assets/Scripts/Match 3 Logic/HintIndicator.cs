using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

/*
 * this script allows displaying a sprite over a transform
 * either when a button is pressed or automatically after time delay
 * or enabling the button after the time delay
 * 
 * this is a singleton and requires a SpriteRenderer component
 */

[RequireComponent(typeof(SpriteRenderer))]
public class HintIndicator : Singleton<HintIndicator>
{
    //the sprite renderer component attached to this game object
    private SpriteRenderer spriteRenderer;

    //where the hint will be displayed
    private Transform hintLocation;

    //a UI Button component that displays a hint when pressed
    [SerializeField] private Button hintButton;

    //how many seconds should we wait before offering a hint?
    [SerializeField] float delayBeforeAutoHint = 10f;

    //the coroutine that delays before autohint
    private Coroutine autoHintCR;

    //disable by default, get sprite renderer
    protected override void Init()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
        hintButton.interactable = false;
    }

    //force the hint to display on the desired transform
    public void Indicator(Transform hintLocation)
    {
        CancelHint();
        transform.position = hintLocation.position;
        spriteRenderer.enabled = true;
        if(autoHintCR != null)
            StopCoroutine(autoHintCR);

        autoHintCR = null;
    }

    //remove the hint, and interrupt whatever coroutine might be running
    public void CancelHint()
    {
        spriteRenderer.enabled = false;
        hintButton.interactable = false;
    }

    //enable the hint button
    public void EnableHintButton()
    {
        hintButton.interactable = true;
    }

    //start a coroutine that will wait before offering the hint
    public void StartAutoHint(Transform hintLocation)
    {
        this.hintLocation = hintLocation;

        autoHintCR =  StartCoroutine(WaitAndIndicateHint());
    }

    //the coroutine that waits before displaying the hint
    private IEnumerator WaitAndIndicateHint()
    {
        yield return new WaitForSeconds(delayBeforeAutoHint);
        EnableHintButton();
        //Indicator(hintLocation);
    }
}
