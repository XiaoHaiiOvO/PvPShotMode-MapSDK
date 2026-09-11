using System.Collections.Generic;
using UnityEngine;

namespace HowToFish.PvPShotMode.Map
{
    // Same geometry in SDK validation and host placement. Character pivot is one metre above the foot marker.
    public static class InfectionSpawnGeometry
    {
        public static bool TryPose(Vector3 candidate, Quaternion rotation, Transform mapRoot, int layers, out Pose pose)
        {
            pose = default;
            if (!Physics.Raycast(candidate + Vector3.up * 1.5f, Vector3.down, out RaycastHit hit,
                    4f, layers, QueryTriggerInteraction.Ignore) || hit.normal.y < .72f) return false;
            if (mapRoot == null || !hit.collider.transform.IsChildOf(mapRoot)) return false;
            Vector3 feet = hit.point + Vector3.up * .08f;
            if (Physics.CheckCapsule(feet + Vector3.up * .4f, feet + Vector3.up * 1.4f,
                .34f, layers, QueryTriggerInteraction.Ignore)) return false;
            pose = new Pose(feet + Vector3.up, Quaternion.Euler(0, rotation.eulerAngles.y, 0));
            return true;
        }
        public static bool TryAirdrop(Vector3 marker, Transform mapRoot, int layers, out Vector3 landing, out float height)
        {
            landing = default; height = 15;
            if (!TryPose(marker, Quaternion.identity, mapRoot, layers, out Pose pose)) return false;
            landing = pose.position - Vector3.up;
            Vector3 start = landing + Vector3.up * 1.5f;
            if (Physics.CheckSphere(start, 1.3f, layers, QueryTriggerInteraction.Ignore)) return false;
            if (Physics.SphereCast(start, 1.3f, Vector3.up, out RaycastHit ceiling, 19, layers, QueryTriggerInteraction.Ignore))
                height = Mathf.Min(15, ceiling.distance - 1);
            return height >= 2;
        }
        public static List<Pose> Allocate(Transform markers, int count, Transform mapRoot, int layers)
        {
            var poses = new List<Pose>();
            if (markers == null || count < 1 || count > 32) return poses;
            for (int ring = 0; ring <= 5 && poses.Count < count; ring++)
            foreach (Transform marker in markers)
            {
                int directions = ring == 0 ? 1 : 12;
                for (int i = 0; i < directions && poses.Count < count; i++)
                {
                    float angle = i * Mathf.PI * 2 / directions;
                    Vector3 candidate = marker.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * ring * 1.25f;
                    if (!TryPose(candidate, marker.rotation, mapRoot, layers, out Pose pose)) continue;
                    bool near = false;
                    foreach (var existing in poses) if ((existing.position - pose.position).sqrMagnitude < 1.44f) { near = true; break; }
                    if (!near) poses.Add(pose);
                }
            }
            return poses;
        }
    }
}
