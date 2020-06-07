using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ViveHandTracking
{

    enum HandColliderType { None, Trigger, Collider }

    class HandRenderer : MonoBehaviour
    {
        public Text test;
        public GameObject player;
        public GameObject[] rightHandModels;
        public GameObject[] leftHandModels;
        public GameObject handModel;
        int init = 4;
        // color look-up for different gestures
        private static Color32[] gesture_colors = new Color32[] {
    new Color32(0, 255, 0, 0), new Color32(255, 255, 255, 0), new Color32(0, 0, 255, 0),
    new Color32(0, 255, 255, 0), new Color32(255, 20, 147, 0), new Color32(255, 215, 0, 0),
    new Color32(255, 128, 64, 0),
  };

        // Links between keypoints, 2*i & 2*i+1 forms a link.
        // keypoint index: 0: palm, 1-4: thumb, 5-8: index, 9-12: middle, 13-16: ring, 17-20: pinky
        // fingers are counted from bottom to top
        private static int[] Connections = new int[] {
    0, 1, 0, 5, 0, 9, 0, 13, 0, 17, // palm and finger starts
    2, 5, 5, 9, 9, 13, 13, 17, // finger starts
    1, 2, 2, 3, 3, 4, // thumb
    5, 6, 6, 7, 7, 8, // index
    9, 10, 10, 11, 11, 12, // middle
    13, 14, 14, 15, 15, 16, // ring
    17, 18, 18, 19, 19, 20, // pinky
  };

        [Tooltip("Draw left hand if true, right hand otherwise")]
        public bool isLeft = false;
        [Tooltip("Default color of hand points")]
        public Color pointColor = Color.blue;
        [Tooltip("Default color of links between keypoints in skeleton mode")]
        public Color linkColor = Color.white;
        [Tooltip("Show gesture color on points (2D/3D mode) or links (skeleton mode)")]
        public bool showGestureColor = false;
        [Tooltip("Material for hand points and links")]
        //제스처에 따라 손 구 색상이 바뀌게 하는 메터리얼 변수
        [SerializeField]
        private Material material = null;
        [Tooltip("Collider type created with hand. The layer of the object is same as this object.")]
        public HandColliderType colliderType = HandColliderType.None;

        // list of points created (1 for 3D/2D point, 21 for skeleton)
        private List<GameObject> points = new List<GameObject>();
        // list of links created (only for skeleton)
        private List<GameObject> links = new List<GameObject>();
        // trigger collider object, only used in skeleton mode
        private GameObject colliderObject = null;

        IEnumerator Start()
        {
            rightHandModels = new GameObject[6];
            leftHandModels = new GameObject[6];
            // wait until detection is started, so we know what mode we are using
            while (GestureProvider.Status == GestureStatus.NotStarted)
                yield return null;

            int count = GestureProvider.HaveSkeleton ? 21 : 1;
            //우덜은 count가 1일거야 2D니깐.
            for (int i = 0; i < count; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = "point" + i; //그럼 포인트가 pint 0 이겠네
                go.transform.parent = transform;
                //go.transform.localScale = Vector3.one * 0.012f;   
                go.transform.localScale = Vector3.one * 0.02f;   //손 크기를 바꾸면 충돌판정이 잘 되겠지
                go.SetActive(false); // 일단 안보이게 한다 이거네?
                points.Add(go);

                // handle layer
                go.layer = gameObject.layer;

                // handle material
                go.GetComponent<Renderer>().material = new Material(material);
                go.GetComponent<Renderer>().material.color = pointColor;

                // handle collider, GameObject.CreatePrimitive returns object with a non-trigger collider
                if (colliderType != HandColliderType.Collider)
                {
                    var collider = go.GetComponent<Collider>();
                    // for trigger collider in skeleton mode, we create an extra game object with one collider
                    if (!GestureProvider.HaveSkeleton && colliderType == HandColliderType.Trigger)
                        collider.isTrigger = true;
                    else
                        GameObject.Destroy(collider);
                }
            }

        }

        void Update()
        {
            // no need to update since result is same as last frame
            if (!GestureProvider.UpdatedInThisFrame)
                return;

            // hide points and links if no hand is detected
            var hand = isLeft ? GestureProvider.LeftHand : GestureProvider.RightHand;
            if (hand == null)
            {
                foreach (var p in points)
                    p.SetActive(false);
                foreach (var l in links)
                    l.SetActive(false);
                if (colliderObject != null)
                    colliderObject.SetActive(false);
                handModel.SetActive(false);
                return;
            }
            else
            {
                handModel = gameObject.transform.GetChild(init).gameObject;
                handModel.SetActive(true);
            }

            //if(hand == isLeft)

            // update base position for collision detection
            transform.position = hand.position;   //원본
                                                  //handModel.transform.position = hand.position;
                                                  //transform.position = hand.position + player.transform.forward;   //수정
                                                  //GameObject camera = GameObject.FindGameObjectWithTag("MainCamera"); //손 z축 좌표 바꾸기 //3-30주석
                                                  //transform.position += camera.transform.forward;  //손 z축 수정 부분                     //3-30주석
                                                  //transform.rotation = hand.rotation;

            handModel.transform.position = hand.position + hand.rotation * new Vector3(0,0,0.5f);
            ////이 부분 바꾸면 될거 같음.
            //// update points and links position
            //for (int i = 0; i < points.Count; i++)
            //{
            //    var go = points[i];
            //    //go.transform.position = hand.points[i] + player.transform.forward; //수정
            //    go.transform.position = hand.points[i];     //원본

            //    //go.SetActive(go.transform.position.IsValidGesturePoint());
            //    // update gesture color on points for non skeleton mode
            //    if (showGestureColor && !GestureProvider.HaveSkeleton)
            //    {
            //        var a = (int)hand.gesture;
            //        test.text = hand.gesture.ToString() + "\n" + a.ToString();
            //        go.GetComponent<Renderer>().material.color = gesture_colors[(int)hand.gesture];
            //    }
            //}
            //go.transform.position = hand.points[i] + player.transform.forward; //수정
            //gameObject.transform.GetChild(4).transform.position = hand.position + hand.rotation * new Vector3(0,1,2f);     //원본
            //go.SetActive(go.transform.position.IsValidGesturePoint());
            // update gesture color on points for non skeleton mode
            if (showGestureColor && !GestureProvider.HaveSkeleton)
            {
                //var a = (int)hand.gesture;
                test.text = hand.gesture.ToString();
                if (handModel.name != hand.gesture.ToString())
                {
                    handModel.SetActive(false);
                    for (int i = 0; i < 6; i++)
                    {
                        if (gameObject.transform.GetChild(i).name == hand.gesture.ToString())
                        {
                            handModel = gameObject.transform.GetChild(i).gameObject;
                            init = i;
                            handModel.SetActive(true);
                        }
                    }
                }
                //go.GetComponent<Renderer>().material.color = gesture_colors[(int)hand.gesture];
            }
            for (int i = 0; i < links.Count; i++)
            {
                var link = links[i];
                link.SetActive(false);

                int startIndex = Connections[i * 2];
                var pose1 = hand.points[startIndex];
                if (!pose1.IsValidGesturePoint())
                    continue;

                var pose2 = hand.points[Connections[i * 2 + 1]];
                if (!pose2.IsValidGesturePoint())
                    continue;

                // calculate link position and rotation based on points on both end
                link.SetActive(true);
                link.transform.position = (pose1 + pose2) / 2;
                var direction = pose2 - pose1;
                link.transform.rotation = Quaternion.FromToRotation(Vector3.forward, direction);
                link.transform.localScale = new Vector3(0.005f, 0.005f, direction.magnitude);

                // update gesture color on links for skeleton mode
                if (showGestureColor)
                    link.GetComponent<Renderer>().material.color = gesture_colors[(int)hand.gesture];
            }

            if (colliderObject == null)
                return;

            // update trigger collider bounds in skeleton mode
            var bounds = new Bounds(transform.position, Vector3.zero);
            foreach (var renderer in transform.GetComponentsInChildren<Renderer>())
                bounds.Encapsulate(renderer.bounds);
            colliderObject.transform.position = bounds.center;
            colliderObject.transform.rotation = Quaternion.identity;
            colliderObject.transform.localScale = bounds.size;
            colliderObject.SetActive(true);
            if (isLeft)
            {
                transform.rotation = player.transform.rotation;
                /*if (handModel != null)
                    handModel.SetActive(false);
                handModel = leftHandModels[(int)hand.gesture];
                handModel.SetActive(true);*/
            }
            else
            {
                transform.rotation = player.transform.rotation;
                /*if (handModel != null)
                    handModel.SetActive(false);
                handModel = leftHandModels[(int)hand.gesture];
                handModel.SetActive(true);*/
            }
        }
    }

}
