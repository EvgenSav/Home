using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Web.Domain
{
    public interface IAutomationProcessorService : IDisposable
    {
        void Initialize();
    }
}
