using System;
using UnityEngine;

public interface NPVoxITransformable
{
    void SetTranslation(Vector3 translation);
    Vector3 GetTranslation();
    void SetRotation(Quaternion quat);
    Quaternion GetRotation();
    void SetScale(Vector3 scale);
    Vector3 GetScale();
//    void ResetTransformation();
    Vector3 GetPivot();
    void SetPivot(Vector3 pivot);
}

