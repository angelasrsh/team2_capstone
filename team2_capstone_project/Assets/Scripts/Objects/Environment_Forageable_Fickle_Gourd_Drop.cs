using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grimoire
{
    public class Environment_Forageable_Fickle_Gourd_Drop : Environment_Forgeable
    {
        [Header("Fickle Gourd Drop Settings")]
        [SerializeField] private GameObject collectiblePrefab;
        [SerializeField] private Ingredient_Data gourdIngredientData;
        [SerializeField] private int gourdCount = 3;

        [Header("Drop Area Settings")]
        [SerializeField] private Vector3 dropArea = new Vector3(1.5f, 0.2f, 1.0f);
        [SerializeField] private Vector3 dropOffset = new Vector3(0f, 0.2f, 1.0f);

        [Header("Spawn Spacing Settings")]
        [SerializeField] private float minSeparation = 0.6f;
        [SerializeField] private int maxSpawnAttempts = 6;

        private Vector3 gizmoForward = Vector3.forward;
        private List<Vector3> lastSpawnPositions = new List<Vector3>();

        public override void PerformInteract()
        {
            SpawnGourds();
            base.PerformInteract();
        }

        private void SpawnGourds()
        {
            if (collectiblePrefab == null || gourdIngredientData == null)
            {
                Debug.LogWarning("[FickleGourdDrop] Missing prefab or ingredient data.");
                return;
            }

            lastSpawnPositions.Clear();

            // --- Determine forward direction (toward player or camera)
            Vector3 forwardDir = transform.forward;
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
            {
                forwardDir = (playerObj.transform.position - transform.position).normalized;
                forwardDir.y = 0f;
            }
            else if (Camera.main != null)
            {
                forwardDir = -Camera.main.transform.forward;
                forwardDir.y = 0f;
            }

            forwardDir.Normalize();
            gizmoForward = forwardDir;
            Vector3 rightDir = Vector3.Cross(Vector3.up, forwardDir);

            // Compute drop area center
            Vector3 dropCenter =
                transform.position +
                (rightDir * dropOffset.x) +
                (Vector3.up * dropOffset.y) +
                (forwardDir * dropOffset.z);

            // --- Spawn loop ---
            for (int i = 0; i < gourdCount; i++)
            {
                Vector3 spawnPos = Vector3.zero;
                bool validPos = false;

                for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
                {
                    float xOffset = Random.Range(-dropArea.x, dropArea.x);
                    float yOffset = Random.Range(0f, dropArea.y);
                    float zOffset = Random.Range(0f, dropArea.z);

                    Vector3 offset = (rightDir * xOffset) + (Vector3.up * yOffset) + (forwardDir * zOffset);
                    Vector3 candidatePos = dropCenter + offset;

                    // Adjust to ground level
                    if (Physics.Raycast(candidatePos + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f))
                        candidatePos.y = hit.point.y + 0.05f;

                    // --- Check for overlap with existing positions ---
                    bool tooClose = false;
                    foreach (var prev in lastSpawnPositions)
                    {
                        if (Vector3.Distance(prev, candidatePos) < minSeparation)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (!tooClose)
                    {
                        spawnPos = candidatePos;
                        validPos = true;
                        break;
                    }
                }

                if (!validPos)
                {
                    Debug.LogWarning($"[FickleGourdDrop] Could not find valid spawn position for gourd {i + 1} after {maxSpawnAttempts} attempts.");
                    continue;
                }

                // Instantiate collectible
                GameObject gourdObj = Instantiate(collectiblePrefab, spawnPos, Quaternion.identity);
                var collectible = gourdObj.GetComponent<Collectible_Object>();
                if (collectible != null)
                    collectible.Initialize(gourdIngredientData);

                Rigidbody rb = gourdObj.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.AddForce(Vector3.up * Random.Range(1.5f, 3f), ForceMode.Impulse);

                lastSpawnPositions.Add(spawnPos);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Vector3 forwardDir = (gizmoForward == Vector3.zero) ? transform.forward : gizmoForward;
            Vector3 rightDir = Vector3.Cross(Vector3.up, forwardDir);

            Vector3 dropCenter =
                transform.position +
                (rightDir * dropOffset.x) +
                (Vector3.up * dropOffset.y) +
                (forwardDir * dropOffset.z);

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Vector3 boxSize = new Vector3(dropArea.x * 2f, dropArea.y, dropArea.z);
            Gizmos.matrix = Matrix4x4.TRS(dropCenter, Quaternion.LookRotation(forwardDir), Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, boxSize);

            Gizmos.matrix = Matrix4x4.identity;

            Gizmos.color = Color.yellow;
            foreach (var pos in lastSpawnPositions)
                Gizmos.DrawSphere(pos, 0.1f);
        }
#endif
    }
}
