using UnityEngine;

namespace PvPShotMode.MapSDK
{
    /// <summary>
    /// 地图标记基类。编辑器中绘制半透明球体 Gizmo，游戏中不可见。
    /// 子类设置不同颜色和大小。
    /// </summary>
    [ExecuteInEditMode]
    public abstract class PvPMapMarker : MonoBehaviour
    {
        [SerializeField] protected Color gizmoColor = Color.white;
        [SerializeField] protected float gizmoRadius = 0.5f;
        [SerializeField] protected string markerLabel = "";

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.35f);
            Gizmos.DrawSphere(transform.position, gizmoRadius);
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, gizmoRadius);
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, gizmoRadius * 1.2f);
            UnityEditor.Handles.Label(transform.position + Vector3.up * gizmoRadius * 1.5f,
                string.IsNullOrEmpty(markerLabel) ? gameObject.name : markerLabel,
                new GUIStyle { normal = { textColor = gizmoColor }, fontSize = 12, fontStyle = FontStyle.Bold });
        }
#endif

        private void Awake()
        {
            if (!Application.isEditor || Application.isPlaying)
            {
                var renderer = GetComponent<Renderer>();
                if (renderer != null) renderer.enabled = false;
            }
        }
    }

    /// <summary>重生点标记。蓝色=CT(警)，红色=T(匪)，绿色=FFA。</summary>
    [AddComponentMenu("PvP Shot Mode/Spawn Point Marker")]
    public class SpawnPointMarker : PvPMapMarker
    {
        public enum SpawnTeam { CT, T, FFA }
        [SerializeField] private SpawnTeam team = SpawnTeam.CT;

        private void OnValidate()
        {
            switch (team)
            {
                case SpawnTeam.CT:
                    gizmoColor = new Color(0.2f, 0.6f, 1f, 1f);
                    markerLabel = "CT 重生点";
                    break;
                case SpawnTeam.T:
                    gizmoColor = new Color(1f, 0.3f, 0.2f, 1f);
                    markerLabel = "T 重生点";
                    break;
                case SpawnTeam.FFA:
                    gizmoColor = new Color(0.2f, 1f, 0.4f, 1f);
                    markerLabel = "FFA 重生点";
                    break;
            }
        }

        private void Reset()
        {
            gizmoRadius = 0.6f;
            OnValidate();
        }
    }

    /// <summary>爆破包点标记。黄色半透明球+BoxCollider(Trigger)。</summary>
    [AddComponentMenu("PvP Shot Mode/Bombsite Marker")]
    [RequireComponent(typeof(BoxCollider))]
    public class BombsiteMarker : PvPMapMarker
    {
        [SerializeField] private string bombsiteName = "A";

        private void Reset()
        {
            gizmoColor = new Color(1f, 0.85f, 0f, 1f);
            gizmoRadius = 1.5f;
            markerLabel = $"包点 {bombsiteName}";
            var col = GetComponent<BoxCollider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnValidate()
        {
            gizmoColor = new Color(1f, 0.85f, 0f, 1f);
            markerLabel = $"包点 {bombsiteName}";
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            var col = GetComponent<BoxCollider>();
            if (col != null)
            {
                Gizmos.color = new Color(1f, 0.85f, 0f, 0.2f);
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(col.center, col.size);
                Gizmos.color = new Color(1f, 0.85f, 0f, 0.7f);
                Gizmos.DrawWireCube(col.center, col.size);
                Gizmos.matrix = Matrix4x4.identity;
            }
        }
    }

    /// <summary>空气墙标记。白色半透明立方体。</summary>
    [AddComponentMenu("PvP Shot Mode/Barrier Group Marker")]
    public class BarrierGroupMarker : PvPMapMarker
    {
        private void Reset()
        {
            gizmoColor = new Color(0.9f, 0.9f, 0.95f, 1f);
            gizmoRadius = 0.4f;
            markerLabel = "空气墙";
        }

        protected override void OnDrawGizmos()
        {
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.2f);
            Gizmos.DrawCube(transform.position, Vector3.one * gizmoRadius * 2f);
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.6f);
            Gizmos.DrawWireCube(transform.position, Vector3.one * gizmoRadius * 2f);
        }
    }

    /// <summary>武器购买墙锚点标记。蓝色/红色半透明立方体。</summary>
    [AddComponentMenu("PvP Shot Mode/Weapon Shop Marker")]
    public class WeaponShopMarker : PvPMapMarker
    {
        public enum ShopTeam { CT, T }
        [SerializeField] private ShopTeam team = ShopTeam.CT;

        private void OnValidate()
        {
            switch (team)
            {
                case ShopTeam.CT:
                    gizmoColor = new Color(0.2f, 0.5f, 1f, 1f);
                    markerLabel = "武器墙 (CT)";
                    break;
                case ShopTeam.T:
                    gizmoColor = new Color(1f, 0.35f, 0.2f, 1f);
                    markerLabel = "武器墙 (T)";
                    break;
            }
        }

        private void Reset()
        {
            gizmoRadius = 0.5f;
            OnValidate();
        }

        protected override void OnDrawGizmos()
        {
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.25f);
            Gizmos.DrawCube(transform.position, new Vector3(2f, 2f, 0.3f));
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.7f);
            Gizmos.DrawWireCube(transform.position, new Vector3(2f, 2f, 0.3f));
        }
    }
}
