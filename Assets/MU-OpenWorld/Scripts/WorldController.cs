using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld
{
    public class WorldController : MonoBehaviour
    {
        private int world_distance = 2;
        private int world_size;

        private GameObject player;
        private Vector3 player_pos;

        private Vector3 reference_pos;
        public GameObject ground;
        public GameObject[] grounds;

        void Awake()
        {
            world_size = world_distance * 2 + 1;
            grounds = new GameObject[world_size * world_size];
        }

        void Start()
        {
            player = GameObject.Find("Player");
            reference_pos = player.transform.position;
            player_pos = reference_pos;

            GenerateWorld();
        }

        void FixedUpdate()
        {
            player_pos = player.transform.position;
            if (player_pos.x < reference_pos.x - Ground.mesh_width / 2f)
                GroundShiftX_Minus();
            else if (player_pos.x > reference_pos.x + Ground.mesh_width / 2f)
                GroundShiftX_Plus();
            else if (player_pos.z < reference_pos.z - Ground.mesh_width / 2f)
                GroundShiftZ_Minus();
            else if (player_pos.z > reference_pos.z + Ground.mesh_width / 2f)
                GroundShiftZ_Plus();
        }

        private void GroundShiftX_Minus()
        {
            reference_pos.x -= Ground.mesh_width;
            for (int z=(world_size-1)*world_size; z<world_size*world_size; z++)
                Destroy(grounds[z]);

            var _grounds = new GameObject[world_size * world_size];
            System.Array.Copy(grounds, 0, _grounds, world_size, (world_size-1)*world_size);

            var x_diff = -world_distance;
            for (int z=0; z<world_size; z++)
            {
                var z_diff = z - world_distance;
                var _pos = new Vector3(Ground.mesh_width*x_diff+reference_pos.x, 0f, Ground.mesh_width*z_diff+reference_pos.z);
                _grounds[z] = Instantiate(ground, _pos, Quaternion.identity);
                _grounds[z].transform.parent = this.transform;
            }
            System.Array.Copy(_grounds, 0, grounds, 0, world_size*world_size);
        }

        private void GroundShiftX_Plus()
        {
            reference_pos.x += Ground.mesh_width;
            for (int z=0; z<world_size; z++)
                Destroy(grounds[z]);

            var _grounds = new GameObject[world_size * world_size];
            System.Array.Copy(grounds, world_size, _grounds, 0, (world_size-1)*world_size);

            var _x = (world_size-1) * world_size;
            var x_diff = world_distance;
            for (int z=_x; z<world_size*world_size; z++)
            {
                var z_diff = z - _x - world_distance;
                var _pos = new Vector3(Ground.mesh_width*x_diff+reference_pos.x, 0f, Ground.mesh_width*z_diff+reference_pos.z);
                _grounds[z] = Instantiate(ground, _pos, Quaternion.identity);
                _grounds[z].transform.parent = this.transform;
            }
            System.Array.Copy(_grounds, 0, grounds, 0, world_size*world_size);
        }

        private void GroundShiftZ_Minus()
        {
            reference_pos.z -= Ground.mesh_width;
            for (int x=0; x<world_size; x++)
                Destroy(grounds[x*world_size + world_size-1]);

            var _grounds = new GameObject[world_size * world_size];
            for (int x=0; x<world_size; x++)
                System.Array.Copy(grounds, x*world_size, _grounds, x*world_size+1, world_size-1);

            var z_diff = -world_distance;
            for (int x=0; x<world_size; x++)
            {
                var x_diff = x - world_distance;
                var _pos = new Vector3(Ground.mesh_width*x_diff+reference_pos.x, 0f, Ground.mesh_width*z_diff+reference_pos.z);
                _grounds[x*world_size] = Instantiate(ground, _pos, Quaternion.identity);
                _grounds[x*world_size].transform.parent = this.transform;
            }
            System.Array.Copy(_grounds, 0, grounds, 0, world_size*world_size);
        }

        private void GroundShiftZ_Plus()
        {
            reference_pos.z += Ground.mesh_width;
            for (int x=0; x<world_size; x++)
                Destroy(grounds[x*world_size]);

            var _grounds = new GameObject[world_size * world_size];
            for (int x=0; x<world_size; x++)
                System.Array.Copy(grounds, x*world_size+1, _grounds, x*world_size, world_size-1);

            var z_diff = world_distance;
            for (int x=0; x<world_size; x++)
            {
                var x_diff = x - world_distance;
                var _pos = new Vector3(Ground.mesh_width*x_diff+reference_pos.x, 0f, Ground.mesh_width*z_diff+reference_pos.z);
                _grounds[x*world_size+world_size-1] = Instantiate(ground, _pos, Quaternion.identity);
                _grounds[x*world_size+world_size-1].transform.parent = this.transform;
            }
            System.Array.Copy(_grounds, 0, grounds, 0, world_size*world_size);
        }

        private void GenerateWorld()
        {
            for (int x=0; x<world_size; x++)
            {
                var _x = x * world_size;
                var x_diff = x - world_distance;
                for (int z=0; z<world_size; z++)
                {
                    var z_diff = z - world_distance;
                    var _pos = new Vector3(Ground.mesh_width*x_diff+reference_pos.x, 0f, Ground.mesh_width*z_diff+reference_pos.z);
                    grounds[_x+z] = Instantiate(ground, _pos, Quaternion.identity);
                    grounds[_x+z].transform.parent = this.transform;
                }
            }
        }
    }
}
