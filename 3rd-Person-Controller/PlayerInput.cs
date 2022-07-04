using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public Vector2 moveInput;
    public Vector2 lookInput;

    public delegate void OnJumpButtonReleased();
    public OnJumpButtonReleased onJumpButtonReleased;

    public delegate void OnJumpButtonPressed();
    public OnJumpButtonPressed onJumpButtonPressed;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        TempControlls(); //Temp code untill new Input System is set up
    }

    void Controlls()
    {

    }

    void TempControlls()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        moveInput = new Vector2(h, v);
        lookInput = new Vector2(mouseX, -mouseY);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (onJumpButtonPressed != null)
                onJumpButtonPressed();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (onJumpButtonReleased != null)
                onJumpButtonReleased();
        }
    }
}
