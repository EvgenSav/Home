using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Db;
using HomeWeb.Models;

namespace HomeWeb.Services {
    public class ActionLogService {
        private MyDb<int, List<ILogItem>> ActionLogBase;

        public SortedDictionary<int, List<ILogItem>> ActionLog => ActionLogBase.Data;
        public void SaveToFile(string path) => ActionLogBase.SaveToFile(path);
        public ActionLogService() {
            ActionLogBase = MyDb<int, List<ILogItem>>.OpenFile("log.json");
        }
    }
}
