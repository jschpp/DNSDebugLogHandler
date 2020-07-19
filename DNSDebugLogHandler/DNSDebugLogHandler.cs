using System;
using System.Management.Automation;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

/* This module is based on the excellent DNSLogModule https://github.com/virot/DNSLogModule by virot https://github.com/virot
 * It was ported with his permission.
 */

namespace DNSDebugLogHandler
{
    [Cmdlet(VerbsData.Import, "DNSDebugLog", ConfirmImpact = ConfirmImpact.None, HelpUri = "https://github.com/jschpp/DNSDebugLogHandler/blob/main/DNSDebugLogHandler/docs/Import-DNSDebugLog.md")]
    public class ImportDNSDebugLog : PSCmdlet, IDisposable
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, Mandatory = true)]
        [ValidateNotNullOrEmpty()]
        public string Path { get; set; }

        [Parameter(Mandatory = false)]
        public CultureInfo Culture { get; set; }

        private const string regexPattern = @"^
                                                                        # Date in multiple locales TODO: maybe use TryParse here and don't try regex
            (?<date>([0-9]{1,2}.[0-9]{1,2}.[0-9]{2,4}|[0-9]{2,4}-[0-9]{2}-[0-9]{2})\s*[0-9: ]{7,8}\s*(PM|AM)?)\s
            ([0-9A-Z]{3,4}\sPACKET\s*[0-9A-Za-z]{8,16})\s               # Packet information
            (?<prot>UDP|TCP)\s
            (?<way>Snd|Rcv)\s
            (?<ip>[0-9.]{7,15}|[0-9a-f:]{3,50})\s*([0-9a-z]{4})\s       # IP Address IPv4 or IPv6
            (?<QR>.)\s
            (?<OpCode>.)\s
            \[.*\]\s
            (?<QuestionType>.*)\s
            (?<Question>\(.*)
            ";

        private Regex rgx;

        private StreamReader file;

        protected override void BeginProcessing()
        {
            // Check for Culture 
            if (Culture == null)
            {
                Culture = System.Globalization.CultureInfo.CurrentCulture;
            }

            // always resolve path to handle PS specific paths and relative paths
            Path = GetUnresolvedProviderPathFromPSPath(Path);

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
                this.ThrowTerminatingError(errorRecord);
            }
            catch (UnauthorizedAccessException ex)
            {
                var errorRecord = new ErrorRecord(
                    ex,
                    "UnauthorizedAccessError",
                    ErrorCategory.InvalidData,
                    Path);
                this.ThrowTerminatingError(errorRecord);
            }
            catch (IOException ioException)
            {
                var errorRecord = new ErrorRecord(
                    ioException,
                    "FileReadError",
                    ErrorCategory.ReadError,
                    Path);
                this.ThrowTerminatingError(errorRecord);
            }

            rgx = new Regex(regexPattern, RegexOptions.IgnorePatternWhitespace);
        }

        protected override void ProcessRecord()
        {
            string line;
            while ((line = file.ReadLine()) != null)
            {
                Match m = rgx.Match(line);

                if (m.Success)
                {
                    DNSLogEntry entry = new DNSLogEntry
                    {
                        ClientIP = m.Groups["ip"].Value.Trim(),
                        DateTime = DateTime.TryParse(m.Groups["date"].Value.Trim(), Culture, DateTimeStyles.None, out DateTime dt) ? dt : DateTime.MinValue,
                        QR = DNSLogEntry.ParseQR(m.Groups["QR"].Value),
                        OpCode = DNSLogEntry.ParseOpCode(m.Groups["OpCode"].Value),
                        Way = m.Groups["way"].Value.Trim(),
                        Protocol = m.Groups["prot"].Value,
                        QuestionType = m.Groups["QuestionType"].Value.Trim(),
                        Question = m.Groups["Question"].Value.Trim()
                    };
                    WriteObject(entry);
                }
                else
                {
                    WriteDebug(string.Format("Could not parse row: <{0}>", line));
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing == true)
            {
                if (file != null)
                {
                    file.Dispose();
                }
            }
        }

        ~ImportDNSDebugLog()
        {
            Dispose(false);
        }
    }

    public class DNSLogEntry
    {
        public string ClientIP { get; set; }
        public DateTime DateTime { get; set; }
        public string QR { get; set; }
        public string OpCode { get; set; }
        public string Way { get; set; }
        public string Protocol { get; set; }
        public string QuestionType { get; set; }
        public string Question { get; set; }

        public static string ParseQR(string QR)
        {
            if (QR != null)
            {
                switch (QR.ToLower())
                {
                    case " " : return "Query";
                    case "R" : return "Response";
                    // This should never happen
                    default : return string.Format("ParseError <{0}>", QR.Trim());
                };
            }
            throw new ArgumentNullException(nameof(QR));
        }

        public static string ParseOpCode(string OP)
        {
            if (OP != null)
            {
                switch (OP.Trim().ToLower())
                {
                    case "q" : return "Standard Query";
                    case "n" : return "Notify";
                    case "u" : return "Update";
                    case "?" : return "Unknown";
                    // This should never happen
                    default : return string.Format("ParseError <{0}>", OP.Trim());
                };
            }
            throw new ArgumentNullException(nameof(OP));
        }
    }
}
