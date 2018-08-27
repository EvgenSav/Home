using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAppRfc.Models;

namespace WebAppRfc.Logics
{
    public static class Status
    {
        private static List<BindStatus> statuses = new List<BindStatus> {
            new BindStatus {
                StatusId = 1,
                StatusMsg = "Bind started OK"
            },
            new BindStatus {
                StatusId = 2,
                StatusMsg = "Bind received OK"
            },
            new BindStatus {
                StatusId = 3,
                StatusMsg = "Bind not received"
            },
            new BindStatus {
                StatusId = 4,
                StatusMsg = "Bind not received"
            },
            new BindStatus {
                StatusId = 4,
                StatusMsg = "Bind not received"
            }
        };

        public static BindStatus BindStarted {
            get {
                return statuses[0];
            }
        }
        public static BindStatus BindSent {
            get {
                return statuses[1];
            }
        }
        public static BindStatus AddOk {
            get {
                return statuses[1];
            }
        }
        public static BindStatus BindFail {
            get {
                return statuses[1];
            }
        }
    }
}
