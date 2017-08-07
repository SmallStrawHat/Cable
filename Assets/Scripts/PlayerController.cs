using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BLINDED_AM_ME{
	public class PlayerController : MonoBehaviour
	{
		private Transform m_Transform;
		public float smoothTime = 0.1f;  //摄像机平滑移动的时间
		private Vector3 cameraVelocity = Vector3.zero;
		private Camera mainCamera;  //主摄像机

		public float speed = 3f;
		public float forwardSpeed = 0.2f;
		public float rotateSpeed = 3.0f;
		private bool rotate = false;
		public float maxView = 90;
		public float minView = 10;

		private Line line;//主电缆对象
		private static int editFlag = 0;
		private static int pointFlag = 0;
		private static int connectFlag = 0;
		private static int deleteFlag = 0;


		// Use this for initialization
		void Start()
		{
			m_Transform = gameObject.GetComponent<Transform>();
			mainCamera = Camera.main;
		}

		// Update is called once per frame
		void Update()
		{
			MoveControl();
			ChangeCamera();
		}

		void MoveControl()
		{
			if (Input.GetKey(KeyCode.W))
			{
				m_Transform.Translate(Vector3.forward * forwardSpeed, Space.Self);
			}

			if (Input.GetKey(KeyCode.S))
			{
				m_Transform.Translate(Vector3.back * forwardSpeed, Space.Self);
			}

			if (Input.GetKey(KeyCode.A))
			{
				m_Transform.Translate(Vector3.left * forwardSpeed, Space.Self);
			}

			if (Input.GetKey(KeyCode.D))
			{
				m_Transform.Translate(Vector3.right * forwardSpeed, Space.Self);
			}

			//up
			if (Input.GetKey(KeyCode.R))
			{
				m_Transform.Translate(Vector3.up * forwardSpeed, Space.Self);
			}
			//down
			if (Input.GetKey(KeyCode.F))
			{
				m_Transform.Translate(Vector3.up * -forwardSpeed, Space.Self);
			}

			if (Input.GetKey(KeyCode.Q))
			{
				m_Transform.Rotate(Vector3.up, -rotateSpeed);
				mainCamera.transform.RotateAround(transform.position, Vector3.up, -rotateSpeed);
			}

			if (Input.GetKey(KeyCode.E))
			{
				m_Transform.Rotate(Vector3.up, rotateSpeed);
				mainCamera.transform.RotateAround(transform.position, Vector3.up, rotateSpeed);
			}

			//尾部加点
			if (Input.GetKey(KeyCode.O))
			{
				//Vector3 point
				Line.addPoint();
			}

			//增加接头
			if (Input.GetKey(KeyCode.K))
			{
				//Vector3 point
				Line.addConnectComp();
			}

			//m_Transform.Rotate(Vector3.up, Input.GetAxis("Mouse X"));
			//m_Transform.Rotate(Vector3.left, Input.GetAxis("Mouse Y"));
			mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, m_Transform.position + new Vector3(0, 2,-3), ref cameraVelocity, smoothTime);
		}

		void ChangeCamera() {
			//滑动鼠标滑轮控制视角的大小
			float offsetView = Input.GetAxis("Mouse ScrollWheel") * -speed;
			float tmpView = offsetView + Camera.main.fieldOfView;
			tmpView = Mathf.Clamp(tmpView, minView, maxView);
			Camera.main.fieldOfView = tmpView;

			//绕主角旋转摄像机
			if (rotate)
			{
				mainCamera.transform.RotateAround(transform.position, Vector3.up, speed * Input.GetAxis("Mouse X"));
			}
			//GetMouseButtonDown : 后面参数0是左键，1是右键，2是中键		
			if (Input.GetMouseButtonDown(1))
			{
				rotate = true;
			}
			if (Input.GetMouseButtonUp(1))
			{
				rotate = false;
			}
		}
	}

}
