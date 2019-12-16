public class Goal : MonoBehaviour
{
    private Player _Player = null;
    private Maze _Maze = null;

    private void Awake()
    {
        this._Player = FindObjectOfType<Player>();
        this._Maze = FindObjectOfType<Maze>();
    }

    /// <summary>
    /// Sets the goal active and gives it a new position
    /// </summary>
    /// <param name="position"></param>
    public void Spawn(Vector3 position)
    {
        this.gameObject.SetActive(true);
        this.transform.position = position;
    }

    /// <summary>
    /// When the player triggers a collision the game ends:
    /// - The walls of the maze drop
    /// - The goal becomes inactive
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == this._Player.gameObject)
        {
            this._Maze.DropWalls();
            this.gameObject.SetActive(false);
        }
    }
}
