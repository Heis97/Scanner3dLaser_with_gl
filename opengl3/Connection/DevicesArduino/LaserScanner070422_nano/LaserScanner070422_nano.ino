#include <StepDirDriverPoz.h>
#include "timer-api.h"
StepDirDriverPoz shvp(5, 6, 7);
int laser = 2; 
int power = 2;
long int posit = 0;
int moveshvp = 1;
int velshvp = 1;
volatile int laser_cur = 0; 
volatile int laser_dest = 3500; 
volatile int laser_sensor = 0; 


volatile float k_p_p = 4; 
volatile float k_v_p = 50; 
//settings for comp_vel
const float nT = 1000;
const float P = 8;
const float rev = 200*16;


void setup() {
  timer_init_ISR_1KHz(_TIMER1);//shvp
  //timer_init_ISR_1KHz(1);//laser
  shvp.setMode(0, false);
  shvp.setDivider(0);
  DDRB |= B11111111;
  Serial.begin(250000);
}


void loop() {
  decod_main();
  controlMove();

}

void controlLaser(int laser_state, int las_power)
{
  if(laser_state == 1)
  {
     timer_init_ISR(_TIMER2, TIMER_PRESCALER_1_256, 5*las_power);
    PORTB |= 1<<5;
  }   
}

void timer_handle_interrupts(int timer)
{
  if (timer == _TIMER1)
  {
    shvp.control();
   // controlLaser(laser,power);
  }
  if(timer == _TIMER2)
  {
     PORTB &= ~(1<<5);
     timer_stop_ISR(_TIMER2);
  }
}


void laser_sens(int cur, int dest)
{
  if (laser_sensor == 1)
  {
    long int cur_pos = shvp.readPoz();
    int delt = cur - dest;

    cur_pos -=  delt;
    if(delt<5)
    {
      shvp.setDivider(10);
    }
    else
    {
      shvp.setDivider(4);
    }
    shvp.gotopoz(cur_pos);
  }  
}
void move_mot(float vel,int steps = 0)
{
  int dir = 0 ;
  if(vel<0) dir = 1;
  if(vel>0) dir = 2;
  if(vel==0) dir = 0;
  switch(dir)
  {
    case 0: shvp.step(0); break;
    case 1: shvp.step(steps); break;
    case 2: shvp.step(-steps); break;
    default: break;
  }
  int vel_div = comp_vel(vel); 
  shvp.setDivider(vel_div);
}

void laser_sens_step(int cur, int dest)
{
  if (laser_sensor == 1)
    {
    int sign = 0;
    if(cur - dest>0) sign = 1;
    if(cur - dest<0) sign = -1;
    int delt_f_abs = abs(cur - dest);
    float vel_move = delt_f_abs;
    move_mot(k_v_p*sign*vel_move,(int)(k_p_p*delt_f_abs));
  }  
}


int comp_vel(float vel)
{  
   int vel_div = abs((int)((1000*nT*P)/(vel*rev))); 
   return vel_div;
}


void controlMove()
{    
  if(moveshvp == 1 && shvp.readSteps()==0)
  {
    
    if(posit!=shvp.readPoz())
    {
      shvp.gotopoz(posit);  
    }
  }
  else if(moveshvp == 0)
  {
    shvp.step(0);
    moveshvp = 1;
  } 
}
void decod_main()
{
  
  int resp = 0;
  if (Serial.available())
  {
    int byte1 = Serial.read() - 48;
    Serial.println(byte1);
    if (byte1 == 50)
    {
      
      int inserial[8];
      for (int i = 0; i < 6; i++)
      {
        inserial[i] = Serial.read() - 48;
        
        Serial.print(i);
        Serial.print(": ");
        Serial.println(inserial[i]);
        if (inserial[i] == -49 )
        {
          resp = 9;
          Serial.println(resp);
          //break;
        }
      }

      if (resp != 9)
      {
        int _var = inserial[4] * 10 + inserial[5];
        int _val = inserial[0] * 1000 +inserial[1] * 100 + inserial[2] * 10 + inserial[3];
        switch (_var) {
          case 1: laser = _val; break;
          case 2: power = _val; break;
          case 3: posit = _val-500; break;//(40*)200 mkm//(2*)1.8/4 = 0.45 degree
          case 4: moveshvp = _val; break;
          case 5: velshvp = _val;shvp.setDivider(velshvp); break;
          case 6: laser_cur = _val;laser_sens_step(laser_cur, laser_dest); break;
          case 7: laser_dest = _val;laser_sens_step(laser_cur, laser_dest); break;
          case 8: laser_sensor = _val; break;
          case 9: k_p_p =0.1* _val; break;
          case 10: k_v_p =0.1* _val; break;
        }
        Serial.print(_var);
        Serial.print(" ");
        Serial.println(_val);
        //Serial.println(resp);
        
        if (Serial.available())
        {
          Serial.read();
        }
      }
    }
  }
}
