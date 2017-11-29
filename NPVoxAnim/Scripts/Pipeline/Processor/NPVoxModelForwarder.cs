using UnityEngine;
using System.Collections.Generic;

[NPipeAppendableAttribute("Model Forwarder", typeof(NPVoxIModelFactory), false, false)]
public class NPVoxModelForwarder : NPVoxForwarderBase<NPVoxIModelFactory, NPVoxModel>, NPVoxIModelFactory
{
    override public string GetTypeName()
    {
        return "Model Forwarder";
    }

    override public NPVoxModel GetProduct()
    {
        if (Input == null)
        {
            return null;
        }

        NPVoxModel model = ((NPVoxIModelFactory)Input).GetProduct();
        if (!model)
        {
            return null;
        }

        return model;
    }
}