using Assets.Scripts.simai;
using Assets.Scripts.SimuUI;
using System.Collections;
using System.Collections.Generic;
using AutoCore.Sim.Vehicle.Control;
using UnityEngine;

namespace Assets.Scripts.simController
{
    [RequireComponent(typeof(WheelDrive))]
    public class EgoVehicleController : ObjEgo, IVehicle
    {
        public WheelDrive wd;
        public float maxSpeed=100;
        private void OnEnable()
        {
            wd = GetComponent<WheelDrive>();
        }

        void FixedUpdate()
        {
            accelerate = (wd.speed - lastSpeed) / 0.02f;
            lastSpeed = wd.speed;
            if (ElementsManager.Instance.CurrentEgo == this)
            {
                if (PanelCarMessage.Instance != null)
                {
                    PanelCarMessage.Instance.UpdateCarmessage(wd.steer, wd.str_Odom, wd.brake, wd.throttle, wd.speed, LinearVelocity);
                }
                if (!TestManager.Instance.isROSControl&&ElementsManager.Instance.CurrentEgo==this)
                {
                    wd.steer = Input.GetAxis("Horizontal");
                    wd.throttle = Mathf.Abs(wd.speed) < maxSpeed / 3.6f ? Input.GetAxis("Vertical") : 0;
                    wd.brake = Input.GetKey(KeyCode.X) ? 1 : 0;
                }
                else
                {
                    SpeedCalculate();
                }
            }
        }
        public float LinearVelocity
        {
            get => aimSpeed;
            set => aimSpeed = value;
        }
        public float LinearAcceleration
        {
            get => accelerate;
            set
            {
            }
        }
        public float SteeringAngle
        {
            get => wd.steer;
            set
            {
                wd.steer = value;
            } 
        }
        public float Speed => wd.speed * 3.6f;
        public float Angle => wd.angle * Mathf.Deg2Rad;

        public void CarPoseReset()
        {
            wd.SetVehiclePos(objAttbutes.TransformData.V3Pos.GetVector3(), Quaternion.Euler(objAttbutes.TransformData.V3Rot.GetVector3()));
            LinearVelocity = 0;
            SteeringAngle = 0;
        }


        public float lastSpeed;
        public float accelerate;
        public enum ControlMode
        {
            Accelerate = 0,
            Decelerate = 1,
            KeepSpeed = 2,
            Stop = 3,
            Brake = 4,
        }
        public ControlMode driveMode;
        public float aimSpeed;
        public float throttle;

        public bool isBackUp; 
        public float addStep = 0.005f;
        public float keepStep = 0.0001f;

        public void SpeedCalculate()
        {
            isBackUp = aimSpeed < 0;
            float _speed = Mathf.Abs(wd.speed);
            float _aimSpeed = Mathf.Abs(aimSpeed);
            if (_aimSpeed == 0 || aimSpeed * wd.speed < 0) driveMode = ControlMode.Brake;
            else if (_speed < _aimSpeed * 0.9f) driveMode = ControlMode.Accelerate;
            else if (_speed > _aimSpeed * 1.1f) driveMode = ControlMode.Stop;
            else if (_speed > _aimSpeed * 1f) driveMode = ControlMode.Decelerate;
            else driveMode = ControlMode.KeepSpeed;
            switch (driveMode)
            {
                case ControlMode.Accelerate:
                    if (throttle < 0) throttle = 0;
                    throttle += addStep;
                    break;
                case ControlMode.Stop:
                    if (throttle > 0) throttle = 0;
                    throttle -= addStep;
                    break;
                case ControlMode.Decelerate:
                    throttle -= addStep;
                    break;
                case ControlMode.KeepSpeed:
                    if (_speed < _aimSpeed * 0.95f && accelerate < 0) throttle += keepStep * _aimSpeed;
                    break;
                case ControlMode.Brake:
                    throttle = 0;
                    break;
            }
            throttle = Mathf.Clamp(throttle, -1, 1);
            if (driveMode == ControlMode.Brake)
            {
                wd.throttle = 0;
                wd.brake = 1;
            }
            else if (driveMode == ControlMode.Decelerate)
            {
                wd.throttle = 0;
                wd.brake = 0;
            }
            else
            {
                if (throttle < 0)
                {
                    wd.throttle = -throttle;
                    wd.brake = 0;
                }
                else
                {
                    wd.throttle = isBackUp ? -throttle : throttle;
                    wd.brake = 0;
                }
            }
        }

        public override void ElementReset()
        {
            base.ElementReset();
            CarPoseReset();
        }
    }
}
