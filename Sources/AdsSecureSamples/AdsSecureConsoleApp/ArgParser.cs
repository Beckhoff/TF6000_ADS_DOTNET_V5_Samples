using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TwinCAT.Ads;

namespace AdsSecureConsoleApp
{
    public static class ArgParser
    {
        /// <summary>
        /// Parses the arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>AmsAddress.</returns>
        public static bool TryParse(string[] args, out AmsAddress address, out string ipOrHostName)
        {
            bool ret = false;
            AmsNetId netId = null;
            int port = -1;
            IPAddress ip;

            address = null;
            ipOrHostName = null;

            if (args != null && args.Length == 3)
            {
                ret = true;
                ret &= AmsNetId.TryParse(args[0], out netId);
                ret &= int.TryParse(args[1], out port);

                if (!string.IsNullOrEmpty(args[2]))
                {
                    if (IPAddress.TryParse(args[2], out ip))
                    {
                        ipOrHostName = ip.ToString();
                    }
                    else
                    {
                        ipOrHostName= args[2];
                    }

                }
                else
                    ret = false;

            }
            else
            {
                ret = false;
            }

            if (ret)
            {
                address = new AmsAddress(netId, port);
            }
            return ret;
        }
    }
}
