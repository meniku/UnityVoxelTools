using UnityEngine;
using System.Collections;
using System;

public delegate void AnimationChangedEventHandler();
public delegate void AnimationEventHandler(NPVoxAnimationEventArgs e);

public class NPVoxAnimationEventArgs : EventArgs
{
    public NPVoxAnimationEventArgs(int frame)
    {
        this.frame = frame;
    }
    private int frame;
    public int Frame
    {
        get
        {
            return frame;
        }
    }
}

[RequireComponent(typeof(MeshFilter))]
public class NPVoxAnimationPlayer : MonoBehaviour
{
    public event AnimationChangedEventHandler OnAnimationChanged;
    public event AnimationEventHandler OnKeyFrameEntered;
    public event AnimationEventHandler OnStarted;
    public event AnimationEventHandler OnStopped;

    [SerializeField, HideInInspector]
    private new NPVoxAnimation animation;

    private MeshFilter MeshFilter;

    public NPVoxAnimation Animation
    {
        get
        {
            return animation;
        }
        set
        {
            animation = value;
            if (animation != null)
            {
                animation.EnsureAllMeshesLoaded();

                FPS = animation.FPS;
                Loop = animation.Loop;
                PingPong = animation.PingPong;
                backwards = false;

                bool previousStopped = Stopped;
                setStoppedInternal(true, false);
                currentKeyFrame = -1;
                setStoppedInternal(previousStopped, false);
                if (OnAnimationChanged != null)
                {
                    OnAnimationChanged();
                }
            }
        }
    }

    public int FPS = 8;

    public bool Loop = false;

    public bool PingPong = false;

    [SerializeField]
    private bool backwards = false;
    public bool Backwards
    {
        get
        {
            return backwards;
        }
        set
        {
            // if(!stopped)
            // {
            // }
            backwards = value;
            if (value)
            {
                CurrentKeyFrame = KeyFrameCount - 1;
            }
            else
            {
                currentKeyFrame = 0;
            }
        }
    }

    public int KeyFrameCount
    {
        get
        {
            if (animation != null)
            {
                return animation.Frames.Length;
            }
            return -1;
        }
    }

    [SerializeField]
    private bool stopped = false;
    private Coroutine runningCoroutine = null;
    public bool Stopped
    {
        get
        {
            return stopped;
        }
        set
        {
            setStoppedInternal(value, true);
        }
    }

    private void setStoppedInternal(bool value, bool dispatchEvents)
    {
        if (stopped != value)
        {
            stopped = value;
            if (runningCoroutine != null)
            {
                StopCoroutine(runningCoroutine);
                runningCoroutine = null;
            }
            if (!stopped && enabled && gameObject.activeInHierarchy)
            {
                runningCoroutine = StartCoroutine(doUpdate());
                if (OnStarted != null && dispatchEvents)
                {
                    OnStarted(new NPVoxAnimationEventArgs(currentKeyFrame));
                }
            }
            else
            {
                if (OnStopped != null && dispatchEvents)
                {
                    OnStopped(new NPVoxAnimationEventArgs(currentKeyFrame));
                }
            }
        }
    }

//    private bool forceFireNextKeyFrameEnteredEvent = false;

    [SerializeField]
    private int currentKeyFrame = 0;
    public int CurrentKeyFrame
    {
        get
        {
            return currentKeyFrame;
        }
        set
        {
            if (currentKeyFrame != value)
            {
                currentKeyFrame = value;
                if (enabled && MeshFilter != null && value >= 0 && animation && value < animation.Frames.Length)
                {
                    MeshFilter.sharedMesh = animation.Frames[value].GetTransformedMesh();
                }

                if (OnKeyFrameEntered != null)
                {
                    OnKeyFrameEntered(new NPVoxAnimationEventArgs(currentKeyFrame));
                }
            }
        }
    }

    public NPVoxFrame CurrentNPVoxFrame
    {
        get 
        {
            return animation.Frames[currentKeyFrame];
        }
    }


    protected void Start()
    {
        MeshFilter = this.GetComponent<MeshFilter>();

        if (animation != null)
        {
            animation.EnsureAllMeshesLoaded();

            if (!Stopped)
            {
                stopped = true;
                Play(true);
            }
        }
    }

    public void Play(bool dispatchEvents = false)
    {
        setStoppedInternal(false, dispatchEvents);
    }

    public void Stop(bool dispatchEvents = false)
    {
        setStoppedInternal(true, dispatchEvents);
    }

    private IEnumerator doUpdate()
    {
        // bool isPingPongingBack = false;
        int keyFrameCount = KeyFrameCount;

        float normalFrameDuration = 1f / (float)FPS;
        float frameDuration = normalFrameDuration;

        float accumFrameTime = 0f;
        while (!Stopped)
        {
            yield return null;
            accumFrameTime += Time.deltaTime;

            while (!Stopped && ( accumFrameTime > frameDuration || currentKeyFrame == -1 ) )
            {
                accumFrameTime -= frameDuration;
                if (currentKeyFrame == -1)
                {
                    // we just started the animation
                    frameDuration = normalFrameDuration * Animation.Frames[ 0 ].Duration;
                    CurrentKeyFrame = 0;
                } 
                else if (CurrentKeyFrame == keyFrameCount - 1)
                {
                    if (Loop)
                    {
                        if (PingPong || backwards)
                        {
                            frameDuration = normalFrameDuration * Animation.Frames[ currentKeyFrame - 1 ].Duration;
                            CurrentKeyFrame = CurrentKeyFrame - 1;
                            backwards = true;
                        }
                        else
                        {
                            currentKeyFrame = -1; // in case we got a looping 1-frame animation.
                            frameDuration = normalFrameDuration * Animation.Frames[ 0 ].Duration;
                            CurrentKeyFrame = 0;
                        }
                    }
                    else if (backwards)
                    {
                        frameDuration = normalFrameDuration * Animation.Frames[ currentKeyFrame - 1].Duration;
                        CurrentKeyFrame = CurrentKeyFrame - 1;
                    }
                    else
                    {
                        setStoppedInternal(true, true);
                    }
                }
                else if (CurrentKeyFrame == 0 && backwards)
                {
                    if (Loop)
                    {
                        if (PingPong)
                        {
                            backwards = false;
                        }
                        else
                        {
                            CurrentKeyFrame = keyFrameCount - 1;
                            frameDuration = normalFrameDuration * Animation.Frames[ currentKeyFrame ].Duration;
                        }
                    }
                    else
                    {
                        setStoppedInternal(true, true);
                    }
                }
                else
                {
                    if (backwards)
                    {
                        CurrentKeyFrame = CurrentKeyFrame - 1;
                        frameDuration = normalFrameDuration * Animation.Frames[ currentKeyFrame ].Duration;
                    }
                    else
                    {
                        CurrentKeyFrame = CurrentKeyFrame + 1;
                        frameDuration = normalFrameDuration * Animation.Frames[ currentKeyFrame ].Duration;
                    }
                }

            }
        }
    }
}