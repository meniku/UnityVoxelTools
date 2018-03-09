using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NPVoxNormalProcessorEditor : Editor
{
    private PreviewRenderUtility m_scene = null;

    void OnEnable()
    {
        m_scene = new PreviewRenderUtility();
    }

    public override bool HasPreviewGUI()
    {
        return true;
    }

    public override void OnPreviewGUI( Rect _rect, GUIStyle _background )
    {

    }
}
