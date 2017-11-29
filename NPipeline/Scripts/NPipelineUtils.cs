#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

public class NPipelineUtils
{
    public static T[] GetTypedFactories<T>(UnityEngine.Object[] objects) where T : class
    {
        List<T> result = new List<T>();
        foreach (UnityEngine.Object item in objects)
        {
            if (item is T)
            {
                result.Add(item as T);
            }
        }
        return result.ToArray();
    }

    public static void InvalidateAndReimportAll(UnityEngine.Object container)
    {
        NPipeIImportable[] allImportables = GetByType<NPipeIImportable>(container);
        InvalidateAll(allImportables);
        EditorUtility.SetDirty(container as UnityEngine.Object);
        AssetDatabase.SaveAssets();
    }

    public static void InvalidateAll(NPipeIImportable[] allImportables, bool deep = false)
    {
        foreach (NPipeIImportable imp in allImportables)
        {
            if(imp != null && ((UnityEngine.Object)imp))
            {
                imp.Invalidate(deep);
            }
        }
    }

    public static bool AreSourcesReady(NPipeIImportable importable)
    {
        // TODO find a more viable way
        foreach (NPipeIImportable source in EachSource(importable))
        {
            if (AreSourcesReady(source))
            {
                return true;
            }
        }
        if (importable is NPVoxMagickaSource)
        {
            return true;
        }
        return false;
    }
    public static void InvalidateAndReimportAllDeep(UnityEngine.Object container)
    {
        NPipeIImportable[] allImportables = FindOutputPipes(GetByType<NPipeIImportable>(container));
        InvalidateAll(allImportables, true);
        EditorUtility.SetDirty(container as UnityEngine.Object);
        AssetDatabase.SaveAssets();
    }


    public static void InvalidateAndReimportDeep(NPipeIImportable output)
    {
        output.Invalidate(true);
        EditorUtility.SetDirty(output as UnityEngine.Object);
        AssetDatabase.SaveAssets();
    }

    public static NPipeIImportable[] GetImportables(NPipeContainer container)
    {
        return GetImportables(AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(container)));
    }

    public static NPipeIImportable[] GetImportables(string path)
    {
        return GetImportables(AssetDatabase.LoadAllAssetsAtPath(path));
    }

    public static NPipeIImportable[] GetImportables(UnityEngine.Object[] objects)
    {
        return GetByType<NPipeIImportable>(objects);
    }

    public static NPipeContainer GetContainer(UnityEngine.Object[] objects)
    {
        NPipeContainer[] container = GetByType<NPipeContainer>(objects);
        if (container.Length > 0)
        {
            return container[0];
        }
        return null;
    }

    public static T[] GetByType<T>(UnityEngine.Object container) where T : class
    {
        return GetByType<T>(AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(container)));
    }

    public static T[] GetByType<T>(UnityEngine.Object[] objects) where T : class
    {
        List<T> result = new List<T>();
        foreach (UnityEngine.Object item in objects)
        {
            if (item is T)
            {
                result.Add(item as T);
            }
        }
        return result.ToArray();
    }

    public static NPipeIImportable[] OrderForImport(NPipeIImportable[] importables)
    {
        List<NPipeIImportable> result = new List<NPipeIImportable>();
        NPipeIImportable[] output = FindOutputPipes(importables);
        foreach (NPipeIImportable item in output)
        {
            if (!item.IsTemplate())
            {
                AddSourcesToList(item, result, importables);
            }
        }
        return result.ToArray();
    }

    /// <summary>
    /// Add Sources to List by checking the Input property ( thus also referencing other assets ).
    /// </summary>
    /// <param name="target">Target.</param>
    /// <param name="outList">Out list.</param>
    private static void AddSourcesToList(NPipeIImportable target, List<NPipeIImportable> outList)
    {
        NPipeIComposite composite = target as NPipeIComposite;
        if (composite == null)
        {
            outList.Add(target);
            return;
        }
        foreach (NPipeIImportable importable in EachSource(target))
        {
            AddSourcesToList(importable, outList);
        }
        outList.Add(target);
    }

    /// <summary>
    /// Add Sources to list that are contained in the sourceList. Removes previous finding ( to ensure only imported once )
    /// </summary>
    /// <param name="target">Target.</param>
    /// <param name="outList">Out list.</param>
    /// <param name="sourceList">Source list.</param>
    private static void AddSourcesToList(NPipeIImportable target, List<NPipeIImportable> outList, NPipeIImportable[] sourceList)
    {
        NPipeIComposite composite = target as NPipeIComposite;
        if (composite == null)
        {
            if (outList.Contains(target))
            {
                outList.Remove(target);
            }
            outList.Add(target);
            return;
        }
        foreach (NPipeIImportable importable in EachSource(target, sourceList))
        {
            AddSourcesToList(importable, outList, sourceList);
        }

        if (outList.Contains(target))
        {
            outList.Remove(target);
        }
        outList.Add(target);
    }

    public static NPipeIComposite[] FindNextPipes(NPipeIImportable[] importables, NPipeIImportable target)
    {
        List<NPipeIComposite> nextPipes = new List<NPipeIComposite>();
        foreach (var pipe in importables)
        {
            if (IsPrevious(pipe, target))
            {
                nextPipes.Add(pipe as NPipeIComposite);
            }
        }
        return nextPipes.ToArray();
    }

    public static T[] FindNextPipeOfType<T>(NPipeIImportable[] importables, NPipeIImportable target)  where T : class
    {
        List<T> nextPipes = new List<T>();
        foreach (var pipe in importables)
        {
            if (IsPrevious(pipe, target) && pipe is T)
            {
                nextPipes.Add(pipe as T);
            }
        }
        return nextPipes.ToArray();
    }

    public static T FindPreviousOfType<T>(NPipeIImportable target)  where T : class
    {
        foreach(NPipeIImportable source in EachSource(target))
        {
            if (source is T)
            {
                return (T) source;
            }
            T previous = FindPreviousOfType<T>(source);
            if (previous != null)
            {
                return previous;
            }
        }
        return null;
    }

    public static T FindPrevious<T>(NPipeIImportable target)  where T : class
    {
        foreach(NPipeIImportable source in EachSource(target))
        {
            if (source is T)
            {
                return (T) source;
            }
            T previous = FindPreviousOfType<T>(source);
            if (previous != null)
            {
                return previous;
            }
        }
        return null;
    }

    public static NPipeIImportable[] FindOutputPipes(NPipeIImportable[] importables)
    {
        List<NPipeIImportable> output = new List<NPipeIImportable>();
        foreach (var pipe in importables)
        {
            bool isOutputPipe = true;
            foreach (var pipe2 in importables)
            {
                if (pipe != pipe2 && IsPrevious(pipe2, pipe))
                {
                    isOutputPipe = false;
                    break;
                }
            }
            if (isOutputPipe)
            {
                output.Add(pipe);
            }
        }
        return output.ToArray();
    }

    public static bool IsPrevious(NPipeIImportable target, NPipeIImportable previous, bool recursive = false)
    {
        foreach (NPipeIImportable importable in EachSource(target))
        {
            if (importable == previous)
            {
                return true;
            }
            else if (recursive && IsPrevious(importable, previous, true))
            {
                return true;
            }
        }
        return false;
    }

    public static IEnumerable<NPipeIImportable> EachSource(NPipeIImportable target)
    {
        NPipeIComposite targetComposite = target as NPipeIComposite;
        if (targetComposite != null)
        {
            NPipeIImportable[] sources = targetComposite.GetAllInputs();
            foreach (NPipeIImportable item in sources)
            {
                yield return item;
            }
        }
    }

    public static IEnumerable<NPipeIImportable> EachSource(NPipeIImportable target, NPipeIImportable[] importables)
    {
        NPipeIComposite targetComposite = target as NPipeIComposite;
        if (targetComposite != null)
        {
            NPipeIImportable[] sources = targetComposite.GetAllInputs();
            foreach (NPipeIImportable item in sources)
            {
                if (ArrayUtility.Contains(importables, item))
                {
                    yield return item;
                }
            }
        }
    }


    public static NPipeContainer ClonePipeContainer(NPipeContainer container, string path)
    {
        NPipeContainer newContainer = NPipeContainer.CreateInstance<NPipeContainer>();
        AssetDatabase.CreateAsset(newContainer, path);

        NPipeIImportable[] templateImportables = NPipelineUtils.GetImportables(container);
        foreach (NPipeIImportable pipe in NPipelineUtils.FindOutputPipes(templateImportables))
        {
            CloneRecursive(templateImportables, pipe, path);
        }
        return newContainer;
    }

    public static NPipeIImportable CloneRecursive(NPipeIImportable[] allImportables, NPipeIImportable sourcePipe, string targetPath)
    {
        NPipeIImportable clone = (NPipeIImportable)sourcePipe.Clone();
        NPipelineUtils.CreateAttachedPipe(targetPath, clone);

        if (clone is NPipeIComposite)
        {
            NPipeIImportable sourceOfSource = ((NPipeIComposite)clone).Input;
            if (ArrayUtility.IndexOf(allImportables, sourceOfSource) > -1)
            {
                ((NPipeIComposite)clone).Input = (NPipeIImportable)CloneRecursive(allImportables, sourceOfSource, targetPath);
            }
        }
        return clone;
    }

    public static T CreatePipeContainer<T>(string path) where T : NPipeContainer
    {
        T pipeContainer = (T)NPipeContainer.CreateInstance(typeof(T));
        // Undo.RegisterCreatedObjectUndo(pipeContainer, "Created a pipe Container");
        AssetDatabase.CreateAsset(pipeContainer, path);
        pipeContainer.OnAssetCreated();
        AssetDatabase.SaveAssets();
        return pipeContainer;
    }

    public static NPipeIImportable CreateAttachedPipe(string path, System.Type type, NPipeIImportable previous = null)
    {
        NPipeIImportable instance = CreateAttachedPipe(path, UnityEngine.ScriptableObject.CreateInstance(type) as NPipeIImportable);
        if (previous != null && (instance is NPipeIComposite))
        {
            ((NPipeIComposite)instance).Input = previous;
            EditorUtility.SetDirty(instance as UnityEngine.Object);
        }
        return instance;
    }

    public static NPipeIImportable CreateSeparatedPipe(string path, System.Type type, NPipeIImportable previous = null)
    {
        NPipeIImportable instance = CreateSeparatedPipe(path, UnityEngine.ScriptableObject.CreateInstance(type) as NPipeIImportable);
        if (previous != null && (instance is NPipeIComposite))
        {
            ((NPipeIComposite)instance).Input = previous;
            EditorUtility.SetDirty(instance as UnityEngine.Object);
        }
        return instance;
    }

    public static NPipeIImportable CreateAttachedPipe(string path, NPipeIImportable pipe)
    {
        UnityEngine.Object obj = pipe as UnityEngine.Object;
        obj.hideFlags = HideFlags.HideInHierarchy;
        NPipeIImportable importable = obj as NPipeIImportable;
        obj.name = importable.GetTypeName();
        AssetDatabase.AddObjectToAsset(obj, path);
        UnityEditor.EditorUtility.SetDirty(pipe as UnityEngine.Object);
        // AssetDatabase.SaveAssets();
        // AssetDatabase.Refresh();
        // UnityEditor.Selection.activeObject = createdFactory;
        return importable;
    }


    public static NPipeIImportable CreateSeparatedPipe(string originalPath, NPipeIImportable pipe)
    {
        string path = originalPath.Substring(0, originalPath.Length - 6) + "_" + pipe.GetTypeName() + ".asset";
        CreatePipeContainer<NPipeContainer>(path);
        return CreateAttachedPipe(path, pipe);
    }

    public static T DrawSourceSelector<T>(string label, T oldValue, NPipeIImportable exclude = null) where T : class
    {
        UnityEngine.Object obj = oldValue as UnityEngine.Object;

        string path = AssetDatabase.GetAssetPath(obj);
        NPipeContainer container = AssetDatabase.LoadAssetAtPath(path, typeof(NPipeContainer)) as NPipeContainer;

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(label);
        container = (NPipeContainer)EditorGUILayout.ObjectField(container, typeof(NPipeContainer), false);
        path = AssetDatabase.GetAssetPath(container);
        T[] factories = GetTypedFactories<T>(container ? container.GetAllSelectableFactories() : new UnityEngine.Object[0]{});

        if (factories.Length != 1)
        {
            string[] options = new string[factories.Length];
            int i = 0;
            int selected = -1;
            for (int j = 0; j < factories.Length; j++)
            {
                NPipeIImportable fact = factories[j] as NPipeIImportable;
                if (fact == oldValue)
                {
                    selected = i;
                }
                if (fact != exclude && !IsPrevious(fact, exclude, true))
                {
                    options[i] = i + " " + container.GetDisplayName(fact);
                    factories[i] = factories[j];
                    i++;
                }
            }
            Array.Resize(ref options, i);
            Array.Resize(ref factories, i);

            if (selected == -1 && options.Length > 0)
            {
                selected = 0;
            }

            int newSelected = EditorGUILayout.Popup(selected, options);
            EditorGUILayout.EndHorizontal();

            if (newSelected == -1)
            {
                return null;
            }
            return factories[newSelected];
        }
        else
        {
            if (factories[0] != exclude && !IsPrevious(factories[0] as NPipeIImportable, exclude, true))
            {
                GUILayout.Label(container.GetDisplayName(factories[0] as NPipeIImportable));
                EditorGUILayout.EndHorizontal();
                return factories[0];
            }
            EditorGUILayout.EndHorizontal();
            return null;
        }
    }


    public static T DrawSourcePropertySelector<T>(GUIContent label, Rect position, T oldValue, NPipeIImportable exclude = null) where T : class
    {
        UnityEngine.Object obj = oldValue as UnityEngine.Object;

        string path = AssetDatabase.GetAssetPath(obj);
        NPipeContainer container = AssetDatabase.LoadAssetAtPath(path, typeof(NPipeContainer)) as NPipeContainer;

        // EditorGUILayout.BeginHorizontal();

        Rect containerPosition = new Rect(position.x, position.y, position.width / 4 * 3, position.height);
        Rect pipePosition = new Rect(position.x + position.width / 4 * 3, position.y, position.width / 4, position.height);

        container = (NPipeContainer)EditorGUI.ObjectField(containerPosition, label, container, typeof(NPipeContainer), false);
        path = AssetDatabase.GetAssetPath(container);
        T[] factories = GetTypedFactories<T>(container ? container.GetAllSelectableFactories() : new UnityEngine.Object[0]{});

        if (factories.Length != 1)
        {
            string[] options = new string[factories.Length];
            int i = 0;
            int selected = -1;
            for (int j = 0; j < factories.Length; j++)
            {
                NPipeIImportable fact = factories[j] as NPipeIImportable;
                if (fact == oldValue)
                {
                    selected = i;
                }
                if (fact != exclude && !IsPrevious(fact, exclude, true))
                {
                    options[i] = i + " " + container.GetDisplayName(fact);
                    factories[i] = factories[j];
                    i++;
                }
            }
            Array.Resize(ref options, i);
            Array.Resize(ref factories, i);

            if (selected == -1 && options.Length > 0)
            {
                selected = 0;
            }

            int newSelected = EditorGUI.Popup(pipePosition, selected, options);

            if (newSelected == -1)
            {
                return null;
            }
            //return oldValue;
            return factories[newSelected];
        }
        else
        {

            if (factories[0] != exclude && !IsPrevious(factories[0] as NPipeIImportable, exclude, true))
            {
                EditorGUI.LabelField(pipePosition, container.GetDisplayName(factories[0] as NPipeIImportable));
                return factories[0];
            }

            return null;
        }
    }

    public static NPipeContainer GetContainerForVoxPath(string asset)
    {
        string filename = Path.GetFileNameWithoutExtension(asset);
        string basename = Path.GetDirectoryName(asset);
        string pipelinePath = Path.Combine(Path.Combine(basename, "Pipeline/"), filename + ".asset");

        // Create or Load existing Pipeline
        NPipeContainer pipeContainer = (NPipeContainer)AssetDatabase.LoadAssetAtPath(pipelinePath, typeof(NPipeContainer));
        return pipeContainer;
    }
    
    /// <summary>
	//	This makes it easy to create, name and place unique new ScriptableObject asset files.
	/// </summary>
    public static string GetCreateScriptableObjectAssetPath<T> (string name = null) where T: ScriptableObject
	{
		string path = AssetDatabase.GetAssetPath (Selection.activeObject);
		if (path == "") 
		{
			path = "Assets";
		} 
		else if (Path.GetExtension (path) != "") 
		{
			path = path.Replace (Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject)), "");
		}

        if (name == null)
        {
            name = "/New " + typeof(T).ToString();
        }
 
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (path + name + ".asset");
        return assetPathAndName;
	}

    /// <summary>
	//This makes it easy to create, name and place unique new ScriptableObject asset files.
    // from http://wiki.unity3d.com/index.php?title=CreateScriptableObjectAsset
	/// </summary>
	public static void CreateScriptableObjectAsset<T> () where T : ScriptableObject
	{
		T asset = ScriptableObject.CreateInstance<T> ();
		AssetDatabase.CreateAsset (asset, GetCreateScriptableObjectAssetPath<T>());
		AssetDatabase.SaveAssets ();
        AssetDatabase.Refresh();
		EditorUtility.FocusProjectWindow ();
		Selection.activeObject = asset;
	}

    public static string GetPipelineDebugString(NPipeIImportable element, bool withTimes = false)
    {
        string prefix = "";
        if (element is NPipeIComposite)
        {
            prefix = GetPipelineDebugString(((NPipeIComposite)element).Input, withTimes);
        }

        string cur = (!string.IsNullOrEmpty(element.GetInstanceName()) ? element.GetInstanceName() : element.GetTypeName());

        if (withTimes)
        {
            cur += " (" + (int)(EditorApplication.timeSinceStartup - element.GetLastInvalidatedTime()) +") ";
        }

        return prefix + " / " + cur;
    }
}

#endif