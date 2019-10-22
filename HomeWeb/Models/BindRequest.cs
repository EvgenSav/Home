﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Web.Models
{
    public enum BindRequestStepEnum
    {
        Created,
        Pending,
        Completed,
        Error
    }
    public class BindRequest : DatabaseModel
    {
        public int? DeviceFk { get; set; }
        public DateTime? BindTime { get; set; }
        public DeviceTypeEnum Type { get; set; }
        public string Name { get; set; }
        public BindRequestStepEnum Step { get; set; } = BindRequestStepEnum.Created;
    }
}
