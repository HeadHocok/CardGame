using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DebugController : MonoBehaviour
{
    GameManagerScrypt gameManager;

    [SerializeField] TMP_InputField inputField;

    [SerializeField] public Toggle DebugToggle;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) //���� ������ �����
        {
            ReadStringInput(inputField.text);
        }
    }

    public void Awake()
    {
        gameManager = GameManagerScrypt.Instance; //�������� ������ ���������������� ����� ��-�� ������� ������� �������������
    }

    public void Start()
    {
        if (PlayerPrefs.GetInt("IsReload") == 1)
        {
            DebugToggle.isOn = true;
            PlayerPrefs.DeleteKey("IsReload");
        }
        else
        {
            DebugToggle.isOn = false;
            PlayerPrefs.DeleteKey("IsReload");
        }
    }

    public void ReadStringInput(string input)
    {
        string[] words = input.Split(" ", StringSplitOptions.RemoveEmptyEntries);

        try
        {
            if (words[0].StartsWith("/heal") || words[0].StartsWith("h"))
            {
                switch (words[1])
                {
                    case "ph":
                        print(gameManager.CurrentGame.Player.HP);
                        if (gameManager.CurrentGame.Player != null)
                        {
                            gameManager.CurrentGame.Player.HP += 5;
                            GameManagerScrypt.Instance.RefreshHeroMaxHp(gameManager.CurrentGame.Player);
                            UIController.Instance.UpdateHPAndMana();
                        }

                        else
                            print("�� ������� ���������� �����");
                        break;

                    case "eh":
                        if (gameManager.CurrentGame.Enemy != null)
                        {
                            gameManager.CurrentGame.Enemy.HP += 5;
                            GameManagerScrypt.Instance.RefreshHeroMaxHp(gameManager.CurrentGame.Enemy);
                            UIController.Instance.UpdateHPAndMana();
                        }

                        else
                            print("�� ������� ���������� �����");
                        break;

                    case "pc":
                        bool result1 = int.TryParse(words[2], out var number1);
                        if (result1)
                            if (number1 >= 0 && number1 < gameManager.PlayerFieldCards.Count)
                            {
                                print(gameManager.PlayerFieldCards[number1].Card.Name + " - ������� �������");
                                gameManager.PlayerFieldCards[number1].Card.Defense += 5;
                                gameManager.PlayerFieldCards[number1].Info.RefreshData();
                            }
                            else
                                print("�� ������� ���������� 3 �������������");
                        break;

                    case "ec":
                        bool result2 = int.TryParse(words[2], out var number2);
                        if (result2)
                            if (number2 >= 0 && number2 < gameManager.EnemyFieldCards.Count)
                            {
                                print(gameManager.EnemyFieldCards[number2].Card.Name + " - ������� �������");
                                gameManager.EnemyFieldCards[number2].Card.Defense += 5;
                                gameManager.EnemyFieldCards[number2].Info.RefreshData();
                            }
                            else
                                print("�� ������� ���������� 3 �������������");
                        break;

                    default:
                        print("�� ������� ���������� 2 �������������");
                        break;
                }
            }
            else if (words[0].StartsWith("/reload") || words[0].StartsWith("r"))
            {
                StopAllCoroutines();
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                PlayerPrefs.SetInt("IsReload", 1);
                return;
            }
            else if (words[0].StartsWith("/show") || words[0].StartsWith("s"))
            {
                bool result3 = int.TryParse(words[1], out var number3);
                if (result3)
                {
                    if (number3 >= 0 && number3 < gameManager.EnemyHandCards.Count)
                    {
                        gameManager.EnemyHandCards[number3].Info.ShowCardInfo();
                        print(gameManager.EnemyHandCards[number3].Card.Name + " - ������� ���������");
                    }
                }
                else
                {
                    if (words[1] == "all")
                    {
                        foreach (var CardC in gameManager.EnemyHandCards)
                            CardC.Info.ShowCardInfo();
                        print("������ ���� ������� ���������");
                    }
                    else
                        print("�� ������� ���������� 2 �������������");
                }
            }
            else
            {
                print("�� ������� ���������� 1 �������������");
                return;
            }
        }
        catch (Exception e)
        { print("�� ������� ���������� �������: " + e); }
    }
}

