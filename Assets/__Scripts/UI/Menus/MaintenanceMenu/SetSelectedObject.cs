using UnityEngine;
using UnityEngine.EventSystems;

public class SetSelectedObject : MonoBehaviour
{
    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}
