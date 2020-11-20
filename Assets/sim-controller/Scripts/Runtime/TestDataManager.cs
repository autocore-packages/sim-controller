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


using Assets.Scripts.simai;
using Assets.Scripts.SimuUI;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.simController
{
    public class TestDataManager :MonoBehaviour
    {
        public string testModeName;
        const string timeFormat = "yyyy-MM-dd-HH-mm-ss";
        public void Init()
        {
            testModeName = TestConfig.TestMode.TestModeName;
            dataFilePath = Path.Combine(Application.streamingAssetsPath, "TestData", DateTime.Now.ToString(timeFormat) + ".txt");
            WriteTestData(DateTime.Now.ToString(timeFormat) + "TestStart");
        }
        public void AddTestMode(string modename, string mapname)
        {
            testModeName = modename;
            TestConfig.testMap = (TestConfig.TestMap)Enum.Parse(typeof(TestConfig.TestMap), mapname);
            WriteTestJson();
        }
        public void LoadRoadData(string path)
        {
            TextAsset textAsset = Resources.Load(path) as TextAsset;
            if (textAsset == null)
            {
                Debug.Log("file failed");
            }
            else
            {
                ElementsManager.Instance.RoadsData = JsonConvert.DeserializeObject<RoadsData>(textAsset.text);
            }
        }

        public void WriteTestJson()
        {
            SimuTestMode td = new SimuTestMode
            {
                TestModeName = testModeName,
                MapName = TestConfig.testMap.ToString(),
                LastTime = DateTime.Now
            };
            if(EgoVehicle.Instance.transform!=null) td.TestCarStart = new TransformData(EgoVehicle.Instance.transform);
            foreach (ElementObject item in ElementsManager.Instance.checkPointManager.CheckPointList)
            {
                td.CheckPointAtts.Add(item.GetObjAttbutes());
            }
            foreach (ElementObject item in ElementsManager.Instance.obstacleManager.ObstacleList)
            {
                td.ObstacleAtts.Add(item.GetObjAttbutes());
            }
            foreach (ElementObject item in ElementsManager.Instance.nPCManager.NPCList)
            {
                td.CarAIAtts.Add(item.GetObjAttbutes());
            }
            foreach (ElementObject item in ElementsManager.Instance.pedestrianManager.PedestrainList)
            {
                td.HumanAtts.Add(item.GetObjAttbutes());
            }
            foreach (ElementObject item in ElementsManager.Instance.trafficlightManager.TrafficLightList)
            {
                td.TrafficLightAtts.Add(item.GetObjAttbutes());
            }
            string content = JsonConvert.SerializeObject(td);
            WriteFile(Path.Combine(Application.streamingAssetsPath, "TestConfigs", td.TestModeName + ".json"), content, true);
            if (MainUI.Instance != null)
            {
                PanelOther.Instance.SetTipText("Mode Save OK");
            }
            TestConfig.TestMode = td;
        }
        private void CheckFileExist(string path)
        {
            string dic = Path.GetDirectoryName(path);
            if (!Directory.Exists(dic))
            {
                Directory.CreateDirectory(dic);
                Debug.Log("");
            }
            if (!File.Exists(path))
            {
                File.CreateText(path).Dispose();
            }
        }

        #region Write data
        private string dataFilePath;
        public void WriteTestData(string content)
        {
            WriteFile(dataFilePath, DateTime.Now.ToString(timeFormat) + " " + content);
        }


        public void WriteFile(string path, string content, bool isCover = false)
        {
            if (isCover)
            {
                File.WriteAllText(path, content);
            }
            else
            {
                File.AppendAllText(path, content + "\n");
            }
        }
        StreamWriter sw;
        private void WriteFileByLine(string strPath, string value)
        {
            try
            {
                sw = new StreamWriter(strPath, true);
                sw.WriteLine(value);
                sw.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region ScreenShot
        public void ScreenShot()
        {
            StartCoroutine(UploadPNG(Path.Combine(Application.streamingAssetsPath, "ScreenShot", DateTime.Now.ToString(timeFormat) + ".png")));
        }
        IEnumerator UploadPNG(string path)
        {
            yield return new WaitForEndOfFrame();
            int width = Screen.width;
            int height = Screen.height;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
        }
        #endregion
    }
}
