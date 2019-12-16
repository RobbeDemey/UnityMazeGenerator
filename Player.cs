public class Player : MonoBehaviour
{
    [SerializeField] private Transform _GroundPlaneTransform = null;

    private Game _Game = null;
    private Maze _Maze = null;
    private CharacterController _CharacterController = null;

    private void Awake()
    {
        this._Game = FindObjectOfType<Game>();
        this._Maze = FindObjectOfType<Maze>();
        this._CharacterController = this.GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)  || Input.GetKeyDown(KeyCode.W))
        {
            Move(new Vector3(0.0f, 0.0f, +1.0f));
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            Move(new Vector3(0.0f, 0.0f, -1.0f));
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            Move(new Vector3(-1.0f, 0.0f, 0.0f));
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            Move(new Vector3(+1.0f, 0.0f, 0.0f));
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryFlipMaze();
        }
    }

    private void Move(Vector3 displacement)
    {
        if(this._Game.IsFlipped)
        {
            displacement = new Vector3(displacement.x, displacement.y, -displacement.z);
        }
        RaycastHit hitInfo;
        if (!Physics.Raycast(this.transform.position, displacement.normalized, out hitInfo, 0.6f))
        {
            this.transform.Translate(displacement);
            LeanTween.scale(this.gameObject, new Vector3(0.6f, 0.6f, 0.6f), 0.01f);
            LeanTween.scale(this.gameObject, new Vector3(0.5f, 0.5f, 0.5f), 0.01f).setDelay(0.02f);
        }
    }

    public void Spawn(Vector3 position)
    {
        this.gameObject.SetActive(true);
        this._CharacterController.enabled = false;
        this._CharacterController.transform.position = position;
        this._CharacterController.enabled = true;
    }

    public void TryFlipMaze()
    {
        RaycastHit hitInfo;
        Physics.Raycast(this.transform.position, Vector3.down, out hitInfo, 1.0f);
        if (hitInfo.transform == this._GroundPlaneTransform)
        {
            this._Game.Flip();
        };
    }
}
