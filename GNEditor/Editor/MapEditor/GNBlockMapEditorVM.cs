using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public delegate void GNBlockMapEditorChangedHandler();

public class GNBlockMapEditorVM : ScriptableObject
{
    #region CONSTANTS

    public const float MIN_GRID_RESOLTION = 0.125f;
    public const float MIN_GRID_OFFSET = 0.25f;
    #endregion

    #region SUBTYPES

    public enum Tool
    {
        NONE,
        PAINT,
        // ERASE,
        // BRUSH,
        // LAYER,
        // GRID,
        BOX,
    }

    public enum ShowLayerMode
    {
        ACTIVE,
        BELOW,
        ALL,
    }

    public enum PaintMode
    {
        BRUSH,
        RANDOM
    }

    public enum BoxTool
    {
        PAINT,
        ERASE,
        TRANSFORM,
        NONE,
    }


    [System.FlagsAttribute]
    public enum RandomBrushFlags
    {
        // None = 0x00,
        Prefab = 0x01,
        XRotation = 0x02,
        YRotation = 0x04,
        ZRotation = 0x08,
        XFlip = 0x10, // WHY THE HELL CAN'T WE USE 0x0F WTF?
        YFlip = 0x20,
        ZFlip = 0x40,
        RandomBrushEnabled = 0x80,
    }

    #endregion
    #region EVENTS

    public event GNBlockMapEditorChangedHandler OnCellZChanged;
    public event GNBlockMapEditorChangedHandler OnCellXYChanged;
    public event GNBlockMapEditorChangedHandler OnPrefabChanged;
    public event GNBlockMapEditorChangedHandler OnGridChanged;
    public event GNBlockMapEditorChangedHandler OnToolChanged;

    #endregion
    #region Properties

    public float CellSize
    {
        get
        {
            return m_blockMap.m_fCellSize;
        }
    }

    public Vector3 CellOffset
    {
        get
        {
            return m_blockMap.m_vCellOffset;
        }
        set
        {
            if (CellOffset != value)
            {
                Undo.RecordObjects(new Object[] { m_blockMap }, "Changed current Cell Offset");
                m_blockMap.m_vCellOffset = value;
                GetCurrentTargetLayer(true).SetStoredCellOffest(CellSize, m_blockMap.m_vCellOffset);
                if (OnGridChanged != null) OnGridChanged();
                UpdateCurrentCell();
            }
        }
    }

    private GNBlockMap m_blockMap = null;

    [SerializeField]
    private GameObject m_currentPrefab = null;
    public GameObject CurrentPrefab
    {
        get { return m_currentPrefab; }
    }

    public Vector3 CurrentPrefabOffset
    {
        get
        {
            return GetOffsetFor(CurrentPrefab);
        }
    }

    public string CurrentPrefabFolder
    {
        get
        {
            return m_prefabNavigation.CurrentPrefabFolder;
        }
    }

    private Vector2 m_vCurrentMousePoint = Vector2.zero;
    public Vector2 CurrentMousePoint
    {
        get
        {
            return m_vCurrentMousePoint;
        }
        set
        {
            m_vCurrentMousePoint = value;
            UpdateCurrentCell();
        }
    }

    public Vector3 CurrentCell
    {
        get
        {
            return m_blockMap.m_currentCell;
        }
        set
        {
            bool zChanged = value.z != m_blockMap.m_currentCell.z;
            bool xyChanged = value.x != m_blockMap.m_currentCell.x || value.y != m_blockMap.m_currentCell.y;
            m_blockMap.m_currentCell = value;
            if (zChanged)
            {
                ResetSelectedBox();
                OnCellZChanged();
                m_currentTargetLayer = null;
                UpdateVisibleLayers();
            }
            if (xyChanged && OnCellXYChanged != null) OnCellXYChanged();
        }
    }

    private RandomBrushFlags currentRandomBrushFlags = RandomBrushFlags.Prefab;
    public RandomBrushFlags CurrentRandomBrushFlags
    {
        get
        {
            return currentRandomBrushFlags & ~RandomBrushFlags.RandomBrushEnabled;
        }
        set
        {
            currentRandomBrushFlags = value | RandomBrushFlags.RandomBrushEnabled;
            SaveCurrentRandomBrushFlags();
        }
    }

    private GNBlockMapLayer m_currentTargetLayer = null;

    private bool m_enabled = false;

    [SerializeField]
    private Tool m_currentTool = Tool.NONE;
    public Tool CurrentTool
    {
        get
        {
            return m_currentTool;
        }
    }

    [SerializeField]
    private ShowLayerMode m_currentShowLayerMode = ShowLayerMode.BELOW;
    public ShowLayerMode CurrentShowLayerMode
    {
        get
        {
            return m_currentShowLayerMode;
        }
    }

    // [SerializeField]
    // private PaintMode m_paintMode = PaintMode.BRUSH;
    public PaintMode CurrentPaintMode
    {
        get
        {
            if ((currentRandomBrushFlags & RandomBrushFlags.RandomBrushEnabled) == RandomBrushFlags.RandomBrushEnabled)
            {
                return PaintMode.RANDOM;
            }
            else
            {
                return PaintMode.BRUSH;
            }
        }
        set
        {
            if (value == PaintMode.RANDOM)
            {
                currentRandomBrushFlags |= RandomBrushFlags.RandomBrushEnabled;
            }
            else
            {
                currentRandomBrushFlags &= ~RandomBrushFlags.RandomBrushEnabled;
            }
            SaveCurrentRandomBrushFlags();
        }
    }

    public Vector3 PrefabOrientation
    {
        get
        {
            return m_vPrefabOrientation;
        }
    }

    public Vector3 PrefabFlip
    {
        get
        {
            return m_vPrefabFlip;
        }
    }

    public string CurrentPrefabPath
    {
        get
        {
            return m_prefabNavigation != null ? m_prefabNavigation.CurrentPrefabPath : null;
        }
    }

    private string[] m_currentPaths;
    private int m_iCurrentPath;

    private Vector3 m_vPrefabOrientation = Vector3.zero;
    private Vector3 m_vPrefabFlip = Vector3.one;

    private GNPrefabNavigation m_prefabNavigation = null;

    private Bounds m_selectedBox = new Bounds(Vector3.zero, Vector3.one);
    public Bounds SelectedBox
    {
        get
        {
            return m_selectedBox;
        }
    }

    #endregion
    #region CODE

    public void UndoRedoPerformed()
    {
        if (OnPrefabChanged != null)
        {
            OnPrefabChanged();
        }
        if (OnCellZChanged != null)
        {
            OnCellZChanged();
        }
        if (OnCellXYChanged != null)
        {
            OnCellXYChanged();
        }
        if (OnGridChanged != null)
        {
            OnGridChanged();
        }
        if (OnToolChanged != null)
        {
            OnToolChanged();
        }

        m_currentTargetLayer = null;
        if (m_blockMap != null)
        {
            UpdateVisibleLayers();
            UpdateCurrentCell();
        }
    }

    public void SelectPrefab(GameObject _prefab)
    {
        m_currentPrefab = PrefabUtility.FindPrefabRoot(_prefab);
        if (OnPrefabChanged != null) OnPrefabChanged();
    }

    public void Disable()
    {
        // Debug.Log("disable");
        if (m_enabled)
        {
            m_enabled = false;
            // SetCurrentTool(Tool.PAINT);
            if (m_blockMap == null)
            {
                return;
            }
            UpdateVisibleLayers(true);
            Renderer[] renderers = m_blockMap.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                EditorUtility.SetSelectedRenderState(renderer, EditorSelectedRenderState.Wireframe);
//                EditorUtility.SetSelectedWireframeHidden(renderer, false);
            }
            m_prefabNavigation.OnFolderChanged -= LoadCurrentRandomBrushFlags;

            m_blockMap.m_prefab = m_currentPrefab;
            m_blockMap.m_bLocked = false;

            CalculateBounds();
            RemoveEmptyLayers();
        }
    }

    public void Enable(GNBlockMap blockMap)
    {
        if (!m_enabled)
        {
            m_prefabNavigation = new GNPrefabNavigation();
            if (m_currentPrefab != null)
            {
                m_prefabNavigation.SetCurrentPrefabPathFromPrefab(m_currentPrefab);
            }

            m_blockMap = blockMap;
            m_blockMap.m_bLocked = true;
            // Undo.RecordObject(m_blockMap, "Scene Save Enforcement");
            // EditorUtility.SetDirty(m_blockMap);
            // UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty();
            m_enabled = true;
            UpdateCurrentCell();
            UpdateVisibleLayers(false);
            ResetSelectedBox();

            Renderer[] renderers = blockMap.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                EditorUtility.SetSelectedRenderState(renderer, EditorSelectedRenderState.Hidden);
//                EditorUtility.SetSelectedWireframeHidden(renderer, true);
            }
            m_prefabNavigation.OnFolderChanged += LoadCurrentRandomBrushFlags;
            
            MoveChildrenToCorrectTargetLayers();
            
            EditorUtility.SetDirty(m_blockMap);

            if ( !m_currentPrefab )
            {
                SelectPrefab(m_blockMap.m_prefab);
                m_prefabNavigation.SetCurrentPrefabPathFromPrefab(m_currentPrefab);
            }
        }
    }

    private void CalculateBounds()
    {
        Bounds bounds = new Bounds();
        Transform[] allTransforms = m_blockMap.GetComponentsInChildren<Transform>();
        foreach(Transform t in allTransforms )
        {
            bounds.Encapsulate( t.position );
        }

        bounds.min -= Vector3.one * 0.5f;
        bounds.max += Vector3.one * 0.5f;

        m_blockMap.m_bounds = bounds;
    }

    private void RemoveEmptyLayers()
    {
        Transform blockMapTransform = m_blockMap.transform;
        // Debug.Log("zLayer " + CurrentCell.z + " " + CellOffset.z);

        for (int i = 0; i <  blockMapTransform.childCount; )
        {
            Transform childTransform = blockMapTransform.GetChild(i);
            GNBlockMapLayer blockMapLayer = childTransform.GetComponent<GNBlockMapLayer>();
            if ( blockMapLayer && childTransform.childCount == 0)
            {
                DestroyImmediate( childTransform.gameObject );
                EditorUtility.SetDirty( m_blockMap );
            }
            else
            {
                i++;
            }
        }
    }


    public bool UpdateCurrentToolFromSceneView(UnityEditor.Tool tool)
    {
        switch (tool)
        {
            case UnityEditor.Tool.Move: return SetCurrentTool(Tool.PAINT);
            // case UnityEditor.Tool.Rotate: return SetCurrentTool(Tool.GRID);
            case UnityEditor.Tool.Rotate: return SetCurrentTool(Tool.BOX);
            default: return SetCurrentTool(Tool.NONE);
        }
    }

    public bool SetCurrentTool(Tool tool)
    {
        // Undo.RecordObjects(new Object[] { this }, "Changed current tool");
        bool changed = this.m_currentTool != tool;
        this.m_currentTool = tool;

        if (this.m_currentTool != Tool.NONE)
        {
            // Reset unity tool to remove drawing of that damn default gizmo
            Tools.current = UnityEditor.Tool.None;
        }

        if (changed)
        {
            UpdateCurrentCell();
            OnToolChanged();
        }

        return changed;
    }

    public bool SetCurrentPaintMode(PaintMode mode)
    {
        if (this.CurrentPaintMode != mode)
        {
            this.CurrentPaintMode = mode;
            return true;
        }
        return false;
    }

    public bool SetCurrentShowLayerMode(ShowLayerMode showLayerMode)
    {
        // Undo.RecordObjects(new Object[] { this }, "Changed current tool");
        bool changed = this.m_currentShowLayerMode != showLayerMode;
        if (this.m_currentShowLayerMode != showLayerMode)
        {
            changed = true;
            this.m_currentShowLayerMode = showLayerMode;
            UpdateVisibleLayers();
        }
        return changed;
    }

    public void EraseObject()
    {
        GameObject previous = GetGameObjectUnderMouse();
        if (previous)
        {
            Undo.DestroyObjectImmediate(previous);
        }
    }

    private void EraseObjectAt(Vector3 cell)
    {
        GameObject previous = GetGameObjectAt(cell);
        if (previous)
        {
            Undo.DestroyObjectImmediate(previous);
        }
    }

    public void PaintObject()
    {
        PaintObjectAt(CurrentCell);
    }

    private void PaintObjectAt(Vector3 _cell)
    {
        if (m_currentPrefab)
        {
            GameObject previous = GetGameObjectAt(_cell);

            if (previous)
            {
                Undo.DestroyObjectImmediate(previous);
            }

            GameObject created = (GameObject)PrefabUtility.InstantiatePrefab(m_currentPrefab);
            created.transform.parent = GetTargetLayer(_cell.z, true).transform;
            Vector3 pos = _cell + GetOffsetFor(created);// + created.transform.parent.position;
            // pos.z = 0;
            created.transform.position = Align(pos);
            // Debug.Log("blubb: " + PrefabOrientation);
            created.transform.eulerAngles = PrefabOrientation;
            created.transform.localScale = PrefabFlip;
            Undo.RegisterCreatedObjectUndo(created, "Painted an Object");

            if (this.CurrentPaintMode == PaintMode.RANDOM)
            {
                this.RandomizeBrush();
            }

            EditorUtility.SetSelectedRenderState(created.GetComponent<Renderer>(), EditorSelectedRenderState.Hidden);
//            EditorUtility.SetSelectedWireframeHidden(created.GetComponent<Renderer>(), true);
        }
        else
        {
            Debug.LogWarning("Could not paint Object because no preab is selected");
        }
    }

    private void RandomizeBrush()
    {
        Undo.RecordObject(this, "Randomize Brush");

        if ((CurrentRandomBrushFlags & RandomBrushFlags.Prefab) != 0)
        {
            SelectPrefab(m_prefabNavigation.RandomInFolder(m_currentPrefab));
        }

        Vector3 vPrefabOrientation = PrefabOrientation;
        Vector3 vPrefabFlip = PrefabFlip;

        if ((CurrentRandomBrushFlags & RandomBrushFlags.XRotation) != 0)
        {
            vPrefabOrientation.x = ((float)Random.Range(0, 4)) * 90f;
        }
        if ((CurrentRandomBrushFlags & RandomBrushFlags.YRotation) != 0)
        {
            vPrefabOrientation.y = ((float)Random.Range(0, 4)) * 90f;
        }
        if ((CurrentRandomBrushFlags & RandomBrushFlags.ZRotation) != 0)
        {
            vPrefabOrientation.z = ((float)Random.Range(0, 4)) * 90f;
        }

        if ((CurrentRandomBrushFlags & RandomBrushFlags.XFlip) != 0)
        {
            vPrefabFlip.x = Random.value > 0.5f ? 1 : -1;
        }
        if ((CurrentRandomBrushFlags & RandomBrushFlags.YFlip) != 0)
        {
            vPrefabFlip.y = Random.value > 0.5f ? 1 : -1;
        }
        if ((CurrentRandomBrushFlags & RandomBrushFlags.ZFlip) != 0)
        {
            vPrefabFlip.z = Random.value > 0.5f ? 1 : -1;
        }

        this.SetPrefabFlip(vPrefabFlip);
        this.OrientPrefab(vPrefabOrientation);
    }

    private GNBlockMapLayer GetCurrentTargetLayer(bool create = true)
    {
        if (m_currentTargetLayer)
        {
            return m_currentTargetLayer;
        }
        float zLayer = CurrentCell.z;
        m_currentTargetLayer = GetTargetLayer(zLayer, create);
        return m_currentTargetLayer;
    }

    private GNBlockMapLayer GetTargetLayer(float zLayer, bool create = true)
    {
        Transform blockMapTransform = m_blockMap.transform;
        int iNumChildren = blockMapTransform.childCount;
        string strLayerName = zLayer.ToString("00.000");
        // Debug.Log("zLayer " + CurrentCell.z + " " + CellOffset.z);

        for (int i = 0; i < iNumChildren; i++)
        {
            Transform childTransform = blockMapTransform.GetChild(i);
            if (childTransform.gameObject.name == strLayerName)
            {
                GNBlockMapLayer blockMapLayer = childTransform.GetComponent<GNBlockMapLayer>();
                if (!blockMapLayer)
                {
                    blockMapLayer = childTransform.gameObject.AddComponent<GNBlockMapLayer>();
                    EditorUtility.SetDirty(blockMapLayer);
                }

                return blockMapLayer;
            }
        }

        if (create)
        {
            GameObject createdGameObject = new GameObject();
            createdGameObject.transform.position = new Vector3(0, 0, zLayer);
            createdGameObject.transform.parent = m_blockMap.transform;
            createdGameObject.name = strLayerName;
            GNBlockMapLayer blockMapLayer = createdGameObject.AddComponent<GNBlockMapLayer>();
            EditorUtility.SetDirty(createdGameObject);
            // Undo.RegisterCreatedObjectUndo(createdGameObject, "Created a new TargetLayer");
            return blockMapLayer;
        }

        return null;
    }

    private float GetTargetZPosition(GameObject gameObject)
    {
        return gameObject.transform.position.z - GetOffsetFor(gameObject).z;
    }

    private GameObject GetGameObjectUnderMouse()
    {
        return GetGameObjectAt(CurrentCell);
    }

    private GameObject GetGameObjectAt(Vector3 cell)
    {
        GNBlockMapLayer currentLayer = cell.z == CurrentCell.z ? GetCurrentTargetLayer() : GetTargetLayer(cell.z);
        if (currentLayer == null)
        {
            return null;
        }
        Transform transform = currentLayer.transform;
        int iNumChildren = transform.childCount;
        for (int i = 0; i < iNumChildren; i++)
        {
            Transform childTransform = transform.GetChild(i);
            Vector3 pos = childTransform.position;
            if (IsMultipleCellBlock(childTransform.gameObject))
            {
                pos -= GetOffsetFor(childTransform.gameObject);
            }
            if (Vector3.Distance(pos, cell) < MIN_GRID_RESOLTION)
            {
                return childTransform.gameObject;
            }
        }
        return null;
    }

    private bool MoveToCorrectTargetLayer(GameObject gameObject)
    {
        GNBlockMapLayer targetLayer = GetTargetLayer(GetTargetZPosition(gameObject), true);
        if (targetLayer.transform != gameObject.transform.parent)
        {
            Debug.LogWarning("Moving GameObject" + gameObject + " from layer + " + gameObject.transform.parent.name + " to layer " + targetLayer.name + " (Tile Info : " + GetTileInfo(gameObject) + " )");

            Undo.RecordObject(gameObject, "Moved to correct target layer");
            EditorUtility.SetDirty(gameObject);
            gameObject.transform.parent = targetLayer.transform;
            return true;
        }
        return false;
    }

    private bool IsMultipleCellBlock(GameObject gameObject)
    {
        GNBlockMapTile tile = gameObject.GetComponent<GNBlockMapTile>();
        if(tile && tile.Offset.magnitude > 0)
        {
            return true;
        }
        
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        if (!meshFilter || !meshFilter.sharedMesh)
        {
            return false;
        }
        Vector3 sizeHalf = meshFilter.sharedMesh.bounds.extents;
        return sizeHalf.x > 1.5f || sizeHalf.y > 1.5f || sizeHalf.z > 1.5f;
    }

    public string GetTileInfo(GameObject go = null)
    {
        // GameObject go = GetGameObjectUnderMouse();
        if( go == null)
        {
            go = GetGameObjectUnderMouse();
        }

        if (go)
        {
            MeshFilter meshFilter = go.GetComponent<MeshFilter>();
            if (!meshFilter || !meshFilter.sharedMesh)
            {
                return "No Mesh";
            }
            return "Bounds extends: " + meshFilter.sharedMesh.bounds.extents + " Center: " + meshFilter.sharedMesh.bounds.center;
        }
        return "No Go";
    } 

    private Vector3 GetOffsetFor(GameObject gameObject)
    {
        GNBlockMapTile tile = gameObject.GetComponent<GNBlockMapTile>();
        if(tile)
        {
            return tile.Offset;
        }
        
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        if (!meshFilter || !meshFilter.sharedMesh)
        {
            return Vector3.zero;
        }
        Vector3 sizeHalf = meshFilter.sharedMesh.bounds.extents - Vector3.one;
        sizeHalf.z = -sizeHalf.z;
        return Align(sizeHalf); 
    }

    public void BrowseLayer(bool next)
    {
        Undo.RecordObjects(new Object[] { this, m_blockMap }, "Changed Cell Layer");
        Vector3 vCell = this.CurrentCell;
        vCell.z = vCell.z + (next ? CellSize : -CellSize);
        vCell.z = CellOffset.z + Mathf.Round((vCell.z - CellOffset.z) / CellSize) * CellSize;

        UpdateCurrentCell(vCell.z);

        // try to load previous set cell offset for current  z layer of current cell
        GNBlockMapLayer targetLayer = GetCurrentTargetLayer(true);
        if (targetLayer.HasStoredCellOffset(CellSize))
        {
            // Debug.Log("Loaded CellOffset");
            CellOffset = targetLayer.GetStoredCellOffset(CellSize);
        }
        else
        {
            targetLayer.SetStoredCellOffest(CellSize, CellOffset);
        }

        // UpdateCurrentCell(vCell.z);
    }

    public void BrowseCellSize(bool next)
    {
        Undo.RecordObjects(new Object[] { this, m_blockMap }, "Changed Cell Size");
        if (!next)
        {
            if (CellSize == 1.0f)
                ChangeCellSize(4.0f);
            else if (CellSize == 2.0f)
                ChangeCellSize(1.0f);
            else
                ChangeCellSize(2.0f);
        }
        else
        {
            if (CellSize == 4.0f)
                ChangeCellSize(1.0f);
            else if (CellSize == 2.0f)
                ChangeCellSize(4.0f);
            else
                ChangeCellSize(2.0f);
        }
    }

    public bool ChangeCellSize(float _fNewValue)
    {
        if (m_blockMap.m_fCellSize != _fNewValue)
        {
            m_blockMap.m_fCellSize = _fNewValue;

            Vector3 cellOffset = new Vector3(
                m_blockMap.m_fCellSize / 2f - 1.0f,
                m_blockMap.m_fCellSize / 2f - 1.0f,
                m_blockMap.m_fCellSize / 2f - 1.0f
            );
            m_blockMap.m_vCellOffset = cellOffset;

            // first, try to get a new Z layer using the default offset
            float newZ = CellOffset.z + Mathf.Round((m_blockMap.m_currentCell.z - CellOffset.z) / CellSize) * CellSize;
            UpdateCurrentCell(newZ);
            newZ = CurrentCell.z;

            // now try to load and apply previous set cell offset for current Z-Layer and CellSize
            GNBlockMapLayer targetLayer = GetTargetLayer(newZ, true);
            if (targetLayer.HasStoredCellOffset(_fNewValue))
            {
                m_blockMap.m_vCellOffset = targetLayer.GetStoredCellOffset(_fNewValue);
                // Debug.Log("Loaded CellOffset");
            }
            else
            {
                m_blockMap.m_vCellOffset = cellOffset;
                targetLayer.SetStoredCellOffest(_fNewValue, cellOffset);
            }

            OnGridChanged();
            UpdateCurrentCell(newZ);

            return true;
        }
        return false;
    }

    public void BrowsePrefabs(bool _bNext, bool _bBrowseFolders)
    {
        Undo.RecordObjects(new Object[] { this, m_blockMap }, "Changed current Prefab");

        SelectPrefab(m_prefabNavigation.Browse(m_currentPrefab, _bNext, _bBrowseFolders));
    }

    public void PickCurrentCellPrefab()
    {
        Undo.RecordObjects(new Object[] { this, m_blockMap }, "Changed current Prefab");

        GameObject go = GetGameObjectUnderMouse();
        if (go)
        {
            GameObject prefab = PrefabUtility.GetPrefabParent(go) as GameObject;
            // Debug.Log(prefab);
            if (prefab)
            {
                SelectPrefab(prefab);
                m_prefabNavigation.SetCurrentPrefabPathFromPrefab(m_currentPrefab);
                OrientPrefab(go.transform.eulerAngles);
                SetPrefabFlip(go.transform.localScale);
            }
        }
    }

    public void OrientPrefab(Vector3 axis, bool _bClockWise)
    {
        OrientPrefab(PrefabOrientation + (axis * (_bClockWise ? -90 : 90)));
    }

    public void OrientPrefab(Vector3 _vPrefabOrientation)
    {
        float fMinAngle = 15.0f;
        m_vPrefabOrientation.x = Mathf.Round((_vPrefabOrientation.x % 360) / fMinAngle) * fMinAngle;
        m_vPrefabOrientation.y = Mathf.Round((_vPrefabOrientation.y % 360) / fMinAngle) * fMinAngle;
        m_vPrefabOrientation.z = Mathf.Round((_vPrefabOrientation.z % 360) / fMinAngle) * fMinAngle;

        if (OnPrefabChanged != null)
        {
            OnPrefabChanged();
        }
    }

    public void FlipPrefab(Vector3 axis)
    {
        Vector3 newFlip = this.PrefabFlip;
        newFlip.x = axis.x != 0 ? -newFlip.x : newFlip.x;
        newFlip.y = axis.y != 0 ? -newFlip.y : newFlip.y;
        newFlip.z = axis.z != 0 ? -newFlip.z : newFlip.z;
        SetPrefabFlip(newFlip);
    }

    public void SetPrefabFlip(Vector3 _vPrefabFlip)
    {
        m_vPrefabFlip = _vPrefabFlip;

        if (OnPrefabChanged != null)
        {
            OnPrefabChanged();
        }
    }

    private void UpdateCurrentCell(float newZ = float.NaN)
    {
        float scale = CellSize;
        Vector3 newCell = CurrentCell;
        newCell.x = CellOffset.x + Mathf.Round((m_vCurrentMousePoint.x - CellOffset.x) / scale) * scale;
        newCell.y = CellOffset.y + Mathf.Round((m_vCurrentMousePoint.y - CellOffset.y) / scale) * scale;

        if (!float.IsNaN(newZ))
        {
            newCell.z = CellOffset.z + Mathf.Round((newZ - CellOffset.z) / scale) * scale;
        }

        if (newCell != CurrentCell)
        {
            CurrentCell = newCell;
        }
    }

    private void UpdateVisibleLayers(bool _enableAll = false)
    {
        GNBlockMapLayer targetLayer = GetCurrentTargetLayer(false);
        float currentZPosition = CurrentCell.z + CellOffset.z; //float.Parse(targetLayer.name);
        Transform blockMapTransform = m_blockMap.transform;
        int iNumChildren = blockMapTransform.childCount;

        for (int i = 0; i < iNumChildren; i++)
        {
            Transform childTransform = blockMapTransform.GetChild(i);

            if (childTransform.gameObject.name == "_brush")
            {
                continue;
            }

            float zPosition;
            try
            {
                zPosition = float.Parse(childTransform.gameObject.name);
            }
            catch (System.Exception)
            {
                Debug.LogWarning("Could not parse zPosition for layer: " + childTransform.gameObject.name);
                continue;
            }

            if ((targetLayer != null && childTransform.gameObject == targetLayer.gameObject) ||
                _enableAll ||
                CurrentShowLayerMode == ShowLayerMode.ALL ||
                (CurrentShowLayerMode == ShowLayerMode.BELOW && zPosition > currentZPosition)
            )
            {
                childTransform.gameObject.SetActive(true);
            }
            else
            {
                childTransform.gameObject.SetActive(false);
            }
        }
    }

    public Bounds CurrentLayerExtends()
    {
        Transform targetLayer = GetCurrentTargetLayer().transform;
        return LayerExtends(targetLayer);
    }

    public Bounds LayerExtends(Transform targetLayer)
    {
        Bounds extends = new Bounds();
        bool first = true;
        for (int i = 0; i < targetLayer.childCount; i++)
        {
            Transform t = targetLayer.GetChild(i);
            if (first)
            {
                first = false;
                extends.SetMinMax(
                    t.transform.position - Vector3.one * CellSize / 2,
                    t.transform.position + Vector3.one * CellSize / 2);
            }
            else
            {
                extends.Encapsulate(t.transform.position - Vector3.one * CellSize / 2);
                extends.Encapsulate(t.transform.position + Vector3.one * CellSize / 2);
            }
        }
        return extends;
    }

    public Bounds FullMapExtends()
    {
        Bounds extends = new Bounds();
        bool first = true;
        for (int i = 0; i < m_blockMap.transform.childCount; i++)
        {
            Bounds bounds = LayerExtends(m_blockMap.transform.GetChild(i));
            if (bounds.size.magnitude < MIN_GRID_RESOLTION)
            {
                continue;
            }
            if (first)
            {
                first = false;
                extends.SetMinMax(bounds.min, bounds.max);
            }
            else
            {
                extends.Encapsulate(bounds);
            }
        }
        return extends;
    }

    public bool SelectBoxFullLayer()
    {
        return SelectBox(CurrentLayerExtends());
    }

    public bool SelectBoxFullMap()
    {
        return SelectBox(FullMapExtends());
    }

    public bool SelectBox(Bounds newBox)
    {
        if (m_selectedBox.Equals(newBox))
        {
            return false;
        }
        if (newBox.size.magnitude < MIN_GRID_RESOLTION)
        {
            ResetSelectedBox();
        }
        else
        {
            m_selectedBox = newBox;
        }
        return true;
    }

    private void ResetSelectedBox()
    {
        m_selectedBox.SetMinMax(
            this.CurrentCell - Vector3.one * CellSize / 2,
            this.CurrentCell + Vector3.one * CellSize / 2
        );
    }

    public bool ApplyBoxTool(BoxTool boxTool)
    {
        if (boxTool == BoxTool.NONE)
        {
            return false;
        }

        if (boxTool == BoxTool.TRANSFORM)
        {
            GNBlockMapEditorBoxTransformWindow.Show(this);
            return true;
        }

        if (boxTool == BoxTool.ERASE)
        {
            foreach (Vector3 cell in EnumerateSelectedBox())
            {
                EraseObjectAt(cell);
            }
        }

        if (boxTool == BoxTool.PAINT)
        {
            foreach (Vector3 cell in EnumerateSelectedBox())
            {
                PaintObjectAt(cell);
            }
        }


        return true;
    }

    public void TransformSelectedBox(Matrix4x4 mat)
    {
        for (int i = 0; i < m_blockMap.transform.childCount; i++)
        {
            Transform layer = m_blockMap.transform.GetChild(i);
            for (int j = 0; j < layer.transform.childCount; j++)
            {
                Transform item = layer.transform.GetChild(j);
                if (m_selectedBox.Contains(item.transform.position))
                {
                    Vector3 newPos = Align( mat.MultiplyPoint(item.transform.position) );
                    
                    Undo.RecordObject(item, "transform");
                    item.transform.position = newPos;
                    if(MoveToCorrectTargetLayer(item.gameObject))
                    {
                        j--;
                    }
                }
            }
        }
        
        // foreach (Transform child in EnumerateSelectedBoxItems())
        // {
        //     Vector3 newPos = Align( mat.MultiplyPoint(child.transform.position) );
            
        //     Undo.RecordObject(child, "transform");
        //     child.transform.position = newPos;
        //     MoveToCorrectTargetLayer(child.gameObject);
        // }
    }
    public void XFlipSelectedBox()
    {
        foreach (Transform child in EnumerateSelectedBoxItems())
        {
            Undo.RecordObject(child, "transform");
            Vector3 scale = child.transform.localScale;
            scale.x *= -1;
            child.transform.localScale = scale;

            Vector3 newPos = child.transform.position;
            newPos.x = m_selectedBox.center.x - (newPos.x - m_selectedBox.center.x);
            newPos.x = Mathf.Round(newPos.x / MIN_GRID_RESOLTION) * MIN_GRID_RESOLTION;
            child.transform.position = newPos;
        }
    }
    
    public Vector3 Align( Vector3 _source )
    {
        _source.x = Mathf.Round(_source.x / MIN_GRID_RESOLTION) * MIN_GRID_RESOLTION;
        _source.y = Mathf.Round(_source.y / MIN_GRID_RESOLTION) * MIN_GRID_RESOLTION;
        _source.z = Mathf.Round(_source.z / MIN_GRID_RESOLTION) * MIN_GRID_RESOLTION;
        return _source;
    }


    private IEnumerable<Vector3> EnumerateSelectedBox()
    {
        Vector3 leftDownFront = this.SelectedBox.min + Vector3.one * CellSize / 2;
        Vector3 rightUpBack = this.SelectedBox.max; // - Vector3.one * CellSize / 2;
        for (float x = leftDownFront.x; x <= rightUpBack.x; x += CellSize)
        {
            for (float y = leftDownFront.y; y <= rightUpBack.y; y += CellSize)
            {
                for (float z = leftDownFront.z; z <= rightUpBack.z; z += CellSize)
                {
                    yield return new Vector3(
                       CellOffset.x + Mathf.Round((x - CellOffset.x) / CellSize) * CellSize,
                       CellOffset.y + Mathf.Round((y - CellOffset.y) / CellSize) * CellSize,
                       CellOffset.z + Mathf.Round((z - CellOffset.z) / CellSize) * CellSize
                    );
                }
            }
        }
    }

    public IEnumerable<Transform> EnumerateSelectedBoxItems()
    {
        for (int i = 0; i < m_blockMap.transform.childCount; i++)
        {
            Transform layer = m_blockMap.transform.GetChild(i);
            for (int j = 0; j < layer.transform.childCount; j++)
            {
                Transform item = layer.transform.GetChild(j);
                if (m_selectedBox.Contains(item.transform.position))
                {
                    yield return item;
                }
            }
        }
    }

    private void LoadCurrentRandomBrushFlags()
    {
        // Debug.Log("LoadCurrentRandomBrushFlags");
        if (m_prefabNavigation.CurrentPrefabFolder != null)
        {
            string key = "GNBlockmapEditor_PrefabFlags_" + m_prefabNavigation.CurrentPrefabFolder;
            if (EditorPrefs.HasKey(key))
            {
                currentRandomBrushFlags = (RandomBrushFlags)EditorPrefs.GetInt(key);
            }
            else
            {
                currentRandomBrushFlags = RandomBrushFlags.Prefab;
            }
        }
    }

    private void SaveCurrentRandomBrushFlags()
    {
        if (m_prefabNavigation.CurrentPrefabFolder != null)
        {
            EditorPrefs.SetInt("GNBlockmapEditor_PrefabFlags_" + m_prefabNavigation.CurrentPrefabFolder, (int)currentRandomBrushFlags);
        }
    }

    public void BrowseLayerMode()
    {
        // Undo.RecordObject(this, "browse layermode");
        this.SetCurrentShowLayerMode((ShowLayerMode)(((int)this.CurrentShowLayerMode + 1) % 3));
    }


    #endregion

    #region FIX_TOOLS

    public void MoveChildrenToCorrectTargetLayers()
    {
        GNBlockMapLayer[] layers = m_blockMap.GetComponentsInChildren<GNBlockMapLayer>(true);

        foreach (GNBlockMapLayer lyr in layers)
        {
            Transform[] transforms = lyr.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if( transforms[i].parent == lyr.transform && FixObject(transforms[i].gameObject) )
                {
                    i--;
                }
            }
        }
    }

    private bool FixObject(GameObject gameObject)
    {
        GNBlockMapTile tile = gameObject.GetComponent<GNBlockMapTile>();
        
        if(tile != null)
        {
            float z = GetTargetZPosition(gameObject);
            GNBlockMapLayer targetLayer = GetTargetLayer(z, false);
            if (targetLayer == null || targetLayer.transform != gameObject.transform.parent)
            {
                Undo.RecordObject(gameObject.transform, "fix position");
                tile.transform.position += tile.Offset;
                Debug.LogWarning(" Moving old collision tile to correct position " + tile.transform.position);
            }
            
            UnityEngine.Assertions.Assert.AreEqual(tile.transform.parent.GetComponent<GNBlockMapLayer>(), GetTargetLayer(GetTargetZPosition(gameObject), false));
            return false;
        }

        if( gameObject.isStatic && gameObject.GetComponent<NPVoxCubeSimplifierInstance>() != null)
        {
            // if(gameObject)
            // TODO: check for negative scale and revert them?!?

            Vector3 scle = gameObject.transform.localScale;
            if(scle.x < 0)
            {
                Undo.RecordObject(gameObject.transform, "fix scale");
                scle.Scale(new Vector3(-1,1,1));
                Debug.LogWarning(" Removing negative scale from object at position " + gameObject.transform.position);
                gameObject.transform.localScale = scle;
            }
            if(scle.y < 0)
            {
                Undo.RecordObject(gameObject.transform, "fix scale");
                scle.Scale(new Vector3(1,-1,1));
                Debug.LogWarning(" Removing negative scale from object at position " + gameObject.transform.position);
                gameObject.transform.localScale = scle;
            }
            if(scle.z < 0)
            {
                Undo.RecordObject(gameObject.transform, "fix scale");
                scle.Scale(new Vector3(1,1,-1));
                Debug.LogWarning(" Removing negative scale from object at position " + gameObject.transform.position);
                gameObject.transform.localScale = scle;
            }

        }

        // Vector3 pos = Align(gameObject.transform.position);
        //  new Vector3(
        //      Mathf.Round(gameObject.transform.localPosition.x / MIN_GRID_RESOLTION) * MIN_GRID_RESOLTION,
        //      Mathf.Round(gameObject.transform.localPosition.y / MIN_GRID_RESOLTION) * MIN_GRID_RESOLTION,
        //      Mathf.Round(gameObject.transform.localPosition.z / MIN_GRID_RESOLTION) * MIN_GRID_RESOLTION
        // );

        // if(gameObject.transform.position.x != pos.x || gameObject.transform.position.y != pos.y || gameObject.transform.position.z != pos.z)
        // {
        //     Debug.LogWarning(" Aligning object to position " + pos);
        //     gameObject.transform.position = pos;
        // }
        
        return MoveToCorrectTargetLayer(gameObject);
        // return false;
    }


    #endregion
}