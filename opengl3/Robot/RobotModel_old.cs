using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace rob_mod_old
{
    /*
    class RobotModel
    {
        robFrame _robFrame;
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
        public RobotModel(robFrame start_frame, int port)
        {
            _TCPserver = new TCPserver(port);
            server_thread = new Thread(_TCPserver.startServer);
            server_thread.Start();
            _robFrame = start_frame;
            _RobLine = new RobLine(this, _msToSec);
           
            tm = new TimerCallback(compPosic);
            timer = new Timer(tm, 0, 0, _t_per);
        }

        public void move(robFrame robFrame,double veloc, double acsel)
        {
            //Console.WriteLine("--------------------");
            //Console.WriteLine("LAST_TIME1: " + _time_last);
            //Console.WriteLine("CURR_TIME1: " + _time_cur);
            if (_time_last< _msToSec * _time_cur)
            {
                _time_last =_msToSec* _time_cur;
            }

            _time_last =_RobLine.addMove(this, robFrame, veloc, acsel);          
           
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
            //Console.WriteLine(_robFrame.x+ " "+ _robFrame.y + " " + _robFrame.z + " " );
            Console.WriteLine(_time_cur + " " + _robFrame.x+ " " + _robFrame.y);
        }
        
        public robFrame getFrame()
        {
            //Console.WriteLine("ROB_FRAME: "+_robFrame);
            return _robFrame;
        }
        public double getTimeCur()
        {
            return _time_cur;
        }

    }

    class RobLine
    {
        robFrame _firstFrame;
        robFrame _robFrameEnd;
        robFrame _robFrameBegin;
        robFrame _robFrame;
        robFrame _lastFrame;

        SimpleMove _simpleMove;
        Vector3d_GL _vectorFrame;
        double _veloc;
        double _acsel;
        double _time_start;
        double _dist;
        double _correction;
        
        public RobLine(RobotModel robotModel, double correction)
        {
            _correction = correction;           
            _simpleMove = new SimpleMove();
            _lastFrame = new robFrame(robotModel.getFrame());
            _firstFrame = new robFrame(_lastFrame);
           // Console.WriteLine("Last_FRAME: "+ _lastFrame);
        }
        public double addMove(RobotModel robotModel, robFrame robFrame, double veloc, double acsel)
        {
            _robFrameEnd = robFrame;
            _robFrameBegin = new robFrame(_lastFrame);
            _robFrame = robotModel.getFrame();
            _veloc = veloc;
            _acsel = acsel;
            _time_start = robotModel._time_last;
           // Console.WriteLine("TIME_L_1: " + _time_start);
           // Console.WriteLine("B_E: " + _robFrameBegin+ " | "+_robFrameEnd);
            _dist = _robFrameBegin.rasstTo(_robFrameEnd);
            _vectorFrame = new Vector3d_GL(_robFrameBegin.pos(), _robFrameEnd.pos());
            
            _vectorFrame.normalize();
            _lastFrame = new robFrame(_robFrameEnd);
            return _simpleMove.addMove(_veloc, _acsel, _dist, _time_start,_vectorFrame);
        }
        public void compLineMove(double time_cur)
        {
            var S = _simpleMove.compCoord(_correction * time_cur);
            
            _robFrame.x = _firstFrame.x + S.x;
            _robFrame.y = _firstFrame.y + S.y;
            _robFrame.z = _firstFrame.z + S.z;
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
        Vector3d_GL _vector;
        double S_ac_m, S_unm;
        double t_ac_m,t_un_m;
        double _t_start,_t_end;
        
       
        List<SegmentMove> shapeSegments;
        public SimpleMove()
        {
            shapeSegments = new List<SegmentMove>();
        }
        public double addMove(double veloc,double acsel, double dist, double t_cur, Vector3d_GL vector3D_GL)
        {
            _veloc = veloc;
            _acsel = acsel;
            _dist = dist;
            _vector = vector3D_GL;
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
        public Vector3d_GL compCoord(double t_cur)
        {
            Vector3d_GL rasst = new Vector3d_GL(0,0,0);
            foreach (var seg in shapeSegments)
            {
                var ras = seg.compSegment(t_cur);
                //if(ras>0)
                rasst += ras;
            }
            return rasst;     
        }
        List<SegmentMove> createPlane()
        {
            var _shapeSegments = new List<SegmentMove>();
            
            if(_shapeMove == ShapeMove.Trapezoid)
            {
                //Console.WriteLine("TRAP");
                t_ac_m = _veloc / _acsel;
                S_ac_m = _acsel * t_ac_m * t_ac_m;
                S_unm = _dist - S_ac_m;
               // Console.WriteLine("Sa_Sun " + S_ac_m + " " + S_unm);
                t_un_m = S_unm / _veloc;
                _t_end = _t_start + 2 * t_ac_m + t_un_m;
                _shapeSegments.Add(new SegmentMove(_t_start, _t_start + t_ac_m, _veloc, _acsel, _vector, SegmentMove.ShapeSegment.Acceleration));
                _shapeSegments.Add(new SegmentMove(_t_start + t_ac_m, _t_start + t_ac_m + t_un_m, _veloc, _acsel, _vector, SegmentMove.ShapeSegment.Uniform));
                _shapeSegments.Add(new SegmentMove(_t_start + t_ac_m + t_un_m, _t_start + 2*t_ac_m + t_un_m, _veloc, _acsel, _vector, SegmentMove.ShapeSegment.Deceleration));
            }
            else
            {
               // Console.WriteLine("TRIANG");
                var _t_half = Math.Sqrt(_dist / _acsel) ;
                _veloc = _acsel * _t_half;
                //Console.WriteLine("_vel_max = " + _veloc);
                _t_end = _t_start + 2 * _t_half;
                _shapeSegments.Add(new SegmentMove(_t_start, _t_start + _t_half,_veloc,_acsel, _vector, SegmentMove.ShapeSegment.Acceleration));
                _shapeSegments.Add(new SegmentMove(_t_start + _t_half, _t_start + 2*_t_half, _veloc, _acsel, _vector, SegmentMove.ShapeSegment.Deceleration));
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
        ShapeSegment shapeSegment;
        Vector3d_GL vector;
        public SegmentMove(double _t_start,double _t_stop,double _vel_max,double _acs,Vector3d_GL vector3D_GL, ShapeSegment _shapeSegment)
        {
            t_start = _t_start;
            t_stop = _t_stop;
            t_all = t_stop - t_start;
            //Console.WriteLine("T_STR_STP " + t_start + " " + t_stop);
            vel_max = _vel_max;
            acs = _acs;
            vector = vector3D_GL;
            shapeSegment = _shapeSegment;
        }
        public Vector3d_GL compSegment(double t_cur)
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
            switch (shapeSegment)
            {
                case ShapeSegment.Acceleration:
                    //Console.WriteLine("ACS: " + compDistAcs(_t_cur));
                    return vector* compDistAcs(_t_cur);
                case ShapeSegment.Deceleration:
                    //Console.WriteLine("DES: " + compDistDec(_t_cur));
                    return vector * compDistDec(_t_cur);
                case ShapeSegment.Uniform:
                    //Console.WriteLine("UNI: " + compDistUni(_t_cur));
                    return vector* compDistUni(_t_cur);
                default:
                    return new Vector3d_GL(0,0,0);
            }
        }
        double compDistAcs(double t_cur)
        {
            
            var vel_cur = acs * t_cur;
            return vel_cur * t_cur / 2;
        }

        double compDistUni(double t_cur)
        {
            return vel_max* t_cur;
        }

        double compDistDec(double t_cur)
        {
            var vel_cur = vel_max - acs*t_cur;
            return (vel_max * t_all / 2) - (vel_cur*(t_all-t_cur))/2;
        }
    }
    */
}
