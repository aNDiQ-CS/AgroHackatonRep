using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 10f;
    public float zoomSpeed = 10f;
    public float minZoom = 5f;
    public float maxZoom = 50f;

    private Vector3 _lastPanPosition;

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Zoom(scroll * zoomSpeed);

        if (Input.GetMouseButtonDown(1))
            _lastPanPosition = Input.mousePosition;
        else if (Input.GetMouseButton(1))
            Pan();
    }

    private void Zoom(float delta)
    {        
        float newZPos = Mathf.Clamp(Camera.main.transform.position.z + delta, minZoom, maxZoom);
        Camera.main.transform.position = new Vector3(Camera.main.transform.position.x,
            Camera.main.transform.position.y, newZPos);
    }

    private void Pan()
    {
        Vector3 delta = Input.mousePosition - _lastPanPosition;
        transform.Translate(-delta * panSpeed * Time.deltaTime);
        _lastPanPosition = Input.mousePosition;
    }
}