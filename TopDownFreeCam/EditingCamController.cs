using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class EditingCamController : MonoBehaviour
{

    private EditingCamModel m_camModel;
    private static EditingCamController m_singleton;

    #region MonoBehaviour Callbacks

    private void Awake()
    {
        if (m_singleton != null)
            DestroyImmediate(gameObject);
        else
        {
            m_singleton = this;
            m_camModel = GetComponent<EditingCamModel>();
        }
    }

    #endregion

    public static T GetCamModel<T>() where T:EditingCamModel
    {
        return m_singleton.m_camModel as T;
    }
}

