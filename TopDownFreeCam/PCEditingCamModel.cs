using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.InputSystem;


public class PCEditingCamModel : EditingCamModel
    ,IEditCamMove<Vector3>
    ,IEditCamRotate<float>
    ,IEditCamZoom<float>
    ,IEditCamPitch<float>
{
    [Header("References")]
    [SerializeField] private CameraRig m_camRig;
    [SerializeField] private Camera m_cam;
    [SerializeField] private LayerMask m_groundLayer;


    [Space]
    [Header("Event Theme")]
    [SerializeField] private InputActionTheme m_inputActionTheme;
    [SerializeField] private NavigationTheme m_navigationTheme;


    [Space]
    [Header("Param")]
    [SerializeField] private MinMax m_zoomRange;
    [SerializeField] private MinMax m_pitchRange;
    [SerializeField] private float m_zoomSpeed;
    [SerializeField] private float m_rotateSpeed;

    [Space]
    [Header("Reset Param")]
    [SerializeField] private float m_resetRotation;
    [SerializeField] private float m_resetPitch;
    [SerializeField] private float m_resetZoom;
    [SerializeField] private Vector3 m_resetCenter;


    private bool m_panMoveFlag = false;
    private Vector3 m_panInitMousePos;
    private Vector3 m_panTargetMousePos;
    private Vector3 m_panInitCamPos;
    private Vector3 m_panTargetPos;

    private bool m_panningFlag;
    private bool m_rotatingFlag;


    private void Awake()
    {
        m_inputActionTheme.Subscribe("Cam_MiddleBtnHold", OnPanningStart);
        m_inputActionTheme.Subscribe("Cam_MiddleBtnRelease", OnPanningCancel);
        m_inputActionTheme.Subscribe("Cam_RightBtnHold", OnRotateStart);
        m_inputActionTheme.Subscribe("Cam_RightBtnRelease", OnRotateCancel);
        m_inputActionTheme.Subscribe("Cam_MouseMove", OnMouseMove);
        m_inputActionTheme.Subscribe("Cam_Scroll", OnZooming);
        m_inputActionTheme.Subscribe("Cam_SpacePress", OnCamReset);
        m_navigationTheme.Subscribe("NavigationCenterSet", SetCenter);
    }

    private void Start()
    {
        SetPitch(m_resetPitch);
        SetRotation(m_resetRotation);
        SetZoom(m_resetZoom);
    }

    private void Update()
    {
        if (m_panningFlag)
        {
            Vector2 v2 = Mouse.current.position.ReadValue();
            MoveUpdate(v2);
        }
    }
    public override void SetCenter(Vector3 posistion)
    {
        m_resetCenter = posistion;
        m_camRig.RootPosition = posistion;
    }

    #region Interface implementation


    public void Zoom(float step, MinMax range)
    {
        float dist = m_camRig.RootToJointDist;
        dist += step;
        m_camRig.RootToJointDist = math.clamp(dist, range.min, range.max);
    }

    public void SetZoom(float zoom_value)
    {
        m_camRig.RootToJointDist = zoom_value;
    }

    public void ZoomRange(MinMax range)
    {
        m_zoomRange = range;
    }

    public void Pitch(float step, MinMax range)
    {
        float angle = m_camRig.Pitch;
        angle += step;
        m_camRig.Pitch = math.clamp(angle, range.min, range.max);
    }

    public void SetPitch(float pitch_angle)
    {
        m_camRig.Pitch = pitch_angle;
    }

    public void Rotate(float step)
    {
        m_camRig.Rotation += step;
    }

    public void SetRotation(float angle)
    {
        m_camRig.Rotation = angle;
    }

    public void MoveStart()
    {
        Ray ray = m_cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 500, m_groundLayer))
        {
            m_panInitMousePos = hit.point;
            m_panInitCamPos = m_camRig.RootPosition;
        }
    }

    public void MoveUpdate(Vector3 mouse_pos)
    {
        Vector3 rootPos = m_camRig.RootPosition;
        if (!m_panMoveFlag)
        {
            Ray ray = m_cam.ScreenPointToRay(mouse_pos);
            if (Physics.Raycast(ray, out RaycastHit hit, 500, m_groundLayer))
            {
                m_panTargetMousePos = hit.point;
                Vector3 delta = -m_panTargetMousePos + m_panInitMousePos;
                m_panTargetPos = m_panInitCamPos + delta;
                m_panMoveFlag = true;
            }
        }
        if (m_panMoveFlag && rootPos != m_panTargetPos)
        {
            //m_camRig.RootPosition = Vector3.MoveTowards(rootPos, m_panTargetPos, pan_speed);
            m_camRig.RootPosition = m_panTargetPos;
        }
        if (m_panMoveFlag && rootPos == m_panTargetPos)
        {
            Ray ray = m_cam.ScreenPointToRay(mouse_pos);
            if (Physics.Raycast(ray, out RaycastHit hit, 500, m_groundLayer))
            {
                m_panInitMousePos = hit.point;
                m_panInitCamPos = rootPos;
            }
            m_panMoveFlag = false;
        }
    }

    public void MoveEnd()
    {
        m_panMoveFlag = false;
    }

    #endregion


    #region InputAction hookup

    private void OnPanningStart(InputAction.CallbackContext arg)
    {
        if (!m_rotatingFlag)
            m_panningFlag = true;
        MoveStart();
    }

    private void OnPanningCancel(InputAction.CallbackContext arg)
    {
        m_panningFlag = false;
        MoveEnd();
    }

    private void OnRotateStart(InputAction.CallbackContext arg)
    {
        if (!m_panningFlag)
            m_rotatingFlag = true;
    }

    private void OnRotateCancel(InputAction.CallbackContext arg)
    {
        m_rotatingFlag = false;
    }

    private void OnMouseMove(InputAction.CallbackContext arg)
    {
        if (m_rotatingFlag)
        {
            Vector2 v2 = arg.ReadValue<Vector2>();
            Pitch(-v2.y * m_rotateSpeed, m_pitchRange);
            Rotate(v2.x * m_rotateSpeed);
        }
    }
    private void OnZooming(InputAction.CallbackContext arg)
    {
        float val = arg.ReadValue<float>();
        float dir = -Mathf.Sign(val);
        Zoom(m_zoomSpeed * dir, m_zoomRange);
    }

    private void OnCamReset(InputAction.CallbackContext arg)
    {
        SetPitch(m_resetPitch);
        SetRotation(m_resetRotation);
        SetZoom(m_resetZoom);
        SetCenter(m_resetCenter);
        EventManager.Excute(new EventInfo("GroundAxis", "AnyPanelToggle"), GridAxis.Y);
    }

    #endregion
}
