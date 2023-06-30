using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    /// Movements
    [SerializeField] private float movementSpeed = 12.5f;
    [SerializeField] private float rotationSpeed = 500;
    [SerializeField] private float jumpForce = 20f;
    private float velocityY;
    private float previousPositionY;
    private float defaultSlopeLimit;

    /// Components
    private CharacterController characterController;
    
    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        defaultSlopeLimit = characterController.slopeLimit;
    }

    // Update is called once per frame
    void Update()
    {
        /// get input values
        float inputAxisHorizontal = Input.GetAxis("Horizontal");
        float inputAxisVertical = Input.GetAxis("Vertical");
        bool inputButtonDownJump = Input.GetButtonDown("Jump");

        /// jump
        if (characterController.isGrounded)
        {
            if (inputButtonDownJump) velocityY = jumpForce;
            else velocityY = -0.5f; // fix the so-so isGrounded flag
        }

        /// gravity
        velocityY += Physics.gravity.y * Time.deltaTime;

        /// get main camera forward & right transform so the movement goes relative to it
        Camera cameraMain = Camera.main;
        Vector3 cameraMainRight = cameraMain.transform.right;
        Vector3 cameraMainForward = cameraMain.transform.forward;
        // remove up from forward & right vector
        cameraMainRight.y = 0f;
        cameraMainForward.y = 0f;

        /// move
        // using cam forward & cam right velocity multiplied by input vertical & horizontal axis values to make a vector3 direction speed relative to the camera (without using up velocity)
        Vector3 movementDirection = (inputAxisVertical * cameraMainForward) + (inputAxisHorizontal * cameraMainRight);
        // store vector clamped magnitude before "loosing it" later using normalization, so you can use the original one on translate and preserve the 0 to 1 clamp that will work even for gamepad users
        float movementDirectionOriginalMagnitudeClamped = Mathf.Clamp01(movementDirection.magnitude);
        // clamp all the values between -1 and 1 and therefore the magnitude as well (speed of the vector between 0 & 1) which prevent your forward & right velocity to exceed movementSpeed if going diagonal
        movementDirection.Normalize();
        //Debug.Log(movementDirectionOriginalMagnitudeClamped + " | " + movementDirection.magnitude + " | " + movementDirection);
        characterController.Move(new Vector3(
            movementDirection.x * movementDirectionOriginalMagnitudeClamped * movementSpeed,
            velocityY,
            movementDirection.z * movementDirectionOriginalMagnitudeClamped * movementSpeed
        ) * Time.deltaTime);

        /// rotate
        if (movementDirection != Vector3.zero)
        {
            // rotate toward current movement forward & right velocity using a custom rotationSpeed
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(movementDirection, Vector3.up),
                rotationSpeed
                * ((movementDirection.magnitude / movementSpeed) * 100f <= 50f ? 5f : 1f) // fast rotation for low velocity, then regular for high velocity
                * Time.deltaTime
            );
        }

        /// fix sticking while jumping and reach top collision by setting the velocity back to 0 if not on ground && previous position y == current position y
        if (!characterController.isGrounded && previousPositionY == transform.position.y && velocityY > 0) velocityY = 0;
        previousPositionY = transform.position.y;

        /// fix step climbing while on the air by setting the character controller's slope to 0, and back to default if on the ground
        if (!characterController.isGrounded) characterController.slopeLimit = 0;
        else characterController.slopeLimit = defaultSlopeLimit;
    }
}
