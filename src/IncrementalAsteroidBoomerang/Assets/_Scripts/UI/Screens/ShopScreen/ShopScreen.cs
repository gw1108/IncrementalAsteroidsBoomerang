using TMPro;

public class ShopScreen : BaseScreen
{
    public TextMeshProUGUI CurrentMoneyLabel;

    protected override void OnShow()
    {
        base.OnShow();
        CurrentMoneyLabel.SetText("$" + PlayerWallet.Instance.Money);
    }

    protected override void OnCloseButtonClicked()
    {
        base.OnCloseButtonClicked();
        // TODO start new game
    }
}
