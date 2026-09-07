using UnityEngine;
using HowToFish.PvPShotMode.Map;
namespace PvPShotMode.MapSDK
{
    [CreateAssetMenu(menuName = "PvPShotMode/地图定义", fileName = "MapDefinition")]
    public sealed class PvPMapDefinition : ScriptableObject
    {
        public GameObject prefab;
        public MapInfo info = new MapInfo();
    }
}
