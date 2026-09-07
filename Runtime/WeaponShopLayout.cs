using UnityEngine;

namespace HowToFish.PvPShotMode.Map
{
    // Shared by the SDK preview and the mod. The exported marker Transform is the contract;
    // no spawn positions, collider centres, camera positions or marker scale enter this layout.
    public static class WeaponShopLayout
    {
        public const int WeaponSlotCount = 10;
        public const int DefaultAttachmentCount = 7;
        public static readonly Vector3 WeaponTileSize = new Vector3(.72f, .46f, .14f);
        public static readonly Vector3 AttachmentTileSize = new Vector3(.54f, .34f, .12f);
        public static readonly Vector3 WeaponLabelOffset = new Vector3(0, .32f, .12f);
        public static readonly Vector3 AttachmentLabelOffset = new Vector3(0, .3f, .12f);
        public static readonly Vector3 GuidePosition = new Vector3(0, 1.47f, .12f);
        // TextMesh's readable side is local -Z; the shop's front is local +Z.
        public static readonly Quaternion TextRotation = Quaternion.Euler(0, 180, 0);

        public static Pose GetPose(Transform marker) => new Pose(marker.position, marker.rotation);
        public static Matrix4x4 PreviewMatrix(Transform marker) =>
            Matrix4x4.TRS(marker.position, marker.rotation, Vector3.one);
        public static Vector3 WorldPoint(Transform marker, Vector3 local) =>
            marker.position + marker.rotation * local;
        public static Vector3 WeaponPosition(int index) =>
            new Vector3((index % 5 - 2) * 1.05f, .35f - index / 5 * .75f, .1f);
        public static Vector3 AttachmentPosition(int index, int count) =>
            new Vector3((index - (count - 1) * .5f) * .72f, -1.15f, .1f);

        // Exact purchase tile/collider bounds; item art and variable-length text are not included.
        public static Bounds TileBounds(int attachmentCount = DefaultAttachmentCount)
        {
            var bounds = new Bounds(WeaponPosition(0), WeaponTileSize);
            for (int i = 1; i < WeaponSlotCount; i++)
                bounds.Encapsulate(new Bounds(WeaponPosition(i), WeaponTileSize));
            for (int i = 0; i < attachmentCount; i++)
                bounds.Encapsulate(new Bounds(AttachmentPosition(i, attachmentCount), AttachmentTileSize));
            return bounds;
        }
    }
}
