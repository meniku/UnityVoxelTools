using UnityEngine;
using System.Collections.Generic;

[NPipeAppendableAttribute("Model Combiner", typeof(NPVoxIModelFactory), false, true)]
public class NPVoxModelCombiner : NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel>, NPVoxIModelFactory
{
    [System.Flags]
    public enum Pivot
    {
        None = 0x0,
        Right = 0x1,
        Up = 0x2,
        Forward = 0x4,
    }

    [System.Serializable]
    public class NPVoxModificationSource
    {
        [NPipeSelectorAttribute(typeof(NPVoxIModelFactory))]
        public UnityEngine.Object Source; // TODO: Rename input
        public byte VoxelGroupIndex = 0;
        public Pivot Pivot;
    }

    [SerializeField]
    public NPVoxModificationSource[] Sources = new NPVoxModificationSource[0];

    override public NPipeIImportable Input
    {
        set
        {
            base.Input = null;
            NPVoxModelCombiner.NPVoxModificationSource source = new NPVoxModelCombiner.NPVoxModificationSource();
            source.Source = value as UnityEngine.Object;
            Sources = new NPVoxModelCombiner.NPVoxModificationSource[] { source };
        }
        get
        {
            return Sources[0].Source as NPipeIImportable;
        }
    }

    override public NPipeIImportable[] GetAllInputs()
    {
        NPipeIImportable[] sources = new NPipeIImportable[Sources.Length];
        int i = 0;
        foreach (NPVoxModificationSource source in Sources)
        {
            sources[i++] = source.Source as NPVoxIModelFactory;
        }
        return sources;
    }

    override protected NPVoxModel CreateProduct(NPVoxModel reuse = null)
    {
        if (Sources.Length > 0)
        {
            bool hasVoxelGroups = System.Array.FindAll(Sources, s => s.VoxelGroupIndex != 0).Length > 0;
            byte numVoxelGroups = 1;
            var firstSource = Sources[0];

            if (!firstSource.Source)
            {
//                foreach (NPipeIImportable input in NPipelineUtils.EachSource(this))
//                {
//                    if (!((UnityEngine.Object)input))
//                    {
//                        return NPVoxModel.NewInvalidInstance(reuse, "XXXXX First Source didn't have a factory set");
//                    }
//                }

//                return NPVoxModel.NewInvalidInstance(reuse, "First Source didn't have a factory set");
                throw new NPipeException("First Source didn't have a factory set");
            }
            NPVoxModel firstProduct = ((NPVoxIModelFactory)firstSource.Source).GetProduct();

            NPVoxModel combinedModel = NPVoxModel.NewInstance(firstProduct, reuse);

            if (firstSource.VoxelGroupIndex >= numVoxelGroups)
            {
                numVoxelGroups = (byte)(firstSource.VoxelGroupIndex + 1);
            }

            if (hasVoxelGroups)
            {
                combinedModel.InitVoxelGroups();

                foreach (NPVoxCoord coord in combinedModel.EnumerateVoxels())
                {
                    combinedModel.SetVoxelGroup(coord, firstSource.VoxelGroupIndex);
                }
            }

            Color32[] colorTable = firstProduct.Colortable;

            bool[] colors = NPVoxModelUtils.GetUsedColors(firstProduct);

            for (int i = 0; i < Sources.Length; i++)
            {
                if (!Sources[i].Source)
                {
//                    foreach (NPipeIImportable input in NPipelineUtils.EachSource(this))
//                    {
//                        if (!((UnityEngine.Object)input))
//                        {
//                            return NPVoxModel.NewInvalidInstance(reuse, "XXXXX Source " + i + " didn't have a factory set");
//                        }
//                    }
//
//
//                    return NPVoxModel.NewInvalidInstance(reuse, "Source " + i + " didn't have a factory set");

                    throw new NPipeException( "Source " + i + " didn't have a factory set");
                }

                NPVoxModel voxModel = ((NPVoxIModelFactory)Sources[i].Source).GetProduct();

//                numVoxels += voxModel.NumVoxels;

                Pivot pivot = Sources[i].Pivot;
                NPVoxCoord offset = NPVoxCoord.ZERO;
                byte voxelGroupIndex = Sources[i].VoxelGroupIndex;

                if ((pivot & Pivot.Right) == Pivot.Right)
                {
                    offset.X = (sbyte)(combinedModel.SizeX - voxModel.SizeX);
                }

                if ((pivot & Pivot.Up) == Pivot.Up)
                {
                    offset.Y = (sbyte)(combinedModel.SizeY - voxModel.SizeY);
                }

                if ((pivot & Pivot.Forward) == Pivot.Forward)
                {
                    offset.Z = (sbyte)(combinedModel.SizeZ - voxModel.SizeZ);
                }

                Dictionary<byte, byte> mapping = new Dictionary<byte, byte>();
                foreach (NPVoxCoord coord in voxModel.EnumerateVoxels())
                {
                    NPVoxCoord targetCoord = coord + offset;
                    byte sourceColor = voxModel.GetVoxel(coord);
                    byte targetColor = 0;
                    if (!mapping.ContainsKey(sourceColor))
                    {
                        targetColor = NPVoxModelUtils.FindUnusedColor(ref colors);
                        colorTable[targetColor] = voxModel.Colortable[sourceColor];
                        mapping.Add(sourceColor, targetColor);
                    }

                    targetColor = mapping[sourceColor];
                    combinedModel.SetVoxel(targetCoord, targetColor);
                    if (hasVoxelGroups)
                    {
                        combinedModel.SetVoxelGroup(targetCoord, voxelGroupIndex);
                    }
                }
                if (Sources[i].VoxelGroupIndex >= numVoxelGroups)
                {
                    numVoxelGroups = (byte)(Sources[i].VoxelGroupIndex + 1);
                }
            }

//            combinedModel.NumVoxels = numVoxels;
            combinedModel.RecalculateNumVoxels();
            combinedModel.NumVoxelGroups = numVoxelGroups;
            combinedModel.Colortable = colorTable;
            combinedModel.name = "zzz Combined Model";
            return combinedModel;
        }
        else
        {
            return NPVoxModel.NewInvalidInstance(reuse, "No Sources Setup");
        }
    }



    override public string GetTypeName()
    {
        return "Model Combiner";
    }

#if UNITY_EDITOR
    override public bool DrawInspector(NPipeEditFlags flags)
    {
        return base.DrawInspector(flags & ~NPipeEditFlags.INPUT);
    }
#endif
}