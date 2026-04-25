using System.Collections.Generic;
using UnityEngine;

public class BoomerangTarget : MonoBehaviour
{
    public static readonly List<BoomerangTarget> All = new List<BoomerangTarget>();

    public bool IsAlive { get; private set; } = true;

    private void OnEnable()  => All.Add(this);
    private void OnDisable() => All.Remove(this);

    public void TakeDamage(int damage, Vector2 contactPoint)
    {
        if (!IsAlive) return;
        IsAlive = false;
        gameObject.SetActive(false);
    }
}
