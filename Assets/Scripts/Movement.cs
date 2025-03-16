using UnityEngine;

public class Movement : MonoBehaviour
{
    private InputSystem controls;
    private Animator animator;
    private Vector2 movementInput;

    private float currentX = 0f;
    private float currentY = 0f;

    private float xVelocity = 0f;
    private float yVelocity = 0f;

    public float moveSpeed = 5f;
    public float inputSmoothTime = 0.1f;

    public Transform cam;

    private void Awake()
    {
        controls = new InputSystem();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        movementInput = controls.Player.Move.ReadValue<Vector2>();
        transform.rotation = Quaternion.Euler(0f, cam.eulerAngles.y, 0f);

        MovePlayer();
        UpdateAnimation();
    }

    private void MovePlayer()
    {
        float speed = moveSpeed;
        float movementX = movementInput.x;
        float movementY = movementInput.y;

        //Slow speed for side to side movements
        if (Mathf.Approximately(movementY, 0))
        {
            speed /= 2;
        }

        //Cap x direction when running forward
        if (Vector2.Angle(Vector2.up, movementInput) <= (45f + float.Epsilon))
        {
            bool negative = movementX < 0;
            movementX = Mathf.Min(Mathf.Abs(movementX), 0.5f);

            if (negative)
                movementX = -movementX;
        }

        Vector3 cameraForward = cam.forward;
        Vector3 cameraRight = cam.right;

        //Flatten the vectors on the X-Z plane (so they don't move up/down)
        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward.Normalize();
        cameraRight.Normalize();

        //Calculate the movement in world space, facing direction camera is pointing
        Vector3 move = (cameraForward * movementY + cameraRight * movementX) * speed * Time.deltaTime;

        //Move the player
        transform.Translate(move, Space.World);
    }

    private void UpdateAnimation()
    {
        //Smooth damp to prevent abrupt transitions
        currentX = Mathf.SmoothDamp(currentX, movementInput.x, ref xVelocity, inputSmoothTime);
        currentY = Mathf.SmoothDamp(currentY, movementInput.y, ref yVelocity, inputSmoothTime);

        animator.SetFloat("Input X", currentX);
        animator.SetFloat("Input Y", currentY);
    }
}
