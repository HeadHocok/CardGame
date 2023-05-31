using UnityEngine;

public class Player
{
    public int HP, MAX_HP, Mana, Manapool;
    const int MAX_MANAPOOL = 10;

    public Player()
    {
        HP = MAX_HP = 30;
        Mana = Manapool = 1;
    }

    public void RestoreRoundMana()
    {
        Mana = Manapool;
    }

    public void IncreaseManapool() //мана растет с каждым ходом, но не больше 10
    {
        Manapool = Mathf.Clamp(Manapool + 1, 0, MAX_MANAPOOL);
    }

    public void GetDamage(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, int.MaxValue);
    }
}
