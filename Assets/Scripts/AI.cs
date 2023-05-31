using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    public void MakeTurn()
    {
        StartCoroutine(EnemyTurn(GameManagerScrypt.Instance.EnemyHandCards));
    }

    IEnumerator EnemyTurn(List<CardController> cards) //√лупый интеллект на первое врем€
    {
        yield return new WaitForSeconds(1);

        int count = cards.Count == 1 ? 1 :
                    Random.Range(0, cards.Count + 1); //≈сли несколько карт, выкидываем рандомное кол-во карт

        for (int i = 0; i < count; i++)
        {
            if (GameManagerScrypt.Instance.EnemyFieldCards.Count > 5 ||
                GameManagerScrypt.Instance.CurrentGame.Enemy.Mana == 0 ||
                GameManagerScrypt.Instance.EnemyHandCards.Count == 0) //ќграничение мин 1 карта, макс 6 карт на столе и присутствие маны
                break;

            List<CardController> cardsList = cards.FindAll(x => GameManagerScrypt.Instance.CurrentGame.Enemy.Mana >= x.Card.Manacost); //—оздаЄм список карт доступных за тек. ману

            if (cardsList.Count == 0)
                break;

            if (cardsList[0].Card.IsSpell)
            {
                CastSpell(cardsList[0]);
                yield return new WaitForSeconds(0.51f);
            }
            else
            {
                cardsList[0].GetComponent<CardMovementScrypt>().MoveToField(GameManagerScrypt.Instance.EnemyField); //ƒвигаем карту на поле (анимаци€)
                yield return new WaitForSeconds(0.51f);
                cardsList[0].transform.SetParent(GameManagerScrypt.Instance.EnemyField);
                cardsList[0].OnCast();
            }
        }

        yield return new WaitForSeconds(1);

        //»спользуем while а не foreach потому что while будет проводить проверку каждую итерацию
        while (GameManagerScrypt.Instance.EnemyFieldCards.Exists(x => x.Card.CanAttack)) //ѕровер€ем все карты которые могут атаковать
        {
            var activeCard = GameManagerScrypt.Instance.EnemyFieldCards.FindAll(x => x.Card.CanAttack)[0];
            bool hasProvocation = GameManagerScrypt.Instance.PlayerFieldCards.Exists(x => x.Card.IsProvocation);

            if (hasProvocation ||
                Random.Range(0, 1) == 0 &&
                GameManagerScrypt.Instance.PlayerFieldCards.Count > 0) //¬ 50% случаев будет бить карту
            {
                CardController enemy;

                if (hasProvocation)
                    enemy = GameManagerScrypt.Instance.PlayerFieldCards.Find(x => x.Card.IsProvocation); //карта с провокацией
                else
                    enemy = GameManagerScrypt.Instance.PlayerFieldCards[Random.Range(0, GameManagerScrypt.Instance.PlayerFieldCards.Count)]; //–андомна€ карта игрока

                Debug.Log(activeCard.Card.Name + " (" + +activeCard.Card.Attack + ";" + activeCard.Card.Defense + ")" + " ---> " +
                          enemy.Card.Name + " (" + enemy.Card.Attack + ";" + enemy.Card.Defense + ")");

                activeCard.GetComponent<CardMovementScrypt>().MoveToTarget(enemy.transform);
                yield return new WaitForSeconds(0.75f);

                GameManagerScrypt.Instance.CardsFight(activeCard, enemy);
            }
            else
            {
                Debug.Log(activeCard.Card.Name + " (" + activeCard.Card.Attack + ") Attacked Hero");

                activeCard.GetComponent<CardMovementScrypt>().MoveToTarget(GameManagerScrypt.Instance.PlayerHero.transform);
                yield return new WaitForSeconds(0.75f);

                GameManagerScrypt.Instance.DamageHero(activeCard, false);
            }

            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(1);

        GameManagerScrypt.Instance.ChangeTurn();
    }

    void CastSpell(CardController card)
    {
        switch (((SpellCard)card.Card).SpellTarget)
        {
            case SpellCard.TargetType.NO_TARGET:

                switch (((SpellCard)card.Card).Spell)
                {
                    case SpellCard.SpellType.HEAL_ALLY_FIELD_CARDS:

                        if (GameManagerScrypt.Instance.EnemyFieldCards.Count > 0)
                            StartCoroutine(CastCard(card));
                        break;

                    case SpellCard.SpellType.DAMAGE_ENEMY_FIELD_CARDS:

                        if (GameManagerScrypt.Instance.PlayerFieldCards.Count > 0)
                            StartCoroutine(CastCard(card));
                        break;

                    case SpellCard.SpellType.HEAL_ALLY_CARD:
                        StartCoroutine(CastCard(card));
                        break;

                    case SpellCard.SpellType.DAMAGE_ENEMY_HERO:
                        StartCoroutine(CastCard(card));
                        break;
                }
                break;

            case SpellCard.TargetType.ALLY_CARD_TARGET:
                if (GameManagerScrypt.Instance.EnemyFieldCards.Count > 0)
                    StartCoroutine(CastCard(card, GameManagerScrypt.Instance.EnemyFieldCards[Random.Range(0, GameManagerScrypt.Instance.EnemyFieldCards.Count)]));
                break;

            case SpellCard.TargetType.ENEMY_CARD_TARGET:
                if (GameManagerScrypt.Instance.PlayerFieldCards.Count > 0)
                    StartCoroutine(CastCard(card, GameManagerScrypt.Instance.PlayerFieldCards[Random.Range(0, GameManagerScrypt.Instance.PlayerFieldCards.Count)]));
                break;
        }
    }

    IEnumerator CastCard(CardController spell, CardController target = null)
    {
        if (((SpellCard)spell.Card).SpellTarget == SpellCard.TargetType.NO_TARGET)
        {
            spell.GetComponent<CardMovementScrypt>().MoveToField(GameManagerScrypt.Instance.EnemyField); //двигаем карту на поле
            yield return new WaitForSeconds(.51f);

            spell.OnCast();
        }
        else
        {
            spell.Info.ShowCardInfo();
            spell.GetComponent<CardMovementScrypt>().MoveToTarget(target.transform);
            yield return new WaitForSeconds(.51f);

            GameManagerScrypt.Instance.EnemyHandCards.Remove(spell);
            GameManagerScrypt.Instance.EnemyFieldCards.Add(spell);
            GameManagerScrypt.Instance.ReduceMana(false, spell.Card.Manacost);

            spell.Card.IsPlaced = true;

            spell.UseSpell(target);
        }

        string targetStr = target == null ? "no_target" : target.Card.Name;
        Debug.Log("AI spell cast: " + (spell.Card).Name + " target: " + targetStr);
    }
}
