using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;

// Enum for tile types including mirrored versions
public enum TileType{
    Single,
    TwoVertical,
    TwoHorizontal,
    Square2x2,
    LShape,
    LShapeMirrored,
    MixedShape,
    MixedShapeMirrored
}

// Represents a Tile with a 2x2 matrix of cubes (color-filled)
public class Tile{
    public TileType Type { get; private set; }
    public string[,] Grid { get; set; } // 2x2 array of colors
    public GameObject UnityObject { get; private set; } // Unity GameObject reference


    public Tile(TileType type, string[,] grid)
    {
        Type = type;
        Grid = grid;
    }

    public void SetUnityObject(GameObject obj)
    {
        UnityObject = obj;
    }


public void UpdateUnityColors()
{
    if (UnityObject == null) return;

    // Extract distinct colors while preserving order
    List<string> distinctColors = new List<string>();
    foreach (string color in Grid)
    {
        if (!distinctColors.Contains(color))
        {
            distinctColors.Add(color);
        }
    }

    // Check if children match the number of distinct colors
    if (UnityObject.transform.childCount != distinctColors.Count)
    {
        Debug.LogError("Mismatch: UnityObject children count does not match distinct colors count!");
        return;
    }

    int index = 0;
    foreach (Transform child in UnityObject.transform)
    {
        SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = GetColorFromString(distinctColors[index]);
            index++;
        }
    }
}

    public Color GetColorFromString(string colorName)
    {
        switch (colorName.ToLower())
        {
            case "red": return Color.red;
            case "yellow": return Color.yellow;
            case "blue": return Color.blue;
            case "green": return Color.green;
            case "purple": return new Color(0.5f, 0f, 0.5f);
            case "orange": return new Color(1f, 0.5f, 0f);
            default: return Color.white;
        }
    }

    public string GetColorNameFromColor(Color color)
    {
        if (color == Color.red) return "red";
        if (color == Color.yellow) return "yellow";
        if (color == Color.blue) return "blue";
        if (color == Color.green) return "green";
        if (color == new Color(0.5f, 0f, 0.5f)) return "purple"; // Custom purple
        if (color == new Color(1f, 0.5f, 0f)) return "orange"; // Custom orange

        return "unknown"; // Default case if the color isn't recognized
    }

}

// Factory class for creating different tile types
public static class TileFactory{
    private static System.Random rand = new System.Random();

    private static string RandomColor(List<string> excludeColors = null)
    {
        string[] colors = { "red", "blue", "yellow", "green", "purple", "orange" };
        List<string> availableColors = new List<string>(colors);

        // Remove excluded colors if provided
        if (excludeColors != null)
        {
            availableColors.RemoveAll(c => excludeColors.Contains(c));
        }

        // Pick a random color from the available set
        return availableColors[rand.Next(availableColors.Count)];
    }


    public static Tile CreateTile(TileType type){
        List<string> usedColors = new List<string>();

        string color1 = RandomColor();
        usedColors.Add(color1);

        string color2 = RandomColor(usedColors);
        usedColors.Add(color2);

        string color3 = type == TileType.Square2x2 || type == TileType.LShape || type == TileType.LShapeMirrored ||
                        type == TileType.MixedShape || type == TileType.MixedShapeMirrored
                        ? RandomColor(usedColors)
                        : color1;  // If the tile doesn't require a third color, reuse an existing one
        usedColors.Add(color3);

        string color4 = type == TileType.Square2x2
                        ? RandomColor(usedColors) // Only Square2x2 needs 4 unique colors
                        : color3;  // Otherwise, reuse existing colors

        switch (type){
            case TileType.Single:
                return new Tile(type, new string[,] {
                    { color1, color1 },
                    { color1, color1 }
                });

            case TileType.TwoVertical:
                return new Tile(type, new string[,] {
                    { color1, color2 },
                    { color1, color2 }
                });

            case TileType.TwoHorizontal:
                return new Tile(type, new string[,] {
                    { color1, color1 },
                    { color2, color2 }
                });

            case TileType.Square2x2:
                return new Tile(type, new string[,] {
                    { color1, color2 },
                    { color3, color4 }
                });

            case TileType.LShape:
                return new Tile(type, new string[,] {
                    { color1, color1 },
                    { color2, color3 }
                });

            case TileType.LShapeMirrored:
                return new Tile(type, new string[,] {
                    { color2, color3 },
                    { color1, color1 }
                });

            case TileType.MixedShape:
                return new Tile(type, new string[,] {
                    { color1, color2 },
                    { color1, color3 }
                });

            case TileType.MixedShapeMirrored:
                return new Tile(type, new string[,] {
                    { color2, color1 },
                    { color3, color1 }
                });

            default:
                throw new ArgumentException("Invalid tile type!");
        }
    }
}


public class Grid : MonoBehaviour{
    private int width;
    private int height;
    private Tile[,] grid; // Now storing Tile objects

    public void Initialize(int gridWidth, int gridHeight)
    {
        width = gridWidth;
        height = gridHeight;
        grid = new Tile[width, height];

    }


    // **Check if a tile can be placed at a given position**
    public bool CanPlaceTile(int x, int y){
        return x >= 0 && x < width && y >= 0 && y < height && grid[x, y] == null;
    }

    public void PlaceTile(Tile tile, int x, int y){
        if (!CanPlaceTile(x, y))
            throw new InvalidOperationException("Cannot place tile here!");

        grid[x, y] = tile; // Place the entire tile in ONE cell
    }



    public IEnumerator HandleTurn(){
        yield return StartCoroutine(MatchRoutine());
    }

    private IEnumerator MatchRoutine()
    {
        List<(int, int, int, int, string)> matchList = FindMatches();

        while (matchList.Count > 0)
        {

            List<(Tile, int, int)> tilesToProcess = new List<(Tile, int, int)>();
            List<IEnumerator> animationCoroutines = new List<IEnumerator>();

            // Step 1: Collect Animations
            foreach (var (tileX, tileY, _, _, matchColor) in matchList)
            {
                Tile tile = grid[tileX, tileY];
                if (tile == null) continue;
                animationCoroutines.Add(RunBoingAnimation(tile, matchColor));
                TileManager.Instance.remainingGoals--;
                TileManager.Instance.UpdateGoalText();
                tilesToProcess.Add((tile, tileX, tileY));
            }

            yield return StartCoroutine(RunMultipleCoroutines(animationCoroutines));

            // Step 2: Process Tiles After Animation
            List<IEnumerator> fillCoroutines = new List<IEnumerator>();
            foreach (var (tile, tileX, tileY) in tilesToProcess)
            {
                fillCoroutines.Add(FillorDeleteTile(tile, tileX, tileY));
            }
            yield return StartCoroutine(RunMultipleCoroutines(fillCoroutines));

            // Step 3: Drop Tiles in Parallel
            yield return StartCoroutine(DropTiles());

            // Step 4: Check for New Matches
            matchList = FindMatches();
        }
    }

    private IEnumerator RunMultipleCoroutines(List<IEnumerator> coroutines)
    {
        int coroutinesRunning = coroutines.Count;

        foreach (var coroutine in coroutines)
        {
            StartCoroutine(RunCoroutineAndTrackCompletion(coroutine, () => coroutinesRunning--));
        }

        yield return new WaitUntil(() => coroutinesRunning == 0);
    }

    private IEnumerator RunCoroutineAndTrackCompletion(IEnumerator coroutine, Action onComplete)
    {
        yield return StartCoroutine(coroutine);
        onComplete?.Invoke();
    }



    private IEnumerator RunBoingAnimation(Tile tile, string matchColor) {
        Transform tileTransform = tile.UnityObject.transform;
        Color targetColor = tile.GetColorFromString(matchColor);
        List<GameObject> matchedChildren = new List<GameObject>();

        foreach (Transform child in tileTransform) {
            SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.color == targetColor) {
                matchedChildren.Add(child.gameObject);
            }
        }

        if (matchedChildren.Count == 0) yield break;

        foreach (GameObject child in matchedChildren) {
            bool isCompleted = false;
            Vector3 originalScale = child.transform.localScale;
            Vector3 stretchX = new Vector3(originalScale.x * 1.2f, originalScale.y * 0.8f, originalScale.z);
            Vector3 stretchY = new Vector3(originalScale.x * 0.8f, originalScale.y * 1.2f, originalScale.z);
            Vector3 tinyScale = new Vector3(0.2f, 0.2f, 1f);

            LeanTween.scale(child, stretchX, 0.05f).setEase(LeanTweenType.easeOutQuad)
                .setOnComplete(() => {
                    LeanTween.scale(child, stretchY, 0.05f).setEase(LeanTweenType.easeOutQuad)
                        .setOnComplete(() => {
                            LeanTween.scale(child, originalScale * 1.1f, 0.05f).setEase(LeanTweenType.easeOutQuad)
                                .setOnComplete(() => {
                                    LeanTween.scale(child, tinyScale, 0.3f).setEase(LeanTweenType.easeInBack)
                                        .setOnComplete(() => {
                                            LeanTween.scale(child, Vector3.zero, 0.01f).setEase(LeanTweenType.easeInBack)
                                                .setOnComplete(() => {
                                                    isCompleted = true;
                                                    DestroyImmediate(child);
                                                });
                                        });
                                });
                        });
                });
            
            yield return new WaitUntil(() => isCompleted);
        }
    }




    private IEnumerator FillorDeleteTile(Tile tile, int tileX, int tileY)
    {
        if (tile.UnityObject == null) yield break;

        List<Transform> activeChildren = GetActiveChildren(tile.UnityObject.transform);
        int numChildren = activeChildren.Count;

        Debug.Log($"🔍 Tile ({tileX}, {tileY}) has {numChildren} active children.");

        if (numChildren == 0)
        {
            yield return StartCoroutine(DeleteTile(tile, tileX, tileY));
        }
        else if (numChildren == 1)
        {
            yield return StartCoroutine(ConvertToSingleBlock(tile, activeChildren[0]));
        }
        else if (numChildren == 2)
        {
            yield return StartCoroutine(HandleTwoChildren(tile, activeChildren));
        }
        else if (numChildren == 3)
        {
            yield return StartCoroutine(HandleThreeChildren(tile, activeChildren));
        }
    }


    /// Returns a list of active children for a given transform.
    private List<Transform> GetActiveChildren(Transform parent){
        List<Transform> children = new List<Transform>();
        foreach (Transform child in parent)
        {
            if (child.gameObject.activeSelf)
                children.Add(child);
        }
        return children;
    }

    /// Deletes a tile with an animation.
    private IEnumerator DeleteTile(Tile tile, int tileX, int tileY){
        if (tile == null || tile.UnityObject == null) yield break;

        grid[tileX, tileY] = null; // Remove reference from grid

        bool isAnimationComplete = false;

        LeanTween.scale(tile.UnityObject, Vector3.zero, 0.2f)
            .setEase(LeanTweenType.easeInBack)
            .setOnComplete(() =>
            {
                DestroyImmediate(tile.UnityObject);
                isAnimationComplete = true; // Mark animation as finished
            });

        // ✅ Wait for animation to finish before continuing
        yield return new WaitUntil(() => isAnimationComplete);
    }

private IEnumerator ConvertToSingleBlock(Tile tile, Transform child)
{
    if (tile == null || child == null) yield break;

    SpriteRenderer sprite = child.GetComponent<SpriteRenderer>();
    if (sprite == null) yield break;

    RectTransform parentRect = tile.UnityObject.GetComponent<RectTransform>();
    if (parentRect == null) yield break;

    float parentSize = parentRect.sizeDelta.x;
    Vector3 newScale = new Vector3(parentSize, parentSize, 1f);

    bool isAnimationComplete = false;

    LeanTween.moveLocal(child.gameObject, Vector3.zero, 0.2f)
        .setEase(LeanTweenType.easeOutQuad);

    LeanTween.scale(child.gameObject, newScale, 0.2f)
        .setEase(LeanTweenType.easeOutElastic)
        .setOnComplete(() => isAnimationComplete = true);

    // ✅ Wait until animation completes
    yield return new WaitUntil(() => isAnimationComplete);

    tile.Grid = new string[2, 2]
    {
        { tile.GetColorNameFromColor(sprite.color), tile.GetColorNameFromColor(sprite.color) },
        { tile.GetColorNameFromColor(sprite.color), tile.GetColorNameFromColor(sprite.color) }
    };
}


private IEnumerator HandleTwoChildren(Tile tile, List<Transform> children)
{
    if (tile == null || children == null || children.Count != 2) yield break;

    Transform firstChild = children[0];
    Transform secondChild = children[1];

    // Handle mirrored versions
    if (tile.Type == TileType.MixedShapeMirrored || tile.Type == TileType.LShapeMirrored)
    {
        firstChild = children[1];
        secondChild = children[0];
    }

    SpriteRenderer firstSprite = firstChild.GetComponent<SpriteRenderer>();
    SpriteRenderer secondSprite = secondChild.GetComponent<SpriteRenderer>();
    if (firstSprite == null || secondSprite == null) yield break;

    RectTransform parentRect = tile.UnityObject.GetComponent<RectTransform>();
    if (parentRect == null) yield break;

    float parentWidth = parentRect.sizeDelta.x;
    float parentHeight = parentRect.sizeDelta.y;

    Vector3 pos1 = firstChild.localPosition;
    Vector3 pos2 = secondChild.localPosition;

    bool isVertical = Mathf.Approximately(pos1.x, pos2.x); // If X positions are the same, it's vertical.
    bool isSameSize = Mathf.Approximately(firstSprite.bounds.size.y, secondSprite.bounds.size.y) &&
                      Mathf.Approximately(firstSprite.bounds.size.x, secondSprite.bounds.size.x);

    bool isAnimationComplete = false;

    if (isSameSize)
    {
        // ✅ Keep children centered and aligned properly
        float yOffset = isVertical ? parentHeight / 4f : 0;
        float xOffset = isVertical ? 0 : parentWidth / 4f;

        if (isVertical)
        {
            // ✅ Vertical - Keep original X, adjust Y only
            firstChild.localPosition = new Vector3(0, yOffset, 0);
            secondChild.localPosition = new Vector3(0, -yOffset, 0);
        }
        else
        {
            // ✅ Horizontal - Keep original Y, adjust X only
            firstChild.localPosition = new Vector3(-xOffset, 0, 0);
            secondChild.localPosition = new Vector3(xOffset, 0, 0);
        }

        // ✅ Expand children properly
        Vector3 newScale = isVertical
            ? new Vector3(parentWidth, parentHeight / 2f, 1f)  // Expand height for vertical stack
            : new Vector3(parentWidth / 2f, parentHeight, 1f); // Expand width for horizontal stack

        // Animate the first and second child
        LeanTween.scale(firstChild.gameObject, newScale, 0.2f)
            .setEase(LeanTweenType.easeOutElastic);

        LeanTween.scale(secondChild.gameObject, newScale, 0.2f)
            .setEase(LeanTweenType.easeOutElastic)
            .setOnComplete(() => isAnimationComplete = true);

        // ✅ Wait for animation to complete
        yield return new WaitUntil(() => isAnimationComplete);

        // ✅ Update tile grid colors
        UpdateGridFromTile(tile, isVertical);
    }
    else
    {
        // ✅ One child is shorter, extend it to match the larger child
        Transform smallerChild, largerChild;
        SpriteRenderer smallerSprite, largerSprite;

        if (firstChild.GetComponent<SpriteRenderer>().bounds.size.y < secondChild.GetComponent<SpriteRenderer>().bounds.size.y ||
            firstChild.GetComponent<SpriteRenderer>().bounds.size.x < secondChild.GetComponent<SpriteRenderer>().bounds.size.x)
        {
            smallerChild = firstChild;
            largerChild = secondChild;
        }
        else
        {
            smallerChild = secondChild;
            largerChild = firstChild;
        }

        smallerSprite = smallerChild.GetComponent<SpriteRenderer>();
        largerSprite = largerChild.GetComponent<SpriteRenderer>();

        if (smallerSprite != null && largerSprite != null)
        {
            // ✅ Determine which dimension increased (width or height)
            bool widthExpanded = firstSprite.bounds.size.x != secondSprite.bounds.size.x;
            bool heightExpanded = firstSprite.bounds.size.y != secondSprite.bounds.size.y;

            // ✅ Get the final width & height from the larger child
            Vector3 newScale = new Vector3(largerChild.localScale.x, largerChild.localScale.y, 1f);
            Vector3 newPosition = smallerChild.localPosition;

            if (widthExpanded)
            {
                // ✅ Width increased → Align X center, keep Y position fixed
                newPosition.y = smallerChild.localPosition.y;
                newPosition.x = largerChild.localPosition.x;
                UpdateGridFromTile(tile, true);
            }
            else if (heightExpanded)
            {
                // ✅ Height increased → Align Y center, keep X position fixed
                if (smallerChild.localPosition.y > largerChild.localPosition.y)
                {
                    UpdateGridFromTile(tile, false);
                }
                else
                {
                    UpdateGridFromTile(tile, false);
                }

                newPosition.x = smallerChild.localPosition.x;
                newPosition.y = largerChild.localPosition.y;
            }

            isAnimationComplete = false;

            // ✅ Apply transformations
            smallerChild.localPosition = newPosition;
            LeanTween.scale(smallerChild.gameObject, newScale, 0.2f)
                .setEase(LeanTweenType.easeOutElastic)
                .setOnComplete(() => isAnimationComplete = true);

            // ✅ Wait for animation to complete
            yield return new WaitUntil(() => isAnimationComplete);
        }
    }
}

    public void UpdateGridFromTile(Tile tile, bool isVertical)
    {
        if (tile == null || tile.UnityObject == null)
        {
            Debug.LogError("UpdateGridFromTile: Tile or UnityObject is null!");
            return;
        }

        Transform tileTransform = tile.UnityObject.transform;
        List<SpriteRenderer> childrenSprites = new List<SpriteRenderer>();

        foreach (Transform child in tileTransform)
        {
            SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                childrenSprites.Add(spriteRenderer);
            }
        }

        if (childrenSprites.Count < 2)
        {
            Debug.LogWarning("UpdateGridFromTile: Not enough children to update the grid.");
            return;
        }

        // Sort children by position (helps align colors properly)
        //childrenSprites = childrenSprites.OrderBy(s => isVertical ? s.transform.localPosition.y : s.transform.localPosition.x).ToList();

        string color1 = tile.GetColorNameFromColor(childrenSprites[0].color);
        string color2 = tile.GetColorNameFromColor(childrenSprites[1].color);

        if (isVertical)
        {
            // Vertical placement
            tile.Grid = new string[2, 2]
            {
                { color1, color1 },
                { color2, color2 }
            };
        }
        else
        {
            // Horizontal placement
            tile.Grid = new string[2, 2]
            {
                { color1, color2 },
                { color1, color2 }
            };
        }

        Debug.Log($"✅ Grid Updated for Tile:\n[{tile.Grid[0, 0]}, {tile.Grid[0, 1]}]\n[{tile.Grid[1, 0]}, {tile.Grid[1, 1]}]");
    }







    /// Updates the tile grid with correct colors after transformation.
    private void UpdateTileGrid(Tile tile, Color color1, Color color2, bool isVertical){
        string colorName1 = tile.GetColorNameFromColor(color1);
        string colorName2 = tile.GetColorNameFromColor(color2);

        if (isVertical)
        {
            tile.Grid = new string[2, 2]
            {
                { colorName1, colorName1 },
                { colorName2, colorName2 }
            };
        }
        else
        {
            tile.Grid = new string[2, 2]
            {
                { colorName1, colorName2 },
                { colorName1, colorName2 }
            };
        }

        Debug.Log($"✅ Updated Grid:\n[{tile.Grid[0, 0]}, {tile.Grid[0, 1]}]\n[{tile.Grid[1, 0]}, {tile.Grid[1, 1]}]");
    }




private IEnumerator HandleThreeChildren(Tile tile, List<Transform> children)
{
    if (tile == null || children == null || children.Count != 3) yield break;

    Transform oddChild = FindOddChild(children);
    Transform alignedChild1, alignedChild2;

    if (oddChild == children[0])
    {
        alignedChild1 = children[1];
        alignedChild2 = children[2];
    }
    else if (oddChild == children[1])
    {
        alignedChild1 = children[0];
        alignedChild2 = children[2];
    }
    else
    {
        alignedChild1 = children[0];
        alignedChild2 = children[1];
    }

    RectTransform parentRect = tile.UnityObject.GetComponent<RectTransform>();
    if (parentRect == null) yield break;

    float parentWidth = parentRect.sizeDelta.x;
    float parentHeight = parentRect.sizeDelta.y;

    bool isAnimationComplete = false;

    // ✅ Keep odd child's original Y but align X to parent
    Vector3 newOddPosition = new Vector3(0, oddChild.localPosition.y, 0);
    Vector3 newOddScale = new Vector3(parentWidth, oddChild.localScale.y, 1f); // ✅ Match width, keep height

    // ✅ Animate only the odd child
    LeanTween.moveLocal(oddChild.gameObject, newOddPosition, 0.2f)
        .setEase(LeanTweenType.easeOutQuad);

    LeanTween.scale(oddChild.gameObject, newOddScale, 0.2f)
        .setEase(LeanTweenType.easeOutElastic)
        .setOnComplete(() => isAnimationComplete = true);

    // ✅ Wait for animation to complete
    yield return new WaitUntil(() => isAnimationComplete);

    // ✅ Update the tile's grid
    UpdateTileGrid4Three(tile, oddChild, alignedChild1, alignedChild2);
}



    private Transform FindOddChild(List<Transform> children){
        Vector3 pos1 = children[0].localPosition;
        Vector3 pos2 = children[1].localPosition;
        Vector3 pos3 = children[2].localPosition;

        if (Mathf.Approximately(pos1.y, pos2.y)) return children[2];
        if (Mathf.Approximately(pos1.y, pos3.y)) return children[1];
        return children[0];
    }
    
    
    private void UpdateTileGrid4Three(Tile tile, Transform oddChild, Transform alignedChild1, Transform alignedChild2){
        SpriteRenderer oddSprite = oddChild.GetComponent<SpriteRenderer>();
        SpriteRenderer alignedSprite1 = alignedChild1.GetComponent<SpriteRenderer>();
        SpriteRenderer alignedSprite2 = alignedChild2.GetComponent<SpriteRenderer>();

        string oddColor = tile.GetColorNameFromColor(oddSprite.color);
        string alignedColor1 = tile.GetColorNameFromColor(alignedSprite1.color);
        string alignedColor2 = tile.GetColorNameFromColor(alignedSprite2.color);

        // ✅ Set grid based on whether the odd child is the first in the list
        if (tile.UnityObject.transform.GetChild(0) == oddChild)
        {
            tile.Grid = new string[2, 2]
            {
                { oddColor, oddColor },
                { alignedColor1, alignedColor2 }
            };
        }
        else
        {
            tile.Grid = new string[2, 2]
            {
                { alignedColor1, alignedColor2 },
                { oddColor, oddColor }
            };
        }

        Debug.Log($"✅ Updated Grid:\n[{tile.Grid[0, 0]}, {tile.Grid[0, 1]}]\n[{tile.Grid[1, 0]}, {tile.Grid[1, 1]}]");
    }

    private void RemoveMatchedChildren(Tile tile, string matchColor) {
        Transform tileTransform = tile.UnityObject.transform;

        Color targetColor = tile.GetColorFromString(matchColor);

        foreach (Transform child in tileTransform){
            SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.color == targetColor)
            {

                UnityEngine.Object.DestroyImmediate(child.gameObject);
                break; 
            }
        }
    }

    public List<(int, int, int, int, string)> FindMatches(){
        (int, int)[] directions = { (1, 0), (0, -1) }; // ✅ Only check Right and Above

        List<(int, int, int, int, string)> matches = new List<(int, int, int, int, string)>(); // ✅ Now includes color

        for (int y = height - 1; y >= 0; y--) // ✅ Start from bottom row (y=height-1) to top (y=0)
        {
            for (int x = 0; x < width; x++) // ✅ Scan left to right
            {
                Tile tile = grid[x, y];
                if (tile == null) continue; // Skip empty tiles

                foreach (var (dx, dy) in directions)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height || grid[nx, ny] == null)
                        continue; // Skip out-of-bounds or empty tiles

                    Tile neighborTile = grid[nx, ny];

                    // ✅ Check Right (x+1, y)
                    if (dx == 1)
                    {
                        if (tile.Grid[0, 1] == neighborTile.Grid[0, 0])
                        {
                            string matchColor = tile.Grid[0, 1];
                            matches.Add((x, y, 0, 1, matchColor));
                            matches.Add((nx, ny, 0, 0, matchColor));
                        }

                        if (tile.Grid[1, 1] == neighborTile.Grid[1, 0])
                        {
                            string matchColor = tile.Grid[1, 1];
                            matches.Add((x, y, 1, 1, matchColor));
                            matches.Add((nx, ny, 1, 0, matchColor));
                        }
                    }

                    // ✅ Check Above (x, y-1)
                    if (dy == -1)
                    {
                        if (tile.Grid[0, 0] == neighborTile.Grid[1, 0]){
                            string matchColor = tile.Grid[0, 0];
                            matches.Add((x, y, 0, 0, matchColor));
                            matches.Add((nx, ny, 1, 0, matchColor));
                        }

                        if (tile.Grid[0, 1] == neighborTile.Grid[1, 1]){
                            string matchColor = tile.Grid[0, 1];
                            matches.Add((x, y, 0, 1, matchColor));
                            matches.Add((nx, ny, 1, 1, matchColor));
                        }
                    }
                }
            }
        }

            // ✅ Filter to keep only unique matches, but allow multiple colors per tile
    Dictionary<(int, int, string), (int, int, int, string)> uniqueMatches = new Dictionary<(int, int, string), (int, int, int, string)>();

    foreach (var (tileX, tileY, cellX, cellY, matchColor) in matches) {
        if (!uniqueMatches.ContainsKey((tileX, tileY, matchColor))) {
            uniqueMatches[(tileX, tileY, matchColor)] = (cellX, cellY, tileX, matchColor);
        }
    }

    // ✅ Convert dictionary back to list
    List<(int, int, int, int, string)> filteredMatches = new List<(int, int, int, int, string)>();

    foreach (var kvp in uniqueMatches) {
        var (tileX, tileY, color) = kvp.Key;
        var (cellX, cellY, _, matchColor) = kvp.Value;
        filteredMatches.Add((tileX, tileY, cellX, cellY, matchColor));
    }

    Debug.Log($"🔍 Filtered Match List Count: {filteredMatches.Count}");
    foreach (var match in filteredMatches) {
        Debug.Log($"✅ Unique Match: Tile ({match.Item1}, {match.Item2}), Cell ({match.Item3}, {match.Item4}), Color: {match.Item5}");
    }

    return filteredMatches;

    }

// private IEnumerator DropTiles() {
//     List<Coroutine> activeCoroutines = new List<Coroutine>();
//     Dictionary<Tile, bool> tileDropStatus = new Dictionary<Tile, bool>();

//     for (int x = 0; x < width; x++) {
//         for (int y = 0; y < height - 1; y++) {
//             if (grid[x, y] != null) {
//                 int dropY = y;

//                 while (dropY + 1 < height && grid[x, dropY + 1] == null) {
//                     dropY++;
//                 }

//                 if (dropY != y) { // If the tile needs to move
//                     Tile tileToMove = grid[x, y];
//                     if (tileToMove == null || tileToMove.UnityObject == null) continue;

//                     RectTransform tileTransform = tileToMove.UnityObject.GetComponent<RectTransform>();
//                     if (tileTransform == null) continue;

//                     Vector2 targetPos = TileManager.Instance.positionGrid[x, dropY];
//                     grid[x, dropY] = tileToMove;
//                     grid[x, y] = null;

//                     // Track tile animation completion
//                     tileDropStatus[tileToMove] = false;

//                     // Start LeanTween animation
//                     LeanTween.moveLocal(tileTransform.gameObject, targetPos, 0.2f)
//                         .setEase(LeanTweenType.easeOutBounce)
//                         .setOnComplete(() => {
//                             Debug.Log($"✅ Tile ({x}, {dropY}) moved successfully!");
//                             tileDropStatus[tileToMove] = true; // Mark animation as done
//                         });
//                 }
//             }
//         }
//     }

//     // ✅ Wait until ALL tile movements finish
//     yield return new WaitUntil(() => tileDropStatus.Values.All(done => done));
// }

private IEnumerator DropTiles() {
    bool tilesMoved;

    do {
        tilesMoved = false; // Reset flag

        Dictionary<Tile, bool> tileDropStatus = new Dictionary<Tile, bool>();

        for (int x = 0; x < width; x++) {
            for (int y = height - 2; y >= 0; y--) { // Start from second-to-last row
                if (grid[x, y] != null) {
                    int dropY = y;

                    while (dropY + 1 < height && grid[x, dropY + 1] == null) {
                        dropY++;
                    }

                    if (dropY != y) { // If the tile needs to move
                        Tile tileToMove = grid[x, y];
                        if (tileToMove == null || tileToMove.UnityObject == null) continue;

                        RectTransform tileTransform = tileToMove.UnityObject.GetComponent<RectTransform>();
                        if (tileTransform == null) continue;

                        Vector2 targetPos = TileManager.Instance.positionGrid[x, dropY];
                        grid[x, dropY] = tileToMove;
                        grid[x, y] = null;

                        // Track tile animation completion
                        tileDropStatus[tileToMove] = false;

                        // Start LeanTween animation
                        LeanTween.moveLocal(tileTransform.gameObject, targetPos, 0.2f)
                            .setEase(LeanTweenType.easeOutBounce)
                            .setOnComplete(() => {
                                Debug.Log($"✅ Tile ({x}, {dropY}) moved successfully!");
                                tileDropStatus[tileToMove] = true; // Mark animation as done
                            });

                        tilesMoved = true; // ✅ Mark that movement happened
                    }
                }
            }
        }

        // ✅ Wait for all tile movements to finish before looping
        yield return new WaitUntil(() => tileDropStatus.Values.All(done => done));

    } while (tilesMoved); // 🔁 Repeat until no more drops are possible
}









public void PrintGrid()
{
    Debug.Log("=== GRID STATE ===");

    for (int y = 0; y <height; y++) // Print from top to bottom
    {
        string row1 = "";
        string row2 = "";

        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] != null)
            {
                Tile tile = grid[x, y];

                // Ensure the tile's internal grid is properly accessed
                string color1 = tile.Grid[0, 0].PadRight(6);
                string color2 = tile.Grid[0, 1].PadRight(6);
                string color3 = tile.Grid[1, 0].PadRight(6);
                string color4 = tile.Grid[1, 1].PadRight(6);

                row1 += $"[{color1} {color2}]  "; // Top row of the tile
                row2 += $"[{color3} {color4}]  "; // Bottom row of the tile
            }
            else
            {
                row1 += "[ .    . ]  "; // Empty space for top row
                row2 += "[ .    . ]  "; // Empty space for bottom row
            }
        }

        Debug.Log(row1);
        Debug.Log(row2);
        Debug.Log(""); // Extra space between rows
    }

    Debug.Log("===================");
}



}
