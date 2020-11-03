using Assets.Scripts.simai;
using Assets.Scripts.SimuUI;
//using AutoCore.Sim.Vehicle.Control;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.simController
{
    [RequireComponent(typeof(WheelDrive))]
    public class EgoVehicle : UnitySingleton<EgoVehicle>
    {
        private EgoVehicleController egoController;
        public EgoVehicleController EgoController
        {
            get
            {
                if (egoController == null)
                {
                    egoController= GetComponent<EgoVehicleController>();
                }
                if (egoController == null)
                {
                    egoController = gameObject.AddComponent<EgoVehicleController>(); ;
                }
                return EgoController;
            }
        }

        private WheelDrive wd;
        public WheelDrive WD
        {
            get
            {
                if (wd == null) wd = GetComponent<WheelDrive>();
                return wd;
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
            set => accelerate = value;
        }
        public float SteeringAngle
        {
            get => WD.steer;
            set
            {
                if (IsHandDrive) WD.steer = value;
            }
        }
        public float Speed => WD.speed * 3.6f;
        public float Angle => WD.angle * Mathf.Deg2Rad;

        public void MaxAngle(float value) 
        {
            wd.maxAngle = value;
        }

        private bool IsHandDrive
        {
            get
            {
                return WD.isHandDrive;
            }
            set
            {
                WD.isHandDrive = value;
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

        private void Awake()
        {
        }
        void Update()
        {
            if (PanelCarMessage.Instance != null)
            {
                PanelCarMessage.Instance.UpdateCarmessage(WD.steer, WD.str_Odom, WD.brake, WD.throttle, wd.speed, LinearVelocity);
            }
            if (IsHandDrive)
            {
                WD.steer = Input.GetAxis("Horizontal");
                WD.throttle = Mathf.Abs(WD.speed) < maxSpeed/3.6f ? Input.GetAxis("Vertical") : 0;
                WD.brake = Input.GetKey(KeyCode.X) ? 1 : 0;
            }
        }
        void FixedUpdate()
        {
            accelerate = (WD.speed - lastSpeed) / 0.02f;
            lastSpeed = WD.speed;
            if (!IsHandDrive)
            {
                SpeedCalculate();
            }
        }

        public void ResetCar()
        {
            WD.SetVehiclePos(TestConfig.TestMode.TestCarStart.V3Pos.GetVector3(), Quaternion.Euler(TestConfig.TestMode.TestCarStart.V3Rot.GetVector3()));
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
            float _speed = Mathf.Abs(WD.speed);
            float _aimSpeed = Mathf.Abs(aimSpeed);
            if (_aimSpeed == 0 || aimSpeed * WD.speed < 0) driveMode = ControlMode.Brake;
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
                WD.throttle = 0;
                WD.brake = 1;
            }
            else if (driveMode == ControlMode.Decelerate)
            {
                WD.throttle = 0;
                WD.brake = 0;
            }
            else
            {
                if (throttle < 0)
                {
                    WD.throttle = -throttle;
                    WD.brake = 0;
                }
                else
                {
                    WD.throttle = isBackUp ? -throttle : throttle;
                    WD.brake = 0;
                }
            }
        }
    }

}
