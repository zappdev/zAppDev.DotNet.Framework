using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zAppDev.DotNet.Framework.Data.Domain;

namespace zAppDev.DotNet.Framework.Identity.Model
{
    public class TokenInfo
    {
        public string Token { get; set; }
        public DateTime ExpiresIn { get; set; }

    }
}
