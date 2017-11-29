using UnityEngine;

[NPipeAppendableAttribute("Model Marker", typeof(NPVoxIModelFactory), true, true)]
public class NPVoxModelMarker : NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel>, NPVoxIModelFactory
{
    public NPVoxBox AffectedArea = NPVoxBox.INVALID;

    public uint boneMask = 0;
    public uint hiddenBonesMask = 0;

    override public string GetTypeName()
    {
        return "Model Marker";
    }

    override protected NPVoxModel CreateProduct(NPVoxModel reuse = null)
    {
        if (Input == null)
        {
            return NPVoxModel.NewInvalidInstance(reuse, "Input was null");
        }
        NPVoxModel inputModel = ((NPVoxIModelFactory) Input).GetProduct();

        bool hasVoxelGroups = inputModel.HasVoxelGroups();

        if (AffectedArea.Equals( NPVoxBox.INVALID) )
        {
            NPVoxModel model = NPVoxModel.NewInstance(inputModel, reuse);
            model.CopyOver(inputModel);
            return model;
        }

        NPVoxBox clampedBox = inputModel.Clamp(AffectedArea);

        NPVoxModel transformedModel = null;

        transformedModel = NPVoxModel.NewInstance(inputModel, reuse);
        if (hasVoxelGroups)
        {
            transformedModel.InitVoxelGroups();
        }
        transformedModel.NumVoxelGroups = inputModel.NumVoxelGroups;
        transformedModel.NumVoxels = inputModel.NumVoxels;
        transformedModel.Colortable = inputModel.Colortable != null ? (Color32[]) inputModel.Colortable.Clone() : null;
        transformedModel.Sockets = inputModel.Sockets != null ? (NPVoxSocket[]) inputModel.Sockets.Clone() : null;

        NPVoxBoneModel transformedBoneModel = transformedModel as NPVoxBoneModel;
        NPVoxBoneModel inputBoneModel = inputModel as NPVoxBoneModel;
        bool isBoneModel = false;

        if (transformedBoneModel != null)
        {
            transformedBoneModel.AllBones = NPVoxBone.CloneBones( inputBoneModel.AllBones );
            isBoneModel = true;
        }
        
        byte brightenedColor = NPVoxModelUtils.FindUnusedColor(inputModel);
        
        if(brightenedColor == 0)
        {
            Debug.LogWarning("could not find a free color to brighten the model");
        }
        
        Color32 brightenColor32 = inputModel.Colortable[brightenedColor];

        foreach (NPVoxCoord coord in inputModel.EnumerateVoxels())
        {
            if (!isBoneModel)
            {
                if (clampedBox.Contains(coord) && brightenedColor != 0)
                {
                    brightenColor32 = inputModel.Colortable[inputModel.GetVoxel(coord)];
                    transformedModel.SetVoxel(coord, brightenedColor);
                }
                else
                {
                    transformedModel.SetVoxel(coord, inputModel.GetVoxel(coord));
                }
            }
            else
            {
                
                if (hiddenBonesMask == 0 || !inputBoneModel.IsInBoneMask(coord, hiddenBonesMask))
                {
                    if (clampedBox.Contains(coord) && brightenedColor != 0 && inputBoneModel.IsInBoneMask(coord, boneMask))
                    {
                        brightenColor32 = inputModel.Colortable[inputModel.GetVoxel(coord)];
                        transformedModel.SetVoxel(coord, brightenedColor);
                    }
                    else
                    {
                        transformedModel.SetVoxel(coord, inputModel.GetVoxel(coord));
                    }
                }
                transformedBoneModel.SetBoneMask( coord, inputBoneModel.GetBoneMask(coord) );
            }

            if (hasVoxelGroups)
            {
                transformedModel.SetVoxelGroup(coord, inputModel.GetVoxelGroup(coord));
            }
        }
        
        if(brightenedColor != 0)
        {
            transformedModel.Colortable[brightenedColor] = NPVoxModelUtils.BrightenColor(brightenColor32);
        }


        transformedModel.RecalculateNumVoxels(true);
        return transformedModel;
    }
}