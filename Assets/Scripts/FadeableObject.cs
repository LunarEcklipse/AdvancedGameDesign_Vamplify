using System;
using System.Collections.Generic;
using UnityEngine;

public class FadeableObject : MonoBehaviour, IEquatable<FadeableObject>
{
    public List<Renderer> renderers = new();
    public Vector3 Position;
    public List<Material> Materials = new();
    public float initialAlpha;
    public float currentAlpha;
    public bool Equals(FadeableObject other)
    {
        return Position.Equals(other.Position);
    }

    public override int GetHashCode()
    {
        return Position.GetHashCode();
    }

    void Awake()
    {
        // Set object to the FadeableObject layer.
        gameObject.layer = LayerMask.NameToLayer("WallBlocker");

        Position = transform.position;

        if (renderers.Count == 0)
        {
            renderers.AddRange(GetComponentsInChildren<Renderer>());
        }
        for (int i = 0; i < renderers.Count; i++)
        {
            Materials.AddRange(renderers[i].materials);
        }

        initialAlpha = Materials[0].color.a;
        currentAlpha = initialAlpha;
    }

    void Update()
    {
        currentAlpha = Materials[0].color.a;
    }
}
