﻿using Assets.Scripts.simai;
using Assets.Scripts.SimuUI;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.simController
{
    public class TestManager :MonoBehaviour
    {

        public static TestManager Instance { get; private set; }

        public TestDataManager DataManager
        {
            get;
            private set;
        }
        
        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log(" has been Init");
            }
            else
            {
                Destroy(gameObject);
            }
            if (DataManager == null)
            {
                GameObject go = new GameObject( "DataManager");
                DataManager = go.AddComponent<TestDataManager>();
                go.transform.SetParent(transform);
            }
        }
        public bool isRepeat = false;
        public bool isROSControl = true;
        public enum EditMode
        {
            Null = 0,
            SetCarPose = 1,
            SetStatic = 2,
            SetPed = 3,
            SetNPC = 4,
            SetCheckPoint = 5
        }
        public EditMode editMode;
        private int indexMode = 0;
        public ObjNPC NPC
        {
            get
            {
                if (ElementsManager.Instance.SelectedElement != null&& ElementsManager.Instance.SelectedElement is ObjNPC obj)
                {
                        return obj;
                }
                return null;
            }
        }
        public PedestrainController Pedestrian
        {
            get
            {
                if (ElementsManager.Instance.SelectedElement != null && ElementsManager.Instance.SelectedElement is PedestrainController obj)
                {
                    return obj;
                }
                return null;
            }
        }
        public ObjTrafficLight TL
        {
            get
            {
                if (ElementsManager.Instance.SelectedElement != null && ElementsManager.Instance.SelectedElement is ObjTrafficLight obj)
                {
                    return obj;
                }
                return null;
            }
        }
        public void OnEnterSim()
        {
            DataManager.Init();
            DataManager.LoadRoadData("RoadData/" + TestConfig.testMap.ToString());
            PanelInspector.Instance.button_AddPos.onClick.AddListener(() =>
            {
                SetHumanPoses();
            });
            PanelInspector.Instance.button_changeLeft.onClick.AddListener(() =>
            {
                if (NPC.CanChangeLaneLeft()) NPC.ChangeLane();
            });
            PanelInspector.Instance.button_changeRight.onClick.AddListener(() =>
            {
                if (NPC.CanChangeLaneRight()) NPC.ChangeLane();
            });
            PanelInspector.Instance.button_SwitchLight.onClick.AddListener(() =>
            {
                TL.SwitchLight();
            });
            PanelInspector.Instance.button_SetAim.onClick.AddListener(() =>
            {
                SetEditMode(EditMode.SetNPC, 3);
            });
            PanelInspector.Instance.btn_DeleteObj.onClick.AddListener(() =>
            {
                ElementsManager.Instance.RemoveSelectedElement();
            });
            PanelInspector.Instance.OnSwitchLight += () =>
            {
                var traffic = TL;
                if (traffic != null)
                {
                    traffic.SwitchLight();
                }
            };
            OverLookCameraController.Instance.IsFollowTargetPos = PanelSettings.Instance.toggle_FollowCarPos.isOn;
            OverLookCameraController.Instance.IsFollowTargetRot = PanelSettings.Instance.toggle_FollowCarRot.isOn;

            PanelTools.Instance.button_resetAll.onClick.AddListener(() =>
            {
                ElementsManager.Instance.RemoveAllElements();
                PanelTools.Instance.CloseAllMenu();
            });
            PanelTools.Instance.button_resetEgo.onClick.AddListener(() =>
            {
                ElementsManager.Instance.CurrentEgo.ElementReset();
                PanelTools.Instance.CloseAllMenu();
            });
            PanelTools.Instance.button_addNPC.onClick.AddListener(() =>
            {
                SetAddCarAI();
                PanelTools.Instance.CloseAllMenu();
            });
            PanelTools.Instance.button_addPed.onClick.AddListener(() =>
            {
                SetAddPed();
                PanelTools.Instance.CloseAllMenu();
            });
            PanelTools.Instance.button_addObs.onClick.AddListener(() =>
            {
                SetAddObstacle();
                PanelTools.Instance.CloseAllMenu();
            });
            PanelTools.Instance.menuButtons[2].m_button.onClick.AddListener(() =>
            {
                SetCarSetMode();
            });
            PanelTools.Instance.menuButtons[2].m_button.onClick.AddListener(() =>
            {
                PanelTools.Instance.OpenGitURL();
            });

            PanelCamera.Instance.btn_SwitchCamera.onClick.AddListener(() => { OverLookCameraController.Instance.SwitchCamera(); });

        }
        public void SetEditMode(EditMode mode, int index = 0)
        {
            editMode = mode;
            indexMode = index;
        }
        public void SetCarSetMode()
        {
            SetEditMode(EditMode.SetCarPose);
        }
        public void SetAddCarAI()
        {
            SetEditMode(EditMode.SetNPC);
        }
        public void SetAddPed()
        {
            SetEditMode(EditMode.SetPed);
        }
        public void SetHumanPoses()
        {
            SetEditMode(EditMode.SetPed, 2);
        }
        public void SetAddObstacle()
        {
            SetEditMode(EditMode.SetStatic);
        }
        public void SetAddCheckPoint()
        {
            SetEditMode(EditMode.SetCheckPoint);
        }
        public Vector3 mousePos;
        ElementObject elementObject;
        private void Update()
        {
            if (OverLookCameraController.Instance == null) return;
            mousePos = OverLookCameraController.Instance.MouseWorldPos;
            Vector3 offset = OverLookCameraController.Instance.MouseWorldPos - ElementsManager.Instance.CurrentEgo.transform.position;
            float dis2Front = Mathf.Abs(Vector3.Dot(offset, ElementsManager.Instance.CurrentEgo.transform.forward));
            float dis2Right = Mathf.Abs(Vector3.Dot(offset, ElementsManager.Instance.CurrentEgo.transform.right));
            PanelOther.Instance.ShowMouseDis2Car(OverLookCameraController.Instance.isCarCameraMain, dis2Front, dis2Right);
            switch (editMode)
            {
                case EditMode.Null:
                    indexMode = 0;
                    break;
                case EditMode.SetCarPose:
                    switch (indexMode)
                    {
                        case 0:
                            PanelOther.Instance.SetTipText("Click to set vehicle position，right click to cancel");
                            indexMode = 1;
                            break;
                        case 1:
                            if (Input.GetMouseButtonDown(0))
                            {
                                PanelOther.Instance.SetTipText("Click to set vehicle orientation");
                                ElementsManager.Instance.CurrentEgo.transform.position = mousePos + Vector3.up * 0.5f;
                                DataManager.WriteTestData("Set ego vehicle position:" + mousePos);
                                indexMode = 2;
                            }
                            else if (MouseInputBase.Button1Down)
                            {
                                editMode = EditMode.Null;
                            }
                            break;
                        case 2:
                            if (Vector3.Distance(ElementsManager.Instance.CurrentEgo.transform.position, mousePos) > 1)
                                ElementsManager.Instance.CurrentEgo.transform.LookAt(mousePos, Vector3.up);
                            if (Input.GetMouseButtonDown(0))
                            {
                                editMode = EditMode.Null;
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case EditMode.SetStatic:
                    switch (indexMode)
                    {
                        case 0:
                            PanelOther.Instance.SetTipText("Click to set obstacle position，left ctrl+ mouse wheel to set obstacle size，right click to cancel");
                            elementObject = ElementsManager.Instance.AddObstacle();
                            elementObject.transform.position = mousePos;
                            ElementsManager.Instance.SelectedElement = elementObject;
                            indexMode = 1;
                            break;
                        case 1:
                            if (MouseInputBase.Button0Down)
                            {
                                DataManager.WriteTestData("Set Static Obstacle,Position:" + mousePos + "Scale:" + elementObject.transform.localScale.x);
                                editMode = EditMode.Null;
                            }
                            else if (MouseInputBase.Button1Down)
                            {
                                ElementsManager.Instance.RemoveSelectedElement();
                                editMode = EditMode.Null;
                            }
                            elementObject.transform.rotation = Quaternion.identity;
                            ElementsManager.Instance.FollowMouse(elementObject);
                            break;
                        default:
                            break;
                    }
                    break;
                case EditMode.SetPed:
                    switch (indexMode)
                    {
                        case 0:
                            PanelOther.Instance.SetTipText("Click to set pedestrian starting position");
                            indexMode = 1;
                            break;
                        case 1:
                            if (MouseInputBase.Button0Down)
                            {
                                PanelOther.Instance.SetTipText("Click to add target position for pedestrian, right click to cancel");
                                elementObject = ElementsManager.Instance.SelectedElement = ElementsManager.Instance.AddPedestrian();
                                elementObject.transform.position = mousePos;
                                Debug.Log(elementObject.transform.position);
                                Pedestrian.AddPedPos(mousePos);
                                DataManager.WriteTestData("Set Human,Position:" + mousePos.ToString());
                                indexMode = 2;
                            }
                            else if (MouseInputBase.Button1Down)
                            {
                                editMode = EditMode.Null;
                            }
                            break;
                        case 2:
                            Debug.Log(elementObject.m_transform.position);
                            ElementsManager.Instance.isShowLine = true;
                            ElementsManager.Instance.LinePoses = new Vector3[Pedestrian.PosList.Count + 1];
                            Pedestrian.PosList.CopyTo(ElementsManager.Instance.LinePoses);
                            ElementsManager.Instance.LinePoses[ElementsManager.Instance.LinePoses.Length - 1] = mousePos;
                            if (MouseInputBase.Button0Down)
                            {
                                if (Pedestrian.PosList.Count > 10)
                                {
                                    PanelOther.Instance.SetTipText("Can't set target positon more than 10");
                                }
                                else
                                {
                                    Pedestrian.AddPedPos(mousePos);
                                    PanelInspector.Instance.OnChangeElement(ElementsManager.Instance.SelectedElement);
                                    PanelOther.Instance.SetTipText("Click to add target position for pedestrian, right click to cancel");
                                }
                            }
                            else if (MouseInputBase.Button1Down)
                            {
                                editMode = EditMode.Null;
                                ElementsManager.Instance.isShowLine = false;
                                PanelOther.Instance.SetTipText("cancelled");
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case EditMode.SetNPC:
                    switch (indexMode)
                    {
                        case 0:
                            PanelOther.Instance.SetTipText("Click to set AI vehicle init position");
                            indexMode = 1;
                            break;
                        case 1:
                            if (Input.GetMouseButtonDown(0))
                            {
                                ElementsManager.Instance.SelectedElement = ElementsManager.Instance.AddNPC();
                                NPC.transform.position = NPC.posInit = mousePos;
                                PanelOther.Instance.SetTipText("Click to set AI vehicle starting position");
                                indexMode = 2;
                            }
                            else if (Input.GetMouseButtonDown(1))
                            {
                                editMode = EditMode.Null;
                            }
                            break;
                        case 2:
                            ElementsManager.Instance.isShowLine = true;
                            ElementsManager.Instance.LinePoses = new Vector3[2] { NPC.transform.position, mousePos };
                            NPC.transform.LookAt(mousePos);
                            if (Input.GetMouseButtonDown(0))
                            {
                                LaneData laneTemp = ElementsManager.Instance.SearchNearestPos2Lane(out int index, mousePos);
                                Vector3 posStart = laneTemp.List_pos[index].GetVector3();
                                ElementsManager.Instance.isShowLine = false;
                                NPC.posStart = posStart;
                                NPC.speedObjTarget = 5;
                                PanelInspector.Instance.OnChangeElement(ElementsManager.Instance.SelectedElement);
                                NPC.UpdateElement2Attributes();
                                NPC.NPCInit();
                                NPC.isCarDrive = true;
                                DataManager.WriteTestData("Set AI vehicle Init Position:" + NPC.posInit + "Start Position:" + posStart);
                                editMode = EditMode.Null;
                            }
                            else if (Input.GetMouseButtonDown(1))
                            {
                                ElementsManager.Instance.isShowLine = false;
                                Destroy(NPC.gameObject);
                                editMode = EditMode.Null;
                            }
                            break;
                        case 3:
                            PanelOther.Instance.SetTipText("Click to set AI vehicle target position");
                            indexMode = 4;
                            break;
                        case 4:
                            if (Input.GetMouseButtonDown(0))
                            {
                                PanelOther.Instance.SetTipText("AI vehicle settled");
                                NPC.SetTarget(mousePos);
                                editMode = EditMode.Null;
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case EditMode.SetCheckPoint:
                    switch (indexMode)
                    {
                        case 0:
                            PanelOther.Instance.SetTipText("Click to set checkpoint position，left ctrl+ mouse wheel to set checkpoint size，right click to cancel");
                            ElementsManager.Instance.SelectedElement = elementObject = ElementsManager.Instance.AddCheckPoint(0);
                            indexMode = 1;
                            break;
                        case 1:
                            if (MouseInputBase.Button0Down)
                            {
                                if (ElementsManager.Instance.CheckPointList.Count == 1)
                                {
                                    //CPController.Instance.SwitchCheckPoint();
                                }
                                DataManager.WriteTestData("Set CheckPoint,Position:" + mousePos + "Scale:" + elementObject.transform.localScale.x);
                                editMode = EditMode.Null;
                            }
                            else if (MouseInputBase.Button1Down)
                            {
                                ElementsManager.Instance.RemoveSelectedElement();
                                editMode = EditMode.Null;
                            }
                            elementObject.transform.rotation = Quaternion.identity;
                            ElementsManager.Instance.FollowMouse(elementObject);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
