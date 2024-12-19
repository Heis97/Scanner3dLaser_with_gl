




#define EN_PIN_disp    -1   //enable 
#define DIR_PIN_disp   3   //direction
#define STEP_PIN_disp  2    //step
#define CS_PIN_disp    A2  //chip select//A1-kuka_sam//A2-rozum_misis

#define EN_PIN_las    -1  //enable 
#define DIR_PIN_las   5   //direction
#define STEP_PIN_las  4    //step
#define CS_PIN_las    A1   //chip select//A2-kuka_sam//A1-rozum_misis

#define EN_PIN_z    -1   //enable
#define DIR_PIN_z   7   //direction
#define STEP_PIN_z  6    //step
#define CS_PIN_z    A0   //chip select


#define MOSI_PIN  11
#define MISO_PIN 12
#define SCK_PIN  13

#define LED_PIN 9

#define i2c_sensor 52
#define len_data 23

#include <TMC2130Stepper.h>
#include <StepDirDriverPoz.h>
//#include "timer-api.h"
#include "GyverTimers.h"
#include <Wire.h> 

StepDirDriverPoz mot_disp(STEP_PIN_disp, DIR_PIN_disp, EN_PIN_disp);
StepDirDriverPoz mot_las(STEP_PIN_las, DIR_PIN_las , EN_PIN_las);
StepDirDriverPoz mot_z(STEP_PIN_z, DIR_PIN_z, EN_PIN_z);

TMC2130Stepper driver_disp = TMC2130Stepper(EN_PIN_disp, DIR_PIN_disp, STEP_PIN_disp, CS_PIN_disp, MOSI_PIN, MISO_PIN, SCK_PIN);
TMC2130Stepper driver_las = TMC2130Stepper(EN_PIN_las, DIR_PIN_las, STEP_PIN_las, CS_PIN_las, MOSI_PIN, MISO_PIN, SCK_PIN);
TMC2130Stepper driver_z = TMC2130Stepper(EN_PIN_z, DIR_PIN_z, STEP_PIN_z, CS_PIN_z, MOSI_PIN, MISO_PIN, SCK_PIN);




const int end_las = A6;
const int end_z = A7;

const int drill_vel = 10;
const int drill_dir = 9;

int laser = 0; 
int power = 2;

int dir_disp = 0;
int div_disp = 1;

int div_las = 1;
int move_las = 1;
long int posit_laser = 0;
long int posit_disp = 0;
long int posit_z = 0;
long int pos = 5000;
int div_z = 1;
bool cycle_dir = false;
bool led_dir = false;
char rec_i2c[10]; 
int led_div = 1;
int cycle_type = 0;
int cycle_ampl = 1000;

int counter_inp = 0;
bool get_req = false;


void setup() {
  Serial.begin(250000);
  Wire.begin();
  driver_disp.begin(); 
  Serial.println( driver_disp.test_connection()); 
  driver_disp.rms_current(1100);
  driver_disp.stealthChop(1); 
  driver_disp.microsteps(16);

  driver_z.begin(); 
  Serial.println( driver_z.test_connection()); 
  driver_z.rms_current(1100);
  driver_z.stealthChop(1); 
  driver_z.microsteps(16);

  driver_las.begin(); 
  Serial.println( driver_las.test_connection()); 
  driver_las.rms_current(900);
  driver_las.stealthChop(1); 
  driver_las.microsteps(16);

 Serial.println("start1");
  mot_z.setDivider(5);

  mot_disp.setDivider(5);

  mot_las.setDivider(5);


  Timer2.setFrequency(5000);          
  Timer2.enableISR(); 

  //mot_disp.step(10000);
  //mot_z.step(10000);
  //mot_las.step(10000);
  //delay(10000);
  pinMode(LED_PIN, OUTPUT);

  analogWrite(LED_PIN,0);
  //PORTB |= 1<<1;
  Serial.println("start2");
}


void loop() {
  decod_main();
  mot_z.gotopoz(posit_z);
  mot_disp.gotopoz(posit_disp);
  mot_las.gotopoz(posit_laser);
  control_cycle();
 //
 // cold_fix(driver_las,mot_las,550);//
 // cold_fix(driver_disp,mot_disp,850);//
 // cold_fix(driver_z,mot_z,850);//

  /*send_i2c("b000113",51);
  delay(1000);
  send_i2c("b000213",51);
  delay(1000);*/
  //delay(1000);
  if(get_req)
  {
    get_request_i2c(i2c_sensor, len_data);
    get_req = false;
  }
  
}

void control_cycle()
{
  
  if(cycle_type == 1)
  {
    if(mot_las.readSteps()==0)
   {
     if(cycle_dir) 
    {
      posit_laser = cycle_ampl;
      cycle_dir = false;
      Serial.println("false");
    }
    else
    {
      posit_laser = 0;
      cycle_dir = true;
      Serial.println("true");
    }
   }
  }
  else
  {
   // mot_las.step(0);
  }
   
}

void cold_fix(TMC2130Stepper motor,StepDirDriverPoz stp,int cur)
{
  if(stp.readSteps()==0)
  {
    motor.rms_current(150);
  }
  else
  {
     motor.rms_current(cur); 
  }
} 

 

void send_i2c(char mes[],byte adr)
{
  Wire.beginTransmission(adr); // начало передачи на устройство номер 8
  Wire.write(mes);       // отправляем цепочку текстовых байт          // отправляем байт из переменной
  Serial.println(mes);
  Wire.endTransmission(); 
  Serial.println("sendi2c_end"); 
}


void get_request_i2c(int adr, int len)
{
  //Serial.println("get_req");
  char rec_i2c_int[23] = "00000000000000000000000";
//  rec_i2c[10];
  int i_m =0; 
  int i_st =0; 
  int i_int =0; 
  bool st_done = false;
  Wire.requestFrom( adr,len);
  while(Wire.available() && i_m<len) {  // Read Received Datat From Slave Device
    int b = Wire.read();
    i_m++;
    if(b<254)
    {
      if(!st_done )
      {
        i_st = i_m;
        st_done = true;
      }
      rec_i2c_int[i_m] = (char)b;
      i_int++;
     /* Serial.print(b);
      Serial.print(" ");
      Serial.println(rec_i2c_int[i_m] );*/
    }
  }
  int ki = 0;
  for(int i = i_st; i<i_int ;i++)
  {
    rec_i2c[ki] = rec_i2c_int[i]; ki++;
  }  
  Serial.println(rec_i2c);
}


ISR(TIMER2_A)
{
  counter_inp++;
  if(counter_inp>5000)
  {
    counter_inp=0;
    get_req = true;
  }
  mot_las.control();
  mot_disp.control();
  mot_z.control();
}


void decod_main()
{
  int resp = 0;
 
  if (Serial.available())
  {
    char mes[] = "b000000";
    int byte1 = Serial.read() - 48;
    Serial.println(byte1);
    if (byte1 == 50)
    {
      
      int inserial[8];
      for (int i = 0; i < 8; i++)
      {
        byte inp = Serial.read();
        inserial[i] = inp - 48;
        if(i<6) {mes[i+1] =inp;}
        Serial.print(i);
        Serial.print(": ");
        Serial.println(inserial[i]);
        if (inserial[i] == -49 )
        {
          //resp = 9;
          //Serial.println(resp);
          //break;
        }
      }

      if (resp != 9)
      {
         byte _adr = inserial[6] * 10 + inserial[7];
        int _var = inserial[4] * 10 + inserial[5];
        long int _val = inserial[0] * 1000 + inserial[1] * 100 + inserial[2] * 10 + inserial[3];
        if(_adr<=0)
        {
          decoding_vals(_var, _val);          
        }
        else
        {
          send_i2c(mes,_adr);
        }
        String decod =String(mes)+ " "+String(_val)+ " "+String(_var)+" "+String(_adr);
        Serial.println (decod);
        while (Serial.available())
        {          
          Serial.read();
        }
        Serial.println("clear");
      }
    }
  }
}

void decod_main_i2c()
{
  
  int resp = 0;
  if (Wire.available()>1)
  {
    int byte1 = Wire.read() - 48;
    Serial.println(byte1);
    if (byte1 == 50)
    {
      
      int inserial[8];
      for (int i = 0; i < 6; i++)
      {
        inserial[i] =Wire.read() - 48;
        if (inserial[i] == -49 )
        {
          resp = 9;
          Serial.println(inserial[i]);
          //break;
        }
      }

      if (resp != 9)
      {
        int _var = inserial[4] * 10 + inserial[5];
        int _val = inserial[0] * 1000 + inserial[1] * 100 + inserial[2] * 10 + inserial[3];
        decoding_vals(_var, _val);
        while(Serial.available())
        {
          Wire.read();
        }
      }
    }
  }
}



void decoding_vals(int _var, long _val)
{
  switch (_var) {
          case 1: laser = _val; digitalWrite(LED_PIN,_val); break;//
          case 2: power = _val; break;
          case 3: posit_laser = _val-5000; break;//(40*)200 mkm//(2*)1.8/4 = 0.45 degree
          case 4: move_las = _val; break;
          case 5: div_las = _val; mot_las.setDivider(div_las);  break;
          
          case 12: div_disp = _val;mot_disp.setDivider(div_disp);   break;
          case 13: dir_disp = _val-1; break;
          case 14: cycle_type = 0; posit_laser = 0;mot_disp.setDivider(10); break;

          case 15: div_z = _val;mot_z.setDivider(div_z);   break;
          case 16: posit_z = (_val-5000); break;
//          case 17: home_z(); break;
//          case 18: push_forward(500); break;
        //  case 19: push_back(500); break;

          case 20: analogWrite(drill_vel,_val); break;
          case 21: digitalWrite(drill_dir,_val); break;

          case 22: mot_z.setPoz(0);pos =10*(_val-5000);  mot_z.setPoz(pos); break;
          case 23: mot_disp.setPoz(0);pos=10*(_val-5000);  mot_disp.setPoz(pos); break;

          case 25: cycle_type = _val; break;
          case 26: cycle_ampl = _val; break;

          case 32: 
         // digitalWrite(LED_PIN,_val);
          analogWrite(LED_PIN,_val);
          break;
          case 40: div_las = _val; mot_las.setDivider(div_las);  break;
          case 49: get_request_i2c(i2c_sensor, len_data);  break;
  }
  }
