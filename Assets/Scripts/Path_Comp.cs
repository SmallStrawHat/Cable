using UnityEngine;
using System.Collections;

namespace BLINDED_AM_ME
{


    [ExecuteInEditMode]//普通的类，加上ExecuteInEditMode， 就可以在编辑器模式中运行。
    public class Path_Comp : MonoBehaviour
    {

        // inspector variables  inspector 检查员 变量
        public bool isSmooth = true;
        public bool isCircuit = false;//是否为环

        [Range(0.01f, 1.0f)]
        public float gizmoLineSize = 1.0f;  //用来变形的常量

        [HideInInspector]//在Inspector版面中隐藏public属性，，只是隐藏，没有序不序列化的功能……
        public Path _path = new Path();

        public float TotalDistance
        {
            //总距离
            get
            {
                return _path.TotalDistance;
            }
        }

        private void Reset()
        {
            Update_Path();
        }

        void Awake()
        {
            Update_Path();
        }

        /*
         *更新路径上的点；（1.按顺序重新命名；2.更新点的位置、up（Y轴）为局部值；3.重新设置_path的点）
         */
        public void Update_Path()
        {
            //transform.childCount 该变换也就当前GameObject的子对象
            Transform[] children = new Transform[transform.childCount];
            Vector3[] points = new Vector3[children.Length];
            Vector3[] ups = new Vector3[children.Length];
            //print ("控制点的数量:" +transform.childCount);
            for (int i = 0; i < transform.childCount; i++)
            {
                children[i] = transform.GetChild(i);
                children[i].gameObject.name = "point " + i;

                points[i] = children[i].localPosition;
                //print ("控制点的坐标:"+points [i]);
                //变换的方向从世界坐标转换到局部坐标。和Transform.TransformDirection相反。
                ups[i] = transform.InverseTransformDirection(children[i].up);
            }

            if (transform.childCount > 1)
            {
                //描述：1.更新路径上的点（点的位置以及方向；均为局部值）；2.更新Path对象其他值：_distances[] _numPoints TotalDistance
                _path.SetPoints(points, ups, isCircuit);
            }
        }

        public Path_Point GetPathPoint(float dist)
        {
            return _path.GetPathPoint(dist, isSmooth);
        }

        #region Gizmo

        //绘制可被点选的gizmos，执行OnDrawGizmos。允许在"场景视图"中快速选择重要的对象。
        //注意: OnDrawGizmos使用相对于场景视图的鼠标位置。如果在检视面板这个组件被折叠，这个函数将不被调用。
        private void OnDrawGizmos()
        {
            DrawGizmos(false);
        }


        //Gizmos只在物体被选择的时候绘制
        private void OnDrawGizmosSelected()
        {
            DrawGizmos(true);
        }

        //Gizmos是用于在场景视图可视化调试或辅助设置。==================game视图中不可用，需要对点做模型，并且提供平移
        private void DrawGizmos(bool selected)
        {

            Update_Path();

            if (transform.childCount <= 1)
            {
                return;
            }

            Path_Point prev = GetPathPoint(0.0f);
            //gizmoLineSize=1.0f; [Range(0.01f, 1.0f)],用来变形的常量
            float dist = -gizmoLineSize;
            //print (dist);
            //print (gizmoLineSize);
            //int i = 0;
            do
            {
                //i += 1;

                //gizmoLineSize / 10 : 控制曲线上点与点的距离
                dist = Mathf.Clamp(dist + gizmoLineSize / 10, 0, _path.TotalDistance);

                //=======！！！此处不断依据CatmullRom算法，找到一个个点
                Path_Point next = GetPathPoint(dist);
                //print("前点："+prev.point.x+","+prev.point.y+","+prev.point.z);
                //print("前点方向："+prev.forward);

                //print("后点："+next.point.x +","+next.point.y+","+next.point.z);
                float distance = Vector3.Distance(prev.point, next.point);
                distance = distance / 2;
                //print("点间距/2："+distance);
                Vector3 V = next.point - prev.point;
                V = V.normalized;
                //print("两点之间的向量："+V);//后点-前点
                float angle = Vector3.Angle(prev.forward, V);
                //print("两向量之间的夹角："+angle);
                //print(Mathf.Sin(angle));
                //print(Mathf.Sin(90));

                //r=(length/2)/sina
                float radius = distance / Mathf.Sin(Math_Functions.DegreesToRadians(angle));
                //print("第" + i + "个点" + "前点：" + prev.point.x + "," + prev.point.y + "," + prev.point.z + "后点：" + next.point.x + "," + next.point.y + "," + next.point.z + "两向量之间的夹角：" + angle + "前点处的转弯半径：" + radius);

                if (radius < 2.7 && radius != 0)
                {

                    Gizmos.color = selected ? Color.red : new Color(1, 0, 0, 0.5f);
                    Gizmos.DrawLine(transform.TransformPoint(prev.point), transform.TransformPoint(next.point));
                    Gizmos.color = selected ? Color.green : new Color(0, 1, 0, 0.5f);
                    //Gizmos.DrawLine(transform.TransformPoint(next.point), transform.TransformPoint(next.point) + transform.TransformDirection(next.up * gizmoLineSize));
                    Gizmos.color = selected ? new Color(0, 1, 1, 1) : new Color(0, 1, 1, 0.5f);
                    //Gizmos.DrawLine(transform.TransformPoint(next.point), transform.TransformPoint(next.point) + transform.TransformDirection(next.right * gizmoLineSize));
                }
                else
                {
                    Gizmos.color = selected ? new Color(0, 1, 1, 1) : new Color(0, 1, 1, 0.5f);
                    Gizmos.DrawLine(transform.TransformPoint(prev.point), transform.TransformPoint(next.point));
                    Gizmos.color = selected ? Color.green : new Color(0, 1, 0, 0.5f);
                    //Gizmos.DrawLine(transform.TransformPoint(next.point), transform.TransformPoint(next.point) + transform.TransformDirection(next.up * gizmoLineSize));
                    Gizmos.color = selected ? Color.red : new Color(1, 0, 0, 0.5f);
                   // Gizmos.DrawLine(transform.TransformPoint(next.point), transform.TransformPoint(next.point) + transform.TransformDirection(next.right * gizmoLineSize));
                }
                prev = next;
            } while (dist < _path.TotalDistance);

        }

        #endregion

    }

    public struct Path_Point
    {
        public Vector3 point;
        public Vector3 forward;//Z轴
        public Vector3 up;//Y轴
        public Vector3 right;//X轴


        public Path_Point(Vector3 point, Vector3 forward, Vector3 up, Vector3 right)
        {
            this.point = point;
            this.forward = forward;
            this.up = up;
            this.right = right;
        }
    }

    public class Path
    {

        public float TotalDistance;

        private Vector3[] _points;
        private Vector3[] _upDirections;
        //每个点距离起始点的距离；起始点为0，终点为总距离
        private float[] _distances;

        private bool _isCircuit = false;
        private int _numPoints;

        //插值参数，用来获取无穷接近的临近点
        private static float interpolationPara = 0.001f;


        // repeatedly used values重复使用的值
        private Path_Point _pathPoint = new Path_Point();
        private float _interpolation = 0.0f;//插值
        private int[] _four_indices = new int[] { 0, 1, 2, 3 }; //指数
        private Vector3[] _four_points = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };

        /*
         *描述：1.更新路径上的点（点的位置以及方向；均为局部值）；2.更新Path对象其他值：_distances[] _numPoints TotalDistance
         *注意：_distances[i] 为点i距离起始点的距离（是之前所有两点之间的直线距离之和，Vector3.Distance(a,b)）
         *      TotalDistance 路径上所有点之间直线距离之和
         */
        public void SetPoints(Vector3[] points, Vector3[] ups, bool isCircuit)
        {

            _isCircuit = isCircuit;
            _numPoints = points.Length;
            _points = points;
            _upDirections = ups;

            TotalDistance = 0.0f;
            _distances = new float[_isCircuit ? _numPoints + 1 : _numPoints];
            for (int i = 0; i < _numPoints - 1; ++i)
            {
                _distances[i] = TotalDistance;
                //Vector3.Distance(a,b)等同于(a-b).magnitude，(a-b)之后向量的长度
                TotalDistance += Vector3.Distance(
                    _points[i],
                    _points[i + 1]);
            }

            // oneMore
            if (_isCircuit)
            {
                _distances[_numPoints - 1] = TotalDistance;
                TotalDistance += Vector3.Distance(
                    _points[_numPoints - 1],
                    _points[0]);
            }

            _upDirections[_numPoints - 1] = ups[_numPoints - 1];
            _distances[_distances.Length - 1] = TotalDistance;

        }

        /*
		 *@dist: distance 点距离起始点的距离(两点之间直线段距离累加)
         * return：返回一个Path_Point 依据传入的distance
		 */
        public Path_Point GetPathPoint(float dist, bool isSmooth)
        {
            
           
            if (_isCircuit)
                dist = (dist + TotalDistance) % TotalDistance;
            else
                dist = Mathf.Clamp(dist, 0.0f, TotalDistance);//限制value的值在min和max之间， 如果value小于min，返回min。 如果value大于max，返回max，否则返回value

            // find segment index
            int index = 1;
            while (_distances[index] < dist)
            {
                index++;
            }            

            // the segment in the middle
            // static float InverseLerp(float from, float to, float value);  eg: (5,10,8)==3/5
            _interpolation = Mathf.InverseLerp(
                _distances[index - 1],
                _distances[index],
                dist);//计算两个值之间的Lerp参数。也就是value在from和to之间的比例值。

            //防止是圆的情况
            index = index % _numPoints;

            // 获取插值两边各两个点的index，总共4个点；
            if (_isCircuit)
            {
                _four_indices[0] = ((index - 2) + _numPoints) % _numPoints;
                _four_indices[1] = ((index - 1) + _numPoints) % _numPoints;
                _four_indices[2] = index % _numPoints;
                _four_indices[3] = (index + 1) % _numPoints;
            }
            else
            {
                _four_indices[0] = Mathf.Clamp(index - 2, 0, _numPoints - 1);
                _four_indices[1] = ((index - 1) + _numPoints) % _numPoints;
                _four_indices[2] = index % _numPoints;
                _four_indices[3] = Mathf.Clamp(index + 1, 0, _numPoints - 1);
            }

            if (isSmooth)
            {

                // assign the four points with the segment in the middle在中间分配四个点与段
                _four_points[0] = _points[_four_indices[0]];
                _four_points[1] = _points[_four_indices[1]];
                _four_points[2] = _points[_four_indices[2]];
                _four_points[3] = _points[_four_indices[3]];

                // you need two points to get a forward direction你需要两点来获得前进的方向
                _pathPoint.point = Math_Functions.CatmullRom(
                    _four_points[0],
                    _four_points[1],
                    _four_points[2],
                    _four_points[3],
                    _interpolation);
                // 获取方向增加点 interpolation值：0.001f
                _pathPoint.forward = Math_Functions.CatmullRom(
                    _four_points[0],
                    _four_points[1],
                    _four_points[2],
                    _four_points[3],
                    _interpolation + interpolationPara) - _pathPoint.point;

                _pathPoint.forward.Normalize();
            }
            else // strait shooting
            {
                _pathPoint.point = Vector3.Lerp(
                    _points[_four_indices[1]],
                    _points[_four_indices[2]],
                    _interpolation);

                _pathPoint.forward = _points[_four_indices[2]] - _points[_four_indices[1]];
                _pathPoint.forward.Normalize();
            }

            // 90 degree turn to right90度右转
            // Vector3.Lerp p=form+(to-form)*t
            //_interpolation = Mathf.InverseLerp
            // 计算点在up方向上的Vector3
            Vector3 tempUpDirection = Vector3.Lerp(_upDirections[_four_indices[1]], _upDirections[_four_indices[2]], _interpolation);
            _pathPoint.right = Vector3.Cross(tempUpDirection, _pathPoint.forward).normalized; // cross

            // 90 degree turn to up90度上转
            _pathPoint.up = Vector3.Cross(_pathPoint.forward, _pathPoint.right).normalized;

            //tempUpDirection.normalized 与 _pathPoint.up不相同

            // now all directions are 90 degrees from each other

            return _pathPoint;
        }

    }
}
