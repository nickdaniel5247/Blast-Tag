using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class Movement : NetworkBehaviour
{
    private InputSystem controls;
    private Animator animator;
    private Vector2 movementInput;

    private NetworkVariable<float> currentX = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<float> currentY = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private float xVelocity = 0f;
    private float yVelocity = 0f;

    private Transform cam;

    public float moveSpeed = 5f;
    public float inputSmoothTime = 0.1f;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            cam.GetComponent<CinemachineCamera>().Follow = transform;
    }

    private void Awake()
    {
        controls = new InputSystem();
        animator = GetComponent<Animator>();
        cam = GameObject.Find("Third Person Camera").transform;
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
        if (!IsOwner)
        {
            //Smooth damp to prevent getting stuck on mediary inputs while waiting for new values
            float smoothX = Mathf.SmoothDamp(animator.GetFloat("Input X"), currentX.Value, ref xVelocity, inputSmoothTime);
            float smoothY = Mathf.SmoothDamp(animator.GetFloat("Input Y"), currentY.Value, ref yVelocity, inputSmoothTime);

            animator.SetFloat("Input X", smoothX);
            animator.SetFloat("Input Y", smoothY);
            return;
        }

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
        currentX.Value = Mathf.SmoothDamp(currentX.Value, movementInput.x, ref xVelocity, inputSmoothTime);
        currentY.Value = Mathf.SmoothDamp(currentY.Value, movementInput.y, ref yVelocity, inputSmoothTime);

        animator.SetFloat("Input X", currentX.Value);
        animator.SetFloat("Input Y", currentY.Value);
    }
}
