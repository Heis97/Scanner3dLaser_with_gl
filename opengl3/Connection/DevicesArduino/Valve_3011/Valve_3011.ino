#include <Servo.h>
#include <Wire.h> 
#define I2C_ADDR    53//new
Servo myservo;
int pos = 0;

void setup() {
  Serial.begin(250000);
  myservo.attach(3);  // подключаем на пин 3
  Wire.begin(I2C_ADDR);
  Wire.onReceive(decod_main_i2c);
  //myservo.write(180);   // поворот на 0 градусов
}
void loop() {
  decod_main();
}
void send_i2c(char mes[],byte adr)
{
  Wire.beginTransmission(adr); // начало передачи на устройство номер 8
  Wire.write(mes);       // отправляем цепочку текстовых байт          // отправляем байт из переменной
  Serial.println(mes);
  Wire.endTransmission(); 
  Serial.println("sendi2c_end");
  
}

void decod_main()
{
  //Serial.println("dec_st");
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
          String decod =String(mes)+ " "+String(_var)+" "+String(_val)+" "+String(_adr);
          Serial.println (decod);
        }
        else
        {
          send_i2c(mes,_adr);
        }
        
        while (Serial.available())
        {
          Serial.println("clear");
          Serial.read();
        }
      }
    }
  }
  //Serial.println("dec_end");
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
        Serial.print(i);
        Serial.print(": ");
        Serial.println(inserial[i]);
        if (inserial[i] == -49 )
        {
          //resp = 9;
          //Serial.println(inserial[i]);
          //break;
        }
      }

      if (resp != 9)
      {
        int _var = inserial[4] * 10 + inserial[5];
        int _val = inserial[0] * 1000 + inserial[1] * 100 + inserial[2] * 10 + inserial[3];
        decoding_vals(_var, _val);
        String decod =String(_var)+" "+String(_val);
        Serial.println(decod);
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
          case 39: pos = _val; myservo.write(pos); break;

  }
  }
