using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class NPVoxNormalProcessorView : EditorWindow
{
    private NPVoxMeshOutput m_meshOutput = null;
    private NPVoxNormalProcessor m_viewedProcessor = null;
    private string m_processorName = "";

    private int iSelectedToolbarOption = 0;

    private Scene m_previewScene;
    private GameObject m_camObject = null;
    private Camera m_camComponent = null;

    private GameObject m_light_01 = null;

    private GameObject m_previewObject = null;


    public static NPVoxNormalProcessorView ShowWindow()
    {
        return GetWindow<NPVoxNormalProcessorView>("Normal Processor");
    }

    private void OnEnable()
    {
        m_previewScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        
        m_camObject = EditorUtility.CreateGameObjectWithHideFlags("NormalProcessorView_Camera", HideFlags.HideAndDontSave);
        m_camObject.transform.position = new Vector3(0, -4, -4);
        m_camObject.transform.rotation = Quaternion.AngleAxis(-45.0f, Vector3.right);
        m_camComponent = m_camObject.AddComponent<Camera>();
        m_camComponent.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        m_camComponent.cullingMask = 64;
        EditorSceneManager.MoveGameObjectToScene(m_camObject, m_previewScene);

        m_light_01 = EditorUtility.CreateGameObjectWithHideFlags("NormalProcessorView_Light01", HideFlags.HideAndDontSave);
        Light lightComp = m_light_01.AddComponent<Light>();
        lightComp.color = new Color(1, 0, 0);
        lightComp.transform.position = new Vector3(0, -4, -4);
        EditorSceneManager.MoveGameObjectToScene(m_light_01, m_previewScene);
        m_light_01.SetActive(false);
    }

    private void OnDestroy()
    {
        EditorSceneManager.CloseScene(m_previewScene, true);

        GameObject.DestroyImmediate(m_camObject);
        GameObject.DestroyImmediate(m_light_01);
        GameObject.DestroyImmediate(m_previewObject);
    }

    void OnGUI()
    {
        if ( m_meshOutput && m_viewedProcessor )
        {
            GUILayoutOption[] layoutNoExpand = { GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false) };
            GUILayout.BeginHorizontal();

            // To minimize space of tools section
            GUIStyle styleNoStretch = new GUIStyle();
            styleNoStretch.stretchWidth = false;

            // Draw tools section
            GUILayout.BeginVertical(styleNoStretch);

            GUILayout.Label(m_processorName, layoutNoExpand );


            iSelectedToolbarOption = GUILayout.Toolbar(iSelectedToolbarOption, new string[] { "View Mode", "Edit Mode" }, layoutNoExpand);

            GUILayout.EndVertical();

            // Draw scene view
            GUILayout.Box("", GUILayout.ExpandWidth( true ), GUILayout.ExpandHeight( true ) );
            if ( Event.current.type == EventType.Repaint )
            {
                m_previewObject.SetActive(true);
                //m_light_01.SetActive(true);

                m_camComponent.pixelRect = GUILayoutUtility.GetLastRect();
                m_camComponent.Render();


                m_previewObject.SetActive(false);
                m_light_01.SetActive(false);
            }

            GUILayout.EndHorizontal();
        }
    }

    public void SetContext( NPVoxMeshOutput meshOutput, NPVoxNormalProcessor viewedProcessor )
    {
        foreach (object attr in viewedProcessor.GetType().GetCustomAttributes(true) )
        {
            NPVoxAttributeNormalProcessorListItem normalAttr = attr as NPVoxAttributeNormalProcessorListItem;
            if ( normalAttr != null )
            {
                m_processorName = normalAttr.ProcessorType.ToString() + ": " + normalAttr.Name;
            }
        }

        if ( meshOutput != m_meshOutput )
        {
            GameObject.DestroyImmediate(m_previewObject);
            
            m_previewObject = meshOutput.Instatiate();
            m_previewObject.hideFlags = HideFlags.HideAndDontSave;
            m_previewObject.layer = 6;
            EditorSceneManager.MoveGameObjectToScene(m_previewObject, m_previewScene);
            m_previewObject.SetActive(false);
        }

        m_meshOutput = meshOutput;
        m_viewedProcessor = viewedProcessor;
    }
}
