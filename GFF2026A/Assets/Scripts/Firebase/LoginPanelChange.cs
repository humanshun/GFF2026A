using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LoginPanelChange : MonoBehaviour
{
    [SerializeField] private GameObject currentPanel;
    [SerializeField] private GameObject changePanel;
    [SerializeField] private TextMeshProUGUI changePanelText;
    [SerializeField] private Button changePanelButton;
    void Start()
    {
        changePanelButton.onClick.AddListener(OnChangePanelButton);
    }

    public void OnChangePanelButton()
    {
        currentPanel.SetActive(false);
        changePanel.SetActive(true);
    }    
}
