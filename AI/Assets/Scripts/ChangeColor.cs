using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    public Transform player;
    public Material enemyMaterial;
    public Material playerMaterial;

    private void OnTriggerEnter(Collider collision)
    {
        Renderer renderer = player.GetComponent<Renderer>();
        renderer.material = enemyMaterial;
    }

    private void OnTriggerExit(Collider other)
    {
        Renderer renderer = player.GetComponent<Renderer>();
        renderer.material = playerMaterial;
    }
}
