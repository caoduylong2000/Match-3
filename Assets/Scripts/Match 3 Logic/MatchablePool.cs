using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchablePool : ObjectPool<Matchable>
{
    [SerializeField] private int howManyTypes;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private Material[] materials;
    [SerializeField] private Animation[] animations;
    [SerializeField] private Animator[] animators;

    [SerializeField] private Sprite crossPowerUp;
    [SerializeField] private Sprite match4PowerUp;
    [SerializeField] private Sprite match5PowerUp;

    [SerializeField] private Animation[] powerupAnimations;
    [SerializeField] private Animator[] powerupAnimators;

    public void RandomizeType(Matchable ToRandomize)
    {
        int random = Random.Range(0, howManyTypes);

        ToRandomize.SetType(random, sprites[random], materials[random], animators[random], animations[random]);
    }

    public Matchable GetRandomMatchable()
    {
        Matchable randomMatchable = GetPooledObject();

        RandomizeType(randomMatchable);

        return randomMatchable;
    }

    public int NextType(Matchable matchable)
    {
        int nextType = (matchable.Type + 1) % howManyTypes;

        matchable.SetType(nextType, sprites[nextType], materials[nextType], animators[nextType], animations[nextType]);

        return nextType;
    }

    public Matchable UpgradeMatchable(Matchable toBeUpgraded, MatchType type)
    {
        if (type == MatchType.cross)
            return toBeUpgraded.Upgrade(MatchType.cross, crossPowerUp);

        if (type == MatchType.match4)
            return toBeUpgraded.Upgrade(MatchType.match4, match4PowerUp);

        if (type == MatchType.match5)
            return toBeUpgraded.Upgrade(MatchType.match5, match5PowerUp);

        Debug.LogWarning("Tried to upgrade a matchable with an invalid match type");

        return toBeUpgraded;
    }

    public void ChangeType(Matchable toChange, int type)
    {
        toChange.SetType(type, sprites[type], materials[type], animators[type], animations[type]);
    }
}
