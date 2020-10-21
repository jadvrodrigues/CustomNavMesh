using UnityEngine;
using UnityEngine.AI;

public class CustomNavMeshObstacle : CustomMonoBehaviour
{
    public delegate void OnChange();
    public event OnChange onChange;

    [SerializeField] NavMeshObstacleShape m_Shape = NavMeshObstacleShape.Box;
    public NavMeshObstacleShape Shape
    {
        get { return m_Shape; }
        set { m_Shape = value; NavMeshObstacle.shape = value; onChange?.Invoke(); }
    }

    [SerializeField] Vector3 m_Center = Vector3.zero;
    public Vector3 Center
    {
        get { return m_Center; }
        set { m_Center = value; NavMeshObstacle.center = value; onChange?.Invoke(); }
    }

    [SerializeField] Vector3 m_Size = Vector3.one;
    public Vector3 Size
    {
        get { return m_Size; }
        set { m_Size = value; NavMeshObstacle.size = value; onChange?.Invoke(); }
    }

    [SerializeField] bool m_Carve = false;
    public bool Carving
    {
        get { return m_Carve; }
        set { m_Carve = value; NavMeshObstacle.carving = value; onChange?.Invoke(); }
    }

    [SerializeField] float m_MoveThreshold = 0.1f;
    public float CarvingMoveThreshold
    {
        get { return m_MoveThreshold; }
        set { m_MoveThreshold = value; NavMeshObstacle.carvingMoveThreshold = value; onChange?.Invoke(); }
    }

    [SerializeField] float m_TimeToStationary = 0.5f;
    public float CarvingTimeToStationary
    {
        get { return m_TimeToStationary; }
        set { m_TimeToStationary = value; NavMeshObstacle.carvingTimeToStationary = value; onChange?.Invoke(); }
    }

    [SerializeField] bool m_CarveOnlyStationary = true;
    public bool CarveOnlyStationary
    {
        get { return m_CarveOnlyStationary; }
        set { m_CarveOnlyStationary = value; NavMeshObstacle.carveOnlyStationary = value; onChange?.Invoke(); }
    }

    public float Radius
    {
        get { return m_Size.x / 2.0f; }
        set { Size = new Vector3(value * 2.0f, m_Size.y, value * 2.0f); }
    }

    public float Height
    {
        get { return m_Size.y / 2.0f; }
        set { Size = new Vector3(m_Size.x, value * 2.0f, m_Size.z); }
    }

    public Vector3 Velocity
    {
        get { return NavMeshObstacle.velocity; }
        set { NavMeshObstacle.velocity = value; }
    }

    NavMeshObstacle navMeshObstacle;
    NavMeshObstacle NavMeshObstacle
    {
        get
        {
            if (navMeshObstacle == null)
            {
                navMeshObstacle = GetComponent<NavMeshObstacle>();
                if (navMeshObstacle == null)
                {
                    navMeshObstacle = gameObject.AddComponent<NavMeshObstacle>();
                }
            }
            return navMeshObstacle;
        }
    }
}
