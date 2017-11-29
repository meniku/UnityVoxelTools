using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class GNBlockMap : MonoBehaviour
{
    public float m_fCellSize = 2.0f;

    public Vector3 m_currentCell;
    public Vector3 m_vCellOffset = Vector3.zero;
    public GameObject m_prefab;
    public Bounds m_bounds;
    // public int version = 0; // this is just 

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (Selection.activeGameObject != this.gameObject)
        {
            return;
        }

        Vector3 pos = Camera.current.transform.position;

        float height = m_fCellSize;
        float width = m_fCellSize;
        float depth = m_fCellSize;
        float xSize = 100f;
        float ySize = 100f;

        float z = m_currentCell.z + depth / 2;
        for (float y = pos.y - ySize; y < pos.y + ySize; y += height)
        {
            float lineY = m_vCellOffset.y + Mathf.Floor(y / height) * height + height / 2f;
            if (Mathf.Abs(lineY + 46f) < 0.1f || Mathf.Abs(lineY + 0f) < 0.1f)
            {
                Gizmos.color = new Color(255, 0, 0, 0.5f);
            }
            else
            {
                Gizmos.color = new Color(255, 255, 255, 0.5f);
            }
            Gizmos.DrawLine(
                new Vector3(m_vCellOffset.x + pos.x - xSize, lineY, z),
                new Vector3(m_vCellOffset.x + pos.x + xSize, lineY, z)
            );
        }

        Gizmos.color = new Color(255, 255, 255, 0.5f);
        for (float x = pos.x - xSize; x < pos.x + xSize; x += width)
        {
            float lineX = m_vCellOffset.x + Mathf.Floor(x / width) * width + width / 2f;

            if (Mathf.Abs(lineX - 40f) < 0.1f || Mathf.Abs(lineX + 0f) < 0.1f)
            {
                Gizmos.color = new Color(255, 0, 0, 0.5f);
            }
            else
            {
                Gizmos.color = new Color(255, 255, 255, 0.5f);
            }
            // x-line
            Gizmos.DrawLine(
                new Vector3(lineX, m_vCellOffset.y + pos.y - ySize, z),
                new Vector3(lineX, m_vCellOffset.y + pos.y + ySize, z)
            );

            // z-line
            Gizmos.DrawLine(
                new Vector3(lineX, m_vCellOffset.y + pos.y + ySize, z),
                new Vector3(lineX, m_vCellOffset.y + pos.y + ySize, z - depth)
            );
        }
    }
#endif
}