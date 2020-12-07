using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld
{
    public class WorldController : MonoBehaviour
    {
        private int world_distance = 5;
        private int world_size;

        private int grass_distance = 2;
        private int grass_size;

        private GameObject player;
        private Vector3 player_pos;

        private Vector3 reference_pos;
        public GameObject ground;
        public GameObject[] grounds;
        public Material grass;
        public MeshRenderer[] grasses;

        void Awake()
        {
            world_size = world_distance * 2 + 1;
            grass_size = grass_distance * 2 + 1;
            grounds = new GameObject[world_size * world_size];
            grasses = new MeshRenderer[grass_size * grass_size];
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

        private void SetNoneMaterial(int index)
        {
            Material[] _materials = grasses[index].sharedMaterials;
            _materials[1] = null;
            grasses[index].sharedMaterials = _materials;
        }

        private void SetGrassMaterial(int index)
        {
            Material[] _materials = grasses[index].sharedMaterials;
            _materials[1] = grass;
            grasses[index].sharedMaterials = _materials;
        }

        private void GroundShiftX_Minus()
        {
            reference_pos.x -= Ground.mesh_width;
            for (int z=(world_size-1)*world_size; z<world_size*world_size; z++)
                Destroy(grounds[z]);
            
            for (int z=(grass_size-1)*grass_size; z<grass_size*grass_size; z++)
                SetNoneMaterial(z);

            System.Array.Copy(grounds, 0, grounds, world_size, (world_size-1)*world_size);
            System.Array.Copy(grasses, 0, grasses, grass_size, (grass_size-1)*grass_size);

            var x_diff = -world_distance;
            for (int z=0; z<world_size; z++)
            {
                var z_diff = z - world_distance;
                var _pos = new Vector3(Ground.mesh_width*x_diff+reference_pos.x, 0f, Ground.mesh_width*z_diff+reference_pos.z);
                grounds[z] = Instantiate(ground, _pos, Quaternion.identity, this.transform);
            }

            var grass_x_diff = world_distance - grass_distance;
            for (int z=0; z<grass_size; z++)
            {
                var grass_z_diff = z - grass_distance;
                grasses[z] = grounds[grass_x_diff*world_size + grass_x_diff + z].GetComponent<MeshRenderer>();
                SetGrassMaterial(z);
            }
        }

        private void GroundShiftX_Plus()
        {
            reference_pos.x += Ground.mesh_width;
            for (int z=0; z<world_size; z++)
                Destroy(grounds[z]);
            
            for (int z=0; z<grass_size; z++)
                SetNoneMaterial(z);

            System.Array.Copy(grounds, world_size, grounds, 0, (world_size-1)*world_size);
            System.Array.Copy(grasses, grass_size, grasses, 0, (grass_size-1)*grass_size);

            var _x = (world_size-1) * world_size;
            var x_diff = world_distance;
            for (int z=_x; z<world_size*world_size; z++)
            {
                var z_diff = z - _x - world_distance;
                var _pos = new Vector3(Ground.mesh_width*x_diff+reference_pos.x, 0f, Ground.mesh_width*z_diff+reference_pos.z);
                grounds[z] = Instantiate(ground, _pos, Quaternion.identity, this.transform);
            }

            var grass_x_diff = (world_distance - grass_distance) + grass_size;
            var _grass_x = (grass_x_diff-1)*world_size + (world_distance-grass_distance);
            for (int z=(grass_size-1)*grass_size; z<grass_size*grass_size; z++)
            {
                var z_diff = z - (grass_size-1)*grass_size;
                grasses[z] = grounds[_grass_x + z_diff].GetComponent<MeshRenderer>();
                SetGrassMaterial(z);
            }
        }

        private void GroundShiftZ_Minus()
        {
            reference_pos.z -= Ground.mesh_width;
            for (int x=0; x<world_size; x++)
                Destroy(grounds[x*world_size + world_size-1]);
            
            for (int z=0; z<grass_size; z++)
                SetNoneMaterial(z*grass_size + grass_size-1);

            for (int x=0; x<world_size; x++)
                System.Array.Copy(grounds, x*world_size, grounds, x*world_size+1, world_size-1);
            
            for (int x=0; x<grass_size; x++)
                System.Array.Copy(grasses, x*grass_size, grasses, x*grass_size+1, grass_size-1);

            var z_diff = -world_distance;
            for (int x=0; x<world_size; x++)
            {
                var x_diff = x - world_distance;
                var _pos = new Vector3(Ground.mesh_width*x_diff+reference_pos.x, 0f, Ground.mesh_width*z_diff+reference_pos.z);
                grounds[x*world_size] = Instantiate(ground, _pos, Quaternion.identity, this.transform);
            }

            var grass_x_diff = (world_distance-grass_distance);
            var grass_z_diff = (world_distance-grass_distance);
            for (int x=0; x<grass_size; x++)
            {
                grasses[x*grass_size] = grounds[(grass_x_diff+x)*world_size + grass_z_diff].GetComponent<MeshRenderer>();
                SetGrassMaterial(x*grass_size);
            }
        }

        private void GroundShiftZ_Plus()
        {
            reference_pos.z += Ground.mesh_width;
            for (int x=0; x<world_size; x++)
                Destroy(grounds[x*world_size]);

            for (int x=0; x<grass_size; x++)
                SetNoneMaterial(x*grass_size);

            for (int x=0; x<world_size; x++)
                System.Array.Copy(grounds, x*world_size+1, grounds, x*world_size, world_size-1);
            for (int x=0; x<grass_size; x++)
                System.Array.Copy(grasses, x*grass_size+1, grasses, x*grass_size, grass_size-1);

            var z_diff = world_distance;
            for (int x=0; x<world_size; x++)
            {
                var x_diff = x - world_distance;
                var _pos = new Vector3(Ground.mesh_width*x_diff+reference_pos.x, 0f, Ground.mesh_width*z_diff+reference_pos.z);
                grounds[x*world_size+world_size-1] = Instantiate(ground, _pos, Quaternion.identity, this.transform);
            }

            var grass_x_diff = (world_distance-grass_distance);
            var grass_z_diff = (world_distance-grass_distance) + (grass_size-1);
            for (int x=0; x<grass_size; x++)
            {
                grasses[x*grass_size+grass_size-1] = grounds[(grass_x_diff+x)*world_size + grass_z_diff].GetComponent<MeshRenderer>();
                SetGrassMaterial(x*grass_size+grass_size-1);
            }
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
                    grounds[_x+z] = Instantiate(ground, _pos, Quaternion.identity, this.transform);

                    // 草の表示切替用
                    if (Mathf.Abs(x_diff) <= grass_distance && Mathf.Abs(z_diff) <= grass_distance)
                    {
                        var grass_x = x_diff + grass_distance;
                        var grass_z = z_diff + grass_distance;
                        grasses[grass_x*grass_size + grass_z] = grounds[_x+z].GetComponent<MeshRenderer>();
                        SetGrassMaterial(grass_x*grass_size + grass_z);
                    }
                }
            }
        }
    }
}
