using UnityEngine;
using UnityEngine.EventSystems;

public class SetSelectedObject : MonoBehaviour
{
    void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}
