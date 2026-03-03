using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSens = 100f;
    public KeyCode toggleCursorKey = KeyCode.Tab; // Key to toggle cursor lock
    
    Transform playerBody;
    float pitch;
    public float pitchMin = -90f;
    public float pitchMax = 90f;
    
    private bool cursorLocked = true;

    void Start()
    {
        playerBody = transform.parent.transform;
        LockCursor();
    }

    void Update()
    {
        // Toggle cursor lock when the specified key is pressed
        if (Input.GetKeyDown(toggleCursorKey))
        {
            ToggleCursorLock();
        }
        
        // Only process mouse look when cursor is locked
        if (cursorLocked)
        {
            float moveX = Input.GetAxis("Mouse X") * mouseSens * Time.deltaTime;
            float moveY = Input.GetAxis("Mouse Y") * mouseSens * Time.deltaTime;
    
            if(playerBody)
                playerBody.Rotate(Vector3.up * moveX);
    
            pitch -= moveY;
            pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
            transform.localRotation = Quaternion.Euler(pitch, 0, 0);
        }
    }
    
    public void ToggleCursorLock()
    {
        if (cursorLocked)
            UnlockCursor();
        else
            LockCursor();
    }
    
    public void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        cursorLocked = true;
    }
    
    public void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        cursorLocked = false;
    }
}