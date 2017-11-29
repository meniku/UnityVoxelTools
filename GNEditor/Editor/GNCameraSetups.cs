using UnityEngine;
using UnityEditor;
using System.IO;

public class GNCameraSetups
{

    public enum CameraMode
    {
        TOP,
        INGAME,
        FRONT,
        RIGHT,
        BACK,
        LEFT,
        CUSTOM,
        INGAME_MINUS_45,
        INGAME_MINUS_90,
        INGAME_PLUS_45,
        INGAME_PLUS_90
    }

    [MenuItem("Gaianigma/CameraSetups/Top &u", false)]
    public static void CameraTop()
    {
        SetCameraMode(CameraMode.TOP);
    }

    [MenuItem("Gaianigma/CameraSetups/Ingame &i", false)]
    public static void CameraIngame()
    {
        SetCameraMode(CameraMode.INGAME);
    }


    [MenuItem("Gaianigma/CameraSetups/Front &o", false)]
    public static void CameraFront()
    {
        SetCameraMode(CameraMode.FRONT);
    }

    [MenuItem("Gaianigma/CameraSetups/Right &p", false)]
    public static void CameraRight()
    {
        SetCameraMode(CameraMode.RIGHT);
    }

    [MenuItem("Gaianigma/CameraSetups/Ingame-90 &h", false)]
    public static void CameraIngameMinus90()
    {
        SetCameraMode(CameraMode.INGAME_MINUS_90);
    }

    [MenuItem("Gaianigma/CameraSetups/Ingame-45 &j", false)]
    public static void CameraIngameMinus45()
    {
        SetCameraMode(CameraMode.INGAME_MINUS_45);
    }
    
    [MenuItem("Gaianigma/CameraSetups/Ingame+45 &k", false)]
    public static void CameraIngamePlus45()
    {
        SetCameraMode(CameraMode.INGAME_PLUS_45);
    }

    [MenuItem("Gaianigma/CameraSetups/Ingame+90 &l", false)]
    public static void CameraIngamePlus90()
    {
        SetCameraMode(CameraMode.INGAME_PLUS_90);
    }

    [MenuItem("Gaianigma/CameraSetups/Back &n", false)]
    public static void CameraBack()
    {
        SetCameraMode(CameraMode.BACK);
    }

    [MenuItem("Gaianigma/CameraSetups/Left &m")]
//    [MenuItem("Gaianigma/CameraSetups/Left &z", false)]
    public static void CameraLeft()
    {
        SetCameraMode(CameraMode.LEFT);
    }
    
    [MenuItem("Gaianigma/CameraSetups/Jump To Center &j", false)]
    public static void CameraCenter()
    {
        JumpToCenter();
    }

    public static void SetCameraMode(CameraMode cameraMode)
    {
        if (!SceneView.lastActiveSceneView)
        {
            return;
        }
        switch(cameraMode)
        {
            case CameraMode.TOP:
                SceneView.lastActiveSceneView.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;
            case CameraMode.INGAME:
                SceneView.lastActiveSceneView.rotation = Quaternion.Euler(-45f, 0f, 0f);
                break;
            case CameraMode.FRONT:
                SceneView.lastActiveSceneView.rotation = Quaternion.Euler(-90f, 0f, 0f);
                break;
            case CameraMode.RIGHT:
                SceneView.lastActiveSceneView.rotation = Quaternion.Euler(0, 270, 90);
                break;
            case CameraMode.BACK:
                SceneView.lastActiveSceneView.rotation = Quaternion.Euler(90f, 180f, 0f);
                break;
            case CameraMode.LEFT:
                SceneView.lastActiveSceneView.rotation = Quaternion.Euler(0, 90, 270);
                break;
            case CameraMode.INGAME_MINUS_90:
                SceneView.lastActiveSceneView.rotation = Quaternion.Euler(0f, +45f, -90f);
                break;
            case CameraMode.INGAME_MINUS_45:
                SceneView.lastActiveSceneView.rotation = Quaternion.Euler(-30f, +30f, -60f);
                break;
            case CameraMode.INGAME_PLUS_45:
                SceneView.lastActiveSceneView.rotation = Quaternion.Euler(-30f, -30f, 60f);
                break;
            case CameraMode.INGAME_PLUS_90:
                SceneView.lastActiveSceneView.rotation = Quaternion.Euler(0f, -45f, 90f);
                break;
        }
    }
    
    public static CameraMode GetCameraMode()
    {
        if (!SceneView.lastActiveSceneView)
        {
            return CameraMode.CUSTOM;
        }
        if (SceneView.lastActiveSceneView.rotation == Quaternion.Euler(0f, 0f, 0f)) 
        {
            return CameraMode.TOP;
        }
        if(SceneView.lastActiveSceneView.rotation == Quaternion.Euler(-45f, 0f, 0f)) 
        {
            return CameraMode.INGAME;
        }
        if(SceneView.lastActiveSceneView.rotation == Quaternion.Euler(-90f, 0f, 0f)) 
        {
            return CameraMode.FRONT;
        }
        if(SceneView.lastActiveSceneView.rotation == Quaternion.Euler(0, 270, 90))
        {
            return CameraMode.RIGHT;
        }
        if(SceneView.lastActiveSceneView.rotation == Quaternion.Euler(90f, 180f, 0f)) 
        {
            return CameraMode.BACK;
        }
        if(SceneView.lastActiveSceneView.rotation == Quaternion.Euler(0, 90, 270)) 
        {
            return CameraMode.LEFT;
        }
        return CameraMode.CUSTOM;
    }

    public static void JumpToCenter()
    {
        SceneView.lastActiveSceneView.LookAt(new Vector3(0f, 0f, 0f), Quaternion.Euler(-45f, 0f, 0f), 20f);
    }
}
