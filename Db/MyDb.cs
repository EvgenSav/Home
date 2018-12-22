using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using System.IO;

namespace Db {
    [Serializable]
    public class MyDb<TKey, TValue> where TKey : struct /*where TValue : class*/ {
        public SortedDictionary<TKey, TValue> Data;

        public MyDb() {
            Data = new SortedDictionary<TKey, TValue>();
        }

        public int SaveToFile(string path) {
            StreamWriter s1 = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.ReadWrite));
            JsonSerializerSettings set1 = new JsonSerializerSettings {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto
            };
            string serData = JsonConvert.SerializeObject(this, set1);
            s1.Write(serData);
            s1.Close();
            return 1;
        }

        public static MyDb<TKey, TValue> OpenFile(string path) {
            string res;
            MyDb<TKey, TValue> data;
            try {
                using (var s1 = new StreamReader(File.Open(path, FileMode.OpenOrCreate))) {
                    res = s1.ReadToEnd();
                    JsonSerializerSettings set1 = new JsonSerializerSettings {
                        Formatting = Formatting.Indented,
                        TypeNameHandling = TypeNameHandling.Auto
                    };
                    data = JsonConvert.DeserializeObject<MyDb<TKey, TValue>>(res, set1);
                }
            } catch(Exception e) {
                throw new Exception(e.Message);
                //data = new MyDb<TKey, TValue>();
            }
            return data;
        }
    }
}
