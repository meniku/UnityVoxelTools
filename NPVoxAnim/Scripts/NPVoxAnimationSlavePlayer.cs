using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
public class NPVoxAnimationSlavePlayer : MonoBehaviour
{
    [SerializeField]
    private new NPVoxAnimation animation;

    [SerializeField, HideInInspector]
    private NPVoxAnimationPlayer animationPlayer;

    private MeshFilter MeshFilter;

    public NPVoxAnimation Animation
    {
        get
        {
            return animation;
        }
        set
        {
//            Debug.Log("Slave Animation Set: " + value.name);
            animation = value;
            if (animation != null)
            {
                animation.EnsureAllMeshesLoaded();
            }
            SyncFrame();
        }
    }

    public NPVoxAnimationPlayer AnimationPlayer
    {
        get
        {
            return animationPlayer;
        }
        set
        {
            if (this.animationPlayer != null)
            {
                animationPlayer.OnKeyFrameEntered -= OnKeyFrameEntered;
            }
            animationPlayer = value;
            if (animationPlayer != null)
            {
                animationPlayer.OnKeyFrameEntered += OnKeyFrameEntered;
                SyncFrame();
            }
        }
    }

    protected void Start()
    {
        MeshFilter = this.GetComponent<MeshFilter>();
        if (animation != null)
        {
            animation.EnsureAllMeshesLoaded();
        }
    }

    protected void OnEnable()
    {
        if (animationPlayer != null)
        {
            animationPlayer.OnKeyFrameEntered += OnKeyFrameEntered;
        }
    }

    protected void OnDisable()
    {
        if (animationPlayer != null)
        {
            animationPlayer.OnKeyFrameEntered -= OnKeyFrameEntered;
        }
    }

    protected void OnKeyFrameEntered(NPVoxAnimationEventArgs args)
    {
        SyncFrame();
    }

    protected void SyncFrame()
    {
        if (enabled && MeshFilter != null && animationPlayer && animationPlayer.CurrentKeyFrame >= 0 && animation && animationPlayer.CurrentKeyFrame < animation.Frames.Length && AnimationPlayer)
        {
            MeshFilter.sharedMesh = animation.Frames[animationPlayer.CurrentKeyFrame].GetTransformedMesh();
        }
    }
}