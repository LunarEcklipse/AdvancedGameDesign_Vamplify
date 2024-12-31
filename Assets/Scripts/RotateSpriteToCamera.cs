using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RotateSpriteToCamera : MonoBehaviour
{
    private SpriteRenderer spriteRender;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!TryGetComponent<SpriteRenderer>(out spriteRender))
        {
            Debug.LogError("SpriteRenderer not found on " + gameObject.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Camera.main.transform.rotation;
    }
}
