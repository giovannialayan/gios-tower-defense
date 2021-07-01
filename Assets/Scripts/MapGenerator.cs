using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    private Tilemap tilemap;

    //tile references
    public TileBase pathTile;
    public TileBase wallTile;
    public TileBase spawnerTile;
    public TileBase baseTile;

    //object references
    public GameObject spawnerObj;
    public GameObject baseObj;

    //input to change pathMapPercent at runtime
    public InputField textField;

    //loading bar variables
    //public GameObject loadingBarParent;
    //private Image loadingBarBackground;
    //private Image loadingBarImage;
    //private Text loadingBarText;
    //private float barFullWidth;
    //private float barHeight;

    //maze variables
    //private string[,] mazeScheme;

    // Start is called before the first frame update
    void Start()
    {
        tilemap = transform.GetComponent<Tilemap>();

        //mazeScheme = InitializeScheme(tilemap.localBounds);

        //loadingBarBackground = loadingBarParent.transform.GetChild(0).GetComponent<Image>();
        //loadingBarImage = loadingBarParent.transform.GetChild(1).GetComponent<Image>();
        //loadingBarText = loadingBarParent.transform.GetChild(2).GetComponent<Text>();

        //barFullWidth = loadingBarBackground.rectTransform.rect.width;
        //barHeight = loadingBarBackground.rectTransform.rect.height;
    }

    // Update is called once per frame
    void Update()
    {
        textField.onEndEdit.AddListener(ChangeMapSize);

        if (Input.GetKeyDown(KeyCode.R))
        {
            BinaryTreeMazeGen(tilemap);

            //SetMazeScheme(tilemap, mazeScheme);
        }
    }

    //change pathMapPercent based on text input value
    private void ChangeMapSize(string size)
    {

    }

    //set tiles in tilemap to match the generated path
    private void PlaceTiles(Tilemap map, Vector3Int[,] tilePositions)
    {
        foreach (Vector3Int pos in tilePositions)
        {
            Vector3Int tilePos = ArrayToTilePos(pos.x, pos.y, map);

            switch (pos.z)
            {
                case -1:
                    map.SetTile(tilePos, spawnerTile);
                    Instantiate(spawnerObj, map.GetCellCenterWorld(tilePos), Quaternion.identity);
                    break;

                case 0:
                    map.SetTile(tilePos, wallTile);
                    break;

                case 1:
                    map.SetTile(tilePos, pathTile);
                    break;

                case 2:
                    map.SetTile(tilePos, baseTile);
                    Instantiate(baseObj, map.GetCellCenterWorld(tilePos), Quaternion.identity);
                    break;
            }
        }
    }

    //replace all tiles with fillTile and delete everything
    private void ResetMap(Tilemap map, TileBase fillTile)
    {
        List<Vector3Int> allTiles = new List<Vector3Int>();

        for (int i = 0; i < tilemap.localBounds.size.x; i++)
        {
            for (int j = 0; j < tilemap.localBounds.size.y; j++)
            {
                allTiles.Add(ArrayToTilePos(i, j, map));
            }
        }

        foreach (Vector3Int pos in allTiles)
        {
            map.SetTile(pos, fillTile);
        }

        //delete spawner
        if (GameObject.FindGameObjectWithTag("spawner") != null)
        {
            Destroy(GameObject.FindGameObjectWithTag("spawner"));
        }

        //delete base
        if (GameObject.FindGameObjectWithTag("base") != null)
        {
            Destroy(GameObject.FindGameObjectWithTag("base"));
        }

        //delete all placed structures
        if (GameObject.FindGameObjectWithTag("structure") != null)
        {
            GameObject[] structures = GameObject.FindGameObjectsWithTag("structure");

            for (int i = 0; i < structures.Length; i++)
            {
                Destroy(structures[i]);
            }
        }

        //delete all enemies
        if (GameObject.FindGameObjectWithTag("enemy") != null)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("enemy");

            for (int i = 0; i < enemies.Length; i++)
            {
                Destroy(enemies[i]);
            }
        }
    }

    //consider using schemes to enhance maze generation
    //replace all tiles with that of a scheme
    // private void SetMazeScheme(Tilemap map, string[,] scheme)
    // {
    //     List<Vector3Int> allTiles = new List<Vector3Int>();
    //     int tileCount = 0;

    //     for (int i = 0; i < tilemap.localBounds.size.x; i++)
    //     {
    //         for (int j = 0; j < tilemap.localBounds.size.y; j++)
    //         {
    //             allTiles.Add(ArrayToTilePos(i, j, map));
    //         }
    //     }

    //     for (int i = 0; i < scheme.GetLength(0); i++)
    //     {
    //         for (int j = 0; j < scheme.GetLength(1); j++)
    //         {
    //             switch (scheme[i, j])
    //             {
    //                 case "wall":
    //                     map.SetTile(allTiles[tileCount], wallTile);
    //                     break;

    //                 case "path":
    //                     map.SetTile(allTiles[tileCount], pathTile);
    //                     break;
    //             }

    //             tileCount++;
    //         }
    //     }
    // }

    // private string[,] InitializeScheme(Bounds bounds)
    // {
    //     string[,] scheme = new string[(int)tilemap.localBounds.size.x, (int)tilemap.localBounds.size.y];

    //     for (int i = 0; i < bounds.size.x; i++)
    //     {
    //         for (int j = 0; j < bounds.size.y; j++)
    //         {
    //             //if (i == 0 || j == 0 || i == bounds.size.x - 1 || j == bounds.size.y - 1)
    //             //{
    //             //    scheme[i, j] = "wall";
    //             //}
    //             if (i % 2 == 0 || j % 2 == 0)
    //             {
    //                 scheme[i, j] = "wall";
    //             }
    //             else
    //             {
    //                 scheme[i, j] = "path";
    //             }
    //         }
    //     }

    //     return scheme;
    // }

    //change 2d array coords to align to tilemap grid
    private Vector3Int ArrayToTilePos(int x, int y, Tilemap map)
    {
        return new Vector3Int(x - tilemap.cellBounds.size.x / 2, y - tilemap.cellBounds.size.y / 2, 0);
    }

    //update loading bar
    //learn how to use async to do this
    /*
    //private void UpdateLoadingBar(float current, float max)
    //{
    //    if (current > max)
    //    {
    //        current = max;
    //    }

    //    //change bar
    //    loadingBarImage.rectTransform.sizeDelta = new Vector2(current / max * barFullWidth, barHeight);

    //    //change text
    //    loadingBarText.text = ((int)(current / max * 100)).ToString();
    //}

    //toggle whether loading bar should be visible
    //private void ToggleLoadingBar(bool state)
    //{
    //    loadingBarParent.SetActive(state);
    //}
    */

    //maze generation algorithm using binary tree
    private void BinaryTreeMazeGen(Tilemap tilemap)
    {
        //set all tiles to wall first
        ResetMap(tilemap, wallTile);

        //start and end variables for the tilemap for the loops
        int cellStartX = 1;
        int cellStartY = 1;
        int cellEndX = tilemap.cellBounds.size.x - 1;
        int cellEndY = tilemap.cellBounds.size.y - 1;

        //loop through every other tile and choose whether to connect the current tile to the one two tiles below or two tiles to the right
        for (int i = cellStartX; i < cellEndX; i += 2)
        {
            for (int j = cellStartY; j < cellEndY; j += 2)
            {
                tilemap.SetTile(ArrayToTilePos(i, j, tilemap), pathTile);

                //Debug.Log(string.Format("x: {0} = {1} , y: {2} = {3}", i, ArrayToTilePos(i, j, tilemap).x, j, ArrayToTilePos(i, j, tilemap).y));

                //only choose between the down and right if it is posible to
                if (i == cellStartX && j != cellStartY)
                {
                    tilemap.SetTile(ArrayToTilePos(i, j - 1, tilemap), pathTile);
                }
                else if (j == cellStartY && i != cellStartX)
                {
                    tilemap.SetTile(ArrayToTilePos(i - 1, j, tilemap), pathTile);
                }
                else if(i != cellStartX && j != cellStartY)
                {
                    //choose whether to go down or to the right
                    if (Random.Range(0f, 1f) > .5f)
                    {
                        tilemap.SetTile(ArrayToTilePos(i - 1, j, tilemap), pathTile);
                    }
                    else
                    {
                        tilemap.SetTile(ArrayToTilePos(i, j - 1, tilemap), pathTile);
                    }
                }
            }
        }

        //place spawner and base tiles in opposite corners of the map
        tilemap.SetTile(ArrayToTilePos(1, 1, tilemap), spawnerTile);
        tilemap.SetTile(ArrayToTilePos(tilemap.cellBounds.size.x - 2, tilemap.cellBounds.size.y - 2, tilemap), baseTile);

        //flip tilemap vertically because tilemaps go from bottom left to top right and i want it from top left to bottom right
        for (int i = 0; i < tilemap.cellBounds.size.x; i++)
        {
            for (int j = 0; j < tilemap.cellBounds.size.y / 2; j++)
            {
                TileBase temp = tilemap.GetTile(ArrayToTilePos(i, tilemap.cellBounds.size.y - j - 1, tilemap));
                tilemap.SetTile(ArrayToTilePos(i, tilemap.cellBounds.size.y - j - 1, tilemap), tilemap.GetTile(ArrayToTilePos(i, j, tilemap)));
                tilemap.SetTile(ArrayToTilePos(i, j, tilemap), temp);
            }
        }

        //instantiate base and sawner objects on the map
        Vector3 spawnerPos = tilemap.CellToWorld(ArrayToTilePos(1, tilemap.cellBounds.size.y - 2, tilemap)) + new Vector3(.5f, .5f, 0);
        Vector3 basePos = tilemap.CellToWorld(ArrayToTilePos(tilemap.cellBounds.size.x - 2, tilemap.cellBounds.size.y - tilemap.cellBounds.size.y + 1, tilemap)) + new Vector3(.5f, .5f, 0);
        Instantiate(spawnerObj, spawnerPos, Quaternion.identity);
        Instantiate(baseObj, basePos, Quaternion.identity);
    }
}
