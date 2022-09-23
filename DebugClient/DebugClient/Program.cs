using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.ComponentModel;

namespace DebugClient;

class Program
{
    static void Main(string[] args)
    {
        DebugClient client = new DebugClient();
        client.Start();
    }
}
