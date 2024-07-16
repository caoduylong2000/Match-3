using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Animation))]
public class Flip : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private MatchablePool pool;

    private Animator controller;
    private Animation anim;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animation>();
        controller = GetComponent<Animator>();
        pool = (MatchablePool)MatchablePool.Instance;

        //if (sprites.Length != 0)
        //    spriteRenderer.sprite = sprites[0];

        //timeBetweenFrames = 1f / framerate;
    }

    //private IEnumerator Animating()
    //{
        //if (sprites.Length < 2)
        //    yield break;

        //animating = true;
        //while (animating)
        //{
        //    if (++frame == sprites.Length)
        //        frame = 0;

        //    spriteRenderer.sprite = sprites[frame];

        //    yield return new WaitForSeconds(timeBetweenFrames);
        //}
        //frame = 0;
        //spriteRenderer.sprite = sprites[0];
    //}

    //private void OnMouseEnter()
    //{
    //    Debug.Log(spriteRenderer.sprite);
    //    if (!animating)
    //        StartCoroutine(Animating());
    //}
    //private void OnMouseExit()
    //{
    //    animating = false;
    //}

    private void OnMouseEnter()
    {
        //if (anim)
        //    Debug.Log("Animation Availabe");
        //else
        //    Debug.LogWarning("Non Animation");

        //if (controller)
        //    Debug.Log("Animator Availabe");
        //else
        //    Debug.LogWarning("Non Animator");
    }
}