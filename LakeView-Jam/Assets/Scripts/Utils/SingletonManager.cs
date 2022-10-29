using UnityEngine;

public abstract class SingletonManager<T> : MonoBehaviour
    where T: class
{
    public static T Instance => m_instance;
    protected static T m_instance = null;

    protected virtual void Constructor() { }
    protected abstract T GetInstance();

    public bool destroyGameObjectIfAlreadyExist = true;
    private void Awake()
    {
        if(m_instance != null)
        {
            Debug.LogWarning($"Singleton Manager of type <{this.GetType().Name}> already exist. Destroying {this}...");
            Destroy(destroyGameObjectIfAlreadyExist ? gameObject : this);
        }

        m_instance = GetInstance();

        Constructor();
    }
}




