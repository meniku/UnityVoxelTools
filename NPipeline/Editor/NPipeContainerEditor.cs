using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(NPipeContainer))]
public class NPipeContainerEditor : Editor
{
    private NPipeIImportable editingImportable = null;

    private int selectedAppendIndex = 0;

    private bool confirmDeletion = false;

    private GUIStyle normalStyle;
    private GUIStyle boldStyle;
    private  Color thisContainerColor;

    public override void OnInspectorGUI()
    {
        DrawPipelineElements();

    }
    protected bool DrawPipelineElements()
    {
        // setup colors
        normalStyle = new GUIStyle( (GUIStyle)"helpbox");
        normalStyle.normal.textColor = Color.black;
        normalStyle.fontStyle = FontStyle.Normal;
        boldStyle = new GUIStyle( (GUIStyle)"helpbox" );
        boldStyle.normal.textColor = Color.black;
        boldStyle.fontStyle = FontStyle.Bold;
        thisContainerColor = new Color(0.8f, 1.0f, 0.6f);

        string assetPath = AssetDatabase.GetAssetPath(target);
        NPipeIImportable[] allImportables = NPipelineUtils.GetByType<NPipeIImportable>(target);
        NPipeIImportable[] outputPipes = NPipelineUtils.FindOutputPipes(allImportables);

        // GUILayout.Label("Tools:");

//        assetPathColors.Clear();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Select Me"))
        {
            Selection.activeObject = target;
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Invalidate & Reimport All"))
        {
            NPipelineUtils.InvalidateAndReimportAll( target );
        }
        if (GUILayout.Button("Invalidate & Reimport All Deep"))
        {
            NPipelineUtils.InvalidateAndReimportAllDeep( target );
        }
        GUILayout.EndHorizontal();

        GUILayout.EndHorizontal();


        if (GUILayout.Button("Delete all payload"))
        {
            UnityEngine.Object[] allObjects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (UnityEngine.Object obj in allObjects)
            {
                if (!(obj is NPipeIImportable) && !(obj is NPipeContainer))
                {
                    DestroyImmediate(obj, true);
                }
            }
            EditorUtility.SetDirty(target);
        }

        foreach (NPipeIImportable imp in allImportables)
        {
            if (imp is NPipeIInstantiable && GUILayout.Button("Instantiate " + imp.GetInstanceName() + " (" + imp.GetTypeName() +")"))
            {
                ((NPipeIInstantiable)imp).Instatiate();
            }
        }

        {
            GUILayout.Space(10f);

            // headline
            GUILayout.Label(string.Format("Pipelines", allImportables.Length), EditorStyles.boldLabel);
//            GUIStyle my = new GUIStyle(EditorStyles.boldLabel);
//            my.normal.textColor = thisContainerColor;
//            GUILayout.Label(string.Format("({0} elements in this container)", allImportables.Length), my);

            // pipelines
            GUILayout.BeginHorizontal();
            HashSet<NPipeIImportable> visited = new HashSet<NPipeIImportable>();
            foreach (NPipeIImportable iimp in outputPipes)
            {
                DrawPipelineElements(assetPath, iimp, visited, false);
            }

            GUI.backgroundColor = thisContainerColor;

            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Add: ");
                {
                    GUILayout.BeginVertical();
                    foreach (Type factoryType in NPipeReflectionUtil.GetAllTypesWithAttribute(typeof(NPipeStartingAttribute)))
                    {
                        NPipeStartingAttribute attr = (NPipeStartingAttribute)factoryType.GetCustomAttributes(typeof(NPipeStartingAttribute), true)[0];
                        if (attr.attached && GUILayout.Button(attr.name))
                        {
                            NPipelineUtils.CreateAttachedPipe(assetPath, factoryType);
                        }
                    }

                    List<System.Type> allTypes = new List<System.Type>(NPipeReflectionUtil.GetAllTypesWithAttribute(typeof(NPipeAppendableAttribute)));
                    List<string> allLabels = new List<string>();
                    allLabels.Add("Other ...");
                    foreach (Type factoryType in allTypes)
                    {
                        NPipeAppendableAttribute attr = (NPipeAppendableAttribute)factoryType.GetCustomAttributes(typeof(NPipeAppendableAttribute), true)[0];
                        allLabels.Add(attr.name);
                    }
                    int selection = EditorGUILayout.Popup(0, allLabels.ToArray());
                    if (selection > 0)
                    {
                        NPipelineUtils.CreateAttachedPipe(assetPath, allTypes[selection - 1]);
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();
        }

        drawPipeEditor( assetPath );

        return false;
    }


    protected string DrawPipelineElements(string assetPath, NPipeIImportable importable, HashSet<NPipeIImportable> visited, bool hasNext, string nextAssetPath = "")
    {
        string thisAssetPath = AssetDatabase.GetAssetPath(importable as UnityEngine.Object);
        bool thisIsInContainer = thisAssetPath == assetPath;
        bool nextIsInContainer = nextAssetPath == assetPath;

        GUILayout.BeginVertical();
        NPipeIComposite composite = importable as NPipeIComposite;
        bool isSource = true;
        bool parentIsInDifferentAsset = false;
        if (composite != null)
        {
            NPipeIImportable[] sources = composite.GetAllInputs();
            GUILayout.BeginHorizontal();
            if (sources != null)
            {
                foreach (NPipeIImportable source in sources)
                {
                    isSource = false;
                    if (source != null)
                    {
                        if (thisAssetPath != DrawPipelineElements(assetPath, source, visited, true, thisAssetPath))
                        {
                            parentIsInDifferentAsset = true;
                        }
                    }
                    else
                    {
                        GUILayout.Label("NULL !");
                    }
                }
            }

            GUILayout.EndHorizontal();
        }

        if ((!thisIsInContainer && parentIsInDifferentAsset) || (isSource && !thisIsInContainer && thisAssetPath != null))
        {
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = GetColorForAssetPath(thisAssetPath);
            EditorGUILayout.ObjectField(AssetDatabase.LoadMainAssetAtPath(thisAssetPath), typeof(NPipeContainer), false );
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();

        if (visited.Contains(importable))
        {
            GUILayout.Label(importable.GetTypeName());
        }
        else
        {

            visited.Add(importable);
            if (!thisIsInContainer)
            {
                GUI.backgroundColor = GetColorForAssetPath(thisAssetPath);
            }
            else
            {
                GUI.backgroundColor = thisContainerColor;
            }

//            smallTextStyle.font = EditorStyles.miniFont;
            if (editingImportable == importable)
            {
                if (GUILayout.Button("Close", GUILayout.Width(40)))
                {
                    editingImportable = null;
                }

                if (!confirmDeletion && GUILayout.Button("Delete"))
                {
                    confirmDeletion = true;
                }
                else if (confirmDeletion && GUILayout.Button("Sure?"))
                {
                    editingImportable = null;
                    Delete(assetPath, importable);
                    AssetDatabase.SaveAssets();
                }
            }
            else
            {
                if (thisIsInContainer && GUILayout.Button("Edit", GUILayout.Width(40)))
                {
                    editingImportable = importable;
                    confirmDeletion = false;
                }
            }


            GUIStyle style = normalStyle;
            if (editingImportable == importable)
            {
                style = boldStyle;
            }
            {
                
                string n = ((NPipeContainer)target).GetDisplayName( importable );
                GUILayout.Label(n, style); 
            }
        }
        GUILayout.EndHorizontal();

        if (hasNext)
        {
            if (thisIsInContainer || nextIsInContainer)
            {
                DrawArrow(thisContainerColor);
            }
            else if (nextAssetPath != thisAssetPath)
            {
                DrawArrow(GetColorForAssetPath(nextAssetPath));
            }
        }

        GUI.backgroundColor = Color.white;

        GUILayout.EndVertical();

        return thisAssetPath;
    }

    private void DrawArrow(Color color)
    {

        Rect rect = GUILayoutUtility.GetLastRect();

        // Draw the lines
        Handles.BeginGUI();

        Handles.color = color;

        float width1 = 2.0f;
        float width2 = 10.0f;
        float height1 = 10.0f;
        float height2 = 12.0f;
        Vector2 anchor = new Vector2(rect.xMin + (rect.xMax - rect.xMin) / 2f, rect.yMax + 4f);

        Vector2 p1 = anchor + Vector2.left * width1;
        Vector2 p2 = anchor + Vector2.right * width1;
        Vector2 p3 = p1 + Vector2.up * height1;
        Vector2 p4 = p2 + Vector2.up * height1;
        Handles.DrawAAConvexPolygon(new Vector3[]{ p1, p3, p4, p2 });

        Vector2 q1 = anchor + Vector2.left * width2 + Vector2.up * height1;
        Vector2 q2 = anchor + Vector2.right * width2 + Vector2.up * height1;
        Vector2 q3 = anchor + Vector2.up * (height1+height2);
        Handles.DrawAAConvexPolygon(new Vector3[]{ q1, q2, q3 });

        Handles.EndGUI();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }

    private static Dictionary<string, Color> assetPathColors = new Dictionary<string, Color>();

    private Color GetColorForAssetPath(string path)
    {
        if (!assetPathColors.ContainsKey(path))
        {
            assetPathColors.Add(path, UnityEngine.Random.ColorHSV( 0.0f, 1.0f, 0.2f, 0.2f, 0.7f, 0.7f, 1, 1));
        }
        return assetPathColors[path];
    }

    protected void Delete(string path, NPipeIImportable importable)
    {
        editingImportable = null;

        importable.Destroy();
        Undo.DestroyObjectImmediate(importable as UnityEngine.Object);

        AssetDatabase.Refresh();
    }

    private void drawPipeEditor( string assetPath )
    {

        NPipeIImportable importable = editingImportable as NPipeIImportable;
        if (importable != null)
        {
            GUILayout.Space(10f);
            GUILayout.Label("Selected: " + editingImportable.GetTypeName(), EditorStyles.boldLabel);

            NPipeIEditable editable = editingImportable as NPipeIEditable;
            if (editable != null)
            {
                GUILayout.Label("Edit:");
                editable.DrawInspector(~NPipeEditFlags.TOOLS);
            }

            GUILayout.Space(10f);
            GUILayout.BeginVertical();
            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();
            List<System.Type> allTypes = new List<System.Type>();
            List<NPipeAppendableAttribute> allAttrsa = new List<NPipeAppendableAttribute>();
            List<string> allLabels = new List<string>();
            foreach (Type factoryType in NPipeReflectionUtil.GetAllTypesWithAttribute(typeof(NPipeAppendableAttribute)))
            {
                NPipeAppendableAttribute attr = (NPipeAppendableAttribute)factoryType.GetCustomAttributes(typeof(NPipeAppendableAttribute), true)[0];
                if (!attr.sourceType.IsAssignableFrom(importable.GetType()))
                {
                    continue;
                }
                allTypes.Add(factoryType);
                allAttrsa.Add(attr);
                allLabels.Add(attr.name);
            }
            if (allTypes.Count > 0)
            {
                GUILayout.Label("Append: ");
                selectedAppendIndex = EditorGUILayout.Popup(selectedAppendIndex, allLabels.ToArray());
                if (selectedAppendIndex >= 0 && selectedAppendIndex < allTypes.Count)
                {
                    NPipeAppendableAttribute pipe = allAttrsa[selectedAppendIndex];
                    NPipeIComposite newImportable = null;

                    if (pipe.attached && GUILayout.Button("This Container"))
                    {
                        newImportable = NPipelineUtils.CreateAttachedPipe(assetPath, allTypes[selectedAppendIndex], importable) as NPipeIComposite;
                        editingImportable = newImportable;
                        confirmDeletion = false;
                    }
                    if (pipe.separate && GUILayout.Button("New Container"))
                    {
                        newImportable = NPipelineUtils.CreateSeparatedPipe(assetPath, allTypes[selectedAppendIndex], importable) as NPipeIComposite;
                    }

                    if (newImportable != null)
                    {
                        AssetDatabase.SaveAssets();
                        UnityEditor.Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(newImportable as UnityEngine.Object));
                    }
                }
                else
                {
                    selectedAppendIndex = 0;
                }
            }
            GUILayout.EndHorizontal();


            GUILayout.EndVertical();
        }
    }
}