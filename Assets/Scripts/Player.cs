using UnityEngine;
using Cinemachine;
public class Player : MonoBehaviour
{
    [SerializeField] GameObject _crosshair;
    [SerializeField] GameObject _mainCam;
    [SerializeField] GameObject _headCam;
    [SerializeField] CinemachineVirtualCamera _playerCam;
    Animator anim;
    float _horizontal = 0f;
    float _vertical = 0f;
    Rigidbody rb;
    float headXRot, headYRot;
    int rotationSpeed = 10;
    int speed = 3;
    RaycastHit hit;
    bool isKneeling;
    bool isAiming;

    float aimZoomDistance = 1f;
    float aimSideOffset = 0.7f;
    float defaultZoomDistance = 2f;
    float defaultSideOffset = 0.6f;

    float zoomSpeed = 5f;
    // Start is called before the first frame update
    void Start()
    {
        _crosshair.SetActive(false);
        isAiming = false;
        isKneeling = false;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
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

    }
    void FixedUpdate()
    {
        anim.SetFloat("Horizontal", _horizontal);
        anim.SetFloat("Vertical", _vertical);
        UpdateCamera();
        CamMovements();
        Movement();
    }
    private void LateUpdate()
    {
    }
    void UpdateCamera()
    {
        Cinemachine3rdPersonFollow thirdPersonCam = _playerCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();

        if (isAiming)
        {
            thirdPersonCam.CameraDistance = Mathf.Lerp(thirdPersonCam.CameraDistance, aimZoomDistance, Time.deltaTime * zoomSpeed);
            thirdPersonCam.CameraSide = Mathf.Lerp(thirdPersonCam.CameraSide, aimSideOffset, Time.deltaTime * zoomSpeed);
            rotationSpeed = 5;
            speed = 1;
            anim.SetFloat("AnimSpeed", 0.5f);
        }
        else
        {
            thirdPersonCam.CameraDistance = Mathf.Lerp(thirdPersonCam.CameraDistance, defaultZoomDistance, Time.deltaTime * zoomSpeed);
            thirdPersonCam.CameraSide = Mathf.Lerp(thirdPersonCam.CameraSide, defaultSideOffset, Time.deltaTime * zoomSpeed);
            rotationSpeed = 10;
            speed = 3;
            anim.SetFloat("AnimSpeed", 1f);
        }
    }
    private void CamMovements()
    {
        //cam movements
        headXRot += Input.GetAxis("Mouse Y") * Time.deltaTime * -150;
        headYRot += Input.GetAxis("Mouse X") * Time.deltaTime * 150;

        headXRot = Mathf.Clamp(headXRot, -20, 20);

        _headCam.transform.rotation = Quaternion.Euler(headXRot, headYRot, transform.eulerAngles.z);
    }
    private void Movement()
    {
        if (isKneeling == false)
        {
            _horizontal = Input.GetAxis("Horizontal");
            _vertical = Input.GetAxis("Vertical");
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
