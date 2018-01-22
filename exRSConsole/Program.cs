using DryIoc;
using exRS;
using System;
using System.Linq;

namespace exRSConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var arguments = new Arguments();

            if (args.Any(x => x == "-d"))
            {
                arguments.Delete = true;
            }
            else if (args.Any(x => x == "-b"))
            {
                arguments.Backup = true;
            }
            else
            {
                var folderNameIndex = Array.IndexOf(args, "-p");
                arguments.FolderName = args[folderNameIndex + 1];
                var reportNameIndex = Array.IndexOf(args, "-n");
                arguments.ReportName = args[reportNameIndex + 1];

                if (args.Any(x => x == "-r"))
                {
                    arguments.Report = true;
                }
                if (args.Any(x => x == "-s"))
                {
                    arguments.Subscription = true;
                }
                if (args.Any(x => x == "-f"))
                {
                    arguments.Recreate = true;
                }
            }

            Module.Install(new Container()).Resolve<IManager>().Execute(arguments);
        }
    }
}
