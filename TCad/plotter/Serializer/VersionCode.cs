using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter.Serializer
{
    public class VersionCode
    {
        private byte[] Code_ = new byte[4];

        public byte[] Code
        {
            get { return Code_; }
        }

        public string Str
        {
            get => $"{Code_[0]}.{Code_[1]}.{Code_[2]}.{Code_[3]}";
        }

        public VersionCode(byte f0, byte f1, byte f2, byte f3 )
        {
            Code_[0] = f0; Code_[1] = f1; Code_[2] = f2; Code_[3] = f3;
        }
    }
}
