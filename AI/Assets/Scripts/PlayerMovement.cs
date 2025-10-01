using UnityEngine;

[RequireComponent (typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private MyActions actions;
    public float speed = 1f;

    private void Start()
    {
        gameObject.TryGetComponent(out controller);

        actions = new MyActions();
        actions.Gameplay.Enable();
    }

    private void Update()
    {
        Vector2 input = actions.Gameplay.Move.ReadValue<Vector2>();
        Vector3 move = new Vector3(input.x, 0, input.y);

        controller.Move(move * Time.deltaTime * speed);
        if (move.sqrMagnitude > 0.001f)
        {
            transform.forward = move.normalized;
        }
    }
}
