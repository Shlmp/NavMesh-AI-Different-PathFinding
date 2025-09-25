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

        controller.Move(new Vector3(input.x, 0, input.y) * Time.deltaTime * speed);
    }
}
