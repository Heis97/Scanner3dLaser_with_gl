

/*
#define EN_PIN_1       4  // Nano v3:   16 Mega:    38  //enable (CFG6)
#define DIR_PIN_1      3  //            19          55  //direction
#define STEP_PIN_1     2  //            18          54  //step
#define CS_PIN_1        A0  //            17          64  //chip select

#define EN_PIN_2        7  // Nano v3:   16 Mega:    38  //enable (CFG6)
#define DIR_PIN_2     6  //            19          55  //direction
#define STEP_PIN_2     5  //            18          54  //step
#define CS_PIN_2       A1  //            17          64  //chip select


*/


#define EN_PIN_disp    4   //enable 
#define DIR_PIN_disp   3   //direction//3 for 50  // 2 for 51(old)
#define STEP_PIN_disp  2    //step//2 for 50 // 3 for 51(old)
#define CS_PIN_disp    A0   //chip select//A1-kuka_sam//A2-rozum_misis
#define I2C_ADDR    51//new


/*
#define EN_PIN_disp    4   //enable 
#define DIR_PIN_disp   2   //direction//3 for 50  // 2 for 51(old)
#define STEP_PIN_disp  3    //step//2 for 50 // 3 for 51(old)
#define CS_PIN_disp    A0   //chip select//A1-kuka_sam//A2-rozum_misis
#define I2C_ADDR    51//old
*/

#define MOSI_PIN  11
#define MISO_PIN 12   
#define SCK_PIN  13
#include <Wire.h> 

#include <TMC2130Stepper.h>
#include <StepDirDriverPoz.h>
#include "timer-api.h"

StepDirDriverPoz mot_disp(STEP_PIN_disp, DIR_PIN_disp, EN_PIN_disp);

TMC2130Stepper driver_disp = TMC2130Stepper(EN_PIN_disp, DIR_PIN_disp, STEP_PIN_disp, CS_PIN_disp, MOSI_PIN, MISO_PIN, SCK_PIN);



const int end_1 = A2;
const int end_2 = A3;

int laser = 2; 
int power = 7;

int dir_disp = 0;
int div_disp = 1;

int div_las = 1;
int move_las = 1;
long int posit_laser = 0;

long int posit_z = 0;
int div_z = 1;

void setup() {
  Serial.begin(250000);

  driver_disp.begin(); 
  Serial.println( driver_disp.test_connection()); 
  driver_disp.rms_current(600);
  driver_disp.stealthChop(1); 
  driver_disp.microsteps(16);

  timer_init_ISR_5KHz(_TIMER1);//shvp 1khz
 //Wire.begin();
  Wire.begin(I2C_ADDR);
  Wire.onReceive(decod_main_i2c);
  
  mot_disp.setMode(true);
  mot_disp.setDivider(2);
  pinMode(end_1,INPUT);
  pinMode(end_2,INPUT);
  digitalWrite(EN_PIN_disp, LOW);  //debug

}


void loop() {
  
  decod_main();
  disp_control(); 
  //delay(50);
  //Serial.print("end1: ");
  /* 
   *  Serial.print("end1: ");
  Serial.println(analogRead(end_1));
   Serial.print("end2: ");
   Serial.println(analogRead(end_2));*/
  if(analogRead(end_2)>800 || analogRead(end_1)>800)
  {
    mot_disp.step(0); //debug 50000
    dir_disp *= -1;
    Serial.println(dir_disp); //debug
   // mot_disp.step(50000); //debug 50000
  //  Serial.println("10"); //debug
    
     disp_control(); 
     delay(1000);
   // Serial.println("ch");
   
  }

  /*send_i2c("b000112");
  delay(1000);
  send_i2c("b000013");
  delay(1000);*/
  //char mes[] = "b001123";
 // mes[2] = 52;
  //Serial.println(mes);
  //delay(500);
}

void send_i2c(char mes[],byte adr)
{
  Wire.beginTransmission(adr); // начало передачи на устройство номер 8
  Wire.write(mes);       // отправляем цепочку текстовых байт          // отправляем байт из переменной
  Wire.endTransmission(); 
}

void disp_control()
{
  if(dir_disp==0)
  {
    mot_disp.step(0);
  }
  else if(dir_disp == -1)
  {
    mot_disp.step(-50000); //debug 50000
  }
  else if(dir_disp == 1)
  {
    mot_disp.step(50000);  //debug 50000
    
  }
  
  //mot_disp.step(32000);
}

void controlLaser(int laser_state, int las_power)
{
  if(laser_state == 1)
  {
     timer_init_ISR(_TIMER2, TIMER_PRESCALER_1_256, 5*las_power);
    PORTC |= 1<<5;
  }   
}

void timer_handle_interrupts(int timer)
{
  if (timer == _TIMER1)
  {
    mot_disp.control();
  }
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
          resp = 9;
          Serial.println(resp);
          //break;
        }
      }

      if (resp != 9)
      {
        byte _adr = inserial[6] * 10 + inserial[7];
        int _var = inserial[4] * 10 + inserial[5];
        int _val = inserial[0] * 1000 + inserial[1] * 100 + inserial[2] * 10 + inserial[3];
        switch (_var) {
          case 1: laser = _val; break;
          case 2: power = _val; break;
          //case 3: posit_laser = _val-5000; driver_las.rms_current(400); break;//(40*)200 mkm//(2*)1.8/4 = 0.45 degree
          case 4: move_las = _val; break;
         // case 5: div_las = _val;mot_las.setDivider(div_las);   break;
          
          case 12: div_disp = _val;mot_disp.setDivider(div_disp);   break;
          case 13: dir_disp = _val-1; break;
         // case 14: home_laser(); break;

         // case 15: div_z = _val;mot_z.setDivider(div_z);   break;
          case 16: posit_z = _val-5000; break;
          //case 17: home_z(); break;
          //case 18: push_forward(500); break;
          //case 19: push_back(500); break;
        }
        Serial.print("var: ");
        Serial.print(_var);
        Serial.print(": ");
        Serial.println(_val);
    
       /* Serial.println(mes);
        if(_adr>0)
        {
          send_i2c(mes,_adr);
        }*/
        
        if (Serial.available())
        {
          Serial.read();
        }
      }
    }
  }
}

void decod_main_i2c()
{
  
  int resp = 0;
  if (Wire.available())
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
        switch (_var) {
          case 1: laser = _val; break;
          case 2: power = _val; break;
          //case 3: posit_laser = _val-5000; driver_las.rms_current(400); break;//(40*)200 mkm//(2*)1.8/4 = 0.45 degree
          case 4: move_las = _val; break;
          case 5: div_las = _val;  break;//mot_las.setDivider(div_las); 
          
          case 12: div_disp = _val;mot_disp.setDivider(div_disp);   break;
          case 13: dir_disp = _val-1; break;
          //case 14: home_laser(); break;

          //case 15: div_z = _val;mot_z.setDivider(div_z);   break;
          case 16: posit_z = 10*(_val-5000); break;
          //case 17: home_z(); break;
          //case 18: push_forward(500); break;
          //case 19: push_back(500); break;

          //case 20: analogWrite(drill_vel,_val); break;
          //case 21: digitalWrite(drill_dir,_val); break;
        }
        Serial.print("var: ");
        Serial.print(_var);
        Serial.print(": ");
        Serial.println(_val);

        while(Wire.available())
        {
          Wire.read();
        }
      }
    }
  }
}
