using System.Collections.Generic;
using UnityEngine;

public class Card //Структура - значимый тип. Можно менять параметры карты независимо друг от друга
{
    public enum AbilityType
    {
        NO_ABILITY,         //Нет способостей
        INSTANT_ACTIVE,     //Сразу акивен
        DOUBLE_ATTACK,      //Двойная атака
        SHIELD,             //Защита от удара
        PROVOCATION,        //Агр на себя
        REGERENATION,       //Регенерирует 2 хп каждый ход
        COUNTER_ATTACK      //Двойная ответная атака
    }

    public string Name;
    public Sprite Logo;
    public int Attack, Defense, Manacost, MaxDefence;
    public bool CanAttack;
    public bool IsPlaced;

    public List<AbilityType> Abilities;

    public bool IsSpell;

    public bool IsAlive
    {
        get
        {
            return Defense > 0; //Вернет true если у карты не 0 хитпоинтов
        }
    }
    public bool HasAbility
    {
        get
        {
            return Abilities.Count > 0;
        }
    }
    public bool IsProvocation
    {
        get
        {
            return Abilities.Exists(x => x == AbilityType.PROVOCATION);
        }
    }

    public int TimesDealedDamage;

    public Card(string name, string logoPatch, int attack, int defense, int manacost, AbilityType abilityType = 0)
    {
        Name = name;
        Logo = Resources.Load<Sprite>(logoPatch);
        Attack = attack;
        Defense = defense;
        Manacost = manacost;
        CanAttack = false;
        IsPlaced = false;
        MaxDefence = defense;

        Abilities = new List<AbilityType>();

        if (abilityType != 0)
            Abilities.Add(abilityType);

        TimesDealedDamage = 0;
    }

    public Card(Card card)
    {
        Name = card.Name;
        Logo = card.Logo;
        Attack = card.Attack;
        Defense = card.Defense;
        Manacost = card.Manacost;
        CanAttack = false;
        IsPlaced = false;
        MaxDefence = card.Defense;

        Abilities = new List<AbilityType>(card.Abilities);

        TimesDealedDamage = 0;
    }

    public void GetDamage(int dmg)
    {
        if (dmg > 0)
        {
            if (Abilities.Exists(x => x == AbilityType.SHIELD)) //если атака больше 0 - удаляем щит
                Abilities.Remove(AbilityType.SHIELD);
            else
                Defense -= dmg;
        }
    }

    public Card GetCopy() //Предотвращает ссылание на один и тот же объект путем создания списка
    {
        return new Card(this);
    }
}

public class SpellCard : Card
{
    public enum SpellType
    {
        NO_SPELL,                       //Нет заклинания
        HEAL_ALLY_FIELD_CARDS,          //Хил всех своих карт
        DAMAGE_ENEMY_FIELD_CARDS,       //Урон всех вражеских карт
        HEAL_ALLY_HERO,                 //Хил своего героя
        DAMAGE_ENEMY_HERO,              //Урон вражеского героя
        HEAL_ALLY_CARD,                 //Хил своей карты
        DAMAGE_ENEMY_CARD,              //Урон вражеской карты
        SHIELD_ON_ALLY_CARD,            //Щит на свою карту
        PROVOCATION_ON_ALLY_CARD,       //Провокацию на свою карту
        BUFF_CARD_DAMAGE,               //Бафф на урон своей карте
        DEBUFF_CARD_DAMAGE              //Дебафф на урон вражеской карте
    }

    public enum TargetType
    {
        NO_TARGET,
        ALLY_CARD_TARGET,
        ENEMY_CARD_TARGET
    }

    public SpellType Spell;
    public TargetType SpellTarget;
    public int SpellValue;

    public SpellCard(string name, string logoPath, int manacost, SpellType spellType = 0,  //Конструктор карты заклинания
                     int spellValue = 0, TargetType targetType = 0) : base(name, logoPath, 0, 0, manacost) //Возвращает в базовый класс Card значения (полиморфизм)
    {
        IsSpell = true;

        Spell = spellType;
        SpellTarget = targetType;
        SpellValue = spellValue;

    }

    public SpellCard(SpellCard card) : base(card)
    {
        IsSpell = true;

        Spell = card.Spell;
        SpellTarget = card.SpellTarget;
        SpellValue = card.SpellValue;
    }

    public new SpellCard GetCopy()
    {
        return new SpellCard(this);
    }
}

public static class CardManager //Хранятся все существующие карты
{
    public static List<Card> AllCards = new List<Card>();
}

public class CardManagerScrypt : MonoBehaviour
{
    public void Awake()
    {
        CardManager.AllCards.Add(new Card("apple", "Sprites/Cards/apple", 3, 3, 3));
        CardManager.AllCards.Add(new Card("banana", "Sprites/Cards/banana", 4, 2, 3));
        CardManager.AllCards.Add(new Card("lemon", "Sprites/Cards/lemon", 5, 3, 4));
        CardManager.AllCards.Add(new Card("orange", "Sprites/Cards/orange", 3, 5, 4));
        CardManager.AllCards.Add(new Card("pineapple", "Sprites/Cards/pineapple", 8, 3, 5));
        CardManager.AllCards.Add(new Card("watermelon", "Sprites/Cards/watermelon", 3, 8, 5));
        CardManager.AllCards.Add(new Card("strawberry", "Sprites/Cards/strawberry", 1, 2, 1));

        CardManager.AllCards.Add(new Card("Instant Active", "Sprites/Cards/instant_active", 2, 1, 2, Card.AbilityType.INSTANT_ACTIVE));
        CardManager.AllCards.Add(new Card("Double Attack", "Sprites/Cards/double_attack", 3, 2, 4, Card.AbilityType.DOUBLE_ATTACK));
        CardManager.AllCards.Add(new Card("Shield", "Sprites/Cards/shield", 8, 1, 7, Card.AbilityType.SHIELD));
        CardManager.AllCards.Add(new Card("Provocation", "Sprites/Cards/provocation", 1, 2, 3, Card.AbilityType.PROVOCATION));
        CardManager.AllCards.Add(new Card("Regeneration", "Sprites/Cards/regeneration", 4, 4, 6, Card.AbilityType.REGERENATION));
        CardManager.AllCards.Add(new Card("Counter Attack", "Sprites/Cards/counter_attack", 3, 5, 5, Card.AbilityType.COUNTER_ATTACK));

        CardManager.AllCards.Add(new SpellCard("Heal Ally Field", "Sprites/Cards/heal_ally_field", 2,
            SpellCard.SpellType.HEAL_ALLY_FIELD_CARDS, 2, SpellCard.TargetType.NO_TARGET));
        CardManager.AllCards.Add(new SpellCard("Damage Enemy Field", "Sprites/Cards/damage_enemy_field", 2,
            SpellCard.SpellType.DAMAGE_ENEMY_FIELD_CARDS, 2, SpellCard.TargetType.NO_TARGET));
        CardManager.AllCards.Add(new SpellCard("Heal Ally Hero", "Sprites/Cards/heal_ally_hero", 2,
            SpellCard.SpellType.HEAL_ALLY_HERO, 2, SpellCard.TargetType.NO_TARGET));
        CardManager.AllCards.Add(new SpellCard("Damage Enemy Hero", "Sprites/Cards/damage_enemy_hero", 2,
            SpellCard.SpellType.DAMAGE_ENEMY_HERO, 2, SpellCard.TargetType.NO_TARGET));
        CardManager.AllCards.Add(new SpellCard("Heal Unit", "Sprites/Cards/heal_ally_card", 2,
            SpellCard.SpellType.HEAL_ALLY_CARD, 2, SpellCard.TargetType.ALLY_CARD_TARGET));
        CardManager.AllCards.Add(new SpellCard("Damage Enemy", "Sprites/Cards/damage_enemy_card", 2,
            SpellCard.SpellType.DAMAGE_ENEMY_CARD, 2, SpellCard.TargetType.ENEMY_CARD_TARGET));
        CardManager.AllCards.Add(new SpellCard("Shield Unit", "Sprites/Cards/shield_unit", 2,
            SpellCard.SpellType.SHIELD_ON_ALLY_CARD, 2, SpellCard.TargetType.ALLY_CARD_TARGET));
        CardManager.AllCards.Add(new SpellCard("Provocation Unit", "Sprites/Cards/provocation_unit", 2,
            SpellCard.SpellType.PROVOCATION_ON_ALLY_CARD, 2, SpellCard.TargetType.ALLY_CARD_TARGET));
        CardManager.AllCards.Add(new SpellCard("Buff Unit", "Sprites/Cards/buff_card_damage", 2,
            SpellCard.SpellType.BUFF_CARD_DAMAGE, 2, SpellCard.TargetType.ALLY_CARD_TARGET));
        CardManager.AllCards.Add(new SpellCard("Debuff Unit", "Sprites/Cards/debuff_card_damage", 2,
            SpellCard.SpellType.DEBUFF_CARD_DAMAGE, 2, SpellCard.TargetType.ENEMY_CARD_TARGET));
    }
}
