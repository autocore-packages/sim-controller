#region License
/*
* Copyright 2018 AutoCore
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*     http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
#endregion


using Assets.Scripts;
using System.Collections;
using UnityEngine;
using Assets.Scripts.SimuUI;
using Assets.Scripts.simai;
namespace Assets.Scripts.simController
{
    public class OverLookCameraController : UnitySingleton<OverLookCameraController>
    {
        private Vector3 PosTarget;
        public Camera oLCamera;
        public Ray rayPos;
        public Vector3 offset = Vector3.zero;
        public Vector3 Offset
        {
            get
            {
                if (isDrageCamera)
                {
                    if (GetWorldPos(mousePosDragStart, out Vector3 worldPosStart))
                    {
                        offset_temp = worldPosStart - MouseWorldPos;
                        return offset + offset_temp;
                    }
                    else
                    {
                        offset += offset_temp;
                        offset_temp = Vector3.zero;
                        isDrageCamera = false;
                    }
                }
                return offset;
            }
        }
        public Vector3 offset_temp;
        private float maxCameraSize = 200;

        private float _cameraRange = 20;
        public float CameraRange
        {
            get
            {
                return _cameraRange;
            }
            set
            {
                _cameraRange = Mathf.Clamp(value, 10f, maxCameraSize);
            }
        }
        public bool isFollowTargetPos = false;
        public bool isFollowTargetRot = false;
        private Vector3 mouseWorldPos;
        public Vector3 MouseWorldPos
        {
            get
            {
                if (GetWorldPos(Input.mousePosition, out Vector3 worldPos))
                {
                    mouseWorldPos = worldPos;
                }
                ElementsManager.Instance.MouseWorldPos = mouseWorldPos;
                return mouseWorldPos;
            }
        }
        public Vector3 mousePosDragStart;
        private bool isDrageCamera = false;
        private void Start()
        {
            oLCamera = transform.GetComponent<Camera>();
            transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
            mask = 1 << 12;
        }
        LayerMask mask;
        void Update()
        {
            KeyInputBase.Update();
            MouseInputBase.Update();

            if (MouseInputBase.Button1Down)
            {
                StartCoroutine(SetSelectedElememtNull());
            }
            if (MouseInputBase.MouseScroll.y < 0)
            {
                if (KeyInputBase.LeftCtrl && ElementsManager.Instance.SelectedElement != null)
                {
                    ElementsManager.Instance.SelectedElement.SetObjScale(0.9f);
                }
                else
                {
                    CameraRange *= 1.1f;
                }
            }
            else if (MouseInputBase.MouseScroll.y > 0)
            {
                if (KeyInputBase.LeftCtrl && ElementsManager.Instance.SelectedElement != null)
                {
                    ElementsManager.Instance.SelectedElement.SetObjScale(1.1f);
                }
                else
                {
                    CameraRange *= 0.9f;
                }
            }
            oLCamera.orthographicSize = CameraRange;
            if (MouseInputBase.Button2Down)
            {
                isDrageCamera = true;
                mousePosDragStart = Input.mousePosition;
            }
            else if (MouseInputBase.Button2Up)
            {
                offset += offset_temp;
                offset_temp = Vector3.zero;
                isDrageCamera = false;
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OLCameraReset();
            }
            else if (Input.GetKeyDown(KeyCode.Delete))
            {
                if (ElementsManager.Instance.SelectedElement != null && ElementsManager.Instance.SelectedElement.transform != EgoVehicle.Instance.transform)
                {
                    ElementsManager.Instance.RemoveSelectedElement();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                MainUI.Instance.CloseLastPanel();
            }
            if (KeyInputBase.LeftCtrl)
            {
                if (Input.GetKeyDown(KeyCode.A))
                {
                    EgoVehicle.Instance.SwitchDriveMode();
                }
            }
            if (isFollowTargetPos)
            {
                PosTarget = EgoVehicle.Instance.transform.position;
            }
            if (isFollowTargetRot) FollowTargetRot();
            transform.position = new Vector3(PosTarget.x + Offset.x, 50, PosTarget.z + Offset.z);
        }
        public void SetCameraFollowTargetRotate(bool value)
        {
            if (value)
            {
                isFollowTargetRot = true;
            }
            else
            {
                isFollowTargetRot = false;
                transform.rotation = Quaternion.Euler(90, 0, 0);
            }
        }
        public void SetCameraFollowTargetPosition(bool value)
        {
            isFollowTargetPos = value;
            if (value)
            {
                OLCameraReset();
            }
            PanelSettings.Instance.toggle_FollowCarPos.isOn = value;
        }
        private void FollowTargetRot()
        {
            Vector3 rot = EgoVehicle.Instance.transform.rotation.eulerAngles;
            rot.x = 90;
            transform.rotation = Quaternion.Euler(rot);
        }
        private bool GetWorldPos(Vector3 mousePos, out Vector3 worldPos)
        {
            rayPos = oLCamera.ScreenPointToRay(mousePos);
            if (Physics.Raycast(rayPos, out RaycastHit raycastHit, Mathf.Infinity, mask))
            {
                worldPos = raycastHit.point;
                return true;
            }
            else
            {
                worldPos = Vector3.zero;
                return false;
            }
        }
        private IEnumerator SetSelectedElememtNull()
        {
            yield return new WaitForEndOfFrame();
            ElementsManager.Instance.SelectedElement = null;
        }

        public bool isCarCameraMain = false;
        public RenderTexture texture_RightDown;
        public void SwitchCamera()
        {
            isCarCameraMain = !isCarCameraMain;
            if (isCarCameraMain)
            {
                CameraController.Instance.m_camera.targetTexture = null;
                Instance.oLCamera.targetTexture = texture_RightDown;
            }
            else
            {
                CameraController.Instance.m_camera.targetTexture = texture_RightDown;
                Instance.oLCamera.targetTexture = null;
            }
        }

        public void SetCameraRange(float value)
        {
            CameraRange = value;
        }
        public void OLCameraReset()
        {
            CameraRange = 20;
            offset = Vector3.zero;
            PosTarget = EgoVehicle.Instance.transform.position;
        }
    }

}

