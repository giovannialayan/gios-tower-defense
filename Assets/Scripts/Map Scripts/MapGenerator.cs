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

    //gamemanager
    public GameManager gameManager;

    //wave manager
    public WaveManager waveManager;

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
        if (Input.GetKeyDown(KeyCode.R))
        {
            //BinaryTreeMazeGen();

            DepthFirstSearchMazeGen();
        }
    }

    //change pathMapPercent based on text input value
    private void ChangeMapSize(string size)
    {

    }

    private Vector3Int[,] GeneratePath(Bounds bounds) //implement maze generation algorithm to generate map shape
    {
        //https://www.gamasutra.com/blogs/HermanTulleken/20161005/282629/Algorithms_for_making_more_interesting_mazes.php
        //make a class for nodes where each node is either a wall or a room
        //paths contain a list of adjacent walls and walls contain a list of adjacent rooms
        //mark all walls as closed
        //select a random room, add it to the final path
        //add that room's walls to the full wall list
        //while the wall list is not empty:
        //  select a wall randomly
        //  get rooms adjacent to that wall
        //  if there are two rooms and one isnt in the final path:
        //    mark the wall as open
        //    add the room to the final path
        //    add that room's adjacent walls to the wall list
        //  remove the wall that was marked open from the wall list

        return new Vector3Int[1, 1];
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
                    GameObject newSpawner = Instantiate(spawnerObj, map.GetCellCenterWorld(tilePos), Quaternion.identity);
                    newSpawner.GetComponent<SpawnManager>().gameManager = gameManager;
                    break;

                case 0:
                    map.SetTile(tilePos, wallTile);
                    break;

                case 1:
                    map.SetTile(tilePos, pathTile);
                    break;

                case 2:
                    map.SetTile(tilePos, baseTile);
                    GameObject newBase = Instantiate(baseObj, map.GetCellCenterWorld(tilePos), Quaternion.identity);
                    gameManager.BaseManager = newBase.GetComponent<BaseManager>();
                    break;
            }
        }
    }

    //replace all tiles with walls
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

    //replace all tiles with that of a scheme
    private void SetMazeScheme(Tilemap map, string[,] scheme)
    {
        List<Vector3Int> allTiles = new List<Vector3Int>();
        int tileCount = 0;

        for (int i = 0; i < tilemap.localBounds.size.x; i++)
        {
            for (int j = 0; j < tilemap.localBounds.size.y; j++)
            {
                allTiles.Add(ArrayToTilePos(i, j, map));
            }
        }

        for (int i = 0; i < scheme.GetLength(0); i++)
        {
            for (int j = 0; j < scheme.GetLength(1); j++)
            {
                switch (scheme[i, j])
                {
                    case "wall":
                        map.SetTile(allTiles[tileCount], wallTile);
                        break;

                    case "path":
                        map.SetTile(allTiles[tileCount], pathTile);
                        break;
                }

                tileCount++;
            }
        }
    }

    private string[,] InitializeScheme(Bounds bounds)
    {
        string[,] scheme = new string[(int)tilemap.localBounds.size.x, (int)tilemap.localBounds.size.y];

        for (int i = 0; i < bounds.size.x; i++)
        {
            for (int j = 0; j < bounds.size.y; j++)
            {
                //if (i == 0 || j == 0 || i == bounds.size.x - 1 || j == bounds.size.y - 1)
                //{
                //    scheme[i, j] = "wall";
                //}
                if (i % 2 == 0 || j % 2 == 0)
                {
                    scheme[i, j] = "wall";
                }
                else
                {
                    scheme[i, j] = "path";
                }
            }
        }

        return scheme;
    }

    //change 2d array coords to align to tilemap grid
    private Vector3Int ArrayToTilePos(int x, int y, Tilemap map)
    {
        return new Vector3Int(x - tilemap.cellBounds.size.x / 2, y - tilemap.cellBounds.size.y / 2, 0);
    }

    //update loading bar
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
    public void BinaryTreeMazeGen()
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

        GameObject newSpawner = Instantiate(spawnerObj, spawnerPos, Quaternion.identity);
        newSpawner.GetComponent<SpawnManager>().gameManager = gameManager;
        waveManager.SpawnMan = newSpawner.GetComponent<SpawnManager>();

        GameObject newBase = Instantiate(baseObj, basePos, Quaternion.identity);
        gameManager.BaseManager = newBase.GetComponent<BaseManager>();
    }

    //maze generation algorithm using depth first search
    public void DepthFirstSearchMazeGen()
    {
        ResetMap(tilemap, wallTile);

        Vector2Int startNode = new Vector2Int(1, 1);

        int startEdge = Random.Range(1, 5);

        switch (startEdge)
        {
            //north edge
            case 1:
                //Debug.Log("north edge start");
                //only get odd numbers in desired range
                int xNPos = Random.Range(1, tilemap.cellBounds.size.x - 1);
                if (xNPos % 2 == 0)
                {
                    xNPos++;
                    if (xNPos > tilemap.cellBounds.size.x)
                    {
                        xNPos = 1;
                    }
                }
                startNode = new Vector2Int(xNPos, tilemap.cellBounds.size.y - 2);
                break;

            //west edge
            case 2:
                //Debug.Log("west edge start");
                int yWPos = Random.Range(1, tilemap.cellBounds.size.y - 1);
                if (yWPos % 2 == 0)
                {
                    yWPos++;
                    if (yWPos > tilemap.cellBounds.size.y)
                    {
                        yWPos = 1;
                    }
                }
                startNode = new Vector2Int(1, yWPos);
                break;

            //south edge
            case 3:
                //Debug.Log("south edge start");
                int xSPos = Random.Range(1, tilemap.cellBounds.size.x - 1);
                if (xSPos % 2 == 0)
                {
                    xSPos++;
                    if (xSPos > tilemap.cellBounds.size.x)
                    {
                        xSPos = 1;
                    }
                }
                startNode = new Vector2Int(xSPos, 1);
                break;

            //east edge
            case 4:
                //Debug.Log("east edge start");
                int yEPos = Random.Range(1, tilemap.cellBounds.size.y - 1);
                if (yEPos % 2 == 0)
                {
                    yEPos++;
                    if (yEPos > tilemap.cellBounds.size.y)
                    {
                        yEPos = 1;
                    }
                }
                startNode = new Vector2Int(tilemap.cellBounds.size.x - 2, yEPos);
                break;

            default:
                break;
        }

        //create stack for depth first search and add starting node
        Stack<Vector2Int> nodes = new Stack<Vector2Int>();
        List<Vector2Int> maze = new List<Vector2Int>();
        List<Vector2Int> visited = new List<Vector2Int>();
        List<Vector2Int> mainRiver = new List<Vector2Int>();
        nodes.Push(startNode);
        visited.Add(startNode);
        Vector2Int lastNode = new Vector2Int(tilemap.cellBounds.size.x - 2, tilemap.cellBounds.size.y - 2);

        //while there are nodes in the stack continue creating the maze
        while (nodes.Count > 0)
        {
            //better way
            //pop top node
            maze.Add(nodes.Pop());
            int currentIndex = maze.Count - 1;

            if (mainRiver.Contains(maze[currentIndex]))
            {
                lastNode = maze[currentIndex];
            }

            //Debug.Log(maze[currentIndex]);

            //get valid neighbors
            List<Vector2Int> neighbors = new List<Vector2Int>();

            for (int i = -2; i <= 2; i += 2)
            {
                for (int j = -2; j <= 2; j += 2)
                {
                    //Debug.Log(maze[currentIndex].x + i + " , " + (maze[currentIndex].y + j));
                    if (
                        maze[currentIndex].x + i > 0 &&
                        maze[currentIndex].y + j > 0 &&
                        maze[currentIndex].x + i < tilemap.cellBounds.size.x &&
                        maze[currentIndex].y + j < tilemap.cellBounds.size.y &&
                        Mathf.Abs(i) != Mathf.Abs(j)
                    )
                    {
                        Vector2Int neighbor = new Vector2Int(maze[currentIndex].x + i, maze[currentIndex].y + j);
                        //Debug.Log(neighbor);

                        if (!visited.Contains(neighbor))
                        {
                            neighbors.Add(neighbor);
                            visited.Add(neighbor);
                            if (j == 0)
                            {
                                maze.Add(new Vector2Int(maze[currentIndex].x + i / 2, maze[currentIndex].y + j));
                            }
                            else if (i == 0)
                            {
                                maze.Add(new Vector2Int(maze[currentIndex].x + i, maze[currentIndex].y + j / 2));
                            }
                        }
                    }
                }
            }

            if (neighbors.Count > 0)
            {
                int randomTop = Random.Range(0, neighbors.Count);

                for (int i = 0; i < neighbors.Count; i++)
                {
                    if (i != randomTop)
                    {
                        nodes.Push(neighbors[i]);
                    }
                }

                nodes.Push(neighbors[randomTop]);
                mainRiver.Add(neighbors[randomTop]);
            }
        }

        foreach (Vector2Int node in maze)
        {
            tilemap.SetTile(ArrayToTilePos(node.x, node.y, tilemap), pathTile);

            if (node == startNode)
            {
                tilemap.SetTile(ArrayToTilePos(node.x, node.y, tilemap), spawnerTile);
            }
            else if (node == lastNode)
            {
                tilemap.SetTile(ArrayToTilePos(node.x, node.y, tilemap), baseTile);
            }
        }

        CreateMapStructures(startNode, lastNode);
    }

    //instantiate base and sawner objects on the map
    private void CreateMapStructures(Vector2Int spawnerCell, Vector2Int baseCell)
    {
        Vector3 spawnerPos = tilemap.CellToWorld(ArrayToTilePos(spawnerCell.x, spawnerCell.y, tilemap)) + new Vector3(.5f, .5f, 0);
        Vector3 basePos = tilemap.CellToWorld(ArrayToTilePos(baseCell.x, baseCell.y, tilemap)) + new Vector3(.5f, .5f, 0);

        GameObject newSpawner = Instantiate(spawnerObj, spawnerPos, Quaternion.identity);
        newSpawner.GetComponent<SpawnManager>().gameManager = gameManager;
        waveManager.SpawnMan = newSpawner.GetComponent<SpawnManager>();

        GameObject newBase = Instantiate(baseObj, basePos, Quaternion.identity);
        gameManager.BaseManager = newBase.GetComponent<BaseManager>();
    }
}
