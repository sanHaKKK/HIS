using UnityEngine;

namespace ViveHandTracking.Sample {

class HandDisplaySwitch : MonoBehaviour {
  public GameObject Model = null;
  public GameObject Skeleton = null;
  [SerializeField]
  private bool showModel = false;

  void Awake() {
    Model.SetActive(showModel);
    Skeleton.SetActive(showModel);
  }

  void Update () {
    if (Input.GetKeyDown(KeyCode.Space))
      SwitchDisplay();
  }

  void SwitchDisplay() {
    showModel = !showModel;
    Model.SetActive(showModel);
    Skeleton.SetActive(!showModel);
  }

  public void OnStateChanged(int state) {
    if (state == 1)
      SwitchDisplay();
  }
}

}
