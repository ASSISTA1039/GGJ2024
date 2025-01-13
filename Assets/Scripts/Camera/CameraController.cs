using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //[SerializeField] private CharacterInputSystem _inputSystem;
    [SerializeField] public Transform LookAttarGet;
    private Transform playerCamera;

    [Range(0.1f, 1f), SerializeField, Header("鼠标灵敏度")] public float RotateSpeed;
    [Range(0.1f, 0.5f), SerializeField, Header("相机旋转平滑度")] public float rotationSmoothTime = 0.2f;
    [Range(0.1f, 1f), SerializeField, Header("滚轮灵敏度")] private float scrollSpeed;
    //摄像头与角色的距离
    [SerializeField, Header("相机距离")] private float _cameraDistance;
    [SerializeField] private Vector2 _cameraDistanceMinMax = new Vector2(0.01f, 3f);
    [SerializeField] private float distancePlayerOffset;
    [SerializeField, Header("相机俯仰角")] private Vector2 ClmpCameraRang = new Vector2(-65f, 65f);
    [SerializeField, Header("相机缓动")] private Vector3 lookAtPlayerLerpTime;
    [SerializeField, Header("碰撞检测")] public LayerMask collisionLayer;
    [SerializeField] private float colliderMotionLerpTime;

    private Vector3 rotationSmoothVelocity;
    private Vector3 currentRotation;
    private Vector3 _camDirection;

    //摄像头发生碰撞时缓存与角色的距离
    private float _cameraDistanceScrollWheel;
    private float yaw;
    private float pitch;
    private bool start = false;

    private void Awake()
    {
        playerCamera = Camera.main.transform;
        //_inputSystem = GetComponent<CharacterInputSystem>();
    }
    private void Start()
    {
        _camDirection = new Vector3(0f, 0f, -1f);
        _cameraDistance = _cameraDistanceMinMax.y;
    }

    private void Update()
    {
        UpdateCursor();
        GetCameraControllerInput();
        ZoomView();
    }
    //LateUpdate在Update之后执行
    private void LateUpdate()
    {
        ControllerCamera();
        CheckCameraOcclusionAndCollision(playerCamera);
    }

    //隐藏鼠标
    private void UpdateCursor()
    {
        ////隐藏鼠标指针
        //Cursor.visible = false;
        ////固定鼠标指针在屏幕中心
        //Cursor.lockState = CursorLockMode.Locked;

        //CharacterInputSystem.Instance.EnablePlayerLAtk();
    }

    //获取鼠标输入（上下左右滑动）
    private void GetCameraControllerInput()
    {
        yaw += CharacterInputSystem.Instance.cameraLook.x * RotateSpeed;
        pitch -= CharacterInputSystem.Instance.cameraLook.y * RotateSpeed;
        //Mathf.Clamp(a,b,c):限定a在b~c之间
        pitch = Mathf.Clamp(pitch, ClmpCameraRang.x, ClmpCameraRang.y);
    }

    //摄像头旋转、移动
    private void ControllerCamera()
    {
        //让currentRotation平滑的过渡到Vector3(pitch, yaw)
        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
        //世界轴旋转
        transform.eulerAngles = currentRotation;

        Vector3 fanlePos = LookAttarGet.position - transform.forward * distancePlayerOffset;

        Vector3 positionX = new Vector3(transform.position.x, 0, 0);
        Vector3 positionY = new Vector3(0, transform.position.y, 0);
        Vector3 positionZ = new Vector3(0, 0, transform.position.z);

        positionX = Vector3.Lerp(positionX, fanlePos, lookAtPlayerLerpTime.x * Time.deltaTime);
        positionY = Vector3.Lerp(positionY, fanlePos, lookAtPlayerLerpTime.y * Time.deltaTime);
        positionZ = Vector3.Lerp(positionZ, fanlePos, lookAtPlayerLerpTime.z * Time.deltaTime);

        transform.position = new Vector3(positionX.x, positionY.y, positionZ.z);
    }

    //相机碰撞检测
    private void CheckCameraOcclusionAndCollision(Transform camera)
    {
        //TransformPoint:将相对坐标转化为世界坐标
        Vector3 desiredCamPosition = transform.TransformPoint(_camDirection * (_cameraDistanceMinMax.y + 0.1f));

        //Physics.Linecast(Vector3 start, Vector3 end, out hit, int Layer) start:射线起点；end:射线终点；hit:返回碰撞体的信息；Layer:检测的层级
        //Physics.Linecast(Vector3 start, Vector3 end, out hit, int Layer) start:射线起点；end:射线终点；hit:返回碰撞体的信息；Layer:检测的层级
        if (Physics.Linecast(transform.position, desiredCamPosition, out var hit, collisionLayer))
        {
            //当检测到有遮挡（碰撞体）时，限制摄像头与角色的最大距离
            _cameraDistance = Mathf.Clamp(hit.distance, _cameraDistanceMinMax.x, _cameraDistanceScrollWheel);

            start = true;
        }
        else
        {
            //从有遮挡变为无遮挡时，将前面缓存的摄像头与角色的距离实例化
            if (start)
            {
                _cameraDistance = _cameraDistanceScrollWheel;
                start = false;
            }
            //缓存摄像头与角色的距离
            _cameraDistanceScrollWheel = _cameraDistance;

        }
        //绘制射线，易于观察
        Debug.DrawRay(transform.position, (desiredCamPosition - transform.position) * (_cameraDistance) / (_cameraDistanceMinMax.y + 0.1f), Color.red);

        camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, _camDirection * (_cameraDistance - 0.1f), colliderMotionLerpTime * Time.deltaTime);

    }

    //镜头远近
    void ZoomView()
    {
        _cameraDistance -= CharacterInputSystem.Instance.CameraDistance * scrollSpeed * 0.01f;
        //远近范围
        _cameraDistance = Mathf.Clamp(_cameraDistance, _cameraDistanceMinMax.x, _cameraDistanceMinMax.y);
        _cameraDistanceScrollWheel -= CharacterInputSystem.Instance.CameraDistance * scrollSpeed * 0.01f;
        _cameraDistanceScrollWheel = Mathf.Clamp(_cameraDistanceScrollWheel, _cameraDistanceMinMax.x, _cameraDistanceMinMax.y);

    }
}
