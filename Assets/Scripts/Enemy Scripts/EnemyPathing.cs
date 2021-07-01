using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyPathing : MonoBehaviour
{
    //2D array representaion of path tiles
    private Vector2Int[,] path2DArray;

    //path root and end goal
    private Vector2Int pathRoot;
    private Vector2Int pathGoal;

    //non path node
    private Vector2Int nonPath = new Vector2Int(int.MaxValue, int.MaxValue);

    //tilemap and grid reference
    private Grid grid;
    private Tilemap tilemap;

    //best path
    private List<Vector3Int> bestPath;

    //bool for toggling showing the best path
    private bool toggleShowPath;

    // Start is called before the first frame update
    void Start()
    {
        grid = FindObjectOfType<Grid>();
        tilemap = FindObjectOfType<Tilemap>();

        toggleShowPath = true;
    }

    // Update is called once per frame
    void Update()
    {
        //get best path
        //if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    List<Vector3Int> newPath = FindBestPath(GetPathArray(), pathRoot, pathGoal);
        //    bestPath = FullArrayToTilePos(newPath);
        //}

        //show path
        //if (Input.GetKeyDown(KeyCode.W) && bestPath != null)
        //{
        //    ShowPath(bestPath, toggleShowPath);

        //    if (toggleShowPath)
        //    {
        //        toggleShowPath = false;
        //    }
        //    else
        //    {
        //        toggleShowPath = true;
        //    }
        //}
    }

    public List<Vector3Int> BestPath{ get{ return bestPath; } }

    //get a 2D array representation of all path tiles
    private Vector2Int[] GetPathArray()
    {
        path2DArray = new Vector2Int[tilemap.cellBounds.size.x, tilemap.cellBounds.size.y];

        int sizeOfPath = 0;

        for (int i = 0; i < path2DArray.GetLength(0); i++)
        {
            for (int j = 0; j < path2DArray.GetLength(1); j++)
            {
                switch (tilemap.GetTile(ArrayToTilePos(i, j)).name)
                {
                    //intermediate nodes
                    case "path":
                        path2DArray[i, j] = new Vector2Int(i, j);
                        sizeOfPath++;
                        //Debug.Log(i + "," + j);
                        break;

                    //root node
                    case "spawner":
                        pathRoot = new Vector2Int(i, j);
                        sizeOfPath++;
                        //Debug.Log(i + "," + j);
                        break;

                    //goal node
                    case "base":
                        pathGoal = new Vector2Int(i, j);
                        sizeOfPath++;
                        //Debug.Log(i + "," + j);
                        break;

                    //not nodes
                    default:
                        path2DArray[i, j] = nonPath;
                        break;
                }
            }
        }

        Vector2Int[] pathArray = new Vector2Int[sizeOfPath];
        int iterable = 0;

        foreach (Vector2Int coord in path2DArray)
        {
            if (coord != nonPath)
            {
                pathArray[iterable] = coord;
                iterable++;
            }
        }

        return pathArray;
    }

    //find the best path through the map using the breadth first search algorithm
    private List<Vector3Int> FindBestPath(Vector2Int[] maze, Vector2Int root, Vector2Int goal)
    {
        PathGraph pathGraph = new PathGraph(maze, root, goal);

        //create queue and set it up
        Queue<PathNode> pathQueue = new Queue<PathNode>();
        pathGraph.RootNode.Visited = true;
        pathQueue.Enqueue(pathGraph.RootNode);

        PathNode currentNode = new PathNode(0, 0);
        //Debug.Log("start");
        
        while (pathQueue.Count > 0)
        {
            currentNode = pathQueue.Dequeue();
            //Debug.Log(currentNode + " : " + currentNode.NeighborNodes.Count);
            //foreach (PathNode testNode in currentNode.NeighborNodes)
            //{
            //    Debug.Log(testNode.BriefToString());
            //}

            if (currentNode == pathGraph.EndNode)
            {
                //change PathNodes to Vector2Ints
                List<Vector3Int> vector3Path = new List<Vector3Int>();

                do
                {
                    vector3Path.Insert(0, new Vector3Int(currentNode.Position[0], currentNode.Position[1], 0));
                    currentNode = currentNode.Parent;
                }
                while (currentNode != null);

                return vector3Path;
            }

            foreach (PathNode neighbor in currentNode.NeighborNodes)
            {
                if (!neighbor.Visited)
                {
                    neighbor.Visited = true;
                    neighbor.Parent = currentNode;
                    pathQueue.Enqueue(neighbor);
                }
            }
        }

        Debug.Log("queue reached 0 (this should never happen)");
        return null;
    }

    //visualize chosen path
    private void ShowPath(List<Vector3Int> chosenPath, bool show)
    {
        Color c = Color.white;

        if (show)
        {
            c = Color.green;
        }

        foreach (Vector3Int pos in chosenPath)
        {
            tilemap.SetTileFlags(pos, TileFlags.None);
            tilemap.SetColor(pos, c);
        }
    }

    //change 2d array coords to align to tilemap grid
    private Vector3Int ArrayToTilePos(int x, int y)
    {
        return new Vector3Int(x - tilemap.cellBounds.size.x / 2, y - tilemap.cellBounds.size.y / 2, 0);
    }

    //change full 2d array coords to align to tilemap grid
    private List<Vector3Int> FullArrayToTilePos(List<Vector3Int> arrayPath)
    {
        for (int i = 0; i < arrayPath.Count; i++)
        {
            arrayPath[i] = ArrayToTilePos(arrayPath[i].x, arrayPath[i].y);
        }

        return arrayPath;
    }

    //set up the path from outside
    public List<Vector3Int> SetUpPath()
    {
        List<Vector3Int> newPath = FindBestPath(GetPathArray(), pathRoot, pathGoal);
        bestPath = FullArrayToTilePos(newPath);
        return bestPath;
    }

    //class to create a graph out nodes
    private class PathGraph
    {
        //graph that holds all path nodes
        private List<PathNode> graph;

        //root and goal node
        private PathNode rootNode;
        private PathNode endNode;

        //constructor that takes a 2d array of Vector2Ints and creates path nodes
        public PathGraph(Vector2Int[] pathCoords, Vector2Int root, Vector2Int end)
        {
            //set up graph
            graph = new List<PathNode>();

            //get root and end nodes
            graph.Add(new PathNode(root.x, root.y));
            rootNode = graph[graph.Count - 1];

            graph.Add(new PathNode(end.x, end.y));
            endNode = graph[graph.Count - 1];

            //Debug.Log("break");

            //create graph
            foreach (Vector2Int coord in pathCoords)
            {
                graph.Add(new PathNode(coord.x, coord.y));
                //Debug.Log(coord.x + ", " + coord.y);
            }

            //get all neighbors
            foreach (PathNode node in graph)
            {
                node.GetNeighbors(graph);
            }
        }

        //property for graph
        public List<PathNode> Graph
        {
            get { return graph; }
        }

        //property for root node
        public PathNode RootNode
        {
            get { return rootNode; }
        }

        //property for end node
        public PathNode EndNode
        {
            get { return endNode; }
        }
    }

    //class for nodes to make finding the best path easier
    private class PathNode
    {
        //position of node
        private int[] pos;

        //has it been visited
        private bool visited;

        //list of neighbors
        private List<PathNode> neighborNodes;

        //parent node of this node
        private PathNode parentNode;

        //constructor that takes a position
        public PathNode(int x, int y)
        {
            pos = new int[]{x, y};
            visited = false;
            neighborNodes = new List<PathNode>();
        }

        //property for position
        public int[] Position
        {
            get { return pos; }
        }

        //property for visited
        public bool Visited
        {
            get { return visited; }
            set { visited = value; }
        }

        //property for neighbors
        public List<PathNode> NeighborNodes
        {
            get { return neighborNodes; }
        }

        //property for parent
        public PathNode Parent
        {
            get { return parentNode; }
            set { parentNode = value; }
        }

        //check if a node neighbors this one
        public List<PathNode> GetNeighbors(List<PathNode> nodes)
        {
            List<PathNode> neighbors = new List<PathNode>();
            foreach (PathNode node in nodes)
            {
                //max of 8 neighbors
                //for (int i = -1; i <= 1; i++)
                //{
                //    for (int j = -1; j <= 1; j++)
                //    {
                //        if (Mathf.Abs(i + j) != 0 &&
                //            pos[0] + i == node.Position[0] &&
                //            pos[1] + j == node.Position[1]
                //        )
                //        {
                //            neighbors.Add(node);
                //        }
                //    }
                //}

                //max of 4 neighbors
                for (int i = -1; i <= 1; i++)
                {
                    if (i != 0 && 
                        pos[0] + i == node.Position[0] &&
                        pos[1] == node.Position[1]
                    )
                    {
                        neighbors.Add(node);
                    }
                }

                for (int j = -1; j <= 1; j++)
                {
                    if (j != 0 &&
                        pos[0] == node.Position[0] &&
                        pos[1] + j == node.Position[1]
                    )
                    {
                        neighbors.Add(node);
                    }
                }
            }

            neighborNodes = neighbors;
            return neighbors;
        }

        //print node as string
        public override string ToString()
        {
            return string.Format("node at ({0}, {1}) visited = {2}", pos[0], pos[1], visited);
        }

        //condensed version of ToString
        public string BriefToString()
        {
            return string.Format("({0}, {1}): {2}", pos[0], pos[1], visited);
        }
    }
}


