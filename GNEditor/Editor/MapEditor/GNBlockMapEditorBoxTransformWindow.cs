using UnityEngine;
using UnityEditor;
public class GNBlockMapEditorBoxTransformWindow : EditorWindow 
{
	GNBlockMapEditorVM viewModel;
	
	Vector3 delta = Vector3.zero;
	
	// Add menu named "My Window" to the Window menu
	public static void Show (GNBlockMapEditorVM viewModel) 
    {
		// Get existing open window or if none, make a new one:
		GNBlockMapEditorBoxTransformWindow window = (GNBlockMapEditorBoxTransformWindow)EditorWindow.GetWindow (typeof (GNBlockMapEditorBoxTransformWindow));
        window.viewModel = viewModel;
		window.Show();
	}
	
	void OnGUI () 
	{
		if(!viewModel)
		{
			return;
		}
		
		GUILayout.Label("CAUTION !!! These tools don't do any checks at all. If you transform stufff");
		GUILayout.Label("in a way that it covers other stuff, the other stuff won't get removed resulting in");
		GUILayout.Label("overlapping geometries");
		GUILayout.Label ("Tranform Box (" + viewModel.SelectedBox + ") ", EditorStyles.boldLabel);
		
		delta = EditorGUILayout.Vector3Field("Delta: ", delta);
		if(GUILayout.Button("Apply"))
		{
			Matrix4x4 mat = Matrix4x4.TRS(delta, Quaternion.identity, Vector3.one);
			viewModel.TransformSelectedBox(mat);
		}
		if(GUILayout.Button("Flip X"))
		{
			viewModel.XFlipSelectedBox();
		}
	}
}