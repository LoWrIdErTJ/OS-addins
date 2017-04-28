using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using UBotPlugin;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Net;
using System.Diagnostics;


// it's required for reading/writing into the registry:
using Microsoft.Win32;
// for SMTP AUTH command
using System.Net.Mail;
// for checking admin rights
using System.Security.Principal;

// for naudio convert mp3 to wav
using NAudio.Wave;

namespace CSVtoHTML
{

    // API KEY HERE
    public class PluginInfo
    {
        public static string HashCode { get { return "6825c26f1fb232fa5c1ffc99352c82e1b5bdf18d"; } }
    }

    // ---------------------------------------------------------------------------------------------------------- //
    //
    // ---------------------------------               COMMANDS               ----------------------------------- //
    //
    // ---------------------------------------------------------------------------------------------------------- //

    
    //
    //
    // SYSTEM SYSTEM_SHUTDOWN_RESTART_LOCK
    //
    //
    public class SYSTEM_SHUTDOWN_RESTART_LOCK : IUBotCommand
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();

        public SYSTEM_SHUTDOWN_RESTART_LOCK()
        {
            var xParameter = new UBotParameterDefinition("System Action", UBotType.String);
            xParameter.Options = new[] { "", "Shutdown", "Restart", "Log off", "Lock PC", "Hibernate", "Sleep/Standby" };
            _parameters.Add(xParameter);

        }

        public string Category
        {
            get { return "OS Commands"; }
        }

        public string CommandName
        {
            get { return "os actions"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {

            string saction = parameters["System Action"];

            if (saction == "Shutdown")
            {
                Process.Start("shutdown", "/s /t 0");
            }
            else if (saction == "Restart")
            {
                Process.Start("shutdown", "/r /t 0");
            }
            else if (saction == "Log off")
            {
                ExitWindowsEx(0, 0);
            }
            else if (saction == "Lock PC")
            {
                LockWorkStation();
            }
            else if (saction == "Hibernate")
            {
                SetSuspendState(true, true, true);
            }
            else if (saction == "Sleep/Standby")
            {
                SetSuspendState(false, true, true);
            }
            else { }

        }

        [DllImport("user32")]
        public static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

        [DllImport("user32")]
        public static extern void LockWorkStation();

        [DllImport("Powrprof.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);


        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }


    //
    //
    // WHOIS SITE BACKLINKER With Results
    //
    //
    public class WhoisBacklinkerWResults : IUBotCommand
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();

        public WhoisBacklinkerWResults()
        {
            _parameters.Add(new UBotParameterDefinition("Domain", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Path to Sites file", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Path to save results file", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Timeout (seconds)", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Position Variable", UBotType.UBotVariable));
        }

        public string Category
        {
            get { return "OS Commands"; }
        }

        public string CommandName
        {
            get { return "os backlink indexer w/Results"; }
        }

        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {

            string url = parameters["Domain"];
            string f = parameters["Path to Sites file"];
            string s = parameters["Path to save results file"];
            string var = parameters["Position Variable"];
            string stout = parameters["Timeout (seconds)"];
            int tout = Convert.ToInt32(stout);

            //const string f = sitesFile;
            //const string url = httpLink;
            const string placeholder = "{website}";
            string backurl, submiturl;

            List<string> lines = new List<string>();
            using (StreamReader r = new StreamReader(f))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            int count = lines.Count();
            int i = 0;
            FileStream MyFile = new FileStream(s, FileMode.OpenOrCreate);
            StreamWriter MyWriter = new StreamWriter(MyFile);
            while (i <= count - 1)
            {

                //get the backlink site from the list
                backurl = lines[i];
                //replace the placeholder text with your url
                submiturl = backurl.Replace(placeholder, url);
                //catch all exceptions
                try
                {
                    WebRequest request = WebRequest.Create(submiturl);
                    int incrementer;
                    incrementer = 1000;
                    request.Timeout = tout * incrementer;

                    // Sends the HttpWebRequest and waits for a response.
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        MyWriter.Write(submiturl + " Successfully Submitted");
                        MyWriter.Write(MyWriter.NewLine);
                    }
                    else
                    {
                        //MyWriter.Write(submiturl + " Failed");
                        //MyWriter.Write(MyWriter.NewLine);
                    }
                    response.Close();

                }
                catch (WebException we)
                {
                    var t = we;
                    //MyWriter.Write("\r\nWebException Raised. The following error occured : {0}", we.Status);
                    //MyWriter.Write(MyWriter.NewLine);
                }
                catch (Exception ex)
                {
                    var t = ex;
                    //MyWriter.Write("\nThe following Exception was raised : {0}", ex.Message);
                   // MyWriter.Write(MyWriter.NewLine);
                }

                ubotStudio.SetVariable(var, i.ToString());

                i++;
            }
            //System.Windows.Forms.MessageBox.Show("Submitted to all Backlink sites");
            MyWriter.Close();
            MyFile.Close();

        }

        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }

    //
    //
    // WHOIS SITE BACKLINKER
    //
    //
    public class WhoisBacklinkerWoutResults : IUBotCommand
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();

        public WhoisBacklinkerWoutResults()
        {
            _parameters.Add(new UBotParameterDefinition("Domain", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Path to Sites file", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Timeout (seconds)", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Position Variable", UBotType.UBotVariable));
        }

        public string Category
        {
            get { return "OS Commands"; }
        }

        public string CommandName
        {
            get { return "os backlink indexer w/o Results"; }
        }

        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {

            string url = parameters["Domain"];
            string f = parameters["Path to Sites file"];
            string var = parameters["Position Variable"];
            string stout = parameters["Timeout (seconds)"];
            int tout = Convert.ToInt32(stout);

            //const string f = sitesFile;
            //const string url = httpLink;
            const string placeholder = "{website}";
            string backurl, submiturl;

            List<string> lines = new List<string>();
            using (StreamReader r = new StreamReader(f))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            int count = lines.Count();
            int i = 0;
            
            while (i <= count - 1)
            {

                //get the backlink site from the list
                backurl = lines[i];
                //replace the placeholder text with your url
                submiturl = backurl.Replace(placeholder, url);
                //catch all exceptions
                try
                {
                    WebRequest request = WebRequest.Create(submiturl);
                    int incrementer;
                    incrementer = 1000;
                    request.Timeout = tout * incrementer;

                    // Sends the HttpWebRequest and waits for a response.
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    
                    response.Close();

                }
                catch (WebException we)
                {
                    var t = we;
                    //MyWriter.Write("\r\nWebException Raised. The following error occured : {0}", we.Status);
                    //MyWriter.Write(MyWriter.NewLine);
                }
                catch (Exception ex)
                {
                    var t = ex;
                    //MyWriter.Write("\nThe following Exception was raised : {0}", ex.Message);
                    // MyWriter.Write(MyWriter.NewLine);
                }

                ubotStudio.SetVariable(var, i.ToString());

                i++;
            }
            //System.Windows.Forms.MessageBox.Show("Submitted to all Backlink sites");
            
        }

        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }

    //
    //
    // BRING TO FRONT A WINDOW
    //
    //
    public class BringToFront : IUBotCommand
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();

        public BringToFront()
        {
            _parameters.Add(new UBotParameterDefinition("Window title", UBotType.String));
        }

        public string Category
        {
            get { return "OS Commands"; }
        }

        public string CommandName
        {
            get { return "os bring to front"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {

            string windownName = parameters["Window title"];

            Process[] objProcesses = System.Diagnostics.Process.GetProcessesByName(windownName);
            if (objProcesses.Length > 0)
            {
                IntPtr hWnd = IntPtr.Zero;
                hWnd = objProcesses[0].MainWindowHandle;
                ShowWindowAsync(new HandleRef(null, hWnd), SW_RESTORE);
                SetForegroundWindow(objProcesses[0].MainWindowHandle);
            }

        }

        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(HandleRef hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr WindowHandle);
        public const int SW_RESTORE = 9;


        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }


    //
    //
    // BYPASS CONFIDENT
    //
    //
    public class BypassCondifent_Captcha : IUBotFunction
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();
        private string _returnValue;

        public BypassCondifent_Captcha()
        {
            _parameters.Add(new UBotParameterDefinition("Username", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Password", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Path to Captcha Image", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Letters", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Words", UBotType.String));

        }

        public string Category
        {
            get { return "OS Functions"; }
        }

        public string FunctionName
        {
            get { return "$bypassConfident"; }
        }

                
        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {
            
            // you need to change the key to your own
            string user = parameters["Username"];
            string password = parameters["Password"];
            string task_id = "-1";
            string captcha_fn = parameters["Path to Captcha Image"];
            string letters = parameters["Letters"];
            string[] words = new[] { parameters["Words"] }; //parameters["Words"];

            string value = Decode(user, password, out task_id, captcha_fn, words, letters);

            _returnValue = value;
        }

        private static string UrlEncode(string str)
        {
            if (str == null) return "";

            Encoding enc = Encoding.ASCII;
            StringBuilder result = new StringBuilder();

            foreach (char symbol in str)
            {
                byte[] bs = enc.GetBytes(new char[] { symbol });
                for (int i = 0; i < bs.Length; i++)
                {
                    byte b = bs[i];
                    if (b >= 48 && b < 58 || b >= 65 && b < 65 + 26 || b >= 97 && b < 97 + 26) // decode non numalphabet
                    {
                        result.Append(Encoding.ASCII.GetString(bs, i, 1));
                    }
                    else
                    {
                        result.Append('%' + String.Format("{0:X2}", (int)b));
                    }
                }
            }

            return result.ToString();
        }

        private static string WebPost(string url, params string[] ps)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                //request.Proxy = WebRequest.DefaultWebProxy.GetEmptyWebProxy();
                request.ServicePoint.ConnectionLimit = 1000;
                string str = "";
                for (int i = 0; i + 1 < ps.Length; i += 2)
                {
                    str += UrlEncode(ps[i]) + "=" + UrlEncode(ps[i + 1]) + "&";
                }
                if (str.EndsWith("&"))
                {
                    str = str.Substring(0, str.Length - 1);
                }
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                byte[] data = Encoding.ASCII.GetBytes(str);
                request.ContentLength = data.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(data, 0, data.Length);

                WebResponse response = request.GetResponse();
                Stream sStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(sStream);
                string retContent = reader.ReadToEnd();
                reader.Close();
                response.Close();
                newStream.Close();
                return retContent;
            }
            catch
            {
                return "";
            }
        }

        private static string Join(string[] parts, string sep)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0) sb.Append(sep);
                sb.Append(parts[i]);
            }
            return sb.ToString();
        }

        // return null if failed
        public static string Decode(string user, string password, out string task_id, string image_fn, string[] words, string letters)
        {
            task_id = "-1";

            // read image data
            byte[] data = File.ReadAllBytes(image_fn);

            // base64 encode it
            string im = Convert.ToBase64String(data);

            // submit captcha to server
            string con = WebPost("http://bypassconfident.com/api_submit.php", new string[] {
                "user", user,
                "password", password,
                "file", im,
                "letters", letters,
                "words", Join(words, "|"),
                "base64_code", "1"});

            Match m = Regex.Match(con, @"^\s*OK\s+(\d+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (!m.Success) return "FAILED REGEX 1";

            task_id = m.Groups[1].Value;
            for (int i = 0; i < 300; i++)
            {
                con = WebPost("http://bypassconfident.com/api_get.php", new string[] {
                    "user", user,
                    "password", password,
                    "id", task_id
                });

                if (con == null) return "FAILED LOGIN";
                con = con.Trim();
                if (con == "NOT_INPUT")
                {
                    Thread.Sleep(1000 * 10);
                    continue;
                }
                m = Regex.Match(con, @"^\s*OK\s*(.+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (!m.Success) return "FAILED REGEX 2";
                return m.Groups[1].Value.Trim();
            }

            return "FAILED OVERALL";
        }

        public object ReturnValue
        {
            // We return our variable _returnValue as the result of the function.
            get { return _returnValue; }
        }

        public UBotType ReturnValueType
        {
            // Our result is text, so the return value type is String.
            get { return UBotType.String; }
        }


        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }

    //
    //
    // BYPASS CONFIDENT CREDITS
    //
    //
    public class BypassCondifent_CaptchaCredits : IUBotFunction
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();
        private string _returnValue;

        public BypassCondifent_CaptchaCredits()
        {
            _parameters.Add(new UBotParameterDefinition("Username", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Password", UBotType.String));

        }

        public string Category
        {
            get { return "OS Functions"; }
        }

        public string FunctionName
        {
            get { return "$bypassConfident credits"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {

            // you need to change the key to your own
            string user = parameters["Username"];
            string password = parameters["Password"];

            
            int value = Left(user, password);
            
            _returnValue = value.ToString();
        }

        private static string UrlEncode(string str)
        {
            if (str == null) return "";

            Encoding enc = Encoding.ASCII;
            StringBuilder result = new StringBuilder();

            foreach (char symbol in str)
            {
                byte[] bs = enc.GetBytes(new char[] { symbol });
                for (int i = 0; i < bs.Length; i++)
                {
                    byte b = bs[i];
                    if (b >= 48 && b < 58 || b >= 65 && b < 65 + 26 || b >= 97 && b < 97 + 26) // decode non numalphabet
                    {
                        result.Append(Encoding.ASCII.GetString(bs, i, 1));
                    }
                    else
                    {
                        result.Append('%' + String.Format("{0:X2}", (int)b));
                    }
                }
            }

            return result.ToString();
        }

        private static string WebPost(string url, params string[] ps)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                //request.Proxy = WebRequest.DefaultWebProxy.GetEmptyWebProxy();
                request.ServicePoint.ConnectionLimit = 1000;
                string str = "";
                for (int i = 0; i + 1 < ps.Length; i += 2)
                {
                    str += UrlEncode(ps[i]) + "=" + UrlEncode(ps[i + 1]) + "&";
                }
                if (str.EndsWith("&"))
                {
                    str = str.Substring(0, str.Length - 1);
                }
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                byte[] data = Encoding.ASCII.GetBytes(str);
                request.ContentLength = data.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(data, 0, data.Length);

                WebResponse response = request.GetResponse();
                Stream sStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(sStream);
                string retContent = reader.ReadToEnd();
                reader.Close();
                response.Close();
                newStream.Close();
                return retContent;
            }
            catch
            {
                return "";
            }
        }

        public static int Left(string user, string password)
        {
            string con = WebPost("http://bypassconfident.com/api_left.php", new string[] { "user", user, "password", password });
            if (con == null) return -1;
            Match m = Regex.Match(con, @"^\s*OK\s+([\-\d]+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (!m.Success) return -1;
            return int.Parse(m.Groups[1].Value);
        }

        public object ReturnValue
        {
            // We return our variable _returnValue as the result of the function.
            get { return _returnValue; }
        }

        public UBotType ReturnValueType
        {
            // Our result is text, so the return value type is String.
            get { return UBotType.String; }
        }


        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }


    //
    //
    // BYPASSCT CONFIDENT
    //
    //
    public class BypassCt_Captcha : IUBotFunction
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();
        private string _returnValue;

        public BypassCt_Captcha()
        {
            _parameters.Add(new UBotParameterDefinition("Username", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Password", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("TaskID", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Path to Captcha Image", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Letters", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Words", UBotType.String));

        }

        public string Category
        {
            get { return "OS Functions"; }
        }

        public string FunctionName
        {
            get { return "$bypassCt"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {

            // you need to change the key to your own
            string user = parameters["Username"];
            string password = parameters["Password"];
            string taskId = parameters["TaskID"];
            string task_id = taskId.ToString();
            string captcha_fn = parameters["Path to Captcha Image"];
            string letters = parameters["Letters"];
            string[] words = new[] { parameters["Words"] }; //parameters["Words"];

            string value = Decode(user, password, out task_id, captcha_fn, words, letters);

            _returnValue = value;
        }

        private static string UrlEncode(string str)
        {
            if (str == null) return "";

            Encoding enc = Encoding.ASCII;
            StringBuilder result = new StringBuilder();

            foreach (char symbol in str)
            {
                byte[] bs = enc.GetBytes(new char[] { symbol });
                for (int i = 0; i < bs.Length; i++)
                {
                    byte b = bs[i];
                    if (b >= 48 && b < 58 || b >= 65 && b < 65 + 26 || b >= 97 && b < 97 + 26) // decode non numalphabet
                    {
                        result.Append(Encoding.ASCII.GetString(bs, i, 1));
                    }
                    else
                    {
                        result.Append('%' + String.Format("{0:X2}", (int)b));
                    }
                }
            }

            return result.ToString();
        }

        private static string WebPost(string url, params string[] ps)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                //request.Proxy = WebRequest.DefaultWebProxy.GetEmptyWebProxy();
                request.ServicePoint.ConnectionLimit = 1000;
                string str = "";
                for (int i = 0; i + 1 < ps.Length; i += 2)
                {
                    str += UrlEncode(ps[i]) + "=" + UrlEncode(ps[i + 1]) + "&";
                }
                if (str.EndsWith("&"))
                {
                    str = str.Substring(0, str.Length - 1);
                }
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                byte[] data = Encoding.ASCII.GetBytes(str);
                request.ContentLength = data.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(data, 0, data.Length);

                WebResponse response = request.GetResponse();
                Stream sStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(sStream);
                string retContent = reader.ReadToEnd();
                reader.Close();
                response.Close();
                newStream.Close();
                return retContent;
            }
            catch
            {
                return "";
            }
        }

        private static string Join(string[] parts, string sep)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0) sb.Append(sep);
                sb.Append(parts[i]);
            }
            return sb.ToString();
        }

        // return null if failed
        public static string Decode(string user, string password, out string task_id, string image_fn, string[] words, string letters)
        {
            task_id = "-1";

            // read image data
            byte[] data = File.ReadAllBytes(image_fn);

            // base64 encode it
            string im = Convert.ToBase64String(data);

            // submit captcha to server
            string con = WebPost("http://api.bypassct.com/api_submit.php", new string[] {
                "user", user,
                "password", password,
                "file", im,
                "letters", letters,
                "words", Join(words, "|"),
                "base64_code", "1"});

            Match m = Regex.Match(con, @"^\s*OK\s+(\d+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (!m.Success) return "FAILED REGEX 1";

            task_id = m.Groups[1].Value;
            for (int i = 0; i < 300; i++)
            {
                con = WebPost("http://api.bypassct.com/api_get.php", new string[] {
                    "user", user,
                    "password", password,
                    "id", task_id
                });

                if (con == null) return "FAILED LOGIN";
                con = con.Trim();
                if (con == "NOT_INPUT")
                {
                    Thread.Sleep(1000 * 10);
                    continue;
                }
                m = Regex.Match(con, @"^\s*OK\s*(.+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (!m.Success) return "FAILED REGEX 2";
                return m.Groups[1].Value.Trim();
            }

            return "FAILED OVERALL";
        }

        public object ReturnValue
        {
            // We return our variable _returnValue as the result of the function.
            get { return _returnValue; }
        }

        public UBotType ReturnValueType
        {
            // Our result is text, so the return value type is String.
            get { return UBotType.String; }
        }


        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }

    //
    //
    // BYPASS CT CREDITS
    //
    //
    public class BypassCt_CaptchaCredits : IUBotFunction
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();
        private string _returnValue;

        public BypassCt_CaptchaCredits()
        {
            _parameters.Add(new UBotParameterDefinition("Username", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Password", UBotType.String));

        }

        public string Category
        {
            get { return "OS Functions"; }
        }

        public string FunctionName
        {
            get { return "$bypassCt credits"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {

            // you need to change the key to your own
            string user = parameters["Username"];
            string password = parameters["Password"];


            int value = Left(user, password);

            _returnValue = value.ToString();
        }

        private static string UrlEncode(string str)
        {
            if (str == null) return "";

            Encoding enc = Encoding.ASCII;
            StringBuilder result = new StringBuilder();

            foreach (char symbol in str)
            {
                byte[] bs = enc.GetBytes(new char[] { symbol });
                for (int i = 0; i < bs.Length; i++)
                {
                    byte b = bs[i];
                    if (b >= 48 && b < 58 || b >= 65 && b < 65 + 26 || b >= 97 && b < 97 + 26) // decode non numalphabet
                    {
                        result.Append(Encoding.ASCII.GetString(bs, i, 1));
                    }
                    else
                    {
                        result.Append('%' + String.Format("{0:X2}", (int)b));
                    }
                }
            }

            return result.ToString();
        }

        private static string WebPost(string url, params string[] ps)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                //request.Proxy = WebRequest.DefaultWebProxy.GetEmptyWebProxy();
                request.ServicePoint.ConnectionLimit = 1000;
                string str = "";
                for (int i = 0; i + 1 < ps.Length; i += 2)
                {
                    str += UrlEncode(ps[i]) + "=" + UrlEncode(ps[i + 1]) + "&";
                }
                if (str.EndsWith("&"))
                {
                    str = str.Substring(0, str.Length - 1);
                }
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                byte[] data = Encoding.ASCII.GetBytes(str);
                request.ContentLength = data.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(data, 0, data.Length);

                WebResponse response = request.GetResponse();
                Stream sStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(sStream);
                string retContent = reader.ReadToEnd();
                reader.Close();
                response.Close();
                newStream.Close();
                return retContent;
            }
            catch
            {
                return "";
            }
        }

        public static int Left(string user, string password)
        {
            string con = WebPost("http://api.bypassct.com/api_left.php", new string[] { "user", user, "password", password });
            if (con == null) return -1;
            Match m = Regex.Match(con, @"^\s*OK\s+([\-\d]+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (!m.Success) return -1;
            return int.Parse(m.Groups[1].Value);
        }

        public object ReturnValue
        {
            // We return our variable _returnValue as the result of the function.
            get { return _returnValue; }
        }

        public UBotType ReturnValueType
        {
            // Our result is text, so the return value type is String.
            get { return UBotType.String; }
        }


        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }


    //
    //
    // BYPASS CT REFUND
    //
    //
    public class BypassCt_CaptchaRefund : IUBotFunction
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();
        private string _returnValue;

        public BypassCt_CaptchaRefund()
        {
            _parameters.Add(new UBotParameterDefinition("Username", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Password", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("TaskID", UBotType.String));

        }

        public string Category
        {
            get { return "OS Functions"; }
        }

        public string FunctionName
        {
            get { return "$bypassCt refund"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {

            // you need to change the key to your own
            string user = parameters["Username"];
            string password = parameters["Password"];
            string TaskID = parameters["TaskID"];

            string value = Feedback(user, password, TaskID, true);

            _returnValue = value.ToString();
        }

        private static string UrlEncode(string str)
        {
            if (str == null) return "";

            Encoding enc = Encoding.ASCII;
            StringBuilder result = new StringBuilder();

            foreach (char symbol in str)
            {
                byte[] bs = enc.GetBytes(new char[] { symbol });
                for (int i = 0; i < bs.Length; i++)
                {
                    byte b = bs[i];
                    if (b >= 48 && b < 58 || b >= 65 && b < 65 + 26 || b >= 97 && b < 97 + 26) // decode non numalphabet
                    {
                        result.Append(Encoding.ASCII.GetString(bs, i, 1));
                    }
                    else
                    {
                        result.Append('%' + String.Format("{0:X2}", (int)b));
                    }
                }
            }

            return result.ToString();
        }

        public static string Feedback(string user, string password, string task_id, bool is_input_correct)
        {
            string result = WebPost("http://api.bypassct.com/api_report.php", new string[] {
                "user", user,
                "password", password,
                "id", task_id,
                "value", (is_input_correct ? "1" : "0"),
            });

            return result.ToString();
        }

        private static string WebPost(string url, params string[] ps)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                //request.Proxy = WebRequest.DefaultWebProxy.GetEmptyWebProxy();
                request.ServicePoint.ConnectionLimit = 1000;
                string str = "";
                for (int i = 0; i + 1 < ps.Length; i += 2)
                {
                    str += UrlEncode(ps[i]) + "=" + UrlEncode(ps[i + 1]) + "&";
                }
                if (str.EndsWith("&"))
                {
                    str = str.Substring(0, str.Length - 1);
                }
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                byte[] data = Encoding.ASCII.GetBytes(str);
                request.ContentLength = data.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(data, 0, data.Length);

                WebResponse response = request.GetResponse();
                Stream sStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(sStream);
                string retContent = reader.ReadToEnd();
                reader.Close();
                response.Close();
                newStream.Close();
                return retContent;
            }
            catch
            {
                return "failed";
            }
        }

        public object ReturnValue
        {
            // We return our variable _returnValue as the result of the function.
            get { return _returnValue; }
        }

        public UBotType ReturnValueType
        {
            // Our result is text, so the return value type is String.
            get { return UBotType.String; }
        }


        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }


    //
    //
    // CHECK IF ADMIN RIGHTS (FUNCTION)
    //
    //
    public class CheckAdminRights : IUBotFunction
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();
        private string _returnValue;

        public CheckAdminRights()
        {

        }

        public string Category
        {
            get { return "OS Functions"; }
        }

        public string FunctionName
        {
            get { return "$check admin rights"; }
        }

        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {

            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            _returnValue = principal.IsInRole(WindowsBuiltInRole.Administrator).ToString();

        }

        public object ReturnValue
        {
            // We return our variable _returnValue as the result of the function.
            get { return _returnValue; }
        }

        public UBotType ReturnValueType
        {
            // Our result is text, so the return value type is String.
            get { return UBotType.String; }
        }

        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }


    //
    //
    // CLEAR CACHE
    //
    //
    public class ClearCache : IUBotCommand
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();

        public ClearCache()
        {

        }

        public string Category
        {
            get { return "OS Commands"; }
        }

        public string CommandName
        {
            get { return "os clear cache"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {

            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo("rundll32.exe", "InetCpl.cpl,ClearMyTracksByProcess 8");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardInput = true;
                psi.RedirectStandardError = true;
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                var temp = ex.ToString();
            }

        }


        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }


    //
    //
    // CLEAR Cookies
    //
    //
    public class ClearCookiesNow : IUBotCommand
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();

        public ClearCookiesNow()
        {

        }

        public string Category
        {
            get { return "OS Commands"; }
        }

        public string CommandName
        {
            get { return "os clear cookies"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {

            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo("rundll32.exe", "InetCpl.cpl,ClearMyTracksByProcess 2");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardInput = true;
                psi.RedirectStandardError = true;
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                var temp = ex.ToString();
            }

        }


        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }


    //
    //
    // CLEAR HISTORY
    //
    //
    public class ClearHistory : IUBotCommand
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();

        public ClearHistory()
        {

        }

        public string Category
        {
            get { return "OS Commands"; }
        }

        public string CommandName
        {
            get { return "os clear history"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {

            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo("rundll32.exe", "InetCpl.cpl,ClearMyTracksByProcess 1");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardInput = true;
                psi.RedirectStandardError = true;
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                var temp = ex.ToString();
            }

        }


        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }


    //
    //
    // CLEAR FLASH COOKIES
    //
    //
    public class ClearFlashCookies : IUBotCommand
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();

        public ClearFlashCookies()
        {

        }

        public string Category
        {
            get { return "OS Commands"; }
        }

        public string CommandName
        {
            get { return "os clear flash"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {

            string RootPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal).ToLower().Replace("documents", "");
            RemoveSpywareFiles(RootPath, @"AppData\Roaming\Macromedia\Flash Player\#SharedObjects", true);
            RemoveSpywareFiles(RootPath, @"AppData\Roaming\Macromedia\Flash Player\macromedia.com\support\flashplayer", true);
            //RemoveSpywareFiles(RootPath, @"AppData\Local\Temporary Internet Files", true);//Not working
            //RemoveSpywareFiles(RootPath, @"AppData\Local\Microsoft\Windows\Caches", false);
            //RemoveSpywareFiles(RootPath, @"AppData\Local\Microsoft\WebsiteCache", false);
            //RemoveSpywareFiles(RootPath, @"AppData\Local\Temp", true);
            //RemoveSpywareFiles(RootPath, @"AppData\LocalLow\Microsoft\CryptnetUrlCache", false);
            //RemoveSpywareFiles(RootPath, @"AppData\LocalLow\Apple Computer\QuickTime\downloads", false);
            RemoveSpywareFiles(RootPath, @"AppData\Roaming\Adobe\Flash Player\AssetCache", true);

        }

        private static void RemoveSpywareFiles(string RootPath, string Path, bool Recursive)
        {
            string FullPath = RootPath + Path + "\\";
            if (Directory.Exists(FullPath))
            {
                DirectoryInfo DInfo = new DirectoryInfo(FullPath);
                FileAttributes Attr = DInfo.Attributes;
                DInfo.Attributes = FileAttributes.Normal;
                foreach (string FileName in Directory.GetFiles(FullPath))
                {
                    RemoveSpywareFile(FileName);
                }
                if (Recursive)
                {
                    foreach (string DirName in Directory.GetDirectories(FullPath))
                    {
                        RemoveSpywareFiles("", DirName, true);
                        try { Directory.Delete(DirName); }
                        catch { }
                    }
                }
                DInfo.Attributes = Attr;
            }
        }

        private static void RemoveSpywareFile(string FileName)
        {
            if (File.Exists(FileName))
            {
                try { File.Delete(FileName); }
                catch { }//Locked by something and you can forget trying to delete index.dat files this way
            }
        }


        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }


    //
    //
    // CLEAR TEMP RECENT AND PREFETCH
    //
    //
    public class ClearSystemFolders : IUBotCommand
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();

        public ClearSystemFolders()
        {
            var xParameter = new UBotParameterDefinition("Clear Action", UBotType.String);
            xParameter.Options = new[] { "", "Clear Temp Folder", "Clear Recent", "Clear Flash Objects", "Clear Temp Internet Files" };
            _parameters.Add(xParameter);

        }

        public string Category
        {
            get { return "OS Commands"; }
        }

        public string CommandName
        {
            get { return "os clear options"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {

            string saction = parameters["Clear Action"];
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString();
            string userpro = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).ToString();
            string recent = Environment.GetFolderPath(Environment.SpecialFolder.Recent).ToString();
            string InetCache = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache).ToString();
            string cookies = Environment.GetFolderPath(Environment.SpecialFolder.Cookies).ToString();

            
            if (saction == "Clear Temp Folder")
            {
                String tempFolder = Environment.ExpandEnvironmentVariables("%TEMP%");
                EmptyFolderContents(tempFolder);

                String tempFolders = userpro + @"\AppData\Local\Temp";
                EmptyFolderContents(tempFolders);

                String tempFolderss = userpro + @"\AppData\Local Settings\Temp";
                EmptyFolderContents(tempFolderss);
            }
            else if (saction == "Clear Recent")
            {
                String recents = userpro + @"\Recent";
                EmptyFolderContents(recents);
            }
            else if (saction == "Clear Flash Objects")
            {
                String FlashFile = appdata + @"\Roaming\Macromedia\Flash Player\#SharedObjects";
                EmptyFolderContents(FlashFile);

                String FlashFiles = appdata + @"\Roaming\Macromedia\Flash Player\macromedia.com\support\flashplayer\sys\#local";
                EmptyFolderContents(FlashFiles);
            }
            else if (saction == "Clear Temp Internet Files")
            {
                String tempInetFilez = Environment.SpecialFolder.InternetCache.ToString();
                EmptyFolderContents(tempInetFilez);

                String tempInetFile = userpro + @"\AppData\Local\Temporary Internet Files";
                EmptyFolderContents(tempInetFile);

                String tempInetFiles = userpro + @"\AppData\Local Settings\Temporary Internet Files";
                EmptyFolderContents(tempInetFiles);

                String tempInetFiless = userpro + @"\AppData\Local\Microsoft\Windows\Temporary Internet Files";
                EmptyFolderContents(tempInetFiless);
            }
            else { }

        }

        private void EmptyFolderContents(string directoryPath)
        {
            try
            {
                // delete directory
                Directory.Delete(directoryPath, true);
                // recreate the directory if it actually gets deleted all the way.
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

            }
            catch (Exception)
            {
            } 
            
            //DirectoryInfo d = new DirectoryInfo(directoryPath);
	 
            //foreach (FileInfo fi in d.GetFiles())
            //{
            //    fi.Delete();
            //}
	 
            //foreach (DirectoryInfo di in d.GetDirectories())
            //{
            //    EmptyFolderContents(di.FullName);
	 
            //    di.Delete(true);
            //}
        }

        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }


    //
    //
    // FREE UP MY MEMORY
    //
    //
    public class FreeupMemorys : IUBotCommand
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();

        public FreeupMemorys()
        {

            // no inputs

        }

        public string Category
        {
            get { return "OS Commands"; }
        }

        public string CommandName
        {
            get { return "os free memory"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {

            FreeAllMemory();

        }

        void FreeAllMemory()
        {
            foreach (Process P in Process.GetProcesses())
            {
                //The try statement is just in case we do not have access to the process
                try
                {
                    SetWorkingSet(P.Handle, -1, -1);
                }
                catch
                {
                    //could not access process...
                }
            }
        }

        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        static extern bool SetWorkingSet(IntPtr handle, int minimum, int maximum);

        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }



    //
    //
    // FREE UP MY MEMORY Single process
    //
    //
    public class FreeupMemorysSingleProcess : IUBotCommand
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();

        public FreeupMemorysSingleProcess()
        {

            _parameters.Add(new UBotParameterDefinition("Window title/Process name", UBotType.String));

        }

        public string Category
        {
            get { return "OS Commands"; }
        }

        public string CommandName
        {
            get { return "os free memory single process"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {
            string windownName = parameters["Window title/Process name"];

            FreeAllMemorySingle(windownName);

        }

        void FreeAllMemorySingle(string input)
        {
            Process[] processes = Process.GetProcessesByName(input);

            foreach (Process p in processes)
            {
                IntPtr windowHandle = p.MainWindowHandle;

                // do something with windowHandle
                SetWorkingSet(p.Handle, -1, -1);
            }

        }

        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        static extern bool SetWorkingSet(IntPtr handle, int minimum, int maximum);

        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }



    //
    //
    // MP3 to WAV
    //
    //
    public class MP3ToWavProcess : IUBotCommand
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();

        public MP3ToWavProcess()
        {

            _parameters.Add(new UBotParameterDefinition("Path to MP3", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Save path to WAV", UBotType.String));

        }

        public string Category
        {
            get { return "OS Commands"; }
        }

        public string CommandName
        {
            get { return "mp3 to wav"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {
            string mp3St = parameters["Path to MP3"];
            string wavSt = parameters["Save path to WAV"];

            ConvertMp3ToWav(mp3St, wavSt);

        }

        private static void ConvertMp3ToWav(string _inPath_, string _outPath_)
        {
            using (Mp3FileReader mp3 = new Mp3FileReader(_inPath_))
            {
                using (WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(mp3))
                {
                    WaveFileWriter.CreateWaveFile(_outPath_, pcm);
                }
            }
        }

        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }




    //
    //
    // HOST TO IP
    //
    //
    public class HostToIP : IUBotFunction
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();
        private string _returnValue;

        public HostToIP()
        {

            _parameters.Add(new UBotParameterDefinition("Host Domain", UBotType.String));

        }

        public string Category
        {
            get { return "OS Functions"; }
        }

        public string FunctionName
        {
            get { return "$host to ip"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {
            string HostNameDomain = parameters["Host Domain"];

            string howtogeek = HostNameDomain;
            IPAddress[] addresslist = Dns.GetHostAddresses(howtogeek);

            StringBuilder sb = new StringBuilder(); 

            foreach (IPAddress theaddress in addresslist)
            {
                //Console.WriteLine(theaddress.ToString());
                sb.AppendLine(theaddress.ToString()); 
            }
            
            _returnValue = sb.ToString();

            sb.Clear();


        }

        public object ReturnValue
        {
            // We return our variable _returnValue as the result of the function.
            get { return _returnValue; }
        }

        public UBotType ReturnValueType
        {
            // Our result is text, so the return value type is String.
            get { return UBotType.String; }
        }

        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }


    //
    //
    // REGISTRY
    //
    //
    public class REGISTRY_read : IUBotFunction
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();
        private string _returnValue;

        public REGISTRY_read()
        {
            var xParameter = new UBotParameterDefinition("Registry Action", UBotType.String);
            xParameter.Options = new[] { "", "Read", "Write", "Delete" };
            _parameters.Add(xParameter);

            _parameters.Add(new UBotParameterDefinition("Key", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("If Write Value", UBotType.String));

        }

        public string Category
        {
            get { return "OS Functions"; }
        }

        public string FunctionName
        {
            get { return "$os registry"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {

            string registry_action = parameters["Registry Action"];
            string registry_key = parameters["Key"];
            string registry_value = parameters["If Write Value"];

            if (registry_action == "Read")
            {
                var answer = Read(registry_key);
                _returnValue = answer.ToString();
            }
            else if (registry_action == "Write")
            {
                Object registry_valueO = (Object)registry_value;
                var answer = Write(registry_key, registry_valueO);
                _returnValue = answer.ToString();
            }
            else if (registry_action == "Delete")
            {
                var answer = DeleteKey(registry_key);
                _returnValue = answer.ToString();
            }
            else { 
                string answer = "Failed";
                _returnValue = answer;
            }

        }

        private bool showError = false;
        /// <summary>
        /// A property to show or hide error messages 
        /// (default = false)
        /// </summary>
        public bool ShowError
        {
            get { return showError; }
            set { showError = value; }
        }

        private string subKey = "SOFTWARE\\" + System.Windows.Forms.Application.ProductName.ToUpper();
        /// <summary>
        /// A property to set the SubKey value
        /// (default = "SOFTWARE\\" + Application.ProductName.ToUpper())
        /// </summary>
        public string SubKey
        {
            get { return subKey; }
            set { subKey = value; }
        }

        private RegistryKey baseRegistryKey = Registry.LocalMachine;
        /// <summary>
        /// A property to set the BaseRegistryKey value.
        /// (default = Registry.LocalMachine)
        /// </summary>
        public RegistryKey BaseRegistryKey
        {
            get { return baseRegistryKey; }
            set { baseRegistryKey = value; }
        }

        /* **************************************************************************
         * **************************************************************************/

        /// <summary>
        /// To read a registry key.
        /// input: KeyName (string)
        /// output: value (string) 
        /// </summary>
        public string Read(string KeyName)
        {
            // Opening the registry key
            RegistryKey rk = baseRegistryKey;
            // Open a subKey as read-only
            RegistryKey sk1 = rk.OpenSubKey(subKey);
            // If the RegistrySubKey doesn't exist -> (null)
            if (sk1 == null)
            {
                return null;
            }
            else
            {
                try
                {
                    // If the RegistryKey exists I get its value
                    // or null is returned.
                    return (string)sk1.GetValue(KeyName.ToUpper());
                }
                catch (Exception e)
                {
                    // AAAAAAAAAAARGH, an error!
                    ShowErrorMessage(e, "Reading registry " + KeyName.ToUpper());
                    return null;
                }
            }
        }

        /* **************************************************************************
         * **************************************************************************/

        /// <summary>
        /// To write into a registry key.
        /// input: KeyName (string) , Value (object)
        /// output: true or false 
        /// </summary>
        public bool Write(string KeyName, object Value)
        {
            try
            {
                // Setting
                RegistryKey rk = baseRegistryKey;
                // I have to use CreateSubKey 
                // (create or open it if already exits), 
                // 'cause OpenSubKey open a subKey as read-only
                RegistryKey sk1 = rk.CreateSubKey(subKey);
                // Save the value
                sk1.SetValue(KeyName.ToUpper(), Value);

                return true;
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                ShowErrorMessage(e, "Writing registry " + KeyName.ToUpper());
                return false;
            }
        }

        /* **************************************************************************
         * **************************************************************************/

        /// <summary>
        /// To delete a registry key.
        /// input: KeyName (string)
        /// output: true or false 
        /// </summary>
        public bool DeleteKey(string KeyName)
        {
            try
            {
                // Setting
                RegistryKey rk = baseRegistryKey;
                RegistryKey sk1 = rk.CreateSubKey(subKey);
                // If the RegistrySubKey doesn't exists -> (true)
                if (sk1 == null)
                    return true;
                else
                    sk1.DeleteValue(KeyName);

                return true;
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                ShowErrorMessage(e, "Deleting SubKey " + subKey);
                return false;
            }
        }

        /* **************************************************************************
         * **************************************************************************/

        /// <summary>
        /// To delete a sub key and any child.
        /// input: void
        /// output: true or false 
        /// </summary>
        public bool DeleteSubKeyTree()
        {
            try
            {
                // Setting
                RegistryKey rk = baseRegistryKey;
                RegistryKey sk1 = rk.OpenSubKey(subKey);
                // If the RegistryKey exists, I delete it
                if (sk1 != null)
                    rk.DeleteSubKeyTree(subKey);

                return true;
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                ShowErrorMessage(e, "Deleting SubKey " + subKey);
                return false;
            }
        }

        /* **************************************************************************
         * **************************************************************************/

        /// <summary>
        /// Retrive the count of subkeys at the current key.
        /// input: void
        /// output: number of subkeys
        /// </summary>
        public int SubKeyCount()
        {
            try
            {
                // Setting
                RegistryKey rk = baseRegistryKey;
                RegistryKey sk1 = rk.OpenSubKey(subKey);
                // If the RegistryKey exists...
                if (sk1 != null)
                    return sk1.SubKeyCount;
                else
                    return 0;
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                ShowErrorMessage(e, "Retriving subkeys of " + subKey);
                return 0;
            }
        }

        /* **************************************************************************
         * **************************************************************************/

        /// <summary>
        /// Retrive the count of values in the key.
        /// input: void
        /// output: number of keys
        /// </summary>
        public int ValueCount()
        {
            try
            {
                // Setting
                RegistryKey rk = baseRegistryKey;
                RegistryKey sk1 = rk.OpenSubKey(subKey);
                // If the RegistryKey exists...
                if (sk1 != null)
                    return sk1.ValueCount;
                else
                    return 0;
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                ShowErrorMessage(e, "Retriving keys of " + subKey);
                return 0;
            }
        }

        /* **************************************************************************
         * **************************************************************************/

        private void ShowErrorMessage(Exception e, string Title)
        {
            if (showError == true)
                System.Windows.Forms.MessageBox.Show(e.Message,
                                Title
                                , MessageBoxButtons.OK
                                , MessageBoxIcon.Error);
        }

        public object ReturnValue
        {
            // We return our variable _returnValue as the result of the function.
            get { return _returnValue; }
        }

        public UBotType ReturnValueType
        {
            // Our result is text, so the return value type is String.
            get { return UBotType.String; }
        }

        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }



    //
    //
    // GET SCREEN RESOLUTION
    //
    //
    public class GETSCreenResolution : IUBotCommand
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();

        public GETSCreenResolution()
        {
            _parameters.Add(new UBotParameterDefinition("Original Height", UBotType.UBotVariable));
            _parameters.Add(new UBotParameterDefinition("Original Width", UBotType.UBotVariable));

        }

        public string Category
        {
            get { return "OS Commands"; }
        }

        public string CommandName
        {
            get { return "os get screen Resolution"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {

            string OrgH = parameters["Original Height"];
            string OrgW = parameters["Original Width"];

            int h, w; 
            h = Screen.PrimaryScreen.Bounds.Height;
            w = Screen.PrimaryScreen.Bounds.Width;

            ubotStudio.SetVariable(OrgH, h.ToString());
            ubotStudio.SetVariable(OrgW, w.ToString());
            
        }

        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }


    //
    //
    // SET SCREEN RESOLUTION
    //
    //
    /*public class SETSCreenResolution : IUBotCommand
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();

        public SETSCreenResolution()
        {
            _parameters.Add(new UBotParameterDefinition("Height", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Width", UBotType.String));

        }

        public string Category
        {
            get { return "OS Commands"; }
        }

        public string CommandName
        {
            get { return "os change screen Resolution"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {

            string OrgH = parameters["Height"];
            string OrgW = parameters["Width"];

            int OrgHInt = Int32.Parse(OrgH);
            int OrgWInt = Int32.Parse(OrgW);


            MyTactics.blogspot.com.NewResolution n =
            new MyTactics.blogspot.com.NewResolution(OrgHInt, OrgWInt);

        }

        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }
    */

    //
    //
    // SYSTEM MUTE UNMUTE
    //
    //
    public class SYSTEM_MUTE_UNMUTE : IUBotCommand
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();

        public SYSTEM_MUTE_UNMUTE()
        {
            // no input needed

        }

        public string Category
        {
            get { return "OS Commands"; }
        }

        public string CommandName
        {
            get { return "os mute/unmute"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {

            IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;

            // MUTE & UNMUTE SPEAKERS
            SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_VOLUME_MUTE);

        }

        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int WM_APPCOMMAND = 0x319;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        //public IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;

        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }


    //
    //
    // SEND KEYS
    //
    //
    public class SendkeysNoWindow : IUBotCommand
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();

        public SendkeysNoWindow()
        {
            _parameters.Add(new UBotParameterDefinition("Sendkey Data", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Delay in MS (Miliseconds)", UBotType.String));
        }

        public string Category
        {
            get { return "OS Commands"; }
        }

        public string CommandName
        {
            get { return "os sendkeys general"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {
            int result;
            string senddata = parameters["Sendkey Data"];
            string DelayInSec = parameters["Delay in MS (Miliseconds)"];

            result = Convert.ToInt32(DelayInSec);

            foreach (char c in senddata)
            {
                SendKeys.SendWait(c.ToString());
                System.Threading.Thread.Sleep(result);

            }

        }

        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }


    //
    //
    // SEND KEYS
    //
    //
    public class SendkeysOptions : IUBotCommand
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();

        public SendkeysOptions()
        {
            var xParameter = new UBotParameterDefinition("Sendkey Data", UBotType.String);
            xParameter.Options = new[] { "", "Right", "Left", "Up", "Down", "Enter", "Tab", "Delete" };
            _parameters.Add(xParameter); 
            
        }

        public string Category
        {
            get { return "OS Commands"; }
        }

        public string CommandName
        {
            get { return "os sendkey options"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {
            int result;
            string senddata = parameters["Sendkey Data"];
            string DelayInSec = parameters["Delay in MS (Miliseconds)"];

            result = Convert.ToInt32(DelayInSec);

            if (senddata.ToString() == "Right")
            {
                SendKeys.SendWait("{RIGHT}");
            }
            if (senddata.ToString() == "Left")
            {
                SendKeys.SendWait("{LEFT}");
            }
            if (senddata.ToString() == "Up")
            {
                SendKeys.SendWait("{UP}");
            }
            if (senddata.ToString() == "Down")
            {
                SendKeys.SendWait("{DOWN}");
            }
            if (senddata.ToString() == "Enter")
            {
                SendKeys.SendWait("{ENTER}");
            }
            if (senddata.ToString() == "Tab")
            {
                SendKeys.SendWait("{TAB}");
            }
            if (senddata.ToString() == "Delete")
            {
                SendKeys.Send("{BACKSPACE}");
            }
            

        }

        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }


    //
    //
    // SEND KEYS TO WINDOW BY NAME
    //
    //
    public class Sendkeys_Command : IUBotCommand
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();

        public Sendkeys_Command()
        {
            _parameters.Add(new UBotParameterDefinition("Window Name", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Sendkey Data", UBotType.String));
        }

        public string Category
        {
            get { return "OS Commands"; }
        }

        public string CommandName
        {
            get { return "os sendkeys command"; }
        }

        //using System.Runtime.InteropServices;
        [DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(int hWnd);

        [DllImport("user32.dll")]
        public static extern int FindWindow(string lpClassName, string lpWindowName);


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {

            string winname = parameters["Window Name"];
            string senddata = parameters["Sendkey Data"];

            //using System.Runtime.InteropServices;   
            int handle = FindWindow(null, winname);
            SetForegroundWindow(handle);

            SendKeys.SendWait(senddata);

        }

        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }


    //
    //
    // SEND KEYS TO WINDOW BY NAME WAIT FOR WINDOW
    //
    //
    public class Sendkeys_Command_Wait : IUBotCommand
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();

        public Sendkeys_Command_Wait()
        {
            _parameters.Add(new UBotParameterDefinition("Window Name", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Sendkey Data", UBotType.String));
        }

        public string Category
        {
            get { return "OS Commands"; }
        }

        public string CommandName
        {
            get { return "os sendkeys wait for window"; }
        }

        //using System.Runtime.InteropServices;
        [DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(int hWnd);

        [DllImport("user32.dll")]
        public static extern int FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private static string GetActiveWindowTitle()
        {
            var buff = new StringBuilder(256);
            var handle = GetForegroundWindow();
            return GetWindowText(handle, buff, 256) > 0 ? buff.ToString() : "";
        }

        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {

            string winname = parameters["Window Name"];
            string senddata = parameters["Sendkey Data"];

            //using System.Runtime.InteropServices;   
            int handle = FindWindow(null, winname);

            while (true)
            {
                string str = winname;
                if (!GetActiveWindowTitle().Contains(str))
                    Thread.Sleep(10);
                else
                    break;
            }
            Thread.Sleep(1500);
            SetForegroundWindow(handle);
            SendKeys.Send(senddata);


        }

        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }


    //
    //
    // SPLASHSCREEN ADDIN
    //
    //
    public class SplashScreen_OsAddin : IUBotCommand
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();

        public SplashScreen_OsAddin()
        {
            _parameters.Add(new UBotParameterDefinition("Image Path", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Window Width", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Window Height", UBotType.String));
        }

        public string Category
        {
            get { return "OS Commands"; }
        }

        public string CommandName
        {
            get { return "os splashscreen"; }
        }

        
        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {

            string imagepath = parameters["Image Path"];
            string windowWidth = parameters["Window Width"];
            string windowHeight = parameters["Window Height"];

            int winwid = Convert.ToInt32(windowWidth);
            int winhei = Convert.ToInt32(windowHeight);

            form22 = new Form();

            form22.Width = winwid;
            form22.Height = winhei;

            //Image image = Image.FromFile(imagepath);

            form22.BackgroundImage = LoadImageNoLock(imagepath);
            form22.BackgroundImageLayout = ImageLayout.Stretch;

            form22.StartPosition = FormStartPosition.CenterScreen;
            form22.FormBorderStyle = FormBorderStyle.None;
            form22.TopMost = true;


            form22.Show();
            System.Threading.Thread.Sleep(5000);
            ubotStudio.RunContainerCommands();

            form22.Close();
            //image.Dispose();
            form22.BackgroundImage.Dispose();

            
        }

        public Form form22 = null;

        public static Image LoadImageNoLock(string path)
        {
            var ms = new MemoryStream(File.ReadAllBytes(path)); // Don't use using!!
            return Image.FromStream(ms);
        }

        public bool IsContainer
        {
            get { return true; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }


    //
    //
    // TYPEWRITTER FUNCTION
    //
    //
    public class TypeWrite : IUBotFunction
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();
        private string _returnValue;

        public TypeWrite()
        {
            _parameters.Add(new UBotParameterDefinition("Text to Type", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Delay in MS (Miliseconds)", UBotType.String));

        }

        public string Category
        {
            get { return "OS Functions"; }
        }

        public string FunctionName
        {
            get { return "$os typewriter effect"; }
        }

                
        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {
            int result;
            string textToType = parameters["Text to Type"];
            string DelayInSec = parameters["Delay in MS (Miliseconds)"];
            result = Convert.ToInt32(DelayInSec);

            foreach (char c in textToType)
            {
                SendKeys.SendWait(c.ToString());
                System.Threading.Thread.Sleep(result);
                
            }

            _returnValue = "";
        }


        public object ReturnValue
        {
            // We return our variable _returnValue as the result of the function.
            get { return _returnValue; }
        }

        public UBotType ReturnValueType
        {
            // Our result is text, so the return value type is String.
            get { return UBotType.String; }
        }


        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }


    //
    //
    // FILTER LARGE FILE DOMAIN TLDS FUNCTION
    //
    //
    public class FilterTLDS : IUBotFunction
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();
        private string _returnValue;

        public FilterTLDS()
        {
            _parameters.Add(new UBotParameterDefinition("Path to file", UBotType.String));
            //_parameters.Add(new UBotParameterDefinition("Delay in MS (Miliseconds)", UBotType.String));

        }

        public string Category
        {
            get { return "OS Functions"; }
        }

        public string FunctionName
        {
            get { return "$url filter US Tlds"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {

            _returnValue = "running"; 

            string FilePath = parameters["Path to file"];

            string[] lines = File.ReadAllLines(FilePath);
            List<string> linesToWrite = new List<string>();
            //int currentCount = 0;

            foreach (string s in lines)
            {
                _returnValue = s + "running";

                if (s.Contains(".com/"))
                    linesToWrite.Add(s);
                else if (s.Contains(".net/"))
                    linesToWrite.Add(s);
                else if (s.Contains(".org/"))
                    linesToWrite.Add(s);
                else if (s.Contains(".edu/"))
                    linesToWrite.Add(s);
                else if (s.Contains(".gov/"))
                    linesToWrite.Add(s);
                
            }
            File.WriteAllLines(FilePath, linesToWrite);

            _returnValue = "complete";
        }


        public object ReturnValue
        {
            // We return our variable _returnValue as the result of the function.
            get { return _returnValue; }
        }

        public UBotType ReturnValueType
        {
            // Our result is text, so the return value type is String.
            get { return UBotType.String; }
        }


        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }



    //
    //
    // Return Clipboard FUNCTION
    //
    //
    public class ReturnClipboard : IUBotFunction
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();
        private string _returnValue;

        public ReturnClipboard()
        {

        }

        public string Category
        {
            get { return "OS Functions"; }
        }

        public string FunctionName
        {
            get { return "$os get clipboard"; }
        }

        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {


            System.Windows.Forms.IDataObject ClipData = System.Windows.Forms.Clipboard.GetDataObject();
            string s = System.Windows.Forms.Clipboard.GetData(System.Windows.DataFormats.Text).ToString();

            //paste it in your application somewhere

            _returnValue = s;
        }


        public object ReturnValue
        {
            // We return our variable _returnValue as the result of the function.
            get { return _returnValue; }
        }

        public UBotType ReturnValueType
        {
            // Our result is text, so the return value type is String.
            get { return UBotType.String; }
        }


        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }

    //
    //
    // DOWNLOAD YOUTUBE
    //
    //
    public class DownloadYoutube : IUBotCommand
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();

        public DownloadYoutube()
        {
            _parameters.Add(new UBotParameterDefinition("Image Path", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Window Width", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Window Height", UBotType.String));
        }

        public string Category
        {
            get { return "OS Commands"; }
        }

        public string CommandName
        {
            get { return "os splashscreen"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {

            string imagepath = parameters["Image Path"];
            string windowWidth = parameters["Window Width"];
            string windowHeight = parameters["Window Height"];

            
        }

        
        public bool IsContainer
        {
            get { return true; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }

    //
    //
    // GET BETWEEN STRINGS
    //
    //

    public class GetBetweenStrings : IUBotFunction
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();
        private string _returnValue;

        public GetBetweenStrings()
        {
            _parameters.Add(new UBotParameterDefinition("String", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Start", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("End", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Type not used", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Index not used", UBotType.String));

        }

        public string Category
        {
            get { return "OS Functions"; }
        }

        public string FunctionName
        {
            get { return "$get text between OS"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {

            // you need to change the key to your own
            string String = parameters["String"];
            string Start = parameters["Start"];
            string End = parameters["End"];

            String St = String;

            int pFrom = St.IndexOf(Start) + Start.Length;
            int pTo = St.LastIndexOf(End);

            String result = St.Substring(pFrom, pTo - pFrom);

            _returnValue = result;
        }
        
        public object ReturnValue
        {
            // We return our variable _returnValue as the result of the function.
            get { return _returnValue; }
        }

        public UBotType ReturnValueType
        {
            // Our result is text, so the return value type is String.
            get { return UBotType.String; }
        }


        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public IEnumerable<string> Options
        {
            get;
            set;
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }


}