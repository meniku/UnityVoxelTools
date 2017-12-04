using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class NPVoxFontRenderer : MonoBehaviour {

	// Use this for initialization

    public NPVoxFont Font;

    public string Text;

    [SerializeField, HideInInspector]
    private string lastText;

	void Awake () {

	}
	
	// Update is called once per frame
	void Update () 
    {
        if (lastText == Text)
        {
            return;
        }


        Font.UpdateCharacters();
        lastText = Text;

        GameObjectUtil.DestroyAllChildren(this.transform);

        char[] chars = Text.ToCharArray();
        Vector3 currentPos = transform.position;
        for (int i = 0; i < Text.Length; i++)
        {
            char c = chars[i];
            NPVoxFont.Character character = Font.GetCharacter(c);

            if (!character.mesh || !character.material)
            {
                Debug.LogWarning("Character " + (int)c+ " Not found in Font: " + c);
                continue;
            }
            NPVoxToUnity n2u = new NPVoxToUnity(character.Size, character.VoxelSize);
            GameObject go = GameObject.Instantiate(Font.CharacterPrefab, this.transform);
            go.transform.position = currentPos + n2u.ToUnityDirection(new Vector3(((float)character.Size.X) * 0.5f, 0.0f, 0.0f));
            currentPos += n2u.ToUnityDirection(new NPVoxCoord(character.Size.X, 0, 0));
            go.name = (int)c+"";
            go.GetComponent<MeshFilter>().sharedMesh = character.mesh;
            go.GetComponent<MeshRenderer>().sharedMaterial = character.material;
        }
	}
}
