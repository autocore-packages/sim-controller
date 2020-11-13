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
    public class DataManager:UnitySingleton<DataManager>
    {
        public string testModeName;
        const string timeFormat = "yyyy-MM-dd-HH-mm-ss";
        public void TDMInit()
        {
            testModeName = TestConfig.TestMode.TestModeName;
            dataFilePath = Path.Combine(Application.streamingAssetsPath, "TestData", DateTime.Now.ToString(timeFormat) + ".txt");
            WriteTestData(DateTime.Now.ToString(timeFormat) +"TestStart");
        }
        public void AddTestMode(string modename,string mapname)
        {
            testModeName = modename;
            TestConfig.testMap = (TestConfig.TestMap)Enum.Parse(typeof(TestConfig.TestMap), mapname);
            WriteTestJson(true);
        }

        public void WriteTestJson(bool isNew = false)
        {
            SimuTestMode td = new SimuTestMode
            {
                TestModeName = testModeName,
                MapName = TestConfig.testMap.ToString(),
                LastTime = DateTime.Now,
                //VoyageTestConfig = VoyageTestManager.Instance.GetVoyageTestConfig()
            };
            if (isNew)
            {
                td.TestCarStart = new TransformData(new Vec3(-200.0f, 0.0f, -4.5f), new Vec3(0.0f, 90.0f, 0.0f), new Vec3(1f, 1f, 1f));
            } 
            else
            {
                td.TestCarStart = new TransformData(EgoVehicle.Instance.transform);
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
            }
            string content = JsonConvert.SerializeObject(td);
            WriteByLineCover(Path.Combine(Application.streamingAssetsPath ,"TestConfigs" , td.TestModeName + ".json"), content);
            if (MainUI.Instance != null)
            {
               PanelOther.Instance.SetTipText("Mode Save OK");
            }
            TestConfig.TestMode = td;
        }
        static FileStream fStream;
        static StreamWriter sw;

        private string dataFilePath;
        public void WriteTestData( string content)
        {
            var dic = Path.GetDirectoryName(dataFilePath);
            if (!Directory.Exists(dic))
            {
                Directory.CreateDirectory(dic);
            }
            if (!File.Exists(dataFilePath))
            {
                File.CreateText(dataFilePath).Dispose();
            }
            WriteFileByLine(dataFilePath, DateTime.Now.ToString(timeFormat) + " " + content);
        }

        public void WriteFileByLine(string strPath, string value)
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
        static string testConfigPath;
        public void WriteByLineCover(string strPath, string content)
        {
            testConfigPath = strPath;
            fStream = File.Open(testConfigPath, FileMode.OpenOrCreate, FileAccess.Write);
            fStream.Seek(0, SeekOrigin.Begin);
            fStream.SetLength(0);
            fStream.Close();
            WriteFileByLine(testConfigPath, content);
        }
        private void OnDestroy()
        {
            if (sw != null) sw.Dispose();
        }
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
    }
}
