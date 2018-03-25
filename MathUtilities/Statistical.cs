using System;
using System.Collections.Generic;
using UnityEngine;

namespace MathUtilities
{
    public class Statistical
    {
        public static Vector3 ComputeSum( Vector3[] _in )
        {
            Vector3 _out = Vector3.zero;
            for ( int i = 0; i < _in.Length; i++ )
            {
                _out += _in[ i ];
            }
            return _out;
        }

        public static bool ComputeAverage( Vector3[] _in, ref Vector3 _out )
        {
            if ( _in.Length == 0 )
            {
                return false;
            }

            Vector3 sum = ComputeSum( _in );
            sum /= _in.Length;

            return true;
        }

        public static Vector3 ComputeSum( List<Vector3> _in )
        {
            Vector3 _out = Vector3.zero;

            if ( _in == null )
            {
                return _out;
            }

            for ( int i = 0; i < _in.Count; i++ )
            {
                _out += _in[ i ];
            }
            return _out;
        }

        public static bool ComputeAverage( List<Vector3> _in, ref Vector3 _out )
        {
            if ( _in == null || _in.Count == 0 )
            {
                return false;
            }

            Vector3 sum = ComputeSum( _in );
            _out = sum / _in.Count;

            return true;
        }
    }
}
