using cowsins2D;
using UnityEngine;

public class JumpPowerUp : PowerUp
{
    [SerializeField] private int jumpAmount = -1;
    [SerializeField] private bool canWallJump = false;

    // this method gets called when the power up is triggered
    public override void TriggerAction(GameObject target)
    {
        PlayerMovement playerMovement = target.GetComponent<PlayerMovement>();
        if (playerMovement == null) return;
        if (canWallJump)
        {
            playerMovement.SetWallJumpPerm(true);
            return;
        }
        if (jumpAmount > 0)
        {
            playerMovement.ChangeJumpAmount(jumpAmount);
        }
        base.TriggerAction(target);
    }
}
