using UnityEngine;
using System.Collections;
using Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class Player : MonoBehaviour
{
    [SerializeField] AudioClip[] footsteps;

    [SerializeField] Volume Volume;
    Vignette vignette;
    DepthOfField dof;

    [SerializeField] GameObject _crosshair;
    [SerializeField] GameObject _mainCam;
    [SerializeField] GameObject _headCam;

    [SerializeField] CinemachineVirtualCamera _playerCam;
    Animator anim;
    Rigidbody rb;  
    [SerializeField] AudioSource audioSource;
    public RaycastHit hit;

    bool isKneeling;
    bool isAiming;
    public bool isJumpscareEnded;

    int rotationSpeed = 20;

    float speed = 3;
    float _horizontal = 0f;
    float _vertical = 0f;
    float headXRot, headYRot;
    float aimZoomDistance = 1f;
    float aimSideOffset = 0.7f;
    float defaultZoomDistance = 2f;
    float defaultSideOffset = 0.6f;
    float zoomSpeed = 5f;


    //timer
    float time;
    float timer;

    // Start is called before the first frame update
    void Start()
    {
        if (Volume != null)
        {
            // Get the vignette effect from the post process volume
            Volume.profile.TryGet(out vignette);
            // Get the depth of field effect from the post process volume
            Volume.profile.TryGet(out dof);
        }
        _crosshair.SetActive(false);
        isAiming = false;
        isKneeling = false;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        time = 1.4f / speed;
        timer = Time.time;
    }

    private void Update()
    {
        if (isJumpscareEnded && vignette.intensity.value <= 0f)
        {
            vignette.intensity.Override(1);
            dof.focalLength.Override(300);
            vignette.intensity.value = 1;
            dof.focalLength.value = 300;
        }
        if (isJumpscareEnded)
        {
            vignette.intensity.value -= Time.deltaTime/10;
            dof.focalLength.value -= Time.deltaTime*30;
            if (vignette.intensity.value <= 0f)
            {
                isJumpscareEnded = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftControl) && isKneeling == false)
        {
            isKneeling = true;
            anim.SetBool("isKneeling", isKneeling);
        }
        else if(Input.GetKeyDown(KeyCode.LeftControl) && isKneeling == true)
        {
            isKneeling = false;
            anim.SetBool("isKneeling", isKneeling);
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            _crosshair.SetActive(true);
            isAiming = true;
        }
        else if(Input.GetKeyUp(KeyCode.Mouse1))
        {
            _crosshair.SetActive(false);
            isAiming = false;
        }
        timer += Time.deltaTime;
    }
    void FixedUpdate()
    {
        anim.SetFloat("Horizontal", _horizontal);
        anim.SetFloat("Vertical", _vertical);
        UpdateCamera();
        CamMovements();
        Movement();
    }
    void UpdateCamera()
    {
        Cinemachine3rdPersonFollow thirdPersonCam = _playerCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();

        if (isAiming)
        {
            thirdPersonCam.CameraDistance = Mathf.Lerp(thirdPersonCam.CameraDistance, aimZoomDistance, Time.deltaTime * zoomSpeed);
            thirdPersonCam.CameraSide = Mathf.Lerp(thirdPersonCam.CameraSide, aimSideOffset, Time.deltaTime * zoomSpeed);
            rotationSpeed = 5;
            speed = 2f;
            time = 1.4f / speed;
            anim.SetFloat("AnimSpeed", 0.5f);
        }
        else
        {
            thirdPersonCam.CameraDistance = Mathf.Lerp(thirdPersonCam.CameraDistance, defaultZoomDistance, Time.deltaTime * zoomSpeed);
            thirdPersonCam.CameraSide = Mathf.Lerp(thirdPersonCam.CameraSide, defaultSideOffset, Time.deltaTime * zoomSpeed);
            rotationSpeed = 20;
            speed = 3;
            time = 1.4f / speed;
            anim.SetFloat("AnimSpeed", 1f);
        }
    }
    private void CamMovements()
    {
        //cam movements
        headXRot += Input.GetAxis("Mouse Y") * Time.deltaTime * -450;
        headYRot += Input.GetAxis("Mouse X") * Time.deltaTime * 450;

        headXRot = Mathf.Clamp(headXRot, -20, 20);

        Physics.Raycast(Vector3.zero, _mainCam.transform.position, out hit);
        _headCam.transform.rotation = Quaternion.Euler(headXRot, headYRot, transform.eulerAngles.z);
        Debug.DrawLine(Vector3.zero, hit.transform.position);
    }
    private void Movement()
    {
        _horizontal = Input.GetAxis("Horizontal");
        _vertical = Input.GetAxis("Vertical");
        if (isKneeling == false && Mathf.Abs(_horizontal) >0f | Mathf.Abs(_vertical) > 0f)
        {
            if (timer >= time)
            {
                timer = 0;
                audioSource.PlayOneShot(footsteps[Random.Range(0, 10)], 1);
            }
            Vector3 moveDirection = new Vector3(_horizontal, 0, _vertical);
            rb.MovePosition(transform.position + transform.TransformDirection(moveDirection.normalized) * Time.deltaTime * speed);
        }

        Vector3 lookDirection = _mainCam.transform.forward;

            lookDirection.y = 0; // Ignore vertical distance

            // Smoothly rotate the character to face the camera direction
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
