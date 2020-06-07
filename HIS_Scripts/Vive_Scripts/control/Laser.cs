using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ViveHandTracking.Sample
{

    class Laser : MonoBehaviour
    {
        private static Color color = new Color(0.3f, 0, 0, 1);
        private const float angularVelocity = 50.0f;

        public GameObject self;

        public GameObject laser = null;
        public GameObject light = null;
        public Collider collider = null;
        private bool visible = false;
        private Renderer hit = null;
        private Transform WTF = null;   //이걸 어케 맞춰줄라나

        public GameObject pauseMenu;

        //레이저를 쏴서 오브젝트 맞추면 오른손으로 끌고 오기
        //근데 이게 레이저가 주가 되는 스크립트라 될지 모르겠네
        void Start()
        {
            self = GameObject.Find("Laser");
            self.SetActive(false);
            light.SetActive(false);
#if UNITY_ANDROID
            laser.transform.localRotation = Quaternion.identity;
#endif
        }

        void Update()
        {
            if (!pauseMenu.activeInHierarchy)
            {
                var hand = GestureProvider.RightHand;
                if (hand == null)
                {
                    laser.SetActive(false);
                    return;
                }
                GameObject camera = GameObject.FindGameObjectWithTag("MainCamera"); //손 z축 좌표 바꾸기
                transform.position = hand.position;
                //transform.position += camera.transform.forward;  //손 z축 수정 부분

                // smooth rotation for skeleton mode
                if (laser.activeSelf && GestureProvider.HaveSkeleton)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, hand.rotation, 
                                                                    angularVelocity * Time.deltaTime);
                else
                    transform.rotation = hand.rotation;

                laser.SetActive(visible);
            }
        }

        public void OnStateChanged(int state)
        {
            if (!pauseMenu.activeInHierarchy)
            {
                light.SetActive(state == 1);
                if (state == 2)
                    visible = true;
                else
                {
                    visible = false;
                    if (hit != null)
                    {
                        collider.GetComponent<Rigidbody>().isKinematic = false;    //로직을 바꿔봅시다
                        collider.transform.SetParent(null);    //로직을 바꿔봅시다
                        //collider.GetComponent<Rigidbody>().AddForce(collider.transform.forward);
                        StopHit();
                    }
                }
            }
            
        }

        void OnTriggerEnter(Collider other)
        {
            if (!pauseMenu.activeInHierarchy)
            {
                if (!other.gameObject.name.StartsWith("Cube"))
                    return;
                collider = other;
                if (hit != null)
                    StopHit();
                hit = other.GetComponent<Renderer>();
                if (hit != null)
                {
                    hit.material.EnableKeyword("_EMISSION");
                    hit.material.SetColor("_EmissionColor", color);
                    other.transform.SetParent(GameObject.FindGameObjectWithTag("RightHand").transform);   //로직을 바꿔봅시다
                    other.GetComponent<Rigidbody>().isKinematic = true; //로직을 바꿔봅시다
                    Invoke("doSomething", 2);   //2초 후에 던지기
                }
            }
            
        }
        /*void OnTriggerStay(Collider other) //충돌하는 동안 계속 실행되는 함수 - 추가함.
        {
            if (!other.gameObject.name.StartsWith("Cube"))
                return;
            //collider = other;
            //other.GetComponent<Rigidbody>().useGravity = false;
            var forward =  GestureProvider.RightHand.position - other.transform.position ;    //여기 계산이 좀 잘못된 것 같음
            /*other.transform.SetPositionAndRotation(new Vector3(GestureProvider.RightHand.position.x, 
                GestureProvider.RightHand.position.y, GestureProvider.RightHand.position.z-forward.z)
                , GestureProvider.RightHand.rotation);    //일단 무조건 오른손 앞으로 떨궈지게 해놨음
            //other.transform.SetPositionAndRotation(other.transform.position - forward, GestureProvider.RightHand.rotation); //일케 하면 밀리네 ㅋㅋ
            //other.transform.SetPositionAndRotation(other.transform.position + forward, GestureProvider.RightHand.rotation); //캐릭터 위치로 끌어오기
            var backward = other.transform.position - transform.position;   //요거 땜에 계속 튕기는듯
            //other.transform.SetPositionAndRotation(backward, other.transform.rotation);
            other.transform.Translate(-backward);    //메소드를 바꿔봅시다
           
        }*/
        void OnTriggerExit(Collider other)
        {
            if (!pauseMenu.activeInHierarchy)
            {
                if (hit != null && hit == other.GetComponent<Renderer>())
                {
                    other.GetComponent<Rigidbody>().isKinematic = false;    //로직을 바꿔봅시다
                    other.transform.SetParent(null);    //로직을 바꿔봅시다
                    //collider.GetComponent<Rigidbody>().AddForce(collider.transform.forward);    //충돌 종료 시에 앞으로 튀어나가게 하기
                    StopHit();
                }
            }
            
                
        }

        void StopHit()
        {
            hit.material.DisableKeyword("_EMISSION");
            hit = null;
            //collider.GetComponent<Rigidbody>().AddForce(collider.transform.forward);        //충돌 종료 시에 앞으로 튀어나가게 하기
            collider = null;
        }
        void doSomething()
        {
            if(collider != null)
            {
                collider.GetComponent<Rigidbody>().isKinematic = false;
                collider.transform.SetParent(null);
                collider.GetComponent<Rigidbody>().AddForce(transform.forward * 50,ForceMode.VelocityChange);
                StopHit();
            }
        }
    }

}
