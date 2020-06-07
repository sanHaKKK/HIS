using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ViveHandTracking.Sample
{
    class GrapplingHook : MonoBehaviour
    {
        public GameObject hook;     //물체에 직접 걸리는 고리
        public GameObject camera;
        public GameObject playerBody;
        public float hookTravelSpeed;   //훅이 이동하는 속도
        public float playerTravelSpeed; //플레이어가 날아가는 속도
        public Collider collider = null;    //이것도 의미 없는 거
        public float angularVelocity = 50.0f;   //이건 뭔지 모르겠음
        private float travelDistance = 0;   //훅이 날아간 거리1
        public bool visible = false;        //Hook이 눈에 보이는가
        public static bool fired;           //Hook이 발사됬는가
        public bool hooked;                 //훅이 물체에 걸렸는가
        private Renderer hit = null;        //이것도 사실 상관 없을듯
        public float maxDistance;   //훅 최대 사거리  
        private float currentDistance;  //훅이 현재 날아간 거리2
        private int state = 0;  //현재 state
        public GameObject player;
        public GameObject hookedObj;
        public GameObject rightHand;
        public Vector3 forward;         //훅 방향 조절용 임시변수
        public GameObject pauseMenu;
        private Text logTxt;
        // Start is called before the first frame update
        void Start()
        {
            hook.SetActive(false);
            hook.SetActive(false);
#if UNITY_ANDROID
            hook.transform.localRotation = Quaternion.identity;
#endif
        }

        // Update is called once per frame
        void Update()
        {
            //hook.transform.localRotation = Quaternion.identity;
            transform.localRotation = Quaternion.identity;
            if (!pauseMenu.activeInHierarchy)
            {

                var hand = GestureProvider.RightHand;

                if (hand == null)
                {
                    hook.SetActive(false);
                    return;
                }
                camera = GameObject.FindGameObjectWithTag("MainCamera"); //손 z축 좌표 바꾸기
                transform.position = hand.position;

                transform.rotation = camera.transform.rotation;

                if (hook.activeSelf && GestureProvider.HaveSkeleton)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, hand.rotation, angularVelocity * Time.deltaTime);
                else
                    transform.rotation = camera.transform.rotation;
                //transform.rotation = hand.rotation;

                hook.SetActive(visible);
                if (fired)
                {
                    LineRenderer rope = hook.GetComponent<LineRenderer>();  //훅이 날아가는 경로 그리는 line Renderer
                    rope.SetVertexCount(2);     //2개의 점으로 선을 그리겠다.
                    rope.SetPosition(0, playerBody.transform.position);        //첫 번째 vertex 는 Grappling Hook Object위치
                    rope.SetPosition(1, hook.transform.position);           //두 번째 vertex는 hook의 현재 위치
                }
                if (fired == true && hooked == false)           //훅이 발사는 됬는데 물체에 걸리진 않았다.
                {

                    forward = camera.transform.rotation * camera.transform.forward;     //카메라 오른쪽, 앞쪽을 외적해서 벡터 계산 했음(근데 쓸모 없음)

                    hook.transform.Translate((Vector3.forward) * Time.deltaTime * hookTravelSpeed, Space.Self);    //Space.Self를 쓰니까 손 기준 앞 방향으로 잘 나가더군요

                    currentDistance = Vector3.Distance(transform.position, hook.transform.position);        //현재 플레이어와 hook의 거리 계산

                    if (currentDistance >= maxDistance)         //최대 사거리를 벗어날 경우 훅을 돌려보낸다
                    {
                        returnHook();
                    }
                }
                if (fired == true && hooked == true)             //훅이 발사 됬고 물체에 걸렸다.
                {
                    hook.transform.parent = hookedObj.transform;                //훅을 부딪힌 옵젝에 붙어있게 하기
                    player.transform.position = Vector3.MoveTowards(transform.position, hook.transform.position, playerTravelSpeed * Time.deltaTime);       //일정 속도로 플레이어를 훅 위치로 이동
                    //player.GetComponent<Rigidbody>().AddForce((hook.transform.position - transform.position).normalized , ForceMode.Force);
                    travelDistance = Vector3.Distance(transform.position, hook.transform.position);                     //플레이어가 이동한 거리

                    player.GetComponent<Rigidbody>().useGravity = false;                                            //날아가는 동안엔 직선으로 이동사니까 중력 삭제

                    if (travelDistance < 10)  //or 2                                                              //플레이어가 물체에 꼬라박지 않기 위해 최소 거리 둠
                    {
                        //player.GetComponent<Rigidbody>().AddForce((playerBody.transform.up + playerBody.transform.forward).normalized * 40, ForceMode.VelocityChange);
                        Invoke("Jump", 1f);
                        returnHook();                                                                           //returnHook() 에서 충돌 종료 및 플레이어 행동 설정해줌
                    }
                    else
                    {
                        //player.GetComponent<Rigidbody>().useGravity = true;                                     //음 이건 솔직히 왜 있는 진 모르겠지만 지금은 문제 없음
                        //hooked = false;
                    }
                }
            }

        }
        public void Jump()
        {
            player.GetComponent<Rigidbody>().AddForce((camera.transform.up + camera.transform.forward).normalized * 10, ForceMode.VelocityChange);
            player.GetComponent<Rigidbody>().useGravity = true;
        }
        public void OnStateChanged(int state)
        {
            if (!pauseMenu.activeInHierarchy)
            {
                this.state = state;

                hook.SetActive(state == 1);
                visible = true;
                if (state == 2)             //훅이 쏴질 때 처리하는 코드
                {
                    if (fired == false)
                    {
                        visible = true;

                        fired = true;
                        hooked = false;
                    }

                }

                else
                {
                    visible = false;
                    if (hookedObj != null)
                    {
                        hooked = false;
                    }
                }
            }

        }
        public void returnHook()            //훅을 돌려보내고 훅 이동 경로 삭제, 플레이어 중력 다시 적용하는 코드
        {
            hook.transform.position = transform.position;
            visible = false;
            fired = false;
            hooked = false;
            hook.transform.parent = transform;
            //player.GetComponent<Rigidbody>().useGravity = true;
            LineRenderer rope = hook.GetComponent<LineRenderer>();
            hook.transform.localRotation = Quaternion.identity;
            rope.SetVertexCount(0);
        }
        
    }
}
