using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

using System.Collections;

public class NPVoxHandles
{
    public static float GetMouseScale(Camera cam)
    {
//        Debug.Log("DPI: " + Screen.dpi);
//        Debug.Log("Res: " +  Screen.currentResolution);
       
        return SceneView.currentDrawingSceneView.camera.rect.width;
        //return Screen.dpi > 100 ? 2.0f : 1.0f;
    }

    public static float GetScreenScale()
    {
        //        Debug.Log("DPI: " + Screen.dpi);
        //        Debug.Log("Res: " +  Screen.currentResolution);
        //Debug.Log( SceneView.currentDrawingSceneView.camera.rect.width + " " + Screen.width);
        //SceneView.currentDrawingSceneView.camera.
        //return SceneView.currentDrawingSceneView.camera.rect.width;
//        return Screen.dpi > 100 ? 0.5f : 1.0f;
        return 1.0f / SceneView.currentDrawingSceneView.camera.rect.width;
    }

    private static Vector2 mouseStartPos = Vector2.zero;
    private static NPVoxCoord startCoordVox;
    private static sbyte startCoord;

    public static NPVoxCoord VoxelPicker(NPVoxToUnity npVoxToUnity, NPVoxCoord previousSelectedCoord, int buttonNum, Transform transform)
    {
        if (npVoxToUnity == null)
        {
            return NPVoxCoord.INVALID;
        }

        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        EventType eventType = Event.current.GetTypeForControl(controlID);

        Vector3 voxelCenter = new Vector3();
        NPVoxCoord impactCoord = NPVoxCoord.INVALID;

//        Vector3 offset = SceneView.currentDrawingSceneView.position;
//        Vector3 mp = Event.current.mousePosition - offset;
        float mouseScale = GetMouseScale(SceneView.currentDrawingSceneView.camera);
        Ray r = SceneView.currentDrawingSceneView.camera.ScreenPointToRay(
            new Vector3(Event.current.mousePosition.x * mouseScale, -Event.current.mousePosition.y * mouseScale + Camera.current.pixelHeight)
        );
        NPVoxRayCastHit raycastHit = npVoxToUnity.Raycast(r, transform, 20);
        
        if (raycastHit.IsHit)
        {
            impactCoord = raycastHit.Coord;
            voxelCenter = npVoxToUnity.ToUnityPosition(impactCoord);

            // if (isMouseDown)
            {
                Handles.color = Color.yellow;
                Handles.CubeCap(controlID, voxelCenter, Quaternion.identity, npVoxToUnity.VoxeSize.x * 2.0f);
//                SceneView.currentDrawingSceneView.Repaint();
            }
        }
        else
        {
            return previousSelectedCoord;
        }
        switch (eventType)
        {
            case EventType.Layout:
                mouseStartPos = Event.current.mousePosition;

                // if (raycastHit.IsHit)
                {
                    HandleUtility.AddControl(
                        controlID,
                        HandleUtility.DistanceToCircle(voxelCenter, npVoxToUnity.VoxeSize.x * 0.25f)
                    );
                }

                break;

            case EventType.MouseDown:
                if (HandleUtility.nearestControl == controlID && Event.current.button == buttonNum)
                {
                    mouseStartPos = Event.current.mousePosition;
                    GUIUtility.hotControl = controlID;
                    Event.current.Use();
                }
                break;

            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID)
                {
                    GUI.changed = true;
                    GUIUtility.hotControl = 0;
                    Event.current.Use();
                    return impactCoord;
                }
                break;

            case EventType.MouseDrag:
                if (GUIUtility.hotControl == controlID)
                {
                    Event.current.Use();
                }
                break;
        }

        return previousSelectedCoord;
    }

    public static NPVoxBox DrawBoxSelection(NPVoxToUnity npVoxToUnity, NPVoxBox box, bool editable = true)
    {
        Vector3 leftDownBack = npVoxToUnity.ToUnityPosition(box.LeftDownBack) - npVoxToUnity.VoxeSize * 0.5f;
        Vector3 rightUpForward = npVoxToUnity.ToUnityPosition(box.RightUpForward) + npVoxToUnity.VoxeSize * 0.5f;
        Vector3 rightDownBack = new Vector3(rightUpForward.x, leftDownBack.y, leftDownBack.z);
        Vector3 leftUpBack = new Vector3(leftDownBack.x, rightUpForward.y, leftDownBack.z);
        Vector3 rightUpBack = new Vector3(rightUpForward.x, rightUpForward.y, leftDownBack.z);
        Vector3 leftDownForward = new Vector3(leftDownBack.x, leftDownBack.y, rightUpForward.z);
        Vector3 rightDownForward = new Vector3(rightUpForward.x, leftDownBack.y, rightUpForward.z);
        Vector3 leftUpForward = new Vector3(leftDownBack.x, rightUpForward.y, rightUpForward.z);
        Handles.DrawDottedLine(leftDownBack, rightDownBack, 1f);
        Handles.DrawDottedLine(leftDownBack, leftUpBack, 1f);
        Handles.DrawDottedLine(leftUpBack, rightUpBack, 1f);
        Handles.DrawDottedLine(rightDownBack, rightUpBack, 1f);
        Handles.DrawDottedLine(leftDownForward, rightDownForward, 1f);
        Handles.DrawDottedLine(leftDownForward, leftUpForward, 1f);
        Handles.DrawDottedLine(leftUpForward, rightUpForward, 1f);
        Handles.DrawDottedLine(rightDownForward, rightUpForward, 1f);
        Handles.DrawDottedLine(leftDownBack, leftDownForward, 1f);
        Handles.DrawDottedLine(rightDownBack, rightDownForward, 1f);
        Handles.DrawDottedLine(leftUpBack, leftUpForward, 1f);
        Handles.DrawDottedLine(rightUpBack, rightUpForward, 1f);
        if(!editable)
        {
            return box;
        }

        NPVoxBox newBox = new NPVoxBox(box.LeftDownBack, box.RightUpForward);

        if (SceneView.currentDrawingSceneView.camera.orthographic)
        {
            NPVoxCoord oldCoord;
            NPVoxCoord newCoord;

            oldCoord = box.LeftDownBack;
            newCoord = NPVoxHandles.DrawVoxelHandle(leftDownBack, oldCoord, npVoxToUnity);
            if (!newCoord.Equals(oldCoord))
            {
                newBox.LeftDownBack = newCoord;
            }

            oldCoord = box.RightDownBack;
            newCoord = NPVoxHandles.DrawVoxelHandle(rightDownBack, oldCoord, npVoxToUnity);
            if (!newCoord.Equals(oldCoord))
            {
                newBox.RightDownBack = newCoord;
            }

            oldCoord = box.LeftUpBack;
            newCoord = NPVoxHandles.DrawVoxelHandle(leftUpBack, oldCoord, npVoxToUnity);
            if (!newCoord.Equals(oldCoord))
            {
                newBox.LeftUpBack = newCoord;
            }

            oldCoord = box.RightUpBack;
            newCoord = NPVoxHandles.DrawVoxelHandle(rightUpBack, oldCoord, npVoxToUnity);
            if (!newCoord.Equals(oldCoord))
            {
                newBox.RightUpBack = newCoord;
            }

            oldCoord = box.LeftDownForward;
            newCoord = NPVoxHandles.DrawVoxelHandle(leftDownForward, oldCoord, npVoxToUnity);
            if (!newCoord.Equals(oldCoord))
            {
                newBox.LeftDownForward = newCoord;
            }

            oldCoord = box.RightDownForward;
            newCoord = NPVoxHandles.DrawVoxelHandle(rightDownForward, oldCoord, npVoxToUnity);
            if (!newCoord.Equals(oldCoord))
            {
                newBox.RightDownForward = newCoord;
            }

            oldCoord = box.LeftUpForward;
            newCoord = NPVoxHandles.DrawVoxelHandle(leftUpForward, oldCoord, npVoxToUnity);
            if (!newCoord.Equals(oldCoord))
            {
                newBox.LeftUpForward = newCoord;
            }

            oldCoord = box.RightUpForward;
            newCoord = NPVoxHandles.DrawVoxelHandle(rightUpForward, oldCoord, npVoxToUnity);
            if (!newCoord.Equals(oldCoord))
            {
                newBox.RightUpForward = newCoord;
            }

            // center mover
            oldCoord = box.LeftDownBack;
            newCoord = NPVoxHandles.DrawVoxelHandle(npVoxToUnity.ToUnityPosition(box.SaveCenter), oldCoord, npVoxToUnity);
            if (!newCoord.Equals(oldCoord))
            {
                newBox.SaveCenter += NPVoxCoordUtil.ToVector(newCoord - oldCoord);
            }
        }
        else
        {
            sbyte oldPos;
            sbyte newPos;
            Vector3 handlePos;

            handlePos = new Vector3(leftDownBack.x + (rightUpForward.x - leftDownBack.x) / 2, leftDownBack.y + (rightUpForward.y - leftDownBack.y) / 2, leftDownBack.z);
            oldPos = box.Back;
            newPos = NPVoxHandles.DrawAxisHandle(handlePos, oldPos, npVoxToUnity, Vector3.forward);
            if (oldPos != newPos)
            {
                newBox.Back = newPos;
            }

            handlePos = new Vector3(leftDownBack.x + (rightUpForward.x - leftDownBack.x) / 2, leftDownBack.y + (rightUpForward.y - leftDownBack.y) / 2, rightUpForward.z);
            oldPos = box.Forward;
            newPos = NPVoxHandles.DrawAxisHandle(handlePos, oldPos, npVoxToUnity, Vector3.forward);
            if (oldPos != newPos)
            {
                newBox.Forward = newPos;
            }

            handlePos = new Vector3(leftDownBack.x + (rightUpForward.x - leftDownBack.x) / 2, leftDownBack.y, leftDownBack.z + (rightUpForward.z - leftDownBack.z) / 2);
            oldPos = box.Down;
            newPos = NPVoxHandles.DrawAxisHandle(handlePos, oldPos, npVoxToUnity, Vector3.up);
            if (oldPos != newPos)
            {
                newBox.Down = newPos;
            }

            handlePos = new Vector3(leftDownBack.x + (rightUpForward.x - leftDownBack.x) / 2, rightUpForward.y, leftDownBack.z + (rightUpForward.z - leftDownBack.z) / 2);
            oldPos = box.Up;
            newPos = NPVoxHandles.DrawAxisHandle(handlePos, oldPos, npVoxToUnity, Vector3.up);
            if (oldPos != newPos)
            {
                newBox.Up = newPos;
            }

            handlePos = new Vector3(leftDownBack.x, leftDownBack.y + (rightUpForward.y - leftDownBack.y) / 2, leftDownBack.z + (rightUpForward.z - leftDownBack.z) / 2);
            oldPos = box.Left;
            newPos = NPVoxHandles.DrawAxisHandle(handlePos, oldPos, npVoxToUnity, Vector3.right);
            if (oldPos != newPos)
            {
                newBox.Left = newPos;
            }

            handlePos = new Vector3(rightUpForward.x, leftDownBack.y + (rightUpForward.y - leftDownBack.y) / 2, leftDownBack.z + (rightUpForward.z - leftDownBack.z) / 2);
            oldPos = box.Right;
            newPos = NPVoxHandles.DrawAxisHandle(handlePos, oldPos, npVoxToUnity, Vector3.right);
            if (oldPos != newPos)
            {
                newBox.Right = newPos;
            }
        }
        return newBox;
    }

    public static NPVoxCoord DrawVoxelHandle(Vector3 handlePosition, NPVoxCoord voxPosition, NPVoxToUnity npVoxToUnity)
    {
        int controlID = GUIUtility.GetControlID(FocusType.Passive);

        // if(HandleUtility.nearestControl == controlID)
        // {
        //     Handles.color = Color.yellow;
        // }
        // else
        // {
        //     Handles.color = Color.white;
        // }

        Handles.DotCap(controlID, handlePosition, Quaternion.identity, npVoxToUnity.VoxeSize.x * 0.25f);

        Vector3 screenPosition = Handles.matrix.MultiplyPoint(handlePosition);

        switch (Event.current.GetTypeForControl(controlID))
        {
            case EventType.Layout:
                HandleUtility.AddControl(
                    controlID,
                    HandleUtility.DistanceToCircle(screenPosition, npVoxToUnity.VoxeSize.x * 0.25f)
                );
                break;

            case EventType.MouseDown:
                if (HandleUtility.nearestControl == controlID && Event.current.button == 0)
                {
                    mouseStartPos = Event.current.mousePosition;
                    startCoordVox = voxPosition;
                    // Respond to a press on this handle. Drag starts automatically.
                    GUIUtility.hotControl = controlID;
                    Event.current.Use();
                }
                break;

            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID)
                {
                    // Respond to a release on this handle. Drag stops automatically.
                    GUIUtility.hotControl = 0;
                    Event.current.Use();
                }
                break;

            case EventType.MouseDrag:
                if (GUIUtility.hotControl == controlID)
                {
                    // Do whatever with mouse deltas here
                    GUI.changed = true;

                    Vector3 pos = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin - HandleUtility.GUIPointToWorldRay(mouseStartPos).origin;
                    NPVoxCoord result = startCoordVox + npVoxToUnity.ToVoxDirection(pos);
                    if (!result.Equals(voxPosition))
                    {
                        SceneView.RepaintAll();
                        return result;
                    }
                }
                break;
        }

        return voxPosition;
    }

    public static sbyte DrawAxisHandle(Vector3 handlePosition, sbyte voxPosition, NPVoxToUnity npVoxToUnity, Vector3 axis)
    {
        int controlID = GUIUtility.GetControlID(FocusType.Passive);

        Handles.DotCap(controlID, handlePosition, Quaternion.identity, npVoxToUnity.VoxeSize.x * 0.25f);

        Vector3 screenPosition = Handles.matrix.MultiplyPoint(handlePosition);

        switch (Event.current.GetTypeForControl(controlID))
        {
            case EventType.Layout:
                HandleUtility.AddControl(
                    controlID,
                    HandleUtility.DistanceToCircle(screenPosition, npVoxToUnity.VoxeSize.x * 0.25f * (1 + npVoxToUnity.UnityVoxModelSize.x))
                );
                break;

            case EventType.MouseDown:
                if (HandleUtility.nearestControl == controlID)
                {
                    mouseStartPos = Event.current.mousePosition;
                    startCoord = voxPosition;
                    // Respond to a press on this handle. Drag starts automatically.
                    GUIUtility.hotControl = controlID;
                    Event.current.Use();
                }
                break;

            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID)
                {
                    // Respond to a release on this handle. Drag stops automatically.
                    GUIUtility.hotControl = 0;
                    Event.current.Use();
                }
                break;

            case EventType.MouseDrag:
                if (GUIUtility.hotControl == controlID)
                {
                    // Do whatever with mouse deltas here
                    GUI.changed = true;

                    float transl = HandleUtility.CalcLineTranslation(mouseStartPos, Event.current.mousePosition, handlePosition, axis);
                    sbyte result;
                    if (axis == Vector3.forward)
                    {
                        result = (sbyte)(startCoord + npVoxToUnity.ToVoxDirection(Vector3.forward * transl).Z);
                    }
                    else if (axis == Vector3.right)
                    {
                        result = (sbyte)(startCoord + npVoxToUnity.ToVoxDirection(Vector3.right * transl).X);
                    }
                    else
                    {
                        result = (sbyte)(startCoord + npVoxToUnity.ToVoxDirection(Vector3.up * transl).Y);
                    }
                    if (result != voxPosition)
                    {
                        SceneView.RepaintAll();
                        return result;
                    }
                }
                break;
        }
        return voxPosition;
    }
}

#endif