using UnityEngine;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GNBlockMapLayer : MonoBehaviour
{
    [Serializable]
    public class CellOffset
    {
        public float m_fCellSize;
        public Vector3 m_vCellOffset;
    }
    
    public CellOffset[] m_aCellOffsets = new CellOffset[]{};
    
#if UNITY_EDITOR
    public bool HasStoredCellOffset(float _fCellSize)
    {
        foreach(CellOffset cellOffset in m_aCellOffsets )
        {
            if(cellOffset.m_fCellSize == _fCellSize)
            {
                return true;
            }
        }
        return false;
    }
    
    public Vector3 GetStoredCellOffset(float _fCellSize)
    {
        foreach(CellOffset cellOffset in m_aCellOffsets )
        {
            if(cellOffset.m_fCellSize == _fCellSize)
            {
                return cellOffset.m_vCellOffset;
            }
        }
        return Vector3.zero;
    }
    
    public void SetStoredCellOffest(float _fCellSize, Vector3 _vOffset)
    {
        foreach(CellOffset cellOffset in m_aCellOffsets )
        {
            if(cellOffset.m_fCellSize == _fCellSize)
            {
                cellOffset.m_vCellOffset = _vOffset;
                return;
            }
        }
        CellOffset newOffset = new CellOffset();
        newOffset.m_fCellSize = _fCellSize;
        newOffset.m_vCellOffset = _vOffset;
        ArrayUtility.Add(ref m_aCellOffsets, newOffset);
        EditorUtility.SetDirty(this);
    }
#endif
}