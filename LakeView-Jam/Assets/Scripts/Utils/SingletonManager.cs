using UnityEngine;

public abstract class SingletonManager<T> : MonoBehaviour
    where T: class
{
    public static T Instance => m_instance;
    protected static T m_instance = null;

    protected virtual void Constructor() { }
    protected abstract T GetInstance();

    private void Awake()
    {
        if(m_instance != null)
        {
            Debug.LogError($"Singleton Manager of type <{this.GetType().Name}> already exist. Destroying {this}...");
            Destroy(this);
        }

        m_instance = GetInstance();

        Constructor();
    }
}




