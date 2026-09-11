using UnityEngine;
namespace PvPShotMode.MapSDK
{
    public sealed class SpawnPointMarker : PvPMapMarker
    {
        public enum SpawnTeam { CT, T, FFA, Human, Zombie }
        public SpawnTeam team;
        protected override Color MarkerColor => team == SpawnTeam.Human ? Color.cyan : team == SpawnTeam.Zombie ? Color.green : team == SpawnTeam.CT ? Color.blue : team == SpawnTeam.T ? Color.red : Color.magenta;
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = MarkerColor;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(new Vector3(0, .85f, 0), new Vector3(.8f, 1.7f, .8f));
            Gizmos.DrawLine(Vector3.up * .2f, new Vector3(0, .2f, 1.5f));
            Gizmos.DrawLine(new Vector3(0, .2f, 1.5f), new Vector3(.2f, .2f, 1.15f));
            Gizmos.DrawLine(new Vector3(0, .2f, 1.5f), new Vector3(-.2f, .2f, 1.15f));
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
