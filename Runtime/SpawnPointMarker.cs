using UnityEngine;
namespace PvPShotMode.MapSDK
{
    public sealed class SpawnPointMarker : PvPMapMarker
    {
        public enum SpawnTeam { CT, T, FFA }
        public SpawnTeam team;
        protected override Color MarkerColor => team == SpawnTeam.CT ? Color.blue : team == SpawnTeam.T ? Color.red : Color.magenta;
    }
}
