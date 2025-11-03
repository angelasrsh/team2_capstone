using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grimoire
{
    public class Recipe_Collectible : Interactable_Object
    {
        [field: SerializeField] public Dish_Data data { get; private set; }
        [SerializeField] private SpriteRenderer spriteRenderer;

        public void Initialize(Dish_Data newData)
        {
            data = newData;
            gameObject.name = $"Recipe_{data.dishType}";
            Debug.Log($"[Recipe_Collectible] Initialized with {data.dishType}");
        }

        public override void PerformInteract()
        {
            if (data == null)
            {
                Debug.LogWarning("[Recipe_Collectible] No Dish_Data assigned!");
                return;
            }
            Debug.Log($"[Recipe_Collectible] Collected recipe: {data.dishType}");

            // SFX
            Audio_Manager.instance?.PlaySFX(Audio_Manager.instance.bagPutIn, 0.4f, Random.Range(0.9f, 1.1f));
            Audio_Manager.instance?.PlaySparkleSFX();

            // Unlock and save
            Player_Progress.Instance.UnlockDish(data.dishType);
            Player_Progress.Instance.MarkRecipeCollectedToday();
            Player_Progress.Instance.ClearDailyRecipe();
            StartCoroutine(SaveAfterDelay());

            HideObject();
        }
        
        private IEnumerator SaveAfterDelay()
        {
            yield return new WaitForSeconds(0.5f);  // wait for Player_Progress/Save_Manager to stabilize
            Save_Manager.instance?.SaveGameData();
        }

        private void HideObject()
        {
            // Hide interaction icon
            playerInside = false;
            if (InteractIcon != null)
            {
                if (InteractIcon.transform.IsChildOf(transform))
                    Destroy(InteractIcon);
                else
                    InteractIcon.SetActive(false);
            }

            // Hide sprite
            if (spriteRenderer != null)
                spriteRenderer.enabled = false;

            // Disable collider(s)
            Collider col = GetComponentInChildren<Collider>();
            if (col != null) col.enabled = false;

            enabled = false;
        }
    }
}
