using cowsins2D;
using UnityEngine;

public class GrapplePowerUp : PowerUp
{
    [SerializeField]
    private bool canGrapple;
    // this method gets called when the power up is triggered
    public override void TriggerAction(GameObject target)
    {
        Grapple grapple = target.GetComponent<Grapple>();
        if(grapple == null) return;
        grapple.enabled = canGrapple;
        base.TriggerAction(target);
    }
}
