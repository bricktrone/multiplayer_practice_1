using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class AttackInput : MonoBehaviour
{
    [SerializeField] private int _damageAmount = 20;
    
    private InputSystem_Actions _input;
    private Camera _camera;
    private PlayerCombat _playerCombat;

    private void Start()
    {
        _camera = Camera.main;
        _playerCombat = FindFirstObjectByType<PlayerCombat>();
    }

    private void Awake()
    {
        _input = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _input.Player.Enable();

        _input.Player.Attack.performed += OnAttack;
    }

    private void OnDisable()
    {
        _input.Player.Attack.performed -= OnAttack;
        
        _input.Player.Disable();
    }
    
    private void OnAttack(InputAction.CallbackContext obj)
    {
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        
        // Create a ray from the camera through the mouse position
        Ray ray = _camera.ScreenPointToRay(mouseScreenPosition);
        RaycastHit hit;

        // Perform the raycast
        if (Physics.Raycast(ray, out hit, 100))
        {
            GameObject hitObject = hit.collider.gameObject;
            if (hitObject.GetComponent<PlayerStats>() != null)
            {
                _playerCombat.TryAttack(_damageAmount, hitObject.GetComponent<PlayerStats>());
            }
        }
    }
}
