using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[NPipeAppendableAttribute("Socket Transformer", typeof(NPVoxIModelFactory), true, true)]
public class NPVoxSocketTransformer : NPVoxModelTransformerBase, NPVoxIModelFactory
{
    [HideInInspector]
    public Matrix4x4 Matrix = Matrix4x4.identity;

    [HideInInspector]
    public Vector3 PivotOffset = Vector3.zero; // offset from AffectedArea.SaveCenter

    [SerializeField, HideInInspector]
    private string quaternionReadFromSocketName = "";

//    [SerializeField, HideInInspector]
//    private string translationReadFromSocketName = "";

    [HideInInspector]
    public string SocketName = "";

    public bool Absolute = true;

    override public string GetTypeName()
    {
        return "Socket Transformer";
    }

    override protected NPVoxModel CreateProduct(NPVoxModel reuse = null)
    {
        if (Input == null)
        {
            return NPVoxModel.NewInvalidInstance(reuse, "No Input Setup");;
        }

        NPVoxModel model = ((NPVoxIModelFactory)Input).GetProduct();

        if (Absolute)
        {
            NPVoxModel newModel = NPVoxModel.NewInstance(model, reuse);
            newModel.CopyOver(model);
            
            NPVoxSocket socket = model.GetSocketByName(SocketName);
            socket.Anchor = NPVoxCoordUtil.ToCoord(GetTranslation() + GetPivot());
            socket.EulerAngles = GetRotation().eulerAngles;
            newModel.SetSocket(socket);
            
            return newModel;
        }
        else
        {
            return NPVoxModelTransformationUtil.MatrixTransformSocket(model, SocketName, Matrix, PivotOffset, reuse);
        }
    }

    #if UNITY_EDITOR
    override public bool DrawInspector(NPipeEditFlags flags)
    {
        bool changed = base.DrawInspector(flags);

        NPVoxIModelFactory modelFactory = Input as NPVoxIModelFactory;
        if (modelFactory != null)
        {
            string[] socketNames = modelFactory.GetProduct().SocketNames;
            if (socketNames == null || socketNames.Length == 0)
            {
                GUILayout.Label("No Socket");
            }
            else
            {
                string newSocketName = NPipeGUILayout.Toolbar(socketNames, socketNames, SocketName); 
                if (SocketName != newSocketName)
                {
                    SocketName = newSocketName;
                    changed = true;
                }
            }
        }

        if (changed && (string.IsNullOrEmpty(InstanceName) || InstanceName.StartsWith("SocketT ")))
        {
            InstanceName = "SocketT " + SocketName;
        }

        return changed;
    }

    #endif
      
    // ===================================================================================================
    // Tools
    // ===================================================================================================

    override public void SetTranslation(Vector3 translation)
    {
        if (float.IsNaN(translation.x) || float.IsNaN(translation.y) || float.IsNaN(translation.z))
        {
            return;
        }
        Matrix = Matrix4x4.TRS(translation, GetRotation(), GetScale());
    }

    override public Vector3 GetTranslation()
    {
//        if (Absolute)
//        {
//            NPVoxIModelFactory modelFactory = (Input as NPVoxIModelFactory);
//            if (modelFactory != null && translationReadFromSocketName != SocketName)
//            {
//                translationReadFromSocketName = SocketName;
//                NPVoxSocket socket = modelFactory.GetProduct().GetSocketByName(SocketName);
//                Matrix = Matrix4x4.TRS(NPVoxCoordUtil.ToVector(socket.Anchor), GetRotation(), Vector3.one);
//            }
//        }
        return Matrix4x4Util.GetPosition(Matrix);// - GetPivot();
    }

    override public void SetRotation(Quaternion quat)
    {
        if (float.IsNaN(quat.x) || float.IsNaN(quat.y) || float.IsNaN(quat.z) || float.IsNaN(quat.w))
        {
            return;
        }
        Matrix = Matrix4x4.TRS(GetTranslation(), quat, GetScale());
    }

    override public Quaternion GetRotation()
    {
        if (Absolute)
        {
            NPVoxIModelFactory modelFactory = (Input as NPVoxIModelFactory);
            if (modelFactory != null && quaternionReadFromSocketName != SocketName)
            {
                quaternionReadFromSocketName = SocketName;
                NPVoxSocket socket = modelFactory.GetProduct().GetSocketByName(SocketName);
                Matrix = Matrix4x4.TRS(GetTranslation(), Quaternion.Euler(socket.EulerAngles), Vector3.one);
            }
        }
        return Matrix4x4Util.GetRotation(Matrix);
    }

    override public void SetScale(Vector3 scale)
    {
        if (float.IsNaN(scale.x) || float.IsNaN(scale.y) || float.IsNaN(scale.z))
        {
            return;
        }
        Matrix = Matrix4x4.TRS(GetTranslation(), GetRotation(), scale);
    }

    override  public Vector3 GetScale()
    {
        return Matrix4x4Util.GetScale(Matrix);
    }

    override public void ResetSceneTools()
    {
        Matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
        PivotOffset = Vector3.zero;
//        translationReadFromSocketName = "";
        quaternionReadFromSocketName = "";
    }

    override public void SetPivot(Vector3 pivot)
    {
        NPVoxIModelFactory modelFactory = (Input as NPVoxIModelFactory);
        if (modelFactory != null)
        {
            NPVoxSocket socket = modelFactory.GetProduct().GetSocketByName(SocketName);
            this.PivotOffset = pivot - NPVoxCoordUtil.ToVector( socket.Anchor );
        }
        else
        {
            this.PivotOffset = Vector3.zero;
        }
    }

    override public Vector3 GetPivot()
    {
        NPVoxIModelFactory modelFactory = (Input as NPVoxIModelFactory);
        if (modelFactory != null)
        {
            NPVoxSocket socket = modelFactory.GetProduct().GetSocketByName(SocketName);
            return this.PivotOffset + NPVoxCoordUtil.ToVector( socket.Anchor );
        }
        else
        {
            return Vector3.zero;
        }
    }
}