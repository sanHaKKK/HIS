using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ViveHandTracking
{
    public class Teleport : MonoBehaviour
    {
        public Transform Cursor = null;
        private Transform Camera = null;
        private Transform Anchor = null;

        private Text logTxt1 = null;
        private Text logTxt2 = null;
        private Text logTxt3 = null;
        private int state = 0;

        public VRTeleporter teleporter;

        public Vector3 point;
        GameObject player;
        public Transform playerChild;

        public GameObject pauseMenu;
        void Awake()
        {
            //Teleporter = GameObject.Find("Teleporter");
            Anchor = new GameObject("Anchor").transform;
            Anchor.parent = transform;
            point = new Vector3(-1, -1, -1);

            player = GameObject.Find("Offset");
            playerChild = player.transform.GetChild(1);

            //logTxt1 = GameObject.Find("LogText").GetComponent<Text>();
            //logTxt1.gameObject.SetActive(true);

            //logTxt1.text = "Hello \n";
            //logTxt2 = GameObject.Find("LogText2").GetComponent<Text>();
            //logTxt2.gameObject.SetActive(true);
            //logTxt1.text = "Hello \n";

        }

        // Start is called before the first frame update
        void Start()
        {
            Cursor.gameObject.SetActive(false);
            teleporter.ToggleDisplay(false);
            Camera = GestureProvider.Current.transform;
        }

        // Update is called once per frame
        void Update()
        {
            if (!pauseMenu.activeInHierarchy)
            {
                // 초기 상태
                if (state == 0)
                {
                    teleporter.ToggleDisplay(false);
                    return;
                }
                // 손꾸락이 인식되는 상태
                Cursor.position = (GestureProvider.LeftHand.position + GestureProvider.RightHand.position) / 2;
                var forward = Cursor.position - Camera.position;
                Cursor.position += forward;
                teleporter.ToggleDisplay(true);
                transform.position = Anchor.position = Camera.position;
                transform.rotation = Anchor.rotation = Quaternion.LookRotation(forward, Camera.up);
                //손바닥 펴진 상태
                if (state == 2)
                    return;
            }
            
        }
        //로그 메시지를 보자
        /*void showMeLog()
        {
            logTxt1.text = "카메라 rotation값 w,x,y,z \n" + Camera.rotation.w.ToString() + "\n" + 
                Camera.rotation.x.ToString() + "\n" + 
                Camera.rotation.y.ToString() + "\n" + 
                Camera.rotation.z.ToString();

            logTxt2.text = "플레이어 rotation값 w,x,y,z \n" + player.transform.rotation.w.ToString() + "\n" + 
                player.transform.rotation.x.ToString() + "\n" + 
                player.transform.rotation.y.ToString() + "\n" + 
                player.transform.rotation.z.ToString();
        }*/
        public void onStateChanged(int state)
        {
            if (!pauseMenu.activeInHierarchy)
            {
                this.state = state;
                if (state == 2)
                {

                    point = Cursor.position;
                    var forward = point - player.transform.position;
                    point += forward;

                    //player.transform.Translate(player.transform.GetChild(1).forward.normalized, Space.World);


                    teleporter.Teleport();
                    //Cursor.gameObject.SetActive(state == 1);

                    teleporter.ToggleDisplay(false);
                }
                //Cursor.gameObject.SetActive(state == 1);
                //teleporter.ToggleDisplay(false);
            }

        }
    }
}