using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Salix.AspNetCore.Utilities
{
    /// <summary>
    /// Extensions to string values for obfuscation
    /// </summary>
    public static class ConfigurationObfuscator
    {
        public static string ObfuscateSqlConnectionString(this string sqlConnectionString, bool partially = false)
        {
            MatchCollection parts = Regex.Matches(sqlConnectionString, @"(?<key>[^=;,]+)=(?<val>[^;,]+(,\d+)?)", RegexOptions.IgnoreCase);

            string obfuscatedResult = string.Empty;
            bool isLocalServer = false;
            foreach (Match part in parts)
            {
                string key = part.Groups["key"].Value.Trim();
                string value = part.Groups["val"].Value.Trim();

                switch (key.ToUpperInvariant())
                {
                    case "DATA SOURCE":
                    case "SERVER":
                    case "ADDRESS":
                    case "ADDR":
                    case "NETWORK ADDRESS":
                        if (value.StartsWith("(localdb)", StringComparison.OrdinalIgnoreCase)
                            || value.StartsWith(".\\SQLExpress", StringComparison.OrdinalIgnoreCase)
                            || value.ToUpper(CultureInfo.InvariantCulture).Contains("LOCALHOST"))
                        {
                            isLocalServer = true;
                            obfuscatedResult += $"Server={value};";
                            break;
                        }

                        if (!partially)
                        {
                            obfuscatedResult += "Server=[hidden];";
                            break;
                        }

                        // address,port
                        string port = string.Empty;
                        if (value.Contains(","))
                        {
                            string[] split = value.Split(',');
                            if (split.Length == 2)
                            {
                                obfuscatedResult += $"Server={HideValuePartially(split[0])},{split[1]};";
                                break;
                            }
                        }

                        // server\instance
                        if (value.Contains("\\"))
                        {
                            string[] split = value.Split('\\');
                            if (split.Length == 2)
                            {
                                obfuscatedResult += $"Server={HideValuePartially(split[0])}\\{HideValuePartially(split[1])};";
                                break;
                            }
                        }

                        obfuscatedResult += $"Server={HideValuePartially(value)};";
                        break;
                    case "INITIAL CATALOG":
                    case "DATABASE":
                        if (isLocalServer)
                        {
                            obfuscatedResult += $"Database={value};";
                            break;
                        }

                        if (!partially)
                        {
                            obfuscatedResult += "Database=[hidden];";
                            break;
                        }

                        obfuscatedResult += $"Database={HideValuePartially(value)};";
                        break;
                    case "USER ID":
                    case "UID":
                        if (isLocalServer)
                        {
                            obfuscatedResult += $"User Id={value};";
                            break;
                        }

                        if (!partially)
                        {
                            obfuscatedResult += "User Id=[hidden];";
                            break;
                        }

                        obfuscatedResult += $"User Id={HideValuePartially(value)};";
                        break;
                    case "PASSWORD":
                    case "PWD":
                        if (isLocalServer)
                        {
                            obfuscatedResult += $"Password={value};";
                            break;
                        }

                        obfuscatedResult += "Password=[hidden];";
                        break;
                    default:
                        obfuscatedResult += $"{key}={value};";
                        break;
                }
            }

            return obfuscatedResult;
        }

        /// <summary>
        /// Hides the string value partially, replacing middle part of string (bit more tha half of it) with asterisks (*).
        /// Example: "SomeServer" = "So******er"
        /// </summary>
        /// <param name="initialValue">The initial string value to obfuscate.</param>
        public static string HideValuePartially(this string initialValue)
        {
            if (initialValue.Length < 6)
            {
                return "[hidden]";
            }

            // email address
            if (initialValue.Split('@').Length - 1 == 1 && initialValue.Contains("."))
            {
                try
                {
                    var email = new MailAddress(initialValue);
                    string[] hostParts = email.Host.Split('.');
                    string hostName = string.Join(".", hostParts, 0, hostParts.Length - 1);
                    if (new HashSet<string> { "OUTLOOK", "YANDEX", "HOTMAIL", "ICLOUD" }.Contains(hostName.ToUpperInvariant()))
                    {
                        hostName = "***";
                    }
                    string topDomain = hostParts[hostParts.Length - 1];
                    return $"{HideValuePartially(email.User)}@{HideValuePartially(hostName)}.{topDomain}";
                }
                catch
                {
                    // It is not an e-mail
                }
            }

            // IP address
            if (Regex.Match(initialValue, @"\A(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\z") != Match.Empty)
            {
                MatchCollection ipParts = Regex.Matches(initialValue,
                    @"\b(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\b");
                if (ipParts[0].Groups.Count == 5) // whole + 4 parts
                {
                    string obfuscatedIp = ipParts[0].Groups[1].Value.Length switch
                    {
                        3 => ipParts[0].Groups[1].Value.Substring(0, 2) + "*",
                        2 => ipParts[0].Groups[1].Value.Substring(0, 1) + "*",
                        _ => "*"
                    };
                    obfuscatedIp += ".*.*.";
                    obfuscatedIp += ipParts[0].Groups[4].Value.Length switch
                    {
                        3 => "*" + ipParts[0].Groups[4].Value.Substring(1, 2),
                        2 => "*" + ipParts[0].Groups[4].Value.Substring(1, 1),
                        _ => "*"
                    };

                    return obfuscatedIp;
                }
            }

            // Text middle part is replaced with * ("SomeServer" = "So******er")
            int replaceablePartLength = (initialValue.Length / 2) + 1;
            int lastThirdLength = ((initialValue.Length - replaceablePartLength) / 2) + ((initialValue.Length - replaceablePartLength) % 2);
            int firstThirdLength = initialValue.Length - replaceablePartLength - lastThirdLength;
            if (firstThirdLength > 3)
            {
                firstThirdLength = 3;
            }

            if (lastThirdLength > 3)
            {
                lastThirdLength = 3;
            }

            return initialValue.Substring(0, firstThirdLength) + new string('*', 5) + initialValue.Substring(firstThirdLength + replaceablePartLength, lastThirdLength);
        }
    }
}
