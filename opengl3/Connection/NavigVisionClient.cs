using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Emgu.CV;

namespace opengl3
{
    public class ExtPose
    {
        public long Frame; public int Ind; public bool Visible; public double Rms;
        public Matrix<double> M; public DateTime Time;
    }

    public class NavigVisionClient : TcpClientWrapper
    {
        readonly StringBuilder _buf = new StringBuilder();
        readonly object _lock = new object();
        readonly Dictionary<int, ExtPose> _poses = new Dictionary<int, ExtPose>();

        protected override void OnMessageReceived(string chunk)
        {
            _buf.Append(chunk);
            var all = _buf.ToString();
            int k = all.LastIndexOf('\n');
            if (k < 0) return;
            _buf.Clear();
            _buf.Append(all, k + 1, all.Length - k - 1);
            foreach (var line in all.Substring(0, k).Split('\n'))
                ParseLine(line.Trim());
        }

        // P;frame;ind;0   или   P;frame;ind;1;rms;m00;m01;m02;m03;m10;...;m23
        void ParseLine(string line)
        {
            var p = line.Split(';');
            if (p.Length < 4 || p[0] != "P") return;
            var ci = CultureInfo.InvariantCulture;
            if (!long.TryParse(p[1], out long frame) || !int.TryParse(p[2], out int ind)) return;

            var pose = new ExtPose { Frame = frame, Ind = ind, Visible = p[3] == "1", Time = DateTime.UtcNow };
            if (pose.Visible)
            {
                if (p.Length < 17) return;
                pose.Rms = double.Parse(p[4], ci);
                var M = UtilMatr.eye_matr(4);
                for (int r = 0; r < 3; r++)
                    for (int c = 0; c < 4; c++)
                        M[r, c] = double.Parse(p[5 + r * 4 + c], ci);
                pose.M = M;
            }

            lock (_lock)
            {
                // старый пакет не перетирает новый (но после перезапуска сервера frame снова с 0)
                if (_poses.TryGetValue(ind, out var old) && old.Frame > frame
                    && (pose.Time - old.Time).TotalSeconds < 1) return;
                _poses[ind] = pose;
            }
        }

        /// Последняя поза инструмента или null, если не виден или данные устарели
        public ExtPose Get(int ind, int max_age_ms = 100)
        {
            lock (_lock)
            {
                if (!_poses.TryGetValue(ind, out var p)) return null;
                if (!p.Visible || (DateTime.UtcNow - p.Time).TotalMilliseconds > max_age_ms) return null;
                return p;    // M не изменяется после создания — отдавать безопасно
            }
        }
    }
}