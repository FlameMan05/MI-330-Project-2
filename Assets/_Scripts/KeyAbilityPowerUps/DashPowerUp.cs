using cowsins2D;
using UnityEngine;

public class DashPowerUp : PowerUp
{
    [SerializeField] private DashMethod dashMode;
    [SerializeField] private int dashNum;

    // this method gets called when the power up is triggered
    public override void TriggerAction(GameObject target)
    {
        PlayerMovement playerMovement = target.GetComponent<PlayerMovement>();
        if (playerMovement == null) return;
        playerMovement.SetDashMode(dashMode);
        if (playerMovement.AmountOfDashes <= 0) playerMovement.SetDashNum(dashNum);
        base.TriggerAction(target);
    }
}
