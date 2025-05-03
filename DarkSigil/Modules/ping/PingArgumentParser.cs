using System;


namespace DarkSigil.Modules.ping
{
    public static class PingArgumentParser
    {
        public static PingOptionsModel Parse(string[] args)
        {
            var options = new PingOptionsModel();
            

            for (int i = 0; i < args.Length; i++) 
            {
                switch (args[i]) {

                    case "-a":
                        options.IsAudible = true;
                        break;

                    case "-c":
                        if(i + 1  < args.Length && int.TryParse(args[i + 1], out int count))
                            options.Count  = count; i++;
                        break;

                    case "-i":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out int interval))
                            options.Interval = interval; i++;
                        break;

                    case "-I":
                    case "-q":
                        if(i + 1 < args.Length)
                            options.InterfaceAddress = args[i + 1]; // ?
                        break;
                    case "-n":
                        options.IsNumeric = true;
                        break;
                    case "-t":
                        if(i + 1 < args.Length && int.TryParse(args[i + 1], out int ttl))
                            options.TTL = ttl; i++;
                        break;
                    case "-v":
                        options.IsVerbose = true;
                        break;
                    case "-w":
                        if(i+1 <  args.Length && int.TryParse(args[i + 1], out int deadline))
                            options.Deadline = deadline; i++; 
                        break;
                    case "-W":
                        if(i+1 < args.Length && int.TryParse(args[i+1], out int timeout))
                            options.Timeout = timeout; i++;
                        break;
                    case "--help":
                    case "-h":
                        options.IsHelp = true;
                        break;

                    default:
                        if (!args[i].StartsWith("-"))
                            options.Host = args[i];
                        break;
                }
            }
                return options;
        }
        
    }
}
