using UnityEngine;
namespace PvPShotMode.MapSDK
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class BarrierGroupMarker : PvPMapMarker
    {
        protected override Color MarkerColor => Color.cyan;
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos(); var box = GetComponent<BoxCollider>();
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(0, 1, 1, .2f); Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = Color.cyan; Gizmos.DrawWireCube(box.center, box.size);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
