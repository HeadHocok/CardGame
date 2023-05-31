using System.Collections.Generic;
using UnityEngine;

public class CardController : MonoBehaviour
{
    public Card Card;

    public bool IsPlayerCard;

    public CardInfoScrypt Info;
    public CardMovementScrypt Movement;
    public CardAbility Ability;

    GameManagerScrypt gameManager;

    public void Init(Card card, bool isPlayerCard)
    {
        Card = card;
        gameManager = GameManagerScrypt.Instance;
        IsPlayerCard = isPlayerCard;

        if (isPlayerCard)
        {
            Info.ShowCardInfo();
            GetComponent<AttackedCard>().enabled = false; //Карта не может атаковать свои карты
        }
        else
            Info.HideCardInfo();
    }

    public void OnCast()
    {
        if (Card.IsSpell && ((SpellCard)Card).SpellTarget != SpellCard.TargetType.NO_TARGET) //если карта - способность и она действует на таргет
            return;

        if (IsPlayerCard)
        {
            gameManager.PlayerHandCards.Remove(this);
            gameManager.PlayerFieldCards.Add(this);
            gameManager.ReduceMana(true, Card.Manacost);
            gameManager.CheckCardForManaAvailability();
        }
        else
        {
            gameManager.EnemyHandCards.Remove(this);
            gameManager.EnemyFieldCards.Add(this);
            gameManager.ReduceMana(false, Card.Manacost);
            Info.ShowCardInfo();
        }

        Card.IsPlaced = true;

        if (Card.HasAbility)
            Ability.OnCast();

        if (Card.IsSpell)
            UseSpell(null);

        GameManagerScrypt.Instance.RefreshCardMaxDefence(Card);
        UIController.Instance.UpdateHPAndMana();
    }

    public void OnTakeDamage(CardController attacker = null) //Параметр attacker необязательный, т.к ему присвоено null
    {
        CheckForAlive();
        Ability.OnDamageTake(attacker);
    }

    public void OnDamageDeal()
    {
        Card.TimesDealedDamage++;
        Card.CanAttack = false;
        Info.HighlightCard(false);

        if (Card.HasAbility)
            Ability.OnDamageDeal();
    }

    public void UseSpell(CardController target)
    {
        var spellCard = (SpellCard)Card;

        switch (spellCard.Spell)
        {
            case SpellCard.SpellType.HEAL_ALLY_FIELD_CARDS:
                var allyCards = IsPlayerCard ?
                                gameManager.PlayerFieldCards :
                                gameManager.EnemyFieldCards;

                foreach (var card in allyCards)
                {
                    card.Card.Defense += spellCard.SpellValue;
                    GameManagerScrypt.Instance.RefreshCardMaxDefence(card.Card);
                    card.Info.RefreshData();
                }
                break;

            case SpellCard.SpellType.DAMAGE_ENEMY_FIELD_CARDS:

                var enemyCards = IsPlayerCard ?
                                 new List<CardController>(gameManager.EnemyFieldCards) :
                                 new List<CardController>(gameManager.PlayerFieldCards); //Создаём новый список ибо нельзя удалять карты будучи в foreach

                foreach (var card in enemyCards)
                    GiveDamageTo(card, spellCard.SpellValue);
                break;

            case SpellCard.SpellType.HEAL_ALLY_HERO:

                if (IsPlayerCard)
                {
                    gameManager.CurrentGame.Player.HP += spellCard.SpellValue;
                    GameManagerScrypt.Instance.RefreshHeroMaxHp(gameManager.CurrentGame.Player);
                }

                else
                {
                    gameManager.CurrentGame.Enemy.HP += spellCard.SpellValue;
                    GameManagerScrypt.Instance.RefreshHeroMaxHp(gameManager.CurrentGame.Enemy);
                }

                UIController.Instance.UpdateHPAndMana();
                break;

            case SpellCard.SpellType.DAMAGE_ENEMY_HERO:

                if (IsPlayerCard)
                    gameManager.CurrentGame.Enemy.HP -= spellCard.SpellValue;
                else
                    gameManager.CurrentGame.Player.HP -= spellCard.SpellValue;

                UIController.Instance.UpdateHPAndMana();
                gameManager.CheckForGameResult();
                break;

            case SpellCard.SpellType.HEAL_ALLY_CARD:
                {
                    target.Card.Defense += spellCard.SpellValue;
                    GameManagerScrypt.Instance.RefreshCardMaxDefence(target.Card);
                }

                break;

            case SpellCard.SpellType.DAMAGE_ENEMY_CARD:
                GiveDamageTo(target, spellCard.SpellValue);
                break;

            case SpellCard.SpellType.SHIELD_ON_ALLY_CARD:
                if (!target.Card.Abilities.Exists(x => x == Card.AbilityType.SHIELD))
                    target.Card.Abilities.Add(Card.AbilityType.SHIELD);
                break;

            case SpellCard.SpellType.PROVOCATION_ON_ALLY_CARD:
                if (!target.Card.Abilities.Exists(x => x == Card.AbilityType.PROVOCATION))
                    target.Card.Abilities.Add(Card.AbilityType.PROVOCATION);
                break;

            case SpellCard.SpellType.BUFF_CARD_DAMAGE:
                target.Card.Attack += spellCard.SpellValue;
                break;

            case SpellCard.SpellType.DEBUFF_CARD_DAMAGE:
                target.Card.Attack = Mathf.Clamp(target.Card.Attack - spellCard.SpellValue, 0, int.MaxValue); //атака карты не падает ниже нуля
                break;
        }

        if (target != null)
        {
            target.Ability.OnCast();
            target.CheckForAlive();
        }

        DestroyCard();
    }

    void GiveDamageTo(CardController card, int damage)
    {
        card.Card.GetDamage(damage);
        card.CheckForAlive();
        card.OnTakeDamage();
    }

    public void CheckForAlive()
    {
        if (Card.IsAlive)
            Info.RefreshData();
        else
            DestroyCard();
    }

    public void DestroyCard()
    {
        Movement.OnEndDrag(null); //предотвращение багов

        RemoveCardFromList(gameManager.EnemyFieldCards);
        RemoveCardFromList(gameManager.EnemyHandCards);
        RemoveCardFromList(gameManager.PlayerFieldCards);
        RemoveCardFromList(gameManager.PlayerHandCards);

        Destroy(gameObject);
    }

    void RemoveCardFromList(List<CardController> list)
    {
        if (list.Exists(x => x == this)) ////Лямбда принимает карту из EnemyFieldCards
            list.Remove(this);
    }
}
