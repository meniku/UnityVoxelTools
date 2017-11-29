using UnityEngine;

/// http://wiki.unity3d.com/index.php/EnumFlagPropertyDrawer
public class EnumFlagAttribute : PropertyAttribute
{
	public string enumName;
 
	public EnumFlagAttribute() {}
 
	public EnumFlagAttribute(string name)
	{
		enumName = name;
	}
}