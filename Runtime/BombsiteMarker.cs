using UnityEngine;
namespace PvPShotMode.MapSDK
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class BombsiteMarker : PvPMapMarker
    {
        protected override Color MarkerColor => Color.yellow;
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            var box = GetComponent<BoxCollider>();
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(1, .75f, 0, .2f); Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = Color.yellow; Gizmos.DrawWireCube(box.center, box.size);
            Gizmos.matrix = Matrix4x4.identity;
        }
        private void Reset() { GetComponent<BoxCollider>().isTrigger = true; }
    }
}
