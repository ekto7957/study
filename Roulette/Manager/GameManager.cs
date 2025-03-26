using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instace { get; private set; }

    private void Awake()
    {
        if(Instace == null)
        {
            Instace = this;
            DontDestroyOnLoad(gameObject);
        }

        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
