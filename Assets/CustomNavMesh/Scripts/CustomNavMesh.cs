using UnityEngine;

public class CustomNavMesh : MonoBehaviour
{
    static CustomNavMesh instance;
    public static CustomNavMesh Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (CustomNavMesh)FindObjectOfType(typeof(CustomNavMesh));

                if (instance == null)
                {
                    var singletonObject = new GameObject();
                    instance = singletonObject.AddComponent<CustomNavMesh>();
                    singletonObject.name = typeof(CustomNavMesh).ToString() + " (Singleton)";
                    singletonObject.transform.SetAsFirstSibling();
                }
            }

            return instance;
        }
    }
}
