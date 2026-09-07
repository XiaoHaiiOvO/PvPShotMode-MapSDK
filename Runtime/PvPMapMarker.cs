using UnityEngine;
namespace PvPShotMode.MapSDK
{
    public abstract class PvPMapMarker : MonoBehaviour
    {
        protected abstract Color MarkerColor { get; }
        protected virtual void OnDrawGizmos()
        {
            var c = MarkerColor; c.a = .3f; Gizmos.color = c;
            Gizmos.DrawSphere(transform.position, .45f);
            c.a = 1; Gizmos.color = c; Gizmos.DrawWireSphere(transform.position, .45f);
            Gizmos.DrawRay(transform.position, transform.forward * 1.3f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * .6f, name);
#endif
        }
    }
}
