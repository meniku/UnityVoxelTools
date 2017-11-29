using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class GNBlockMapEditorInputController
{
    private GNBlockMapEditorVM viewModel;
    private Dictionary<KeyCode, bool> m_aKeysDown = new Dictionary<KeyCode, bool>();
    private Vector2 m_vCellOffsetMouseStartPoint;
    private Vector3 m_vCellOffsetStartOffset;
    private bool m_bAlreadyPaintedToCell = false;
    private bool m_bWheelScrolledSinceLastKeyPress = false;

    public GNBlockMapEditorInputController(GNBlockMapEditorVM _viewModel)
    {
        this.viewModel = _viewModel;
        this.viewModel.OnCellXYChanged += OnCellChanged;
        this.viewModel.OnCellZChanged += OnCellChanged;
    }

    public bool MouseMoved(Vector3 worldPosition)
    {
        if (IsKeyDown(KeyCode.P))
        {
            viewModel.PickCurrentCellPrefab();
        }
        viewModel.CurrentMousePoint = worldPosition;

        return true; // TODO: is that really wise? May BURN CPU
    }

    public bool TakeEvents()
    {
        return viewModel.CurrentTool != GNBlockMapEditorVM.Tool.NONE;
    }

    public bool WheelScrolled(Vector2 delta, bool shift, bool alt, bool ctrl)
    {
        m_bWheelScrolledSinceLastKeyPress = true;
        switch (viewModel.CurrentTool)
        {
            case GNBlockMapEditorVM.Tool.PAINT:
                if (IsKeyDown(KeyCode.Y) || IsKeyDown(KeyCode.Z))
                {
                    viewModel.BrowseLayer(Mathf.Sign(delta.y) > 0);
                    return true;
                }
                else if (IsKeyDown(KeyCode.J))
                {
                    viewModel.OrientPrefab(Vector3.right, Mathf.Sign(delta.y) > 0);
                    return true;
                }
                else if (IsKeyDown(KeyCode.K))
                {
                    viewModel.OrientPrefab(Vector3.up, Mathf.Sign(delta.y) > 0);
                    return true;
                }
                else if (ctrl || IsKeyDown(KeyCode.L))
                {
                    viewModel.OrientPrefab(Vector3.forward, Mathf.Sign(delta.y) > 0);
                    return true;
                }
                else if (IsKeyDown(KeyCode.U))
                {
                    viewModel.BrowsePrefabs(delta.y > 0, true);
                    return true;
                }
                else if (IsKeyDown(KeyCode.I))
                {
                    viewModel.BrowsePrefabs(delta.y > 0, false);
                    // viewModel.BrowseLayer(Mathf.Sign(delta.y) > 0);
                    return true;
                }
                else if(IsKeyDown(KeyCode.G))
                {
                    viewModel.BrowseCellSize(Mathf.Sign(delta.y) > 0);
                    return true;
                }
                return false;

            // case GNBlockMapEditorVM.Tool.ERASE:
            //     if (IsKeyDown(KeyCode.Y))
            //     {
            //         viewModel.BrowseLayer(Mathf.Sign(delta.y) > 0);
            //         return true;
            //     }
            //     return false;

        }
        return false;
    }

    public bool KeyStateChanged(bool pressed, Event e)
    {
        if (!pressed)
        {
            if (!m_bWheelScrolledSinceLastKeyPress && IsKeyDown(e.keyCode))
            {
                if( viewModel.CurrentTool == GNBlockMapEditorVM.Tool.PAINT )
                {
                    if (e.keyCode == KeyCode.J)
                    {
                        m_aKeysDown[e.keyCode] = false;
                        viewModel.FlipPrefab(Vector3.right);
                        return true;
                    }
                    if (e.keyCode == KeyCode.K)
                    {
                        m_aKeysDown[e.keyCode] = false;
                        viewModel.FlipPrefab(Vector3.up);
                        return true;
                    }
                    if (e.keyCode == KeyCode.L)
                    {
                        m_aKeysDown[e.keyCode] = false;
                        viewModel.FlipPrefab(Vector3.forward);
                        return true;
                    }
                    if (e.keyCode == KeyCode.M)
                    {
                        m_aKeysDown[e.keyCode] = false;
                        viewModel.SetPrefabFlip(Vector3.one);
                        viewModel.OrientPrefab(Vector3.zero);
                        return true;
                    }
                } 
                if (e.keyCode == KeyCode.Z || e.keyCode == KeyCode.Y)
                {
                    m_aKeysDown[e.keyCode] = false;
                    viewModel.BrowseLayerMode();
                    return true;
                }
            }
            m_bWheelScrolledSinceLastKeyPress = false;
        }
        

        switch (e.keyCode)
        {
            // case KeyCode.B:
            case KeyCode.G: // Browse Cell Size / Move Grid
            case KeyCode.Z: // Browse Layer / Layer Modes
            case KeyCode.Y: // Browse Layer / Layer Modes
            case KeyCode.N: // Clear Objects
            case KeyCode.M: // Clear Prefab Orentation
            case KeyCode.U: // Browse Brush Folders
            case KeyCode.I: // Browse Brush in Folder
            case KeyCode.O: // Pick prefab under mouse
            case KeyCode.J: // Browse X-Rotation / X-Flip
            case KeyCode.K: // Browse Y-Rotation / Y-Flip
            case KeyCode.L: // Browse Z-Rotation / Z-Flip
            
                if(e.command || e.control) 
                {
                    return false;
                }
                m_aKeysDown[e.keyCode] = pressed;

                if (IsKeyDown(KeyCode.O))
                {
                    viewModel.PickCurrentCellPrefab();
                }
                return true;

            case KeyCode.A:
                if (viewModel.CurrentTool == GNBlockMapEditorVM.Tool.BOX)
                {
                    if (e.alt)
                    {
                        if (e.shift)
                        {
                            viewModel.SelectBoxFullMap();
                        }
                        else
                        {
                            viewModel.SelectBoxFullLayer();
                        }
                        return true;
                    }
                }
                return false;

        }
        return false;
    }

    public bool MouseDrag(Vector3 mousePos)
    {
        if (MouseMoved(mousePos))
        {
            switch (viewModel.CurrentTool)
            {
                case GNBlockMapEditorVM.Tool.PAINT:
                    {
                        if(IsKeyDown(KeyCode.G))
                        {
                            return UpdateMouseDragGridOffset();
                        }
                        else if (!m_bAlreadyPaintedToCell)
                        {
                            m_bAlreadyPaintedToCell = true;
                            if (IsKeyDown(KeyCode.N))
                            {
                                viewModel.EraseObject();
                            }
                            else
                            {
                                viewModel.PaintObject();
                            }
                        }
                        return true;
                    }

                // case GNBlockMapEditorVM.Tool.GRID:
                //     {
                //         return UpdateMouseDragGridOffset();
                //     }
            }
        }
        return false;
    }


    public bool MouseDown(Event e)
    {
        switch (viewModel.CurrentTool)
        {
            case GNBlockMapEditorVM.Tool.PAINT:
                {
                    if (IsKeyDown(KeyCode.G))
                    {
                        m_vCellOffsetMouseStartPoint = viewModel.CurrentMousePoint;
                        m_vCellOffsetStartOffset = viewModel.CellOffset;
                    }
                    else if (IsKeyDown(KeyCode.N))
                    {
                        viewModel.EraseObject();
                    }
                    else if (!IsKeyDown(KeyCode.O))
                    {
                        viewModel.PaintObject();
                    }
                    m_bAlreadyPaintedToCell = true;
                    return true;
                }
        }
        return false;
    }

    private bool UpdateMouseDragGridOffset()
    {
        float scale = GNBlockMapEditorVM.MIN_GRID_OFFSET;
        Vector3 delta = viewModel.CurrentMousePoint - m_vCellOffsetMouseStartPoint;
        // Debug.Log(delta);
        delta.x = Mathf.Round((delta.x) / scale) * scale;
        delta.y = Mathf.Round((delta.y) / scale) * scale;
        delta.z = 0;
        Vector3 newOffset = (m_vCellOffsetStartOffset + delta);
        if (viewModel.CellOffset != newOffset)
        {
            viewModel.CellOffset = newOffset;
            return true;
        }
        return false;
    }


    private void OnCellChanged()
    {
        this.m_bAlreadyPaintedToCell = false;
    }

    private bool IsKeyDown(KeyCode keyCode)
    {
        if (!m_aKeysDown.ContainsKey(keyCode))
        {
            return false;
        }
        return m_aKeysDown[keyCode];
    }


    public bool IsErasingActive()
    {
        return viewModel.CurrentTool == GNBlockMapEditorVM.Tool.PAINT && IsKeyDown(KeyCode.N);
    }

    public bool IsPickingActive()
    {
        return viewModel.CurrentTool == GNBlockMapEditorVM.Tool.PAINT && IsKeyDown(KeyCode.O);
    }
    public bool IsRotationActive()
    {
        return viewModel.CurrentTool == GNBlockMapEditorVM.Tool.PAINT &&
            (IsKeyDown(KeyCode.J) || IsKeyDown(KeyCode.K) || IsKeyDown(KeyCode.L));
    }

    public bool IsPrefabNavigationActive()
    {
        return viewModel.CurrentTool == GNBlockMapEditorVM.Tool.PAINT &&
            (IsKeyDown(KeyCode.U) || IsKeyDown(KeyCode.I));
    }


    public string GetCurrentToolInfo()
    {
        switch (viewModel.CurrentTool)
        {
            case GNBlockMapEditorVM.Tool.BOX:
                return "-= ADDITIONAL HOTKEYS =-\n" +
                       "Alt+A: Enclose Layer | Alt+Shift+A: Enclose Map\n";
            case GNBlockMapEditorVM.Tool.PAINT:
            // case Tool.BRUSH:
                return "-= ADDITIONAL HOTKEYS =-\n" +
                       "(Y|Z)+Wheel: Browse Layer | (Y|Z): Browse Layermode\n" +
                       "G+Wheel: Browse Grid Size | G+Mouse: Move Grid\n" +
                       "U+Wheel: Browse Folder | I+Wheel: Browse Prefab\n" +
                       "O: Pick Prefab\n" +
                       "J+Wheel: RotateX | K+Wheel: RotateY | L+Wheel: RotateZ\n" +
                       "J: FlipX | K: FlipY | L: FlipZ \n" +
                       "N+Click: Erase | M: Clear Orientation & Flip \n";
        }
        return "";
    }
}