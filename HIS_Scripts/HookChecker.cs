using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ViveHandTracking.Sample
{
    class HookChecker : MonoBehaviour
    {
        public GameObject Grapple;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "CanGrapple")
            {
                Grapple.GetComponent<GrapplingHook>().hooked = true;
                Grapple.GetComponent<GrapplingHook>().hookedObj = other.gameObject;
            }


        }
    }
}
