using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BrakeZone : MonoBehaviour
{
    [Header("Brake")]
    [Range(0f, 1f)]
    public float brakeStrength = 0.5f;

    [Header("Drift")]
    public bool allowDrift = false;

    [Header("Debug")]
    public Color gizmoColor = Color.red;

    private void Reset()
    {
        BoxCollider box =
            GetComponent<BoxCollider>();

        box.isTrigger = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        BoxCollider box =
            GetComponent<BoxCollider>();

        if (box == null)
            return;

        Gizmos.matrix =
            transform.localToWorldMatrix;

        Gizmos.DrawCube(
            box.center,
            box.size
        );
    }
}