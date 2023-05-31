using UnityEngine;
using UnityEngine.EventSystems;

public class AttackedCard : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (!GameManagerScrypt.Instance.IsPlayerTurn)
            return;

        CardController attacker = eventData.pointerDrag.GetComponent<CardController>(),
                       defender = GetComponent<CardController>(); //берем информацию о брошенной карте

        if (attacker && attacker.Card.CanAttack && defender.Card.IsPlaced) //если может атаковать и на поле
        {
            if (GameManagerScrypt.Instance.EnemyFieldCards.Exists(x => x.Card.IsProvocation) &&
                !defender.Card.IsProvocation) //если на поле есть провокация, но мы пытаемся атаковать не её
                return;

            GameManagerScrypt.Instance.CardsFight(attacker, defender);
        }
    }
}
