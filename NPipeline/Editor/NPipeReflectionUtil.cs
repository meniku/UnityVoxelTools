using System.Collections.Generic;

public class NPipeReflectionUtil
{
    private static Dictionary<System.Type, List<System.Type>> attributeCache = new Dictionary<System.Type, List<System.Type>>();

    public static IEnumerable<System.Type> GetAllTypesWithAttribute(System.Type attribute)
    {
        if (!attributeCache.ContainsKey(attribute))
        {
            attributeCache[attribute] = new List<System.Type>();

            System.Reflection.Assembly[] AS = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var A in AS)
            {
                System.Type[] types = A.GetTypes();
                foreach (var T in types)
                {
                    if (T.GetCustomAttributes(attribute, true).Length > 0)
                    {
                        attributeCache[attribute].Add(T);
                    }
                }
            }
        }
        foreach (System.Type type in attributeCache[attribute])
        {
            yield return type;
        }
    }

}