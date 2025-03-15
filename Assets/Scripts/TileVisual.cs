using UnityEngine;

public class TileVisual : MonoBehaviour
{
    //TODO: bunu nası yapçam tam çözemedim
    public SpriteRenderer[] colorBlocks; // Assign in Unity Inspector

    public void SetColors(string[,] grid)
    {
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                int index = i * 2 + j; // Convert 2D array to index
                if (index < colorBlocks.Length)
                {
                    colorBlocks[index].color = GetColorFromString(grid[i, j]);
                }
            }
        }
    }

    private Color GetColorFromString(string colorName)
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
}
