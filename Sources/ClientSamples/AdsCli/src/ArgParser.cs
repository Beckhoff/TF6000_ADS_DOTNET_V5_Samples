using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwinCAT.Ads;

namespace TwinCAT.Ads.Cli
{
    public static class ArgParser
    {
        /// <summary>
        /// Parses the arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>AmsAddress.</returns>
        public static AmsAddress Parse(string[] args)
        {
            AmsNetId netId = AmsNetId.Local;
            int port = 851;

            if (args != null)
            {
                if (args.Length > 0 && args[0] != null)
                    netId = AmsNetId.Parse(args[0]);

                if (args.Length > 1 && args[1] != null)
                    port = int.Parse(args[1]);
            }
            return new AmsAddress(netId, port);
        }
    }
}
