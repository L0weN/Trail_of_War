using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private GameObject _mainCamera;
    [Space]

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private float transitionDuration = 1f;
    public bool canChangeCameraSide = true;

    private CameraSide currentCameraSide = CameraSide.Right;

    private const float _threshold = 0.01f;
    
    CinemachineVirtualCamera VirtualCamera;
    Cinemachine3rdPersonFollow cinemachine3RdPersonFollow;
    CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;

    [Header("Noise Settings")]
    [SerializeField] NoiseSettings cameraHandheldNoiseSettings;
    [SerializeField] NoiseSettings cameraShakeNoiseSettings;

    [Space]
#if ENABLE_INPUT_SYSTEM
    [SerializeField] private PlayerInput _playerInput;
#endif
    [SerializeField] private Inputs _input;

    private bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
        }
    }

    private void Start()
    {
        //Cinemachine Camera Yaw follows the player
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

        //Virtual Camera
        VirtualCamera = GetComponent<CinemachineVirtualCamera>();

        //Cinemachine Components
        cinemachine3RdPersonFollow = VirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        cinemachineBasicMultiChannelPerlin = VirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void LateUpdate()
    {
        CameraRotation();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ChangeCameraSide();
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            CameraShake(.25f);
        }
    }

    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }
    private void CameraShake(float time)
    {
        StartCoroutine(CameraShaker(time));
    }

    private void ChangeCameraSide()
    {
        if (!canChangeCameraSide) return;
        canChangeCameraSide = false;
        currentCameraSide = (currentCameraSide == CameraSide.Right) ? CameraSide.Left : CameraSide.Right;
        StartCoroutine(SideChanger());
    }


    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    IEnumerator CameraShaker(float time)
    {
        cinemachineBasicMultiChannelPerlin.m_NoiseProfile = cameraShakeNoiseSettings;
        Debug.Log(cinemachineBasicMultiChannelPerlin.m_NoiseProfile + " " + cameraShakeNoiseSettings);

        float elapsedTime = 0;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cinemachineBasicMultiChannelPerlin.m_NoiseProfile = cameraHandheldNoiseSettings;
        Debug.Log(cinemachineBasicMultiChannelPerlin.m_NoiseProfile + " " + cameraHandheldNoiseSettings);
    }

    IEnumerator SideChanger()
    {
        float elapsedTime = 0;

        switch (currentCameraSide)
        {
            case CameraSide.Left:
                while (elapsedTime < transitionDuration)
                {
                    cinemachine3RdPersonFollow.CameraSide = Mathf.Lerp(
                        cinemachine3RdPersonFollow.CameraSide, 0, elapsedTime / transitionDuration);
                    elapsedTime += Time.deltaTime;
                    
                    yield return null;
                }
                break;
            case CameraSide.Right:
                while (elapsedTime < transitionDuration)
                {
                    cinemachine3RdPersonFollow.CameraSide = Mathf.Lerp(
                        cinemachine3RdPersonFollow.CameraSide, 1, elapsedTime / transitionDuration);
                    elapsedTime += Time.deltaTime;
                    
                    yield return null;
                }
                break;
        }

        canChangeCameraSide = true;
    }
}

public enum CameraSide
{
    Left,
    Right
}
