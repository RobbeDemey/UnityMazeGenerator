public class GameCamera : MonoBehaviour
{
    private Camera _OrthographicCamera;

    private void Awake()
    {
        this._OrthographicCamera = this.GetComponent<Camera>();
    }

    /// <summary>
    /// Resets the projection maxtrix of the attached camera component
    /// </summary>
    public void Reset()
    {
        this._OrthographicCamera.ResetProjectionMatrix();
    }

    /// <summary>
    /// Adapts the attached camera component's size to the minimum size where 
    ///  the attached camera component's width is smaller than the width parameter and 
    ///  the attached camera component's height is smaller than the height parameter
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void Fit(int width, int height)
    {
        if (width / (float)height < this._OrthographicCamera.aspect)
        {
            this._OrthographicCamera.orthographicSize = height / 2.0f;
        }
        else
        {
            this._OrthographicCamera.orthographicSize = width / (2.0f * this._OrthographicCamera.aspect);
        }
    }

    /// <summary>
    /// Flips the y value of the projection matrix of the attached camera component
    /// </summary>
    public void Flip()
    {
        this._OrthographicCamera.projectionMatrix = this._OrthographicCamera.projectionMatrix * Matrix4x4.Scale(new Vector3(1, -1, 1));
    }
}
