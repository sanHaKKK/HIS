using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ViveHandTracking.Sample {

// This script controls action about grabing boxes remotely
class RemoteGrab : MonoBehaviour {
  private static Color grabColor = new Color(0, 0, 0.3f, 1);
  private static Color selectColor = new Color(0, 0.3f, 0, 1);

  public Transform Cursor = null;

  private Transform Camera = null;
  private Transform Anchor = null;
  private int state = 0;
  private Renderer candidate = null;
  private Renderer selected = null;
  private Transform Poong = null;

  void Awake() {
    Anchor = new GameObject("Anchor").transform;
    Anchor.parent = transform;
    Poong = new GameObject("Poong").transform;  //끌어오는 기능 때문에 만들어놈 
    //Poong.parent = transform;
  }

  void Start() {
    Cursor.gameObject.SetActive(false);
    Camera = GestureProvider.Current.transform;
  }

  void Update() {
    if (state == 0)
      return;
    Cursor.position = (GestureProvider.LeftHand.position + GestureProvider.RightHand.position) / 2;
    Poong.position = (GestureProvider.LeftHand.position + GestureProvider.RightHand.position) / 2;  //물체를 두 손 가운데로 끌어오자
    var forward = Cursor.position - Camera.position;
    var backward = Camera.position - Cursor.position;   //딱 캐릭터 시점으로 가기 위함
    Cursor.position += forward;
    Poong.position += backward; //딱 캐릭터 시점으로 가기 위함
    transform.position = Anchor.position = Camera.position;
    transform.rotation = Anchor.rotation = Quaternion.LookRotation(forward, Camera.up);

    if (state == 2)
      return;

    // find hit objects by raycast
    RaycastHit hit;
    LayerMask mask = LayerMask.GetMask("Default"); //이게 무슨 기능이지? 찾아보자
    if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, mask)) {
      if (candidate == hit.collider.GetComponent<Renderer>())
        return;
      SetCandidate(hit.collider);
    } else
      ClearCandidate();
  }

  public void OnStateChanged(int state) {
    this.state = state;
    Cursor.gameObject.SetActive(state == 1);
    if (state == 2) {
      selected = candidate;
      if (selected != null) {
        //selected.transform.Translate((GestureProvider.LeftHand.position + GestureProvider.RightHand.position) / 2 , Space.World);
        selected.GetComponent<Rigidbody>().useGravity = false;
        selected.GetComponent<Rigidbody>().drag = 5f;
        Anchor.SetParent(selected.transform.parent, true);
        //Poong.SetParent(selected.transform.parent, true);
        selected.transform.SetParent(Anchor, true);
        //selected.transform.SetParent(Poong, true);  //이러면 될라나
      }
    } else if (selected != null) {
      selected.GetComponent<Rigidbody>().useGravity = true;
      selected.GetComponent<Rigidbody>().drag = 0.5f;
      selected.transform.SetParent(Anchor.parent, true);
      Anchor.SetParent(transform, true);
      selected = null;
    }
    if (selected != null)
      selected.material.SetColor ("_EmissionColor", selectColor);
    else if (state != 1)
      ClearCandidate();
  }

  void SetCandidate(Collider other) {
    if (candidate != null)
      ClearCandidate();
    candidate = other.GetComponent<Renderer>();
    if (candidate != null) {
      candidate.material.EnableKeyword("_EMISSION");
      candidate.material.SetColor ("_EmissionColor", grabColor);
    }
  }

  void ClearCandidate() {
    if (candidate != null)
      candidate.material.DisableKeyword("_EMISSION");
    candidate = null;
  }
}

}
