using MessagePack;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace TestApp
{

    internal class Program
    {
        static void test001()
        {
            string src1 = "x=abc123";
            string src2 = "x=abc123(456), ";
            //Regex WordPattern = new Regex(@"[@a-zA-Z_0-9]+[\(]*");

            //Regex WordPattern = new Regex(@"[@a-zA-Z_0-9]+");
            Regex WordPattern = new Regex(@"[@a-zA-Z_0-9\(\)]+");

            int cpos = 2;

            MatchCollection mc = WordPattern.Matches(src2);

            int replacePos;
            int replaceLen;
            string targetWord = "";

            foreach (Match m in mc)
            {
                if (cpos >= m.Index && cpos <= m.Index + m.Length)
                {
                    replacePos = m.Index;
                    replaceLen = m.Length;
                    targetWord = m.Value;

                    break;
                }
            }

            Console.WriteLine(targetWord);
        }

        static void Main(string[] args)
        {
            test001();
            Console.WriteLine("<<< END >>>");
            Console.ReadLine();
        }
    }
}
