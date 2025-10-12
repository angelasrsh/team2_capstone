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
  [SerializeField] private GameObject shopUI;

  private void Awake()
  {
    var playerInput = FindObjectOfType<PlayerInput>();
    if (playerInput != null)
    {
      interactAction = playerInput.actions["Interact"];
      if (interactAction != null)
        interactAction.performed += ctx => interactPressed = true;
    }
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
      interactAction.performed -= ctx => interactPressed = true;
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

  private void OpenShopUI()
  {
    shopUI.SetActive(true);
    shopOpen = true;
  }

  private void CloseShopUI()
  {
    shopUI.SetActive(false);
    shopOpen = false;
  }
}
