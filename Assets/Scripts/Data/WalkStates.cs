using UnityEngine;

public enum WalkState
{
    Walkable, // Can move through unrestricted
    Unwalkable, // Cannot move through at all
    Flyable, // Can only move across via fly, no arms
    Swimmable // Can only move across via swim, no legs
}