﻿// Copyright 2017 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NLog;

namespace Ghosts.Domain.Code
{
    public class ClientConfiguration
    {
        /// <summary>
        /// Should each instance generate and use its own ID to identify with server?
        /// </summary>
        public bool IdEnabled { get; set; }

        /// <summary>
        /// API URL for client to get its instance ID
        /// </summary>
        public string IdUrl { get; set; }

        /// <summary>
        /// guest|guestinfo
        /// </summary>
        public string IdFormat { get; set; }

        /// <summary>
        /// if using guestinfo, the key to query for the value to use as hostname
        /// </summary>
        public string IdFormatKey { get; set; }

        public string IdFormatValue { get; set; }

        /// <summary>
        /// Where is vmtools?
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string VMWareToolsLocation { get; set; }

        /// <summary>
        /// Are client health checks enabled?
        /// </summary>
        public bool HealthIsEnabled { get; set; }

        /// <summary>
        /// Is client executing a timeline of user activities?
        /// </summary>
        public bool HandlersIsEnabled { get; set; }

        /// <summary>
        /// Disable client adding itself to auto-start
        /// </summary>
        public bool DisableStartup { get; set; }

        /// <summary>
        /// Comma sep list of extensions for chrome (aka c:\path\to\extension)
        /// </summary>
        public string ChromeExtensions { get; set; }

        /// <summary>
        /// The amount of hours that office docs will live before being cleaned (based on FileListing class reaper)
        /// Set to -1 to disable
        /// </summary>
        public int OfficeDocsMaxAgeInHours { get; set; }

        /// <summary>
        /// Tokens to be replaced within email-content.csv for a more customized storyline
        /// </summary>
        public Dictionary<string, string> EmailContent { get; set; }

        /// <summary>
        /// Client survey values
        /// </summary>
        public SurveySettings Survey { get; set; }

        public ClientResultSettings ClientResults { get; set; }
        public ClientUpdateSettings ClientUpdates { get; set; }
        public EmailSettings Email { get; set; }
        public ContentSettings Content { get; set; }

        /// <summary>
        /// Settable install location for non-standard "c:\program files\" or "c:\program files (x86)\" installs
        /// </summary>
        public string FirefoxInstallLocation { get; set; }

        /// <summary>
        /// Geckodriver depends on at least this version of FF to be used
        /// </summary>
        public int FirefoxMajorVersionMinimum { get; set; }
        public bool EncodeHeaders { get; set; }
        public ListenerSettings Listener { get; set; }

        public ResourceControlSettings ResourceControl { get; set; }

        public class ContentSettings
        {
            public string EmailContent { get; set; }
            public string EmailReply { get; set; }
            public string EmailDomain { get; set; }
            public string EmailOutside { get; set; }
            public string BlogContent { get; set; }
            public string BlogReply { get; set; }
            public string FileNames { get; set; }
            public string Dictionary { get; set; }
        }

        public class ClientResultSettings
        {
            /// <summary>
            /// Is client posting its results to server?
            /// </summary>
            public bool IsEnabled { get; set; }

            public bool IsSecure { get; set; }

            /// <summary>
            /// API URL for client to post activity results, like timeline, health, etc.
            /// </summary>
            public string PostUrl { get; set; }

            /// <summary>
            /// How often should client post results? (this number is the ms sleep between cycles)
            /// </summary>
            public int CycleSleep { get; set; }
        }

        public class ClientUpdateSettings
        {
            /// <summary>
            /// Is client attempting to pull down updates from server?
            /// </summary>
            public bool IsEnabled { get; set; }

            /// <summary>
            /// API URL for client to get updates like timeline, health, etc.
            /// </summary>
            public string PostUrl { get; set; }

            /// <summary>
            /// How often should client poll for updates? (this number is the ms sleep between cycles)
            /// </summary>
            public int CycleSleep { get; set; }
        }

        /// <summary>
        ///     Survey is local information gathering
        /// </summary>
        public class SurveySettings
        {
            public bool IsEnabled { get; set; }
            public bool IsSecure { get; set; }
            public string Frequency { get; set; }
            public string OutputFormat { get; set; }
            public int CycleSleepMinutes { get; set; }
            public string PostUrl { get; set; }
        }

        public class EmailSettings
        {
            public bool SetAccountFromConfig { get; set; }
            public bool SetAccountFromLocal { get; set; }
            public bool SaveToOutbox { get; set; }
            public bool SetForcedSendReceive { get; set; }
            public int RecipientsToMin { get; set; }
            public int RecipientsToMax { get; set; }
            public int RecipientsCcMin { get; set; }
            public int RecipientsCcMax { get; set; }
            public int RecipientsBccMin { get; set; }
            public int RecipientsBccMax { get; set; }
            public int RecipientsOutsideMin { get; set; }
            public int RecipientsOutsideMax { get; set; }
            public string EmailDomainSearchString { get; set; }
        }

        public class ListenerSettings
        {
            /// <summary>
            /// Set to -1 to disable
            /// </summary>
            public int Port { get; set; }
        }

        public class ResourceControlSettings
        {
            public bool ManageProcesses { get; set; }

            public ResourceControlSettings()
            {
                this.ManageProcesses = true;
            }
        }
    }

    public class ClientConfigurationLoader
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private static ClientConfiguration _conf;

        private ClientConfigurationLoader()
        {
        }

        public static ClientConfiguration Config
        {
            get
            {
                if (_conf == null)
                {
                    var file = ApplicationDetails.ConfigurationFiles.Application;
                    var raw = File.ReadAllText(file);
                    _conf = JsonConvert.DeserializeObject<ClientConfiguration>(raw);

                    _log.Debug($"App config loaded successfully: {file}");
                }

                return _conf;
            }
        }

        public static void UpdateConfigurationWithEnvVars()
        {
            try
            {
                var baseurl = Environment.GetEnvironmentVariable("BASE_URL");
                if (string.IsNullOrEmpty(baseurl)) return;

                var filePath = ApplicationDetails.ConfigurationFiles.Application;
                var raw = File.ReadAllText(filePath);
                var conf = JsonConvert.DeserializeObject<ClientConfiguration>(raw);

                var uri = new Uri(Config.IdUrl);
                conf.IdUrl = $"{baseurl}{uri.AbsolutePath}";

                uri = new Uri(Config.ClientResults.PostUrl);
                conf.ClientResults.PostUrl = $"{baseurl}{uri.AbsolutePath}";

                uri = new Uri(Config.Survey.PostUrl);
                conf.Survey.PostUrl = $"{baseurl}{uri.AbsolutePath}";

                uri = new Uri(Config.ClientUpdates.PostUrl);
                conf.ClientUpdates.PostUrl = $"{baseurl}{uri.AbsolutePath}";

                File.WriteAllText(filePath, JsonConvert.SerializeObject(conf, Formatting.Indented));

                Console.WriteLine($"Updating base configuration... BASE_URL is: {baseurl}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception updating config with env vars: {e}");
            }   
        }
    }
}