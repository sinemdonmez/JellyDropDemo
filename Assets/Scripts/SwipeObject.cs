using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class SwipeObject : MonoBehaviour
{
    private Vector2 startTouchPos;
    private bool isDragging = false;
    private float minX, maxX;
    public float swipeMultiplier = 1f;
    public float fallDuration = 0.5f;
    
    private int lastLitColumn = -1; // Stores the last lit column
    private bool isFalling = false;
    public static SwipeObject ActiveInstance; // Singleton instance



     void Awake()
    {
        // ✅ If an older instance exists, disable it
        if (ActiveInstance != null && ActiveInstance != this)
        {
            Debug.LogWarning($"🚨 Disabling previous SwipeObject: {ActiveInstance.gameObject.name}");
            ActiveInstance.enabled = false; // Disable the old instance
        }

        // ✅ Set this as the new active instance
        ActiveInstance = this;
        Debug.Log($"✅ New active SwipeObject: {gameObject.name}");
    }

    void Start()
    {
        RectTransform parentRect = transform.parent.GetComponent<RectTransform>();
        float parentWidth = parentRect.rect.width;
        RectTransform objectRect = GetComponent<RectTransform>();
        float objectWidth = objectRect.rect.width * transform.localScale.x;

        minX = transform.parent.position.x - parentWidth / 2 + objectWidth / 2;
        maxX = transform.parent.position.x + parentWidth / 2 - objectWidth / 2;
    }

 void LateUpdate()
{
    if (isFalling) return; // ✅ Block any input while falling

    if (Input.GetMouseButtonDown(0))
    {
        startTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        isDragging = true;
    }

    if (Input.GetMouseButton(0) && isDragging)
    {
        Vector2 touchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        // Moderate swipe movement scaling (not too fast, not too slow)
        float deltaX = (touchPosition.x - startTouchPos.x) * swipeMultiplier * 1.5f; // Fine-tuned multiplier

        // Prevent very tiny movements from causing jitter
        if (Mathf.Abs(deltaX) < 0.001f) deltaX = 0;

        Tile movableTile = TileManager.Instance.GetCurrentMovableTile();
        if (movableTile == null) return;

        Transform tileTransform = movableTile.UnityObject.transform;

        // Apply movement smoothly but with enough responsiveness
        float targetX = tileTransform.position.x + deltaX;

        tileTransform.position = new Vector3(
            Mathf.Lerp(tileTransform.position.x, targetX, 0.3f), // Smooth movement, but responsive
            tileTransform.position.y,
            tileTransform.position.z
        );

        // Ensure object stays within bounds
        tileTransform.position = new Vector3(
            Mathf.Clamp(tileTransform.position.x, minX, maxX),
            tileTransform.position.y,
            tileTransform.position.z
        );

        startTouchPos = touchPosition;
        HandleColumnLighting(tileTransform.position.x);
    }





    if (Input.GetMouseButtonUp(0) && isDragging)
    {
        isDragging = false;

        if (!isFalling) // ✅ Ensure DropTile() is only called once
        {
            DropTile();
        }

    }
}


    // ✅ Column lighting logic moved here
    void HandleColumnLighting(float tileXPosition)
    {
        int closestColumn = TileManager.Instance.GetClosestColumn(tileXPosition);

        if (closestColumn != lastLitColumn) // Only update if the column changes
        {
            if (lastLitColumn != -1)
                ResetColumn(lastLitColumn);

            LightUpColumn(closestColumn);
            lastLitColumn = closestColumn;
        }
    }


// void DropTile()
// {
//     if (isFalling) 
//     {
//         Debug.LogWarning("🚨 DropTile() blocked because isFalling is true");
//         return;
//     }

//     Debug.Log($"✅ DropTile() started - Stack Trace: {System.Environment.StackTrace}");
//     Debug.Log($"🔍 Update() running on: {gameObject.name} at Frame {Time.frameCount}");



//     isFalling = true; // ✅ Set this immediately

//     Debug.Log("✅ DropTile() started");

//     Tile movableTile = TileManager.Instance.GetCurrentMovableTile();
//     if (movableTile == null)
//     {
//         Debug.LogWarning("🚨 DropTile() exited - No movable tile");
//         isFalling = false;
//         return;
//     }

//     Transform tileTransform = movableTile.UnityObject.transform;
//     tileTransform.SetParent(TileManager.Instance.gridParent, true);

//     RectTransform tileRect = movableTile.UnityObject.GetComponent<RectTransform>();
//     Vector3 worldPos = tileRect.position;

//     int closestColumn = TileManager.Instance.GetClosestColumn(worldPos.x);
//     int lowestY = TileManager.Instance.GetLowestAvailableRow(closestColumn);

//     if (lowestY == -1)
//     {
//         Debug.LogError($"🚨 No available row in column {closestColumn}");
//         isFalling = false;
//         return;
//     }

//     Vector2 targetAnchoredPosition = TileManager.Instance.GetGridAnchoredPosition(closestColumn, lowestY);
//     Debug.Log($"📌 Dropping Tile to Column {closestColumn}, Row {lowestY}, Target Pos: {targetAnchoredPosition}");

//     LeanTween.cancel(tileRect.gameObject);

//     LeanTween.value(tileRect.gameObject, tileRect.anchoredPosition, targetAnchoredPosition, 0.5f)
//         .setEase(LeanTweenType.easeInQuad)
//         .setOnUpdate((Vector2 val) => {
//             tileRect.anchoredPosition = val;
//         })
//         .setOnComplete(() => {
//             if (!isFalling) return; // ✅ Ensures the function runs only once
//             Debug.Log("✅ LeanTween animation completed. Now placing tile.");

//             TileManager.Instance.LockCurrentTile();
//             TileManager.Instance.PlaceTileOnGrid(movableTile, closestColumn, lowestY);
//             //Debug.Log($"✅ Placed tile at ({closestColumn}, {lowestY})");

//             TileManager.Instance.SpawnRandomTileAtTheTop();

//             TileManager.Instance.grid.MatchRoutine();


//             isFalling = false; // ✅ Reset flag only after full completion
//         });
// }

void DropTile()
{
    if (isFalling) 
    {
        Debug.LogWarning("🚨 DropTile() blocked because isFalling is true");
        return;
    }

    isFalling = true; // ✅ Set this immediately

    Tile movableTile = TileManager.Instance.GetCurrentMovableTile();
    if (movableTile == null)
    {
        Debug.LogWarning("🚨 DropTile() exited - No movable tile");
        isFalling = false;
        return;
    }

    RectTransform tileRect = movableTile.UnityObject.GetComponent<RectTransform>();

    // ✅ Set parent first to ensure correct coordinate space
    tileRect.SetParent(TileManager.Instance.gridParent, true);

    // ✅ Find closest column
    Vector3 worldPos = tileRect.position;

    int closestColumn = TileManager.Instance.GetClosestColumn(worldPos.x);

    // ✅ Get the exact UI-based X position using GetGridAnchoredPosition()
    Vector2 anchoredTargetPosition = TileManager.Instance.GetGridAnchoredPosition(closestColumn, 0);

    // ✅ Keep the current Y position, only update X
    Vector2 finalAnchoredPosition = new Vector2(anchoredTargetPosition.x, tileRect.anchoredPosition.y);

    Debug.Log($"📌 Aligning Tile to column {closestColumn} at UI X: {anchoredTargetPosition.x}");

    // ✅ Smoothly move the tile using UI-based anchored position
    LeanTween.value(tileRect.gameObject, tileRect.anchoredPosition, finalAnchoredPosition, 0.05f)
        .setEase(LeanTweenType.easeOutQuad)
        .setOnUpdate((Vector2 val) => {
            tileRect.anchoredPosition = val;
        })
        .setOnComplete(() =>
        {
            // ✅ Once aligned, proceed with the drop
            PerformTileDrop(movableTile, closestColumn);
        });
}

// ✅ Handles actual tile drop after alignment
void PerformTileDrop(Tile movableTile, int closestColumn)
{
    RectTransform tileRect = movableTile.UnityObject.GetComponent<RectTransform>();

    int lowestY = TileManager.Instance.GetLowestAvailableRow(closestColumn);

    if (lowestY == -1)
    {
        Debug.LogError($"🚨 No available row in column {closestColumn}");
        isFalling = false;
        return;
    }

    Vector2 targetAnchoredPosition = TileManager.Instance.GetGridAnchoredPosition(closestColumn, lowestY);
    Debug.Log($"📌 Dropping Tile to Column {closestColumn}, Row {lowestY}, Target Pos: {targetAnchoredPosition}");

    LeanTween.cancel(tileRect.gameObject);

    LeanTween.value(tileRect.gameObject, tileRect.anchoredPosition, targetAnchoredPosition, 0.25f)
        .setEase(LeanTweenType.easeInQuad)
        .setOnUpdate((Vector2 val) => {
            tileRect.anchoredPosition = val;
        })
        .setOnComplete(() => {
            if (!isFalling) return; // ✅ Ensures the function runs only once
            Debug.Log("✅ LeanTween animation completed. Now placing tile.");

            TileManager.Instance.LockCurrentTile();
            TileManager.Instance.PlaceTileOnGrid(movableTile, closestColumn, lowestY);
            TileManager.Instance.SpawnRandomTileAtTheTop();

            StartCoroutine(TileManager.Instance.grid.HandleTurn());

            isFalling = false; // ✅ Reset flag only after full completion
        });
}











    void ResetColumn(int columnIndex)
    {
        TileManager.Instance.ResetColumn(columnIndex);
    }

    void LightUpColumn(int columnIndex)
    {
        TileManager.Instance.LightUpColumn(columnIndex);
    }
}
