using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GNBlockMap))]
public class GNBlockMapEditorView : Editor
{
    public static NPVoxHotkey HOTKEY_FOCUS_CAMERA = new NPVoxHotkey(KeyCode.F, false, false, false);

    const float TOOLBAR_HEIGHT = 75;

    private static GNBlockMapEditorVM useNextViewModel = null;
    private GNBlockMapEditorVM viewModel;
    private GNBlockMapEditorInputController inputController;
    private GNBlockMap m_blockMap;
    private GameObject m_currentBrush;
    private bool m_bActive = false;

    private float screenScale = 1.0f;

    public override void OnInspectorGUI()
    {
        // GUILayout.Button("StopEditor");
        DrawDefaultInspector();
        if( !m_bActive && GUILayout.Button("Edit Blockmap"))
        {
            Enable();
        }
        else if( m_bActive && GUILayout.Button("Stop Edit Blockmap"))
        {
            Disable();
        }

        if (m_bActive)
        {
            DrawTileEditor();
        }
    }

    public void OnEnable()
    {
    }

    private void Enable()
    {
        if (useNextViewModel != null)
        {
            viewModel = useNextViewModel;
            inputController = new GNBlockMapEditorInputController(viewModel);
            useNextViewModel = null;
        }
        else
        {
            viewModel = GNBlockMapEditorVM.CreateInstance<GNBlockMapEditorVM>();
            inputController = new GNBlockMapEditorInputController(viewModel);
        }

        m_blockMap = (GNBlockMap)target;

        viewModel.Enable(m_blockMap);

        Undo.undoRedoPerformed += MyUndoCallback;
        viewModel.OnCellZChanged += OnCellZChanged;
        viewModel.OnCellXYChanged += OnCellXYChanged;
        viewModel.OnPrefabChanged += OnPrefabChanged;
        viewModel.OnGridChanged += OnGridChanged;
        viewModel.OnToolChanged += OnToolChanged;

        // clean previous brushes
        for (int i = 0; i < m_blockMap.transform.childCount; i++)
        {
            Transform currentChild = m_blockMap.transform.GetChild(i);
            if (currentChild.name == "_brush")
            {
                GameObject.DestroyImmediate(currentChild.gameObject);
            }
        }

        OnPrefabChanged();
        OnGridChanged();
        FocusSceneView();
        Repaint();
        m_bActive = true;
    }

    public void OnDisable()
    {
        if (m_bActive)
        {
            Disable();
        }
    }

    private void Disable()
    {
        m_bActive = false;
        viewModel.Disable();

        Undo.undoRedoPerformed -= MyUndoCallback;
        viewModel.OnCellZChanged -= OnCellZChanged;
        viewModel.OnCellXYChanged -= OnCellXYChanged;
        viewModel.OnPrefabChanged -= OnPrefabChanged;
        viewModel.OnGridChanged -= OnGridChanged;
        viewModel.OnToolChanged -= OnToolChanged;


        GameObject.DestroyImmediate(m_currentBrush);
        m_currentBrush = null;

        // Try to re-select if there was just a prefab selected
        if (Selection.activeGameObject != null && !Selection.activeGameObject.scene.IsValid())
        {
            viewModel.SelectPrefab(Selection.activeGameObject);

            if (target)
            {
                Selection.activeGameObject = ((MonoBehaviour)target).gameObject;
            }

            useNextViewModel = viewModel;
            viewModel = null;
        }
    }

    private void MyUndoCallback()
    {
        viewModel.UndoRedoPerformed();
        SceneView.RepaintAll();
        Repaint();
    }

    public void OnSceneGUI()
    {
        if( !m_bActive ) return;

        if (viewModel.CurrentTool == GNBlockMapEditorVM.Tool.BOX)
        {
            if (viewModel.SelectBox(GNHandles.DrawBoundsSelection(viewModel.SelectedBox, viewModel.CellOffset, viewModel.CellSize)))
            {
                // SceneView.RepaintAll();
            }
        }
        else
        {
            MouseInput();
        }
        KeyboardInput();
        KeyboardShortcuts();

        GUILayout.BeginArea(new Rect(0, 0, 880, 100));
        EditorGUILayout.BeginVertical();
        {
            EditorGUILayout.BeginHorizontal();
            {
                DrawToolbar();
                DrawCameraToolbar();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            {
                DrawPaintMode();

                EditorGUILayout.BeginVertical();
                DrawShowLayerMode();
                if (viewModel.CurrentTool == GNBlockMapEditorVM.Tool.BOX)
                {
                    DrawBoxTools();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
        GUILayout.EndArea();

        if(inputController.IsPickingActive() )
        {
            GUILayout.BeginArea(new Rect(10, 100, 400, 100));
            GUILayout.Label("Tile Info: " + viewModel.GetTileInfo(), EditorStyles.whiteLargeLabel);
            GUILayout.EndArea();
        }

        if ( Event.current.type == EventType.Layout )
        {
            screenScale = NPVoxHandles.GetScreenScale();
        }

        GUILayout.BeginArea(new Rect(10, Screen.height * screenScale - 70, 400, 100));
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("CellSize: " + viewModel.CellSize, EditorStyles.whiteLargeLabel);
        GUILayout.Label("CellOffset: " + viewModel.CellOffset, EditorStyles.whiteLargeLabel);
        GUILayout.Label("Cell: " + viewModel.CurrentCell, EditorStyles.whiteLargeLabel);
        // GUILayout.Label("Brush: " + viewModel.CurrentPrefabPath, EditorStyles.whiteLargeLabel);
        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(Screen.width * screenScale - 350, Screen.height * screenScale - 240, 300, 200));
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        GUILayout.Label(inputController.GetCurrentToolInfo(), EditorStyles.whiteLargeLabel);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        Selection.activeGameObject = ((MonoBehaviour)target).gameObject;

        DrawZIndicator(screenScale);
    }

    private void DrawTileEditor()
    {
        if (viewModel.CurrentPrefab != null)
        {
            GUILayout.Label("Tile Settings: " + viewModel.CurrentPrefabPath);
            GNBlockMapTile tile = viewModel.CurrentPrefab.GetComponent<GNBlockMapTile>();
            if (tile)
            {
                UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(tile);
                bool changed = editor.DrawDefaultInspector();
                if (changed)
                {
                    // blubb
                }
            }
        }
    }

    private void DrawShowLayerMode()
    {
        GNBlockMapEditorVM.ShowLayerMode mode = viewModel.CurrentShowLayerMode;
        GUILayout.BeginHorizontal();
        GUILayout.Label("Layer Mode (z/y): ", EditorStyles.whiteBoldLabel);
        if (viewModel.SetCurrentShowLayerMode(NPVoxGUILayout.HotkeyToggleBar<GNBlockMapEditorVM.ShowLayerMode>(
           new string[] { "Active", "Below", "All" },
           new GNBlockMapEditorVM.ShowLayerMode[] {
                GNBlockMapEditorVM.ShowLayerMode.ACTIVE,
                GNBlockMapEditorVM.ShowLayerMode.BELOW,
                GNBlockMapEditorVM.ShowLayerMode.ALL
           },
        //    KeyCode.Z,
           KeyCode.None,
           mode,
           true
       )))
        {
            SceneView.RepaintAll();
        }
        GUILayout.EndHorizontal();
    }

    private void DrawPaintMode()
    {
        GNBlockMapEditorVM.PaintMode mode = viewModel.CurrentPaintMode;
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Paint Mode (p): ", EditorStyles.whiteBoldLabel);
        if (viewModel.SetCurrentPaintMode(NPVoxGUILayout.HotkeyToggleBar<GNBlockMapEditorVM.PaintMode>(
            new string[] { "Brush", "Random" },
            new GNBlockMapEditorVM.PaintMode[] {
                    GNBlockMapEditorVM.PaintMode.BRUSH,
                    GNBlockMapEditorVM.PaintMode.RANDOM
            },
            KeyCode.P,
            mode,
            true
        )))
        {
            SceneView.RepaintAll();
        }
        GUILayout.EndHorizontal();

        // if (viewModel.CurrentPaintMode == GNBlockMapEditorVM.PaintMode.RANDOM) // this leads to exception
        {
            GUILayout.BeginHorizontal();
            GNBlockMapEditorVM.RandomBrushFlags flags = (GNBlockMapEditorVM.RandomBrushFlags)EditorGUILayout.MaskField(
                new GUIContent("Options"),
                (int)viewModel.CurrentRandomBrushFlags,
                new string[] { "Prefab", "Rotate X", "Rotate Y", "Rotate Z", "Flip X", "Flip Y", "Flip Z" });
            if (flags != viewModel.CurrentRandomBrushFlags)
            {
                viewModel.CurrentRandomBrushFlags = flags;
                SceneView.RepaintAll();
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
        DrawBrushSetup();
        GUILayout.EndHorizontal();
    }

    private void DrawBrushSetup()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Brush: ", EditorStyles.whiteBoldLabel);
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        string folder = viewModel.CurrentPrefabFolder;
        string substr = folder != null && folder.Length > 24 ? folder.Substring(folder.Length - 24) : folder;
        EditorGUILayout.TextField(folder != null ? substr : "NONE");
        EditorGUILayout.ObjectField(viewModel.CurrentPrefab, typeof(GameObject), false);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Rx: " + viewModel.PrefabOrientation.x.ToString("000")))
        {
            viewModel.OrientPrefab(Vector3.right, true);
        }
        if (GUILayout.Button("Ry: " + viewModel.PrefabOrientation.y.ToString("000")))
        {
            viewModel.OrientPrefab(Vector3.up, true);
        }
        if (GUILayout.Button("Rz: " + viewModel.PrefabOrientation.z.ToString("000")))
        {
            viewModel.OrientPrefab(Vector3.forward, true);
        }
        if (GUILayout.Button("Fx: " + viewModel.PrefabFlip.x.ToString("0")))
        {
            viewModel.FlipPrefab(Vector3.right);
        }
        if (GUILayout.Button("Fy: " + viewModel.PrefabFlip.y.ToString("0")))
        {
            viewModel.FlipPrefab(Vector3.up);
        }
        if (GUILayout.Button("Fz: " + viewModel.PrefabFlip.z.ToString("0")))
        {
            viewModel.FlipPrefab(Vector3.forward);
        }
        // Editor
        // EditorGUILayout.Vector3Field("Rot", Vector3.zero);        
        // EditorGUILayout.Vector3Field("Scale", Vector3.zero);        
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    private void DrawBoxTools()
    {
        //       GNBlockMapEditorVM.PaintMode mode = viewModel.CurrentPaintMode;
        GUILayout.BeginHorizontal();
        GUILayout.Label("Box Tools: ", EditorStyles.whiteBoldLabel);
        if (viewModel.ApplyBoxTool(NPVoxGUILayout.HotkeyToolbar<GNBlockMapEditorVM.BoxTool>(
            new string[] { "Paint", "Erase", "Transform" },
            new GNBlockMapEditorVM.BoxTool[] {
                GNBlockMapEditorVM.BoxTool.PAINT,
                GNBlockMapEditorVM.BoxTool.ERASE,
                GNBlockMapEditorVM.BoxTool.TRANSFORM
            },
            new KeyCode[] { KeyCode.None, KeyCode.None, KeyCode.None },
            GNBlockMapEditorVM.BoxTool.NONE,
            false,
            false,
            true
        )))
        {
            SceneView.RepaintAll();
        }
        GUILayout.EndHorizontal();
    }


    private void DrawToolbar()
    {
        GNBlockMapEditorVM.Tool tool = viewModel.CurrentTool;
        if (viewModel.CurrentTool != (tool = NPVoxGUILayout.HotkeyToolbar<GNBlockMapEditorVM.Tool>(
            new string[] { "View", "Paint", /*"Erase", "Brush", "Layer", "Grid",*/  "Box" },
           new GNBlockMapEditorVM.Tool[] {
                GNBlockMapEditorVM.Tool.NONE,
                GNBlockMapEditorVM.Tool.PAINT,
                // GNBlockMapEditorVM.Tool.ERASE,
                // GNBlockMapEditorVM.Tool.BRUSH,
                // GNBlockMapEditorVM.Tool.LAYER,
                // GNBlockMapEditorVM.Tool.GRID,
                GNBlockMapEditorVM.Tool.BOX
           },
           new KeyCode[] { KeyCode.Q, KeyCode.W, /*KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.E,*/ KeyCode.E },
           tool,
            true,
            false,
            true
       )))
        {
            if (tool == GNBlockMapEditorVM.Tool.NONE)
            {
                Tools.current = Tool.View;
            }
            else
            {
                viewModel.SetCurrentTool(tool);
            }
            SceneView.RepaintAll();
        }
    }

    private void KeyboardShortcuts()
    {
        if (Tools.current != Tool.None)
        {
            if (viewModel.UpdateCurrentToolFromSceneView(Tools.current))
            {
                Repaint();
            }
        }
    }

    private void MouseInput()
    {
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        EventType eventType = Event.current.GetTypeForControl(controlID);
        Event e = Event.current;

        float mouseScale = NPVoxHandles.GetMouseScale(SceneView.currentDrawingSceneView.camera);
        Ray r = Camera.current.ScreenPointToRay(new Vector3(Event.current.mousePosition.x * mouseScale, -Event.current.mousePosition.y * mouseScale + Camera.current.pixelHeight));

        bool bShiftHeld = e.shift;
        bool bAltHeld = e.alt;
        bool bCtrlHeld = e.control;

        if (Event.current.mousePosition.y < TOOLBAR_HEIGHT)
        {
            // allow to click the buttons always
            return;
        }

        // if (viewModel.CurrentTool == GNBlockMapEditorVM.Tool.ERASE)
        if( inputController.IsErasingActive() )
        {
            Handles.color = new Color(128, 0, 0, 0.25f);
            Handles.CubeCap(controlID, viewModel.CurrentCell, Quaternion.identity, viewModel.CellSize);
        }
        if( inputController.IsPrefabNavigationActive() )
        {
            Handles.color = new Color(128, 128, 0, 0.05f);
            Handles.CubeCap(controlID, viewModel.CurrentCell, Quaternion.identity, viewModel.CellSize);
        }
        if( inputController.IsPickingActive() )
        {
            Handles.color = new Color(128, 128, 0, 0.25f);
            Handles.CubeCap(controlID, viewModel.CurrentCell, Quaternion.identity, viewModel.CellSize);
        }
        if( inputController.IsRotationActive() )
        {
            Handles.color = new Color(0, 128, 0, 0.05f);
            Handles.CubeCap(controlID, viewModel.CurrentCell, Quaternion.identity, viewModel.CellSize);
        }
        
        switch (eventType)
        {
            case EventType.Layout:

                if (inputController.TakeEvents())
                {
                    HandleUtility.AddControl(
                        controlID,
                        HandleUtility.DistanceToCircle(r.origin, 10f)
                    );
                }

                break;

            case EventType.MouseDown:
                if (HandleUtility.nearestControl == controlID && e.button == 0)
                {
                    GUIUtility.hotControl = controlID;

                    if (inputController.MouseDown(e))
                    {
                        e.Use();
                        SceneView.RepaintAll();
                    }
                }
                break;

            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID)
                {
                    GUI.changed = true;
                    GUIUtility.hotControl = 0;
                    e.Use();
                }
                break;

            case EventType.MouseDrag:
                if (GUIUtility.hotControl == controlID)
                {
                    if (inputController.MouseDrag(RayCast(r)))
                    {
                        e.Use();
                        SceneView.RepaintAll();
                    }
                }
                break;

            case EventType.ScrollWheel:
                if (inputController.WheelScrolled(e.delta, bShiftHeld, bAltHeld, bCtrlHeld))
                {
                    e.Use();
                    if (inputController.MouseMoved(RayCast(r)))
                    {
                    }
                    SceneView.RepaintAll();
                }
                break;
            case EventType.MouseMove:
                if (inputController.MouseMoved(RayCast(r)))
                {
                    e.Use();
                    SceneView.RepaintAll();
                }
                break;
        }
    }

    private void KeyboardInput()
    {
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        EventType eventType = Event.current.GetTypeForControl(controlID);
        Event e = Event.current;

        switch (eventType)
        {
            case EventType.KeyDown:
                if (inputController.KeyStateChanged(true, e))
                {
                    e.Use();
                    SceneView.RepaintAll();
                }
                break;

            case EventType.KeyUp:
                if (inputController.KeyStateChanged(false, e))
                {
                    e.Use();
                    SceneView.RepaintAll();
                }
                break;
        }
    }

    private Vector3 RayCast(Ray r)
    {
        float z = viewModel.CurrentCell.z + viewModel.CellSize / 2;
        float delta = (z - r.origin.z) / r.direction.z;
        if (delta > 0.1f && delta < 10000f)
        {
            return r.origin + r.direction * delta;
        }
        else
        {
            return Vector3.zero;
        }
    }

    private void DrawCameraToolbar()
    {
        GNCameraSetups.CameraMode previousMode = GNCameraSetups.GetCameraMode();
        GNCameraSetups.CameraMode mode;
        if (previousMode != (mode = NPVoxGUILayout.HotkeyToolbar<GNCameraSetups.CameraMode>(
            new string[] { "Top", "Ingame", "Front" },
            new GNCameraSetups.CameraMode[] { GNCameraSetups.CameraMode.TOP, GNCameraSetups.CameraMode.INGAME, GNCameraSetups.CameraMode.FRONT },
            new KeyCode[] { KeyCode.A, KeyCode.S, KeyCode.D },
            previousMode,
            true, false, true
        )))
        {
            GNCameraSetups.SetCameraMode(mode);
        }

        if (NPVoxGUILayout.HotkeyButton("Center", HOTKEY_FOCUS_CAMERA))
        {
            SceneView.lastActiveSceneView.LookAt(viewModel.FullMapExtends().center, Quaternion.Euler(-45f, 0f, 0f), 20f);
        }
    }

    private void DrawZIndicator(float screenScale)
    {
        float uiWidth = 30f;
        float uiHeight = 150f;
        float uiX = 5f;

        float uiY = (screenScale * Screen.height) / 2 - uiHeight / 2; //30f;

        float cellStepSize = viewModel.CellSize;
        float cellMinZ = Mathf.Min(-12.0f, viewModel.CurrentCell.z);
        float cellMaxZ = Mathf.Max(+12.0f, viewModel.CurrentCell.z);

        int numSteps = (int)Mathf.Ceil((cellMaxZ - cellMinZ) / cellStepSize);

        float uiStepSize = uiHeight / numSteps;

        // draw layers in white
        for (int i = 0; i <= numSteps; i++)
        {
            float uiCurrentY = uiY + uiStepSize * i;
            float cellCurrentZ = cellMinZ + cellStepSize * i;
            if (Mathf.Abs(cellCurrentZ - viewModel.CurrentCell.z) > 0.1f)
            {
                EditorGUI.DrawRect(new Rect(uiX, uiCurrentY, uiWidth, 2f), Color.yellow);
            }
        }

        // draw current layer in green
        {
            float cellCurrentZ = viewModel.CurrentCell.z;
            float uiCurrentY = uiY + Mathf.Lerp(0, uiHeight, Mathf.InverseLerp(cellMinZ, cellMaxZ, cellCurrentZ));
            GUIStyle style = EditorStyles.whiteBoldLabel;
            GUILayout.BeginArea(new Rect(uiX + uiWidth, uiCurrentY - 5f, 100f, cellCurrentZ + 30f - 5f));
            EditorGUILayout.LabelField(cellCurrentZ + "", style);
            GUILayout.EndArea();
            EditorGUI.DrawRect(new Rect(uiX, uiCurrentY, uiWidth, 2f), Color.green);
        }
    }

    private void OnToolChanged()
    {
        OnCellChanged();
    }

    private void OnPrefabChanged()
    {
        if (m_currentBrush)
        {
            GameObject.DestroyImmediate(m_currentBrush);
            m_currentBrush = null;
        }

        if (viewModel.CurrentPrefab)
        {
            m_currentBrush = GameObject.Instantiate(viewModel.CurrentPrefab);
            m_currentBrush.transform.parent = m_blockMap.transform;
            m_currentBrush.transform.localEulerAngles = viewModel.PrefabOrientation;
            m_currentBrush.transform.localScale = viewModel.PrefabFlip;
            m_currentBrush.name = "_brush";
            m_currentBrush.hideFlags = HideFlags.HideInInspector | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            m_currentBrush.transform.position = viewModel.CurrentCell + viewModel.CurrentPrefabOffset;
            m_currentBrush.gameObject.SetActive(viewModel.CurrentTool == GNBlockMapEditorVM.Tool.PAINT /*|| viewModel.CurrentTool == GNBlockMapEditorVM.Tool.BRUSH*/);
            SceneView.RepaintAll();
            Repaint();

            GUI.changed = true;
        }
    }

    private void OnCellZChanged()
    {
        OnCellChanged();
    }

    private void OnCellXYChanged()
    {
        OnCellChanged();
    }

    private void OnCellChanged()
    {
        if (m_currentBrush != null)
        {
            m_currentBrush.transform.position = viewModel.CurrentCell + viewModel.CurrentPrefabOffset;;
            m_currentBrush.gameObject.SetActive(viewModel.CurrentTool == GNBlockMapEditorVM.Tool.PAINT /*|| viewModel.CurrentTool == GNBlockMapEditorVM.Tool.BRUSH*/);
        }

        // m_blockMap.m_currentCell = viewModel.CurrentCell;
    }

    private void OnGridChanged()
    {
        // m_blockMap.m_vCellOffset = viewModel.CellOffset;
        // m_blockMap.m_vCellSize = Vector3.one * viewModel.CellSize;
    }

    private void FocusSceneView()
    {
        if (SceneView.sceneViews.Count > 0)
        {
            SceneView biggest = null;
            foreach (SceneView sceneView in SceneView.sceneViews)
            {
                if (biggest == null || sceneView.size > biggest.size)
                {
                    biggest = sceneView;
                }
            }
            if (biggest != null)
            {
                biggest.Focus();
            }
        }
    }
}