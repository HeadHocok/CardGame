using UnityEngine;
using UnityEngine.EventSystems;

public class SpellTarget : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (!GameManagerScrypt.Instance.IsPlayerTurn)
            return;

        CardController spell = eventData.pointerDrag.GetComponent<CardController>(),
                       target = GetComponent<CardController>(); //����� ���������� � ��������� �����

        if (spell && spell.Card.IsSpell && spell.IsPlayerCard && target.Card.IsPlaced && GameManagerScrypt.Instance.CurrentGame.Player.Mana >= spell.Card.Manacost) //���� ����� ��������� � �� ����
        {
            var spellCard = (SpellCard)spell.Card;

            if ((spellCard.SpellTarget == SpellCard.TargetType.ALLY_CARD_TARGET && target.IsPlayerCard) ||
                (spellCard.SpellTarget == SpellCard.TargetType.ENEMY_CARD_TARGET && !target.IsPlayerCard))
            {
                GameManagerScrypt.Instance.ReduceMana(true, spell.Card.Manacost);
                spell.UseSpell(target);
                GameManagerScrypt.Instance.CheckCardForManaAvailability();
            }
        }
    }
}
