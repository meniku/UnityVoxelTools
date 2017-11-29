using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

public delegate void GNPrefabNavigationChangedHandler();

public class GNPrefabNavigation
{
    public event GNPrefabNavigationChangedHandler OnFolderChanged;
        
    private string[] m_prefabFolders = null;
    private string[] m_currentFolderFiles = null;
    private int m_iCurrentFileIndex = -1;
    private int m_iCurrentFolderIndex = -1;

    public string CurrentPrefabPath
    {
        get
        {
            if (m_iCurrentFileIndex > -1 && m_currentFolderFiles != null)
            {
                return m_currentFolderFiles[m_iCurrentFileIndex];
            }
            return null;
        }
    }
    
    public string CurrentPrefabFolder
    {
        get
        {
            if (UpdateCurrentFolderIndex() != -1)
            {
                return m_prefabFolders[m_iCurrentFolderIndex];
            }
            return null;
        }
    }

    public bool SetCurrentPrefabPathFromPrefab(GameObject prefab)
    {
        string assetPath = AssetDatabase.GetAssetPath(prefab);

        if (assetPath == null || assetPath.Length < 1)
        {
            Debug.LogWarning("Could not get path from selected prefab sorry ...");
            return false;
        }

        if (m_iCurrentFileIndex > -1 && m_currentFolderFiles != null && m_currentFolderFiles.Length > m_iCurrentFileIndex && m_currentFolderFiles[m_iCurrentFileIndex] == assetPath)
        {
            // Debug.Log("path was cached");
            return true;
        }
        else
        {
            string sFolderPath = Path.GetDirectoryName(assetPath);

            m_currentFolderFiles = Directory.GetFiles(sFolderPath);
            m_iCurrentFileIndex = -1;
            for (int i = 0; i < m_currentFolderFiles.Length; i++)
            {
                if (m_currentFolderFiles[i] == assetPath)
                {
                    m_iCurrentFileIndex = i;
                    return true;
                }
            }
            //    Debug.Log("path was not cached: " +m_iCurrentFileIndex );
        }

        return false;
    }

    public GameObject Browse(GameObject _currentPrefab, bool _bForward, bool _bBrowseFolders)
    {
        if (!_currentPrefab)
        {
            GameObject anyPrefab = GetAnyPrefab();
            if(anyPrefab) 
            {
                SetCurrentPrefabPathFromPrefab(anyPrefab);
                return anyPrefab;
            }
        }

        if (!SetCurrentPrefabPathFromPrefab(_currentPrefab))
        {
            return null;
        }

        if (!_bBrowseFolders)
        {
            return BrowsePrefabInCurrentFolder(_bForward);
        }
        else
        {
            return GetFirstPrefabInNextFolder(_bForward);
        }
    }

    public GameObject RandomInFolder(GameObject _currentPrefab)
    {
        if (!_currentPrefab)
        {
            GameObject anyPrefab = GetAnyPrefab();
            if(anyPrefab) 
            {
                SetCurrentPrefabPathFromPrefab(anyPrefab);
                return anyPrefab;
            }
        }

        if (!SetCurrentPrefabPathFromPrefab(_currentPrefab))
        {
            return null;
        }

        return GetRandomPrefabInCurrentFolder();
    }

    private GameObject GetFirstPrefabInNextFolder(bool _bForward)
    {
        string[] prefabFolders = PrefabFolders;
        if(UpdateCurrentFolderIndex() == -1)
        {
            return null;
        }

        int iNumTries = 0;
        while (iNumTries++ < prefabFolders.Length)
        {
            if (!_bForward)
            {
                if (--m_iCurrentFolderIndex < 0)
                {
                    m_iCurrentFolderIndex = prefabFolders.Length - 1;
                }
            }
            else
            {
                if (++m_iCurrentFolderIndex > prefabFolders.Length - 1)
                {
                    m_iCurrentFolderIndex = 0;
                }
            }

            m_currentFolderFiles = Directory.GetFiles(prefabFolders[m_iCurrentFolderIndex]);
            if (m_currentFolderFiles.Length > 0)
            {
                // if (!_bForward)
                // {
                //     m_iCurrentFileIndex = m_currentFolderFiles.Length - 1;
                // }
                // else
                // {
                    m_iCurrentFileIndex = 0;
                // }

                for ( int j = 0; j < m_currentFolderFiles.Length; j++ )
                {
                    GameObject prefab = ( GameObject ) AssetDatabase.LoadAssetAtPath<GameObject>( m_currentFolderFiles[ j ] );
                    if ( prefab )
                    {
                        if ( OnFolderChanged != null )
                            OnFolderChanged();
                        m_iCurrentFileIndex = j;
                        return prefab;
                    }
                }
            }
        }
        Debug.Log("Could not find any prfeb in current path");
        return null;
    }

    private GameObject BrowsePrefabInCurrentFolder(bool forward)
    {
        if (m_iCurrentFileIndex == -1 || m_currentFolderFiles == null)
        {
            Debug.Log("Could not find your current prefab file path");
            return null;
        }
        int iNumTries = 0;
        while (iNumTries++ < m_currentFolderFiles.Length)
        {
            if (!forward)
            {
                if (--m_iCurrentFileIndex < 0)
                {
                    m_iCurrentFileIndex = m_currentFolderFiles.Length - 1;
                }
            }
            else
            {
                if (++m_iCurrentFileIndex > m_currentFolderFiles.Length - 1)
                {
                    m_iCurrentFileIndex = 0;
                }
            }
            // Debug.Log("loading prefab at path: " + (m_currentPaths[m_iCurrentPath]));
            GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath<GameObject>(m_currentFolderFiles[m_iCurrentFileIndex]);
            if (prefab)
            {
                return prefab;
            }
        }
        Debug.Log("Could not find any prfeb in current path");
        return null;
    }

    private GameObject GetRandomPrefabInCurrentFolder()
    {
        if (m_iCurrentFileIndex == -1)
        {
            Debug.Log("Could not find your current prefab file path");
            return null;
        }
        int numTries = 50;
        while(numTries-- > 0)
        {
            m_iCurrentFileIndex = UnityEngine.Random.Range(0, m_currentFolderFiles.Length);
            GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath<GameObject>(m_currentFolderFiles[m_iCurrentFileIndex]);
            if (prefab)
            {
                return prefab;
            }
        }
        Debug.Log("Could not find any prfeb in current path");
        return null;
    }

    private GameObject GetAnyPrefab()
    {
        string[] prefabFolders = PrefabFolders;
        string[] filePaths = new string[] { };
        int i = 0;
        foreach (string path in prefabFolders)
        {
            string[] currentFilePaths = Directory.GetFiles(path);
            if (currentFilePaths.Length > filePaths.Length)
            {
                filePaths = currentFilePaths;
                m_iCurrentFolderIndex = i;
            }
            i++;
        }

        if (filePaths.Length > 0)
        {
            i = 0;
            foreach (string path in filePaths)
            {
                GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab)
                {
                    m_iCurrentFileIndex = i;
                    if(OnFolderChanged != null) OnFolderChanged();
                    return prefab;
                }
                i++;
            }
        }

        Debug.LogWarning("Could not get any prefab from your library sorry ...");

        return null;
    }

    public string[] PrefabFolders
    {
        get
        {
            if (m_prefabFolders != null)
            {
                return m_prefabFolders;
            }

            HashSet<string> allFolders = new HashSet<string>();
            string[] paths = AssetDatabase.GetAllAssetPaths();

            foreach (string path in paths)
            {
                if(path.Contains("Generated")) // ignore some folder 
                {
                    continue;
                }
                if (Path.GetExtension(path) == ".prefab")
                {
                    allFolders.Add(Path.GetDirectoryName(path));
                }
            }

            m_prefabFolders = new string[allFolders.Count];
            allFolders.CopyTo(m_prefabFolders);
            Array.Sort(m_prefabFolders);
            return m_prefabFolders;
        }
    }
    
    private int UpdateCurrentFolderIndex()
    {
        int previous = m_iCurrentFolderIndex;
        if(m_iCurrentFileIndex < 0 || m_currentFolderFiles == null)
        {
            m_iCurrentFolderIndex = -1;
            if(OnFolderChanged != null && previous != m_iCurrentFolderIndex) OnFolderChanged();
            return -1;
        }
        string[] prefabFolders = PrefabFolders;
        string assetPath = m_currentFolderFiles[m_iCurrentFileIndex];
        string directory = Path.GetDirectoryName(assetPath);

        if (m_iCurrentFolderIndex > -1 &&
            prefabFolders.Length > m_iCurrentFolderIndex && prefabFolders[m_iCurrentFolderIndex] == directory)
        {
            // Debug.Log("path was cached");
        }
        else
        {
            m_iCurrentFolderIndex = -1;
            for (int i = 0; i < prefabFolders.Length; i++)
            {
                if (prefabFolders[i] == directory)
                {
                    m_iCurrentFolderIndex = i;
                }
            }
        }

        if (m_iCurrentFolderIndex == -1)
        {
            Debug.Log("Could not find your current prefab folder path");
        }
        if(OnFolderChanged != null && previous != m_iCurrentFolderIndex) OnFolderChanged();
        return m_iCurrentFolderIndex;
    }
}