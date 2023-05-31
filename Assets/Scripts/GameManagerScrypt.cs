using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game
{
    public Player Player, Enemy;
    public List<Card> EnemyDeck, PlayerDeck;

    public Game()
    {
        EnemyDeck = GiveDeckCard(30);
        PlayerDeck = GiveDeckCard(30);

        Player = new Player();
        Enemy = new Player();
    }

    List<Card> GiveDeckCard(int num) //выдает 30 случайных карт
    {
        List<Card> list = new List<Card>();

        for (int i = 0; i < num; i++)
        {
            var card = CardManager.AllCards[Random.Range(0, CardManager.AllCards.Count)];

            if (card.IsSpell)
                list.Add(((SpellCard)card).GetCopy()); //нужно приводить тип т.к в колоде все карты имеют тип карт Card
            else
                list.Add(card.GetCopy());
        }

        return list;
    }
}

public class GameManagerScrypt : MonoBehaviour
{
    public static GameManagerScrypt Instance; //Вложенный статический класс

    public Game CurrentGame;
    public Transform EnemyHand, PlayerHand,
                     EnemyField, PlayerField;

    public GameObject CardPref;

    int Turn, TurnTime = 30;

    public AttackedHero EnemyHero, PlayerHero;
    public AI EnemyAI;
    public List<CardController> PlayerHandCards = new List<CardController>(),
                                PlayerFieldCards = new List<CardController>(),
                                EnemyHandCards = new List<CardController>(),
                                EnemyFieldCards = new List<CardController>();

    public bool IsPlayerTurn
    {
        get
        {
            return Turn % 2 == 0;
        }
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        StartGame();
    }

    public void RestartGame()
    {
        StopAllCoroutines();

        foreach (var card in PlayerHandCards)
            Destroy(card.gameObject);
        foreach (var card in PlayerFieldCards)
            Destroy(card.gameObject);
        foreach (var card in EnemyHandCards)
            Destroy(card.gameObject);
        foreach (var card in EnemyFieldCards)
            Destroy(card.gameObject);

        PlayerHandCards.Clear();
        PlayerFieldCards.Clear();
        EnemyHandCards.Clear();
        EnemyFieldCards.Clear();

        StartGame();
    }

    void StartGame()
    {
        Turn = 0;

        CurrentGame = new Game();

        GiveHandCard(CurrentGame.EnemyDeck, EnemyHand, EnemyHandCards);
        GiveHandCard(CurrentGame.PlayerDeck, PlayerHand, PlayerHandCards);

        UIController.Instance.StartGame();

        StartCoroutine(TurnFunc());
    }

    void GiveHandCard(List<Card> deck, Transform hand, List<CardController> handCards) //выдача стартовых карт
    {
        int i = 0;
        while (i++ < 4)
            GiveCardsToHand(deck, hand, handCards);
    }

    void GiveCardsToHand(List<Card> deck, Transform hand, List<CardController> handCards)
    {
        if (deck.Count == 0)
            return;

        if (handCards.Count < 10) //если рука не переполнена
        {
            CreateCardPref(deck[0], hand); //Берем карту из колоды
            deck.RemoveAt(0); //удаляем карту из колоды
        }
        else
        {
            deck.RemoveAt(0);
            UIController.Instance.UpdateDelCard(hand);
        }
    }

    public void CreateCardPref(Card card, Transform hand)
    {
        GameObject cardGO = Instantiate(CardPref, hand, false);
        CardController cardC = cardGO.gameObject.GetComponent<CardController>(); //добавляем карту в список

        cardC.Init(card, hand == PlayerHand);

        if (cardC.IsPlayerCard)
            PlayerHandCards.Add(cardC);
        else
            EnemyHandCards.Add(cardC);
    }

    IEnumerator TurnFunc()
    {
        TurnTime = 30;

        UIController.Instance.UpdateTurnTime(TurnTime);

        foreach (var card in PlayerFieldCards)
            card.Info.HighlightCard(false);

        CheckCardForManaAvailability();

        if (IsPlayerTurn)
        {
            foreach (var card in PlayerFieldCards)
            {
                card.Card.CanAttack = (true);
                card.Info.HighlightCard(true);
                card.Ability.OnNewTurn(); //Абилки начала хода
            }

            while (TurnTime-- > 0)
            {
                UIController.Instance.UpdateTurnTime(TurnTime);
                yield return new WaitForSeconds(1);
            }

            ChangeTurn();
        }
        else
        {
            foreach (var card in EnemyFieldCards)
            {
                card.Card.CanAttack = true;
                card.Ability.OnNewTurn();
            }


            EnemyAI.MakeTurn();

            while (TurnTime-- > 0)
            {
                UIController.Instance.UpdateTurnTime(TurnTime);
                yield return new WaitForSeconds(1);
            }

            ChangeTurn();
        }
    }

    public void ChangeTurn()
    {
        StopAllCoroutines();
        Turn++;

        UIController.Instance.DisableTurnButton(); //Если ход противника - кнопку нельзя нажать

        if (IsPlayerTurn)
        {
            GiveNewCards();

            CurrentGame.Player.IncreaseManapool();
            CurrentGame.Player.RestoreRoundMana();

            UIController.Instance.UpdateHPAndMana();
        }
        else
        {
            CurrentGame.Enemy.IncreaseManapool();
            CurrentGame.Enemy.RestoreRoundMana();

            UIController.Instance.UpdateHPAndMana();
        }

        void GiveNewCards()
        {
            GiveCardsToHand(CurrentGame.EnemyDeck, EnemyHand, EnemyHandCards);
            GiveCardsToHand(CurrentGame.PlayerDeck, PlayerHand, PlayerHandCards);
        }
        StartCoroutine(TurnFunc());
    }

    public void CardsFight(CardController attacker, CardController defender)
    {
        defender.Card.GetDamage(attacker.Card.Attack);
        attacker.OnDamageDeal();
        defender.OnTakeDamage(attacker);

        attacker.Card.GetDamage(defender.Card.Attack);
        attacker.OnTakeDamage();

        attacker.CheckForAlive();
        defender.CheckForAlive();
    }

    public void ReduceMana(bool playerMana, int manacost)
    {
        if (playerMana)
            CurrentGame.Player.Mana -= manacost;
        else
            CurrentGame.Enemy.Mana -= manacost;

        UIController.Instance.UpdateHPAndMana();
    }

    public void DamageHero(CardController card, bool isEnemyAttacked)
    {
        if (isEnemyAttacked)
            CurrentGame.Enemy.GetDamage(card.Card.Attack);
        else
            CurrentGame.Player.GetDamage(card.Card.Attack);

        UIController.Instance.UpdateHPAndMana();
        card.OnDamageDeal();
        CheckForGameResult();
    }

    public void RefreshHeroMaxHp(Player player)
    {
        if (player.HP > player.MAX_HP)
            player.HP = player.MAX_HP;
    }

    public void RefreshCardMaxDefence(Card card)
    {
        if (card.Defense > card.MaxDefence)
            card.Defense = card.MaxDefence;
    }

    public void CheckForGameResult()
    {
        if (CurrentGame.Enemy.HP == 0 || CurrentGame.Player.HP == 0)
        {
            StopAllCoroutines();
            UIController.Instance.ShowResult();
        }
    }

    public void CheckCardForManaAvailability() //Проверяем карты на доступность маны
    {
        foreach (var card in PlayerHandCards) //Перебираем карты
            card.Info.HighlightManaAvaliability(CurrentGame.Player.Mana);
    }

    public void HighlightTargets(CardController attacker, bool highlight)
    {
        List<CardController> targets = new List<CardController>();

        if (attacker.Card.IsSpell)
        {
            var spellCard = (SpellCard)attacker.Card;

            if (spellCard.SpellTarget == SpellCard.TargetType.NO_TARGET)
                targets = new List<CardController>();
            else if (spellCard.SpellTarget == SpellCard.TargetType.ALLY_CARD_TARGET)
                targets = PlayerFieldCards;
            else
                targets = EnemyFieldCards;
        }
        else
        {
            if (EnemyFieldCards.Exists(x => x.Card.IsProvocation)) //Если есть провокации, то подсвечиваем их
                targets = EnemyFieldCards.FindAll(x => x.Card.IsProvocation);
            else
            {
                targets = EnemyFieldCards;
                EnemyHero.HighlightAsTarget(highlight);
            }
        }

        foreach (var card in targets)
        {
            if (attacker.Card.IsSpell)
                card.Info.HighlightAsSpellTarget(highlight);
            else
                card.Info.HighlightAsTarget(highlight);
        }

    }
}
