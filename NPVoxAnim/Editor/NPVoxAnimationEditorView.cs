using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//http://code.tutsplus.com/tutorials/how-to-add-your-own-tools-to-unitys-editor--active-10047

[CustomEditor(typeof(NPVoxAnimationEditorSession))]
public class NPVoxAnimationEditorView : Editor
{
    // ===================================================================================================
    // Hotkeys
    // ===================================================================================================

    public static NPVoxHotkey HOTKEY_NEXT_FRAME = new NPVoxHotkey(KeyCode.RightArrow, false, false, true);
    public static NPVoxHotkey HOTKEY_PREVIOUS_FRAME = new NPVoxHotkey(KeyCode.LeftArrow, false, false, true);
    public static NPVoxHotkey HOTKEY_PREVIOUS_TRANSFORMATION = new NPVoxHotkey(KeyCode.UpArrow, false, false, true);
    public static NPVoxHotkey HOTKEY_NEXT_TRANSFORMATION = new NPVoxHotkey(KeyCode.DownArrow, false, false, true);
    public static NPVoxHotkey HOTKEY_MOVE_TRANSFORMATION_UP = new NPVoxHotkey(KeyCode.UpArrow, true, true, false);
    public static NPVoxHotkey HOTKEY_MOVE_TRANSFORMATION_DOWN = new NPVoxHotkey(KeyCode.DownArrow, true, true, false);
    public static NPVoxHotkey DUPLICATE_TRANSFORMATION = new NPVoxHotkey(KeyCode.D, false, false, true);
    public static NPVoxHotkey HOTKEY_EDIT_LAST_TRANSFOMRATION = new NPVoxHotkey(KeyCode.A,  false, false, false);
    public static NPVoxHotkey HOTKEY_AFFECTED_AREA_SELECT_ALL = new NPVoxHotkey(KeyCode.A,  false, false, true);

    public static NPVoxHotkey[] HOTKEYS_SELECT_FRAME = new NPVoxHotkey[]
    {
        new NPVoxHotkey(KeyCode.Alpha1, false, false, true),
        new NPVoxHotkey(KeyCode.Alpha2, false, false, true),
        new NPVoxHotkey(KeyCode.Alpha3, false, false, true),
        new NPVoxHotkey(KeyCode.Alpha4, false, false, true),
        new NPVoxHotkey(KeyCode.Alpha5, false, false, true),
        new NPVoxHotkey(KeyCode.Alpha6, false, false, true),
        new NPVoxHotkey(KeyCode.Alpha7, false, false, true),
        new NPVoxHotkey(KeyCode.Alpha8, false, false, true),
        new NPVoxHotkey(KeyCode.Alpha9, false, false, true),
        new NPVoxHotkey(KeyCode.Alpha0, false, false, true)
    };
    public static NPVoxHotkey[] HOTKEYS_SELECT_TRANSFORMATION = new NPVoxHotkey[]
    {
        new NPVoxHotkey(KeyCode.Alpha1, false, false, false),
        new NPVoxHotkey(KeyCode.Alpha2, false, false, false),
        new NPVoxHotkey(KeyCode.Alpha3, false, false, false),
        new NPVoxHotkey(KeyCode.Alpha4, false, false, false),
        new NPVoxHotkey(KeyCode.Alpha5, false, false, false),
        new NPVoxHotkey(KeyCode.Alpha6, false, false, false),
        new NPVoxHotkey(KeyCode.Alpha7, false, false, false),
        new NPVoxHotkey(KeyCode.Alpha8, false, false, false),
        new NPVoxHotkey(KeyCode.Alpha9, false, false, false),
        new NPVoxHotkey(KeyCode.Alpha0, false, false, false)
    };
    public static NPVoxHotkey HOTKEY_DELETE_LAST_TRANSFOMRATION = new NPVoxHotkey(KeyCode.X,  false, false, false);
    public static NPVoxHotkey HOTKEY_PREVIEW = new NPVoxHotkey(KeyCode.P, false, true, true);
    public static NPVoxHotkey HOTKEY_APPEND_TRANSFORMATION = new NPVoxHotkey(KeyCode.C, false, false, false);
    public static NPVoxHotkey HOTKEY_APPEND_BONETRANSFORMATION = new NPVoxHotkey(KeyCode.C, false, true, false);
    public static NPVoxHotkey HOTKEY_RESET_SELECTED_TRANSFORMATION = new NPVoxHotkey(KeyCode.S, false, false, false);
    public static NPVoxHotkey HOTKEY_HIDE_SELECTED_BONES = new NPVoxHotkey(KeyCode.H, false, false, false);
      
    // ===================================================================================================
    // View Enabling/disabling
    // ===================================================================================================

    private NPVoxAnimationEditorVM viewModel;
    private UnityEditor.Tool previousUnityTool;
    private bool meshRefreshRequested = false;

    public void OnEnable()
    {
        viewModel = (NPVoxAnimationEditorVM)ScriptableObject.CreateInstance(typeof(NPVoxAnimationEditorVM));
        previousUnityTool = Tools.current;
        viewModel.SelectAnimation(((NPVoxAnimationEditorSession)target).animation);
        Undo.undoRedoPerformed += MyUndoCallback;
        viewModel.OnMeshChange += OnMeshChange;
        viewModel.OnCheckForInvalidation += OnCheckForInvalidation;
    }

    public void OnDisable()
    {
        Tools.current = previousUnityTool;
        viewModel.SelectAnimation(null);
        NPVoxAnimationEditorSession session = ((NPVoxAnimationEditorSession)target);
        if (session)
        {
            session.WipeSocketPreviewFilters();
        }
        Undo.undoRedoPerformed -= MyUndoCallback;
        viewModel.OnMeshChange -= OnMeshChange;
        viewModel.OnCheckForInvalidation -= OnCheckForInvalidation;
    }

    private void MyUndoCallback()
    {
        viewModel.OnUndoPerformed();
    }

    private void OnMeshChange()
    {
        meshRefreshRequested = true;
        Repaint();
    }

    private void OnCheckForInvalidation()
    {
        Repaint();
    }

    // ===================================================================================================
    // Material Preview
    // ===================================================================================================

    private void DrawMaterialSelection()
    {
        NPVoxAnimationEditorSession session = ((NPVoxAnimationEditorSession)target);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Preview Materials:");
        if (session.animation && session.animation.PreviewMaterials != null && session.animation.PreviewMaterials.Length < 3)
        {
            session.animation.PreviewMaterials = new Material[3];
        }
        session.animation.PreviewMaterials[0] = EditorGUILayout.ObjectField(session.animation.PreviewMaterials[0], typeof(Material), false, null) as Material;
        session.animation.PreviewMaterials[1] = EditorGUILayout.ObjectField(session.animation.PreviewMaterials[1], typeof(Material), false, null) as Material;
        session.animation.PreviewMaterials[2] = EditorGUILayout.ObjectField(session.animation.PreviewMaterials[2], typeof(Material), false, null) as Material;
        EditorGUILayout.EndHorizontal();

        if (!session.previewFilter)
        {
            return;
        }
        MeshRenderer renderer = session.previewFilter.GetComponent<MeshRenderer>();

        Material[] materials = renderer.sharedMaterials;
        bool somethingChanged = false;
        for (int i = 0; i < 3; i++)
        {
            if (session.animation.PreviewMaterials[i])
            {
                if (materials.Length > i)
                {
                    if (materials[i] != session.animation.PreviewMaterials[i])
                    {
                        materials[i] = session.animation.PreviewMaterials[i];
                        somethingChanged = true;
                    }
                }
                else
                {
                    somethingChanged = true;
                    ArrayUtility.Add(ref materials, session.animation.PreviewMaterials[i]);
                }
            }
        }
        if (somethingChanged)
        {
            renderer.sharedMaterials = materials;
        }
    }
      
    // ===================================================================================================
    // Animation Preview
    // ===================================================================================================

    private double lastFrameTime = 0f;
    private double accumAnimTime = 0f;
    private bool wasPreview;

    private void Preview()
    {
        lastFrameTime = EditorApplication.timeSinceStartup;
        accumAnimTime = 0f;
        viewModel.Preview();
        wasPreview = true;
        // previewCurrentUnityFrame = 0;
    }

    private void DrawPreview()
    {
        if (!viewModel || !viewModel.Animation)
        {
            return;
        }
        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        viewModel.SetPingPong(EditorGUILayout.Toggle("PingPong", viewModel.Animation.PingPong));
        viewModel.SetLoop(EditorGUILayout.Toggle("Loop", viewModel.Animation.Loop));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        viewModel.SetFPS(Mathf.Max(EditorGUILayout.IntField("FPS", viewModel.Animation.FPS), 1));

        if (!viewModel.IsPlaying && NPVoxGUILayout.HotkeyButton("Preview", HOTKEY_PREVIEW))
        {
            Preview();
        }
        if (viewModel.IsPlaying && NPVoxGUILayout.HotkeyButton("Stop", HOTKEY_PREVIEW))
        {
            viewModel.StopPreview();
        }
        if (viewModel.IsPlaying)
        {
            double timeForFrame = (viewModel.SelectedFrame.Duration / (double)viewModel.Animation.FPS);
            double thisFrameTime = EditorApplication.timeSinceStartup;
            accumAnimTime += thisFrameTime - lastFrameTime;
            while(accumAnimTime > timeForFrame && viewModel.IsPlaying)
            {
                accumAnimTime -= timeForFrame;
                viewModel.UpdatePreview();
            }
            lastFrameTime = thisFrameTime;
            Repaint();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    // ===================================================================================================
    // Inspector UI
    // ===================================================================================================

    public override void OnInspectorGUI()
    {
        if (!viewModel.IsPlaying && wasPreview)
        {
            // workaround for stupid "getting control 5" exception
            wasPreview = false;
            Repaint();
            return;
        }

        viewModel.ProcessInvalidations();

        NPVoxAnimationEditorSession session = ((NPVoxAnimationEditorSession)target);

        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Close Editor") || !session.animation)
        {
            NPVoxAnimationEditor.CloseAnimationEditor(session);
        }
        if (GUILayout.Button("Debug"))
        {
            viewModel.DebugButton();
        }
        if (GUILayout.Button("Invalidate & Reimport All"))
        {
            NPipelineUtils.InvalidateAndReimportAll( session.animation );
        }
        if (GUILayout.Button("Invalidate & Reimport All Deep"))
        {
            NPipelineUtils.InvalidateAndReimportAllDeep( session.animation );
        }
        if (GUILayout.Button("Help"))
        {
            NPVoxAnimHelpWindow.ShowWindow();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Label("Preview: ", EditorStyles.boldLabel);
        DrawPreview();

        if (!viewModel.IsPlaying)
        {
            GUILayout.Space(10);

            GUILayout.Label("Frames: ", EditorStyles.boldLabel);

            DrawFrameSelection();
            GUILayout.Space(10);
            DrawModelSelection();
            DrawTransformationSelector();
            GUILayout.Space(10);

            GUILayout.Label("Presentation: ", EditorStyles.boldLabel);
            DrawModeToolbar();
            DrawSocketTools();
        }
        else
        {
//            Debug.Log("playing");
            DrawFrameList();
        }
        DrawMaterialSelection();

        session.groundplateOffset = EditorGUILayout.FloatField("Ground Offset", session.groundplateOffset);
        EditorGUILayout.EndVertical();

        if (meshRefreshRequested)
        {
            meshRefreshRequested = false;
            UpdateMeshes();
            SceneView.RepaintAll();
        }

        // unfocus by pressing escape
        Event e = Event.current;
        if (e.isKey && e.keyCode == KeyCode.Escape)
        {
            unfocus();
        }

        // ground plate
        if(viewModel.SelectedFrame != null && viewModel.SelectedFrame.Source != null && session.groundFilter)
        {
            NPVoxModel model = viewModel.SelectedFrame.Source.GetProduct();
            NPVoxToUnity n2u = new NPVoxToUnity(model, viewModel.Animation.MeshFactory.VoxelSize);
            Vector3 pos = n2u.ToUnityPosition(new Vector3(model.BoundingBox.SaveCenter.x, model.BoundingBox.SaveCenter.y, model.BoundingBox.Forward + 1));
            session.groundFilter.transform.position = pos + Vector3.forward * session.groundplateOffset;
        }

        KeyboardShortcuts();
    }

    private void DrawFrameSelection()
    {
        if (viewModel.Animation == null)
        {
            return;
        }

        NPVoxFrame[] frames = viewModel.Frames;

        GUILayout.BeginHorizontal();

        if (frames.Length == 0)
        {
            if (GUILayout.Button(new GUIContent("++", "Create Frame at Position 0")))
            {
                viewModel.AddFrameAt(0);
            }
        }
        else
        {
            if (NPVoxGUILayout.HotkeyButton(new GUIContent("<", "Select frame " + viewModel.GetPreviousFrameIndex() + "(" + HOTKEY_PREVIOUS_FRAME +")" ), HOTKEY_PREVIOUS_FRAME, false, false, true) )
            {
                viewModel.SelectFrame(viewModel.GetPreviousFrameIndex());
            }
            else if (NPVoxGUILayout.HotkeyButton(new GUIContent(">", "Select frame " + viewModel.GetNextFrameIndex() + "(" + HOTKEY_PREVIOUS_FRAME +")" ), HOTKEY_NEXT_FRAME, false, false, true))
            {
                viewModel.SelectFrame(viewModel.GetNextFrameIndex());
            }

            if (viewModel.SelectedFrameIndex < 0 && frames.Length > 0)
            {
                viewModel.SelectFrame(0, true);
            }
            if (viewModel.SelectedFrameIndex >= frames.Length && frames.Length > 0)
            {
                viewModel.SelectFrame(frames.Length - 1, true);
            }

            string[] labels = new string[frames.Length];
            for (int i = 0; i < frames.Length; i++)
            {
                labels[i] = (i+1)+"";
            }

            int newIndex = -1;
            if (viewModel.SelectedFrameIndex != (newIndex = GUILayout.Toolbar(viewModel.SelectedFrameIndex, labels)))
            {
                viewModel.SelectFrame(newIndex);
            }
                
            DrawFrameCreationTools();
        }

        GUILayout.EndHorizontal();


        // GUILayout.EndArea();
    }

    private void DrawFrameList()
    {
        if (viewModel.Animation == null)
        {
            return;
        }
        NPVoxFrame[] frames = viewModel.Frames;

        GUILayout.BeginHorizontal();
        GUILayout.Label("Frame list:");

        for (int i = 0; i < frames.Length; i++)
        {
            if (viewModel.SelectedFrameIndex == i)
            {
                GUIStyle style = EditorStyles.centeredGreyMiniLabel;
                style.fontSize = 20;
                GUILayout.Label((i + 1) + "", style);
            }
            else
            {
                GUIStyle style = EditorStyles.centeredGreyMiniLabel;
                style.fontSize = 16;
                GUILayout.Label((i + 1) + "", style);
            }
        }
        GUILayout.EndHorizontal();
    }

    private void DrawModelSelection()
    {
        if (viewModel.SelectedFrame == null)
        {
            return;
        }
        viewModel.SelectedFrame.Duration = EditorGUILayout.FloatField( "Duration", viewModel.SelectedFrame.Duration );
        GUILayout.BeginHorizontal();
//        GUILayout.Label("Frame " + ( viewModel.SelectedFrameIndex + 1 ) + " : ");
        // NPVoxModelPipeBase modelFactory = (NPVoxModelPipeBase)EditorGUILayout.ObjectField(viewModel.SelectedFrame.Source, typeof(NPVoxModelPipeBase), false);
        NPVoxIModelFactory modelFactory = (NPVoxIModelFactory) NPipelineUtils.DrawSourceSelector<NPVoxIModelFactory>("Source", viewModel.SelectedFrame.Source );
        if (modelFactory != viewModel.SelectedFrame.Source)
        {
            viewModel.SetFrameModel(viewModel.SelectedFrameIndex, modelFactory);
        }

       

        GUILayout.EndHorizontal();
    }

    private void DrawFrameCreationTools()
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button(new GUIContent("<+", "Add new frame at position " + viewModel.SelectedFrameIndex)))
        {
            viewModel.AddFrameAt(viewModel.SelectedFrameIndex);
        }

        if (GUILayout.Button(new GUIContent("--", "Delete frame at position " + viewModel.SelectedFrameIndex)))
        {
            viewModel.RemoveFrameAt(viewModel.SelectedFrameIndex);
        }

        if (GUILayout.Button(new GUIContent("+>", "Add new frame at position " + (viewModel.SelectedFrameIndex + 1))))
        {
            viewModel.AddFrameAt(viewModel.SelectedFrameIndex + 1);
        }

        if (GUILayout.Button(new GUIContent("<<", "Move Frame " + viewModel.SelectedFrameIndex + " to position " + (viewModel.SelectedFrameIndex > 0 ? (viewModel.SelectedFrameIndex - 1) : 0))) && viewModel.SelectedFrameIndex > 0)
        {
            viewModel.MoveFrame(viewModel.SelectedFrameIndex, -1);
        }

        if (GUILayout.Button(new GUIContent(">>", "Move Frame " + viewModel.SelectedFrameIndex + " to position " + (viewModel.SelectedFrameIndex + 1))) && viewModel.SelectedFrameIndex < viewModel.Frames.Length - 1)
        {
            viewModel.MoveFrame(viewModel.SelectedFrameIndex, +1);
        }

        if (GUILayout.Button(new GUIContent("Copy", "Copy Frame to Clipboard")))
        {
            viewModel.CopyFrame();
        }

        if (viewModel.HasFrameToPaste() && GUILayout.Button(new GUIContent("Paste", "Paste Frame from Clipboard")))
        {
            viewModel.Paste();
        }

        GUILayout.EndHorizontal();
    }

    private void DrawTransformationSelector()
    {
        NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel>[] transformers = viewModel.Transformers;
        if (transformers == null)
        {
            return;
        }
        GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
        GUIStyle foldoutSelectedStyle = new GUIStyle(foldoutStyle);
        GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
        GUIStyle labelSelectedStyle = new GUIStyle(labelStyle);
        GUIStyle deleteStyle = new GUIStyle(EditorStyles.miniButtonMid);
        deleteStyle.fixedWidth = 70;

        foldoutSelectedStyle.fontStyle = FontStyle.Bold;
        labelSelectedStyle.fontStyle = FontStyle.Bold;

        for (int i = 0; i < transformers.Length; i++)
        {
            NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel> transformer = transformers[i];

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            bool isSelected = i == viewModel.SelectedTransformationIndex;
            bool somethingSelected = viewModel.SelectedTransformationIndex != -1;
            bool isSelectedOrLast = isSelected || (!somethingSelected && i == transformers.Length - 1);

            string name = transformer.GetInstanceName();
            if( name == null || name.Length < 1 )
            {
                name = transformer.GetTypeName();
            }

            GUILayout.Label((i + 1) + (isSelectedOrLast ? "|A" : ""), isSelected ? labelSelectedStyle : labelStyle, GUILayout.Width(30));

            if (isSelected != EditorGUILayout.Foldout(isSelected, name, isSelected ? foldoutSelectedStyle : foldoutStyle))
            {
                if (isSelected)
                {
                    viewModel.SelectTransformation(-1);
                    unfocus();
                }
                else
                {
                    viewModel.SelectTransformation(i);
                }
            }

            GUILayout.Space(100);

            if (GUILayout.Button("Delete" + (isSelectedOrLast ? " ("+HOTKEY_DELETE_LAST_TRANSFOMRATION.ToString()+")" : ""),  deleteStyle))
            {
                viewModel.RemoveTransformationAt(i);
            }
            else if (GUILayout.Button("Copy", EditorStyles.miniButtonMid))
            {
                viewModel.CopyTransformation(i);
            }

            if (isSelected)
            {
                
                if (NPVoxGUILayout.HotkeyButton(new GUIContent("Up", "Move Up"), HOTKEY_MOVE_TRANSFORMATION_UP, false, false, false, EditorStyles.miniButtonMid) )
                {
                    viewModel.MoveTransformation(i, -1);
                }

                if( NPVoxGUILayout.HotkeyButton(new GUIContent("Down", "Move Down"), HOTKEY_MOVE_TRANSFORMATION_DOWN, false, false, false, EditorStyles.miniButtonMid) )
                {
                    viewModel.MoveTransformation(i, +1);
                }

                if( NPVoxGUILayout.HotkeyButton(new GUIContent("Duplicate", "Duplicate"), DUPLICATE_TRANSFORMATION, false, false, false, EditorStyles.miniButtonMid) )
                {
                    viewModel.DuplicateTransformation(i);
                }
            }
            else
            {
                if (GUILayout.Button(new GUIContent("Up", "Move Up"), EditorStyles.miniButtonMid))
                {
                    viewModel.MoveTransformation(i, -1);
                }

                if (GUILayout.Button(new GUIContent("Down", "Move Down"), EditorStyles.miniButtonRight))
                {
                    viewModel.MoveTransformation(i, +1);
                }
            }

            GUILayout.EndHorizontal();

            if (isSelected && viewModel.SelectedTransformer != null)
            {
                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.Space();
                    GUILayout.BeginVertical();
                    {
                        EditorGUILayout.Space();

                        if (viewModel.SelectedTransformer is NPVoxISceneEditable)
                        {
                            DrawTransformationToolbar();
                        }
                        {
                            if (viewModel.SelectedTransformer.DrawInspector(~NPipeEditFlags.TOOLS & ~NPipeEditFlags.INPUT & ~NPipeEditFlags.STORAGE_MODE))
                            {
                                GUI.changed = true;
                                viewModel.SelectedTransformerChanged();
                                viewModel.RegeneratePreview();
//                                GUI.changed = true; // TODO: is this still needed?
                            }
                        }

                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        GUILayout.BeginHorizontal();
        if (NPVoxGUILayout.HotkeyButton("+ New T", HOTKEY_APPEND_TRANSFORMATION))
        {
            viewModel.AddTransformation();
        }
        if (viewModel.IsSkeletonBuilderAppendable() && GUILayout.Button("+ Skeleton"))
        {
            viewModel.AddSkeletonBuilder();
        }
        if (viewModel.IsSkeletonTransformerAppendable() && NPVoxGUILayout.HotkeyButton("+ SkeletonT", HOTKEY_APPEND_BONETRANSFORMATION))
        {
            viewModel.AddSkeletonTransformer();
        }
        if (GUILayout.Button("+ Mirror"))
        {
            viewModel.AddMirrorTransformators();
        }
//        if (GUILayout.Button("+ Combiner"))
//        {
//            viewModel.AddSocketCombiner();
//        }
//        if (GUILayout.Button("+ SocketT"))
//        {
//            viewModel.AddSocketTransformer();
//        }
        List<System.Type> allTypes = new List<System.Type>(NPipeReflectionUtil.GetAllTypesWithAttribute(typeof(NPipeAppendableAttribute)));
        List<string> allLabels = new List<string>();
        allLabels.Add("Other ...");
        foreach (System.Type factoryType in allTypes)
        {
            NPipeAppendableAttribute attr = (NPipeAppendableAttribute)factoryType.GetCustomAttributes(typeof(NPipeAppendableAttribute), true)[0];

            if (!attr.sourceType.IsAssignableFrom(typeof(NPVoxIModelFactory)))
            {
                continue;
            }
            allLabels.Add(attr.name);
        }
        int selection = EditorGUILayout.Popup(0, allLabels.ToArray());
        if (selection > 0)
        {
            viewModel.AddTransformer(allTypes[selection - 1]);
        }


        if (viewModel.HasTransformationToPaste() && GUILayout.Button("+ Paste"))
        {
            viewModel.Paste();
        }
        GUILayout.EndHorizontal();
    }

    private void DrawTransformationToolbar()
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label("Tools: ");

        NPVoxAnimationEditorVM.Tool tool = viewModel.CurrentTool;

        if (viewModel.CurrentTool != (tool = NPVoxGUILayout.HotkeyToolbar<NPVoxAnimationEditorVM.Tool>(
            viewModel.GetTools(),
            new NPVoxAnimationEditorVM.Tool[] {
                NPVoxAnimationEditorVM.Tool.CUSTOM1,
                NPVoxAnimationEditorVM.Tool.CUSTOM2,
                NPVoxAnimationEditorVM.Tool.CUSTOM3,
                NPVoxAnimationEditorVM.Tool.CUSTOM4,
                NPVoxAnimationEditorVM.Tool.AREA
            },
            new KeyCode[] { KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.D, KeyCode.T },
            tool
        )))
        {
            viewModel.SetCurrentTool(tool);
            unfocus();
        }

        if (NPVoxGUILayout.HotkeyButton("Reset", HOTKEY_RESET_SELECTED_TRANSFORMATION))
        {
            viewModel.ResetTransformation();
        }

        GUILayout.EndHorizontal();
    }

    private void DrawModeToolbar()
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label("Brightenmod (V): ");
        NPVoxAnimationEditorVM.BrightenMode brightenMode = viewModel.CurrentBrightenMode;
        if (viewModel.CurrentBrightenMode != (brightenMode = NPVoxGUILayout.HotkeyToggleBar<NPVoxAnimationEditorVM.BrightenMode>(
            new string[] { "Off", "Selected" },
            new NPVoxAnimationEditorVM.BrightenMode[] {
                NPVoxAnimationEditorVM.BrightenMode.OFF,
                NPVoxAnimationEditorVM.BrightenMode.SELECTED,
                // NPVoxAnimationEditorVM.BrightenMode.CURRENT
            },
            KeyCode.V,
            brightenMode
        )))
        {
            viewModel.SetCurrentBrightenMode(brightenMode);
        }


        GUILayout.EndHorizontal();
    }


    //======================================================================================================================================
    // Socket Tools
    //======================================================================================================================================

    private void DrawSocketTools()
    {
        EditorGUILayout.BeginVertical();
        if (viewModel.DrawSockets != EditorGUILayout.Toggle("Show Socket Axes In View", viewModel.DrawSockets))
        {
            viewModel.SetDrawSockets(!viewModel.DrawSockets);
        }

        foreach (string targetSocketName in viewModel.GetPreviewTargetSocketNames())
        {
            EditorGUILayout.BeginHorizontal();
            if (viewModel.GetPreviewSocketEnabled(targetSocketName) != EditorGUILayout.Toggle(viewModel.GetPreviewSocketEnabled(targetSocketName)))
            {
                viewModel.SetPreviewSocketEnabled(targetSocketName, !viewModel.GetPreviewSocketEnabled(targetSocketName));
            }
            NPVoxIMeshFactory selectedFactory = viewModel.GetPreviewFactoryForTargetSocket(targetSocketName);
            NPVoxIMeshFactory newSelectedFactory = NPipelineUtils.DrawSourceSelector(targetSocketName, selectedFactory);
            if (selectedFactory != newSelectedFactory)
            {
                viewModel.SetPreviewFactoryForTargetSocket(targetSocketName, newSelectedFactory);
            }

            string[] sourceSockets = viewModel.GetSourceSocketsForTargetSocket(targetSocketName);
            if (sourceSockets == null || sourceSockets.Length == 0)
            {
                GUILayout.Label("No Socket");
            }
            else
            {
                string selectedSourceSocket = viewModel.GetSourceSocketForTargetSocket(targetSocketName);
                string newSelectedSourceSocket = NPipeGUILayout.Popup(sourceSockets, sourceSockets, selectedSourceSocket, true); 
                if (selectedSourceSocket != newSelectedSourceSocket)
                {
                    viewModel.SetSourceSocketForTargetSocket(targetSocketName, newSelectedSourceSocket);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }

    //======================================================================================================================================
    // Scene Editing View
    //======================================================================================================================================

    void OnSceneGUI()
    {
//        if (!meshRefreshRequested)
        {
            if (DrawBoxSelection())
            {
                NPVoxAnimHelpWindow.GetWindow().SetContext(NPVoxAnimHelpWindow.Context.Box);
            }
            else if (DrawBoneSelection())
            {
                NPVoxAnimHelpWindow.GetWindow().SetContext(NPVoxAnimHelpWindow.Context.Bone);
            }
            else if (viewModel.SelectedTransformer != null)
            {
                NPVoxAnimHelpWindow.GetWindow().SetContext(NPVoxAnimHelpWindow.Context.Misc);
            }
            else
            {
                NPVoxAnimHelpWindow.GetWindow().SetContext(NPVoxAnimHelpWindow.Context.None);
            }
        }
        DrawSceneEditTools();
//        if (!meshRefreshRequested)
        {
            DrawSockets(viewModel.SelectedTransformer != null);
        }

        //Return focus back to THIS GameObject to be able to continue processing events
        //This is very important, this part should be outside of the previous condition!!!
        Selection.activeGameObject = ((MonoBehaviour)target).transform.gameObject;

        KeyboardShortcuts();
    }

    private bool DrawBoxSelection()
    {
        if (viewModel.PreviousModelFactory == null)
        {
            return false;
        }
        NPVoxModel previousTransformedModel = viewModel.PreviousModelFactory.GetProduct();
        NPVoxToUnity npVoxToUnity = new NPVoxToUnity(previousTransformedModel, viewModel.Animation.MeshFactory.VoxelSize);

        List<NPVoxBox> boxes = viewModel.GetNonEditableBoxes();
        if(boxes != null)
        {
            foreach (NPVoxBox b in boxes)
                NPVoxHandles.DrawBoxSelection(npVoxToUnity, b, false);
         }

        if (!viewModel.IsAreaSelectionActive())
        {
            return false;
        }


        NPVoxBox box = viewModel.GetAffectedBox();

        if (Event.current.shift)
        {
            // affected area picker
            NPVoxCoord someCoord = box.RoundedCenter;
            NPVoxCoord someNewCoord = NPVoxHandles.VoxelPicker(new NPVoxToUnity(previousTransformedModel, viewModel.Animation.MeshFactory.VoxelSize), someCoord, 0, ((NPVoxAnimationEditorSession)target).previewFilter.transform);
            if (!someCoord.Equals(someNewCoord))
            {
                viewModel.ChangeAffectedBox(new NPVoxBox(someNewCoord, someNewCoord));
            }
        }
        else
        {
            // affected area box
            NPVoxBox newBox = NPVoxHandles.DrawBoxSelection(npVoxToUnity, box);
            if (!newBox.Equals(box))
            {
                viewModel.ChangeAffectedBox(newBox);
            }
        }

        return true;
    }

    private bool DrawBoneSelection()
    {
        if (viewModel.PreviousModelFactory == null)
        {
            return false;
        }
        NPVoxBoneModel previewModel = viewModel.EditorModelFactory.GetProduct() as NPVoxBoneModel;

        if (previewModel == null)
        {
            return false;
        }

        if (!viewModel.IsBoneSelectionActive())
        {
            return false;
        }

        NPVoxToUnity npVoxToUnity = new NPVoxToUnity(previewModel, viewModel.Animation.MeshFactory.VoxelSize);

        // affected area picker
        if (Event.current.isMouse && Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
//            NPVoxCoord someCoord = NPVoxCoord.INVALID;
            float mouseScale = NPVoxHandles.GetMouseScale(SceneView.currentDrawingSceneView.camera);
            Camera cam = SceneView.currentDrawingSceneView.camera;
            Ray r = SceneView.currentDrawingSceneView.camera.ScreenPointToRay(
                new Vector3(Event.current.mousePosition.x * mouseScale, -Event.current.mousePosition.y * mouseScale + Camera.current.pixelHeight)
            );
            NPVoxRayCastHit raycastHit = npVoxToUnity.Raycast(r, ((NPVoxAnimationEditorSession)target).previewFilter.transform, 20);
            if (raycastHit.IsHit)
            {
                uint boneMask = previewModel.GetBoneMask(raycastHit.Coord);
                if (boneMask != 0)
                {
                    if (Event.current.control || Event.current.command)
                    {
                        viewModel.ToggleBoneMask(boneMask, Event.current.shift);
                    }
                    else
                    {
                        viewModel.SetBoneMask(boneMask, Event.current.shift);
                    }
                }
            }
        }
        return true;
    }

    private void DrawSceneEditTools()
    {
        if (!viewModel.IsSceneEditToolsActive())
        {
            return;
        }

        NPVoxToUnity npVoxToUnity = new NPVoxToUnity(viewModel.PreviousModelFactory.GetProduct(), viewModel.Animation.MeshFactory.VoxelSize);

        if (!(viewModel.SelectedTransformer is NPVoxISceneEditable))
        {
            return;
        }

        System.Func<NPVoxISceneEditable, bool> applyFunction = 
            ((NPVoxISceneEditable)viewModel.SelectedTransformer).DrawSceneTool(npVoxToUnity, ((NPVoxAnimationEditorSession)target).previewFilter.transform, viewModel.CurrentCustomTool);

        if (applyFunction != null)
        {
            viewModel.ChangeTransformation(applyFunction);
        }
    }
   
    private void DrawSockets(bool isEditing)
    {
        NPVoxIModelFactory modelFactory = viewModel.EditorModelFactory;

        if (!viewModel.DrawSockets)
        {
            return;
        }

        if (modelFactory != null)
        {
            NPVoxModel model = modelFactory.GetProduct();
            NPVoxToUnity npVoxToUnity = new NPVoxToUnity(model, viewModel.Animation.MeshFactory.VoxelSize);

            if (model)
            {
                foreach (NPVoxSocket socket in model.Sockets)
                {
                    Vector3 anchorPos = npVoxToUnity.ToUnityPosition(socket.Anchor);
                    Quaternion rotation = Quaternion.Euler(socket.EulerAngles);
                    Vector3 anchorRight = npVoxToUnity.ToUnityDirection(rotation * Vector3.right);
                    Vector3 anchorUp = npVoxToUnity.ToUnityDirection(rotation  * Vector3.up);
                    Vector3 anchorForward = npVoxToUnity.ToUnityDirection(rotation  * Vector3.forward);

                    Vector3 position = Vector3.zero;
                    float size = 2.5f;
                    if (!isEditing)
                    {
                        Handles.color = new Color(0.5f, 1.0f, 0.1f, 0.5f);
                        Handles.CubeCap(0, position + anchorPos, rotation,  0.125f);
                        size = 10.0f;
                    }
                    Handles.color = Color.red;
                    Handles.DrawLine(position + anchorPos, position + anchorPos + anchorRight * size);
                    Handles.color = Color.green;
                    Handles.DrawLine(position + anchorPos, position + anchorPos + anchorUp * size);
                    Handles.color = Color.blue;
                    Handles.DrawLine(position + anchorPos, position + anchorPos + anchorForward * size);
                }
            }
        }
    }


    private void KeyboardShortcuts()
    {
        if (Tools.current != Tool.None)
        {
            if (viewModel.UpdateCurrentToolFromSceneView(Tools.current))
            {
            }
        }

        Event e = Event.current;

        if (e.type == EventType.keyDown && e.isKey)
        {
            if (viewModel.SelectedTransformer == null)
            {
                // Start Editing Transformation
                if (HOTKEY_EDIT_LAST_TRANSFOMRATION.IsDown( e ))
                {
                    if (viewModel.Transformers != null && viewModel.Transformers.Length > 0)
                    {
                        e.Use();
                        viewModel.SelectTransformation(viewModel.Transformers.Length - 1);
                    }
                }
            }
            else
            {
                // Maximize Affected Area
                if (HOTKEY_AFFECTED_AREA_SELECT_ALL.IsDown(e))
                {
                    e.Use();
                    viewModel.MaximizeAffectedArea();
                }
                // Reset current Editing Transformation
                if (HOTKEY_RESET_SELECTED_TRANSFORMATION.IsDown(e))
                {
                    e.Use();
                    viewModel.ResetTransformation();
                }

                // Stop Editing Transformation
                if (HOTKEY_EDIT_LAST_TRANSFOMRATION.IsDown( e ) && !e.alt)
                {
                    e.Use();
                    viewModel.SelectTransformation(-1);
                    unfocus();
                }

                // Select Pivot Tool
                if (e.keyCode == KeyCode.D)
                {
                    e.Use();
                    viewModel.SetCurrentTool(NPVoxAnimationEditorVM.Tool.CUSTOM4);
                }

                // move selected up
                if (HOTKEY_MOVE_TRANSFORMATION_UP.IsDown(e))
                {
                    e.Use();
                    viewModel.MoveTransformation(viewModel.SelectedTransformationIndex, -1);
                }

                // move selected down
                if (HOTKEY_MOVE_TRANSFORMATION_DOWN.IsDown(e))
                {
                    e.Use();
                    viewModel.MoveTransformation(viewModel.SelectedTransformationIndex, +1);
                }
            }

            // Add new Transformation
            if (HOTKEY_APPEND_TRANSFORMATION.IsDown(e))
            {
                e.Use();
                viewModel.AddTransformation();
            }

            // Add new Transformation
            if (HOTKEY_APPEND_BONETRANSFORMATION.IsDown(e))
            {
                e.Use();
                viewModel.AddSkeletonTransformer();
            }

            // Delete Transformation
            if (HOTKEY_DELETE_LAST_TRANSFOMRATION.IsDown(e))
            {
                if (viewModel.SelectedTransformationIndex >= 0)
                {
                    e.Use();
                    viewModel.RemoveTransformationAt(viewModel.SelectedTransformationIndex);
                }
                else if (viewModel.Transformers.Length > 0)
                {
                    e.Use();
                    viewModel.RemoveTransformationAt(viewModel.Transformers.Length - 1);
                }
            }

            if (HOTKEY_PREVIOUS_FRAME.IsDown(e))
            {
                e.Use();
                viewModel.SelectFrame(viewModel.GetPreviousFrameIndex());
            }

            if (HOTKEY_NEXT_FRAME.IsDown(e))
            {
                e.Use();
                viewModel.SelectFrame(viewModel.GetNextFrameIndex());
            }

            if (HOTKEY_PREVIOUS_TRANSFORMATION.IsDown(e))
            {
                e.Use();
                viewModel.SelectTransformation(viewModel.GetPreviousTransformationIndex());
            }

            if (HOTKEY_NEXT_TRANSFORMATION.IsDown(e))
            {
                e.Use();
                viewModel.SelectTransformation(viewModel.GetNextTransformationIndex());
            }

            if (HOTKEY_HIDE_SELECTED_BONES.IsDown(e))
            {
                e.Use();
                viewModel.ApplyHiddenBoneMask();
            }

            for(int i = 0; i < HOTKEYS_SELECT_FRAME.Length; i++)
            {
                if (HOTKEYS_SELECT_FRAME[i].IsDown(e))
                {
                    e.Use();
                    viewModel.SelectFrame(i);
                    unfocus();
                }
            }


            for(int i = 0; i < HOTKEYS_SELECT_TRANSFORMATION.Length; i++)
            {
                if (HOTKEYS_SELECT_TRANSFORMATION[i].IsDown(e))
                {
                    e.Use();
                    if (viewModel.SelectedTransformationIndex == i)
                    {
                        unfocus();
                        viewModel.SelectTransformation(-1);
                    }
                    else
                    {
                        viewModel.SelectTransformation(i);
                    }
                }
            }

            // preview
            if (HOTKEY_PREVIEW.IsDown(e))
            {
                e.Use();
                if (viewModel.IsPlaying)
                {
                    viewModel.StopPreview();
                }
                else
                {
                    Preview();
                }
            }
        }
    }

    private void unfocus()
    {
        EditorGUIUtility.keyboardControl = 0;
    }

    private void UpdateMeshes()
    {
        NPVoxAnimationEditorSession session = ((NPVoxAnimationEditorSession)target);
        if (session != null)
        {
            session.previewFilter.sharedMesh = viewModel.Mesh;

            foreach (string socketName in viewModel.GetSocketPreviewTargetNames())
            {
                if (!viewModel.GetPreviewSocketEnabled(socketName))
                {
                    session.GetSocketPreviewFilter(socketName).gameObject.SetActive(false);
                }
                else
                {
                    var factory = viewModel.GetSocketPreviewMeshFactoryForCurrentFrame(socketName);
                    if (factory != null)
                    {
                        session.GetSocketPreviewFilter(socketName).gameObject.SetActive(true);
                        session.GetSocketPreviewFilter(socketName).sharedMesh = factory.GetProduct();
                    }
                    else
                    {
                        session.GetSocketPreviewFilter(socketName).gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}