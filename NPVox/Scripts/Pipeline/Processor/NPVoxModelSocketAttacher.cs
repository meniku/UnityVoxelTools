using UnityEngine;
using System.Collections.Generic;

[NPipeAppendableAttribute("Socket Attacher", typeof(NPVoxIModelFactory), true, true)]
public class NPVoxModelSocketAttacher : NPVoxCompositeProcessorBase<NPVoxIModelFactory, NPVoxModel>, NPVoxIModelFactory
{
    public NPVoxSocket[] AddSockets = new NPVoxSocket[]{};

    override protected NPVoxModel CreateProduct(NPVoxModel reuse = null)
    {
        if(Input == null)
        {
            return NPVoxModel.NewInvalidInstance(reuse, "No Input Setup");
        }
        else
        {
            NPVoxModel sourceModel = ((NPVoxIModelFactory)Input).GetProduct();

            NPVoxModel productModel = NPVoxModel.NewInstance(sourceModel, reuse);
            productModel.CopyOver(sourceModel);
            if (sourceModel.IsValid)
            {
                foreach (NPVoxCoord coord in sourceModel.BoundingBox.Enumerate())
                {
                    if (sourceModel.HasVoxel(coord))
                    {
                        productModel.SetVoxel(coord, sourceModel.GetVoxel(coord));
                    }
                }

                /// add all the sockets
                List<NPVoxSocket> productSockets = new List<NPVoxSocket>();
                productSockets.AddRange(productModel.Sockets);
                productSockets.AddRange(AddSockets);
                productModel.Sockets = productSockets.ToArray();
            }
            else
            {
                Debug.LogWarning("Couldn't create model: source was not valid [ TRY REIMPORTING ]");
            }

            return productModel;
        }
    }

    public override string GetTypeName()
    {
        return "Socket Attacher";
    }
}