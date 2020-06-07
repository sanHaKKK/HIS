using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlaceOnPlane : MonoBehaviour
{
    public GameObject board;
    public GameObject arr;
    public GameObject arr2;
    public GameObject grabtp;
    public GameObject picktp;
    public List<GameObject> allArr;

    public Text gest;
    public Text track;

    
    private ManoGestureContinuous pinch;
    private ManoGestureContinuous release;
    private ManoGestureContinuous open;
    private ManoGestureContinuous grab;
    private ManoGestureContinuous point;


    private Vector2 beVec;
    private Vector2 afVec;
    private Vector3 pose;
    private Vector3 normVec;
    private int idx;
    private bool pick = false;
    private bool take = false;

    void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        pinch = ManoGestureContinuous.HOLD_GESTURE;
        release = ManoGestureContinuous.OPEN_HAND_GESTURE;
        open = ManoGestureContinuous.OPEN_PINCH_GESTURE;
        grab = ManoGestureContinuous.CLOSED_HAND_GESTURE;
        point = ManoGestureContinuous.POINTER_GESTURE;

        normVec = new Vector3(0, 0, 0);
    }

    private void normalize()
    {
        Vector3 before = new Vector3(beVec.x - 0.5f, -0.35f, beVec.y + 0.5f);
        Vector3 after = new Vector3(afVec.x - 0.5f, -0.35f, afVec.y + 0.5f);
        normVec = Vector3.Normalize(before - after);
    }


    void Update()
    {
        if (normVec != new Vector3(0, 0, 0))
        {
            allArr[idx].transform.position += normVec / 60;
            if (allArr[idx].transform.position.x > 1.0f || allArr[idx].transform.position.x < -1.0f || allArr[idx].transform.position.z > 2.0f || allArr[idx].transform.position.x < 0f)
            {
                Destroy(allArr[idx]);
                //normVec = new Vector3(0, 0, 0);
            }
            else return;
        }

        //track.text = ManomotionManager.Instance.Hand_infos[0].hand_info.tracking_info.palm_center.ToString();
        gest.text = ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.mano_gesture_continuous.ToString();

        if (ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.mano_gesture_continuous == pinch)
        {
            float min = 1000;
            idx = 0;
            beVec = ManomotionManager.Instance.Hand_infos[0].hand_info.tracking_info.poi;
            for (int i = 0; i < allArr.Count; i++)
            {
                float compare = Vector2.Distance(beVec, new Vector2(allArr[i].transform.position.x + 0.5f, allArr[i].transform.position.z - 0.5f));
                if (compare < min) { idx = i; min = compare; }
            }
            track.text = idx.ToString() + ", " + min.ToString();
            
            if (pick == false)
            {
                picktp = Instantiate(arr2, allArr[idx].transform.position, board.transform.rotation);
                pick = true;
            }
            else picktp.transform.position = allArr[idx].transform.position;

        }
        else if (ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.mano_gesture_continuous == open)
        {
            Destroy(picktp);
            if (pick == true)
            {
                afVec = ManomotionManager.Instance.Hand_infos[0].hand_info.tracking_info.poi;
                pick = false;
                normalize();
            }
        }
        else if (ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.mano_gesture_continuous == grab)
        {
            pose = new Vector3(ManomotionManager.Instance.Hand_infos[0].hand_info.tracking_info.palm_center.x - 0.5f, -0.35f, ManomotionManager.Instance.Hand_infos[0].hand_info.tracking_info.palm_center.y + 0.5f);
            if (take == false) grabtp = Instantiate(arr2, pose, board.transform.rotation);
            else grabtp.transform.position = pose;

            take = true;
        }
        else if (ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.mano_gesture_continuous == release)
        {
            Destroy(grabtp);
            if (take == true)
            {
                take = false;
                if (ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.hand_side == HandSide.Palmside)
                {
                    foreach (GameObject destroyArray in allArr)
                    {
                        Destroy(destroyArray);
                    }
                    normVec = new Vector3(0, 0, 0);
                    return;
                }
                pose = new Vector3(ManomotionManager.Instance.Hand_infos[0].hand_info.tracking_info.palm_center.x - 0.5f, -0.35f, ManomotionManager.Instance.Hand_infos[0].hand_info.tracking_info.palm_center.y + 0.5f);
                allArr.Add(Instantiate(arr, pose, board.transform.rotation));
            }
        }
        else if (ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.mano_gesture_continuous == point)
        {
            normVec = new Vector3(0, 0, 0);
        }



        //Instantiate(spawnPrefab, spawnPosition, board.transform.rotation);
    }
}
