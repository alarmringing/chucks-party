using UnityEngine;
using System.Collections;

// Camera Controller Isometric
// Allows the camera to move left, right along a fixed axis, zoom in and out.
// Attach to a camera GameObject (e.g IsoCamera) for functionality.

public class CameraController : MonoBehaviour
{

    //Include the CameraObject
    public Camera IsoCamera;
    // How fast the camera moves (panning)
    public int cameraVelocity = 10;
    // How fast the camera zooms (zooming)
    public float cameraZoomStep = 2f;
    // how much the camera rotates per rotation (rotating) - if this gets changed to another angle, the rest of the controls will not work properly
    float cameraRotationStep = 90f;
    // how much rotation do we need to do each frame
    float cameraRotationQueued = 0f;
    // declaring the rotation we'll be using
    Quaternion newRotation;
    public string cameraPointing = "N";
    public float mouseAxisCorrection = 10.5f;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        KeyboardControl(); // call Keyboard controls
        MouseControl(); // call Mouse controls
        GamePadControl(); // call GamePad controls
                          //Check every frame if there is rotation to be done
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, 0.2f);
    }

    void RotateRight()
    {
        //Keeping track of camera orientation to translate correctly
        if (cameraPointing == "N")
        {
            cameraPointing = "E";
        }
        else if (cameraPointing == "E")
        {
            cameraPointing = "S";
        }
        else if (cameraPointing == "S")
        {
            cameraPointing = "W";
        }
        else if (cameraPointing == "W")
        {
            cameraPointing = "N";
        }
        cameraRotationQueued -= cameraRotationStep; //adding a left step to rotation needed to be done
        newRotation = Quaternion.AngleAxis(cameraRotationQueued, Vector3.up);
    }

    void RotateLeft()
    {
        //Keeping track of camera orientation to translate correctly
        if (cameraPointing == "N")
        {
            cameraPointing = "W";
        }
        else if (cameraPointing == "W")
        {
            cameraPointing = "S";
        }
        else if (cameraPointing == "S")
        {
            cameraPointing = "E";
        }
        else if (cameraPointing == "E")
        {
            cameraPointing = "N";
        }
        cameraRotationQueued += cameraRotationStep; //adding a right step to ritation needed to be done
        newRotation = Quaternion.AngleAxis(cameraRotationQueued, Vector3.up);
    }

    void TranslateLeft()
    {
        if (cameraPointing == "N" | cameraPointing == "W")
        {
            transform.Translate((Vector3.left * cameraVelocity) * Time.deltaTime, Space.World);
            transform.Translate((Vector3.up * 0.4f * cameraVelocity) * Time.deltaTime, Space.World);
        }
        else
        {
            transform.Translate((Vector3.left * cameraVelocity) * Time.deltaTime, Space.World);
            transform.Translate((Vector3.down * 0.4f * cameraVelocity) * Time.deltaTime, Space.World);
        }
    }

    void TranslateRight()
    {
        if (cameraPointing == "N" | cameraPointing == "W")
        {
            transform.Translate((Vector3.right * cameraVelocity) * Time.deltaTime, Space.World);
            transform.Translate((Vector3.down * 0.4f * cameraVelocity) * Time.deltaTime, Space.World);
        }
        else
        {
            transform.Translate((Vector3.right * cameraVelocity) * Time.deltaTime, Space.World);
            transform.Translate((Vector3.up * 0.4f * cameraVelocity) * Time.deltaTime, Space.World);
        }
    }

    void TranslateUp()
    {
        transform.Translate((Vector3.up * cameraVelocity) * Time.deltaTime, Space.World);
    }
    void TranslateDown()
    {
        transform.Translate((Vector3.down * cameraVelocity) * Time.deltaTime, Space.World);
    }

    // Keyboard input controls
    void KeyboardControl()
    {
        // Left (screen-wise)
        if ((Input.GetKey(KeyCode.LeftArrow)))
        {
            TranslateLeft();
        }
        // Right (screen-wise)
        if ((Input.GetKey(KeyCode.RightArrow)))
        {
            TranslateRight();
        }
        // Up
        if ((Input.GetKey(KeyCode.UpArrow)))
        {
            TranslateUp();
        }
        // Down
        if (Input.GetKey(KeyCode.DownArrow))
        {
            TranslateDown();
        }
        // rotate left one step when key pressed
        if (Input.GetKeyDown(KeyCode.A))
        {
            RotateLeft();
        }
        //rotate right one step when key pressed
        if (Input.GetKeyDown(KeyCode.E))
        {
            RotateRight();
        }
    }

    //GamePad input controls
    void GamePadControl()
    {

    }

    //Mouse input controls
    void MouseControl()
    {
        //Zooming
        //First, adjust Zoom Step depending on current Zoom level
        if (IsoCamera.orthographicSize < 8.5f)
        {
            cameraZoomStep = 1f;
        }
        else if (IsoCamera.orthographicSize < 20f)
        {
            cameraZoomStep = 5f;
        }
        else if (IsoCamera.orthographicSize < 50f)
        {
            cameraZoomStep = 10f;
        }
        else
        {
            cameraZoomStep = 20f;
        }
        //Zoom when mouse wheel used
        if (!Input.GetMouseButton(0))
        { // Make sure the player isn't trying to rotate rather than zoom
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                IsoCamera.orthographicSize = Mathf.Clamp(IsoCamera.orthographicSize + cameraZoomStep, 1f, 50f);
            }
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                IsoCamera.orthographicSize = Mathf.Clamp(IsoCamera.orthographicSize - cameraZoomStep, 1f, 50f);
            }
        }

        //Panning and rotating with mouse via left button
        if (Input.GetMouseButton(0))
        {
            if (Input.GetAxis("Mouse X") < 0)
            {
                if (cameraPointing == "N")
                {
                    transform.Translate((Vector3.right * cameraVelocity * Mathf.Abs(Input.GetAxis("Mouse X")) * mouseAxisCorrection) * Time.deltaTime, Space.World);
                    transform.Translate((Vector3.down * 0.4f * cameraVelocity * Mathf.Abs(Input.GetAxis("Mouse X")) * mouseAxisCorrection) * Time.deltaTime, Space.World);
                }
                else if (cameraPointing == "E")
                {
                    transform.Translate((Vector3.right * cameraVelocity * Mathf.Abs(Input.GetAxis("Mouse X")) * mouseAxisCorrection) * Time.deltaTime, Space.World);
                    transform.Translate((Vector3.up * 0.4f * cameraVelocity * Mathf.Abs(Input.GetAxis("Mouse X")) * mouseAxisCorrection) * Time.deltaTime, Space.World);
                }
                else if (cameraPointing == "S")
                {
                    transform.Translate((Vector3.left * cameraVelocity * Mathf.Abs(Input.GetAxis("Mouse X")) * mouseAxisCorrection) * Time.deltaTime, Space.World);
                    transform.Translate((Vector3.down * 0.4f * cameraVelocity * Mathf.Abs(Input.GetAxis("Mouse X")) * mouseAxisCorrection) * Time.deltaTime, Space.World);
                }
                else
                {
                    transform.Translate((Vector3.left * cameraVelocity * Mathf.Abs(Input.GetAxis("Mouse X")) * mouseAxisCorrection) * Time.deltaTime, Space.World);
                    transform.Translate((Vector3.up * 0.4f * cameraVelocity * Mathf.Abs(Input.GetAxis("Mouse X")) * mouseAxisCorrection) * Time.deltaTime, Space.World);
                }
            }
            if (Input.GetAxis("Mouse X") > 0)
            {
                if (cameraPointing == "N")
                {
                    transform.Translate((Vector3.left * cameraVelocity * Mathf.Abs(Input.GetAxis("Mouse X")) * mouseAxisCorrection) * Time.deltaTime, Space.World);
                    transform.Translate((Vector3.up * 0.4f * cameraVelocity * Mathf.Abs(Input.GetAxis("Mouse X")) * mouseAxisCorrection) * Time.deltaTime, Space.World);
                }
                else if (cameraPointing == "E")
                {
                    transform.Translate((Vector3.left * cameraVelocity * Mathf.Abs(Input.GetAxis("Mouse X")) * mouseAxisCorrection) * Time.deltaTime, Space.World);
                    transform.Translate((Vector3.down * 0.4f * cameraVelocity * Mathf.Abs(Input.GetAxis("Mouse X")) * mouseAxisCorrection) * Time.deltaTime, Space.World);
                }
                else if (cameraPointing == "S")
                {
                    transform.Translate((Vector3.right * cameraVelocity * Mathf.Abs(Input.GetAxis("Mouse X")) * mouseAxisCorrection) * Time.deltaTime, Space.World);
                    transform.Translate((Vector3.up * 0.4f * cameraVelocity * Mathf.Abs(Input.GetAxis("Mouse X")) * mouseAxisCorrection) * Time.deltaTime, Space.World);
                }
                else if (cameraPointing == "W")
                {
                    transform.Translate((Vector3.right * cameraVelocity * Mathf.Abs(Input.GetAxis("Mouse X")) * mouseAxisCorrection) * Time.deltaTime, Space.World);
                    transform.Translate((Vector3.down * 0.4f * cameraVelocity * Mathf.Abs(Input.GetAxis("Mouse X")) * mouseAxisCorrection) * Time.deltaTime, Space.World);
                }
            }
            if (Input.GetAxis("Mouse Y") < 0)
            {
                transform.Translate((Vector3.up * cameraVelocity * Mathf.Abs(Input.GetAxis("Mouse Y")) * mouseAxisCorrection) * Time.deltaTime, Space.Self);
            }
            if (Input.GetAxis("Mouse Y") > 0)
            {
                transform.Translate((Vector3.down * cameraVelocity * Mathf.Abs(Input.GetAxis("Mouse Y")) * mouseAxisCorrection) * Time.deltaTime, Space.World);
            }
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                RotateLeft();
            }
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                RotateRight();
            }
        }
    }
}