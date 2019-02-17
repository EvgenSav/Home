using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Db.Storage;
using Home.Web.Models;

namespace Home.Web.Services {
    public class ActionLogService {
        private MyDb<int, List<ILogItem>> ActionLogBase;

        public SortedDictionary<int, List<ILogItem>> ActionLog => ActionLogBase.Data;
        public async Task SaveToFile(string path) => await ActionLogBase.SaveToFile(path);
        public ActionLogService() {
            ActionLogBase = MyDb<int, List<ILogItem>>.OpenFile("log.json");
        }
    }
}
