using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using Yhsb.Util;

namespace Yhsb.Net
{
    public class HttpSocket : IDisposable
    {
        readonly string _host;
        readonly int _port;
        TcpClient _client;
        NetworkStream _stream;
        readonly Encoding _encoding;

        public string Url => $"{_host}:{_port}";

        public HttpSocket(string host, int port, string encoding = "utf-8")
        {
            _host = host;
            _port = port;
            _encoding = Encoding.GetEncoding(encoding);

            _client = new TcpClient(host, port);
            _stream = _client.GetStream();
        }

        public void Dispose()
        {
            if (_stream != null)
            {
                _stream.Close();
                _stream = null;
            }
            if (_client != null)
            {
                _client.Close();
                _client = null;
            }
        }

        public void Write(String s) => _stream.Write(_encoding.GetBytes(s));

        public (byte[] buffer, int length) Read(int size)
        {
            var buffer = new byte[size];
            var length = _stream.Read(buffer, 0, size);
            return (buffer, length);
        }

        public string ReadLine()
        {
            int c, n;
            using var stream = new MemoryStream(512);
            while (true)
            {
                c = _stream.ReadByte();
                if (c == -1) // end of stream
                {
                    break;
                }
                else if (c == 0x0D) // \r
                {
                    n = _stream.ReadByte();
                    if (n == -1)
                    {
                        stream.WriteByte((byte)c);
                        break;
                    }
                    else if (n == 0x0A) // \n
                    {
                        break;
                    }
                    else
                    {
                        stream.WriteByte((byte)c);
                        stream.WriteByte((byte)n);
                    }
                }
                else
                {
                    stream.WriteByte((byte)c);
                }
            }
            return _encoding.GetString(stream.GetBuffer(), 0, (int)stream.Length);
        }

        public HttpHeader ReadHeader()
        {
            var header = new HttpHeader();
            while (true)
            {
                var line = ReadLine();
                if (line == "") break;
                var i = line.IndexOf(':');
                if (i > 0)
                {
                    header.Add(line.Substring(0, i).Trim(), line.Substring(i + 1).Trim());
                }
            }
            return header;
        }

        public string ReadBody(HttpHeader header = null)
        {
            if (header == null) header = ReadHeader();

            using var stream = new MemoryStream(512);
            void readToStream(int len)
            {
                while (len > 0)
                {
                    (var rec, var rlen) = Read(len);
                    stream.Write(rec, 0, rlen);
                    len -= rlen;
                }
            }

            if (header.TryGetValue("Transfer-Encoding", out var values) && values.Contains("chunked"))
            {
                while (true)
                {
                    var len = Convert.ToInt32(ReadLine(), 16);
                    if (len <= 0)
                    {
                        ReadLine();
                        break;
                    }
                    readToStream(len);
                    ReadLine();
                }
            }
            else if (header.TryGetValue("Content-Length", out values))
            {
                var len = Convert.ToInt32(values[0], 10);
                readToStream(len);
            }
            else
            {
                throw new Exception("Unsupported transfer mode");
            }
            return _encoding.GetString(stream.GetBuffer(), 0, (int)stream.Length);
        }

        public string GetHttp(string path, string encoding = "utf-8")
        {
            var request = new HttpRequest(path, encoding: encoding);
            request.AddHeader("Host", Url)
                .AddHeader("Connection", "keep-alive")
                .AddHeader("Cache-Control", "max-age=0")
                .AddHeader("Upgrade-Insecure-Requests", "1")
                .AddHeader("User-Agent",
                "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36")
                .AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8")
                .AddHeader("Accept-Encoding", "gzip, deflate")
                .AddHeader("Accept-Language", "zh-CN,zh;q=0.9");
            _stream.Write(request.ToArray());
            return ReadBody();
        }
    }

    public class HttpHeader : IEnumerable<(string, string)>
    {
        readonly Dictionary<string, List<string>> _header = new Dictionary<string, List<string>>();

        public List<string> this[string key]
        {
            get { return _header[key.ToLower()]; }
            set { _header[key.ToLower()] = value; }
        }

        public bool TryGetValue(string key, out List<string> values) => _header.TryGetValue(key.ToLower(), out values);


        public void Add(string name, string value)
        {
            var key = name.ToLower();
            if (!_header.ContainsKey(key))
            {
                _header[key] = new List<string>();
            }
            _header[key].Add(value);
        }

        public void Add(HttpHeader header)
        {
            foreach ((var key, var value) in header)
            {
                Add(key, value);
            }
        }

        public IEnumerator<(string, string)> GetEnumerator()
        {
            foreach (var entry in _header)
            {
                foreach (var value in entry.Value)
                {
                    yield return (entry.Key, value);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class HttpRequest
    {
        readonly HttpHeader _header = new HttpHeader();
        readonly MemoryStream _body = new MemoryStream(512);
        readonly string _path, _method;

        readonly Encoding _encoding;

        public HttpRequest(string path, string method = "GET", HttpHeader header = null, string encoding = "utf-8")
        {
            _path = path;
            _method = method;
            _encoding = Encoding.GetEncoding(encoding);
            if (header != null) _header.Add(header);
        }

        public HttpRequest AddHeader(String name, String value)
        {
            _header.Add(name, value);
            return this;
        }

        public HttpRequest AddBody(String body)
        {
            var buffer = _encoding.GetBytes(body);
            _body.Write(buffer, 0, buffer.Length);
            return this;
        }

        public byte[] ToArray()
        {
            using var buffer = new MemoryStream(512);
            buffer.Write(_encoding.GetBytes($"{_method} {_path} HTTP/1.1\r\n"));
            foreach ((var key, var value) in _header)
            {
                buffer.Write(_encoding.GetBytes($"{key}: {value}\r\n"));
            }
            if (_body.Length > 0)
            {
                buffer.Write(_encoding.GetBytes($"content-length: {_body.Length}\r\n"));
            }
            buffer.Write(_encoding.GetBytes("\r\n"));
            if (_body.Length > 0)
            {
                _body.WriteTo(buffer);
            }
            return buffer.ToArray();
        }

        ~HttpRequest() => _body.Dispose();
    }
}