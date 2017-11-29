using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(NPVoxAnimation))]
public class NPVoxAnimationEditor : Editor
{
    private string selectedTargetSocket = null;
    private string selectedInputSocket = null;
    private NPVoxIModelFactory selectedModelFactory = null;

    public override void OnInspectorGUI()
    {
        GUIStyle boldStyle = new GUIStyle();
        boldStyle.fontStyle = FontStyle.Bold;

        NPVoxAnimation animation = (NPVoxAnimation)target;
        GUILayout.Label("NPVox Animation: " + animation.name);
        
        GUILayout.Label("Tools:");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Invalidate & Reimport All"))
        {
            NPipelineUtils.InvalidateAndReimportAll( animation );
        }
        if (GUILayout.Button("Invalidate & Reimport All Deep"))
        {
            NPipelineUtils.InvalidateAndReimportAllDeep( animation );
        }
        GUILayout.EndHorizontal();
        
        if (GUILayout.Button("Edit Animation"))
        {
            OpenAnimationScene(animation);
        }

        if (animation.Frames != null && animation.Frames.Length > 0 && animation.Frames[0].Source != null)
        {
            NPVoxModel model = animation.Frames[0].Source.GetProduct();

            if(model != null && model.SocketNames.Length > 0)
            {
                EditorGUILayout.BeginHorizontal();
                selectedTargetSocket = NPipeGUILayout.Popup(model.SocketNames, model.SocketNames, selectedTargetSocket, true);
                selectedModelFactory = NPipelineUtils.DrawSourceSelector<NPVoxIModelFactory>("Input:", selectedModelFactory);

                if (selectedModelFactory != null && selectedModelFactory.GetProduct())
                {
                    selectedInputSocket = NPipeGUILayout.Popup(selectedModelFactory.GetProduct().SocketNames, selectedModelFactory.GetProduct().SocketNames, selectedInputSocket, true);
                }

                if (GUILayout.Button("Create Slave Animation") && selectedModelFactory != null)
                {
                    createSlaveAnimation(animation, selectedTargetSocket, selectedModelFactory, selectedInputSocket);
                }

                if (GUILayout.Button("Create Slave Animation From Preview") && selectedModelFactory != null)
                {
                    createSlaveAnimationFromPreview(animation, selectedModelFactory);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUILayout.Label("Animation Default Settings", boldStyle);
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        // Mesh Output Settings

        GUILayout.Label("Mesh Output Settings", boldStyle); 
        if(animation.MeshFactory.DrawInspector(~NPipeEditFlags.TOOLS & ~NPipeEditFlags.INPUT))
        {
            // Cascade to all frames
            foreach(NPVoxFrame frame in animation.Frames)
            {
                frame.Output.BloodColorIndex = animation.MeshFactory.BloodColorIndex;
                frame.Output.Cutout = animation.MeshFactory.Cutout;
                frame.Output.Loop = animation.MeshFactory.Loop;
                frame.Output.NormalMode = animation.MeshFactory.NormalMode;
                frame.Output.NormalVariance = animation.MeshFactory.NormalVariance;
                frame.Output.NormalVarianceSeed = animation.MeshFactory.NormalVarianceSeed;
                frame.Output.VoxelSize = animation.MeshFactory.VoxelSize;
                frame.Output.StorageMode = animation.MeshFactory.StorageMode;
                frame.Output.MinVertexGroups = animation.MeshFactory.MinVertexGroups;
                frame.Output.NormalModePerVoxelGroup = animation.MeshFactory.NormalModePerVoxelGroup;
            }
        }


        // Destroy unconnected things

        NPipeIImportable[] importables = NPipelineUtils.GetImportables(AssetDatabase.GetAssetPath(animation));
        foreach(NPipeIImportable importable in importables)
        {
            NPipeIImportable prev = NPipelineUtils.FindPreviousOfType<NPVoxIModelFactory>(importable);
            if ((importable as NPVoxMeshOutput) != animation.MeshFactory && (prev == null || prev == importable))
            {
                Debug.LogWarning("Destroying orphaning importable: " + importable);
                if (importable is  NPipeIComposite )
                {
                    ((NPipeIComposite)importable).Input = null;
                }
                importable.Destroy(); // destroy the product
                Undo.DestroyObjectImmediate((UnityEngine.Object)importable);
            }
        }
    }

    public static void OpenAnimationScene(NPVoxAnimation animation)
    {
        bool proceed = true;
        UnityEngine.SceneManagement.Scene previousScene = EditorSceneManager.GetActiveScene();

//        GameObject activeSession = GameObject.Find("~NPVoxAnimEditorSession");
//        if(activeSession)
//        {
//            CloseAnimationEditor(activeSession.GetComponent<NPVoxAnimationEditorSession>());
//        }

        if (previousScene.isDirty)
        {
            proceed = false;
            if (EditorUtility.DisplayDialog("Unsaved Changes", "You need to save any changes to your active scene before you can edit NPVox Animations", "Save Now", "Abort"))
            {
                proceed = true;
                EditorSceneManager.SaveScene(previousScene);
            }
        }

        if (proceed)
        {
            UnityEngine.SceneManagement.Scene editorScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            GameObject gameObject = new GameObject();
            gameObject.name = "~NPVoxAnimEditorSession";
            
            NPVoxAnimationEditorSession session = gameObject.AddComponent<NPVoxAnimationEditorSession>();
            session.animation = animation;
            session.previousScene = previousScene;
            session.previousScenePath = previousScene.path;
            EditorSceneManager.CloseScene(previousScene, false);
            
            session.editorScenePath = FileUtil.GetUniqueTempPathInProject () + ".unity";
            EditorSceneManager.SaveScene(editorScene,session.editorScenePath , false);
            session.editorScene = editorScene;
            gameObject.transform.position = Vector3.zero;
            Selection.objects = new Object[] { gameObject };
            
            RenderSettings.skybox = null;
            
            // add light
            GameObject lightGameObject = new GameObject("The Light");
            Light lightComp = lightGameObject.AddComponent<Light>();
            lightComp.color = new Color(1.0f,1.0f,0.9f);
            lightComp.type = LightType.Directional;
            lightComp.intensity = 0.5f;
            lightGameObject.transform.rotation = Quaternion.Euler(300, 0, 235);
            lightGameObject.transform.position = Vector3.one * 1000f;
            
            // add preview
            gameObject = new GameObject();
            gameObject.name = "Preview";
            session.previewFilter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();

            // TODO allow arbitrary number of materials
            Material mat = (Material) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath("45ff6773d76864cb0b24978a7bced992"), typeof(Material));
            renderer.sharedMaterials = new Material[] {
                mat, mat, mat, mat, mat, mat
            }; 

            // Add ground plate
            gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject.name = "GroundPlate";
            gameObject.transform.localScale = new Vector3(10, 10, 0.125f);
            gameObject.transform.position = new Vector3(0, 0, 2.335f);
            session.groundFilter = gameObject.GetComponent<MeshFilter>();
            renderer = gameObject.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = (Material)AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath("8ad74ef8a5b1547aba7e742c4650e508"));
//            renderer.material.SetColor("_Albedo", Color.red);
        }
    }

    public static void CloseAnimationEditor(NPVoxAnimationEditorSession session)
    {
        NPVoxAnimation animation = session.animation;
        string previousScenePath = session.previousScenePath;
        string editorScenePath = session.editorScenePath;
        UnityEngine.SceneManagement.Scene editorScene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(editorScenePath);

        GameObject.DestroyImmediate(session.gameObject);

        if (previousScenePath != null)
        {
            EditorSceneManager.OpenScene(previousScenePath, OpenSceneMode.Additive);
        }
        else
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        }
        EditorSceneManager.CloseScene(editorScene, true);

        File.Delete(editorScenePath);

        animation.EnsureAllMeshesLoaded();

        Selection.objects = new Object[] { animation };
    }

    private void createSlaveAnimation(NPVoxAnimation animation, string targetSocket, NPVoxIModelFactory inputModelFactory, string inputSocket)
    {
        var path = NPipelineUtils.GetCreateScriptableObjectAssetPath<NPVoxAnimation>( AssetDatabase.LoadMainAssetAtPath( AssetDatabase.GetAssetPath( inputModelFactory as Object)).name + "_" + targetSocket + "_" + animation.name);
        if (path.Length != 0)
        {
            NPVoxAnimation slaveAnimation = NPipelineUtils.CreatePipeContainer<NPVoxAnimation>(path);
            foreach (NPVoxFrame sourceFrame in animation.Frames)
            {
                NPVoxFrame targetFrame = slaveAnimation.AppendFrame();
                targetFrame.Source = inputModelFactory;
                NPVoxModelSocketCombiner combiner = NPVoxModelSocketCombiner.CreateInstance<NPVoxModelSocketCombiner>();

                if (!sourceFrame.PreOutput)
                {
                    sourceFrame.FixStuff();
                }
                combiner.Target = sourceFrame.PreOutput;
                combiner.InputSocketName = inputSocket;
                combiner.TargetSocketName = targetSocket;
                targetFrame.AppendTransformer(combiner);
            }

            Selection.objects = new Object[] { slaveAnimation };
        }
    }

    private void createSlaveAnimationFromPreview(NPVoxAnimation animation, NPVoxIModelFactory inputModelFactory)
    {
        var path = NPipelineUtils.GetCreateScriptableObjectAssetPath<NPVoxAnimation>( AssetDatabase.LoadMainAssetAtPath( AssetDatabase.GetAssetPath( inputModelFactory as Object)).name + "_" + animation.name);
        if (path.Length != 0)
        {
            NPVoxAnimation slaveAnimation = NPipelineUtils.CreatePipeContainer<NPVoxAnimation>(path);
            foreach (NPVoxFrame sourceFrame in animation.Frames)
            {
                NPVoxFrame targetFrame = slaveAnimation.AppendFrame();

                NPVoxSocketAttachment attachment = sourceFrame.GetPreviewAttachmentForFactory(inputModelFactory);
                if (attachment == null)
                {
                    Debug.LogWarning("no attachment found for the given model factory");
                    continue;
                }

                targetFrame.Source = inputModelFactory;
                NPVoxModelSocketCombiner combiner = NPVoxModelSocketCombiner.CreateInstance<NPVoxModelSocketCombiner>();

                if (!sourceFrame.PreOutput)
                {
                    sourceFrame.FixStuff();
                }
                combiner.Target = sourceFrame.PreOutput;
                combiner.InputSocketName = attachment.sourceSocketName;
                combiner.TargetSocketName = attachment.targetSocketName;
                targetFrame.AppendTransformer(combiner);
            }

            Selection.objects = new Object[] { slaveAnimation };
        }
    }
}
