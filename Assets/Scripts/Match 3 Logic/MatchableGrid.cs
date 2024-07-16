using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class MatchableGrid : GridSystem<Matchable>
{

    [SerializeField] private Vector3 offscreenOffset;
    [SerializeField] private List<Matchable> possibleMoves;

    private MatchablePool pool;
    private ScoreManager score;
    private HintIndicator hint;
    private AudioMixer audioMixer;

    private void Start()
    {
        pool = (MatchablePool)MatchablePool.Instance;
        score = ScoreManager.Instance;
        hint = HintIndicator.Instance;
        audioMixer = AudioMixer.Instance;
    }

    public IEnumerator Reset()
    {
        for (int y = 0; y != Dimensions.y; ++y)
            for (int x = 0; x != Dimensions.x; ++x)
                if (!IsEmpty(x, y))
                    pool.ReturnObjectToPool(RemoveItemAt(x, y));

        yield return StartCoroutine(PopulateGrid(false, true));
    }

    
    //Populate the grid ưith matchables from the pool
    //Optionally allow or not allow match to  be made while populating
    public IEnumerator PopulateGrid(bool allowMatched = false, bool initialPopulation = false)
    {
        //list of new matchables addded during population
        List<Matchable> newMatchables = new List<Matchable>();

        Matchable newMatchable;
        Vector3 onscreenPosition;

        for (int y = 0; y != Dimensions.y; ++y)
            for (int x = 0; x != Dimensions.x; ++x)
                if(IsEmpty(x,y))
                {
                    //get a matchable from the pool
                    newMatchable = pool.GetRandomMatchable();

                    //position the matchable on screen
                    //newMatchable.transform.position = transform.position + new Vector3(x, y);
                    newMatchable.transform.position =  transform.position + new Vector3(x, y) + offscreenOffset;

                    //activate the matchable
                    newMatchable.gameObject.SetActive(true);

                    //tell this matchable where it is on the grid
                    newMatchable.position = new Vector2Int(x, y);

                    //place the matchable in the grid
                    PutItemAt(newMatchable, x, y);

                    //add the new matchable to the list
                    newMatchables.Add(newMatchable);

                    //what was the initial type of the new matchable?
                    int initialType = newMatchable.Type;

                    while (!allowMatched && IsPartOfAMatch(newMatchable))
                    {
                        // change the matchable's type until it isn't a match anymore
                        if (pool.NextType(newMatchable) == initialType)
                        {
                            Debug.LogWarning("Failed to find a matchable type that didn't match at (" + x + "," + y + ")");
                            Debug.Break();
                            break;
                        }
                    }
                    
                    //pause for 1/10 of a second after each for cool effect (Generate Pool)
                    yield return new WaitForSeconds(0.001f);
                }
        
        //move each matchable to its on screen position, yielding unitil the alst has finished
        for(int i = 0; i != newMatchables.Count; ++i)
        {
            //calculate the future on screen posotion of the matchable
            onscreenPosition = transform.position + new Vector3(newMatchables[i].position.x, newMatchables[i].position.y);

            //play a landing sound after a delay when the matchable land in position
            audioMixer.PlayDelayedSound(SoundEffects.land, 1f / newMatchables[i].Speed);
            

            //move the matchable to its on screen position
            if (i == newMatchables.Count -1)
                yield return StartCoroutine(newMatchables[i].MoveToPosition(onscreenPosition));
            else
                StartCoroutine(newMatchables[i].MoveToPosition(onscreenPosition));

            //pause for 1/10 of a second after each for cool effect
            if(initialPopulation)
                yield return new WaitForSeconds(0.1f);
        }
    }

    //check if the matchable being populated is part of a match or not
    private bool IsPartOfAMatch(Matchable toMatch)
    {
        int horizontalMatches = 0,
            verticalMatches = 0;

        //Look left and right
        horizontalMatches += CountMatchesInDirection(toMatch, Vector2Int.left);
        horizontalMatches += CountMatchesInDirection(toMatch, Vector2Int.right);

        if (horizontalMatches > 1)
            return true;

        //Look up and down
        verticalMatches += CountMatchesInDirection(toMatch, Vector2Int.up);
        verticalMatches += CountMatchesInDirection(toMatch, Vector2Int.down);

        if (verticalMatches > 1)
            return true;

        return false;
    }

    //count the number of matches on the grid starting from the mathcable to match moving in the direction indicated
    private int CountMatchesInDirection(Matchable toMatch, Vector2Int direction)
    {
        int matches = 0;
        Vector2Int position = toMatch.position + direction;

        while (CheckBounds(position) && !IsEmpty(position) && GetItemAt(position).Type == toMatch.Type)
        {
            ++matches;
            position += direction;
        }
        return matches;
    }

    public IEnumerator TrySwap(Matchable[] toBeSwapped)
    {
        //make a local copy of what we're swapping so Cursor doesn't overwrite
        Matchable[] copies = new Matchable[2];
        copies[0] = toBeSwapped[0];
        copies[1] = toBeSwapped[1];

        // hide the hint indicator
        hint.CancelHint();

        //yield until matchables animate swapping
        yield return StartCoroutine(Swap(copies));

        //special cases for gems
        //if 1 is a gem, then match all matching the color of the other
        if (copies[0].IsGem && copies[1].IsGem)
        {
            MatchEverything();
            yield break;
        }
        else if (copies[0].IsGem)
        {
            MatchEverythingByType(copies[0], copies[1].Type);
            yield break;
        }
        else if (copies[1].IsGem)
        {
            MatchEverythingByType(copies[1], copies[0].Type);
            yield break;
        }

        //check for a valid match
        Match[] matches = new Match[2];

        matches[0] = GetMatch(copies[0]);
        matches[1] = GetMatch(copies[1]);

        //check for a valid match
        if (matches[0] != null)
            StartCoroutine(score.ResolveMatch(matches[0]));

        if (matches[1] != null)
            StartCoroutine(score.ResolveMatch(matches[1]));

        //if there's no match, swap them back
        if (matches[0] == null && matches[1] == null)
        {
            yield return StartCoroutine(Swap(copies));

            if(ScanForMatches())
                StartCoroutine(Swap(copies));
        }
        else
            StartCoroutine(FillAndScanGrid());
    }

    private IEnumerator FillAndScanGrid()
    {
        //if there's no match, swap them back
        CollapseGrid();
        yield return StartCoroutine(PopulateGrid(true));
        //scan the grid for chain reactions
        if (ScanForMatches())
            StartCoroutine(FillAndScanGrid());
        //if no chain reactions, grid is idle, so check the possible moves
        else
            CheckPossibleMoves();
    }

    public void CheckPossibleMoves()
    {
        if(ScanForMove() == 0)
        {
            //no move
            GameManager.Instance.NoMoreMoves();
        }
        else
        {
            //offer a hint
            //hint.EnableHintButton();

            hint.StartAutoHint(possibleMoves[Random.Range(0, possibleMoves.Count)].transform);
        }
    }

    private Match GetMatch(Matchable toMatch)
    {
        Match match = new Match(toMatch);

        Match horizontalMatch,
              verticalMatch;

        horizontalMatch = GetMatchesInDirection(match, toMatch, Vector2Int.left);
        horizontalMatch.Merge(GetMatchesInDirection(match, toMatch, Vector2Int.right));

        horizontalMatch.orientation = Orientation.horizontal;

        if(horizontalMatch.Count > 1)
        {
            match.Merge(horizontalMatch);
            //scan for horizontal branches
            GetBranches(match, horizontalMatch, Orientation.vertical);
        }
            

        verticalMatch = GetMatchesInDirection(match, toMatch, Vector2Int.up);
        verticalMatch.Merge(GetMatchesInDirection(match, toMatch, Vector2Int.down));

        verticalMatch.orientation = Orientation.vertical;

        if (verticalMatch.Count > 1)
        {
            match.Merge(verticalMatch);
            //scan for vertical branches
            GetBranches(match, verticalMatch, Orientation.horizontal);
        }
            

        if (match.Count == 1)
            return null;

        return match;
    }

    private void GetBranches(Match tree, Match branchToSearch, Orientation perpendicular)
    {
        Match branch;

        foreach(Matchable matchable in branchToSearch.Matchables)
        {
            branch = GetMatchesInDirection(tree, matchable, perpendicular == Orientation.horizontal ? Vector2Int.left : Vector2Int.down);
            branch.Merge(GetMatchesInDirection(tree, matchable, perpendicular == Orientation.horizontal ? Vector2Int.right : Vector2Int.up));

            branch.orientation = perpendicular;

            if(branch.Count> 1)
            {
                tree.Merge(branch);
                GetBranches(tree, branch, perpendicular == Orientation.horizontal ? Orientation.vertical : Orientation.horizontal);
            }
        }
    }

    //Add each matching matchable in the direction to a match and return in
    private Match GetMatchesInDirection(Match tree, Matchable toMatch, Vector2Int direction)
    {
        Match match = new Match();
        Vector2Int position = toMatch.position + direction;
        Matchable next;

        while (CheckBounds(position) && !IsEmpty(position))
        {
            next = GetItemAt(position);
            if (next.Type == toMatch.Type && next.Idle)
            {
                if (!tree.Contains(next))
                    match.AddMatchable(GetItemAt(position));
                else
                    match.AddUnlisted();

                position += direction;
            }
            else
                break;
            
        }
        return match;
    }

    private IEnumerator Swap(Matchable[] toBeSwapped)
    {
        // swap them in the grid data structure
        SwapItemsAt(toBeSwapped[0].position, toBeSwapped[1].position);

        //tell the mathcables their new positions
        Vector2Int temp = toBeSwapped[0].position;
        toBeSwapped[0].position = toBeSwapped[1].position;
        toBeSwapped[1].position = temp;

        //get the world positions of boths
        Vector3[] worldPosition = new Vector3[2];
        worldPosition[0] = toBeSwapped[0].transform.position;
        worldPosition[1] = toBeSwapped[1].transform.position;

        //play swap sound
        audioMixer.PlaySound(SoundEffects.swap);

        //move them to their new positions on screen
                     StartCoroutine(toBeSwapped[0].MoveToPosition(worldPosition[1]));
        yield return StartCoroutine(toBeSwapped[1].MoveToPosition(worldPosition[0]));
    }
    
    private void CollapseGrid()
    {
    /*
     * Go through each column left to right,
     * search from bottom up to find an empty space,
     * then look above the empty space, and up through the rest of the column,
     * until you find a non empty space.
     * Move the matchable at the non empty space into the empty sapce,
     * then contunue looking for empty spaces;
     */
        for (int x = 0; x != Dimensions.x; ++x)
            for(int yEmpty = 0; yEmpty != Dimensions.y -1; ++yEmpty)
                if(IsEmpty(x, yEmpty))
                    for(int yNotEmpty = yEmpty +1; yNotEmpty != Dimensions.y; ++ yNotEmpty)
                        //if(!IsEmpty(x, yNotEmpty) && GetItemAt(x, yNotEmpty).Idle)
                        if (!IsEmpty(x, yNotEmpty))
                        {
                            MoveMatchableToPosition(GetItemAt(x, yNotEmpty), x, yEmpty);
                            break;
                        }
    }

    private void MoveMatchableToPosition(Matchable toMove, int x, int y)
    {
        //move the matchable to its new position on the grid
        MoveItemTo(toMove.position, new Vector2Int(x, y));

        //update the matchable's internal grid position
        toMove.position = new Vector2Int(x, y);

        //start animation to move it on screen
        StartCoroutine(toMove.MoveToPosition(transform.position + new Vector3(x, y)));

        //play a landing sound after a delay when the matchable land in position
        audioMixer.PlayDelayedSound(SoundEffects.land, 1f / toMove.Speed);
    }

    //scan the grid for any matches and resolve them
    private bool ScanForMatches()
    {
        bool madeMatch = false;
        Matchable toMatch;
        Match match;

        //interate through the grid, looking for non-empty and idle matchables
        for(int y = 0; y != Dimensions.y; ++y)
            for (int x = 0; x != Dimensions.x; ++x)
                if(!IsEmpty(x,y))
                {
                    toMatch = GetItemAt(x, y);

                    if (!toMatch.Idle)
                        continue;

                    // try to match and resolve
                    match = GetMatch(toMatch);

                    if (match != null)
                    {
                        madeMatch = true;
                        StartCoroutine(score.ResolveMatch(match));
                    }
                        
                }
        return madeMatch;
    }

    //make a match of all matchables adjacent to this power up on the gird and resolve it
    public void MatchAllAdjancent(Matchable powerup)
    {
        Match allAdjacent = new Match();

        for(int y = powerup.position.y - 1; y != powerup.position.y + 2; ++y)
            for (int x = powerup.position.x - 1 ; x != powerup.position.x + 2; ++x)
                if(CheckBounds(x,y) && !IsEmpty(x,y) && GetItemAt(x, y).Idle)
                {
                    allAdjacent.AddMatchable(GetItemAt(x, y));
                }
        StartCoroutine(score.ResolveMatch(allAdjacent, MatchType.cross));

        //play powerup sound
        audioMixer.PlaySound(SoundEffects.powerup);
    }

    public void MatchRow(Matchable powerup)
    {
        Match rowAndColumn = new Match();

        for (int y = 0; y != Dimensions.y; ++y)
            if (CheckBounds(powerup.position.x, y) && !IsEmpty(powerup.position.x, y) && GetItemAt(powerup.position.x, y).Idle)
                rowAndColumn.AddMatchable(GetItemAt(powerup.position.x, y));

        StartCoroutine(score.ResolveMatch(rowAndColumn, MatchType.match4));

        //play powerup sound
        audioMixer.PlaySound(SoundEffects.powerup);
    }

    public void MatchColumn(Matchable powerup)
    {
        Match rowAndColumn = new Match();

        for (int x = 0; x != Dimensions.x; ++x)
            if (CheckBounds(x, powerup.position.y) && !IsEmpty(x, powerup.position.y) && GetItemAt(x, powerup.position.y).Idle)
                rowAndColumn.AddMatchable(GetItemAt(x, powerup.position.y));

        StartCoroutine(score.ResolveMatch(rowAndColumn, MatchType.match4));

        //play powerup sound
        audioMixer.PlaySound(SoundEffects.powerup);
    }

    //matke a match of everthing in the row and column that contains the power up and resolve it
    public void MatchRowAndColumn(Matchable powerup)
    {
        Match rowAndColumn = new Match();

        for (int y = 0; y != Dimensions.y; ++y)
            if (CheckBounds( powerup.position.x, y) && !IsEmpty(powerup.position.x, y) && GetItemAt(powerup.position.x, y).Idle)
                rowAndColumn.AddMatchable(GetItemAt(powerup.position.x, y));

        for (int x = 0; x != Dimensions.x; ++x)
            if (CheckBounds(x, powerup.position.y) && !IsEmpty(x, powerup.position.y) && GetItemAt(x, powerup.position.y).Idle)
                rowAndColumn.AddMatchable(GetItemAt(x, powerup.position.y));

        StartCoroutine(score.ResolveMatch(rowAndColumn, MatchType.match4));

        //play powerup sound
        audioMixer.PlaySound(SoundEffects.powerup);
    }

    //match everything on the grid with a specific type and resolve it
    public void MatchEverythingByType(Matchable gem, int type)
    {
        Match everythingByType = new Match(gem);

        for (int y = 0; y != Dimensions.y; ++y)
            for (int x = 0; x != Dimensions.x; ++x)
                if (CheckBounds(x, y) && !IsEmpty(x, y) && GetItemAt(x, y).Idle && GetItemAt(x,y).Type == type)
                    everythingByType.AddMatchable(GetItemAt(x, y));

        StartCoroutine(score.ResolveMatch(everythingByType, MatchType.match5));
        StartCoroutine(FillAndScanGrid());

        //play powerup sound
        audioMixer.PlaySound(SoundEffects.powerup);
    }

    //match everything on the grid and resolve it
    public void MatchEverything()
    {
        Match everything = new Match();

        for (int y = 0; y != Dimensions.y; ++y)
            for (int x = 0; x != Dimensions.x; ++x)
                if (CheckBounds(x, y) && !IsEmpty(x, y) && GetItemAt(x, y).Idle)
                    everything.AddMatchable(GetItemAt(x, y));

        StartCoroutine(score.ResolveMatch(everything, MatchType.match5));
        StartCoroutine(FillAndScanGrid());

        //play powerup sound
        audioMixer.PlaySound(SoundEffects.powerup);
    }

    //scan for all possible move
    private int ScanForMove()
    {
        possibleMoves = new List<Matchable>();

        //scan through the entire grid
        //if a matchable can move, add it to the list of possible moves
        for (int y = 0; y != Dimensions.y; ++y)
            for (int x = 0; x != Dimensions.x; ++x)
                if (CheckBounds(x, y) && !IsEmpty(x, y) && CanMove(GetItemAt(x, y)))
                    possibleMoves.Add(GetItemAt(x, y));


        return possibleMoves.Count;
    }


    //check if this matchable can move to from a valid match
    private bool CanMove(Matchable toCheck)
    {
        if (
            CanMove(toCheck, Vector2Int.up) ||
            CanMove(toCheck, Vector2Int.down) ||
            CanMove(toCheck, Vector2Int.left) ||
            CanMove(toCheck, Vector2Int.right)
        )
            return true;

        if (toCheck.IsGem)
            return true;

        return false;
    }

    //can this matchable move in 1 direction?
    private bool CanMove(Matchable toCheck, Vector2Int direction)
    {
        //look 2 positions aways straight ahead
        Vector2Int position1 = toCheck.position + direction * 2,
                   position2 = toCheck.position + direction * 3;

        if (IsAPotentialMatch(toCheck, position1, position2))
            return true;

        //what is the clockwise direction?
        Vector2Int cw = new Vector2Int(direction.y, -direction.x),
                   ccw = new Vector2Int(-direction.y, direction.x);

        //look diagonally clockwise
        position1 = toCheck.position + direction + cw;
        position2 = toCheck.position + direction + cw * 2;

        if (IsAPotentialMatch(toCheck, position1, position2))
            return true;

        //look at both diagonals
        position2 = toCheck.position + direction + ccw;

        if (IsAPotentialMatch(toCheck, position1, position2))
            return true;

        //look diagonally counterclockwise
        position1 = toCheck.position + direction + ccw * 2;

        if (IsAPotentialMatch(toCheck, position1, position2))
            return true;

        return false;
    }

    //will these matchables from a potential match?
    private bool IsAPotentialMatch(Matchable toCompare, Vector2Int position1, Vector2Int position2)
    {
        if (
               CheckBounds(position1)                       && CheckBounds(position2)
            && !IsEmpty(position1)                          && !IsEmpty(position2)
            && GetItemAt(position1).Idle                    && GetItemAt(position2).Idle
            && GetItemAt(position1).Type == toCompare.Type  && GetItemAt(position2).Type == toCompare.Type
        )
            return true;

        return false;
    }

    //show a hint to the player
    public void ShowHint()
    {
        hint.Indicator(possibleMoves[Random.Range(0, possibleMoves.Count)].transform);
    }
}
