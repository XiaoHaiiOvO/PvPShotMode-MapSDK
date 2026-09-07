using UnityEditor;
using UnityEngine;

namespace PvPShotMode.MapSDK.Editor
{
    [CustomEditor(typeof(WeaponShopMarker)), CanEditMultipleObjects]
    public sealed class WeaponShopMarkerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.HelpBox("位置 = 武器墙布局原点；本地蓝色 +Z = 展示正面，绿色 +Y = 上方。切换工具栏到 Local 后移动/旋转此对象。不会再朝向出生点或追踪玩家。\n绿色为 10 个武器/投掷物槽，青色为配件槽；小短线指向每个展示模型的正面。", MessageType.Info);
            EditorGUILayout.HelpBox("布局尺寸为固定米制，不随标记 Scale 缩放。包围框对应购买板/碰撞体，不包括模型突出部分和可变长度文字。配件预览数量不参与导出；游戏按原版实际可用配件数量布局。\n旧地图：给 GamePlay/武器墙位置 下的警、匪对象添加 WeaponShopMarker，开启 Scene Gizmos 后调整。", MessageType.None);
            foreach (Object value in targets)
            {
                var marker = (WeaponShopMarker)value;
                for (Transform t = marker.transform; t != null; t = t.parent)
                    if ((t.localScale - Vector3.one).sqrMagnitude > .0001f)
                    {
                        EditorGUILayout.HelpBox(marker.name + "：标记或上级存在非 1 缩放。请保持武器墙及 Gameplay 层级 Scale=(1,1,1)，避免旋转叠加非均匀缩放导致运行时展示板变形。", MessageType.Warning);
                        break;
                    }
            }
        }
    }
}
