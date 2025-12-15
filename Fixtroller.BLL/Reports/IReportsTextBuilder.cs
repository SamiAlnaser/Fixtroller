using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Reports
{
    public interface IReportsTextBuilder
    {
        string Get(
            string key,
            string language,
            params object[] args);
    }
}
