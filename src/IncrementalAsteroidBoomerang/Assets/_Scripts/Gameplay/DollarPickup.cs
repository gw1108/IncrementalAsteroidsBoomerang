using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DollarPickup : MonoBehaviour
{
    [SerializeField] private int value = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerWallet.Instance.AddMoney(value);
            Destroy(gameObject);
        }
    }
}
