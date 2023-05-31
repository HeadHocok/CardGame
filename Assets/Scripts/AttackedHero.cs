using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AttackedHero : MonoBehaviour, IDropHandler
{
    public enum HeroType
    {
        ENEMY,
        PLAYER
    }

    public HeroType Type;
    public GameManagerScrypt GameManager;
    public Color NormalCol, TargetCol;

    public void OnDrop(PointerEventData eventData)
    {
        if (!GameManagerScrypt.Instance.IsPlayerTurn)
            return;

        CardController card = eventData.pointerDrag.GetComponent<CardController>();

        if (card && //Если может атаковать, если герой противника, если нету карт-провокаций у противника
           card.Card.CanAttack &&
           Type == HeroType.ENEMY &&
           !GameManagerScrypt.Instance.EnemyFieldCards.Exists(x => x.Card.IsProvocation))
        {
            GameManagerScrypt.Instance.DamageHero(card, true);
        }
    }

    public void HighlightAsTarget(bool highlight)
    {
        GetComponent<Image>().color = highlight ? TargetCol : NormalCol;

    }
}
