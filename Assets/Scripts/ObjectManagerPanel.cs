using UnityEngine;

/// <summary>
/// Handles all functionality provided by the object manager panel
/// </summary>
public class ObjectManagerPanel : MonoBehaviour
{
    public GameObject panel;
    private void Start()
    {
        panel.SetActive(false);
    }

    /// <summary>
    /// Toggles the object manager panel to spawn new objects
    /// </summary>
    public void ToggleObjectManager()
    {
        panel.SetActive(!panel.activeSelf);
    }
}
