using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace DataForLife
{
    public class GameJson
    {
        public int Turn;

        public int ID;

        public int Row;
        public int Col;

        public int TresureRow;
        public int TresureCol;



        public int InintRow;
        public int InintCol;

        public bool HasTresure;
        public bool IsShoutDown;
        public bool IsUsingDefence;
        public bool DidShoot;

    }
    public class DataSending
    {
        public void SendData(GameJson gj)
        {

            GameJson a = new GameJson();
            a.ID = 2;
            a.HasTresure = true;


            string Data = JsonConvert.SerializeObject(a);
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(@"https://blinding-torch-5242.firebaseio.com/GameData.json");

            httpWebRequest.ContentType = "text/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {


                streamWriter.Write(Data);
                streamWriter.Flush();
                streamWriter.Close();
            }
        }

    }
}
