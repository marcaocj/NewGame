using UnityEngine;

public class AttributeMenuToggle : MonoBehaviour
{
    public GameObject statsPanel; // Referência ao painel de atributos
    private bool isOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            isOpen = !isOpen;
            statsPanel.SetActive(isOpen);
        }
    }
}
