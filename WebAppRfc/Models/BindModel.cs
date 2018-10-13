using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAppRfc.RF;

namespace WebAppRfc.Models
{
    public enum BindStatus {
        BindStartOk = 1,
        BindStartFail = 2,
        BindStartMemoryFull = 3,
        BindReceiveOk = 4,
        BindReceiveFail = 5,
        BindNotReceived = 6,
        AddOk = 7,
        AddFail = 8,
        AddFailAlreadyExists = 9,
        CancelOk = 10
    }

    public class BindModel {
        public BindModel(RfDevice devicce, BindStatus status) {
            Device = devicce;
            Status = status;
        }
        public RfDevice Device { get; set; }
        public BindStatus Status { get; set; }
    }
}
