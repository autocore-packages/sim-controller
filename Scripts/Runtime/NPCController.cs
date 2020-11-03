using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.simai;

namespace Assets.Scripts.simController
{
    public class NPCController : NPCObj
    {
        public override float SpeedCurrent
        {
            get => currentSpeed;
            set
            {
            }
        }
        public float currentSpeed = 0;
        public override float BrakeDistance
        {
            get => currentSpeed * currentSpeed / acceleration_break / 2 + 2;
            set
            {
            }
        }
        public float acceleration_break = 5;
        public float acceleration_forward = 3;

        protected override void Update()
        {
            base.Update();
            if (!isCarDrive) return;
            transform.LookAt(posAimTemp);
            if (currentSpeed < speedAim) currentSpeed += Time.deltaTime / 1 * acceleration_forward;
            else if (currentSpeed > speedAim) currentSpeed -= Time.deltaTime / 1 * acceleration_break;
            transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);
        }
    }
}
