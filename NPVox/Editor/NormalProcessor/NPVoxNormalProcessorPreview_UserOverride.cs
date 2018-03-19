using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ NPVoxAttributeNormalProcessorPreview( typeof( NPVoxNormalProcessor_UserOverride ) ) ]
public class NPVoxNormalProcessorPreview_UserOverride : NPVoxNormalProcessorPreview
{
    private Ray m_rayA = new Ray( Vector3.zero, Vector3.zero );
    private Ray m_rayB = new Ray( Vector3.zero, Vector3.zero );
    private NPVoxCoord m_selected = new NPVoxCoord();

    protected override void OnGUIInternal()
    {
    }

    protected override void DrawSceneInternal( Rect _rect )
    {
        if ( m_rayA.direction.sqrMagnitude > 0 )
        {
            NPipeGL.DrawLine( m_rayA.origin, m_rayA.origin + m_rayA.direction * 30, Color.red );
        }
        if ( m_rayB.direction.sqrMagnitude > 0 )
        {
            NPipeGL.DrawLine( m_rayB.origin, m_rayB.origin + m_rayB.direction * 30, Color.green );
        }

        NPipeGL.DrawLine( new Vector3( 0, 0, 0 ), new Vector3( 100, 0, 0 ), Color.green );

        Vector3 voxSize = m_context.MeshOutput.VoxelSize;
        Vector3 voxExtent = voxSize * 0.5f;
        Vector3 v1 = new Vector3( voxSize.x, 0, 0 );
        Vector3 v2 = new Vector3( 0, voxSize.y, 0 );
        Vector3 v3 = new Vector3( 0, 0, voxSize.z );

        if ( m_selected.Valid )
        {
            NPVoxToUnity voxToUnity = new NPVoxToUnity( m_context.MeshOutput.GetVoxModel(), m_context.MeshOutput.VoxelSize );
            Vector3 voxPosition = voxToUnity.ToUnityPosition( m_selected );
            NPipeGL.DrawParallelepiped( voxPosition - voxExtent, v1, v2, v3, Color.red );
        }
    }

    protected override void UpdateInputInternal()
    {
        Event e = Event.current;

        switch ( e.type )
        {
            case EventType.MouseDown:
            case EventType.MouseDrag:
                HandleRays();
                break;
        }
    }

    private void HandleRays()
    {
        Event e = Event.current;
        Vector2 rayScreenPosition = new Vector2( e.mousePosition.x - m_sceneRect.xMin, m_sceneRect.height - ( e.mousePosition.y - m_sceneRect.yMin ) );
        if ( e.shift )
        {
            m_rayA = m_context.m_camera.ScreenPointToRay( rayScreenPosition );
            NPVoxToUnity voxToUnity = new NPVoxToUnity( m_context.MeshOutput.GetVoxModel(), m_context.MeshOutput.VoxelSize );
            NPVoxRayCastHit hit = voxToUnity.Raycast( m_rayA, m_context.PreviewObject.transform, 50 );
            if ( hit.IsHit )
            {
                m_selected = hit.Coord;
            }
        }
        else if ( e.control )
        {
            m_rayB = m_context.m_camera.ScreenPointToRay( rayScreenPosition );
        }
    }

    protected override void UpdateSceneInternal()
    {
    }
}
