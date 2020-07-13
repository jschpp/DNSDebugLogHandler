using System;
using System.Management.Automation;
using System.IO;
using System.Text.RegularExpressions;


namespace ParseDNSDebugLog
{
    [Cmdlet(VerbsData.Import, "DNSDebugLog")]
    public class ImportDNSDebugLog : Cmdlet
    {
        [Parameter(Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, Mandatory = true)]
        public string Path { get; set; }

        private StreamReader file;

        protected override void BeginProcessing()
        {
            try
            {
                file = new StreamReader(Path);
            }
            catch (FileNotFoundException ex)
            {
                var errorRecord = new ErrorRecord(
                    ex,
                    "FileNotFound",
                    ErrorCategory.ObjectNotFound,
                    Path);
                WriteError(errorRecord);
            }
            catch (UnauthorizedAccessException ex)
            {
                var errorRecord = new ErrorRecord(
                    ex,
                    "UnauthorizedAccessError",
                    ErrorCategory.InvalidData,
                    Path);
                WriteError(errorRecord);
            }
            catch (IOException ioException)
            {
                var errorRecord = new ErrorRecord(
                    ioException,
                    "FileReadError",
                    ErrorCategory.ReadError,
                    Path);
                WriteError(errorRecord);
            }
        }

        protected override void ProcessRecord()
        {
            string line;
            Regex rgx = new Regex(@"^(?<date>([0-9]{1,2}.[0-9]{1,2}.[0-9]{2,4}|[0-9]{2,4}-[0-9]{2}-[0-9]{2})\s*[0-9: ]{7,8}\s*(PM|AM)?) ([0-9A-Z]{3,4} PACKET\s*[0-9A-Za-z]{8,16}) (UDP|TCP) (?<way>Snd|Rcv) (?<ip>[0-9.]{7,15}|[0-9a-f:]{3,50})\s*([0-9a-z]{4}) (?<QR>.) (?<OpCode>.) \[.*\] (?<QuestionType>.*?) (?<query>\(.*)");
            while ((line = file.ReadLine()) != null)
            {
                Match m = rgx.Match(line);
                if (m.Success)
                {
                    DNSLogEntry.DNSLogEntry entry = new DNSLogEntry.DNSLogEntry();
                    entry.ClientIP = m.Groups["ip"].Value.Trim();
                    DateTime.TryParse(m.Groups["date"].Value.Trim(), out DateTime dt);
                    entry.DateTime = dt;
                    entry.QR = DNSLogEntry.DNSLogEntry.ParseQR(m.Groups["QR"].Value);
                    entry.OpCode = DNSLogEntry.DNSLogEntry.ParseOpCode(m.Groups["OpCode"].Value);
                    entry.Way = m.Groups["way"].Value.Trim();
                    entry.QuestionType = m.Groups["QuestionType"].Value.Trim();
                    entry.Query = m.Groups["query"].Value.Trim();
                    try
                    {
                        WriteObject(entry);
                    } catch (System.Management.Automation.PipelineStoppedException) {
                        // This is needed if someone prematurely closes the pipe with CTRL-C or Select-Object -First
                        file.Dispose();
                        break;
                    }
                }
            }
        }

        protected override void EndProcessing()
        {
            if (file != null)
            {
                file.Dispose();
            }
        }
    }
}

 namespace DNSLogEntry { 
    public class DNSLogEntry
    {
        public string ClientIP { get; set; }
        public DateTime DateTime { get; set; }
        public string QR { get; set; }
        public string OpCode { get; set; }
        public string Way { get; set; }
        public string QuestionType { get; set; }
        public string Query { get; set; }

        public static string ParseQR ( string QR)
        {
            switch (QR.ToLower())
            {
                case " ": return "Query";
                case "R": return "Response";
            }
            return "ParseError";
        }

        public static string ParseOpCode(string OP)
        {
            switch (OP.Trim().ToLower())
            {
                case "q": return "Standard Query";
                case "n": return "Notify";
                case "u": return "Update";
                case "?": return "Unknown";
            }
            return "ParseError";
        }
    }
}
