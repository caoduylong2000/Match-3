using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T: Singleton<T>
{
    private static T instance;

    //Getter
    public static T Instance
    {
        get
        {
            if (instance == null)
                Debug.Log("No instance of " + typeof(T) + " exists in scene");

            return instance;
        }
    }

    //Create the reference in Awake()
    protected void Awake()
    {
        if(instance == null)
        {
            instance = this as T;
            Init();
        }
        else
        {
            Debug.LogWarning("An instance of " + typeof(T) + " already exists in the scene. Self-detructing");
            Destroy(gameObject);
        }
    }

    //Destroy the reference in OnDestroy()
    protected void OnDestroy()
    {
        if (this == instance)
            instance = null;
    }

    //Init will replace the functionality of Awake()
    protected virtual void Init() { }
}