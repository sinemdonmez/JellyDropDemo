using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
//manages both individual tiles and the grid

public class TileManager : MonoBehaviour
{
    public GameObject singleTilePrefab;
    public GameObject twoVerticalPrefab;
    public GameObject twoHorizontalPrefab;
    public GameObject square2x2Prefab;
    public GameObject lShapePrefab;
    public GameObject lShapeMirroredPrefab;
    public GameObject mixedShapePrefab;
    public GameObject mixedShapeMirroredPrefab;
    public GameObject defaultTilePrefab;
    public GameObject TopUI; 
    private TextMeshProUGUI goalText;

    public static TileManager Instance; // Singleton reference




    public Transform gridParent;
    public Transform swipe;
    

    public int gridWidth;
    public int gridHeight;
    private float spacing = 3f; // Space between images
    private float imageSize = 30f; // Size of each square image
    public int remainingGoals;

    public GameObject celebrationEffect;
    public GameObject wellDone;
    public Button nextLevelButton;
    //private TextMeshProUGUI goalText;
    private int currentLevel = 1;
    private int maxLevels = 3;

    
    
    
    public UnityEngine.Vector3[,] positionGrid;
    private GameObject[,] backgroundTiles;

    
    public Grid grid;


    private Dictionary<TileType, GameObject> tilePrefabs;

    void Awake(){
        tilePrefabs = new Dictionary<TileType, GameObject>
        {
            { TileType.Single, singleTilePrefab },
            { TileType.TwoVertical, twoVerticalPrefab },
            { TileType.TwoHorizontal, twoHorizontalPrefab },
            { TileType.Square2x2, square2x2Prefab },
            { TileType.LShape, lShapePrefab },
            { TileType.LShapeMirrored, lShapeMirroredPrefab},
            { TileType.MixedShape, mixedShapePrefab},
            { TileType.MixedShapeMirrored, mixedShapeMirroredPrefab}
        };

        Instance = this;
        LeanTween.init(800);
        goalText = TopUI.GetComponentInChildren<TextMeshProUGUI>();

        if (nextLevelButton != null)
        {
            nextLevelButton.gameObject.SetActive(false);
            nextLevelButton.onClick.AddListener(LoadNextLevel);
        }
        
    }

    



    // private Vector3 CalculateGridStartPosition(){
    //     float totalWidth = gridWidth * cellSize + (gridWidth - 1) * spacing;
    //     float totalHeight = gridHeight * cellSize + (gridHeight - 1) * spacing;

    //     return new Vector3(-totalWidth / 2 + cellSize / 2, totalHeight / 2 - cellSize / 2, 0);
    // }

    public Tile SpawnInitialTile(int x, int y, TileType type, string[,] colors){
        if (!grid.CanPlaceTile(x, y))
        {
            Debug.LogWarning($"❗ Cannot place tile at ({x}, {y}) - Space is occupied!");
            return null;
        }

        // ✅ Step 1: Instantiate the tile
        GameObject tileObject = Instantiate(tilePrefabs[type]);

        if (gridParent != null)
        {
            tileObject.transform.SetParent(gridParent, false); // Set it as a child of the grid
        }
        else
        {
            Debug.LogError("TileManager: Grid parent not assigned!");
            return null;
        }

        // ✅ Step 2: Get RectTransform and set position
        RectTransform tileTransform = tileObject.GetComponent<RectTransform>();

        if (tileTransform != null)
        {
            // ✅ Step 3: Correct positioning inside the grid
            tileTransform.anchoredPosition = positionGrid[x, y];

            // ✅ Step 4: Scale the tile properly
            float scaleFactor = imageSize / 50f;
            tileTransform.localScale *= scaleFactor;
        }
        else
        {
            Debug.LogError("TileManager: Tile does not have a RectTransform!");
            return null;
        }

        // ✅ Step 5: Create tile logic object
        Tile tile = new Tile(type, colors);
        tile.SetUnityObject(tileObject);
        tile.UpdateUnityColors();

        Debug.Log($"✅ Spawned tile at {x}, {y}");

        // ✅ Step 6: Place tile inside the grid using the `Grid` class
        grid.PlaceTile(tile, x, y);

        return tile;
    }


private Tile currentMovableTile = null; // Stores the active moving tile

public Tile SpawnTileAtTheTop(TileType type, string[,] colors) {
    GameObject tileObject = Instantiate(tilePrefabs[type]);
    tileObject.transform.SetParent(swipe, false); 

    RectTransform tileTransform = tileObject.GetComponent<RectTransform>();
    float scaleFactor = imageSize / 50f;
    tileTransform.localScale *= scaleFactor;

    tileTransform.anchoredPosition = new Vector2(0, 0); // Centered at top

    Tile tile = new Tile(type, colors);
    tile.SetUnityObject(tileObject);
    tile.UpdateUnityColors();

    currentMovableTile = tile; // ✅ Set this as the single active tile

    return tile;
}

public Tile SpawnCreatedTileAtTheTop(Tile tile) {
    GameObject tileObject = Instantiate(tilePrefabs[tile.Type]);
    tileObject.transform.SetParent(swipe, false); 

    RectTransform tileTransform = tileObject.GetComponent<RectTransform>();
    float scaleFactor = imageSize / 50f;
    tileTransform.localScale *= scaleFactor;

    tileTransform.anchoredPosition = new Vector2(0, 0); // Centered at top

    
    tile.SetUnityObject(tileObject);
    tile.UpdateUnityColors();

    currentMovableTile = tile; // ✅ Set this as the single active tile

    return tile;
}

    // Returns the only movable tile
    public Tile GetCurrentMovableTile() {
        return currentMovableTile;
    }

    // When tile lands, clear the reference
    public void LockCurrentTile() {
        currentMovableTile = null;
    }

    public void UpdateGoalText(){
        if (goalText != null && remainingGoals>0)
        {
            goalText.text = $"{remainingGoals}";
        }else{
             goalText.text = $"{0}";
            StartCoroutine(HandleLevelCompletion());
        }
    }

    private IEnumerator HandleLevelCompletion(){
        Debug.Log("level done");
        if (celebrationEffect != null)
        {
            celebrationEffect.SetActive(true);
        }

        yield return new WaitForSeconds(2f);





        if (nextLevelButton != null && currentLevel < maxLevels)
        {
            wellDone.SetActive(false);
            nextLevelButton.gameObject.SetActive(true);
        }
    }

    public void LoadNextLevel(){
        if (currentLevel < maxLevels)
        {
            currentLevel++;
            wellDone.SetActive(true);
            nextLevelButton.gameObject.SetActive(false);
            celebrationEffect.SetActive(false);
            LevelLoader.Instance.LoadLevelByNumber(currentLevel);
        }
    }



    public void InitializeGrid(LevelData levelData){
        ClearExistingTiles();

        gridWidth = levelData.grid_size.width;
        gridHeight = levelData.grid_size.height;
        remainingGoals = levelData.goal_count;
        UpdateGoalText();

        //TODO: get rid of all tiles in the scene

        GameObject gridObject = new GameObject("GridManager"); // ✅ Create a GameObject for the grid
        grid = gridObject.AddComponent<Grid>(); // ✅ Attach the Grid script
        grid.Initialize(gridWidth, gridHeight); // ✅ Initialize with dynamic width & height


        //grid = new Grid(gridWidth, gridHeight); // ✅ Correctly initialize the grid instance

        int rows = gridHeight;
        int columns = gridWidth;

        Debug.Log($"Grid initialized with size: {gridWidth} x {gridHeight}");

        float totalWidth = columns * imageSize + (columns - 1) * spacing;
        float totalHeight = rows * imageSize + (rows - 1) * spacing;

        RectTransform rectTransform = gridParent.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(totalWidth, totalHeight);

        RectTransform swipeRect = swipe.GetComponent<RectTransform>();
        swipeRect.sizeDelta = new Vector2(totalWidth, swipeRect.sizeDelta.y);

        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;

        Vector2 startOffset = new Vector2(-(totalWidth) / 2 + imageSize / 2, (totalHeight) / 2 - imageSize / 2);

        positionGrid = new Vector3[gridWidth, gridHeight];
        backgroundTiles = new GameObject[gridWidth, gridHeight];

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Vector2 tilePosition = new Vector2(
                    startOffset.x + x * (imageSize + spacing),
                    startOffset.y - y * (imageSize + spacing)
                );

                positionGrid[x, y] = new Vector3(tilePosition.x, tilePosition.y, -3f);

                GameObject tileObject = Instantiate(defaultTilePrefab, gridParent);
                RectTransform tileRect = tileObject.GetComponent<RectTransform>();
                tileRect.anchoredPosition = tilePosition;
                tileRect.localScale = new Vector3(imageSize, imageSize, 1f);

                backgroundTiles[x, y] = tileObject;
            }
        }

        // ✅ Fill grid with starting tiles
        foreach (TileData tileData in levelData.starting_tiles)
        {
            SpawnInitialTile(tileData.x, tileData.y, System.Enum.Parse<TileType>(tileData.type), ConvertToGrid(tileData.colors));
        }

        SpawnTileAtTheTop(System.Enum.Parse<TileType>(levelData.next_tile.type), ConvertToGrid(levelData.next_tile.colors));
    }

    public void ClearExistingTiles(){
        // Clear all child objects in gridParent (removing all tiles)
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        // Clear all child objects in swipe (removing any swiping tile)
        foreach (Transform child in swipe)
        {
            Destroy(child.gameObject);
        }

        // Reset grid structure
        if (backgroundTiles != null)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (backgroundTiles[x, y] != null)
                    {
                        Destroy(backgroundTiles[x, y]);
                        backgroundTiles[x, y] = null;
                    }
                }
            }
        }

        // Reset grid references
        backgroundTiles = null;
        positionGrid = null;
    }



    string[,] ConvertToGrid(string[] colors){
        return new string[,] { { colors[0], colors[1] }, { colors[2], colors[3] } };
    }


    private int lastLitColumn = -1; // Stores the last lit column to track changes

    void Update()
    {
        if (swipe.childCount > 0) // Check if there is a tile in the swipe area
        {
            Transform tileTransform = swipe.GetChild(0); // Get the first tile (assuming only one is present)
            int closestColumn = GetClosestColumn(tileTransform.position.x);

            if (closestColumn != lastLitColumn) // Only update if the column changes
            {
                if (lastLitColumn != -1) // Reset only the previously lit column
                    ResetColumn(lastLitColumn);

                LightUpColumn(closestColumn);
                lastLitColumn = closestColumn; // Update last column reference
            }
        }
        else if (lastLitColumn != -1) // Reset only once when swipe is empty
        {
            ResetColumn(lastLitColumn);
            lastLitColumn = -1;
        }
    }

    public int GetClosestColumn(float tileXPosition)
    {
        float minDistance = Mathf.Infinity;
        int closestColumn = 0;

        for (int x = 0; x < gridWidth; x++)
        {
            // Get any tile from this column to check its X position
            for (int y = 0; y < gridHeight; y++)
            {
                if (backgroundTiles[x, y] != null) // Find the first valid tile in this column
                {
                    float columnX = backgroundTiles[x, y].transform.position.x; // Get tile X position
                    float distance = Mathf.Abs(tileXPosition - columnX);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestColumn = x;
                    }
                    break; // No need to check more tiles in this column
                }
            }
        }
        return closestColumn;
    }

    public void ResetColumn(int columnIndex)
    {
        if (backgroundTiles == null) return;

        Color defaultTileColor = new Color(56f / 255f, 56f / 255f, 56f / 255f); // #383838

        for (int y = 0; y < gridHeight; y++)
        {
            if (backgroundTiles[columnIndex, y] != null)
            {
                SpriteRenderer spriteRenderer = backgroundTiles[columnIndex, y].GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = defaultTileColor; // Reset to original color
                }
            }
        }
    }


    public void ResetColumnHighlights()
    {
        if (backgroundTiles == null) return;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (backgroundTiles[x, y] != null)
                {
                    backgroundTiles[x, y].GetComponent<UnityEngine.UI.Image>().color = Color.white;
                }
            }
        }
    }

    public Vector2 GetGridAnchoredPosition(int x, int y){
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            return positionGrid[x, y]; // UI-based position from the stored grid
        }

        Debug.LogError($"Invalid grid position requested: ({x}, {y})");
        return Vector2.zero;
    }



    public void LightUpColumn(int columnIndex)
    {
        if (backgroundTiles == null)
        {
            Debug.LogError("backgroundTiles array is null! Ensure InitializeGrid() is called.");
            return;
        }

        if (columnIndex < 0 || columnIndex >= backgroundTiles.GetLength(0)) return;

        Color lightUpColor = new Color(140f / 255f, 138f / 255f, 138f / 255f); // #8C8A8A

        for (int y = 0; y < backgroundTiles.GetLength(1); y++)
        {
            if (backgroundTiles[columnIndex, y] != null)
            {
                SpriteRenderer spriteRenderer = backgroundTiles[columnIndex, y].GetComponent<SpriteRenderer>();

                if (spriteRenderer != null)
                {
                    spriteRenderer.color = lightUpColor; // ✅ Apply light-up color
                }
                else
                {
                    Debug.LogError($"Tile at ({columnIndex}, {y}) does not have a SpriteRenderer component!");
                }
            }
            else
            {
                Debug.LogWarning($"Tile at ({columnIndex}, {y}) is null!");
            }
        }
    }


    public GameObject GetBackgroundTile(int x, int y)
    {
        // Check if the coordinates are within the grid bounds
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            return backgroundTiles[x, y]; // Return the tile at the specified position
        }

        Debug.LogWarning($"GetBackgroundTile: Invalid position ({x}, {y})");
        return null; // Return null if the position is out of bounds
    }

    // Returns the lowest available row in a given column
    public int GetLowestAvailableRow(int column)
    {
        for (int y = gridHeight - 1; y >= 0; y--)
        {
            if (grid.CanPlaceTile(column, y))
            {
                return y;
            }
        }
        return -1; // Column is full
    }

    // Converts a grid position (x, y) to world position
    public Vector3 GridToWorldPosition(int x, int y)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            return positionGrid[x, y]; // Return precomputed world position
        }
        Debug.LogError($"Invalid grid position: ({x}, {y})");
        return Vector3.zero;
    }

    // Places a tile at the given (x, y) position in the grid
    public void PlaceTileOnGrid(Tile tile, int x, int y)
{


    if (!grid.CanPlaceTile(x, y))
    {
        Debug.LogWarning($"❗ Cannot place tile at ({x}, {y}) - Space is occupied!");
        return;
    }

    grid.PlaceTile(tile, x, y);

}



    public void SpawnRandomTileAtTheTop()
    {
        // ✅ Get a random TileType from the dictionary
        TileType randomType = (TileType)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(TileType)).Length);

        Tile newTile = TileFactory.CreateTile(randomType);
        SpawnCreatedTileAtTheTop(newTile);
    }


}
