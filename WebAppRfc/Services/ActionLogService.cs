using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Db;
using WebAppRfc.Models;

namespace WebAppRfc.Services {
    public class ActionLogService {
        private MyDB<int, List<ILogItem>> ActionLogBase;

        public SortedDictionary<int, List<ILogItem>> ActionLog => ActionLogBase.Data;
        public void SaveToFile(string path) => ActionLogBase.SaveToFile(path);
        public ActionLogService() {
            ActionLogBase = MyDB<int, List<ILogItem>>.OpenFile("log.json");
        }
    }
}
