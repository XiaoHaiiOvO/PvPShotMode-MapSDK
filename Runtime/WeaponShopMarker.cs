using UnityEngine;
using HowToFish.PvPShotMode.Map;
namespace PvPShotMode.MapSDK
{
    public sealed class WeaponShopMarker : PvPMapMarker
    {
        protected override Color MarkerColor => Color.green;

        [Tooltip("仅用于编辑器预览，不改变游戏配件数量；当前原版为 7。")]
        [Range(0, 32)] public int previewAttachmentCount = WeaponShopLayout.DefaultAttachmentCount;
        private int PreviewWeaponCount => name == "人类" ? 7 : WeaponShopLayout.WeaponSlotCount;
        private static readonly string[] Slots =
            { "手枪", "霰弹枪", "冲锋枪", "狙击枪", "步枪", "指虎", "小刀", "手雷", "烟雾弹", "闪光弹" };

        protected override void OnDrawGizmos()
        {
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Color previousColor = Gizmos.color;
            try
            {
                Gizmos.matrix = WeaponShopLayout.PreviewMatrix(transform);
                for (int i = 0; i < PreviewWeaponCount; i++)
                    DrawTile(WeaponShopLayout.WeaponPosition(i), WeaponShopLayout.WeaponTileSize, Color.green);
                int count = Mathf.Clamp(previewAttachmentCount, 0, 32);
                for (int i = 0; i < count; i++)
                    DrawTile(WeaponShopLayout.AttachmentPosition(i, count), WeaponShopLayout.AttachmentTileSize, Color.cyan);
                Bounds bounds = WeaponShopLayout.TileBounds(count);
                Gizmos.color = new Color(.5f, 1, .5f);
                Gizmos.DrawWireCube(bounds.center, bounds.size);
                // A three-dimensional front arrow remains unambiguous in top and side views.
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(Vector3.zero, Vector3.forward * 1.5f);
                foreach (Vector3 side in new[] { Vector3.right, Vector3.left, Vector3.up, Vector3.down })
                    Gizmos.DrawLine(Vector3.forward * 1.5f, Vector3.forward * 1.15f + side * .18f);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(Vector3.zero, Vector3.up * .7f);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(Vector3.zero, Vector3.right * .7f);
#if UNITY_EDITOR
                UnityEditor.Handles.Label(WeaponShopLayout.WorldPoint(transform, Vector3.forward * 1.55f), "+Z 展示正面 / 玩家站这一侧");
                UnityEditor.Handles.Label(WeaponShopLayout.WorldPoint(transform, WeaponShopLayout.GuidePosition), name + " · 选购区标题");
                if (UnityEditor.Selection.Contains(gameObject))
                {
                    for (int i = 0; i < PreviewWeaponCount; i++)
                        UnityEditor.Handles.Label(WeaponShopLayout.WorldPoint(transform,
                            WeaponShopLayout.WeaponPosition(i) + WeaponShopLayout.WeaponLabelOffset), Slots[i]);
                    UnityEditor.Handles.Label(WeaponShopLayout.WorldPoint(transform, bounds.min - Vector3.up * .25f),
                        $"槽位占用：{bounds.size.x:F2} × {bounds.size.y:F2} × {bounds.size.z:F2} 米；青色为配件");
                }
#endif
            }
            finally { Gizmos.matrix = previousMatrix; Gizmos.color = previousColor; }
        }

        private static void DrawTile(Vector3 position, Vector3 size, Color color)
        {
            Gizmos.color = new Color(color.r, color.g, color.b, .18f);
            Gizmos.DrawCube(position, size);
            Gizmos.color = color;
            Gizmos.DrawWireCube(position, size);
            // Draw the front normal on every tile, not just the whole shop.
            Gizmos.DrawLine(position, position + Vector3.forward * .3f);
        }
    }
}
