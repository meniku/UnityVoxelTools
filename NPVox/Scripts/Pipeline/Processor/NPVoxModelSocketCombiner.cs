using UnityEngine;

[NPipeAppendableAttribute("Model Socket Combiner", typeof(NPVoxIModelFactory), true, true)]
public class NPVoxModelSocketCombiner : NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel>, NPVoxIModelFactory
{
    // Voxel Group for the Socketable model
    public byte socketVoxelGroupIndex = 0;

    // name of the socket in the input model
    [HideInInspector]
    public string InputSocketName = "";

    // name of the socket in the socketable model
    [HideInInspector]
    public string TargetSocketName = "";
    
    // Socketable to combine
    [NPipeSelectorAttribute(typeof(NPVoxIModelFactory))]
    public UnityEngine.Object Target;

    // method to use to resolve transformation conflicts
    public NPVoxModelTransformationUtil.ResolveConflictMethodType ResolveConflictMethod = NPVoxModelTransformationUtil.ResolveConflictMethodType.FILL_GAPS;

    override public NPipeIImportable[] GetAllInputs()
    {
        if (Input != null && Target != null)
        {
            return new NPipeIImportable[] { (NPipeIImportable)Input, (NPipeIImportable)Target };
        }
        else if (Input != null)
        {
            return new NPipeIImportable[] { (NPipeIImportable)Input };
        }
        else if (Target != null)
        {
            return new NPipeIImportable[] { (NPipeIImportable)Target };
        }
        else
        {
            return new NPipeIImportable[] { };
        }
    }

    override protected NPVoxModel CreateProduct(NPVoxModel reuse = null)
    {
        if (Input != null)
        {
            // todo voxel group

            NPVoxModel inputModel = ((NPVoxIModelFactory)Input).GetProduct();

            NPVoxIModelFactory targetFactory = this.Target as NPVoxIModelFactory; 
            if (targetFactory != null)
            {
                NPVoxModel targetModel = targetFactory.GetProduct();

                NPVoxSocket inputSocket = inputModel.GetSocketByName(InputSocketName);
                NPVoxSocket targetSocket = targetModel.GetSocketByName(TargetSocketName);

                NPVoxModel model = NPVoxModelTransformationUtil.SocketTransform(inputModel, targetModel, inputSocket, targetSocket, ResolveConflictMethod, reuse);
                model.name = "zzz Model Socket Combiner";
                return model;
            }
            else
            {
                reuse = NPVoxModel.NewInstance(inputModel, reuse);
                reuse.CopyOver(inputModel);
                return reuse;
            }
        }
        else
        {
//            return NPVoxModel.NewInvalidInstance(reuse, "No Input Setup");
            throw new NPipeException("No Input Setup");
        }
    }


    override public string GetTypeName()
    {
        return "Model Socket Combiner";
    }


    #if UNITY_EDITOR
    override public bool DrawInspector(NPipeEditFlags flags)
    {
        bool changed = base.DrawInspector(flags);

        if (Input != null)
        {
            string newInputValue = NPVoxGUILayout.DrawSocketSelector("Input Socket", InputSocketName, Input as NPVoxIModelFactory);
            if (newInputValue != InputSocketName)
            {
                InputSocketName = newInputValue;
                changed = true;
            }
            string newTargetValue = NPVoxGUILayout.DrawSocketSelector("Target Socket", TargetSocketName, Target as NPVoxIModelFactory);
            if (newTargetValue != TargetSocketName)
            {
                TargetSocketName = newTargetValue;
                changed = true;
            }
        }

        return changed;
    }

    #endif
}