using _Scripts.Utility;

public class PlayerWallet : SingletonMonoBehaviour<PlayerWallet>
{
    public int Money;

    public void AddMoney(int amount) => Money += amount;
}
