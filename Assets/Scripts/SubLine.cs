using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BLINDED_AM_ME
{

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(Path_Comp))]
    public class SubLine : MonoBehaviour
    {


        public bool removeDuplicateVertices = false; // removes doubles删除重复的顶点
		public Mesh segmentSourceMesh;

        private float _segment_length;
        private float _segment_MinZ;
        private float _segment_MaxZ;

        private Path_Comp _path;
        private Transform _helpTransform1;
        private Transform _helpTransform2;

        private Mesh_Maker _maker = new Mesh_Maker();

#if UNITY_EDITOR
        public enum LightmapUnwrapping
        {
            UseFirstUvSet,
            DefaultUnwrapParam
        }
        // 枚举值，刚开始初始值为UseFirstUvSet
        public LightmapUnwrapping lightmapUnwrapping = LightmapUnwrapping.UseFirstUvSet;
#endif

        /// <summary>
        /// You can call this during runtime and in the editor
        /// </summary>
        public void ShapeIt()
        {

            //====================！！！segmentSourceMesh 如何被初始化？
            //----------------可能因为添加[RequireComponent(typeof(MeshFilter))]组件，MeshFilter中有mesh变量
            if (segmentSourceMesh == null)
            {//segmentSourceMesh 没有添加源报错：缺失
                Debug.LogError("missing source mesh");
                return;
            }
            _helpTransform1 = new GameObject("_helpTransform1").transform;//新建游戏对象1
            _helpTransform2 = new GameObject("_helpTransform2").transform;//新建游戏对象2

            // because it messes it up
            //Quaternion是四元数 用Quaternion来存储和表示对象的旋转角度

            //====================！！！ transform 谁的转换坐标？为什么将旋转角值初始值？
            //--------------猜想可能是调用本方法的gameObject,的transform 
            Quaternion oldRotation = transform.rotation;//旧的旋转 
            transform.rotation = Quaternion.identity;
            //Quaternion.identity就是指Quaternion(0,0,0,0),就是没旋转前的初始角度,是一个确切的值,而transform.rotation是指本物体的角度,值是不确定的,比如可以这么设置transform.rotation = Quaternion.identity;

            // 生成新对象时覆盖之前的Mesh_Maker
            // 新生成两个GameObject 获取他们的transform，以及将原有对象的transform的rotation 设为初始角度=================对下面处理有何影响？？？？？
            _maker = new Mesh_Maker();
            //获取所有顶点的min_z，max_z，赋值_segment_length
            ScanSourceMesh();
            Craft(); // make segments
            //1.需要清除相同顶点则清除;2.将新生成的mesh，赋给当前GameObject
            Apply(); // apply values

            transform.rotation = oldRotation;

            //g_1.GetComponent<Renderer>().marterial = Dl_COLOR;

            //清除之前申请的两个gameObject 对象
            // #if UNITY_EDITOR unity 代码运行平台判断代码，类似的有UNITY_IPHONE，UNITY_ANDROID
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                //程序运行异步销毁 程序停止，立即销毁
                //DestroyImmediate是立即销毁，立即释放资源，做这个操作的时候，会消耗很多时间的，影响主线程运行
                //Destroy是异步销毁，一般在下一帧就销毁了，不会影响主线程的运行。
                DestroyImmediate(_helpTransform1.gameObject);
                DestroyImmediate(_helpTransform2.gameObject);
            }
            else
            {
                //程序运行，正常销毁
                Destroy(_helpTransform1.gameObject);
                Destroy(_helpTransform2.gameObject);
            }
#else
			Destroy(_helpTransform1.gameObject);
			Destroy(_helpTransform2.gameObject);

#endif
        }

        private void Craft()
        {
            //初始化组件Path_camp
            _path = GetComponent<Path_Comp>();
            // struct Path_Point 包含point，forward，up，right;
            //GetPathPoint() 返回一个Path_Point 依据传入的distance(距离起始点的距离)
            Path_Point pointA = _path.GetPathPoint(0.0f);//获得路径的起点
            Path_Point pointB = pointA;
            Path_Point pointB2 = pointA;

            //_segment_length = max_z - min_z; 传入的dist 为距离起点的两两直线段距离
            for (float dist = 0.0f; dist < _path._path.TotalDistance; dist += _segment_length)
            {//对于dist=0，dist<总长度，dist=dist+分段的距离

                pointB = _path.GetPathPoint(Mathf.Clamp(dist + _segment_length, 0, _path._path.TotalDistance));//确定B点的位置  
                pointB2 = _path.GetPathPoint(Mathf.Clamp(dist + _segment_length - (float)0.05, 0, _path._path.TotalDistance));//确定B点的位置  

                _helpTransform1.rotation = Quaternion.LookRotation(pointA.forward, pointA.up);//游戏对象1的旋转=z轴朝向view，y轴朝向up
                _helpTransform1.position = transform.TransformPoint(pointA.point);//对象1的位置是A点的位置

                _helpTransform2.rotation = Quaternion.LookRotation(pointB.forward, pointB.up);//游戏对象2的旋转=z轴朝向view，y轴朝向up
                _helpTransform2.position = transform.TransformPoint(pointB.point);//对象2的位置是B点的位置


                Add_Segment();//添加分段

                pointA = pointB2;//A->B
            }

        }

        private void Add_Segment()
        {

            int[] indices;

            // go throughout the submeshes遍及子网
            for (int sub = 0; sub < segmentSourceMesh.subMeshCount; sub++)
            {
                indices = segmentSourceMesh.GetIndices(sub);
                
                for (int i = 0; i < indices.Length; i += 3)
                {
                    print("indices====" + indices[i]+","+indices[i + 1]+","+indices[i + 2]);

                    AddTriangle(new int[]{
                        indices[i],
                        indices[i+1],
                        indices[i+2]
                    }, sub);
                }
            }
        }

        private void AddTriangle(int[] indices, int submesh)
        {

            // vertices
            Vector3[] verts = new Vector3[3]{
                segmentSourceMesh.vertices[indices[0]],
                segmentSourceMesh.vertices[indices[1]],
                segmentSourceMesh.vertices[indices[2]]
            };
            // normals
            Vector3[] norms = new Vector3[3]{
                segmentSourceMesh.normals[indices[0]],
                segmentSourceMesh.normals[indices[1]],
                segmentSourceMesh.normals[indices[2]]
            };
            // uvs
            Vector2[] uvs = new Vector2[3]{
                segmentSourceMesh.uv[indices[0]],
                segmentSourceMesh.uv[indices[1]],
                segmentSourceMesh.uv[indices[2]]
            };
            // tangent
            Vector4[] tangents = new Vector4[3]{
                segmentSourceMesh.tangents[indices[0]],
                segmentSourceMesh.tangents[indices[1]],
                segmentSourceMesh.tangents[indices[2]]
            };

            // apply offset
            float lerpValue = 0.0f;
            Vector3 pointA, pointB;
            Vector3 normA, normB;
            Vector4 tangentA, tangentB;
            //Matrix4x4 4*4 矩阵；Transform.localToWorldMatrix 局部转世界矩阵
            Matrix4x4 localToWorld_A = _helpTransform1.localToWorldMatrix;
            Matrix4x4 localToWorld_B = _helpTransform2.localToWorldMatrix;
            Matrix4x4 worldToLocal = transform.worldToLocalMatrix;
            for (int i = 0; i < 3; i++)
            {

                lerpValue = Math_Functions.Value_from_another_Scope(verts[i].z, _segment_MinZ, _segment_MaxZ, 0.0f, 1.0f);
                verts[i].z = 0.0f;

                pointA = localToWorld_A.MultiplyPoint(verts[i]); // to world
                pointB = localToWorld_B.MultiplyPoint(verts[i]);

                verts[i] = worldToLocal.MultiplyPoint(Vector3.Lerp(pointA, pointB, lerpValue)); // to local

                normA = localToWorld_A.MultiplyVector(norms[i]);
                normB = localToWorld_B.MultiplyVector(norms[i]);

                norms[i] = worldToLocal.MultiplyVector(Vector3.Lerp(normA, normB, lerpValue));

                tangentA = localToWorld_A.MultiplyVector(tangents[i]);
                tangentB = localToWorld_B.MultiplyVector(tangents[i]);

                tangents[i] = worldToLocal.MultiplyVector(Vector3.Lerp(tangentA, tangentB, lerpValue));

            }

            _maker.AddTriangle(verts, norms, uvs, tangents, submesh);

        }

        //扫描源网格
        //获取所有顶点的min_z，max_z，赋值_segment_length
        private void ScanSourceMesh()
        {

            float min_z = 0.0f, max_z = 0.0f;

            // find length
            for (int i = 0; i < segmentSourceMesh.vertexCount; i++)
            {//Mesh.vertexCount 网格中的顶点数

                Vector3 vert = segmentSourceMesh.vertices[i];//mesh.vertices用于存储三角形顶点坐标。数组mesh.triangles用来记录连接三角形的顺序的。
                if (vert.z < min_z)
                    min_z = vert.z;

                if (vert.z > max_z)
                    max_z = vert.z;
            }

            //forward---Z轴；right---X轴；up---Y轴；Z代表GameObject前进的方向
            //======================！！！ 为将_segment_length 距离设置为max_z - min_z Z轴距离？
            _segment_MinZ = min_z;
            _segment_MaxZ = max_z;
            _segment_length = max_z - min_z;
        }



        /**
		 *1.需要清除相同顶点则清除
		 *2.将新生成的mesh，赋给当前GameObject
		 */
        private void Apply()
        {


            if (removeDuplicateVertices)
            {
                _maker.RemoveDoubles();
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {

                switch (lightmapUnwrapping)
                {
                    case LightmapUnwrapping.UseFirstUvSet:
                        GetComponent<MeshFilter>().mesh = _maker.GetMesh();
                        break;
                    case LightmapUnwrapping.DefaultUnwrapParam:
                        GetComponent<MeshFilter>().mesh = _maker.GetMesh_GenerateSecondaryUVSet();
                        break;
                    default:
                        GetComponent<MeshFilter>().mesh = _maker.GetMesh();
                        break;
                }

            }
            else
                GetComponent<MeshFilter>().mesh = _maker.GetMesh();
#else
			GetComponent<MeshFilter>().mesh = _maker.GetMesh();
#endif

        }

    }
}