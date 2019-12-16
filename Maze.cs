using Node = Node<NodeData>;
class NodeData
{
    public Vector3 Position;
    public bool IsVisitedByAlgorithm;
    public NodeData(Vector3 position)
    {
        this.Position = position;
        this.IsVisitedByAlgorithm = false;
    }
}

/// <summary>
/// Wallprefab/Floorprefab size should be max 1x1x1
/// </summary>
public class Maze : MonoBehaviour
{
    //[Tooltip("Wherether the maze is generated")]
    [HideInInspector] public bool IsGenerated { get; private set; } = false;

    [Tooltip("List of possible walls, the maze will pick a random wall out of the list for each instance")]
    [SerializeField] private List<GameObject> _WallPrefabs = null;
    [Tooltip("List of possible floors, the maze will pick a random floor out of the list for each instance")]
    [SerializeField] private List<GameObject> _FloorPrefabs = null;
  
    private Node[,,] _Nodes; //Nodes of the maze
    private int _CurrentCoroutineId;
    private float _LastYieldReturnNullTime;

    /// <summary>
    /// Generates a random 3D width x height x depth maze centered around (0, 0, 0)
    /// </summary>
    public IEnumerator Generate(Vector3Int size)
    {
        StopAllCoroutines();

        ++this._CurrentCoroutineId;

        this.IsGenerated = false;

        ResetMaze();
        ConstructOuterWalls(size);
  
        yield return StartCoroutine(GenerateNodes(this._CurrentCoroutineId, size));
        yield return StartCoroutine(ConstructInnerWalls(this._CurrentCoroutineId));
    }

    /// <summary>
    /// Randomized Prim's algorithm from https://en.wikipedia.org/wiki/Maze_generation_algorithm
    /// Use the algorithm to construct the graph from which we can generate the maze
    /// A node represents a cell, a link represents a wall
    /// </summary>
    private IEnumerator GenerateNodes(int id, Vector3Int size)
    {
        //Wikipedia: Start with a grid full of walls.
        this._Nodes = new Node[size.x, size.y, size.z];
        for (int x = 0; x < size.x; ++x)
        {
            for (int y = 0; y < size.y; ++y)
            {
                for (int z = 0; z < size.z; ++z)
                {
                    this._Nodes[x, y, z] = new Node(new NodeData(new Vector3(
                        -(size.x - 1) / 2.0f + x,
                        -(size.y - 1) / 2.0f + y,
                        -(size.z - 1) / 2.0f + z)));
                    if (x != 0) new Node.Link(this._Nodes[x - 1, y, z], this._Nodes[x, y, z]);
                    if (y != 0) new Node.Link(this._Nodes[x, y - 1, z], this._Nodes[x, y, z]);
                    if (z != 0) new Node.Link(this._Nodes[x, y, z - 1], this._Nodes[x, y, z]);
                }
            }
        }

        //Use the System.Random because Unity's random does not allow threading
        System.Random random = new System.Random();
        foreach (Node node in this._Nodes)
        {
            if (node.Data.IsVisitedByAlgorithm == false)
            {
                //Wikipedia: Pick a cell, mark it as part of the maze. Add the walls of the cell to the wall list.
                node.Data.IsVisitedByAlgorithm = true;
                List<Node.Link> links = new List<Node.Link>();
                foreach (Node.Link link in node.Links)
                {
                    if (!links.Contains(link))
                    {
                        links.Add(link);
                    }
                }

                //Wikipedia: While there are walls in the list:
                while (links.Count != 0)
                {
                    //if (TargetFrameTime + (TargetFrameTime - DeltaTime) <= TimeInbetweenYieldReturnNulls), pause this coroutine and continue next frame
                    if (2.0f / (float)Application.targetFrameRate - Time.deltaTime <= Time.realtimeSinceStartup - this._LastYieldReturnNullTime)
                    {
                        this._LastYieldReturnNullTime = Time.realtimeSinceStartup;
                        yield return null;
                    }

                    //Wikipedia: Pick a random wall from the list. If only one of the two cells that the wall divides is visited, then:
                    Node.Link randomLink = links[random.Next(0, links.Count)];
                    if (randomLink.Node0.Data.IsVisitedByAlgorithm ^ randomLink.Node1.Data.IsVisitedByAlgorithm)
                    {
                        //Wikipedia: Make the wall a passage and mark the unvisited cell as part of the maze.
                        Node newNode = randomLink.Node0.Data.IsVisitedByAlgorithm ? randomLink.Node1 : randomLink.Node0;
                        randomLink.Unlink(); //Remove link represents deleting the wall
                        newNode.Data.IsVisitedByAlgorithm = true;

                        //Wikipedia: Add the neighboring walls of the cell to the wall list.
                        foreach (Node.Link link in newNode.Links)
                        {
                            if (!links.Contains(link))
                            {
                                links.Add(link);
                            }
                        }
                    }

                    //Wikipedia: Remove the wall from the list.
                    links.Remove(randomLink);
                }
            }
        }
    }

    /// <summary>
    /// Construct the outer walls of the maze
    /// </summary>
    private void ConstructOuterWalls(Vector3Int size)
    {
        for (int y = 0; y < size.y; ++y)
        {
            for (int x = 0; x < size.x; ++x)
            {
                InstantiateRandom(this._WallPrefabs, new Vector3(-(size.x - 1) / 2.0f + x, -(size.y - 1) / 2.0f + y, -size.z / 2.0f), Vector3.zero);
                InstantiateRandom(this._WallPrefabs, new Vector3(-(size.x - 1) / 2.0f + x, -(size.y - 1) / 2.0f + y, +size.z / 2.0f), Vector3.zero);
            }
            for (int z = 0; z < size.z; ++z)
            {
                InstantiateRandom(this._WallPrefabs, new Vector3(-size.x / 2.0f, -(size.y - 1) / 2.0f + y, -(size.z - 1) / 2.0f + z), new Vector3(0.0f, 90.0f, 0.0f));
                InstantiateRandom(this._WallPrefabs, new Vector3(+size.x / 2.0f, -(size.y - 1) / 2.0f + y, -(size.z - 1) / 2.0f + z), new Vector3(0.0f, 90.0f, 0.0f));
            }
        }
    }

    /// <summary>
    /// Construct the inner walls of the maze
    /// The coroutine stops itself when a new GenerateMazeNodesThread is started
    /// </summary>
    private IEnumerator ConstructInnerWalls(int id)
    {
        foreach (Node node in this._Nodes)
        {
            foreach (Node.Link link in node.Links)
            {
                //if another GenerateMazeNodes coroutine is running, break this coroutine
                if (this._CurrentCoroutineId != id) yield break;
                //if (TargetFrameTime + (TargetFrameTime - DeltaTime) <= TimeInbetweenYieldReturnNulls), pause this coroutine and continue next frame
                if (2.0f / (float)Application.targetFrameRate - Time.deltaTime <= Time.realtimeSinceStartup - this._LastYieldReturnNullTime)
                {
                    this._LastYieldReturnNullTime = Time.realtimeSinceStartup;
                    yield return null;
                }

                //Build the link
                if (link.Node0.Data.Position.x == link.Node1.Data.Position.x)
                {
                    if (link.Node0.Data.Position.z == link.Node1.Data.Position.z) //Floor
                    {
                         InstantiateRandom(this._FloorPrefabs, (link.Node0.Data.Position.y < link.Node1.Data.Position.y ? link.Node1.Data.Position : link.Node0.Data.Position) - new Vector3(0.0f, 0.5f, 0.0f), Vector3.zero);
                    }
                    else //Horizontal wall
                    {
                        InstantiateRandom(this._WallPrefabs, (link.Node0.Data.Position + link.Node1.Data.Position) / 2.0f, Vector3.zero);
                    }
                }
                else //Vertical wall
                {
                    InstantiateRandom(this._WallPrefabs, (link.Node0.Data.Position + link.Node1.Data.Position) / 2.0f, new Vector3(0.0f, 90.0f, 0.0f));
                }
            }
        
            //Remove links to avoid doubles
            Node.Link[] links = new Node.Link[node.Links.Count];
            node.Links.CopyTo(links);
            foreach (Node.Link link in links)
            {
                link.Unlink();
            }
        }

        //Let the maze know it is generated
        this.IsGenerated = true;
    }

    /// <summary>
    /// Lift walls in the air
    /// Attached rigidbodies to the walls
    /// </summary>
    public void DropWalls()
    {
        foreach (Transform child in this.transform)
        {
            child.position = new Vector3(child.position.x, child.position.y + 1.0f, child.position.z);
            child.gameObject.AddComponent<Rigidbody>();
        }
    }

    /// <summary>
    /// Resets the rotation and deletes all the walls
    /// </summary>
    private void ResetMaze()
    {
        this.transform.rotation = Quaternion.identity;
        foreach (Transform child in this.transform)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Picks a random wall prefab from the wall prefab list
    /// Sets its transform
    /// Makes it a child of the maze transform
    /// </summary>
    /// <param name="prefabs">List of prefabs where random one is picked from</param>
    /// <param name="position">Position of the prefab when spawned</param>
    /// <param name="rotation">Rotation of the prefab when spawned</param>
    private void InstantiateRandom(List<GameObject> prefabs, Vector3 position, Vector3 rotation)
    {
        GameObject gameObject = Instantiate(
            prefabs[UnityEngine.Random.Range(0, prefabs.Count)], //Pick random wall
            position,                                //Set position
            Quaternion.Euler(rotation),              //Set rotation
            this.transform);                         //Make child
    }
}
