using Assets.Scripts.simai;
using Assets.Scripts.SimuUI;
//using AutoCore.Sim.Vehicle.Control;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.simController
{
    [RequireComponent(typeof(WheelDrive))]
    [RequireComponent(typeof(EgoVehicleController))]
    public class EgoVehicle : UnitySingleton<EgoVehicle>
    {
        public EgoVehicleController egoController;

        public WheelDrive wd;
        public float LinearVelocity
        {
            get => aimSpeed;
            set => aimSpeed = value;
        }
        public float LinearAcceleration 
        {
            get => accelerate;
            set => accelerate = value;
        }
        public float SteeringAngle
        {
            get => wd.steer;
            set
            {
                if (IsHandDrive) wd.steer = value;
            }
        }
        public float Speed => wd.speed * 3.6f;
        public float Angle => wd.angle * Mathf.Deg2Rad;

        public void MaxAngle(float value) 
        {
            wd.maxAngle = value;
        }

        private bool IsHandDrive
        {
            get
            {
                return wd.isHandDrive;
            }
            set
            {
                wd.isHandDrive = value;
                PanelSimuMessage.Instance.SetControlModeText(value ? "KeyBoard" : "RosControl");
            }
        }
        public void SwitchDriveMode()
        {
            IsHandDrive = !IsHandDrive;
        }
        public void SetDriveMode(bool isHand=false)
        {
            IsHandDrive = isHand;
        }

        public float maxSpeed = 100;
        public void SetMaxSpeed(float value)
        {
            maxSpeed = value / 3.6f;
        }


        public float lastSpeed;
        public float accelerate;
        public float aimSteer;

        private void OnEnable()
        {
            egoController = GetComponent<EgoVehicleController>();
        }

        void Update()
        {
            if (PanelCarMessage.Instance != null)
            {
                PanelCarMessage.Instance.UpdateCarmessage(wd.steer, wd.str_Odom, wd.brake, wd.throttle, wd.speed, LinearVelocity);
            }
            if (IsHandDrive)
            {
                wd.steer = Input.GetAxis("Horizontal");
                wd.throttle = Mathf.Abs(wd.speed) < maxSpeed/3.6f ? Input.GetAxis("Vertical") : 0;
                wd.brake = Input.GetKey(KeyCode.X) ? 1 : 0;
            }
        }
        void FixedUpdate()
        {
            accelerate = (wd.speed - lastSpeed) / 0.02f;
            lastSpeed = wd.speed;
            if (!IsHandDrive)
            {
                SpeedCalculate();
            }
        }

        public void ResetCar()
        {
            wd.SetVehiclePos(TestConfig.TestMode.TestCarStart.V3Pos.GetVector3(), Quaternion.Euler(TestConfig.TestMode.TestCarStart.V3Rot.GetVector3()));
            LinearVelocity = 0;
            SteeringAngle = 0;
        }

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
    }

}
