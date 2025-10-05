using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // ← Necessari per canviar escenes

public class SceneControllerMenu : MonoBehaviour
{
    public TextMeshProUGUI contadorText; // Assigna el TextMeshPro al Inspector
    private bool isCounting = false;

    void Start()
    {
        contadorText.gameObject.SetActive(false);
    }

    // Funció pública per cridar des del botó
    public void StartCountdown()
    {
        if (!isCounting)
        {
            StartCoroutine(CountdownCoroutine());
        }
    }

    private System.Collections.IEnumerator CountdownCoroutine()
    {
        isCounting = true;
        contadorText.gameObject.SetActive(true);

        int count = 3;

        while (count > 0)
        {
            contadorText.text = count.ToString();
            yield return new WaitForSeconds(1f);
            count--;
        }

        contadorText.text = "START!";
        yield return new WaitForSeconds(1f);

        // Canvia a l'escena 1
        SceneManager.LoadScene(1);
    }
}
