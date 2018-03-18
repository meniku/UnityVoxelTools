using System;
using System.Collections.Generic;
using UnityEngine;

public class NPipeGL
{
    public static void DrawLine( Vector3 _p1, Vector3 _p2, Color _color )
    {
        GL.Begin( GL.LINES );
        GL.Color( _color );
        GL.Vertex( _p1 );
        GL.Color( _color );
        GL.Vertex( _p2 );
        GL.End();
    }

    public static void DrawLines( Vector3[] _p, Color _color )
    {
        GL.Begin( GL.LINE_STRIP );
        for ( int i = 0; i < _p.Length; i++ )
        {
            GL.Color( _color );
            GL.Vertex( _p[ i ] );
        }
        GL.End();
    }

    public static void DrawPoly( Vector3[] _p, Color _color )
    {
        if ( _p.Length == 0 )
        {
            return;
        }

        GL.Begin( GL.LINE_STRIP );
        for ( int i = 0; i < _p.Length; i++ )
        {
            GL.Color( _color );
            GL.Vertex( _p[ i ] );
        }
        GL.Color( _color );
        GL.Vertex( _p[ 0 ] );
        GL.End();
    }

    public static void DrawQuad( Vector3 _p1, Vector3 _p2, Vector3 _p3, Vector3 _p4, Color _color )
    {
        GL.Begin( GL.QUADS );
        GL.Color( _color );
        GL.Vertex( _p1 );
        GL.Color( _color );
        GL.Vertex( _p2 );
        GL.Color( _color );
        GL.Vertex( _p3 );
        GL.Color( _color );
        GL.Vertex( _p4 );
        GL.End();
    }

    public static void DrawParallelogram( Vector3 _p, Vector3 _v1, Vector3 _v2, Color _color )
    {
        Vector3 p2 = _p + _v1;
        Vector3 p3 = p2 + _v2;
        Vector3 p4 = _p + _v2;
        GL.Begin( GL.LINE_STRIP );
        GL.Color( _color );
        GL.Vertex( _p );
        GL.Color( _color );
        GL.Vertex( p2 );
        GL.Color( _color );
        GL.Vertex( p3 );
        GL.Color( _color );
        GL.Vertex( p4 );
        GL.Color( _color );
        GL.Vertex( _p );
        GL.End();
    }

    public static void DrawParallelepiped( Vector3 _p, Vector3 _v1, Vector3 _v2, Vector3 _v3, Color _color )
    {
        DrawParallelogram( _p, _v1, _v2, _color );
        DrawParallelogram( _p + _v3, _v1, _v2, _color );
        Vector3 p2 = _p + _v1;
        Vector3 p3 = p2 + _v2;
        Vector3 p4 = _p + _v2;
        DrawLine( _p, _p + _v3, _color );
        DrawLine( p2, p2 + _v3, _color );
        DrawLine( p3, p3 + _v3, _color );
        DrawLine( p4, p4 + _v3, _color );
    }

    public static void PostRenderBegin( Matrix4x4 _projection, Matrix4x4 _modelview, Material _material )
    {
        GL.PushMatrix();
        GL.LoadProjectionMatrix( _projection );
        GL.modelview = _modelview;
        _material.SetPass( 0 );
    }

    public static void PostRenderEnd()
    {
        GL.PopMatrix();
    }
}
