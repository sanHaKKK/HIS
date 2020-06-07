using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ViveHandTracking
{
    public class RightHandActiveChecker : MonoBehaviour
    {
        // Start is called before the first frame update
        void Awake()
        {
        }
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            var hand = GestureProvider.RightHand;
            if (hand == null)
            {
                gameObject.SetActive(false);
            }
            else
                gameObject.SetActive(true);
        }
    }
}
