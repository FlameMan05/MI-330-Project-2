using UnityEngine;
using UnityEngine.Events;

namespace cowsins2D
{
    public class PlayerControl : MonoBehaviour, IPlayerControl
    {
        public PlayerControlEvents PlayerControlEvents { get; set; } = new PlayerControlEvents();
        public bool Controllable { get; private set; }

        private PlayerDependencies playerDependencies;
        private IPlayerStats playerStats;
        private IInventoryManager inventoryManager;
        private Rigidbody2D rb;

        private void Start()
        {
            playerDependencies = GetComponent<PlayerDependencies>();
            playerStats = playerDependencies.PlayerStats;
            inventoryManager = playerDependencies.InventoryManager;
            rb = playerDependencies.Rigidbody;
            GrantControl();
        }

        public void GrantControl()
        {
            Controllable = true;
            PlayerControlEvents.onGrantControl?.Invoke();
            if (rb != null) rb.linearDamping = 0;
        }

        public void LoseControl()
        {
            Controllable = false;
            PlayerControlEvents.onLoseControl?.Invoke();
            if (rb != null) rb.linearDamping = 2;
        }

        public void ToggleControl()
        {
            Controllable = !Controllable;
            if (Controllable) PlayerControlEvents.onGrantControl?.Invoke();
            else PlayerControlEvents.onLoseControl?.Invoke();
        }

        public void CheckIfCanGrantControl()
        {
            if (playerDependencies == null) playerDependencies = GetComponent<PlayerDependencies>();
            if (playerStats == null) playerStats = playerDependencies?.PlayerStats;
            if (inventoryManager == null) inventoryManager = playerDependencies?.InventoryManager;

            if (playerStats == null || inventoryManager == null || playerStats.IsDead || inventoryManager.InventoryOpen) return;

            GrantControl();
        }
    }
}