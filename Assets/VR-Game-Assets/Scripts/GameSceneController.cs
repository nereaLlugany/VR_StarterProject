using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSceneController : MonoBehaviour
{
    [SerializeField] private Image Image_Background;
    [SerializeField] private TMP_Text TitleScore_Text;
    [SerializeField] private TMP_Text TitleHighScore_Text;
    [SerializeField] private TMP_Text FinalScore_Text;
    [SerializeField] private TMP_Text HighScore_Text;
    [SerializeField] private GameObject BackButton;

    [SerializeField] private Image Image_Background2;
    [SerializeField] private TMP_Text TitleScore_Text2;
    [SerializeField] private TMP_Text CurrentScore_Text;

    private int currentScore = 0; // Variable per guardar el marcador actual

    void Start()
    {
        Image_Background.enabled = false;
        TitleScore_Text.enabled = false;
        TitleHighScore_Text.enabled = false;
        FinalScore_Text.enabled = false;
        HighScore_Text.enabled = false;
        BackButton.SetActive(false);

        // Inicialitza el text del marcador
        if (CurrentScore_Text != null)
            CurrentScore_Text.text = currentScore.ToString();
    }

    public void Sum()
    {
        
        Debug.Log("Sum");
        currentScore += 1;
        if (CurrentScore_Text != null)
            CurrentScore_Text.text = currentScore.ToString();
        
        Debug.Log(currentScore);
    }
    
    public void Substract()
    {
        
        currentScore -= 1;
        if (CurrentScore_Text != null)
            CurrentScore_Text.text = currentScore.ToString();
        Debug.Log("Subs: " + currentScore);
    }

    public void EndGame()
    {
        Image_Background.enabled = true;
        TitleScore_Text.enabled = true;
        TitleHighScore_Text.enabled = true;
        FinalScore_Text.enabled = true;
        HighScore_Text.enabled = true;
        BackButton.SetActive(true);
        
        
        Image_Background2.enabled = false;
        TitleScore_Text2.enabled = false;
        CurrentScore_Text.enabled = false;
    }
}