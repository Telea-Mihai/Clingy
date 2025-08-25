using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour

{
    [Header("Inputs")]
    public Vector2 moveInputs;
    public Vector2 lookInputs;

    [Header("Moving")]
    public Rigidbody playerBody;
    public float movementSpeed = 25;
    public float turnSpeed = 100;
    public float maxspeed = 25;
    public float jumppower = 3;
    public float brakespeed = 7;
    public LayerMask groundlayer;

    [Header("Looking")]
    public Transform playerHead;
    public float lookAngleRange = 60;
    private float camRotation = 0;

    [Header("Other")]
    public Animator animator;
    public Camera playerCamera;
    public float RunningFOV;
    private float oldfov;
    [Header("Camera Effects")]
    public float cameraTiltAmount = 10f;
    public float cameraTiltSpeed = 8f;
    public float fovTransitionSpeed = 5f;
    public float windVolumeTransitionSpeed = 8f;
    private float currentTilt = 0f;
    private float targetFOV;
    private float targetWindVolume;
    private bool wasGrounded = true;
    private bool landingPopActive = false;
    private float landingPopTimer = 0f;
    public float landingPopDuration = 0.18f;
    public float landingPopAmount = 0.25f;
    private float defaultCameraLocalY;
    public GameObject SpeedParticleSystem;
    public AudioSource walksource;
    public AudioClip walkclip;
    public AudioClip runclip;
    public AudioSource source;
    public AudioClip windclip;

    private bool grounded;
    private bool running;
    private bool walks;
    private bool fovincreased;

    [Header("Wall Running")]
    public float wallRunDuration = 1.5f;
    public float wallRunGravity = 2f;
    public float wallRunSpeed = 15f;
    public float wallCheckDistance = 1f;
    public LayerMask wallLayer;
    private bool isWallRunning = false;
    private float wallRunTimer = 0f;
    public float wallJumpForce = 5f;
    private Vector3 wallNormal;

    [Header("Vaulting")]
    public float vaultHeight = 2f;
    public float vaultDistance = 1.5f;
    public float vaultSpeed = 15f;
    public float vaultCooldown = 1f;
    private bool isVaulting = false;
    private float lastVaultTime = 0f;



    private void Start()
    {
        walksource.clip = walkclip;
        walksource.loop = true;
        source.volume = 0;
        source.clip = windclip;
        source.loop = true;
        source.Play();

        playerCamera.orthographic = false;
        oldfov = playerCamera.fieldOfView;
        targetFOV = oldfov;
        targetWindVolume = source.volume;
        if (playerCamera != null)
            defaultCameraLocalY = playerCamera.transform.localPosition.y;
    }


    public void UpdateMoveInputs(InputAction.CallbackContext context)
    {
        moveInputs = context.ReadValue<Vector2>();
    }

    public void UpdateLookInputs(InputAction.CallbackContext context)
    {
        lookInputs = context.ReadValue<Vector2>();
    }

    public void Jumping(InputAction.CallbackContext context)
    {
        if (grounded && context.started)
        {
            playerBody.AddForce(Vector3.up * jumppower, ForceMode.Impulse);
        }
        else if (isWallRunning && context.started)
        {
            Debug.Log("wall jumping");
            Vector3 jumpDirection = wallNormal * wallJumpForce * 0.7f + Vector3.up * wallJumpForce * 0.8f;

            isWallRunning = false;
            wallRunTimer = 0f;

            Vector3 currentVel = playerBody.linearVelocity;
            playerBody.linearVelocity = new Vector3(0, Mathf.Max(0, currentVel.y), 0);

            playerBody.AddForce(jumpDirection, ForceMode.Impulse);
        }
    }

    private void FixedUpdate()
    {
        CheckForVaulting();

        if (moveInputs != Vector2.zero)
        {
            if (isWallRunning)
            {
                Vector3 wallDirection = Vector3.ProjectOnPlane(playerBody.transform.forward, wallNormal).normalized;

                float inputForward = moveInputs.y;

                Vector3 wallMovement = wallDirection * inputForward * wallRunSpeed;
                playerBody.linearVelocity = new Vector3(wallMovement.x, playerBody.linearVelocity.y, wallMovement.z);

                playerBody.AddForce(Vector3.down * wallRunGravity, ForceMode.Force);
            }
            else if (!isVaulting)
            {
                playerBody.AddRelativeForce(new Vector3(moveInputs.x * movementSpeed * Time.deltaTime, 0, moveInputs.y * movementSpeed * Time.deltaTime), ForceMode.Impulse);
            }
            walks = true;
            if (!walksource.isPlaying && !running)
            {
                walksource.clip = walkclip;
                walksource.Play();
            }
        }
        else
        {
            if (grounded && !isVaulting)
            {
                Vector3 normalisedVelocity = playerBody.linearVelocity.normalized;
                Vector3 brakeVelocity = normalisedVelocity * brakespeed;
                playerBody.AddForce(-brakeVelocity);
            }
            walks = false;
            walksource.Stop();
        }
    }

    private void Update()
    {
        if (lookInputs != Vector2.zero)
        {
            playerBody.transform.Rotate(new Vector3(0, lookInputs.x * turnSpeed * Time.deltaTime), Space.Self);
            camRotation += lookInputs.y;
            camRotation = Mathf.Clamp(camRotation, -lookAngleRange, lookAngleRange);
            playerHead.localRotation = Quaternion.Euler(-camRotation, 0, 0);
        }

        float targetTilt = 0f;
        if (isWallRunning)
        {
            RaycastHit hitLeft, hitRight;
            bool leftWall = Physics.Raycast(playerBody.position, -playerBody.transform.right, out hitLeft, wallCheckDistance, wallLayer);
            bool rightWall = Physics.Raycast(playerBody.position, playerBody.transform.right, out hitRight, wallCheckDistance, wallLayer);

            if (leftWall)
                targetTilt = -cameraTiltAmount;
            else if (rightWall)
                targetTilt = cameraTiltAmount;
        }

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * cameraTiltSpeed);
        if (playerCamera != null)
        {
            var camEuler = playerCamera.transform.localEulerAngles;
            camEuler.z = currentTilt;
            playerCamera.transform.localEulerAngles = camEuler;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
        }
        if (source != null)
        {
            source.volume = Mathf.Lerp(source.volume, targetWindVolume, Time.deltaTime * windVolumeTransitionSpeed);
        }

        if (!wasGrounded && grounded)
        {
            landingPopActive = true;
            landingPopTimer = 0f;
        }
        wasGrounded = grounded;

        if (landingPopActive && playerCamera != null)
        {
            landingPopTimer += Time.deltaTime;
            float t = landingPopTimer / landingPopDuration;
            if (t < 0.5f)
            {
                float offset = Mathf.Lerp(0, -landingPopAmount, t * 2f);
                var camPos = playerCamera.transform.localPosition;
                camPos.y = defaultCameraLocalY + offset;
                playerCamera.transform.localPosition = camPos;
            }
            else if (t < 1f)
            {
                float offset = Mathf.Lerp(-landingPopAmount, 0, (t - 0.5f) * 2f);
                var camPos = playerCamera.transform.localPosition;
                camPos.y = defaultCameraLocalY + offset;
                playerCamera.transform.localPosition = camPos;
            }
            else
            {
                var camPos = playerCamera.transform.localPosition;
                camPos.y = defaultCameraLocalY;
                playerCamera.transform.localPosition = camPos;
                landingPopActive = false;
            }
        }

        bool wasWallRunning = isWallRunning;
        isWallRunning = false;
        wallNormal = Vector3.zero;
        checkWallrun();

        if (isWallRunning)
        {
            Debug.Log("wall running");
            wallRunTimer += Time.deltaTime;
            if (wallRunTimer > wallRunDuration)
            {
                isWallRunning = false;
                wallRunTimer = 0f;
            }
        }
        else
        {
            wallRunTimer = 0f;
        }


        float curspeed = Vector3.Magnitude(playerBody.linearVelocity);
        if (curspeed > maxspeed)
        {
            float brakeSpeed = curspeed - maxspeed;
            Vector3 normalisedVelocity = playerBody.linearVelocity.normalized;
            Vector3 brakeVelocity = normalisedVelocity * brakeSpeed;
            playerBody.AddForce(-brakeVelocity);
        }

        if (curspeed >= maxspeed - 5 && !fovincreased)
        {
            fovincreased = true;
            walks = false;
            running = true;
            walksource.clip = runclip;
            walksource.Play();
            targetFOV = RunningFOV;

            targetWindVolume = Mathf.Lerp(0, 1, (curspeed - (maxspeed - 5)) / 5);

            if (SpeedParticleSystem != null)
                SpeedParticleSystem.SetActive(true);
        }
        else if (curspeed < maxspeed - 5)
        {
            fovincreased = false;
            targetWindVolume = Mathf.Lerp(1, 0, (curspeed - (maxspeed - 5)) / 5);
            fovincreased = false;
            targetFOV = oldfov;
            running = false;
            if (SpeedParticleSystem != null)
                SpeedParticleSystem.SetActive(false);
        }

        grounded = Physics.Raycast(playerBody.position, Vector3.down, 1f, groundlayer, QueryTriggerInteraction.Ignore);

        if (animator != null)
        {
            if (running)
            {
                animator.SetBool("Walk", false);
                animator.SetBool("Running", running);
            }
            else
            {
                animator.SetBool("Running", running);
                animator.SetBool("Walk", walks);
            }
        }

        if (!grounded && !isWallRunning)
        {
            walksource.volume = 0;
            targetWindVolume = 1;
        }
        else if (grounded && !isWallRunning)
        {
            walksource.volume = 1;
            targetWindVolume = 0f;
        }
        else if (isWallRunning)
        {
            walksource.volume = 1f;
            targetWindVolume = 0.5f;
        }
    }

    private bool checkWallrun()
    {
        if (!grounded)
        {
            RaycastHit hitLeft, hitRight;
            bool leftWall = Physics.Raycast(playerBody.position, -playerBody.transform.right, out hitLeft, wallCheckDistance, wallLayer);
            bool rightWall = Physics.Raycast(playerBody.position, playerBody.transform.right, out hitRight, wallCheckDistance, wallLayer);
            if (leftWall)
            {
                isWallRunning = true;
                wallNormal = hitLeft.normal;
                return true;
            }
            else if (rightWall)
            {
                isWallRunning = true;
                wallNormal = hitRight.normal;
                return true;
            }
        }
        return false;
    }

    private void CheckForVaulting()
    {
        if (isVaulting || moveInputs.y <= 0.1f || Time.time - lastVaultTime < vaultCooldown)
            return;

        Vector3 playerCenter = playerBody.position + Vector3.up * 0.5f;
        Vector3 forwardDirection = playerBody.transform.forward;
        RaycastHit obstacleHit;
        if (Physics.Raycast(playerCenter, forwardDirection, out obstacleHit, vaultDistance, wallLayer))
        {
            Vector3 topCheckStart = obstacleHit.point + Vector3.up * vaultHeight;
            RaycastHit topHit;
            if (!Physics.Raycast(topCheckStart, Vector3.down, out topHit, vaultHeight, wallLayer))
            {
                Vector3 landingCheckStart = obstacleHit.point + forwardDirection * 1.5f + Vector3.up * vaultHeight;
                RaycastHit landingHit;
                if (Physics.Raycast(landingCheckStart, Vector3.down, out landingHit, vaultHeight + 2f, wallLayer + groundlayer ))
                {
                    PerformVault(obstacleHit.point, landingHit.point);
                }
            }
        }
    }


    private void PerformVault(Vector3 obstaclePoint, Vector3 landingPoint)
    {
        isVaulting = true;
        lastVaultTime = Time.time;

        Vector3 vaultDirection = (landingPoint - playerBody.position).normalized;
        Vector3 vaultForce = vaultDirection * vaultSpeed + Vector3.up * (vaultSpeed * 0.6f);

        playerBody.linearVelocity = Vector3.zero;
        playerBody.AddForce(vaultForce, ForceMode.Impulse);

        Invoke(nameof(StopVaulting), 0.5f);
    }

    private void StopVaulting()
    {
        isVaulting = false;
    }

    private void OnDrawGizmos()
    {
        if (playerBody == null) return;

        if (isWallRunning)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(playerBody.position, playerBody.position + wallNormal);

            Vector3 localWallNormal = playerBody.transform.InverseTransformDirection(wallNormal);
            Vector3 alongWallLocal = Vector3.Cross(localWallNormal, Vector3.up);
            Vector3 alongWallWorld = playerBody.transform.TransformDirection(alongWallLocal);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(playerBody.position, playerBody.position + alongWallLocal.normalized * 2f);
        }

        if (isWallRunning)
        {
            Vector3 localWallNormal = playerBody.transform.InverseTransformDirection(wallNormal);
            Vector3 jumpDirLocal = localWallNormal + Vector3.up;
            Vector3 jumpDirWorld = playerBody.transform.TransformDirection(jumpDirLocal);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(playerBody.position, playerBody.position + jumpDirWorld.normalized * 2f);
        }

        if (grounded && moveInputs.y > 0.1f)
        {
            Vector3 playerCenter = playerBody.position + Vector3.up * 0.5f;
            Vector3 forwardDirection = playerBody.transform.forward;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(playerCenter, playerCenter + forwardDirection * vaultDistance);
            Gizmos.color = Color.cyan;
            Vector3 vaultCheckStart = playerCenter + forwardDirection * vaultDistance;
            Gizmos.DrawWireCube(vaultCheckStart + Vector3.up * (vaultHeight * 0.5f), new Vector3(0.5f, vaultHeight, 0.5f));
        }

        if (isVaulting)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(playerBody.position + Vector3.up * 2f, 0.5f);
        }
    }
}
