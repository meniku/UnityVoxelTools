using UnityEngine;

[NPipeAppendableAttribute("Model Slicer", typeof(NPVoxIModelFactory), true, true)]
public class NPVoxModelSlicer : NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel>, NPVoxIModelFactory
{
    public NPVoxBox slice;

    override protected NPVoxModel CreateProduct(NPVoxModel reuse = null)
    {
        if(Input == null)
        {
            return NPVoxModel.NewInvalidInstance(reuse, "No Input Setup");
        }
        else
        {
            NPVoxModel model = ((NPVoxIModelFactory)Input).GetProduct();

            return CreateSlicedModel(model, reuse);
        }
    }

    private NPVoxModel CreateSlicedModel(NPVoxModel source, NPVoxModel reuse)
    {
        NPVoxBox targetBox = slice.Clone();
        NPVoxBox sourceBounds = source.BoundingBox;
        targetBox.Clamp(source.BoundingBox);
        
        NPVoxCoord origin = targetBox.LeftDownBack;
        NPVoxModel model = NPVoxModel.NewInstance(source, targetBox.Size, reuse);
        int numVoxels = 0;
        foreach(NPVoxCoord coord in targetBox.Enumerate())
        {
            if( source.HasVoxel( coord ) )
            {
                numVoxels ++;
                model.SetVoxel( coord - origin, source.GetVoxel(coord) );
            }
        }

        model.NumVoxels = numVoxels;
        model.Colortable = source.Colortable;

        return model;
    }


    public override string GetTypeName()
    {
        return "Model Slicer";
    }
}