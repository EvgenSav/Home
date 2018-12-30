using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Home.Web.Services {
    public class HomeService {
        public List<string> Rooms;
        public string RoomsPath { get; private set; } =  "rooms.json"; 
        public HomeService() {
            Rooms = GetRooms();
        }
        List<string> GetRooms() {
            List<string> rooms;
            using (StreamReader s1 = new StreamReader(new FileStream(RoomsPath, FileMode.OpenOrCreate))) {
                string res = s1.ReadToEnd();

                try {
                    rooms = JsonConvert.DeserializeObject<List<string>>(res, new JsonSerializerSettings {
                        Formatting = Formatting.Indented
                    });
                } catch {
                    rooms = new List<string> { "All" };
                }
            }
            return rooms;
        }
    }
}
