using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    // private Grid grid;
    // private int width;
    // private int height;
    // private TileManager tileManager;

    // public GameLogic(int width, int height){
    //     // this.width = width;
    //     // this.height = height;
    //     // grid = new Grid(width, height);
    //     // tileManager = GameObject.FindFirstObjectByType<TileManager>();

    // }

    // void Start(){
    //     // Debug.Log("Game started!");
    //     // grid = new Grid(width, height);
    //     // tileManager = FindObjectOfType<TileManager>();

    //     //StartCoroutine(GameLoop());
    // }

    // IEnumerator GameLoop()
    // {
    //     for (int i = 0; i < 5; i++) // Simulate 5 turns for testing
    //     {
    //         Debug.Log($"Turn {i + 1}");
    //         UpdateGame();
    //         yield return new WaitForSeconds(1f); // Wait between moves
    //     }

    //     grid.FindMatches();
    //     grid.PrintGrid();
    // }



    // // public void PlayerMove(Tile newTile, int column){

    // //     // Start from the bottom row and find the lowest available position
    // //     int lowestY = 0; // Start at row 0

    // //     for (int y = 0; y < height; y++){
    // //         if (grid.CanPlaceTile(column, y)) // Found an occupied tile
    // //         {
    // //             lowestY = y;
    // //             break; // Stop at the first unoccupied row
    // //         }
    // //         // Keep updating until we find a unfilled row
    // //     }

    // //     if (grid.CanPlaceTile(column, lowestY)){
    // //         grid.PlaceTile(newTile, column, lowestY);
    // //         Debug.Log($"Placed {newTile.Type} at ({column}, {lowestY})");
    // //     }
    // //     else
    // //     {
    // //         Debug.Log($"Column {column} is full! Cannot place tile.");
    // //     }

    // //     // Check for matches after placing the tile
    // //     //grid.RemoveMatches();
    // // }

    // // // **Run one game loop step**
    // // public void UpdateGame(){
    // //     Debug.Log("Before Move:");
    // //     grid.PrintGrid();

    // //     // //spawned new tile
    // //     // Tile newTile = TileFactory.CreateTile((TileType)new Random().Next(0, 8));

    // //     // // Simulate a random column move
    // //     // int column = new Random().Next(0, width - 1);
    // //      //TODO: get this from the swipe.
    // //      //TODO fix tf is going on here
    // //     TileType newTileType = (TileType)UnityEngine.Random.Range(0, 5);
    // //     int column = UnityEngine.Random.Range(0, width);
    // //     string[,] colors = { { "red", "yellow" }, { "red", "yellow" } };
    // //     Tile newTile = tileManager.SpawnTile(column, lowestY, newTileType, colors);

    // //     PlayerMove(newTile, column);

    // //     Debug.Log("After Move:");
    // //     grid.PrintGrid();
    // // }

    // public void UpdateGame()
    // {
    //     // Debug.Log("Spawning new tile...");

    //     // // Generate a random tile
    //     // TileType newTileType = (TileType)UnityEngine.Random.Range(0, 5);
    //     // int column = UnityEngine.Random.Range(0, width);
    //     // string[,] colors = { { "red", "yellow" }, { "red", "yellow" } };

    //     // Tile newTile = tileManager.SpawnTile(column, 0, newTileType, colors);

    //     // if (newTile != null)
    //     // {
    //     //     PlayerMove(newTile, column);
    //     // }
    //     // else
    //     // {
    //     //     Debug.LogError("Failed to spawn tile!");
    //     // }
    // }

    // public void PlayerMove(Tile newTile, int column)
    // {
    //     int lowestY = 0;

    //     for (int y = 0; y < height; y++)
    //     {
    //         if (grid.CanPlaceTile(column, y))
    //         {
    //             lowestY = y;
    //             break;
    //         }
    //     }

    //     if (grid.CanPlaceTile(column, lowestY))
    //     {
    //         grid.PlaceTile(newTile, column, lowestY);
    //         Debug.Log($"Placed {newTile.Type} at ({column}, {lowestY})");
    //     }
    //     else
    //     {
    //         Debug.Log($"Column {column} is full! Cannot place tile.");
    //     }
    // }


    // // **Start the game loop**
    // public void StartGame(){
    //     for (int i = 0; i < 5; i++) // Simulate 5 turns for testing
    //     {
    //         Debug.Log($"Turn {i + 1}");
    //         UpdateGame();
    //     }

    //     //grid.RemoveMatches();

    //     grid.FindMatches();
    //     grid.PrintGrid();
    // }
}
