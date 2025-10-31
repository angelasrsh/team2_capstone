using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Grimoire;

public class Foraging_Area_NPC_Actor : MonoBehaviour
{
    public CustomerData data;

    [Header("Dialog Settings")]
    [Tooltip("Base key used to generate dialogue (e.g. 'ForagingArea').")]
    [SerializeField] private string dialogueContext = "ForagingArea";
    [SerializeField] private float talkCooldown = 1f;

    private bool playerInRange = false;
    private bool canTalk = true;
    private bool hasTalked = false;

    private InputAction talkAction;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [Header("Visuals")]
    [SerializeField] private Transform spriteTransform;
    [SerializeField] private GameObject thoughtBubbleUI;

    private void Awake()
    {
        if (spriteTransform != null)
        {
            spriteRenderer = spriteTransform.GetComponent<SpriteRenderer>();
            animator = spriteTransform.GetComponent<Animator>();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += RebindInput;
        TryBindInput();
    }

    private void OnDisable() => SceneManager.sceneLoaded -= RebindInput;
    private void RebindInput(Scene scene, LoadSceneMode mode) => TryBindInput();

    private void TryBindInput()
    {
        PlayerInput playerInput = null;

        if (Game_Manager.Instance != null)
            playerInput = Game_Manager.Instance.GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            var pic = FindObjectOfType<Player_Input_Controller>();
            if (pic != null)
                playerInput = pic.GetComponent<PlayerInput>();
        }

        if (playerInput == null)
        {
            Debug.LogWarning("[Foraging_Area_NPC_Actor] No PlayerInput found to bind actions.");
            return;
        }

        talkAction = playerInput.actions["Talk"];
        talkAction?.Enable();
    }

    public void Init(CustomerData npcData, Transform spawnPoint)
    {
        data = npcData;

        // Set sprite and animator
        if (spriteTransform != null)
        {
            var sr = spriteTransform.GetComponent<SpriteRenderer>();
            sr.sprite = data.overworldSprite;

            if (data.walkAnimatorController != null)
            {
                var anim = spriteTransform.GetComponent<Animator>();
                anim.runtimeAnimatorController = data.walkAnimatorController;
            }
        }

        transform.position = spawnPoint.position;
        Debug.Log($"[Foraging_Area_NPC_Actor] Spawned {data.customerName} for foraging area.");

        // Reset state on spawn
        hasTalked = false;
        canTalk = true;

        // Show thought bubble at start
        SetThoughtBubbleVisible(true);
    }

    private void Update()
    {
        if (!playerInRange || !canTalk || talkAction == null || hasTalked)
            return;

        if (talkAction.WasPerformedThisFrame())
        {
            StartCoroutine(HandleTalk());
        }

        HandleIdleAnimation();
    }

    private IEnumerator HandleTalk()
    {
        canTalk = false;

        Dialogue_Manager dm = FindObjectOfType<Dialogue_Manager>();

        if (dm != null && data != null)
        {
            string baseKey = $"{data.npcID}.{dialogueContext}";
            string resolvedKey = dm.ResolveDialogKey(baseKey);
            var emotion = CustomerData.EmotionPortrait.Emotion.Neutral;

            dm.PlayScene(resolvedKey, emotion);
            Debug.Log($"[Foraging_Area_NPC_Actor] Playing dialogue key: {resolvedKey}");
        }
        else
            Debug.LogWarning("[Foraging_Area_NPC_Actor] Dialogue_Manager or CustomerData missing.");

        yield return new WaitForSeconds(talkCooldown);

        // after talking once, disable further talking
        hasTalked = true;
        canTalk = false;

        // Hide thought bubble and disable collider
        SetThoughtBubbleVisible(false);
        var col = GetComponent<BoxCollider>();
        if (col != null)
            col.enabled = false;
    }

    private void HandleIdleAnimation()
    {
        if (animator == null)
            return;

        if (animator.HasParameterOfType("speed", AnimatorControllerParameterType.Float))
            animator.SetFloat("speed", 0);

        if (animator.HasParameterOfType("isSeated", AnimatorControllerParameterType.Bool))
            animator.SetBool("isSeated", false);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    private void SetThoughtBubbleVisible(bool visible)
    {
        if (thoughtBubbleUI != null)
            thoughtBubbleUI.SetActive(visible);
    }
}
