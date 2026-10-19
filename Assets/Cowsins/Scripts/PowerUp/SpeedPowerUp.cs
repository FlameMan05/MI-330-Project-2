using UnityEngine;
using System.Collections;

namespace cowsins2D
{
    public class SpeedPowerUp : PowerUp
    {
        [SerializeField] private float valueAdded;
        [SerializeField] private float duration = 5f;

        public override void TriggerAction(GameObject target)
        {
            PlayerMultipliers playerMultipliers = target.GetComponent<PlayerMultipliers>();

            if (playerMultipliers == null) return;

            StartCoroutine(ApplySpeedBoost(playerMultipliers));

            base.TriggerAction(target);
        }

        private IEnumerator ApplySpeedBoost(PlayerMultipliers playerMultipliers)
        {
            float originalSpeed = playerMultipliers.speedModifier;

            playerMultipliers.speedModifier += valueAdded;

            yield return new WaitForSeconds(duration);

            if (playerMultipliers != null)
            {
                playerMultipliers.speedModifier = originalSpeed;
            }
        }
    }
}