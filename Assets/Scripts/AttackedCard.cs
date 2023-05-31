using UnityEngine;
using UnityEngine.EventSystems;

public class AttackedCard : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (!GameManagerScrypt.Instance.IsPlayerTurn)
            return;

        CardController attacker = eventData.pointerDrag.GetComponent<CardController>(),
                       defender = GetComponent<CardController>(); //����� ���������� � ��������� �����

        if (attacker && attacker.Card.CanAttack && defender.Card.IsPlaced) //���� ����� ��������� � �� ����
        {
            if (GameManagerScrypt.Instance.EnemyFieldCards.Exists(x => x.Card.IsProvocation) &&
                !defender.Card.IsProvocation) //���� �� ���� ���� ����������, �� �� �������� ��������� �� �
                return;

            GameManagerScrypt.Instance.CardsFight(attacker, defender);
        }
    }
}
