public class Game : MonoBehaviour
{
    //[Tooltip("Wherether the maze is showing to top side or the bottom side of a 3D ?x2x? maze")]
    public bool IsFlipped { get; private set; } = false;

    [Tooltip("Slider to change the width of the maze")]
    [SerializeField] private Slider _SliderX = null;
    [Tooltip("Slider to change the depth of the maze")]
    [SerializeField] private Slider _SliderY = null;

    //GameObjects
    private Maze _Maze = null;
    private Player _Player = null;
    private Goal _Goal = null;
    private GameCamera _GameCamera = null;

    //GameVariables
    private Vector2Int _MazeSize = new Vector2Int(1, 1); //Size of the maze
    private bool _Is2D = true; //Wherether the maze is a 2D or a 3D maze

    private void Awake()
    {
        //Is used to guide coroutine yields
        Application.targetFrameRate = 30;

        this._Maze = FindObjectOfType<Maze>();
        this._Player = FindObjectOfType<Player>();
        this._Goal = FindObjectOfType<Goal>();
        this._GameCamera = FindObjectOfType<GameCamera>();
    }

    private void FixedUpdate()
    {
        //Checks if sliders have changed values this frame (to avoid unnecessary OnValueChanged calls on the sliders)
        if (this._MazeSize.x != this._SliderX.value || this._MazeSize.y != this._SliderY.value)
        {
            this._MazeSize.x = (int)this._SliderX.value;
            this._MazeSize.y = (int)this._SliderY.value;
            StartCoroutine(ChangeMaze());
        }
    }

    /// <summary>
    /// Starts ChangeMaze Coroutine
    /// </summary>
    public void Restart()
    {
        StartCoroutine(ChangeMaze());
    }

    /// <summary>
    /// Toggles between a 2D and 3D maze
    /// Starts ChangeMaze Coroutine
    /// </summary>
    public void ChangeMazeDimensions()
    {
        this._Is2D = !this._Is2D;
        StartCoroutine(ChangeMaze());
    }

    /// <summary>
    /// Flips the camera and maze so the invisible layer becomes visible
    /// </summary>
    public void Flip()
    {
        if (!this._Is2D && this._Maze.IsGenerated)
        {
            //Change variables
            this.IsFlipped = !this.IsFlipped;
            this._GameCamera.Flip();

            //Parent goal to maze, flip maze and unparent the goal
            this._Goal.transform.parent = this._Maze.transform;
            this._Maze.transform.Rotate(new Vector3(180.0f, 0.0f, 0.0f));
            this._Goal.transform.parent = null;

            //Move player to the active level
            this._Player.Spawn(new Vector3(this._Player.transform.position.x, 0.3f, -this._Player.transform.position.z));
        }
    }

    /// <summary>
    /// Changes the maze:
    /// Resets the game and the camera
    /// Fips the camera to the new maze size
    /// Hides the player and the goal
    /// Deletes the old maze and generates a new maze with the new size
    /// Spawns the player and the goal
    /// </summary>
    private IEnumerator ChangeMaze()
    {
        //Reset
        this.IsFlipped = false;
        this._GameCamera.Reset();

        //Adapt camera
        this._GameCamera.Fit(this._MazeSize.x, this._MazeSize.y);

        //Hide player and goal
        this._Player.gameObject.SetActive(false);
        this._Goal.gameObject.SetActive(false);

        //Wait until maze is build
        yield return StartCoroutine(_Maze.Generate(new Vector3Int(this._MazeSize.x, this._Is2D ? 1 : 2, this._MazeSize.y)));

        //Spawn player and goal
        this._Player.Spawn(new Vector3(-(this._MazeSize.x - 1) / 2.0f, 0.3f, -(this._MazeSize.y - 1) / 2.0f));
        this._Goal.Spawn(new Vector3(+(this._MazeSize.x - 1) / 2.0f, 0.5f, +(this._MazeSize.y - 1) / 2.0f));
    }
}
