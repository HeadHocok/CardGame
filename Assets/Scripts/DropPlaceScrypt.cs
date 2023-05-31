using UnityEngine;
using UnityEngine.EventSystems;

public enum FieldType
{
    SELF_HAND,
    SELF_FIELD,
    ENEMY_HAND,
    ENEMY_FIELD
}

public class DropPlaceScrypt : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public FieldType Type;

    public void OnDrop(PointerEventData eventData)
    {
        if (Type != FieldType.SELF_FIELD)
            return;

        CardController card = eventData.pointerDrag.GetComponent<CardController>();

        if (card &&
            GameManagerScrypt.Instance.IsPlayerTurn &&
            GameManagerScrypt.Instance.CurrentGame.Player.Mana >= card.Card.Manacost &&
            !card.Card.IsPlaced) //если ход игрока, если карта не выставлена и хватает маны
        {
            if (!card.Card.IsSpell)
                card.Movement.DefaultParent = transform; //принимает этот трансформ (поля)

            card.OnCast();
        }

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null || Type == FieldType.ENEMY_FIELD ||
            Type == FieldType.ENEMY_HAND || Type == FieldType.SELF_HAND)
            return;

        CardMovementScrypt card = eventData.pointerDrag.GetComponent<CardMovementScrypt>();

        if (card)
            card.DefaultTempCardParent = transform;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        CardMovementScrypt card = eventData.pointerDrag.GetComponent<CardMovementScrypt>();

        if (card && card.DefaultTempCardParent == transform)
            card.DefaultTempCardParent = card.DefaultParent;
    }
}
