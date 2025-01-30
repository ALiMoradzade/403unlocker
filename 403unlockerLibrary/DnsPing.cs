﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Security.Policy;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Diagnostics;
using DnsClient;
using System.Security.Cryptography;
using System.Threading;
using System.Text.RegularExpressions;
using DnsClient.Protocol;
using HtmlAgilityPack;

namespace _403unlockerLibrary
{
    public class DnsPing : DnsProvider
    {
        private int status;
        private long latency = 0;

        public string Name
        {
            get => base.Name;
        }

        public string DNS
        {
            get => base.DNS;
        }

        public int Status
        {
            get => status;
        }

        public long Latency
        {
            get => latency;
        }

        public DnsPing(string provider, string dns)
        {
            base.Name = provider;
            base.DNS = dns;
        }

        public DnsPing(DnsProvider dnsRecord)
        {
            base.Name = dnsRecord.Name;
            base.DNS = dnsRecord.DNS;
        }

        public static bool IsValidUrl(string url)
        {
            if (Regex.IsMatch(url, @"^(www\.)?[a-zA-Z0-9]+\.[a-zA-Z]+$")) return true;
            return false;
        }

        public async Task GetPing(int timeOut_ms)
        {
            using (Ping ping = new Ping())
            {
                try
                {
                    PingReply reply = await ping.SendPingAsync(IPAddress.Parse(DNS), timeOut_ms);
                    latency = reply.RoundtripTime;
                    status = (int)HttpStatusCode.OK;
                }
                catch (TaskCanceledException)
                {
                    latency = 0;
                    status = (int)HttpStatusCode.RequestTimeout;
                }
            }
        }

        public async Task GetPing(string hostName, int timeOut_s)
        {
            try
            {
                // seeking for IPs
                string[] resolvedIP = await ResolveDNS(DNS, hostName, timeOut_s);
                if (resolvedIP.Length == 0)
                {
                    throw new DnsResponseException();
                }

                var httpreq = await HttpRequestAsWeb(resolvedIP.First(), timeOut_s);
                status = (int)HttpStatusCode.OK;
            }
            catch (HttpRequestException)
            {
                latency = 0;
                status = (int)HttpStatusCode.ServiceUnavailable;
            }
            catch (DnsResponseException)
            {
                latency = 0;
                status = (int)HttpStatusCode.NotFound;
            }
            catch (TaskCanceledException)
            {
                latency = 0;
                status = (int)HttpStatusCode.RequestTimeout;
            }
        }

        public async static Task<HtmlDocument> HttpRequest(string url, int timeOut_s = 5)
        {
            using (var handler = new HttpClientHandler())
            {
                handler.UseCookies = true;
                using (HttpClient client = new HttpClient(handler))
                {
                    // content to accept in response
                    client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

                    // OS, browser version, html layout rendering engine
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:133.0) Gecko/20100101 Firefox/133.0");

                    client.Timeout = TimeSpan.FromSeconds(timeOut_s);

                    // get html as string
                    string htmlString = await client.GetStringAsync(url);

                    var htmlDocument = new HtmlDocument();

                    // make html to tree
                    htmlDocument.LoadHtml(htmlString);
                    return htmlDocument;
                }
            }
        }
        public async static Task<HttpResponseMessage> HttpResponse(string url, int timeOut_s = 2)
        {
            using (var handler = new HttpClientHandler())
            {
                handler.UseCookies = true;
                using (HttpClient client = new HttpClient(handler))
                {
                    // content to accept in response
                    client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

                    // OS, browser version, html layout rendering engine
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:133.0) Gecko/20100101 Firefox/133.0");

                    client.Timeout = TimeSpan.FromSeconds(timeOut_s);

                    // get html response
                    HttpResponseMessage htmlResponse = await client.GetAsync(url);
                    return htmlResponse;
                }
            }
        }

        public async static Task<HtmlDocument> HttpRequestAsWeb(string url, int timeOut_s)
        {
            HtmlWeb web = new HtmlWeb();
            web.Timeout = timeOut_s;
            var htmlDoc = await web.LoadFromWebAsync(url);
            return htmlDoc;
        }
            
       
        public async static Task<string[]> ResolveDNS(string customeDNS, string hostName, int timeOut_s = 5)
        {
            // initialize settings
            var options = new LookupClientOptions(IPAddress.Parse(customeDNS))
            {
                Timeout = TimeSpan.FromSeconds(timeOut_s),
                UseCache = false,
                ThrowDnsErrors = true,
                ContinueOnDnsError = false
            };
            // apply settings to query
            var lookup = new LookupClient(options);
            // query DNS server
            var result = await lookup.QueryAsync(hostName, QueryType.A);

            string[] resolvedIP = result.Answers.OfType<ARecord>().Select(x => $"http://{x.Address}").ToArray();
            return resolvedIP;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
