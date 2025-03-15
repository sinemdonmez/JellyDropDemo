using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

[System.Serializable]
public class LevelData
{
    public GridSize grid_size;
    public List<TileData> starting_tiles;
    public NextTile next_tile;
    public int goal_count;
}

[System.Serializable]
public class GridSize
{
    public int width;
    public int height;
}

[System.Serializable]
public class TileData
{
    public int x;
    public int y;
    public string type;
    public string[] colors;
}

[System.Serializable]
public class NextTile
{
    public string type;
    public string[] colors;
}

[System.Serializable]
public class Position
{
    public int x;
    public int y;
}

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance;
    public string[] levelFiles = { "level01.json", "level02.json", "level03.json" };
    private int currentLevelIndex = 0;
    [SerializeField] private TileManager tileManager;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        LoadLevelByNumber(1);
    }

    public void LoadLevelByNumber(int levelNumber)
    {
        if (levelNumber - 1 < levelFiles.Length)
        {
            currentLevelIndex = levelNumber - 1;
            LoadLevel(levelFiles[currentLevelIndex]);
        }
        else
        {
            Debug.LogError("No more levels available!");
        }
    }

    private void LoadLevel(string fileName)
    {
        string filePath = Path.Combine(Application.dataPath, "Levels", fileName);

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            LevelData levelData = JsonUtility.FromJson<LevelData>(json);
            tileManager.InitializeGrid(levelData);
        }
        else
        {
            Debug.LogError($"JSON file not found at {filePath}");
        }
    }

    
}
