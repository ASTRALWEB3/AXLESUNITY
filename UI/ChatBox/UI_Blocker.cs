using UnityEngine;

public class UI_Blocker : MonoBehaviour
{
    private static UI_Blocker instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
        
        // Hide the blocker by default on startup
        gameObject.SetActive(false);
    }

    public static void Show_Static()
    {
        if (instance != null)
        {
            instance.gameObject.SetActive(true);
            instance.transform.SetAsLastSibling();
        }
    }

    public static void Hide_Static()
    {
        if (instance != null)
        {
            instance.gameObject.SetActive(false);
        }
    }
}
