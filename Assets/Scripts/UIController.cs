using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    public TextMeshProUGUI PlayerMana, EnemyMana;
    public TextMeshProUGUI PlayerHP, EnemyHP;

    public GameObject ResultGO;
    public TextMeshProUGUI ResultTxt;

    public TextMeshProUGUI TurnTime;
    public Button EndTurnBtn;

    int PlayerDelCardsCount;
    public TextMeshProUGUI PlayerDelCardTxt;
    public GameObject PlayerDelCardUI;

    int EnemyDelCardsCount;
    public TextMeshProUGUI EnemyDelCardTxt;
    public GameObject EnemyDelCardUI;

    private void Awake()
    {
        if (!Instance)
            Instance = this; //Синглтон (Нельзя создавать экземпляры класса)
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void StartGame()
    {
        EndTurnBtn.interactable = true;
        ResultGO.SetActive(false);
        UpdateHPAndMana();
    }

    public void UpdateHPAndMana()
    {
        PlayerMana.text = GameManagerScrypt.Instance.CurrentGame.Player.Mana.ToString();
        EnemyMana.text = GameManagerScrypt.Instance.CurrentGame.Enemy.Mana.ToString();
        PlayerHP.text = GameManagerScrypt.Instance.CurrentGame.Player.HP.ToString();
        EnemyHP.text = GameManagerScrypt.Instance.CurrentGame.Enemy.HP.ToString();
    }

    public void ShowResult()
    {
        StopAllCoroutines();
        ResultGO.SetActive(true);

        if (GameManagerScrypt.Instance.CurrentGame.Enemy.HP == 0)
            ResultTxt.text = "WIN";
        else
            ResultTxt.text = "LOSE";
    }

    public void UpdateTurnTime(int time)
    {
        TurnTime.text = time.ToString();
    }

    public void DisableTurnButton()
    {
        EndTurnBtn.interactable = GameManagerScrypt.Instance.IsPlayerTurn;
    }

    public void UpdateDelCard(Transform PlayerHand)
    {
        if (PlayerHand == GameManagerScrypt.Instance.PlayerHand)
        {
            if (!PlayerDelCardUI.activeSelf)
                PlayerDelCardUI.SetActive(true);

            PlayerDelCardTxt.text = (PlayerDelCardsCount += 1).ToString();
        }
        else
        {
            if (!EnemyDelCardUI.activeSelf)
                EnemyDelCardUI.SetActive(true);

            EnemyDelCardTxt.text = (EnemyDelCardsCount += 1).ToString();
        }
    }
}
