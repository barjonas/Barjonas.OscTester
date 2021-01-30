using Rug.Osc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Extensions.Logging;

namespace VirtualSeat.DownlinkLauncher.Model
{
    internal class OscWrapper : IDisposable
    {
        private readonly ILogger _logger;
        private readonly OscReceiver _server;

        public OscWrapper(IPEndPoint receiveEndpoint, ILogger logger)
        {
            _logger = logger;
            _server = new OscReceiver(IPAddress.Any, receiveEndpoint.Address, receiveEndpoint.Port);
            _server.Connect();
            _logger.LogInformation("Listening for OSC commands on {0}", receiveEndpoint);
            Task.Run(() => ListenLoop());
        }

        private void ListenLoop()
        {
            try
            {
                while (_server.State != OscSocketState.Closed)
                {
                    if (_server.State == OscSocketState.Connected)
                    {
                        // get the next message 
                        // this will block until one arrives or the socket is closed
                        OscPacket packet = _server.Receive();
                        if (packet.Error != OscPacketError.None)
                        {
                            continue;
                        }
                        ProcessOscPacket(string.Empty, packet);
                    }
                }
            }
            catch (Exception ex)
            {
                // if the socket was connected when this happens
                // then tell the user
                if (_server.State == OscSocketState.Connected)
                {
                    _logger.LogError(ex, "Exception in OSC listen loop");
                }
            }
        }

        private void ProcessOscPacket(string bundlePrefix, OscPacket packet)
        {
            switch (packet)
            {
                case OscBundle b:
                    _logger.LogTrace($"┌Start bundle");
                    foreach (OscPacket m in b)
                    {
                        try
                        {
                            ProcessOscPacket(bundlePrefix + "│", m);
                        }
                        catch { }
                    }
                    _logger.LogTrace($"└End bundle");
                    break;
                case OscMessage m:
                    try
                    {
                        ProcessOscMessage(bundlePrefix, m);
                    }
                    catch { }
                    break;
            }
        }

        private void ProcessOscMessage(string bundlePrefix, OscMessage message)
        {
            _logger.LogInformation($"{bundlePrefix}{message.Address}");
            int i = 0;
            foreach (object? value in message)
            {
                LogValue(bundlePrefix, string.Empty, i.ToString(), value);
                i++;
            }
        }

        private void LogValue(string bundlePrefix, string arrayPrefix, string index, object? value)
        {
            string prefix = $"{bundlePrefix}{arrayPrefix}";
            switch (value)
            {
                case Array a:
                    _logger.LogTrace($"{prefix}╓{index}: Array (count {a.Length})");
                    arrayPrefix += '║';
                    int i = 0;
                    foreach (object o in a)
                    {
                        LogValue(bundlePrefix, arrayPrefix, $"{index}.{i}", o);
                        i++;
                    }
                    _logger.LogTrace($"{prefix}╙");
                    break;
                case string s:
                    _logger.LogTrace($"{prefix}{index}: string {s}");
                    break;
                case int nt:
                    _logger.LogTrace($"{prefix}{index}: int {nt}");
                    break;
                case float f:
                    _logger.LogTrace($"{prefix}{index}: float {f}");
                    break;
                case double d:
                    _logger.LogTrace($"{prefix}{index}: double {d}");
                    break;
                case bool b:
                    _logger.LogTrace($"{prefix}{index}: bool {b}");
                    break;
            }
        }

        public void Dispose()
        {
            _server.Close();
        }
    }
}
