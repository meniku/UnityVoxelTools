using System;
using System.Collections.Generic;
using UnityEngine;


[System.AttributeUsage( System.AttributeTargets.Class | System.AttributeTargets.Struct )]
public class NPVoxAttributeNormalProcessor : System.Attribute
{
    public string m_editorName;

    public NPVoxAttributeNormalProcessor( string editorName )
    {
        m_editorName = editorName;
    }
}

public class NPVoxAttributeNormalProcessorParam : PropertyAttribute
{
    public string m_editorName;

    public NPVoxAttributeNormalProcessorParam( string editorName )
    {
        m_editorName = editorName;
    }
}

[System.AttributeUsage( System.AttributeTargets.Class | System.AttributeTargets.Struct )]
public class NPVoxAttributeNormalProcessorPass : System.Attribute
{
    public string m_editorName;

    public NPVoxAttributeNormalProcessorPass( string editorName )
    {
        m_editorName = editorName;
    }
}

public abstract class NPVoxNormalProcessorPass
{
    protected Vector3[] m_normalBuffer;

    public abstract void Process( NPVoxModel model, NPVoxMeshTempData[] tempdata, Vector3[] inNormals, out Vector3[] outNormals );
    
    public bool IsEnabled { get; set; }

    public NPVoxNormalProcessorPass()
    {
        IsEnabled = true;
    }

    public Vector3[] GetNormalBuffer()
    {
        return m_normalBuffer;
    }
}

public abstract class NPVoxNormalProcessor
{
    protected List<NPVoxNormalProcessorPass> m_passes;

    public NPVoxNormalProcessor()
    {
        m_passes = new List<NPVoxNormalProcessorPass>();
    }

    protected abstract void PerModelInit();

    public abstract void OneTimeInit();

    public void Process( NPVoxModel model, NPVoxMeshTempData[] tempdata, Vector3[] inNormals, out Vector3[] outNormals )
    {
        outNormals = null;

        PerModelInit();

        foreach ( NPVoxNormalProcessorPass pass in m_passes )
        {
            if ( pass.IsEnabled )
            {
                pass.Process( model, tempdata, inNormals, out outNormals );
                inNormals = outNormals;
            }
        }

        if ( outNormals == null )
        {
            outNormals = inNormals;
        }
    }
}
