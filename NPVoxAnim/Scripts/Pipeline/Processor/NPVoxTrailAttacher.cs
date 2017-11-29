using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

[NPipeAppendableAttribute("Trail Attacher", typeof(NPVoxIModelFactory), true, true)]
public class NPVoxTrailAttacher : NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel>, NPVoxIModelFactory, NPVoxISceneEditable
{
    public NPVoxCoord startCoord1;
    public NPVoxCoord startCoord2;
    public Quaternion rotation1 = Quaternion.identity;
    public Quaternion rotation2 = Quaternion.identity;
    public int numSteps = 20;
    public float stepSize1 = 2f;
    public float stepSize2 = 2f;
    public bool addVoxelGroup = true;

    public Color32 startColor = new Color32(255, 255, 255, 255);
    public Color32 endColor  = new Color32(255, 0, 0, 0);

    override public string GetTypeName()
    {
        return "Trail Attacher";
    }

    override protected NPVoxModel CreateProduct(NPVoxModel reuse = null)
    {
        if (Input == null)
        {
            return NPVoxModel.NewInvalidInstance(reuse, "No Input Setup");;
        }

        NPVoxModel model = ((NPVoxIModelFactory)Input).GetProduct() as NPVoxModel;

        NPVoxModel newModel =  NPVoxModel.NewInstance(model, NPVoxCoord.ZERO, reuse);
        newModel.CopyOver(model);

        bool addVoxelGroups = addVoxelGroup || newModel.HasVoxelGroups();
        byte theVoxelGroup = (byte)255;
        if (addVoxelGroups)
        {
            if (!newModel.HasVoxelGroups())
            { 
                newModel.InitVoxelGroups();
                foreach (NPVoxCoord coord in newModel.EnumerateVoxels())
                {
                    newModel.SetVoxelGroup(coord, 0);
                }
                theVoxelGroup = 1;
                newModel.NumVoxelGroups = 2;
            }
            else
            {
                theVoxelGroup = newModel.NumVoxelGroups;
                newModel.NumVoxelGroups++;
            }
        }

        Vector3 direction1 = rotation1 * Vector3.forward;
        Vector3 direction2 = rotation2 * Vector3.forward;

        bool[] usedColors = NPVoxModelUtils.GetUsedColors(newModel);
        Color32[] colorTable = newModel.Colortable;

        Vector3 currentP1 = NPVoxCoordUtil.ToVector( startCoord1 );
        Vector3 currentP2 = NPVoxCoordUtil.ToVector( startCoord2 );
        for(int i = 0; i < numSteps; i++)
        {
            byte color = NPVoxModelUtils.FindUnusedColor(ref usedColors);
            colorTable[color] = Color32.Lerp(startColor, endColor, (float)i / (float)numSteps);
            NPVoxGeomUtil.DrawLine(newModel, currentP1, currentP2, color, theVoxelGroup);
            currentP1 += direction1 * stepSize1;
            currentP2 += direction2 * stepSize2;
        }
        newModel.Colortable = colorTable;


        newModel.RecalculateNumVoxels();
//
        return newModel;
    }

    #if UNITY_EDITOR
    override public bool DrawInspector(NPipeEditFlags flags)
    {
        bool changed = base.DrawInspector(flags & ~NPipeEditFlags.INPUT);
        return changed;
    }

    public string[] GetSceneEditingTools()
    {
        return new string[] { "Point 1", "Point 2", "Direction 1", "Direction 2" };
    }

    public System.Func<NPVoxISceneEditable, bool> DrawSceneTool(NPVoxToUnity npVoxToUnity, Transform transform, int tool)
    {
        if (Input == null)
        {
            return null;
        }

        NPVoxModel model = ((NPVoxIModelFactory)Input).GetProduct() as NPVoxModel;
        if (!model)
        {
            return null;
        }

        // Start Coord 1
        if (tool == 0)
        {
            NPVoxCoord someNewCoord = NPVoxHandles.VoxelPicker(npVoxToUnity, startCoord1, 0, transform );
            if (!startCoord1.Equals(someNewCoord))
            {
                return (NPVoxISceneEditable t) =>
                {
                    (t as NPVoxTrailAttacher).startCoord1 = someNewCoord;
                    return true;
                };
            }
        }

        // Start Coord 2
        if (tool == 1)
        {
            NPVoxCoord someNewCoord = NPVoxHandles.VoxelPicker(npVoxToUnity, startCoord2, 0, transform );
            if (!startCoord2.Equals(someNewCoord))
            {
                return (NPVoxISceneEditable t) =>
                {
                    (t as NPVoxTrailAttacher).startCoord2 = someNewCoord;
                    return true;
                };
            }
        }

        // Direction 1
        if (tool == 2)
        {
            Quaternion newQuaternion = Handles.RotationHandle(rotation1, npVoxToUnity.ToUnityPosition(startCoord1));
            if (!newQuaternion.Equals(rotation1))
            {
                return (NPVoxISceneEditable t) =>
                {
                    (t as NPVoxTrailAttacher).rotation1 = newQuaternion;
                    return true;
                };
            }
        }

        // Direction 2
        if (tool == 3)
        {
            Quaternion newQuaternion = Handles.RotationHandle(rotation2, npVoxToUnity.ToUnityPosition(startCoord2));
            if (!newQuaternion.Equals(rotation2))
            {
                return (NPVoxISceneEditable t) =>
                {
                    (t as NPVoxTrailAttacher).rotation2 = newQuaternion;
                    return true;
                };
            }
        }

        return null;
    }

    public void ResetSceneTools()
    {
        rotation1 = Quaternion.identity;
        rotation2 = Quaternion.identity;
        startCoord1 = NPVoxCoord.ZERO;
        startCoord2 = NPVoxCoord.ONE;
    }

    #endif

}