using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPVoxModelTransformationUtil 
{
    [System.Serializable]
    public enum ResolveConflictMethodType
    {
        NONE,
        CLOSEST,
        FILL_GAPS
    }

    public static NPVoxModel SocketTransform( NPVoxModel sourceModel, NPVoxModel targetModel, NPVoxSocket sourceSocket, NPVoxSocket targetSocket, ResolveConflictMethodType resolveConflictMethod = ResolveConflictMethodType.CLOSEST, NPVoxModel reuse = null )
    {
        NPVoxToUnity sourceVox2Unity = new NPVoxToUnity(sourceModel, Vector3.one);
        NPVoxToUnity targetVox2Unity = new NPVoxToUnity(targetModel, Vector3.one);

        Vector3 sourceAnchorPos = sourceVox2Unity.ToUnityPosition(sourceSocket.Anchor);
        Quaternion sourceRotation = Quaternion.Euler( sourceSocket.EulerAngles );
        Vector3 targetAnchorPos = targetVox2Unity.ToUnityPosition(targetSocket.Anchor);
        Quaternion targetRotation = Quaternion.Euler( targetSocket.EulerAngles );
        Vector3 diff = sourceVox2Unity.ToSaveVoxDirection( targetAnchorPos - sourceAnchorPos );

        Matrix4x4 A = Matrix4x4.TRS(sourceVox2Unity.ToSaveVoxCoord(sourceAnchorPos), Quaternion.identity, Vector3.one);
        Matrix4x4 B = Matrix4x4.TRS(Vector3.zero, sourceRotation , Vector3.one).inverse * Matrix4x4.TRS(Vector3.zero, targetRotation, Vector3.one);
        Matrix4x4 C = Matrix4x4.TRS(-sourceVox2Unity.ToSaveVoxCoord(sourceAnchorPos), Quaternion.identity, Vector3.one);
        Matrix4x4 D = Matrix4x4.TRS(diff, Quaternion.identity, Vector3.one);

        Matrix4x4 transformMatrix = D * A * B * C;
        
        return Transform(sourceModel, sourceModel.BoundingBox, transformMatrix, resolveConflictMethod, reuse);
    }

    public static NPVoxModel MatrixTransform( NPVoxModel sourceModel, NPVoxBox affectedArea, Matrix4x4 matrix, Vector3 pivot, ResolveConflictMethodType resolveConflictMethod = ResolveConflictMethodType.CLOSEST, NPVoxModel reuse = null )
    {
        Vector3 pivotPoint = affectedArea.SaveCenter + pivot;
        Matrix4x4 transformMatrix = (Matrix4x4.TRS(pivotPoint, Quaternion.identity, Vector3.one) * matrix) * Matrix4x4.TRS(-pivotPoint, Quaternion.identity, Vector3.one);

        return Transform(sourceModel, affectedArea, transformMatrix, resolveConflictMethod, reuse);
    }

    public static NPVoxBoneModel MatrixTransform( NPVoxBoneModel sourceModel, NPVoxBox affectedArea, uint boneMask, Matrix4x4 matrix, Vector3 pivot, ResolveConflictMethodType resolveConflictMethod = ResolveConflictMethodType.CLOSEST, NPVoxModel reuse = null, byte markColor = 0 )
    {
        Vector3 pivotPoint = affectedArea.SaveCenter + pivot;
        Matrix4x4 transformMatrix = (Matrix4x4.TRS(pivotPoint, Quaternion.identity, Vector3.one) * matrix) * Matrix4x4.TRS(-pivotPoint, Quaternion.identity, Vector3.one);

        return Transform(sourceModel, affectedArea, boneMask, transformMatrix, resolveConflictMethod, reuse, markColor);
    }

    public static NPVoxModel Transform( NPVoxModel sourceModel, NPVoxBox affectedArea, Matrix4x4 transformMatrix, ResolveConflictMethodType resolveConflictMethod = ResolveConflictMethodType.CLOSEST, NPVoxModel reuse = null )
    {
        NPVoxBox clampedBox = sourceModel.Clamp(affectedArea);

        // calculate size & offset for new model
        NPVoxCoord size = sourceModel.Size;
        NPVoxCoord offset = NPVoxCoord.ZERO;
        {
            NPVoxBox parentBounds = sourceModel.BoundingBox;
            NPVoxBox thisBounds = parentBounds.Clone();

            // transform voxels
            foreach (NPVoxCoord coord in clampedBox.Enumerate())
            {
                Vector3 saveCoord = transformMatrix.MultiplyPoint(NPVoxCoordUtil.ToVector(coord));
                NPVoxCoord newCoord = NPVoxCoordUtil.ToCoord(saveCoord);
                if (!sourceModel.IsInside(newCoord))
                {
                    thisBounds.EnlargeToInclude(newCoord);
                }
            }
            // transform sockets
            foreach (NPVoxSocket socket in sourceModel.Sockets)
            {
                NPVoxCoord newCoord = NPVoxCoordUtil.ToCoord(transformMatrix.MultiplyPoint(NPVoxCoordUtil.ToVector(socket.Anchor)));
                if (clampedBox.Contains(socket.Anchor) && !sourceModel.IsInside(newCoord))
                {
                    thisBounds.EnlargeToInclude(newCoord);
                }
            }

           CalculateResizeOffset(parentBounds, thisBounds, out offset, out size);
        }


        bool hasVoxelGroups = sourceModel.HasVoxelGroups();
        NPVoxBoneModel sourceBoneModel = sourceModel as NPVoxBoneModel;
        bool hasBoneGropus = sourceBoneModel != null;

        NPVoxModel transformedModel = NPVoxModel.NewInstance(sourceModel, size, reuse);
        NPVoxBoneModel transformedBoneModel = transformedModel as NPVoxBoneModel;
        if (hasVoxelGroups)
        {
            transformedModel.InitVoxelGroups();
            transformedModel.NumVoxelGroups = sourceModel.NumVoxelGroups;
        }
        if (hasBoneGropus)
        {
            transformedBoneModel.AllBones = NPVoxBone.CloneBones(sourceBoneModel.AllBones);
        }

        // 1. copy all voxels over that are not affected by the transformation
        transformedModel.NumVoxels = sourceModel.NumVoxels;
        transformedModel.Colortable = sourceModel.Colortable;
        foreach (NPVoxCoord coord in sourceModel.EnumerateVoxels())
        {
            NPVoxCoord movedCoord = coord + offset;
            if (!clampedBox.Contains(coord))
            {
                transformedModel.SetVoxel(movedCoord, sourceModel.GetVoxel(coord));
                if (hasVoxelGroups)
                {
                    transformedModel.SetVoxelGroup(movedCoord, sourceModel.GetVoxelGroup(coord));
                }
                if (hasBoneGropus)
                {
                    transformedBoneModel.SetBoneMask(movedCoord, sourceBoneModel.GetBoneMask(coord));
                }
            }
        }

        // 2. copy all voxels that can be tranformed without conflict,
        Dictionary<NPVoxCoord, Vector3> conflictVoxels = new Dictionary<NPVoxCoord, Vector3>();

        foreach (NPVoxCoord sourceCoord in clampedBox.Enumerate())
        {
            if (sourceModel.HasVoxelFast(sourceCoord))
            {
                Vector3 saveCoord = transformMatrix.MultiplyPoint(NPVoxCoordUtil.ToVector(sourceCoord));
                Vector3 targetCoordSave = saveCoord + NPVoxCoordUtil.ToVector(offset);
                NPVoxCoord targetCoord = NPVoxCoordUtil.ToCoord(targetCoordSave);

                if (!transformedModel.HasVoxelFast(targetCoord))
                {
                    transformedModel.SetVoxel(targetCoord, sourceModel.GetVoxel(sourceCoord));
                    if (hasVoxelGroups)
                    {
                        transformedModel.SetVoxelGroup(targetCoord, sourceModel.GetVoxelGroup(sourceCoord));
                    }
                    if (hasBoneGropus)
                    {
                        transformedBoneModel.SetBoneMask(targetCoord, sourceBoneModel.GetBoneMask(sourceCoord));
                    }
                }
                else
                {
                    conflictVoxels[sourceCoord] = targetCoordSave;
                }
            }
        }

        // 3. try to fit in voxels that had conflicts
        int numberOfConflictsSolved = 0;
        if (resolveConflictMethod  != ResolveConflictMethodType.NONE)
        {
            foreach (NPVoxCoord sourceCoord in conflictVoxels.Keys)
            {
                if( sourceModel.HasVoxelFast(sourceCoord) )
                {
                    Vector3 targetSaveCoord = conflictVoxels[sourceCoord];
                    NPVoxCoord nearbyCoord = GetNearbyCoord(transformedModel, targetSaveCoord, resolveConflictMethod);
                    if (! nearbyCoord.Equals( NPVoxCoord.INVALID) )
                    {
                        transformedModel.SetVoxel(nearbyCoord, sourceModel.GetVoxel(sourceCoord));
                        if (hasVoxelGroups)
                        {
                            transformedModel.SetVoxelGroup(nearbyCoord, sourceModel.GetVoxelGroup(sourceCoord));
                        }
                        if (hasBoneGropus)
                        {
                            transformedBoneModel.SetBoneMask(nearbyCoord, sourceBoneModel.GetBoneMask(sourceCoord));
                        }
                        numberOfConflictsSolved++;
                    }
                }
            }

            if (numberOfConflictsSolved != conflictVoxels.Count)
            {
                Debug.Log(string.Format("transformation has resolved {0}/{1} conflicting voxels", numberOfConflictsSolved, conflictVoxels.Count));
            }
        }

        // 4. transform all sockets
        NPVoxSocket[] sockets = new NPVoxSocket[sourceModel.Sockets.Length];
        for (int i = 0; i < sockets.Length; i++)
        {
            NPVoxSocket socket = sourceModel.Sockets[i];
            if (clampedBox.Contains(socket.Anchor))
            {
                // transform anchor
                Vector3 saveOriginalAnchor = NPVoxCoordUtil.ToVector(socket.Anchor);
                Vector3 saveTargetAnchor = transformMatrix.MultiplyPoint(saveOriginalAnchor) + NPVoxCoordUtil.ToVector(offset);
                socket.Anchor = NPVoxCoordUtil.ToCoord(saveTargetAnchor);

                // transform Quaternion
                Quaternion originalRotation = Quaternion.Euler(socket.EulerAngles);
                Matrix4x4 rotated = (Matrix4x4.TRS(Vector3.zero, originalRotation, Vector3.one) * transformMatrix);
                socket.EulerAngles = Matrix4x4Util.GetRotation(rotated).eulerAngles;
            }
            else
            {
                socket.Anchor = socket.Anchor + offset;
            }
            sockets[i] = socket;
        }
        transformedModel.Sockets = sockets;


        // 5. count all voxels
        transformedModel.NumVoxels = transformedModel.NumVoxels - ( conflictVoxels.Count - numberOfConflictsSolved );
        transformedModel.RecalculateNumVoxels(true);
        return transformedModel;
    }

    public static NPVoxCoord GetNearbyCoord(NPVoxModel model, Vector3 saveCoord, ResolveConflictMethodType resolveConflictMethod)
    {
        NPVoxCoord favoriteCoord = NPVoxCoordUtil.ToCoord(saveCoord);
        if (model.HasVoxelFast(favoriteCoord))
        {
            NPVoxBox box = new NPVoxBox(favoriteCoord - NPVoxCoord.ONE, favoriteCoord + NPVoxCoord.ONE);
            favoriteCoord = NPVoxCoord.INVALID;
            float nearestDistance = 9999f;
            int favoriteEnclosingVoxelCount = -1;
            foreach (NPVoxCoord currentTestCoord in box.Enumerate())
            {
                if (model.IsInside(currentTestCoord) && !model.HasVoxelFast(currentTestCoord))
                {
                    if (resolveConflictMethod == ResolveConflictMethodType.CLOSEST)
                    {
                        float distance = Vector3.Distance(NPVoxCoordUtil.ToVector(currentTestCoord), saveCoord);
                        if (distance < nearestDistance)
                        {
                            nearestDistance = distance;
                            favoriteCoord = currentTestCoord;
                        }
                    }
                    else
                    {
                        int enclosingVoxelCount = 0;
                        NPVoxBox enclosingBoxCheck = new NPVoxBox(currentTestCoord - NPVoxCoord.ONE, currentTestCoord + NPVoxCoord.ONE);
                        foreach (NPVoxCoord enclosingTestCoord in enclosingBoxCheck.Enumerate())
                        {
                            if (model.IsInside(currentTestCoord) && model.HasVoxelFast(currentTestCoord))
                            {
                                enclosingVoxelCount++;
                            }
                        }

                        if (enclosingVoxelCount > favoriteEnclosingVoxelCount)
                        {
                            enclosingVoxelCount = favoriteEnclosingVoxelCount;
                            favoriteCoord = currentTestCoord;
                        }
                    }
                }
            }
        }
        return favoriteCoord;
    }

    public static NPVoxBoneModel Transform( NPVoxBoneModel sourceModel, NPVoxBox affectedArea, uint boneMask, Matrix4x4 transformMatrix, ResolveConflictMethodType resolveConflictMethod = ResolveConflictMethodType.CLOSEST, NPVoxModel reuse = null, byte markColor = 0 )
    {
        NPVoxBox clampedBox = sourceModel.Clamp(affectedArea);

        // calculate size & offset for new model
        NPVoxCoord size = sourceModel.Size;
        NPVoxCoord offset = NPVoxCoord.ZERO;
        {
            NPVoxBox parentBounds = sourceModel.BoundingBox;
            UnityEngine.Assertions.Assert.AreEqual(size, parentBounds.Size);
            NPVoxBox thisBounds = parentBounds.Clone();

            // transform voxels
            foreach (NPVoxCoord coord in clampedBox.Enumerate())
            {
                Vector3 saveCoord = transformMatrix.MultiplyPoint(NPVoxCoordUtil.ToVector(coord));
                NPVoxCoord newCoord = NPVoxCoordUtil.ToCoord(saveCoord);
                if (!sourceModel.IsInside(newCoord))
                {
                    thisBounds.EnlargeToInclude(newCoord);
                }
            }
            // transform sockets
            foreach (NPVoxSocket socket in sourceModel.Sockets)
            {
                NPVoxCoord newCoord = NPVoxCoordUtil.ToCoord(transformMatrix.MultiplyPoint(NPVoxCoordUtil.ToVector(socket.Anchor)));
                if (clampedBox.Contains(socket.Anchor) && !sourceModel.IsInside(newCoord))
                {
                    thisBounds.EnlargeToInclude(newCoord);
                }
            }

            CalculateResizeOffset(parentBounds, thisBounds, out offset, out size);
        }


        bool hasVoxelGroups = sourceModel.HasVoxelGroups();
        NPVoxBoneModel transformedModel = (NPVoxBoneModel) NPVoxModel.NewInstance(sourceModel, size, reuse);

        if (hasVoxelGroups)
        {
            transformedModel.InitVoxelGroups();
            transformedModel.NumVoxelGroups = sourceModel.NumVoxelGroups;
        }
        transformedModel.AllBones = NPVoxBone.CloneBones(sourceModel.AllBones);

        // 1. copy all voxels over that are not affected by the transformation
        transformedModel.NumVoxels = sourceModel.NumVoxels;
        transformedModel.Colortable = sourceModel.Colortable;
        foreach (NPVoxCoord coord in sourceModel.EnumerateVoxels())
        {
            NPVoxCoord movedCoord = coord + offset;
            if (!sourceModel.IsInBoneMask(coord, boneMask))
            {
                transformedModel.SetVoxel(movedCoord, sourceModel.GetVoxel(coord), sourceModel.GetBoneMask(coord));
                if (hasVoxelGroups)
                {
                    transformedModel.SetVoxelGroup(movedCoord, sourceModel.GetVoxelGroup(coord));
                }
            }
        }

        // 2. copy all voxels that can be tranformed without conflict,
        Dictionary<NPVoxCoord, Vector3> conflictVoxels = new Dictionary<NPVoxCoord, Vector3>();

        foreach (NPVoxCoord sourceCoord in clampedBox.Enumerate())
        {
            if (sourceModel.IsInBoneMask(sourceCoord, boneMask))
            {
                Vector3 saveCoord = transformMatrix.MultiplyPoint(NPVoxCoordUtil.ToVector(sourceCoord));
                Vector3 targetCoordSave = saveCoord + NPVoxCoordUtil.ToVector(offset);
                NPVoxCoord targetCoord = NPVoxCoordUtil.ToCoord(targetCoordSave);

                if (!transformedModel.HasVoxelFast(targetCoord))
                {
                    transformedModel.SetVoxel(targetCoord, markColor == 0 ? sourceModel.GetVoxel(sourceCoord) : markColor, sourceModel.GetBoneMask(sourceCoord));
                    if (hasVoxelGroups)
                    {
                        transformedModel.SetVoxelGroup(targetCoord, sourceModel.GetVoxelGroup(sourceCoord));
                    }
                }
                else
                {
                    conflictVoxels[sourceCoord] = targetCoordSave;
                }
            }
        }

        // 3. try to fit in voxels that had conflicts
        int numberOfConflictsSolved = 0;
        if (resolveConflictMethod != ResolveConflictMethodType.NONE)
        {
            foreach (NPVoxCoord sourceCoord in conflictVoxels.Keys)
            {
                if( sourceModel.IsInBoneMask(sourceCoord, boneMask) )
                {
                    Vector3 targetSaveCoord = conflictVoxels[sourceCoord];
                    NPVoxCoord nearbyCoord = GetNearbyCoord(transformedModel, targetSaveCoord, resolveConflictMethod);
                    if (!nearbyCoord.Equals( NPVoxCoord.INVALID) )
                    {
                        transformedModel.SetVoxel(nearbyCoord, markColor == 0 ? sourceModel.GetVoxel(sourceCoord) : markColor, sourceModel.GetBoneMask(sourceCoord));
                        if (hasVoxelGroups)
                        {
                            transformedModel.SetVoxelGroup(nearbyCoord, sourceModel.GetVoxelGroup(sourceCoord));
                        }
                        numberOfConflictsSolved++;
                    }
                }
            }
            if (numberOfConflictsSolved != conflictVoxels.Count)
            {
//                Debug.Log(string.Format("transformation has resolved {0}/{1} conflicting voxels", numberOfConflictsSolved, conflictVoxels.Count));
            }
        }

        // 4. transform all sockets
        NPVoxSocket[] sockets = new NPVoxSocket[sourceModel.Sockets.Length];
        for (int i = 0; i < sockets.Length; i++)
        {
            NPVoxSocket socket = sourceModel.Sockets[i];
            if (clampedBox.Contains(socket.Anchor))
            {
                // transform anchor
                Vector3 saveOriginalAnchor = NPVoxCoordUtil.ToVector(socket.Anchor);
                Vector3 saveTargetAnchor = transformMatrix.MultiplyPoint(saveOriginalAnchor) + NPVoxCoordUtil.ToVector(offset);
                socket.Anchor = NPVoxCoordUtil.ToCoord(saveTargetAnchor);

                // transform Quaternion
                Quaternion originalRotation = Quaternion.Euler(socket.EulerAngles);
                Matrix4x4 rotated = (Matrix4x4.TRS(Vector3.zero, originalRotation, Vector3.one) * transformMatrix);
                socket.EulerAngles = Matrix4x4Util.GetRotation(rotated).eulerAngles;
            }
            else
            {
                socket.Anchor = socket.Anchor + offset;
            }
            sockets[i] = socket;
        }
        transformedModel.Sockets = sockets;


        // 5. count all voxels
//        transformedModel.RecalculateNumVoxels();
        transformedModel.NumVoxels = transformedModel.NumVoxels - ( conflictVoxels.Count - numberOfConflictsSolved );

        return transformedModel;
    }

    public static NPVoxModel MatrixTransformSocket( NPVoxModel sourceModel, string socketName, Matrix4x4 matrix, Vector3 pivot, NPVoxModel reuse = null )
    {
        NPVoxSocket socket = sourceModel.GetSocketByName(socketName);
        Vector3 pivotPoint = NPVoxCoordUtil.ToVector( socket.Anchor ) + pivot;
        Matrix4x4 transformMatrix = (Matrix4x4.TRS(pivotPoint, Quaternion.identity, Vector3.one) * matrix) * Matrix4x4.TRS(-pivotPoint, Quaternion.identity, Vector3.one);

        return TransformSocket(sourceModel, socketName, transformMatrix, reuse);
    }

    public static NPVoxModel TransformSocket( NPVoxModel sourceModel, string socketName, Matrix4x4 transformMatrix, NPVoxModel reuse = null )
    {
        NPVoxModel transformedModel = null;

        transformedModel = NPVoxModel.NewInstance(sourceModel, reuse);
        transformedModel.CopyOver(sourceModel);

        NPVoxSocket[] sockets = new NPVoxSocket[sourceModel.Sockets.Length];
        for (int i = 0; i < sockets.Length; i++)
        {
            NPVoxSocket socket = sourceModel.Sockets[i];
            if (socket.Name == socketName)
            {
                // transform anchor
                Vector3 saveOriginalAnchor = NPVoxCoordUtil.ToVector(socket.Anchor);
                Vector3 saveTargetAnchor = transformMatrix.MultiplyPoint(saveOriginalAnchor);
                socket.Anchor = sourceModel.Clamp( NPVoxCoordUtil.ToCoord(saveTargetAnchor) );

                // transform Quaternion
                Quaternion originalRotation = Quaternion.Euler(socket.EulerAngles);
                Matrix4x4 rotated = (Matrix4x4.TRS(Vector3.zero, originalRotation, Vector3.one) * transformMatrix);
                socket.EulerAngles = Matrix4x4Util.GetRotation(rotated).eulerAngles;
            }
            sockets[i] = socket;
        }
        transformedModel.Sockets = sockets;

        return transformedModel;
    }

    public static NPVoxModel CreateWithNewSize(NPVoxModel source, NPVoxBox newBounds, NPVoxModel reuse = null)
    {
        NPVoxCoord delta;
        NPVoxCoord newSize;
        CalculateResizeOffset(source.BoundingBox, newBounds, out delta, out newSize);

        NPVoxModel newModel = NPVoxModel.NewInstance(source, newSize, reuse);
        newModel.NumVoxels = source.NumVoxels;
        newModel.NumVoxelGroups = source.NumVoxelGroups;
        newModel.Colortable = source.Colortable != null ? (Color32[]) source.Colortable.Clone() : null;
        newModel.Sockets = source.Sockets != null ? (NPVoxSocket[]) source.Sockets.Clone() : null;

        if (newModel.Sockets != null)
        {
            for (int i = 0; i < newModel.Sockets.Length; i++)
            {
                newModel.Sockets[i].Anchor = newModel.Sockets[i].Anchor + delta;
            }
        }

        bool hasVoxelGroups = source.HasVoxelGroups();

        if (hasVoxelGroups)
        {
            newModel.InitVoxelGroups();
            newModel.NumVoxelGroups = source.NumVoxelGroups;
        }

        foreach(NPVoxCoord coord in source.EnumerateVoxels())
        {
            NPVoxCoord targetCoord = coord + delta;
            newModel.SetVoxel(targetCoord, source.GetVoxel(coord));
            if (hasVoxelGroups)
            {
                newModel.SetVoxelGroup(targetCoord, source.GetVoxelGroup(coord));
            }
        }

//        newModel.InvalidateVoxelCache();

        return newModel;
    }

    public static void CalculateResizeOffset(NPVoxBox parentBounds, NPVoxBox thisBounds, out NPVoxCoord delta, out NPVoxCoord size)
    {
        if (!thisBounds.Equals(parentBounds))
        {
            size = parentBounds.Size;
            bool isOverflow = false;

            sbyte deltaX = (sbyte)(Mathf.Max(parentBounds.Left - thisBounds.Left, thisBounds.Right - parentBounds.Right));
            if ((int)deltaX * 2 + (int)size.X > 126) // check for overflow
            {
                deltaX = (sbyte)((float)deltaX - Mathf.Ceil(((float)deltaX * 2f + (float)size.X) - 126) / 2f);
                isOverflow = true;
            }

            sbyte deltaY = (sbyte)(Mathf.Max(parentBounds.Down - thisBounds.Down, thisBounds.Up - parentBounds.Up));
            if ((int)deltaY * 2 + (int)size.Y > 126) // check for overflow
            {
                deltaY = (sbyte)((float)deltaY - Mathf.Ceil(((float)deltaY * 2f + (float)size.Y) - 126) / 2f);
                isOverflow = true;
            }

            sbyte deltaZ = (sbyte)(Mathf.Max(parentBounds.Back - thisBounds.Back, thisBounds.Forward - parentBounds.Forward));
            if ((int)deltaZ * 2 + (int)size.Z > 126) // check for overflow
            {
                deltaZ = (sbyte)((float)deltaZ - Mathf.Ceil(((float)deltaZ * 2f + (float)size.Z) - 126) / 2f);
                isOverflow = true;
            }

            delta = new NPVoxCoord(deltaX, deltaY, deltaZ);
            size = size + delta + delta;

            if (isOverflow)
            {
                Debug.LogWarning("Transformed Model is large, clamped to " + size);
            }
        }
        else
        {
            size = parentBounds.Size;
            delta = NPVoxCoord.ZERO;
        }
    }
}
