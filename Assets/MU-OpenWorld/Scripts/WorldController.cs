using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld
{
    public class WorldController : MonoBehaviour
    {
        private GameObject player;
        public GameObject ground;
        public List<GameObject> grounds;

        void Start()
        {
            player = GameObject.Find("Player");
            var pos = player.transform.position;

            grounds = new List<GameObject>();
            grounds.Add(Instantiate(ground, new Vector3(pos.x, 0f, pos.z), Quaternion.identity));
            grounds[grounds.Count - 1].transform.parent = this.transform;
            grounds.Add(Instantiate(ground, new Vector3(pos.x+Ground.mesh_width, 0f, pos.z), Quaternion.identity));
            grounds[grounds.Count - 1].transform.parent = this.transform;
        }
    }
}
