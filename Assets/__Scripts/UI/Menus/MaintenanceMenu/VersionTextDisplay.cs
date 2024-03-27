using UnityEngine;
using TMPro;

public class VersionTextDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI versionTMP;

    private void Awake()
    {
        versionTMP.text = $"Version : {Application.version}";
    }
}