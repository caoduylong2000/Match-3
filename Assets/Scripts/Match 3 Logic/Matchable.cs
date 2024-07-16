using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Matchable : Movable
{
    private MatchablePool pool;
    private MatchableGrid grid;
    private Cursor cursor;
    private Animation animations;
    private Animator animators;
    private MatchType powerup = MatchType.invalid;
    public bool IsGem
    {
        get
        {
            return powerup == MatchType.match5;
        }
    }

    private int type;
    public int Type
    {
        get
        {
            return type;
        }
    }

    private SpriteRenderer spriteRenderer;

    //where is the matchable on the grid?
    public Vector2Int position;

    private void Awake()
    {
        cursor = Cursor.Instance;
        pool = (MatchablePool)MatchablePool.Instance;
        grid = (MatchableGrid)MatchableGrid.Instance;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animations = GetComponent<Animation>();
        animators = GetComponent<Animator>();
    }

    public void SetType(int type, Sprite sprite, Material material, Animator animator, Animation animation)
    {
        this.type = type;
        animators = animator;
        animations = animation;
        spriteRenderer.sprite = sprite;
        spriteRenderer.material = material;
    }

    public IEnumerator Resolve(Transform collectionPoint)
    {
        // if matchable is a powerup, resolve is as such
        if(powerup != MatchType.invalid)
        {
            // resolve a match4 powerup
            if(powerup == MatchType.match4)
            {
                //score everything adjacent to this
                grid.MatchRowAndColumn(this);

            }

            // resolve a cross powerup
            if(powerup == MatchType.cross)
            {
                grid.MatchAllAdjancent(this);
            }

            powerup = MatchType.invalid;
        }
        //if this was called as the result of a powerup being upgraded, then don't move this off the grid
        if (collectionPoint == null)
            yield break;

        //draw above other the grid
        spriteRenderer.sortingOrder = 2;

        //move off the grid to a collection point
        yield return StartCoroutine(MoveToTransform(collectionPoint));

        //reset
        spriteRenderer.sortingOrder = 1;

        //return back to the pool
        pool.ReturnObjectToPool(this);
    }

    //change the sprite of this matchale to be a powerup while retaining colour and type (will be change to add VFX)
    public Matchable Upgrade(MatchType powerupType, Sprite powerupSprite)
    {
        //if this is already a powerup, resolve it before upgrading
        if(powerup != MatchType.invalid)
        {
            idle = false;
            StartCoroutine(Resolve(null));
            idle = true;
        }

        if (powerupType == MatchType.match5)
        {
            type = -1;
            spriteRenderer.color = Color.white;
        }

        powerup = powerupType;
        spriteRenderer.sprite = powerupSprite;
        StartCoroutine(BeBusyForOneFrame());
        return this;
    }

    public IEnumerator BeBusyForOneFrame()
    {
        idle = false;
        yield return null;
        idle = true;
    }

    //set the sorting order of the sprite renderer so it will be drawn above or below others
    public int SortingOrder
    {
        set
        {
            spriteRenderer.sortingOrder = value;
        }
    }

    private void OnMouseDown()
    {
        cursor.SelectFirst(this);
    }

    private void OnMouseUp()
    {
        cursor.SelectFirst(null);
    }

    private void OnMouseEnter()
    {
        cursor.SelectSecond(this);
    }

    public override string ToString()
    {
        return gameObject.name;
    }
}
