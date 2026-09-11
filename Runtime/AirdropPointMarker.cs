using UnityEngine;
namespace PvPShotMode.MapSDK
{
    public sealed class AirdropPointMarker : PvPMapMarker
    {
        protected override Color MarkerColor => Color.yellow;
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 1.6f, new Vector3(2.6f, 3.2f, 2.6f));
            Gizmos.DrawWireCube(transform.position + Vector3.up * 9.1f, new Vector3(2.6f, 18.2f, 2.6f));
        }
    }
}
