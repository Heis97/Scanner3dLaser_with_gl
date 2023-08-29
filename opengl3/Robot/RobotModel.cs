using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace opengl3
{
    class RobotModel
    {
        RobLine _RobLine;
        TCPserver _TCPserver;
        double _time_cur = 0;
        bool _isMove = false;
        public double _time_last = 0;
        const int _t_per = 10;
        const double _msToSec = 0.001;
        TimerCallback tm;
        Thread server_thread;
        Timer timer;
        public RobotModel(RobotFrame start_frame, int port)
        {
            _TCPserver = new TCPserver(port);
            server_thread = new Thread(_TCPserver.startServer);
            server_thread.Start();
            _RobLine = new RobLine(this, start_frame, _msToSec);
             tm = new TimerCallback(compPosic);
            timer = new Timer(tm, 0, 0, _t_per);
        }

        public void move(RobotFrame RobotFrame,double veloc, double acsel)
        {
            //Console.WriteLine("--------------------");
            //Console.WriteLine("LAST_TIME1: " + _time_last);
            //Console.WriteLine("CURR_TIME1: " + _time_cur);
            if (_time_last< _msToSec * _time_cur)
            {
                _time_last =_msToSec* _time_cur;
            }

            _time_last =_RobLine.addMove(this, RobotFrame, veloc, acsel);          
           
           // Console.WriteLine("LAST_TIME2: " + _time_last);
            _isMove = true;
        }
        public void sendMes(string mes)
        {
            _TCPserver.pushBuffer(mes);
        }
        public string recieveMes()
        {
           return  _TCPserver.getBuffer();
        }
        void compPosic(object obj)
        {
            _time_cur += _t_per;
            if(_isMove == true)
            {
                _RobLine.compLineMove(_time_cur);
            }
            //Console.WriteLine(_RobotFrame.x+ " "+ _RobotFrame.y + " " + _RobotFrame.z + " " );
            Console.WriteLine(_time_cur + " " + _RobLine?._RobotFrame?.ToStr());
        }
        
        public RobotFrame getFrame()
        {
            //Console.WriteLine("ROB_FRAME: "+_RobotFrame);
            return _RobLine._RobotFrame;
        }
        public double getTimeCur()
        {
            return _time_cur;
        }

    }

    class RobLine
    {
        RobotFrame _firstFrame;
        RobotFrame _RobotFrameEnd;
        RobotFrame _RobotFrameBegin;
        public RobotFrame _RobotFrame;
        RobotFrame _lastFrame;

        SimpleMove _simpleMove;
        Vector3d_GL _vectorPosition;
        Vector3d_GL _vectorRotation;
        double _veloc;
        double _acsel;
        double _time_start;
        double _dist;
        double _correction;
        
        public RobLine(RobotModel robotModel, RobotFrame RobotFrame, double correction)
        {
            _correction = correction;           
            _simpleMove = new SimpleMove();
            _lastFrame = RobotFrame.Clone();
            _firstFrame = _lastFrame.Clone();
           // Console.WriteLine("Last_FRAME: "+ _lastFrame);
        }
        public double addMove(RobotModel robotModel, RobotFrame RobotFrame, double veloc, double acsel)
        {
            _RobotFrameEnd = RobotFrame;
            _RobotFrameBegin = _lastFrame.Clone();
            _RobotFrame = robotModel.getFrame();
            _veloc = veloc;
            _acsel = acsel;
            _time_start = robotModel._time_last;
           // Console.WriteLine("TIME_L_1: " + _time_start);
           // Console.WriteLine("B_E: " + _RobotFrameBegin+ " | "+_RobotFrameEnd);
            _dist = RobotFrame.dist(_RobotFrameBegin,_RobotFrameEnd);

            _vectorPosition = new Vector3d_GL(_RobotFrameBegin.get_pos(), _RobotFrameEnd.get_pos());
           // _vectorPosition.normalize();

            _vectorRotation = new Vector3d_GL(_RobotFrameBegin.get_rot(), _RobotFrameEnd.get_rot());
           // _vectorRotation.normalize();

            _lastFrame = _RobotFrameEnd.Clone();

            var t_move = _simpleMove.addMove(_veloc, _acsel, _dist, _time_start, _vectorPosition, _vectorRotation);


            return t_move;
        }
        public void compLineMove(double time_cur)
        {
            var S = _simpleMove.compCoord(_correction * time_cur);
            _RobotFrame = _firstFrame + S;
        }
       
    }
    public class SimpleMove
    {
        enum ShapeMove { Triangle, Trapezoid }
        enum ShapeSegment { Acceleration, Deceleration, Uniform }
        double _veloc;
        double _acsel;
        double _dist;
        ShapeMove _shapeMove;
        Vector3d_GL _vect_pos;
        Vector3d_GL _vect_rot;
        double S_ac_m, S_unm;
        double t_ac_m,t_un_m;
        double _t_start,_t_end;
        
       
        List<SegmentMove> shapeSegments;
        public SimpleMove()
        {
            shapeSegments = new List<SegmentMove>();
        }
        public double addMove(double veloc,double acsel, double dist, double t_cur, Vector3d_GL vec_pos, Vector3d_GL vec_rot)
        {
            _veloc = veloc;
            _acsel = acsel;
            _dist = dist;
            _vect_pos = vec_pos;
            _vect_rot = vec_rot;
            //Console.WriteLine("DIST "+_dist);
            _t_start = t_cur;
            //Console.WriteLine("T_ST_1:  " + _t_start);
            _shapeMove = checkShape();
            shapeSegments.AddRange(createPlane());
            return _t_end;
        }
        
        ShapeMove checkShape()
        {
            var half_dist = _dist / 2;
            var t = _veloc / _acsel;
            var triang = _veloc * t / 2;
           // Console.WriteLine("HALF "+half_dist);
           // Console.WriteLine("HALF_tr " + triang);
            if (half_dist > triang)
            {
                return ShapeMove.Trapezoid;
            }
            else
            {
                return ShapeMove.Triangle;
            }
        }
        public RobotFrame compCoord(double t_cur)
        {
            Vector3d_GL rasst = new Vector3d_GL(0,0,0);
            Vector3d_GL rot = new Vector3d_GL(0, 0, 0);
            foreach (var seg in shapeSegments)
            {
                rasst += seg.vec_pos* seg.compSegment(t_cur);
                rot += seg.vec_rot * seg.compSegment(t_cur);
                //Console.WriteLine(seg.compSegment(t_cur) + " " + seg.dist_seg);
            }
            return new RobotFrame(rasst.x, rasst.y, rasst.z,
                rot.x, rot.y, rot.z);
        }
        List<SegmentMove> createPlane()
        {
            var _shapeSegments = new List<SegmentMove>();
            
            if(_shapeMove == ShapeMove.Trapezoid)
            {
                //Console.WriteLine("TRAPEZ");
                t_ac_m = _veloc / _acsel;
                S_ac_m = _acsel * t_ac_m * t_ac_m;
                S_unm = _dist - S_ac_m;
               // Console.WriteLine("Sa_Sun " + S_ac_m + " " + S_unm);
                t_un_m = S_unm / _veloc;
                _t_end = _t_start + 2 * t_ac_m + t_un_m;

                var t_all = _t_end - _t_start;
                var part1 = t_ac_m/ t_all ;
                var part2 = t_un_m / t_all;
                var part3 = t_ac_m / t_all;
                _shapeSegments.Add(new SegmentMove(_t_start, _t_start + t_ac_m, _veloc, _acsel, _vect_pos*part1, _vect_rot * part1, SegmentMove.ShapeSegment.Acceleration));
                _shapeSegments.Add(new SegmentMove(_t_start + t_ac_m, _t_start + t_ac_m + t_un_m, _veloc, _acsel, _vect_pos * part2, _vect_rot * part2, SegmentMove.ShapeSegment.Uniform));
                _shapeSegments.Add(new SegmentMove(_t_start + t_ac_m + t_un_m, _t_start + 2*t_ac_m + t_un_m, _veloc, _acsel, _vect_pos * part3, _vect_rot* part3, SegmentMove.ShapeSegment.Deceleration));
                
            }
            else
            {
               // Console.WriteLine("TRIANG");
                var _t_half = Math.Sqrt(_dist / _acsel) ;
                _veloc = _acsel * _t_half;
                //Console.WriteLine("_vel_max = " + _veloc);
                _t_end = _t_start + 2 * _t_half;
                _shapeSegments.Add(new SegmentMove(_t_start, _t_start + _t_half,_veloc,_acsel, _vect_pos, _vect_rot, SegmentMove.ShapeSegment.Acceleration));
                _shapeSegments.Add(new SegmentMove(_t_start + _t_half, _t_start + 2*_t_half, _veloc, _acsel, _vect_pos, _vect_rot, SegmentMove.ShapeSegment.Deceleration));
            }
            return _shapeSegments;
        }

        
    }
    public class SegmentMove
    {
        public enum ShapeSegment { Acceleration, Deceleration, Uniform }
        double t_start;
        double t_stop;
        double t_all;
        double vel_max;
        double acs;
        public double dist_seg;
        ShapeSegment shapeSegment;
        public Vector3d_GL vec_pos;
        public Vector3d_GL vec_rot;
        public SegmentMove(double _t_start,double _t_stop,double _vel_max,double _acs,Vector3d_GL vec_pos, Vector3d_GL vec_rot, ShapeSegment _shapeSegment)
        {
            t_start = _t_start;
            t_stop = _t_stop;
            t_all = t_stop - t_start;
            //Console.WriteLine("T_STR_STP " + t_start + " " + t_stop);
            vel_max = _vel_max;
            acs = _acs;
            this.vec_pos = vec_pos;
            this.vec_rot = vec_rot;
            shapeSegment = _shapeSegment;
            dist_seg = compDist(t_stop - t_start, shapeSegment);
        }

        public double compSegment(double t_cur)
        {          
            if(t_cur>t_stop)
            {
                t_cur = t_stop;
            }
            if (t_cur < t_start)
            {
                t_cur = t_start;
            }
            var _t_cur = t_cur - t_start;
            var dist = compDist(_t_cur, shapeSegment);
            return dist/ dist_seg;
        }
        double compDist(double t_cur, ShapeSegment shapeSegment)
        {

            switch (shapeSegment)
            {
                case ShapeSegment.Acceleration:
                    {
                        var vel_cur = acs * t_cur;
                        return vel_cur * t_cur / 2;
                    }
                case ShapeSegment.Uniform:
                    return vel_max * t_cur;
                case ShapeSegment.Deceleration:
                    {
                        var vel_cur = vel_max - acs * t_cur;
                        return (vel_max * t_all / 2) - (vel_cur * (t_all - t_cur)) / 2;
                    }
                default:
                    return 0;
            }
        }

    }
}
