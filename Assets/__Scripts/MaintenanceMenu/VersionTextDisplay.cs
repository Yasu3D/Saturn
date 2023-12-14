using UnityEngine;
using TMPro;

public class VersionTextDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI versionTMP;
    void Awake()
    {
        versionTMP.text = $"Version : {Application.version}";
    }
}
