using UnityEngine;

public class CardAbility : MonoBehaviour
{
    public CardController CC;

    public GameObject Shield, Provocation;

    public void OnCast()
    {
        foreach (var ability in CC.Card.Abilities)
        {
            switch (ability)
            {
                case Card.AbilityType.INSTANT_ACTIVE:

                    CC.Card.CanAttack = true;

                    if (CC.IsPlayerCard)
                        CC.Info.HighlightCard(true);

                    break;

                case Card.AbilityType.SHIELD:
                    Shield.SetActive(true);
                    break;

                case Card.AbilityType.PROVOCATION:
                    Provocation.SetActive(true);
                    break;
            }
        }
    }

    public void OnDamageDeal()
    {
        foreach (var ability in CC.Card.Abilities)
        {
            switch (ability)
            {
                case Card.AbilityType.DOUBLE_ATTACK:

                    if (CC.Card.TimesDealedDamage == 1)
                    {
                        CC.Card.CanAttack = true;

                        if (CC.IsPlayerCard)
                            CC.Info.HighlightCard(true);
                    }

                    break;
            }
        }
    }

    public void OnDamageTake(CardController attacker = null)
    {
        Shield.SetActive(false); //Для того, чтобы щит не спадал когда карта бьёт с 0 урона

        foreach (var ability in CC.Card.Abilities)
        {
            switch (ability)
            {
                case Card.AbilityType.SHIELD:
                    Shield.SetActive(true);
                    break;

                case Card.AbilityType.COUNTER_ATTACK:

                    if (attacker != null) //Если ударили картой текущую карту
                        attacker.Card.GetDamage(CC.Card.Attack);

                    break;
            }
        }
    }

    public void OnNewTurn()
    {
        CC.Card.TimesDealedDamage = 0;

        foreach (var ability in CC.Card.Abilities)
        {
            switch (ability)
            {
                case Card.AbilityType.REGERENATION:

                    CC.Card.Defense += 2;
                    GameManagerScrypt.Instance.RefreshCardMaxDefence(CC.Card);
                    CC.Info.RefreshData();

                    break;
            }
        }
    }
}
