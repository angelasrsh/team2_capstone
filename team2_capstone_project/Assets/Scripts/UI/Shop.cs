using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Grimoire;
using TMPro;

public class Shop : MonoBehaviour
{
  private bool isPlayerInRange = false;
  private bool interactPressed = false;
  private bool shopOpen = false;
  private Player_Controller player;
  private InputAction interactAction;
  private InputAction closeAction;
  private PlayerInput playerInput;
  [SerializeField] private GameObject shopUI;

  private void Awake()
  {
    playerInput = Game_Manager.Instance.GetComponent<PlayerInput>();
    if (playerInput != null)
    {
      // From Player Input Map
      interactAction = playerInput.actions["Interact"];

      // From UI Input Map
      closeAction = playerInput.actions.FindAction("CloseInteract", true);
    }

    if (interactAction != null)
      interactAction.performed += InteractPressed;

    if (closeAction != null)
      closeAction.performed += InteractPressed;
    CloseShopUI();
  }

  private void Update()
  {
    // Only process input if player is inside trigger and interact pressed once
    if (isPlayerInRange && interactPressed)
    {
      interactPressed = false;
      Debug.Log("[Shop]: Player interacted with shop.");

      if (player != null)
      {
        if (shopOpen)
          CloseShopUI();
        else
          OpenShopUI();
      }
    }
  }

  private void OnDestroy()
  {
    if (interactAction != null)
      interactAction.performed -= InteractPressed;

    if (closeAction != null)
      closeAction.performed -= InteractPressed;
  }

  private void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Player"))
    {
      isPlayerInRange = true;
      player = other.GetComponent<Player_Controller>();
    }
  }

  private void OnTriggerExit(Collider other)
  {
    if (other.CompareTag("Player"))
    {
      isPlayerInRange = false;
      player = null;
    }
  }

  private void InteractPressed(InputAction.CallbackContext context)
  {
    if (isPlayerInRange)
      interactPressed = true;
    else
      interactPressed = false;
  }

  private void OpenShopUI()
  {
    shopUI.SetActive(true);
    shopOpen = true;

    playerInput.SwitchCurrentActionMap("UI");

    // Just in case cursor isn't movable/invisible for some reason
    // Cursor.lockState = CursorLockMode.None;
    // Cursor.visible = true;
  }

  private void CloseShopUI()
  {
    shopUI.SetActive(false);
    shopOpen = false;

    playerInput.SwitchCurrentActionMap("Player");

    // Cursor.lockState = CursorLockMode.Locked;
    // Cursor.visible = false;
  }
}
