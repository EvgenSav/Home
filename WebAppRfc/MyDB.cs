using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using System.IO;

namespace RFController {
    [Serializable]
    public class MyDB<TKey, TValue> where TKey : struct /*where TValue : class*/ {
        public SortedDictionary<TKey, TValue> Data;

        public MyDB() {
            //Data = new SortedDictionary<TKey, List<TValue>>();
            Data = new SortedDictionary<TKey, TValue>();
        }

        public int SaveToFile(string path) {
            StreamWriter s1 = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.ReadWrite));
            JsonSerializerSettings set1 = new JsonSerializerSettings {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto
            };
            string serData = JsonConvert.SerializeObject(this,set1);
            s1.Write(serData);
            s1.Close();
            return 1;
        }

        public static MyDB<TKey,TValue> OpenFile(string path) {
            string res;
            MyDB<TKey, TValue> data;
            try {
                using (StreamReader s1 = new StreamReader(new FileStream(path, FileMode.Open))) {
                    res = s1.ReadToEnd();
                    JsonSerializerSettings set1 = new JsonSerializerSettings {
                        Formatting = Formatting.Indented,
                        TypeNameHandling = TypeNameHandling.Auto
                    };
                    data = JsonConvert.DeserializeObject<MyDB<TKey, TValue>>(res, set1);
                }
            } catch {
                data = new MyDB<TKey, TValue>();
            }
            return data;
        }
    }
    public interface ILogItem {
        DateTime CurrentTime { get; set; }
        int Cmd { get; set; }
    }

    [Serializable]
    public class LogItem:ILogItem {
        public DateTime CurrentTime { get;  set; }
        public int Cmd { get;  set; }
        public LogItem(DateTime dt, int cmd) {
            CurrentTime = dt;
            Cmd = cmd;
        }
    }

    [Serializable]
    public class PuLogItem : LogItem {
        public int State { get; set; }
        public int Bright { get; set; }

        public PuLogItem(DateTime dt, int cmd, int state, int bright) : base(dt, cmd) {
            State = state;
            Bright = bright;
        }
    }

    [Serializable]
    public class SensLogItem : LogItem {
        public float SensVal { get; set; }
        public SensLogItem(DateTime dt, int cmd, float val):base(dt,cmd) {
            SensVal = val;
        }
        public override string ToString() {
            return String.Format("{0:#.##} {1}C", SensVal, (char)176);
        }
    }
}
