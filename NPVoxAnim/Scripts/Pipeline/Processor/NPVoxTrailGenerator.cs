using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

[NPipeAppendableAttribute("Trail Generator", typeof(NPVoxIModelFactory), true, true)]
public class NPVoxTrailGenerator : NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel>, NPVoxIModelFactory, NPVoxISceneEditable
{
    // Previous Frame
    [NPipeSelectorAttribute(typeof(NPVoxIModelFactory))]
    public UnityEngine.Object PreviousFrame;

    // Previous Frame
    [NPipeSelectorAttribute(typeof(NPVoxIModelFactory))]
    public UnityEngine.Object TargetFrame;

    [HideInInspector]
    public string SocketName1;

    [HideInInspector]
    public string SocketName2;

//    public Vector3 Offset1 = Vector3.zero;
//    public Vector3 Offset2 = Vector3.zero;
//    public Vector3 ControlOffset1 = Vector3.zero;
//    public Vector3 ControlOffset2 = Vector3.zero;
    const int INDEX_TARGET_1 = 0;
    const int INDEX_TARGET_2 = 1;
    const int INDEX_SOURCE_1 = 2;
    const int INDEX_SOURCE_2 = 3;

    public Vector3[] SocketOffsets = new Vector3[]{ Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
    public Vector3[] ControlPointOffsets = new Vector3[]{ Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };

    public float TheStepSize = 0.25f;

    public float MaxDistance = 1000f;

    // Voxel Group to set for the weapon
    public byte SetBaseVoxelGroup = 2;

    // Voxel group to set for the tral
    public byte SetVoxelGroup = 1;

    public int ColorNumFromModel = -1; // -1 to disable

    public Color32 Color1 = new Color32(255, 255, 255, 255);

    public Color32 Color2 = new Color32(255, 255, 255, 25);

    public int NumColorSteps = 10;

    override public string GetTypeName()
    {
        return "Trail Generator";
    }

    override protected NPVoxModel CreateProduct(NPVoxModel reuse = null)
    {
        if (Input == null)
        {
            return NPVoxModel.NewInvalidInstance(reuse, "No Input Setup");
        }

        NPVoxModel model = ((NPVoxIModelFactory)Input).GetProduct() as NPVoxModel;

        NPVoxModel targetModel = model;

        if (TargetFrame is NPVoxIModelFactory)
        {
            targetModel = ((NPVoxIModelFactory)TargetFrame).GetProduct() as NPVoxModel;
        }

        if (!(PreviousFrame is NPVoxIModelFactory))
        {
            Debug.LogWarning("previous frame is not a model factory");
            return NPVoxModel.NewInstance(NPVoxCoord.ZERO, reuse);
        }

        #if UNITY_EDITOR
        if (SocketOffsets.Length < 4 || ControlPointOffsets.Length < 4)
        {
            ResetSceneTools();
        }
        #endif

        NPVoxModel sourceModel = ((NPVoxIModelFactory)PreviousFrame).GetProduct();

        NPVoxSocket sourceSocket1 = sourceModel.GetSocketByName(SocketName1);
        NPVoxSocket sourceSocket2 = sourceModel.GetSocketByName(SocketName2);
        NPVoxSocket targetSocket1 = targetModel.GetSocketByName(SocketName1);
        NPVoxSocket targetSocket2 = targetModel.GetSocketByName(SocketName2);

        if (sourceSocket1.IsInvalid() )
        {
            Debug.LogWarning("SocketName1 not found in sourceModel");
            return NPVoxModel.NewInstance(NPVoxCoord.ZERO, reuse);
        }

        if (sourceSocket2.IsInvalid())
        {
            Debug.LogWarning("SocketName2 not found in sourceModel");
            return NPVoxModel.NewInstance(NPVoxCoord.ZERO, reuse);
        }

        if (targetSocket1.IsInvalid())
        {
            Debug.LogWarning("SocketName1 not found in newModel");
            return NPVoxModel.NewInstance(NPVoxCoord.ZERO, reuse);
        }

        if (targetSocket2.IsInvalid())
        {
            Debug.LogWarning("SocketName2 not found in oldModel");
            return NPVoxModel.NewInstance(NPVoxCoord.ZERO, reuse);
        }

        if (TheStepSize < 0.01f)
        {
            Debug.LogWarning("Stepsize too small");
            return NPVoxModel.NewInstance(NPVoxCoord.ZERO, reuse);
        }

        NPVoxToUnity sourceN2U = new NPVoxToUnity(sourceModel, Vector3.one);
        NPVoxToUnity targetN2U = new NPVoxToUnity(targetModel, Vector3.one);
        NPVoxToUnity modelN2U = new NPVoxToUnity(model, Vector3.one);

        // calculate size for our new model
        NPVoxBox requiredBounds = model.BoundingBox;
        requiredBounds.EnlargeToInclude(modelN2U.ToVoxCoord(sourceN2U.ToUnityPosition(sourceSocket1.Anchor) + SocketOffsets[INDEX_SOURCE_1]));
        requiredBounds.EnlargeToInclude(modelN2U.ToVoxCoord(sourceN2U.ToUnityPosition(sourceSocket2.Anchor) + SocketOffsets[INDEX_SOURCE_2]));
        requiredBounds.EnlargeToInclude(modelN2U.ToVoxCoord(targetN2U.ToUnityPosition(targetSocket1.Anchor) + SocketOffsets[INDEX_TARGET_1]));
        requiredBounds.EnlargeToInclude(modelN2U.ToVoxCoord(targetN2U.ToUnityPosition(targetSocket2.Anchor) + SocketOffsets[INDEX_TARGET_2]));
        requiredBounds.EnlargeToInclude(modelN2U.ToVoxCoord(sourceN2U.ToUnityPosition(sourceSocket1.Anchor) + SocketOffsets[INDEX_SOURCE_1] + ControlPointOffsets[INDEX_SOURCE_1]));
        requiredBounds.EnlargeToInclude(modelN2U.ToVoxCoord(sourceN2U.ToUnityPosition(sourceSocket2.Anchor) + SocketOffsets[INDEX_SOURCE_2] + ControlPointOffsets[INDEX_SOURCE_2]));
        requiredBounds.EnlargeToInclude(modelN2U.ToVoxCoord(targetN2U.ToUnityPosition(targetSocket1.Anchor) + SocketOffsets[INDEX_TARGET_1] + ControlPointOffsets[INDEX_TARGET_1]));
        requiredBounds.EnlargeToInclude(modelN2U.ToVoxCoord(targetN2U.ToUnityPosition(targetSocket2.Anchor) + SocketOffsets[INDEX_TARGET_2] + ControlPointOffsets[INDEX_TARGET_2]));

        // create our product model
        NPVoxModel productModel =  NPVoxModelTransformationUtil.CreateWithNewSize(model, requiredBounds, reuse);

        // prepare voxel groups
        bool addVoxelGroups = SetVoxelGroup > 0 || productModel.HasVoxelGroups() || SetBaseVoxelGroup > 0;
        byte theVoxelGroup = (byte)SetVoxelGroup;
        if (addVoxelGroups)
        {
            if (!productModel.HasVoxelGroups())
            {
                productModel.InitVoxelGroups();
                foreach (NPVoxCoord coord in productModel.EnumerateVoxels())
                {
                    productModel.SetVoxelGroup(coord, SetBaseVoxelGroup);
                }
            }
            if (theVoxelGroup > productModel.NumVoxelGroups - 1)
            {
                productModel.NumVoxelGroups = (byte) ( theVoxelGroup + 1 );
            }
            if (SetBaseVoxelGroup > productModel.NumVoxelGroups - 1)
            {
                productModel.NumVoxelGroups = (byte) ( SetBaseVoxelGroup + 1 );
            }
        }

        // check if we have a circularloop
        #if UNITY_EDITOR
        if (NPipelineUtils.IsPrevious(PreviousFrame as NPipeIImportable, this, true))
        {
            Debug.LogWarning("cycular pipeline detected");
            return productModel;
        }
        #endif

        NPVoxToUnity productN2U = new NPVoxToUnity(productModel, Vector3.one);

        // build our colortable
        bool[] usedColors = NPVoxModelUtils.GetUsedColors(productModel);

        Color32[] colorTable = productModel.Colortable;
        byte[] Colors = new byte[NumColorSteps];

        Color32 startColor = Color1;
        Color32 endColor = Color2;

        bool takeColorFromModel = ColorNumFromModel > -1;
        if (takeColorFromModel)
        {
            byte color1 = NPVoxModelUtils.FindUsedColor(ref usedColors, ColorNumFromModel);
            startColor = colorTable[color1];
            endColor = colorTable[color1];
            endColor.a = 15;
        }

//        Debug.Log("Me: " + NPipelineUtils.GetPipelineDebugString(this));
        for(int i = 0; i < NumColorSteps; i++)
        {
            byte color = NPVoxModelUtils.FindUnusedColor(ref usedColors);
//            Debug.Log("Color: " + color);
            colorTable[color] = Color32.Lerp(startColor, endColor, ((float)i / (float)NumColorSteps));
            Colors[i] = color;
        }


        // calculate mathetmatical constants
        Vector3 unityStartPoint1 = targetN2U.ToUnityPosition( targetSocket1.Anchor ) + targetN2U.ToUnityDirection( SocketOffsets[INDEX_TARGET_1] );
        Vector3 bezierStartPoint1 = unityStartPoint1 + targetN2U.ToUnityDirection(ControlPointOffsets[INDEX_TARGET_1]);

        Vector3 unityEndPoint1 = sourceN2U.ToUnityPosition( sourceSocket1.Anchor ) + sourceN2U.ToUnityDirection( SocketOffsets[INDEX_SOURCE_1] );
        Vector3 bezierEndPoint1 = unityEndPoint1 + sourceN2U.ToUnityDirection(ControlPointOffsets[INDEX_SOURCE_1]);

        Vector3 direction1 = unityEndPoint1 - unityStartPoint1;
        float dir1len = direction1.magnitude;

        Vector3 unityStartPoint2 = targetN2U.ToUnityPosition( targetSocket2.Anchor ) + targetN2U.ToUnityDirection( SocketOffsets[INDEX_TARGET_2] );
        Vector3 bezierStartPoint2 = unityStartPoint2 + targetN2U.ToUnityDirection(ControlPointOffsets[INDEX_TARGET_2]);

        Vector3 unityEndPoint2 = sourceN2U.ToUnityPosition( sourceSocket2.Anchor) + sourceN2U.ToUnityDirection( SocketOffsets[INDEX_SOURCE_2] );
        Vector3 bezierEndPoint2 = unityEndPoint2 + sourceN2U.ToUnityDirection(ControlPointOffsets[INDEX_SOURCE_2]);

        Vector3 direction2 = unityEndPoint2 - unityStartPoint2;
        float dir2len = direction2.magnitude;

        float travelled = 0.0f;
        float distance = dir1len > dir2len ? dir1len : dir2len;
        if (distance > MaxDistance)
        {
            distance = MaxDistance;
        }

        float StepSize = TheStepSize / distance;

        // draw the trail
        while(travelled < distance)
        {
            float alpha = (travelled / distance);
            float idx = alpha * (float)(NumColorSteps - 1);
            byte color = Colors[(int)Mathf.Round(idx)];

            Vector3 currentP1 = NPVoxGeomUtil.GetBezierPoint(unityStartPoint1, bezierStartPoint1, bezierEndPoint1, unityEndPoint1, alpha);
            Vector3 currentP2 = NPVoxGeomUtil.GetBezierPoint(unityStartPoint2, bezierStartPoint2, bezierEndPoint2, unityEndPoint2, alpha);
            Vector3 currentP1vox = productN2U.ToSaveVoxCoord(currentP1);
            Vector3 currentP2vox = productN2U.ToSaveVoxCoord(currentP2);
            NPVoxGeomUtil.DrawLine(productModel, currentP1vox, currentP2vox, color, theVoxelGroup, false);
//            currentP1 += direction1 * stepSize1;
//            currentP2 += direction2 * stepSize2;

            travelled += StepSize;
        }

        productModel.Colortable = colorTable;
        productModel.RecalculateNumVoxels();
        return productModel;
    }

    #if UNITY_EDITOR
    override public bool DrawInspector(NPipeEditFlags flags)
    {
        bool changed = base.DrawInspector(flags & ~NPipeEditFlags.INPUT);

        if (Input != null)
        {
            string newInputValue = NPVoxGUILayout.DrawSocketSelector("Socket 1", SocketName1, Input as NPVoxIModelFactory);
            if (newInputValue != SocketName1)
            {
                SocketName1 = newInputValue;
                changed = true;
            }
            string newTargetValue = NPVoxGUILayout.DrawSocketSelector("Socket 2", SocketName2, Input as NPVoxIModelFactory);
            if (newTargetValue != SocketName2)
            {
                SocketName2 = newTargetValue;
                changed = true;
            }
        }


        return changed;
    }

    public string[] GetSceneEditingTools()
    {
        string str = "Toggle Socket";

        return new string[] { str, "Offset", "Control Point" };
//        return new string[] { };
    }

    private int currentEditedSocket = 0;

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

        if (!(PreviousFrame is NPVoxIModelFactory))
        {
            return null;
        }

        if (SocketOffsets.Length < 4 || ControlPointOffsets.Length < 4)
        {
            ResetSceneTools();
        }

        NPVoxToUnity sourceN2U = new NPVoxToUnity(((NPVoxIModelFactory)PreviousFrame).GetProduct(), npVoxToUnity.VoxeSize);
        NPVoxSocket sourceSocket1 = ((NPVoxIModelFactory)PreviousFrame).GetProduct().GetSocketByName(SocketName1);
        NPVoxSocket sourceSocket2 = ((NPVoxIModelFactory)PreviousFrame).GetProduct().GetSocketByName(SocketName2);

        NPVoxModel targetModel = model;

        if (TargetFrame is NPVoxIModelFactory)
        {
            targetModel = ((NPVoxIModelFactory)TargetFrame).GetProduct() as NPVoxModel;
        }
        NPVoxToUnity targetN2U = new NPVoxToUnity(targetModel, npVoxToUnity.VoxeSize);

        NPVoxSocket targetSocket1 = targetModel.GetSocketByName(SocketName1);
        NPVoxSocket targetSocket2 = targetModel.GetSocketByName(SocketName2);

        if (targetSocket1.IsInvalid())
        {
            Debug.LogWarning("SocketName1 not found in targetModel");
            return null;
        }

        if (targetSocket2.IsInvalid())
        {
            Debug.LogWarning("SocketName2 not found in targetModel");
            return null;
        }

        NPVoxSocket[] sockets = new NPVoxSocket[4];
        sockets[INDEX_SOURCE_1] = sourceSocket1;
        sockets[INDEX_SOURCE_2] = sourceSocket2;
        sockets[INDEX_TARGET_1] = targetSocket1;
        sockets[INDEX_TARGET_2] = targetSocket2;

        NPVoxToUnity[] n2u = new NPVoxToUnity[4];
        n2u[INDEX_SOURCE_1] = sourceN2U;
        n2u[INDEX_SOURCE_2] = sourceN2U;
        n2u[INDEX_TARGET_1] = targetN2U;
        n2u[INDEX_TARGET_2] = targetN2U;

        NPVoxToUnity n = n2u[currentEditedSocket];

        Vector3 pos = n.ToUnityPosition( sockets[currentEditedSocket].Anchor );

        if (tool == 0)
        {
            if (Event.current.type == EventType.MouseDown)
            {
                currentEditedSocket = (currentEditedSocket + 1) % 4;
            }

            switch (currentEditedSocket)
            {
                case INDEX_TARGET_1:
                    Handles.color = Color.green;
                    break;
                case INDEX_TARGET_2:
                    Handles.color = Color.green;
                    break;
                case INDEX_SOURCE_1:
                    Handles.color = Color.yellow;
                    break;
                case INDEX_SOURCE_2:
                    Handles.color = Color.yellow;
                    break;
            }
            Handles.CubeCap(0, pos, Quaternion.identity, 0.5f);
        }

        // offset 
        Vector3 offset = n.ToUnityDirection( SocketOffsets[currentEditedSocket] );
        if (tool == 1)
        {
            offset = n.ToSaveVoxDirection( Handles.PositionHandle(pos + offset, Quaternion.identity) - pos );
            if (offset != SocketOffsets[currentEditedSocket])
            {
                return (NPVoxISceneEditable t) =>
                {
                    (t as NPVoxTrailGenerator).SocketOffsets[currentEditedSocket] = offset;
                    return true;
                };
            }
        }

        Vector3 controlOffset = n.ToUnityDirection( ControlPointOffsets[currentEditedSocket] );
        // Control Point
        if (tool == 2)
        {
            controlOffset = n.ToSaveVoxDirection( Handles.PositionHandle(pos + offset + controlOffset, Quaternion.identity) - pos - offset );
            if (controlOffset != ControlPointOffsets[currentEditedSocket])
            {
                return (NPVoxISceneEditable t) =>
                {
                    (t as NPVoxTrailGenerator).ControlPointOffsets[currentEditedSocket] = controlOffset;
                    return true;
                };
            }
        }

        return null;
    }

    public void ResetSceneTools()
    {
        SocketOffsets = new Vector3[]{ Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        ControlPointOffsets = new Vector3[]{ Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
    }

    #endif

}