//разные частоты
//ускорение немного


#define EN_PIN_disp    -1   //enable 
#define DIR_PIN_disp   3   //direction
#define STEP_PIN_disp  2    //step
#define CS_PIN_disp    A0   //chip select//A1-kuka_sam//A2-rozum_misis

#define EN_PIN_las    -1   //enable 
#define DIR_PIN_las   4   //direction
#define STEP_PIN_las  7    //step
#define CS_PIN_las    A3   //chip select//A2-kuka_sam//A1-rozum_misis

#define EN_PIN_z    -1   //enable
#define DIR_PIN_z   5   //direction
#define STEP_PIN_z  6    //step
#define CS_PIN_z    A1   //chip select


#define MOSI_PIN  11
#define MISO_PIN 12
#define SCK_PIN  13




#include <TMC2130Stepper.h>
#include <StepDirDriverPoz.h>
//#include "timer-api.h"
//#include <Wire.h> 
#include "GyverTimers.h"
StepDirDriverPoz mot_disp(STEP_PIN_disp, DIR_PIN_disp, EN_PIN_disp);
StepDirDriverPoz mot_las(STEP_PIN_las, DIR_PIN_las , EN_PIN_las);
StepDirDriverPoz mot_z(STEP_PIN_z, DIR_PIN_z, EN_PIN_z);

TMC2130Stepper driver_disp = TMC2130Stepper(EN_PIN_disp, DIR_PIN_disp, STEP_PIN_disp, CS_PIN_disp, MOSI_PIN, MISO_PIN, SCK_PIN);
TMC2130Stepper driver_las = TMC2130Stepper(EN_PIN_las, DIR_PIN_las, STEP_PIN_las, CS_PIN_las, MOSI_PIN, MISO_PIN, SCK_PIN);
TMC2130Stepper driver_z = TMC2130Stepper(EN_PIN_z, DIR_PIN_z, STEP_PIN_z, CS_PIN_z, MOSI_PIN, MISO_PIN, SCK_PIN);




const int end_las = A4;
const int end_z = A5;

const int drill_vel = 9;
const int drill_dir = 8;

const int water_vel = 10;
//int timer_main = _TIMER5;
int laser = 2; 
int power = 7;

int dir_disp = 0;
int div_disp = 1;

int div_las = 1;
int move_las = 1;
long int posit_las = 0;

long int posit_z = 0;
int div_z = 1;
bool homed_z = true;
bool homed_las = true;
bool homing_las = false;
bool homing_z = false;

void setup() {
  Serial.begin(250000);

  driver_disp.begin(); 
  Serial.println( driver_disp.test_connection()); 
  driver_disp.rms_current(500);
  driver_disp.stealthChop(1); 
  driver_disp.microsteps(16);

   driver_las.begin(); 
  Serial.println( driver_las.test_connection()); 
  driver_las.rms_current(500);
  driver_las.stealthChop(1); 
  driver_las.microsteps(16);

  driver_z.begin(); 
  Serial.println( driver_z.test_connection()); 
  driver_z.rms_current(500);
  driver_z.stealthChop(1); 
  driver_z.microsteps(16);

 
  
  mot_z.setMode(true);
  mot_z.setDivider(2);
  //Wire.begin(50);
  //Wire.onReceive(decod_main_i2c);
  mot_disp.setMode(false);
  mot_disp.setDivider(2);

   mot_las.setMode(false);
  mot_las.setDivider(2);
  
 // pinMode(end_las,INPUT);
 // pinMode(end_z,INPUT);
pinMode(10,OUTPUT);
pinMode(9,OUTPUT);
Serial.println("test1");

//mot_disp.step(1000);
//delay(3000);
//mot_disp.step(-1000);
   //analogWrite(10,50);
  // analogWrite(9,50);
  // delay(5000);
  // digitalWrite(10,1);
  // mot_las.step(1000);
//delay(3000);  

//home_z();
 Timer2.setFrequency(5000);               // Высокоточный таймер 1 для первого прерывания, частота - 3 Герца
  //Timer1.setPeriod(333333);           // то же самое! Частота 3 Гц это период 333 333 микросекунд
  //Timer1.setFrequencyFloat(4.22);     // Если нужна дробная частота в Гц  
  Timer2.enableISR();    
 //timer_init_ISR_5KHz(timer_main);//shvp 1khz
}


void loop() {
  decod_main();
  if(homed_las){ mot_las.gotopoz(posit_las);}
  if(homed_z){ mot_z.gotopoz(posit_z);}
  cold_fix(driver_las,mot_las,550);
  //Serial.print("disp ");
  cold_fix(driver_disp,mot_disp,550);
  cold_fix(driver_z,mot_z,250);
  disp_control();
  //delay(10);

  /*Serial.print(analogRead(end_las));
   Serial.print(" ");
    Serial.println(analogRead(end_z));*/
}

void cold_fix(TMC2130Stepper motor,StepDirDriverPoz stp,int cur)
{
  if(stp.readSteps()==0)
  {
    motor.rms_current(150);
    //Serial.println("steps 0");
  }
  else
  {
     motor.rms_current(cur); 
      // Serial.println("steps 1");
  }
}

void home_laser()
{
  mot_las.setDivider(2);
  driver_las.rms_current(550); 
  homing_las = true;
  int end_val = analogRead(end_las);
  
  while(end_val>500 && homing_las)
  {
    mot_las.step(-100);
    decod_main();
    end_val = analogRead(end_las);
  }
  if(end_val<500)
  {
    mot_las.setPoz(0);
    posit_las = 1000;
    homed_las = true;
  }
  homing_las = false;
  
}  

void home_z()
{
  mot_z.setDivider(2);
  driver_z.rms_current(550); 
  int end_val = analogRead(end_z);
  homing_z = true;
  while(end_val>500 && homing_z)
  {
    mot_z.step(-100); 
    decod_main();
     end_val = analogRead(end_z);  
  }
  
  if(end_val<500)
  {
    mot_z.setPoz(0);
    posit_z = 1000;
    homed_z = true;
  }
  homing_z = false;
}

void push_forward(int delay_time)
{
  mot_disp.setDivider(2);   
  mot_disp.step(2000);
  delay(delay_time);
  mot_disp.step(0);  
  mot_disp.setDivider(div_disp);   
}
void push_back(int delay_time)
{
  mot_disp.setDivider(2);   
  mot_disp.step(-2000);
  delay(delay_time);
  mot_disp.step(0);  
  mot_disp.setDivider(div_disp); 
}

void disp_control()
{
  if(dir_disp==0)
  {
    mot_disp.step(0);
  }
  else if(dir_disp == -1)
  {
    mot_disp.step(-500);
  }
  else if(dir_disp == 1)
  {
    mot_disp.step(500);
  }
  
  //mot_disp.step(32000);
}

//void timer_handle_interrupts(int timer)
ISR(TIMER2_A) 
{

   mot_las.control();
  mot_disp.control();
  mot_z.control();


}

void decod_main()
{ 
  int resp = 0;
  if (Serial.available())
  {
    int inserial[8];
    char mes[8];
    int byte1 = Serial.read() - 48;
    if (byte1 == 50)
    {
      for (int i = 0; i < 6; i++)
      {
        byte inp = Serial.read();
        mes[i] = inp;
        inserial[i] = inp - 48;
        if (inserial[i] == -49 )
        {
          resp = 9;
        }
      }
      if (resp != 9)
      {
        int _var = inserial[4] * 10 + inserial[5];
        long _val = inserial[0] * 1000 + inserial[1] * 100 + inserial[2] * 10 + inserial[3];
        decoding_vals(_var, _val);
        String decod =String(mes)+ " "+String(_var)+" "+String(_val);
        Serial.println (decod);
        while (Serial.available())
        {
          Serial.read();
        }
      }
    }
  }
}

void decoding_vals(int _var, long _val)
{
  switch (_var) {
    case 1: laser = _val; break;
    case 2: power = _val; break;
    case 3: posit_las = _val-5000; break;//(40*)200 mkm//(2*)1.8/4 = 0.45 degree
    case 4: move_las = _val; break;
    case 5: div_las = _val;  break;//mot_las.setDivider(div_las); 
    
    case 12: div_disp = _val;mot_disp.setDivider(div_disp);   break;
    case 13: dir_disp = _val-1; break;
    case 14: home_laser(); break;

    case 15: div_z = _val;mot_z.setDivider(div_z);   break;
    case 16: posit_z = (10*_val)-5000; break;
    case 17: home_z(); break;
    case 18: push_forward(500); break;
    case 19: push_back(500); break;

    case 20: analogWrite(drill_vel,_val); break;
    case 21: digitalWrite(drill_dir,_val); break;
     case 24: analogWrite(water_vel,_val); break;

     case 40: div_las = _val; mot_las.setDivider(div_las);  break;
     case 41: mot_las.step(0);posit_las = mot_las.readPoz();  homing_las = false;  break;
     case 42: mot_z.step(0);  posit_z =  mot_z.readPoz();     homing_z = false;   break;

     case 43: mot_las.setPoz(0); posit_las = 0; homing_las = false;  break;
     case 44: mot_z.setPoz(0);   posit_z = 0;   homing_z = false;  break;
     case 45: posit_las = (10*_val)-5000;  break;
  }
}
