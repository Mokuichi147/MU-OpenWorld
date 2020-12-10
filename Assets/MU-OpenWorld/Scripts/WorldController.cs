using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld
{
    enum Axis
    {
        None   =  0,
        Xplus  =  1,
        Xminus = -1,
        Zplus  =  2,
        Zminus = -2
    }

    struct WorldShift
    {
        public Vector3 reference_pos;
        public Axis axis;
        public int index;
    }

    public class WorldController : MonoBehaviour
    {
        private int world_distance = 10;
        private int world_size;

        private int grass_distance = 4;
        private int grass_size;

        private int collider_distance = 1;
        private int collider_size;

        private GameObject player;
        private Vector3 player_pos;

        private Vector3 reference_pos;
        public GameObject ground;
        public GameObject[] grounds;
        public Material grass;
        public MeshRenderer[] grasses;
        public MeshCollider[] colliders;

        private List<WorldShift> world_shift;


        void Awake()
        {
            world_size = world_distance * 2 + 1;
            grass_size = grass_distance * 2 + 1;
            collider_size = collider_distance * 2 + 1;
            grounds = new GameObject[world_size * world_size];
            grasses = new MeshRenderer[grass_size * grass_size];
            colliders = new MeshCollider[collider_size * collider_size];

            world_shift = new List<WorldShift>();
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
            Axis axis = Axis.None;
            if (player_pos.x > reference_pos.x + Ground.mesh_xwidth / 2f)
            {
                axis = Axis.Xplus;
                reference_pos.x += Ground.mesh_xwidth;
            }
            else if (player_pos.x < reference_pos.x - Ground.mesh_xwidth / 2f)
            {
                axis = Axis.Xminus;
                reference_pos.x -= Ground.mesh_xwidth;
            }
            else if (player_pos.z > reference_pos.z + Ground.mesh_zwidth / 2f)
            {
                axis = Axis.Zplus;
                reference_pos.z += Ground.mesh_zwidth;
            }
            else if (player_pos.z < reference_pos.z - Ground.mesh_zwidth / 2f)
            {
                axis = Axis.Zminus;
                reference_pos.z -= Ground.mesh_zwidth;
            }

            if (axis != Axis.None)    
            {
                var world_shift_temp = new List<WorldShift>();
                world_shift_temp.Add(new WorldShift() {reference_pos=reference_pos, axis=axis, index=world_distance});
                for (int i=1; i<=world_distance; i++)
                {
                    world_shift_temp.Add(new WorldShift() {reference_pos=reference_pos, axis=axis, index=world_distance+i});
                    world_shift_temp.Add(new WorldShift() {reference_pos=reference_pos, axis=axis, index=world_distance-i});
                }
                var max_count = world_shift.Count;
                if (max_count != 0)
                {
                    if ((int)world_shift[max_count-1].axis == -1 * (int)axis && world_shift[max_count-1].index == 0)
                    {
                        for (int i=1; i<=System.Math.Min(world_size, max_count); i++)
                        {
                            world_shift.RemoveAt(max_count - i);
                            world_shift_temp.RemoveAt(world_size - i);
                        }
                    }
                }
                world_shift.AddRange(world_shift_temp);
            }

            if (world_shift.Count == 0)
                return;

            var shift_data = world_shift[0];
            world_shift.RemoveAt(0);

            GroundShift(shift_data.reference_pos, shift_data.axis, shift_data.index);
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

        private void ColiderShift(Axis axis, int world_index)
        {
            if (Mathf.Abs(world_index - world_distance) > collider_distance)
                return;
            
            int world_point, collider_point;
            int collider_index = world_index - world_distance + collider_distance;
            int collider_diff = world_distance - collider_distance;

            switch (axis)
            {
                case Axis.Xplus:
                    colliders[collider_index].enabled = false;
                    for (int x=1; x<collider_size; x++)
                        System.Array.Copy(colliders, collider_index+x*collider_size, colliders, collider_index+(x-1)*collider_size, 1);
                    collider_point = (collider_size-1) * collider_size + collider_index;
                    world_point = (collider_diff+collider_size-1)*world_size + collider_diff + collider_index;
                    break;
                case Axis.Xminus:
                    colliders[(collider_size-1)*collider_size+collider_index].enabled = false;
                    for (int x=collider_size-1; x>0; x--)
                        System.Array.Copy(colliders, collider_index+(x-1)*collider_size, colliders, collider_index+x*collider_size, 1);
                    collider_point = collider_index;
                    world_point = collider_diff*world_size + collider_diff + collider_index;
                    break;
                case Axis.Zplus:
                    colliders[collider_index*collider_size].enabled = false;
                    System.Array.Copy(colliders, collider_index*collider_size+1, colliders, collider_index*collider_size, collider_size-1);
                    collider_point = (collider_index+1) * collider_size - 1;
                    world_point = collider_diff*world_size + 2*collider_diff*collider_index + collider_diff + collider_point;
                    break;
                case Axis.Zminus:
                    colliders[(collider_index+1)*collider_size-1].enabled = false;
                    System.Array.Copy(colliders, collider_index*collider_size, colliders, collider_index*collider_size+1, collider_size-1);
                    collider_point = collider_index * collider_size;
                    world_point = collider_diff*world_size + 2*collider_diff*collider_index + collider_diff + collider_point;
                    break;
                default:
                    Debug.Log("grass shift error!");
                    return;

            }
            colliders[collider_point] = grounds[world_point].GetComponent<MeshCollider>();
            colliders[collider_point].enabled = true;
        }

        private void GrassShift(Axis axis, int world_index)
        {
            if (Mathf.Abs(world_index - world_distance) > grass_distance)
                return;
            
            int world_point, grass_point;
            int grass_index = world_index - world_distance + grass_distance;
            int grass_diff = world_distance - grass_distance;
            
            switch (axis)
            {
                case Axis.Xplus:
                    SetNoneMaterial(grass_index);
                    for (int x=1; x<grass_size; x++)
                        System.Array.Copy(grasses, grass_index+x*grass_size, grasses, grass_index+(x-1)*grass_size, 1);
                    grass_point = (grass_size-1) * grass_size + grass_index;
                    world_point = (grass_diff+grass_size-1)*world_size + grass_diff + grass_index;
                    break;
                case Axis.Xminus:
                    SetNoneMaterial((grass_size-1)*grass_size+grass_index);
                    for (int x=grass_size-1; x>0; x--)
                        System.Array.Copy(grasses, grass_index+(x-1)*grass_size, grasses, grass_index+x*grass_size, 1);
                    grass_point = grass_index;
                    world_point = grass_diff*world_size + grass_diff + grass_index;
                    break;
                case Axis.Zplus:
                    SetNoneMaterial(grass_index*grass_size);
                    System.Array.Copy(grasses, grass_index*grass_size+1, grasses, grass_index*grass_size, grass_size-1);
                    grass_point = (grass_index+1) * grass_size - 1;
                    world_point = grass_diff*world_size + 2*grass_diff*grass_index + grass_diff + grass_point;
                    break;
                case Axis.Zminus:
                    SetNoneMaterial((grass_index+1)*grass_size-1);
                    System.Array.Copy(grasses, grass_index*grass_size, grasses, grass_index*grass_size+1, grass_size-1);
                    grass_point = grass_index * grass_size;
                    world_point = grass_diff*world_size + 2*grass_diff*grass_index + grass_diff + grass_point;
                    break;
                default:
                    Debug.Log("grass shift error!");
                    return;
            }
            grasses[grass_point] = grounds[world_point].GetComponent<MeshRenderer>();
            SetGrassMaterial(grass_point);
        }

        private void GroundShift(Vector3 r_pos, Axis axis, int index)
        {
            Vector3 add_diff_pos;
            int create_index;
            float index_diff = index - world_distance;

            switch (axis)
            {
                case Axis.Xplus:
                    Destroy(grounds[index]);
                    for (int x=1; x<world_size; x++)
                        System.Array.Copy(grounds, index+x*world_size, grounds, index+(x-1)*world_size, 1);
                    add_diff_pos = new Vector3((float)world_distance * Ground.mesh_xwidth, 0f, index_diff * Ground.mesh_zwidth) + r_pos;
                    create_index = (world_size - 1) * world_size + index;
                    break;
                case Axis.Xminus:
                    Destroy(grounds[(world_size-1)*world_size+index]);
                    for (int x=world_size-1; x>0; x--)
                        System.Array.Copy(grounds, index+(x-1)*world_size, grounds, index+x*world_size, 1);
                    add_diff_pos = new Vector3((float)(-1*world_distance) * Ground.mesh_xwidth, 0f, index_diff * Ground.mesh_zwidth) + r_pos;
                    create_index = index;
                    break;
                case Axis.Zplus:
                    Destroy(grounds[index*world_size]);
                    System.Array.Copy(grounds, index*world_size+1, grounds, index*world_size, world_size-1);
                    add_diff_pos = new Vector3(index_diff * Ground.mesh_xwidth, 0f, (float)world_distance * Ground.mesh_zwidth) + r_pos;
                    create_index = (index + 1) * world_size - 1;
                    break;
                case Axis.Zminus:
                    Destroy(grounds[(index+1)*world_size-1]);
                    System.Array.Copy(grounds, index*world_size, grounds, index*world_size+1, world_size-1);
                    add_diff_pos = new Vector3(index_diff * Ground.mesh_xwidth, 0f, (float)(-1*world_distance) * Ground.mesh_zwidth) + r_pos;
                    create_index = index * world_size;
                    break;
                default:
                    Debug.Log("ground shift error!");
                    return;
            }
            grounds[create_index] = Instantiate(ground, add_diff_pos, Quaternion.identity, this.transform);
            GrassShift(axis, index);
            ColiderShift(axis, index);
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
                    var _pos = new Vector3(Ground.mesh_xwidth*x_diff+reference_pos.x, 0f, Ground.mesh_zwidth*z_diff+reference_pos.z);
                    grounds[_x+z] = Instantiate(ground, _pos, Quaternion.identity, this.transform);

                    // 草の表示切替用
                    if (Mathf.Abs(x_diff) <= grass_distance && Mathf.Abs(z_diff) <= grass_distance)
                    {
                        var grass_x = x_diff + grass_distance;
                        var grass_z = z_diff + grass_distance;
                        grasses[grass_x*grass_size + grass_z] = grounds[_x+z].GetComponent<MeshRenderer>();
                        SetGrassMaterial(grass_x*grass_size + grass_z);
                    }
                    // メッシュコライダー切替用
                    if (Mathf.Abs(x_diff) <= collider_distance && Mathf.Abs(z_diff) <= collider_distance)
                    {
                        var collider_x = x_diff + collider_distance;
                        var collider_z = z_diff + collider_distance;
                        colliders[collider_x*collider_size + collider_z] = grounds[_x+z].GetComponent<MeshCollider>();
                        colliders[collider_x*collider_size + collider_z].enabled = true;
                    }
                }
            }
        }
    }
}
