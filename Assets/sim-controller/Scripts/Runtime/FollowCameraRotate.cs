using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.simController
{

    public class FollowCameraRotate : MonoBehaviour
    {
        public Vector3 offset;
        // Update is called once per frame
        void Update()
        {
            transform.rotation = Quaternion.Euler(OverLookCameraController.Instance.transform.rotation.eulerAngles + offset);
        }
    }
}
